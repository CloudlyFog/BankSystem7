using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;

namespace BankSystem7.Models;

[BsonIgnoreExtraElements]
public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public ExceptionModel ExceptionModel { get; set; } = ExceptionModel.Successfully;
}

[BsonIgnoreExtraElements]
public class GeneralReport<T> : UserReport<T> where T : Enum
{
    public GeneralReport()
    {
        
    }

    public GeneralReport(Report report)
    {
        SetReport(report);
    }

    public GeneralReport(GeneralReport<T> report)
    {
        SetReport(report);
        MethodName = report.MethodName;
    }
    public string MethodName { get; set; }

    private void SetReport(Report report)
    {
        Id = report.Id;
        ReportDate = report.ReportDate;
        ExceptionModel = report.ExceptionModel;
    }
}

[BsonIgnoreExtraElements]
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
    Exist,
    All,
    FitsConditions,
    Constructor,
    Destructor,
    Other,
}

public enum UserOperationType
{
    TransferMoney = 1,
    TakeCredit,
    PayCredit,
    RepayCredit
}

