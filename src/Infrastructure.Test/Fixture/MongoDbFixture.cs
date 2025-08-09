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
        var databaseName = Configuration.GetValue<string>("MongoDB:DatabaseName") ??
                           throw new ArgumentException("MongoDB:DatabaseName is not configured");
        var collectionName = Configuration.GetValue<string>("MongoDB:CollectionName") ??
                             throw new ArgumentException("MongoDB:CollectionName is not configured");

        var url = $"mongodb://{username}:{password}@{host}:{port}";
        Client = new MongoClient(url);
        Database = Client.GetDatabase(databaseName);
        Database.CreateCollection(collectionName);
    }

    public void Dispose()
    {
        var databaseName = Configuration.GetValue<string>("MongoDB:DatabaseName");
        Client.DropDatabase(databaseName);
    }
}
