using BankSystem7.Services;
using Microsoft.EntityFrameworkCore;
using Standart7.Models;

namespace BankSystem7.AppContext
{
    internal sealed class BankAccountContext : DbContext
    {
        private readonly string _connection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=CabManagementSystem;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";
        private readonly string _table = "CabManagementSystem";
        private readonly string _dataSource = "maxim";
        private readonly DatabaseType _type = DatabaseType.MSSQL;
        
        public BankAccountContext(string connection)
        {
            _connection = connection;
            DatabaseHandle();
        }

        public BankAccountContext(DatabaseType type, string connection)
        {
            _connection = connection;
            _type = type;
            DatabaseHandle();

        }

        public BankAccountContext(string database, string dataSource)
        {
            DatabaseHandle();
            _connection =
                @$"Server=localhost\\SQLEXPRESS;Data Source={dataSource};Initial Catalog={database};
                Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";
        }

        public BankAccountContext() => DatabaseHandle();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            switch (_type)
            {
                case DatabaseType.MySql:
                    optionsBuilder.UseMySQL(_connection);
                    break;
                case DatabaseType.MSSQL:
                    optionsBuilder.UseSqlServer(_connection);
                    break;
                case DatabaseType.PostgreSql:
                    optionsBuilder.UseNpgsql(_connection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public DbSet<User> Users { get; private set; } = null!;
        public DbSet<BankAccount> BankAccounts { get; private set; } = null!;

        private void DatabaseHandle()
        {
            if (BankServicesOptions.EnsureDeleted)
                Database.EnsureDeleted();
            if (BankServicesOptions.EnsureCreated)
                Database.EnsureCreated();
        }
    }

    public enum DatabaseType
    {
        MSSQL,
        PostgreSql,
        MySql
    }
}
