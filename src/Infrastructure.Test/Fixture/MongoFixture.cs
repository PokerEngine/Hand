using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Infrastructure.Test.Fixture;

public class MongoFixture : IDisposable
{
    public IConfiguration Configuration { get; }
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    public MongoFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var host = Configuration.GetValue<string>("Mongo:Host") ??
                   throw new ArgumentException("Mongo:Host is not configured");
        var port = Configuration.GetValue<int?>("Mongo:Port") ??
                   throw new ArgumentException("Mongo:Port is not configured");
        var username = Configuration.GetValue<string>("Mongo:Username") ??
                       throw new ArgumentException("Mongo:Username is not configured");
        var password = Configuration.GetValue<string>("Mongo:Password") ??
                       throw new ArgumentException("Mongo:Password is not configured");
        var databaseName = Configuration.GetValue<string>("Mongo:DatabaseName") ??
                           throw new ArgumentException("Mongo:DatabaseName is not configured");
        var collectionName = Configuration.GetValue<string>("Mongo:CollectionName") ??
                             throw new ArgumentException("Mongo:CollectionName is not configured");

        var url = $"mongodb://{username}:{password}@{host}:{port}";
        Client = new MongoClient(url);
        Database = Client.GetDatabase(databaseName);
        Database.CreateCollection(collectionName);
    }

    public void Dispose()
    {
        var databaseName = Configuration.GetValue<string>("Mongo:DatabaseName");
        Client.DropDatabase(databaseName);
    }
}
