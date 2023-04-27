using BankSystem7.Models;
using BankSystem7.Services.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Threading.Tasks;

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
    public static ServiceConfiguration<TUser, TCard, TBankAccount, TBank, TCredit>? ServiceConfiguration { get; private set; }

    public ServiceConfigurationMiddleware(RequestDelegate next, ConfigurationOptions options) : base(next, options)
    {
        Options = options;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        ServiceConfiguration = CreateInstance(Options);
        await context.Response.WriteAsync("Inside of new custom middleware");
        await _next(context); 
    }
}

public static class ServiceConfigurationMiddlewareExtensions
{
    public static IApplicationBuilder UseNationBankSystem<TUser, TCard, TBankAccount, TBank, TCredit>(this IApplicationBuilder builder, Action<ConfigurationOptions> options) 
        where TUser : User
        where TCard : Card
        where TBankAccount : BankAccount
        where TBank : Bank
        where TCredit : Credit
    {
        return builder.UseMiddleware<ServiceConfigurationMiddleware<TUser, TCard, TBankAccount, TBank, TCredit>>(options);
    }
}