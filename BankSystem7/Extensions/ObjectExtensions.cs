using System.Text;

namespace BankSystem7.Extensions
{
    internal static class ObjectExtensions
    {
        public static bool EqualsTo<TItem, TCompare>(this TItem obj1, TCompare obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;

            foreach (var thisProp in obj1.GetType().GetProperties())
            {
                var objProp = obj2.GetType().GetProperty(thisProp.Name);
                if (thisProp?.GetValue(obj1)?.Equals(objProp?.GetValue(obj2)) == false)
                    return false;
            }
            return true;
        }

        public static string ConvertToString<T>(this T item)
        {
            if (item is null)
                return "Passed item is null.";
            var sb = new StringBuilder();
            foreach (var prop in item.GetType().GetProperties())
                sb.AppendLine($"{prop.Name}: {prop.GetValue(item)}");
            return sb.ToString();
        }
    }
}