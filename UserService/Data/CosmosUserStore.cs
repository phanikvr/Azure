using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Linq;

namespace UserService.Data;
public class CosmosUserStore :  IUserStore<ApplicationUser>, 
                                IUserEmailStore<ApplicationUser> , 
                                IUserPasswordStore<ApplicationUser>
{
    private readonly CosmosDbContext _dbContext;

    public CosmosUserStore(CosmosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await _dbContext.UsersContainer.CreateItemAsync(user, new PartitionKey(user.UserName), cancellationToken: cancellationToken);
        return IdentityResult.Success;
    }

    public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        var query = _dbContext.UsersContainer
                        .GetItemLinqQueryable<ApplicationUser>(allowSynchronousQueryExecution :true )
                        .Where(a => a.UserName == normalizedEmail)
                        .FirstOrDefault<ApplicationUser>();
            
            return await Task.FromResult(query);
        /*
        var query = new QueryDefinition("SELECT * FROM c WHERE c.UserName = @email")
            .WithParameter("@email", normalizedEmail);
        
        using FeedIterator<ApplicationUser> resultSet = _dbContext.UsersContainer.GetItemQueryIterator<ApplicationUser>(query);
        while (resultSet.HasMoreResults)
        {
            foreach (var user in await resultSet.ReadNextAsync(cancellationToken))
            {
                return user;
            }
        }
        return null;        
        */
    }
    public async Task<ApplicationUser> FindByNameAsync(string normalizedEmail, CancellationToken cancellationToken) => await FindByEmailAsync(normalizedEmail, cancellationToken);
     

    public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        //FeedResponse<ApplicationUser>? response;
        try
        {
            var query = _dbContext.UsersContainer
                        .GetItemLinqQueryable<ApplicationUser>()
                        .Where(a => a.Id == userId)
                        .FirstOrDefault<ApplicationUser>();
            
            return await Task.FromResult(query);
            /*
            var iterator = query.ToFeedIterator();

            while (iterator.HasMoreResults)
            {
                response = await iterator.ReadNextAsync();                
            }
            return response?.First<ApplicationUser>();
            */
        }
        catch (CosmosException)
        {
            return null;
        }
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.Id);
    public Task<string> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
    public Task SetUserNameAsync(ApplicationUser user, string userName, CancellationToken cancellationToken) { user.UserName = userName; return Task.CompletedTask; }
    public Task<string> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
    public Task SetNormalizedUserNameAsync(ApplicationUser user, string normalizedName, CancellationToken cancellationToken) { user.UserName = normalizedName; return Task.CompletedTask; }

    public Task<string> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);
    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(true);
    public Task SetEmailAsync(ApplicationUser user, string email, CancellationToken cancellationToken) { user.UserName = email; return Task.CompletedTask; }
    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken) { return Task.CompletedTask; }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string normalizedEmail, CancellationToken cancellationToken) { return Task.CompletedTask; }
    public Task<string> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken) => Task.FromResult(user.UserName);

    public Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken) => CreateAsync(user, cancellationToken);
    public Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken) 
    {
        _dbContext.UsersContainer.DeleteItemAsync<ApplicationUser>(user.UserName, new PartitionKey(user.UserName), cancellationToken: cancellationToken);
        return Task.FromResult(IdentityResult.Success);
    }


    public Task<string> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }

    public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash, CancellationToken cancellationToken)
    {
        user.PasswordHash = passwordHash;
        return Task.CompletedTask;
    }
    public void Dispose() { }
}