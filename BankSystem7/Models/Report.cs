using System.Reflection;

namespace BankSystem7.Models;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime ReportDate { get; set; } = DateTime.Now;
    public ExceptionModel ExceptionModel { get; set; } = ExceptionModel.Successfully;
}

public class GeneralReport<T> : UserReport<T> where T : Enum
{
    public string MethodName { get; set; }
    public Assembly? Assembly { get; set; }
}

public class UserReport<T> : Report where T : Enum
{
    public T OperationType { get; set; }
}

public enum OperationType
{
    Create = 1,
    Read,
    Update,
    Delete,
    Exists,
    Other,
}

public enum UserOperationType
{
    TransferMoney = 1,
    TakeCredit,
    PayCredit,
    RepayCredit
}

