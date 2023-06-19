using BankSystem7.Models;

namespace BankSystem7.Services.Interfaces.Base;

public interface IBankAccountRepository<TBankAccount> : IRepository<TBankAccount>,
    IReaderServiceWithTracking<TBankAccount>, IRepositoryAsync<TBankAccount> where TBankAccount : BankAccount
{
}