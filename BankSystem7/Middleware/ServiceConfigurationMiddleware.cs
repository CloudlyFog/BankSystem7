using BankSystem7.Models;
using BankSystem7.Services;
using BankSystem7.Services.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Middleware;


/// <summary>
/// before using this class as middleware define <see cref="DbLoggerCategory.Database.Connection"/>
/// default value for <see cref="DbLoggerCategory.Database.Connection"/> is connection to database
/// </summary>
public sealed class ServiceConfigurationMiddleware<TUser> : ServiceConfiguration<TUser>, IDisposable where TUser : User
{
    private readonly RequestDelegate _next;

    private bool _disposed;

    public ServiceConfigurationMiddleware(RequestDelegate next, ConfigurationOptions options) : base(next, options)
    {
        Options = options;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next.Invoke(context);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            BankAccountRepository?.Dispose();
            BankRepository?.Dispose();
            CardRepository?.Dispose();
        }

        BankAccountRepository = null;
        BankRepository = null;
        CardRepository = null;
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ServiceConfigurationMiddleware()
    {
        Dispose(false);
    }
}