using System.Data.HashFunction.Jenkins;
using System.Text;

namespace BankSystem7.UserAggregate;

public static class SecurePasswordHasher
{
    private static readonly IJenkinsOneAtATime _jenkins = JenkinsOneAtATimeFactory.Instance.Create();

    /// <summary>
    /// Creates a hash from a password.
    /// </summary>
    /// <param name="password">The password.</param>
    /// <returns>The hash.</returns>
    public static string Hash(string password)
    {
        return _jenkins.ComputeHash(Encoding.UTF8.GetBytes(password)).AsHexString();
    }
}