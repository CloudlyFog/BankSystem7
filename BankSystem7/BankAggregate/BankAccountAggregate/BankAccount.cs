using System.ComponentModel.DataAnnotations.Schema;
using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.BankAggregate.CardAggregate;
using BankSystem7.UserAggregate;
using Newtonsoft.Json;

namespace BankSystem7.BankAggregate.BankAccountAggregate;

[Table("BankAccounts")]
public class BankAccount : Entity
{
    [NotMapped]
    public static readonly BankAccount Default = new(Guid.Empty)
    {
        BankId = Guid.Empty,
        UserId = Guid.Empty,
        PhoneNumber = "123456789",
    };

    public BankAccount(User user, Bank bank) : base(Guid.NewGuid())
    {
        PhoneNumber = user.PhoneNumber;
        UserId = user.Id;
        Bank = bank;
        BankId = bank.Id;
    }

    private BankAccount(Guid id) : base(id)
    {
    }

    public Guid? BankId { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; } = Guid.Empty;
    public Card? Card { get; set; }
    public Bank? Bank { get; set; }
    public User? User { get; set; }
    public string PhoneNumber { get; set; }
    public AccountType AccountType { get; set; } = AccountType.User;
    public decimal BankAccountAmount { get; set; }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string? ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}