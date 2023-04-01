using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using System.Data;
using BankSystem7.Models;
using BankSystem7.Services;
using BankSystem7.Services.Configuration;
using BankSystem7.Services.Repositories;

namespace BankSystem7.AppContext
{
    internal sealed class BankContext : DbContext
    {
        private readonly OperationService<Operation> _operationService;

        public BankContext()
        {
            _operationService = new OperationService<Operation>();
            DatabaseHandle();
        }
        public BankContext(string connection)
        {
            ServiceConfiguration.SetConnection(connection);
            _operationService = new OperationService<Operation>(ServiceConfiguration.Options.DatabaseName ?? "CabManagementSystemReborn");
            DatabaseHandle();
        }

        /// <summary>
        /// dataSource is name of server.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="dataSource"></param>
        public BankContext(string database, string dataSource = "maxim")
        {
            _operationService = new OperationService<Operation>(database);
            var connection =
                @$"Server=localhost\\SQLEXPRESS;Data Source={dataSource};Initial Catalog={database};
                Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=False";
            ServiceConfiguration.SetConnection(connection);
            
            DatabaseHandle();
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Bank> Banks { get; set; } = null!;
        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<Card> Cards { get; set; } = null!;
        
        public DbSet<Credit> Credits { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.UseSqlServer(ServiceConfiguration.Connection);
        }
        
        private void DatabaseHandle()
        {
            if (BankServicesOptions.EnsureDeleted)
                Database.EnsureDeleted();
            if (BankServicesOptions.EnsureCreated)
                Database.EnsureCreated();
        }

        /// <summary>
        /// creates transaction operation
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="operationKind"></param>
        public ExceptionModel CreateOperation(Operation? operation, OperationKind operationKind)
        {
            if (operation is null)
                return ExceptionModel.VariableIsNull;

            // Find(Builders<Operation>.Filter.Eq(predicate)).Any() equals
            // Operations.Any(predicate)
            // we find and in the same time check is there object in database
            if (_operationService.Collection.Find(Builders<Operation>.Filter.Eq(x => x.ID, operation.ID)).Any())
                return ExceptionModel.OperationNotExist;

            operation.OperationStatus = StatusOperation(operation, operationKind);
            if (operation.OperationStatus != StatusOperationCode.Successfully)
                return ExceptionModel.OperationRestricted;

            _operationService.Collection.InsertOne(operation);
            return ExceptionModel.Successfully;
        }

        /// <summary>
        /// delete transaction operation
        /// </summary>
        /// <param name="operation"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ExceptionModel DeleteOperation(Operation? operation)
        {
            if (operation is null)
                return ExceptionModel.VariableIsNull;
            if (_operationService.Collection.Find(Builders<Operation>.Filter.Eq(x => x.ID, operation.ID)).Any())
                return ExceptionModel.OperationNotExist;

            _operationService.Collection.DeleteOne(x => x.ID == operation.ID);
            return ExceptionModel.Successfully;
        }

        /// <summary>
        /// withdraw money from user bank account and accrual to bank's account
        /// </summary>
        /// <param name="user"></param>
        /// <param name="bank"></param>
        /// <param name="operation"></param>
        /// <exception cref="Exception"></exception>
        internal ExceptionModel BankAccountWithdraw(User? user, Bank? bank, Operation operation)
        {
            if (user.Card?.BankAccount?.Bank is null || bank is null || operation is null)
                return ExceptionModel.VariableIsNull;
            if (operation.OperationStatus != StatusOperationCode.Successfully)
                return (ExceptionModel)operation.OperationStatus.GetHashCode();

            bank.AccountAmount += operation.TransferAmount;
            user.Card.BankAccount.BankAccountAmount -= operation.TransferAmount;
            user.Card.Amount -= operation.TransferAmount;
            ChangeTracker.Clear();
            Update(user.Card.BankAccount);
            Update(user.Card.BankAccount.Bank);
            SaveChanges();
            return ExceptionModel.Successfully;
        }

        /// <summary>
        /// accrual money to user bank account from bank's account
        /// </summary>
        /// <param name="user"></param>
        /// <param name="bank"></param>
        /// <param name="operation"></param>
        /// <exception cref="Exception"></exception>
        internal ExceptionModel BankAccountAccrual(User? user, Bank? bank, Operation operation)
        {
            if (user.Card?.BankAccount?.Bank is null || bank is null || operation is null)
                return ExceptionModel.VariableIsNull;
            if (operation.OperationStatus != StatusOperationCode.Successfully)
                return (ExceptionModel)operation.OperationStatus.GetHashCode();

            bank.AccountAmount -= operation.TransferAmount;
            user.Card.BankAccount.BankAccountAmount += operation.TransferAmount;
            user.Card.Amount += operation.TransferAmount;
            ChangeTracker.Clear();
            Update(user.Card.BankAccount);
            Update(user.Card.BankAccount.Bank);
            SaveChanges();
            return ExceptionModel.Successfully;

        }

        private ExceptionModel AddCredit(Credit? creditModel)
        {
            if (creditModel is null)
                return ExceptionModel.VariableIsNull;
            if (Credits.AsNoTracking().Any(x => Equals(creditModel)))
                return ExceptionModel.OperationFailed;
            Add(creditModel);
            SaveChanges();
            return ExceptionModel.Successfully;
        }

        private ExceptionModel RemoveCredit(Credit? creditModel)
        {
            if (creditModel is null)
                return ExceptionModel.VariableIsNull;
            if (!Credits.AsNoTracking().Any(x => x.ID == creditModel.ID))
                return ExceptionModel.OperationFailed;
            Remove(creditModel);
            SaveChanges();
            return ExceptionModel.Successfully;
        }

        /// <summary>
        /// check: 
        /// 1) is exist user with the same ID and bank with the same BankID as a sender or reciever in the database.
        /// 2) is exist bank with the same BankID as a single bank.
        /// 3) is bank's money enough for transaction.
        /// 4) is user's money enough for transaction.
        /// </summary>
        /// <param name="operationModel"></param>
        /// <param name="operationKind"></param>
        /// <returns>status of operation, default - Successfully</returns>
        /// <exception cref="ArgumentNullException"></exception>
        private StatusOperationCode StatusOperation(Operation? operationModel, OperationKind operationKind)
        {
            if (operationModel is null)
                return StatusOperationCode.Error;

            if (operationKind == OperationKind.Accrual)
            {
                // SenderID is ID of bank
                // ReceiverID is ID of user
                if (!Banks.AsNoTracking().Any(x => x.ID == operationModel.SenderID) || !Users.Any(x => x.ID == operationModel.ReceiverID))
                    operationModel.OperationStatus = StatusOperationCode.Error;

                if (Banks.AsNoTracking().FirstOrDefault(x => x.ID == operationModel.SenderID)?.AccountAmount < operationModel.TransferAmount)
                    operationModel.OperationStatus = StatusOperationCode.Restricted;
            }
            else if(operationKind == OperationKind.Withdraw)
            {
                // SenderID is ID of user
                // ReceiverID is ID of bank
                if (!Banks.AsNoTracking().Any(x => x.ID == operationModel.ReceiverID) || !Users.Any(x => x.ID == operationModel.SenderID))
                    operationModel.OperationStatus = StatusOperationCode.Error;
                if (BankAccounts.AsNoTracking().FirstOrDefault(x => x.UserID == operationModel.SenderID)?.BankAccountAmount < operationModel.TransferAmount)
                    operationModel.OperationStatus = StatusOperationCode.Restricted;
            }

            if (!Banks.AsNoTracking().Any(x => x.ID == operationModel.BankID))
                operationModel.OperationStatus = StatusOperationCode.Error;

            return operationModel.OperationStatus;
        }

    }
}
