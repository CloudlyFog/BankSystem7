using Microsoft.EntityFrameworkCore;
using Standart7.Models;

namespace BankSystem7.AppContext
{
    internal sealed class BankAccountContext : DbContext
    {
        private readonly string connection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=CabManagementSystem;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";
        private readonly string table = "CabManagementSystem";
        private readonly string dataSource = "maxim";
        private readonly DatabaseType type = DatabaseType.MSSQL;
        
        public BankAccountContext(string connection)
        {
            this.connection = connection;
            Database.EnsureCreated();
        }

        public BankAccountContext(DatabaseType type, string connection)
        {
            this.connection = connection;
            this.type = type;
            Database.EnsureCreated();
        }

        public BankAccountContext(string database, string dataSource)
        {
            connection =
                @$"Server=localhost\\SQLEXPRESS;Data Source={dataSource};Initial Catalog={database};
                Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";
        }
        public BankAccountContext() => Database.EnsureCreated();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            switch (type)
            {
                case DatabaseType.MySql:
                    optionsBuilder.UseMySQL(connection);
                    break;
                case DatabaseType.MSSQL:
                    optionsBuilder.UseSqlServer(connection);
                    break;
                case DatabaseType.PostgreSql:
                    optionsBuilder.UseNpgsql(connection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public DbSet<User> Users { get; private set; } = null!;
        public DbSet<BankAccount> BankAccounts { get; private set; } = null!;
    }

    public enum DatabaseType
    {
        MSSQL,
        PostgreSql,
        MySql
    }
}
