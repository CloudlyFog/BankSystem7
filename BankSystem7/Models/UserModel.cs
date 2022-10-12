using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystem.Models
{
    public class User
    {
        [Key]
        public Guid ID { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool Authenticated { get; set; }
        public bool Access { get; set; }
        public int Age { get; set; }
        public bool HasOrder { get; set; }
        public Guid BankAccountID { get; set; } = Guid.NewGuid();
        public Guid BankID { get; set; } = Guid.Empty;
        public Guid CardID { get; set; } = Guid.Empty;
        public decimal BankAccountAmount { get; set; }

        [NotMapped]
        public ExceptionModel Exception { get; set; } = ExceptionModel.Successfull;

        [NotMapped]
        public Operation? OperationModel { get; set; }
    }

    /// <summary>
    /// defines model of possible exceptions of methods'es returns
    /// </summary>
    public enum ExceptionModel
    {
        VariableIsNull = 100,
        Successfull = 200,
        OperationRestricted = 300,
        OperationFailed = 400,
        OperationNotExist = 401
    }
    public enum Warning
    {
        NoRestrictions,
        AgeRestricted,
        Initiable
    }
}
