using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.BankAggregate.BankAccountAggregate;
using BankSystem7.UserAggregate;
using Newtonsoft.Json;

namespace BankSystem7.BankAggregate.CardAggregate;

[Table("Cards")]
public class Card : Entity
{
    public static readonly Card Default = new(Guid.Empty)
    {
        UserId = Guid.Empty,
        BankAccountID = Guid.Empty,
        Exception = CardException.Error,
    };

    private const int CvvLength = 3;

    public Card(Guid id, int age, string cvv = "default", User user = null, BankAccount bankAccount = null)
        : base(id)
    {
        Age = SetAge(age);
        CVV = SetCvv(cvv);
        if (user is null)
            return;
        UserId = user.Id;
        User = user;

        if (bankAccount is null)
            return;

        BankAccount = bankAccount;
        BankAccountID = bankAccount.Id;
        Amount = bankAccount.BankAccountAmount;

        BankAccount.Bank = bankAccount.Bank;
    }

    private Card(Guid id) : base(id)
    {
    }

    public Guid? BankAccountID { get; set; } = Guid.Empty;
    public Guid? UserId
    { get; set; } = Guid.Empty;
    public decimal Amount { get; set; }
    public DateTime Expiration { get; } = DateTime.Now.AddYears(4);
    public CardKind CardKind { get; set; } = CardKind.DebitCard;
    public string CVV { get; init; } = "000";
    public int Age { get; init; } = 0;
    public User? User { get; set; }
    public BankAccount? BankAccount { get; set; }

    [NotMapped]
    public CardException Exception { get; private set; } = CardException.NoRestrictions;

    private static string SetCvv(string cvv)
    {
        if (cvv is null || cvv.Length < CvvLength)
            cvv = Random.Shared.Next(100, 999).ToString();
        if (cvv.All(x => !char.IsDigit(x)))
            cvv = RandomString(3, true);
        return cvv.Length > CvvLength ? cvv[..CvvLength] : cvv;
    }

    private int SetAge(int age)
    {
        if (age < 14 || age > 99)
            Exception = CardException.AgeRestricted;
        return age;
    }

    private static string RandomString(int length, bool onlyDigits = false)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var chars = $"{alphabet.ToLower()}{alphabet}0123456789_@#$%";
        if (onlyDigits)
            chars = "0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string? ToString()
    {
        return JsonConvert.SerializeObject(this);
    }
}