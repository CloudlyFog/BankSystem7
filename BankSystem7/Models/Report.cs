using System.Reflection;

namespace BankSystem7.Models;

public class Report
{
    public int Id { get; set; }
    public DateTime ReportDate { get; set; }
    public string OperationName { get; set; }
    public ExceptionModel ExceptionModel { get; set; } = ExceptionModel.Successfully;
}

public class GeneralReport<T> : UserReport<T> where T : Enum
{
    public string MethodName { get; set; }
    public Assembly Assembly { get; set; }
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

