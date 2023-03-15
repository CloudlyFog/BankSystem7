using System.ComponentModel.DataAnnotations;
using MongoDB.Driver;
using Standart7.Models;

namespace BankSystem7.Services;

public class OperationService<T> where T : class
{
    public IMongoCollection<T> Collection { get; private set; }
    private IMongoDatabase _database;
    private MongoClientSettings _settings;
    private MongoClient _client;
    public const string DefaultConnection = "mongodb://localhost:27017";
    public const string DefaultDatabaseName = "BankSystem";
    public const string DefaultCollectionName = "Operations";
    
    [Obsolete("This constructor contains logic which now in development.")]
    public OperationService(bool str)
    {
        OperationOptionsBuilder<T>.Options.Name = "BankSystem";
        _settings = MongoClientSettings.FromConnectionString(OperationOptionsBuilder<T>.Options.Connection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(OperationOptionsBuilder<T>.Options.DatabaseName);
        Collection = _database.GetCollection<T>(OperationOptionsBuilder<T>.Options.CollectionName);
    }
    
    public OperationService(string databaseName)
    {
        _settings = MongoClientSettings.FromConnectionString(DefaultConnection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(databaseName);
        Collection = _database.GetCollection<T>(DefaultCollectionName);
    }
    public OperationService()
    {
        _settings = MongoClientSettings.FromConnectionString(DefaultConnection);
        _client = new MongoClient(_settings);
        _database = _client.GetDatabase(DefaultCollectionName);
        Collection = _database.GetCollection<T>(DefaultCollectionName);
    }

    // TODO: implement OperationOptionsBuilder
    // public OperationService(string optionsName)
    // {
    //     OperationOptionsBuilder<T>.Options.Name = optionsName;
    //     _settings = MongoClientSettings.FromConnectionString(OperationOptionsBuilder<T>.Options.Connection);
    //     _client = new MongoClient(_settings);
    //     _database = _client.GetDatabase(OperationOptionsBuilder<T>.Options.DatabaseName);
    //     Collection = _database.GetCollection<T>(OperationOptionsBuilder<T>.Options.CollectionName);
    //     
    // }
    
    // public OperationService(string connection, string nameDatabase)
    // {
    //     _settings = MongoClientSettings.FromConnectionString(connection);
    //     _client = new MongoClient(_settings);
    //     _database = _client.GetDatabase(nameDatabase);
    //     Collection = _database.GetCollection<T>($"{typeof(Credit)}s");
    // }
    //
    // public OperationService(OperationOptionsBuilder<T> optionsBuilder)
    // {
    //     Initialize(optionsBuilder);
    // }
    //
    // protected OperationService<T> Initialize(OperationOptionsBuilder<T> optionsBuilder)
    // {
    //     OnConfiguring(optionsBuilder);
    //     _settings = MongoClientSettings.FromConnectionString(OperationOptionsBuilder<T>.Options.Connection);
    //     _client = new MongoClient(_settings);
    //     _database = _client.GetDatabase(OperationOptionsBuilder<T>.Options.DatabaseName);
    //     Collection = _database.GetCollection<T>(OperationOptionsBuilder<T>.Options.CollectionName);
    //     return this;
    // }
    // protected virtual void OnConfiguring(OperationOptionsBuilder<T> optionsBuilder)
    // {
    //     optionsBuilder.Set(optionsBuilder);
    // }
}

public class OperationOptionsBuilder<T> where T : class
{
    [Required] public string Name { get; set; } = "Default";
    public string Connection { get; set; } = OperationService<T>.DefaultConnection;
    public string DatabaseName { get; set; } = "";
    public string CollectionName { get; set; } = "";
    public static OperationOptionsBuilder<T> Options { get; private set; } = new ();

    internal void Set(OperationOptionsBuilder<T> options)
    {
        Options = options;
    }
}
