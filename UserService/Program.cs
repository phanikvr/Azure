using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using UserService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDb"));
builder.Services.AddSingleton<CosmosDbContext>(sp => {
    var cosmosSettings = builder.Configuration.GetSection("CosmosDb").Get<CosmosDbSettings>();
    return new CosmosDbContext(cosmosSettings);
});
builder.Services.AddSingleton<IUserStore<ApplicationUser>, CosmosUserStore>(sp => {
    var dbContext = sp.GetRequiredService<CosmosDbContext>();
    return new CosmosUserStore(dbContext);
});

builder.Services.AddSingleton<IRoleStore<IdentityRole>, CosmosRoleStore>(sp => {
    var dbContext = sp.GetRequiredService<CosmosDbContext>();
    return new CosmosRoleStore(dbContext);
});

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false; // Don't require digits
    options.Password.RequireLowercase = false; // Don't require lowercase
    options.Password.RequireUppercase = false; // Don't require uppercase
    options.Password.RequireNonAlphanumeric = false; // Don't require special characters
    options.Password.RequiredLength = 6; // Minimum password length
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddUserStore<CosmosUserStore>()
    .AddDefaultTokenProviders();

// builder.Services.AddSingleton<CosmosClient>((sp) =>
// {    
//     var config = sp.GetRequiredService<IOptions<CosmosDbSettings>>().Value;
//     CosmosClient client = new(
//         connectionString: $"AccountEndpoint={config.AccountEndpoint};AccountKey={config.AccountKey};Database={config.DatabaseName};"
//     );
//     return client;
// });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.UseAuthentication();

/*
app.MapGet("/getUsers", async (CosmosClient cosmosClient, IOptions<CosmosDbSettings> options) =>
{
    var cosmosSettings = options.Value;

    Database database = cosmosClient.GetDatabase(cosmosSettings.DatabaseName);
    database = await database.ReadAsync();

    //await writeOutputAync($"Get database:\t{database.Id}");

    Container container = database.GetContainer(cosmosSettings.ContainerName);
    container = await container.ReadContainerAsync();

    var query = new QueryDefinition(
                query: "SELECT * FROM ApplicationUsers"
                //query: "SELECT * FROM products p WHERE p.category = @category"
            );
            //.WithParameter("@category", "gear-surf-surfboards");

    return container.GetItemLinqQueryable<ApplicationUser>(allowSynchronousQueryExecution :true);    
    
}); 
*/
app.Run();

