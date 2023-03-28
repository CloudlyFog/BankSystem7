using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystem7.Models;
public class User
{

    [Key]
    public Guid? ID { get; set; } = Guid.NewGuid();
    
    public string Name { get; set; } = string.Empty;
    
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; }
    
    public bool Authenticated { get; set; }

    public string PhoneNumber { get; set; }

    public Guid? BankID { get; set; } = Guid.Empty;
    
    public int Age { get; set; }
    public Credit? Credit { get; set; }
    
    public Card? Card { get; set; }
    
    [NotMapped]
    public ExceptionModel Exception { get; set; } = ExceptionModel.Successfully;
}

/// <summary>
/// defines model of possible exceptions of methods's returns
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