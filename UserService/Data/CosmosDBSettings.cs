namespace UserService.Data;
public record CosmosDbSettings
{
    public required string AccountEndpoint { get; set; }
    public required string AccountKey { get; set; }
    public required string DatabaseName { get; set; }
    public required string ContainerName { get; set; }

}