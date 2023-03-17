using BankSystem7.AppContext;

namespace BankSystem7.Services;

public class BankServicesOptions
{
    public static bool EnsureCreated { get; set; }
    public static bool EnsureDeleted { get; set; }
    public static string Connection { get; set; } = null;

    internal static BankContext? BankContext { get; set; }
    internal static BankAccountContext? BankAccountContext { get; set; }
}