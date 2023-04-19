using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Middleware;

/// <summary>
/// before using this class as middleware define <see cref="DbLoggerCategory.Database.Connection"/>
/// default value for <see cref="DbLoggerCategory.Database.Connection"/> is connection to database
/// </summary>
public sealed class ServiceConfigurationMiddleware<TUser, TCard, TBankAccount, TBank, TCredit> : ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>, IDisposable
    where TUser : User
    where TCard : Card
    where TBankAccount : BankAccount
    where TBank : Bank
    where TCredit : Credit
{
    private readonly RequestDelegate _next;

    public ServiceConfigurationMiddleware(RequestDelegate next, ConfigurationOptions options) : base(next, options)
    {
        Options = options;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next.Invoke(context);
    }
}