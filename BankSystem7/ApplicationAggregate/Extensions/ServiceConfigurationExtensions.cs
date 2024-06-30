using BankSystem7.ApplicationAggregate.Data;
using BankSystem7.BankAggregate;
using BankSystem7.BankAggregate.BankAccountAggregate;
using BankSystem7.BankAggregate.CardAggregate;
using BankSystem7.BankAggregate.CreditAggregate;
using BankSystem7.Configuration;
using BankSystem7.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace BankSystem7.ApplicationAggregate.Extensions;

public static class ServiceConfigurationExtensions
{
    public static IServiceCollection AddBankSystem(this IServiceCollection services, ConfigurationOptions options)
    {
        services.AddScoped<IMemoryCache>();
        services.AddSerilog(o =>
        {
            o.WriteTo.Console().MinimumLevel.Warning();
        });
        services.AddDbContext<GenericDbContext>(o => SetDbContext(services, options));
        services.AddDbContext<ApplicationContext>(o => SetDbContext(services, options));
        services.AddDbContext<BankContext>(o => SetDbContext(services, options));

        services.AddScoped<IBankRepository, BankRepository>();
        services.AddScoped<ICreditRepository, CreditRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }

    public static IServiceCollection AddBankSystem(this IServiceCollection services, Action<ConfigurationOptions> options)
    {
        var resultOptions = new ConfigurationOptions();
        options?.Invoke(resultOptions);

        return services.AddBankSystem(resultOptions);
    }

    private static DbContextOptionsBuilder SetDbContext(IServiceCollection services, ConfigurationOptions options)
    {
        var builder = new DbContextOptionsBuilder();
        var connection = ServiceSettings.GetConnectionString(options.ConnectionConfiguration.DatabaseManagementSystemType,
                                                             options.ConnectionConfiguration,
                                                             options.Credentials);

        return options.ConnectionConfiguration.DatabaseManagementSystemType switch
        {
            DatabaseManagementSystemType.MicrosoftSqlServer => builder.UseSqlServer(connection),
            DatabaseManagementSystemType.PostgreSql => builder.UseNpgsql(connection),
            DatabaseManagementSystemType.MySql => builder.UseMySQL(connection),
            _ => builder.UseMemoryCache(services.BuildServiceProvider().GetRequiredService<IMemoryCache>()),
        };
    }
}