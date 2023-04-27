using System.Reflection;

namespace BankSystem7.Extensions
{
    public static class BankExtensions
    {
        public static TBank SetValuesTo<TBank>(this TBank from, TBank to, PropertyInfo[]? excludeProperties = null)
        {
            foreach (var toProperty in to.GetType().GetProperties())
            {
                if (excludeProperties?.FirstOrDefault(x => x.Name == toProperty.Name) is not null)
                    continue;
                var fromPropertyValue = from.GetType().GetProperty(toProperty.Name).GetValue(from);
                toProperty.SetValue(to, fromPropertyValue);
            }

            return to;
        }

        public static TBank GetValuesFrom<TBank>(this TBank to, TBank from, PropertyInfo[]? excludeProperties = null)
        {
            return from.SetValuesTo(to, excludeProperties);
        }
    }
}