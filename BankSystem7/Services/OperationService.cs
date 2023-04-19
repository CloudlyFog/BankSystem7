using MongoDB.Driver;

namespace BankSystem7.Services;

internal sealed class OperationService<T> where T : class
{
    public IMongoCollection<T> Collection { get; }
    private IMongoDatabase _database;
    private MongoClientSettings _settings;
    private MongoClient _client;
    public const string DefaultConnection = "mongodb://localhost:27017";
    public const string DefaultDatabaseName = "Test";
    public const string DefaultCollectionName = "Operations";

    public OperationService()
    {
        _settings = MongoClientSettings.FromConnectionString(DefaultConnection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(DefaultDatabaseName);
        Collection = _database.GetCollection<T>(DefaultCollectionName);
    }

    public OperationService(string databaseName)
    {
        _settings = MongoClientSettings.FromConnectionString(DefaultConnection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(databaseName);
        Collection = _database.GetCollection<T>(DefaultCollectionName);
    }

    public OperationService(OperationServiceOptions? options)
    {
        _settings = MongoClientSettings.FromConnectionString(options?.Connection ?? DefaultConnection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(options?.DatabaseName ?? DefaultDatabaseName);
        Collection = _database.GetCollection<T>(options?.CollectionName ?? DefaultCollectionName);
    }
}

public sealed class OperationServiceOptions
{
    public string? Connection { get; set; } = null;
    public string? DatabaseName { get; set; } = null;
    public string? CollectionName { get; set; } = null;
}