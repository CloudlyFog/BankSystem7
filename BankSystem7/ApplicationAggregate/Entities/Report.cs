using BankSystem7.ApplicationAggregate.Extensions;
using BankSystem7.UserAggregate;
using MongoDB.Bson.Serialization.Attributes;

namespace BankSystem7.ApplicationAggregate.Entities;

[BsonIgnoreExtraElements]
public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public ExceptionModel ExceptionModel { get; set; } = ExceptionModel.Ok;

    public override bool Equals(object? obj)
    {
        return this.EqualsTo(obj as Report);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string? ToString()
    {
        return this.ConvertToString();
    }
}

[BsonIgnoreExtraElements]
public class GeneralReport<TOperationType> : Report
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
    public TOperationType OperationType { get; set; }

    private void SetReport(Report report)
    {
        Id = report.Id;
        ReportDate = report.ReportDate;
        ExceptionModel = report.ExceptionModel;
    }

    public override bool Equals(object? obj)
    {
        return this.EqualsTo(obj as GeneralReport<TOperationType>);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string? ToString()
    {
        return this.ConvertToString();
    }
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
}