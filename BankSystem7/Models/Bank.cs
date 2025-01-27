﻿using BankSystem7.Entities;
using BankSystem7.Extensions;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankSystem7.Models;

[Table("Banks")]
public class Bank : Entity
{
    [NotMapped]
    public static readonly Bank Default = new(Guid.Empty)
    {
        BankName = "bankName",
        AccountAmount = 0,
        Credits = new List<Credit>(),
        BankAccounts = new List<BankAccount>(),
    };

    public Bank(Guid id) : base(id)
    {
    }

    public string BankName { get; set; } = string.Empty;
    public List<Credit> Credits { get; set; } = new();
    public List<BankAccount> BankAccounts { get; set; } = new();

    [Precision(18, 2)]
    public decimal AccountAmount { get; set; }

    public override string? ToString()
    {
        return this.ConvertToString();
    }
}

[BsonIgnoreExtraElements]
public class Operation : Entity
{
    public Operation(Guid id) : base(id)
    {
    }
    public Guid? BankId { get; set; } = Guid.NewGuid();
    public Guid? ReceiverId { get; set; } = Guid.NewGuid();
    public Guid? SenderId { get; set; } = Guid.NewGuid();
    public decimal TransferAmount { get; set; }
    public StatusOperationCode OperationStatus { get; set; } = StatusOperationCode.Ok;
    public OperationKind OperationKind { get; set; }

    public override string? ToString()
    {
        return this.ConvertToString();
    }
}

[Table("Credits")]
public class Credit : Entity
{
    [NotMapped]
    public static readonly Credit Default = new(Guid.Empty)
    {
        OperationStatus = StatusOperationCode.Error,
    };

    private Credit(Guid id) : base(id)
    {
    }

    public Credit(
        decimal creditAmount, 
        decimal interestRate, 
        DateTime repaymentDate, 
        User user = null, 
        Bank bank = null) : base(Guid.NewGuid())
    {
        UserId = user.Id;
        BankId = bank.Id;
        CreditAmount = creditAmount;
        InterestRate = interestRate;
        RepaymentDate = repaymentDate;
        RepaymentAmount = CalculateRepaymentAmount();
    }

    public Guid? BankId { get; set; }
    public Guid? UserId { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal RepaymentAmount { get; set; }
    public DateTime RepaymentDate { get; set; }
    public DateTime IssueCreditDate { get; set; } = DateTime.Now;
    public Bank? Bank { get; set; }
    public User? User { get; set; }

    [NotMapped]
    public StatusOperationCode OperationStatus { get; set; } = StatusOperationCode.Ok;

    public decimal CalculateRepaymentAmount()
    {
        var a = double.Parse((1 + InterestRate / 1200).ToString());
        var repaymentDateMonth = (RepaymentDate.Year - IssueCreditDate.Year) * 12;
        var x = Math.Pow(a, repaymentDateMonth);
        x = 1 / x;
        x = 1 - x;
        var monthlyPayment = CreditAmount * (InterestRate / 1200) / decimal.Parse(x.ToString());
        return monthlyPayment * repaymentDateMonth;
    }

    public override string? ToString()
    {
        return this.ConvertToString();
    }
}

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
        return this.ConvertToString();
    }
}

[Table("Cards")]
public class Card
{
    public static readonly Card Default = new()
    {
        ID = Guid.Empty,
        UserID = Guid.Empty,
        BankAccountID = Guid.Empty,
        Exception = CardException.Error,
    };

    private const int CvvLength = 3;

    public Card(int age, string cvv = "default", User user = null, BankAccount bankAccount = null)
    {
        Age = SetAge(age);
        CVV = SetCvv(cvv);
        if (user is null)
            return;
        UserID = user.Id;
        User = user;

        if (bankAccount is null)
            return;

        BankAccount = bankAccount;
        BankAccountID = bankAccount.Id;
        Amount = bankAccount.BankAccountAmount;

        BankAccount.Bank = bankAccount.Bank;
    }

    private Card()
    {
    }

    [Key]
    public Guid ID { get; set; } = Guid.NewGuid();

    public Guid? BankAccountID { get; set; } = Guid.Empty;
    public Guid? UserID { get; set; } = Guid.Empty;
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
        return ID.GetHashCode();
    }

    public override string? ToString()
    {
        return this.ConvertToString();
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