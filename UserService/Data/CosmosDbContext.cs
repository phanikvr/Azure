using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace UserService.Data;
public class CosmosDbContext
{
    public Container UsersContainer { get; }

    public CosmosDbContext(CosmosDbSettings settings)
    {        
        var cosmosClient = new CosmosClient(settings.AccountEndpoint, settings.AccountKey);
        var database = cosmosClient.GetDatabase(settings.DatabaseName);
        UsersContainer = database.GetContainer(settings.ContainerName);
    }
}
