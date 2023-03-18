// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Data;
using Standart7.Models;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankSystem7.Services.Repositories
{
    public sealed class UserRepository : ApplicationContext, IRepository<User>
    {
        private BankAccountRepository _bankAccountRepository;
        private BankRepository _bankRepository;
        private CardRepository _cardRepository;
        private BankAccountContext _bankAccountContext;
        private bool _disposed;
        public UserRepository()
        {
            _bankAccountRepository = new BankAccountRepository(BankServicesOptions.Connection);
            _bankAccountContext = BankServicesOptions.BankAccountContext ??
                                  new BankAccountContext(BankServicesOptions.Connection);
            _bankRepository = new BankRepository(BankServicesOptions.Connection);
            _cardRepository = new CardRepository(_bankAccountRepository);
        }
        public UserRepository(string connection) : base(connection)
        {
            _bankAccountRepository = new BankAccountRepository(connection);
            _bankAccountContext = BankServicesOptions.BankAccountContext ??
                                  new BankAccountContext(BankServicesOptions.Connection);
            _bankRepository = new BankRepository(connection);
            _cardRepository = new CardRepository(_bankAccountRepository);
        }
        public UserRepository(BankAccountRepository repository) : base(repository)
        {
            _bankAccountRepository = repository;
            _bankAccountContext = BankServicesOptions.BankAccountContext ??
                                  new BankAccountContext(BankServicesOptions.Connection);
            _bankRepository = new BankRepository(BankServicesOptions.Connection);
            _cardRepository = new CardRepository(_bankAccountRepository);
        }
        public ExceptionModel Create(User item)
        {
            if (item is null)
                return ExceptionModel.OperationFailed;
            
            //if user isn`t exist method will send false
            if (Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;
            
            item.Authenticated = true;

            using var userCreationTransaction = _bankAccountContext.Database.CurrentTransaction ??
                                                _bankAccountContext.Database.BeginTransaction(IsolationLevel
                                                    .RepeatableRead);
            
            // before save instance of BankAccount need save instance of User
            Users.Add(item);
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                userCreationTransaction.Rollback();
                throw;
            }
            
            var createBankAccountOperation = _bankAccountRepository.Create(item.Card.BankAccount);
            if (createBankAccountOperation != ExceptionModel.Successfully)
            {
                userCreationTransaction.Rollback();
                return createBankAccountOperation;
            }
            
            
            userCreationTransaction.Commit();
            return ExceptionModel.Successfully;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _bankAccountRepository.Dispose();
                _bankRepository.Dispose();
                _cardRepository.Dispose();
                _bankAccountContext.Dispose();
            }
            _bankAccountRepository = null;
            _bankRepository = null;
            _cardRepository = null;
            _bankAccountContext = null;
            _disposed = true;
        }

        public ExceptionModel Delete(User item)
        {
            if (item is null)
                return ExceptionModel.OperationFailed;
            if (!Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;
            
            using var userDeleteTransaction = _bankAccountContext.Database.CurrentTransaction ??
                                              _bankAccountContext.Database.BeginTransaction(IsolationLevel
                                                  .RepeatableRead);
            
            Users.Remove(item);
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                userDeleteTransaction.Rollback();
                throw;
            }
            
            var bankAccountDeleteOperation = _bankAccountRepository.Delete(item.Card.BankAccount);
            if (bankAccountDeleteOperation != ExceptionModel.Successfully)
            {
                userDeleteTransaction.Rollback();
                return bankAccountDeleteOperation;
            }
            

            userDeleteTransaction.Commit();
            return ExceptionModel.Successfully;
        }

        public bool Exist(Expression<Func<User, bool>> predicate) => Users.AsNoTracking().Any(predicate);

        public IEnumerable<User> All => Users.AsNoTracking();

        public User? Get(Expression<Func<User, bool>> predicate)
        {
            var user = Users.AsNoTracking().FirstOrDefault(predicate);
            user.Card = _cardRepository.Get(x => x.UserID == user.ID);
            user.Card.BankAccount = _bankAccountRepository.Get(x => x.UserID == user.ID);
            user.Card.BankAccount.Bank = _bankRepository.Get(x => x.BankAccounts.Contains(user.Card.BankAccount));
            return user;
        }

        public ExceptionModel Update(User item)
        {
            if (item is null)
                return ExceptionModel.OperationFailed;
            if (!Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;
            using var userUpdateTransaction = _bankAccountContext.Database.CurrentTransaction ??
                                              _bankAccountContext.Database.BeginTransaction(IsolationLevel
                                                  .RepeatableRead);
            
            
            Users.Update(item);
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                userUpdateTransaction.Rollback();
                throw;
            }

            userUpdateTransaction.Commit();
            return ExceptionModel.Successfully;
        }

        ~UserRepository()
        {
            Dispose(false);
        }
    }
}
