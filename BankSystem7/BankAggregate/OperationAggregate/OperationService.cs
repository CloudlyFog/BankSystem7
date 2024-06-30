using MongoDB.Driver;

namespace BankSystem7.BankAggregate.OperationAggregate;
internal sealed class OperationService<TOperation> where TOperation : class
{
    public const string DefaultConnection = "mongodb://localhost:27017";
    public const string DefaultDatabaseName = "BankSystemOperation";
    public const string DefaultCollectionName = "Operations";
    private IMongoDatabase _database;
    private MongoClientSettings _settings;
    private MongoClient _client;


    public OperationService()
    {
        _settings = MongoClientSettings.FromConnectionString(DefaultConnection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(DefaultDatabaseName);
        Collection = _database.GetCollection<TOperation>(DefaultCollectionName);
    }

    public OperationService(string databaseName)
    {
        _settings = MongoClientSettings.FromConnectionString(DefaultConnection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(databaseName);
        Collection = _database.GetCollection<TOperation>(DefaultCollectionName);
    }

    public OperationService(OperationServiceOptions? options)
    {
        _settings = MongoClientSettings.FromConnectionString(options?.Connection ?? DefaultConnection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(options?.DatabaseName ?? DefaultDatabaseName);
        Collection = _database.GetCollection<TOperation>(options?.CollectionName ?? DefaultCollectionName);
    }

    public IMongoCollection<TOperation> Collection { get; }
}

public sealed class OperationServiceOptions
{
    public string? Connection { get; set; } = null;
    public string? DatabaseName { get; set; } = null;
    public string? CollectionName { get; set; } = null;
}