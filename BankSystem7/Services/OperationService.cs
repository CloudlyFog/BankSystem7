using MongoDB.Driver;
using Standart7.Models;

namespace BankSystem7.Services;

internal class OperationService
{
    public IMongoCollection<Operation> Collection { get; private set; }
    private readonly IMongoDatabase _database;
    private readonly MongoClientSettings _settings;
    private readonly MongoClient _client;
    public OperationService()
    {
        _settings = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase("BankSystem");
        Collection = _database.GetCollection<Operation>($"{nameof(Operation)}s");
    }

    public OperationService(string connection, string nameDatabase)
    {
        _settings = MongoClientSettings.FromConnectionString(connection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(nameDatabase);
        Collection = _database.GetCollection<Operation>($"{nameof(Operation)}s");
    }
    public OperationService(IMongoCollection<Operation> collection)
    {
        Collection = collection;
    }
}