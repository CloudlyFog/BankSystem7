using BankSystem7.Extensions;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystem7.Models;

public class User
{
    [NotMapped]
    public static readonly User Default = new()
    {
        ID = Guid.Empty,
        Name = "name",
        Email = "email",
        Password = "password",
        PhoneNumber = "123456789",
        Age = 0,
    };

    [Key]
    public Guid? ID { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; }

    public string PhoneNumber { get; set; }

    public int Age { get; set; }
    public Credit? Credit { get; set; }

    public Card? Card { get; set; }

    [NotMapped]
    public ExceptionModel Exception { get; set; } = ExceptionModel.Ok;

    public override bool Equals(object? obj)
    {
        return this.EqualsTo(obj as User);
    }

    public override int GetHashCode()
    {
        return ID.GetHashCode();
    }

    public override string? ToString()
    {
        return this.ConvertToString();
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
    IsDefaultValue,
    ThrewException,
}

public enum CardException
{
    NoRestrictions,
    AgeRestricted,
    Error,
}