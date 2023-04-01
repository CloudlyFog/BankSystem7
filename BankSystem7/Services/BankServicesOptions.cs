using BankSystem7.AppContext;
using BankSystem7.Services.Configuration;

namespace BankSystem7.Services;

public sealed class BankServicesOptions
{
    public static bool EnsureCreated { get; set; }
    public static bool EnsureDeleted { get; set; }
    public static string? Connection { get; set; }
    public static ServiceConfiguration? ServiceConfiguration { get; set; }

    internal static BankContext? BankContext { get; set; } 
    internal static ApplicationContext? ApplicationContext { get; set; }
}