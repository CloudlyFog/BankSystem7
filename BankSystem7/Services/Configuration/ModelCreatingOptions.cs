namespace BankSystem7.Services.Configuration;

public class ModelCreatingOptions
{
    internal static ModelConfiguration? ModelConfiguration { get; set; }
    internal static List<ModelConfiguration>? ModelConfigurations { get; set; } = new();
    internal static bool LastModelConfiguration { get; set; }
}