using System.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Services.Interfaces;
using Standart7.Models;

namespace BankSystem7.Services.Repositories
{
    public sealed class BankAccountRepository : IRepository<BankAccount>
    {
        private BankRepository _bankRepository;
        private BankAccountContext _bankAccountContext;
        private BankContext _bankContext;
        private bool _disposedValue;
        private const string Connection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=CabManagementSystem;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";

        public BankAccountRepository()
        {
            _bankAccountContext = new BankAccountContext(Connection);
            _bankContext = _bankRepository.BankContext;
            SetBankServicesOptions();
            _bankRepository = new BankRepository(Connection);
        }
        public BankAccountRepository(BankRepository bankRepository)
        {
            _bankRepository = bankRepository;
        }
        public BankAccountRepository(string connection)
        {
            _bankAccountContext = BankServicesOptions.BankAccountContext ?? new BankAccountContext(connection);
            _bankContext = BankServicesOptions.BankContext ?? new BankContext(connection);
            SetBankServicesOptions();
            _bankRepository = new BankRepository(connection);
        }

        [Obsolete("This constructor has bad implementation. We don't recommend to use it.")]
        public BankAccountRepository(DatabaseType type, string connection)
        {
            _bankAccountContext = new BankAccountContext(connection);
            _bankContext = new BankContext(connection);
            _bankRepository = new BankRepository(connection);
        }

        // Public implementation of Dispose pattern callable by consumers.
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
                _bankAccountContext.Dispose();
                _bankContext.Dispose();
                _bankRepository.Dispose();
            }
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _bankContext = null;
            _bankAccountContext = null;
            _bankRepository = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            _disposedValue = true;
        }

        public async Task<ExceptionModel> Transfer(BankAccount? from, BankAccount? to, decimal transferAmount)
        {
            if (from is null || to is null || transferAmount <= 0)
                return ExceptionModel.OperationFailed;
            
            if (!Exist(x => x.ID == from.ID) || !Exist(x => x.ID == to.ID))
                return ExceptionModel.OperationFailed;
            
            using var transaction = _bankContext.Database.BeginTransaction(IsolationLevel.RepeatableRead);
            await WithdrawAsync(from, transferAmount);
            await AccrualAsync(to, transferAmount);
            transaction.Commit();
            return ExceptionModel.Successfully;
        }

        /// <summary>
        /// asynchronously accrual money on account with the same user id
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amountAccrual"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        private async Task AccrualAsync(BankAccount? item, decimal amountAccrual)
        {
            CheckBankAccount(item);

            var operation = new Operation()
            {
                BankID = item.BankID,
                SenderID = item.BankID,
                ReceiverID = item.UserID,
                TransferAmount = amountAccrual,
                OperationKind = OperationKind.Accrual
            };
            var createOperation = _bankContext.CreateOperation(operation, OperationKind.Accrual);
            
            if (createOperation != ExceptionModel.Successfully)
                throw new Exception($"Operation can't create due to exception: {createOperation}");
            
            var bank = _bankRepository.Get(x => x.BankID == operation.BankID);
            if (_bankRepository.BankAccountAccrual(item, bank, operation) != ExceptionModel.Successfully)
                throw new Exception($"Failed withdraw money from {bank}");
        }

        /// <summary>
        /// withdraw money from account with the same user id
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amountAccrual"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        private async Task WithdrawAsync(BankAccount? item, decimal amountAccrual)
        {
            CheckBankAccount(item);

            var operation = new Operation()
            {
                BankID = item.BankID,
                SenderID = item.UserID,
                ReceiverID = item.BankID,
                TransferAmount = amountAccrual,
                OperationKind = OperationKind.Withdraw
            };
            
            var createOperation = _bankContext.CreateOperation(operation, OperationKind.Withdraw);
            if (createOperation != ExceptionModel.Successfully)
                throw new Exception($"Operation can't create due to exception: {createOperation}");
            
            var bank = _bankRepository.Get(x => x.BankID == operation.BankID);
            var withdraw = _bankRepository.BankAccountWithdraw(item, bank, operation);
            if (withdraw != ExceptionModel.Successfully)
                throw new Exception($"Failed withdraw money from {bank}\nException: {withdraw}");
        }

        /// <summary>
        /// adds bank account of user
        /// </summary>
        /// <param name="item"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public ExceptionModel Create(BankAccount item)
        {
            if (item is null || Exist(x => x.ID == item.ID))
                return ExceptionModel.VariableIsNull;
            _bankAccountContext.BankAccounts.Add(item);
            _bankAccountContext.SaveChanges();
            return ExceptionModel.Successfully;
        }

        public IEnumerable<BankAccount> All => _bankAccountContext.BankAccounts.AsNoTracking();

        public BankAccount? Get(Expression<Func<BankAccount, bool>> predicate) => _bankAccountContext.BankAccounts.AsNoTracking().FirstOrDefault(predicate);

        /// <summary>
        /// updates only BankAccount. Referenced user won't be changed
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ExceptionModel Update(BankAccount item)
        {
            if (item is null || Exist(x => x.ID == item.ID))
                return ExceptionModel.VariableIsNull;
            _bankAccountContext.BankAccounts.Update(item);
            _bankAccountContext.SaveChanges();
            return ExceptionModel.Successfully;
        }

        /// <summary>
        /// removes bank account of user from database
        /// </summary>
        /// <param name="item"></param>
        /// <returns>object of <see cref="ExceptionModel"/></returns>
        public ExceptionModel Delete(BankAccount item)
        {
            if (item is null || !Exist(x => x.ID == item.ID))
                return ExceptionModel.VariableIsNull;
            _bankAccountContext.BankAccounts.Remove(item);
            _bankAccountContext.SaveChanges();
            return ExceptionModel.Successfully;
        }

        public bool Exist(Expression<Func<BankAccount, bool>> predicate)
            => _bankAccountContext.BankAccounts.AsNoTracking().Any(predicate);

        public ExceptionModel Update(BankAccount item, User user, Card card)
        {
            if (item is null || !Exist(x => x.ID == item.ID))
                return ExceptionModel.VariableIsNull;

            if (user is null || !_bankRepository.Exist(x => x.ID == user.ID))
                return ExceptionModel.VariableIsNull;
            
            if (card is null || !_bankContext.Cards.AsNoTracking().Any(x => x.ID == card.ID))
                return ExceptionModel.VariableIsNull;

            _bankAccountContext.ChangeTracker.Clear();
            using var transaction = _bankAccountContext.Database.BeginTransaction();
            _bankAccountContext.BankAccounts.Update(item);
            _bankAccountContext.Users.Update(user);
            _bankAccountContext.SaveChanges(); 
            transaction.Commit();

            return ExceptionModel.Successfully;
        }

        private void CheckBankAccount(BankAccount item)
        {
            if (item is null)
                throw new Exception("Passed instance of BankAccount is null.");
            
            if (!_bankAccountContext.Users.AsNoTracking().Any(x => x.ID == item.UserID))
                throw new Exception("Doesn't exist user with specified ID in the database.");

            if (!Exist(x => x.ID == item.ID)) 
                throw new Exception($"Doesn't exist bank with id {{{item.ID}}}");
        }

        private void SetBankServicesOptions()
        {
            BankServicesOptions.BankContext = _bankContext;
            BankServicesOptions.BankAccountContext = _bankAccountContext;
        }

        ~BankAccountRepository()
        {
            Dispose(false);
        }
    }
}