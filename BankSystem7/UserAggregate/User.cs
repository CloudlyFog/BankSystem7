using System.ComponentModel.DataAnnotations.Schema;
using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.BankAggregate.CardAggregate;
using BankSystem7.BankAggregate.CreditAggregate;
using Newtonsoft.Json;

namespace BankSystem7.UserAggregate;

[Table("Users")]
public class User : Entity
{
    [NotMapped]
    public static readonly User Default = new(Guid.Empty)
    {
        Age = 0,
    };

    public User(Guid id) : base(id)
    {
    }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; }

    public string PhoneNumber { get; set; }

    public int Age { get; set; }
    public Credit? Credit { get; set; }

    public Card? Card { get; set; }

    [NotMapped]
    public ExceptionModel Exception { get; set; } = ExceptionModel.Ok;

    public override string? ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

/// <summary>
/// defines model of possible exceptions of returns of methods
/// </summary>
public enum ExceptionModel
{
    Ok = 1,
    Successfully = 2,
    Restricted = 3,
    Error = 4,
    EntityIsNull,
    VariableIsNull,
    OperationRestricted,
    OperationFailed,
    OperationNotExist,
    EntityNotExist,
    NotFitsConditions,
    IsDefaultValue,
    ThrewException,
}

public enum CardException
{
    NoRestrictions,
    AgeRestricted,
    Error,
}