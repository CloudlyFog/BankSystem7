namespace Synchronizer.Services;

public class SyncService
{
    public static T[] Sync<T>(IEnumerable<T> resource1, IEnumerable<T> resource2)
    {
        if (resource1 is null || resource2 is null)
            return Array.Empty<T>();
        
        if (resource1.Count() < 1 || resource2.Count() < 1)
            return Array.Empty<T>();
        
        var allData = Enumerable.Empty<T>();
        foreach (var item in resource1)
            allData = allData.Append(item);

        foreach (var item in resource2)
            allData = allData.Append(item);

        allData = allData.Distinct();
        return allData.ToArray();
    }
}