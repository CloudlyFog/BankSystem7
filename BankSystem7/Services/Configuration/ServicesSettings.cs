namespace BankSystem7.Services.Configuration;

public class ServicesSettings
{
    public const string DefaultConnection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=Test;
            Integrated Security=True;Persist Security Info=False;Pooling=False;
            MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False";

    public const string DefaultDataSource = "maxim";
    private const string DefaultDatabaseName = "Test";
    public static bool EnsureDeleted { get; set; }
    public static bool EnsureCreated { get; set; } = true;
    public static string? Connection { get; set; }
    public static bool Ensured { get; set; }
    internal static bool InitializeAccess { get; set; }

    public static void SetConnection(string? connection = null, string? databaseName = DefaultDatabaseName, string? dataSource = DefaultDataSource)
    {
        if (connection is not null && connection != string.Empty)
        {
            Connection = connection;
            return;
        }

        if (databaseName is null || dataSource is null)
        {
            Connection = DefaultConnection;
            return;
        }

        Connection = @$"Server=localhost\\SQLEXPRESS;Data Source={dataSource};Initial Catalog={databaseName};
            Integrated Security=True;Persist Security Info=False;Pooling=False;
            MultipleActiveResultSets=False; Encrypt=False;TrustServerCertificate=False";
    }
}