using BankSystem7.AppContext;
using BankSystem7.Models;
using BankSystem7.Services.Interfaces;

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

    public static IServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>? ServiceConfiguration { get; internal set; }
    internal static BankContext<TUser, TCard, TBankAccount, TBank, TCredit>? BankContext { get; set; }
    internal static ApplicationContext<TUser, TCard, TBankAccount, TBank, TCredit>? ApplicationContext { get; set; }
}