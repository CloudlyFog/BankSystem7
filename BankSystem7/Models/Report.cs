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
public class GeneralReport<TOperationType> : UserReport<TOperationType> where TOperationType : Enum
{
    public GeneralReport()
    {
        
    }

    public GeneralReport(Report report)
    {
        SetReport(report);
    }

    public GeneralReport(GeneralReport<TOperationType> report)
    {
        SetReport(report);
        MethodName = report.MethodName;
    }
    public string MethodName { get; set; }
    public string ClassName { get; set; }

    private void SetReport(Report report)
    {
        Id = report.Id;
        ReportDate = report.ReportDate;
        ExceptionModel = report.ExceptionModel;
    }
}

[BsonIgnoreExtraElements]
public class UserReport<TOperationType> : Report where TOperationType : Enum
{
    public TOperationType OperationType { get; set; }
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

