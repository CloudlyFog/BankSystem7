using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BankSystem7.Services;

public class Logger 
{
    private const string Path = @"";
    public async Task Log(string message, bool async)
    {
        if (message is null)
            throw new Exception("Passed message is null. Please check accuracy passing data to message.");
        if (async)
            await File.WriteAllTextAsync(Path, message);
        else
            File.WriteAllText(Path, message);
    }
}