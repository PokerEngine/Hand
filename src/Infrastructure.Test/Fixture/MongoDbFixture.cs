using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Infrastructure.Test.Fixture;

public class MongoDbFixture : IDisposable
{
    public IConfiguration Configuration { get; }
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    public MongoDbFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var host = Configuration.GetValue<string>("MongoDB:Host") ??
                   throw new ArgumentException("MongoDB:Host is not configured");
        var port = Configuration.GetValue<int?>("MongoDB:Port") ??
                   throw new ArgumentException("MongoDB:Port is not configured");
        var username = Configuration.GetValue<string>("MongoDB:Username") ??
                       throw new ArgumentException("MongoDB:Username is not configured");
        var password = Configuration.GetValue<string>("MongoDB:Password") ??
                       throw new ArgumentException("MongoDB:Password is not configured");
        var database = Configuration.GetValue<string>("MongoDB:Database") ??
                       throw new ArgumentException("MongoDB:Database is not configured");

        var url = $"mongodb://{username}:{password}@{host}:{port}";
        Client = new MongoClient(url);
        Database = Client.GetDatabase(database);
    }

    public void Dispose()
    {
        var database = Configuration.GetValue<string>("MongoDB:Database");
        Client.DropDatabase(database);
    }
}
