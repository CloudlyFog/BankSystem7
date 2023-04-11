using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;

namespace BankSystem7.Services;

public sealed class BankServicesOptions<TUser> where TUser : User
{
    public static bool EnsureCreated { get; set; }
    public static bool EnsureDeleted { get; set; }
    public static string? Connection { get; set; }
    public static bool Ensured { get; set; }
    public static ServiceConfiguration<TUser>? ServiceConfiguration { get; set; }

    internal static BankContext<TUser>? BankContext { get; set; } 
    internal static ApplicationContext<TUser>? ApplicationContext { get; set; }
}