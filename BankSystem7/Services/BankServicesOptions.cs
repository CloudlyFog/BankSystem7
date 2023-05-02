using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Configuration;

namespace BankSystem7.Services;

public sealed class BankServicesOptions<TUser, TCard, TBankAccount, TBank, TCredit>
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private BankServicesOptions()
    {
        
    }
    public static bool EnsureCreated { get; set; }
    public static bool EnsureDeleted { get; set; }
    public static string? Connection { get; set; }
    public static bool Ensured { get; set; }
    public static ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>? ServiceConfiguration { get; set; }

    internal static bool InitializeAccess { get; set; } = false;
    internal static ModelConfiguration? ModelConfiguration { get; set; }
    internal static BankContext<TUser, TCard, TBankAccount, TBank, TCredit>? BankContext { get; set; }
    internal static ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>? ApplicationContext { get; set; }
}