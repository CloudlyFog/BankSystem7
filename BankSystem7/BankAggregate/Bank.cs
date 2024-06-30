using System.ComponentModel.DataAnnotations.Schema;
using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.BankAggregate.BankAccountAggregate;
using BankSystem7.BankAggregate.CreditAggregate;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace BankSystem7.BankAggregate;

[Table("Banks")]
public class Bank : Entity
{
    [NotMapped]
    public static readonly Bank Default = new(Guid.Empty)
    {
        BankName = "bankName",
        AccountAmount = 0,
        Credits = [],
        BankAccounts = [],
    };

    public Bank(Guid id) : base(id)
    {
    }

    public string BankName { get; set; } = string.Empty;
    public List<Credit> Credits { get; set; } = [];
    public List<BankAccount> BankAccounts { get; set; } = [];

    public decimal AccountAmount { get; set; }

    public override string? ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}

public enum CardKind
{
    CreditCard,
    DebitCard,
    JuniorCard
}

public enum StatusOperationCode
{
    Ok = 1,
    Successfully = 2,
    Restricted = 3,
    Error = 4,
}

public enum OperationKind
{
    Accrual = 1,
    Withdraw
}

public enum AccountType
{
    User,
    Bank
}