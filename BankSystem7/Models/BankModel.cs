using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystem.Models
{
    public class Bank
    {
        public Guid ID { get; set; } = Guid.NewGuid(); // id for identification in the database
        public Guid BankID { get; set; }
        public string BankName { get; set; } = string.Empty;
        public decimal AccountAmount { get; set; }
    }
    public class Operation
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public Guid BankID { get; set; } = Guid.NewGuid();
        public Guid ReceiverID { get; set; } = Guid.NewGuid();
        public Guid SenderID { get; set; } = Guid.NewGuid();
        public decimal TransferAmount { get; set; }
        public StatusOperationCode OperationStatus { get; set; } = StatusOperationCode.Successfully;
        public OperationKind OperationKind { get; set; }

    }
    public class Credit
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public Guid BankID { get; set; }
        public Guid UserBankAccountID { get; set; }
        public decimal CreditAmount { get; set; }

        [NotMapped]
        public StatusOperationCode OperationStatus { get; set; } = StatusOperationCode.Successfully;
    }

    public class BankAccount
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public Guid BankID { get; set; } = Guid.NewGuid();
        public Guid UserID { get; set; } = Guid.Empty;
        public Guid CardID { get; set; } = Guid.Empty;
        public decimal BankAccountAmount { get; set; }
    }
    /// <summary>
    /// Describes debit/credit card.
    /// Necessarily:
    /// 1) Check property <see cref="Exception"/> because it will changed in constructor if any condititon will violated 
    /// 2) Set value for <see cref="CVV"/> and <see cref="Age"/>
    /// </summary>
    public class Card
    {
        private const int MaxLength = 3;
        public Card(int age, string cvv)
        {
            var rnd = new Random();
            if (cvv.Length < MaxLength)
                cvv = rnd.Next(100, 999).ToString();
            if (cvv.Length > MaxLength)
                CVV = cvv[..MaxLength];
            else
                CVV = cvv;

            if (age < 14 || age > 99)
                Exception = Warning.AgeRestricted;
            else
                Age = age;
        }
        public Card()
        {

        }
        public Guid ID { get; set; } = Guid.NewGuid();
        public Guid BankID { get; set; } = Guid.NewGuid();
        public Guid BankAccountID { get; set; } = Guid.Empty;
        public Guid UserID { get; set; } = Guid.Empty;
        public decimal Amount { get; set; }
        public DateTime Expiration { get; set; } = DateTime.Now.AddYears(4);
        public CardKind CardKind { get; set; }
        public string CVV { get; set; } = "000";
        public int Age { get; set; }
        [NotMapped]
        public Warning Exception { get; private set; } = Warning.NoRestrictions;
    }
    public enum CardKind
    {
        CreditCard,
        DebitCard,
        JuniorCard
    }
    public enum StatusOperationCode
    {
        Successfully = 200,
        Restricted = 300,
        Error = 400,
    }
    public enum OperationKind
    {
        Accrual = 1,
        Withdraw
    }
    
    /// <summary>
    /// defines model of possible exceptions of methods'es returns
    /// </summary>
    public enum ExceptionModel
    {
        VariableIsNull = 100,
        Successfully = 200,
        OperationRestricted = 300,
        OperationFailed = 400,
        OperationNotExist = 401
    }
    public enum Warning
    {
        NoRestrictions,
        AgeRestricted,
    }
}