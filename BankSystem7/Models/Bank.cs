﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BankSystem7.Services.Repositories;
using Microsoft.EntityFrameworkCore.Query.Internal;
using MongoDB.Bson.Serialization.Attributes;

namespace BankSystem7.Models;

public class Bank
{
    [Key]
    public Guid ID { get; set; } = Guid.NewGuid(); // id for identification in the database
    public string BankName { get; set; } = string.Empty;
    public List<Credit>? Credits { get; set; } = new();
    public List<BankAccount>? BankAccounts { get; set; } = new();
    public decimal AccountAmount { get; set; }
}

[BsonIgnoreExtraElements]
public class Operation
{
    public Guid ID { get; set; } = Guid.NewGuid();
    public Guid? BankID { get; set; } = Guid.NewGuid();
    public Guid? ReceiverID { get; set; } = Guid.NewGuid();
    public Guid? SenderID { get; set; } = Guid.NewGuid();
    public decimal TransferAmount { get; set; }
    public StatusOperationCode OperationStatus { get; set; } = StatusOperationCode.Successfully;
    public OperationKind OperationKind { get; set; }
}

public class Credit
{
    public static Credit CreateInstance1()
    {
        return new Credit();
    }

    public static Credit CreateInstance(decimal creditAmount, decimal interestRate, DateTime repaymentDate,
        DateTime issueCreditDate, User user = null, Bank bank = null)
    {
        var credit = new Credit(creditAmount, user, bank)
        {
            CreditAmount = creditAmount,
            InterestRate = interestRate,
            IssueCreditDate = issueCreditDate,
            RepaymentDate = repaymentDate,
        };
        credit.RepaymentAmount = credit.CalculateRepaymentAmount();
        return credit;
    }

    private Credit()
    {
        
    }
    private Credit(decimal creditAmount, User user = null, Bank bank = null)
    {
        if (bank is null || user is null)
            return;

        Bank = bank;
        BankID = bank.ID;
        User = user;
        UserID = user.ID;
    }
    [Key]
    public Guid ID { get; set; } = Guid.NewGuid();
    public Guid? BankID { get; set; }
    public Guid? UserID { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal InterestRate { get; set; }
    public decimal RepaymentAmount { get; set; }
    public DateTime RepaymentDate { get; set; }
    public DateTime IssueCreditDate { get; set; } = DateTime.Now;
    public Bank? Bank { get; set; }
    public User? User { get; set; }

    [NotMapped] 
    public StatusOperationCode OperationStatus { get; set; } = StatusOperationCode.Successfully;

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
}

public class BankAccount
{

    public BankAccount()
    {
        
    }
    public BankAccount(User user, Bank bank)
    {
        PhoneNumber = user.PhoneNumber;
        UserID = user.ID;
        Bank = bank;
        BankID = bank.ID;
    }
    
    [Key]
    public Guid ID { get; set; } = Guid.NewGuid();
    public Guid? BankID { get; set; } = Guid.NewGuid();
    public Guid? UserID { get; set; } = Guid.Empty;
    
    public Card? Card { get; set; }
    public Bank? Bank { get; set; }
    public User? User { get; set; }

    public string PhoneNumber { get; set; }

    public AccountType AccountType { get; set; } = AccountType.User;
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
    private const int CvvLength = 3;
 
    public Card(int age, string cvv = "default", User user = null, BankAccount bankAccount = null)
    {
        Age = SetAge(age);
        CVV = SetCvv(cvv);
        if (user is null)
            return;
        UserID = user.ID;
        user.BankID = bankAccount.BankID;
        User = user;

        if (bankAccount is null)
            return;

        BankAccount = bankAccount;
        BankAccountID = bankAccount.ID;
        BankID = bankAccount.BankID;
        Amount = bankAccount.BankAccountAmount;

        BankAccount.Bank = bankAccount.Bank;
    }

    private Card()
    {
    }

    [Key]
    public Guid ID { get; set; } = Guid.NewGuid();
    public Guid? BankID { get; set; } = Guid.Empty;
    public Guid? BankAccountID { get; set; } = Guid.Empty;
    public Guid? UserID { get; set; } = Guid.Empty;
    public decimal Amount { get; set; }
    public DateTime Expiration { get; } = DateTime.Now.AddYears(4);
    public CardKind CardKind { get; set; } = CardKind.DebitCard;
    public string CVV { get; init; } = "000";
    public int Age { get; init; } = -1;
    public User? User { get; set; }
    public BankAccount? BankAccount { get; set; }

    [NotMapped] 
    public Warning Exception { get; private set; } = Warning.NoRestrictions;

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
            Exception = Warning.AgeRestricted;
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

public enum AccountType
{
    User,
    Bank
}