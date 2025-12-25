using Infrastructure.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure.Test.Fixture;

public class MongoDbFixture : IDisposable
{
    public IOptions<MongoDbRepositoryOptions> Options { get; }
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }

    public MongoDbFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Test.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var opt = configuration.GetSection(MongoDbRepositoryOptions.SectionName).Get<MongoDbRepositoryOptions>();
        Options = Microsoft.Extensions.Options.Options.Create(opt!);

        var url = $"mongodb://{Options.Value.Username}:{Options.Value.Password}@{Options.Value.Host}:{Options.Value.Port}";
        Client = new MongoClient(url);
        Database = Client.GetDatabase(Options.Value.Database);
    }

    public void Dispose()
    {
        Client.DropDatabase(Options.Value.Database);
    }
}
