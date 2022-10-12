using Microsoft.EntityFrameworkCore;
using Standart7.Models;
using System.Linq.Expressions;
using BankSystem7.AppContext;
using BankSystem7.Services.Interfaces;

namespace BankSystem7.Services.Repositories
{
    public sealed class CardRepository : IRepository<Card>
    {
        private CardContext _cardContext;
        private BankAccountRepository _bankAccountRepository;
        private bool _disposedValue;
        private readonly string _connection = @"Server=localhost\\SQLEXPRESS;Data Source=maxim;Initial Catalog=CabManagementSystem;Integrated Security=True;
            Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;
            Encrypt=False;TrustServerCertificate=False";

        public CardRepository()
        {
            _cardContext = new CardContext(_connection);
            _bankAccountRepository = new BankAccountRepository(_connection);
        }
        public CardRepository(bool initializable)
        {
            if (!initializable) return;
            _cardContext = new CardContext(_connection);
            _bankAccountRepository = new BankAccountRepository(_connection);
        }
        public CardRepository(bool initializable, BankAccountRepository bankAccountRepository)
        {
            if (!initializable) return;
            _bankAccountRepository = bankAccountRepository;
            _cardContext = new CardContext(_connection);
        }
        public CardRepository(bool initializable, BankAccountRepository bankAccountRepository, string connection)
        {
            if (!initializable) return;
            _connection = connection;
            _bankAccountRepository = bankAccountRepository;
            _cardContext = new CardContext(connection);
        }
        public CardRepository(string connection)
        {
            _connection = connection;
            _cardContext = new CardContext(connection);
            _bankAccountRepository = new BankAccountRepository(connection);
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Private implementation of Dispose pattern.
        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            if (disposing)
            {
                _cardContext.Dispose();
            }
            _cardContext = null;
            _bankAccountRepository = null;
            _disposedValue = true;
        }

        public async Task<ExceptionModel> Transfer(BankAccount? from, BankAccount? to, decimal transferAmount) 
            => await _bankAccountRepository.Transfer(from, to, transferAmount);

        /// <summary>
        /// use this method for designing cards
        /// </summary>
        /// <param name="card"></param>
        /// <param name="bankAccount"></param>
        /// <returns></returns>
        public async Task<ExceptionModel> CreateAsync(Card? card, BankAccount? bankAccount)
        {
            if (card is null || bankAccount is null)
                return ExceptionModel.VariableIsNull;
            if (card.Exception != Warning.NoRestrictions)
                return ExceptionModel.OperationRestricted;
            if (Exist(card.ID) || _bankAccountRepository.Exist(bankAccount.ID))
                return ExceptionModel.OperationFailed;
            card.BankAccountID = bankAccount.ID;
            bankAccount.CardID = card.ID;
            await Task.Run(async () =>
            {
                using var transaction = await _cardContext.Database.BeginTransactionAsync();
                _cardContext.Cards.Add(card);
                _bankAccountRepository.Create(bankAccount);
                await _cardContext.SaveChangesAsync();
                await transaction.CommitAsync();
            });
            return ExceptionModel.Successfull;
        }

        public async Task<ExceptionModel> DeleteAsync(Card? card, BankAccount? bankAccount)
        {
            if (card is null || bankAccount is null)
                return ExceptionModel.VariableIsNull;
            if (!Exist(card.ID) || _bankAccountRepository.Exist(bankAccount.ID))
                return ExceptionModel.OperationFailed;
            await Task.Run(async () =>
            {
                using var transaction = await _cardContext.Database.BeginTransactionAsync();
                _cardContext.Cards.Remove(card);
                _bankAccountRepository.Delete(bankAccount);
                await _cardContext.SaveChangesAsync();
                await transaction.CommitAsync();
            });
            return ExceptionModel.Successfull;
        }

        public async Task<ExceptionModel> UpdateAsync(Card? card, BankAccount? bankAccount)
        {
            if (card is null || bankAccount is null)
                return ExceptionModel.VariableIsNull;
            if (!Exist(card.ID) || _bankAccountRepository.Exist(bankAccount.ID))
                return ExceptionModel.OperationFailed;
            await Task.Run(async () =>
            {
                using var transaction = await _cardContext.Database.BeginTransactionAsync();
                _cardContext.Cards.Update(card);
                _bankAccountRepository.Update(bankAccount);
                await _cardContext.SaveChangesAsync();
                await transaction.CommitAsync();
            });
            return ExceptionModel.Successfull;
        }

        public async Task<ExceptionModel> UpdateAsync(BankAccount? bankAccount, User? user, Card? card)
        {
            if (card is null || bankAccount is null || user is null)
                return ExceptionModel.VariableIsNull;
            if (!Exist(card.ID) || _bankAccountRepository.Exist(bankAccount.ID) || !_cardContext.Users.Any(x => x.ID == user.ID))
                return ExceptionModel.OperationFailed;
            await Task.Run(async () =>
            {
                _cardContext.ChangeTracker.Clear();
                using var transaction = await _cardContext.Database.BeginTransactionAsync();
                _cardContext.Cards.Update(card);
                _cardContext.Users.Update(user);
                _bankAccountRepository.Update(bankAccount);
                await _cardContext.SaveChangesAsync();
                await transaction.CommitAsync();
            });
            return ExceptionModel.Successfull;
        }

        public ExceptionModel Update(Card item)
        {
            if (item is null)
                return ExceptionModel.VariableIsNull;
            if (Exist(item.ID))
                return ExceptionModel.OperationFailed;
            _cardContext.Cards.Update(item);
            _cardContext.SaveChanges();
            return ExceptionModel.Successfull;
        }

        /// <summary>
        /// use this method only for creating new card. 
        /// if you'll try to use this method in designing card you'll get big bug which 'll force you program work wrong.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public ExceptionModel Create(Card item)
        {
            if (item.Exception == Warning.AgeRestricted)
                return ExceptionModel.OperationRestricted;
            if (item is null)
                return ExceptionModel.VariableIsNull;
            if (Exist(item.ID))
                return ExceptionModel.OperationFailed;
            _cardContext.Cards.Add(item);
            _cardContext.SaveChanges();
            return ExceptionModel.Successfull;
        }

        public ExceptionModel Delete(Card item)
        {
            if (item is null)
                return ExceptionModel.VariableIsNull;
            if (!Exist(item.ID))
                return ExceptionModel.OperationFailed;
            _cardContext.Cards.Remove(item);
            _cardContext.SaveChanges();
            return ExceptionModel.Successfull;
        }

        public bool Exist(Guid id) => _cardContext.Cards.AsNoTracking().Any(card => card.ID == id);

        public bool Exist(Expression<Func<Card, bool>> predicate) => _cardContext.Cards.AsNoTracking().Any(predicate);

        public Card? Get(Guid id) => _cardContext.Cards.AsNoTracking().FirstOrDefault(x => x.ID == id);

        public Card? Get(Expression<Func<Card, bool>> predicate) => _cardContext.Cards.AsNoTracking().FirstOrDefault(predicate);

        public IEnumerable<Card> All => _cardContext.Cards.AsNoTracking();
        public void ChangeTrackerCardContext() => _cardContext.ChangeTracker.Clear();

        ~CardRepository()
        {
            Dispose(false);
        }
    }
}
