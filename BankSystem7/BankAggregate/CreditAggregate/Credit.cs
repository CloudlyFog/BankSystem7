using System.ComponentModel.DataAnnotations.Schema;
using BankSystem7.ApplicationAggregate.Entities;
using BankSystem7.UserAggregate;
using Newtonsoft.Json;

namespace BankSystem7.BankAggregate.CreditAggregate;

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
        return JsonConvert.SerializeObject(this);
    }
}