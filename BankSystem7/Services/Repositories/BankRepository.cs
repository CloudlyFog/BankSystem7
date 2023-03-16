using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Standart7.Models;
using System.Linq.Expressions;
using BankSystem7.AppContext;

namespace BankSystem7.Services.Repositories
{
    public sealed class BankRepository : IRepository<Bank>
    {
        private BankAccountContext _bankAccountContext;
        private BankContext _bankContext;
        internal BankContext? BankContext { get; set; }

        private bool _disposedValue;
        public BankRepository()
        {
            _bankAccountContext = new BankAccountContext();
            _bankContext = new BankContext();
            BankContext = _bankContext;
        }
        public BankRepository(string connection)
        {
            _bankAccountContext = new BankAccountContext(connection);
            _bankContext = new BankContext(connection);
            BankContext = _bankContext;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern.
        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                if (BankContext is null) return;
                _bankAccountContext.Dispose();
                _bankContext.Dispose();
                BankContext.Dispose();
            }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _bankContext = null;
            _bankAccountContext = null;
            BankContext = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            _disposedValue = true;
        }

        /// <summary>
        /// asynchronously accrual money to user bank account from bank's account
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="bank"></param>
        /// <param name="operation"></param>
        /// <exception cref="Exception"></exception>
        public ExceptionModel BankAccountAccrual(BankAccount bankAccount, Bank bank, Operation operation)
        {
            if (bankAccount is null || bank is null || !Exist(x => x.ID == bank.ID))
                return ExceptionModel.VariableIsNull;
            if (operation.OperationStatus != StatusOperationCode.Successfully)
                return (ExceptionModel)operation.OperationStatus.GetHashCode();

            var user = _bankContext.Users.FirstOrDefault(x => x.ID == bankAccount.UserID);
            if (user is null)
                return ExceptionModel.VariableIsNull;

            var card = _bankContext.Cards.FirstOrDefault(x => x.BankAccountID == user.BankAccountID);
            if (card is null)
                return ExceptionModel.VariableIsNull;

            bank.AccountAmount -= operation.TransferAmount;
            bankAccount.BankAccountAmount += operation.TransferAmount;
            user.BankAccountAmount = bankAccount.BankAccountAmount;
            card.Amount = user.BankAccountAmount;
            _bankContext.ChangeTracker.Clear();
            _bankContext.BankAccounts.Update(bankAccount);
            _bankContext.Banks.Update(bank);
            _bankContext.Users.Update(user);
            _bankContext.Cards.Update(card);
            try
            {
                _bankContext.SaveChanges();
                _bankContext.DeleteOperation(operation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return ExceptionModel.Successfully;
        }

        /// <summary>
        /// asynchronously withdraw money from user bank account and accrual to bank's account
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="bank"></param>
        /// <param name="operation"></param>
        /// <exception cref="Exception"></exception>
        public ExceptionModel BankAccountWithdraw(BankAccount bankAccount, Bank bank, Operation operation)
        {
            if (bankAccount is null || bank is null)
                return ExceptionModel.VariableIsNull;
            if (operation.OperationStatus != StatusOperationCode.Successfully)
                return (ExceptionModel)operation.OperationStatus.GetHashCode();

            
            var user = _bankContext.Users.FirstOrDefault(x => x.ID == bankAccount.UserID);
            if (user is null)
                return ExceptionModel.VariableIsNull;

            var card = _bankContext.Cards.FirstOrDefault(x => x.BankAccountID == user.BankAccountID);
            if (card is null)
                return ExceptionModel.VariableIsNull;

            bank.AccountAmount += operation.TransferAmount;
            bankAccount.BankAccountAmount -= operation.TransferAmount;
            user.BankAccountAmount = bankAccount.BankAccountAmount;
            card.Amount = user.BankAccountAmount;
            _bankContext.ChangeTracker.Clear();
            _bankContext.BankAccounts.Update(bankAccount);
            _bankContext.Banks.Update(bank);
            _bankContext.Users.Update(user);
            _bankContext.Cards.Update(card);
            try
            {
                _bankContext.SaveChanges();
                _bankContext.DeleteOperation(operation); // doesn't exist operation
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            return ExceptionModel.Successfully;
        }

        public ExceptionModel Create(Bank item)
        {
            if (item is null || !Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;

            _bankContext.Add(item);
            _bankContext.SaveChanges();
            return ExceptionModel.Successfully;
        }

        public ExceptionModel Delete(Bank item)
        {
            if (item is null || !Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;

            _bankContext.Remove(item);
            _bankContext.SaveChanges();
            return ExceptionModel.Successfully;
        }

        public bool Exist(Expression<Func<Bank, bool>> predicate) => _bankContext.Banks.AsNoTracking().Any(predicate);

        public IEnumerable<Bank> All => _bankContext.Banks.AsNoTracking();

        public Bank? Get(Expression<Func<Bank, bool>> predicate) => _bankContext.Banks.AsNoTracking().FirstOrDefault(predicate);

        /// <summary>
        /// repays user's credit
        /// removes from the table field with credit's data of user
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="credit"></param>
        /// <returns></returns>
        public ExceptionModel RepayCredit(BankAccount bankAccount, Credit credit) => _bankContext.RepayCredit(bankAccount, credit);

        /// <summary>
        /// repays user's credit
        /// removes from the table field with credit's data of user
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="credit"></param>
        /// <returns></returns>
        public ExceptionModel TakeCredit(BankAccount bankAccount, Credit credit) => _bankContext.TakeCredit(bankAccount, credit);

        public ExceptionModel Update(Bank item)
        {
            if (item is null || !Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;

            _bankContext.Banks.Update(item);
            _bankContext.SaveChanges();
            return ExceptionModel.Successfully;
        }

        ~BankRepository()
        {
            Dispose(false);
        }
    }
}
