using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserService.Data;

public class CosmosRoleStore : IRoleStore<IdentityRole>
{
    private readonly CosmosDbContext _dbContext;
    private const string PartitionKey = "UserName";

    public CosmosRoleStore(CosmosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        await _dbContext.UsersContainer.CreateItemAsync(role, new PartitionKey(PartitionKey), cancellationToken: cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
    {
        await _dbContext.UsersContainer.DeleteItemAsync<IdentityRole>(role.Id, new PartitionKey(PartitionKey), cancellationToken: cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        try
        {
            ItemResponse<IdentityRole> response = await _dbContext.UsersContainer.ReadItemAsync<IdentityRole>(roleId, new PartitionKey(PartitionKey), cancellationToken: cancellationToken);
            return response.Resource;
        }
        catch (CosmosException)
        {
            return null;
        }
    }

    public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.username = @name")
            .WithParameter("@name", normalizedRoleName);

        using FeedIterator<IdentityRole> resultSet = _dbContext.UsersContainer.GetItemQueryIterator<IdentityRole>(query);
        while (resultSet.HasMoreResults)
        {
            foreach (var role in await resultSet.ReadNextAsync(cancellationToken))
            {
                return role;
            }
        }
        return null;
    }

    public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.NormalizedName);

    public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.Id);

    public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        => Task.FromResult(role.Name);

    public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
    {
        role.NormalizedName = normalizedName;
        return Task.CompletedTask;
    }

    public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
    {
        role.Name = roleName;
        return Task.CompletedTask;
    }

    public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken) => CreateAsync(role, cancellationToken);

    public void Dispose() { }
}
