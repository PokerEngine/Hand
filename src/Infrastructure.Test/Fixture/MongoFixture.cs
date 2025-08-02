using MongoDB.Driver;

namespace Infrastructure.Test.Fixture;

public class MongoFixture : IDisposable
{
    public IMongoClient Client { get; }
    public IMongoDatabase Database { get; }
    public string Host { get; }
    public int Port { get; }
    public string Username { get; }
    public string Password { get; }
    public string DatabaseName { get; }
    public string CollectionName { get; }

    public MongoFixture()
    {
        Host = Environment.GetEnvironmentVariable("MONGO_HOST") ?? "localhost";
        Port = Int32.Parse(Environment.GetEnvironmentVariable("MONGO_PORT") ?? "27017");
        Username = Environment.GetEnvironmentVariable("MONGO_USERNAME") ?? "root";
        Password = Environment.GetEnvironmentVariable("MONGO_PASSWORD") ?? "root";
        DatabaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE") ?? "hand";
        CollectionName = Environment.GetEnvironmentVariable("MONGO_COLLECTION") ?? "events";
        DatabaseName = $"test_{DatabaseName}";

        var url = $"mongodb://{Username}:{Password}@{Host}:{Port}";
        Client = new MongoClient(url);
        Database = Client.GetDatabase(DatabaseName);
        Database.CreateCollection(CollectionName);

        Console.WriteLine($"Created test database: {DatabaseName}");
    }

    public void Dispose()
    {
        Client.DropDatabase(DatabaseName);
        Console.WriteLine($"Dropped test database: {DatabaseName}");
    }
}

public abstract class BaseMongoTest : IClassFixture<MongoFixture>
{
    protected readonly MongoFixture Fixture;

    protected BaseMongoTest(MongoFixture fixture)
    {
        Fixture = fixture;
        Fixture.Database.DropCollection(Fixture.CollectionName);
    }
}
