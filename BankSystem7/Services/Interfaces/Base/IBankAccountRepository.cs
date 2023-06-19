using BankSystem7.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankSystem7.Services.Interfaces.Base;

public interface IBankAccountRepository<TBankAccount> : IRepository<TBankAccount>,
    IReaderServiceWithTracking<TBankAccount>, IRepositoryAsync<TBankAccount> where TBankAccount : BankAccount
{

}
