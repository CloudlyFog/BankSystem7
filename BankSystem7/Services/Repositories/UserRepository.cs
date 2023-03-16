// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

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
        private bool _disposed;
        public UserRepository()
        {
            _bankAccountRepository = new BankAccountRepository();
        }
        public UserRepository(string connection) : base(connection)
        {
            _bankAccountRepository = new BankAccountRepository(connection);
        }
        public UserRepository(BankAccountRepository repository) : base(repository)
        {
            _bankAccountRepository = repository;
        }
        public ExceptionModel Create(User item)
        {
            if (item is null)
                return ExceptionModel.OperationFailed;
            if (Exist(x => x.ID == item.ID))//if user isn`t exist method will send false
                return ExceptionModel.OperationFailed;
            item.Password = new Password().GetHash(item.Password);
            item.Authenticated = true;
            item.ID = Guid.NewGuid();
            var bankAccountModel = new BankAccount()
            {
                ID = item.BankAccountID,
                UserID = item.ID
            };
            var operation = _bankAccountRepository.Create(bankAccountModel);
            if (operation != ExceptionModel.Successfully)
                return operation;
            Users.Add(item);
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
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
            }
            _bankAccountRepository = null;
            _disposed = true;
        }

        public ExceptionModel Delete(User item)
        {
            if (item is null)
                return ExceptionModel.OperationFailed;
            if (Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;
            var bankAccountModel = _bankAccountRepository.Get(x => x.UserID == item.ID);
            var operation = _bankAccountRepository.Delete(bankAccountModel);
            if (operation != ExceptionModel.Successfully)
                return operation;
            Users.Remove(item);
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return ExceptionModel.Successfully;
        }

        public bool Exist(Expression<Func<User, bool>> predicate) => Users.AsNoTracking().Any(predicate);

        public IEnumerable<User> All => Users.AsNoTracking();

        public User? Get(Expression<Func<User, bool>> predicate) => Users.AsNoTracking().FirstOrDefault(predicate);

        public ExceptionModel Update(User item)
        {
            if (item is null)
                return ExceptionModel.OperationFailed;
            if (Exist(x => x.ID == item.ID))
                return ExceptionModel.OperationFailed;
            Users.Update(item);
            try
            {
                SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            return ExceptionModel.Successfully;
        }

        ~UserRepository()
        {
            Dispose(false);
        }
    }
}
