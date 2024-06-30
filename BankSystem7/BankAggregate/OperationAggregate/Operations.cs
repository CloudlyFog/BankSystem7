using BankSystem7.ApplicationAggregate.Entities;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace BankSystem7.BankAggregate.OperationAggregate;

[BsonIgnoreExtraElements]
public sealed class Operation : Entity
{
    public static readonly Operation Default = new(Guid.Empty)
    {
        BankId = Guid.Empty,
        ReceiverId = Guid.Empty,
        SenderId = Guid.Empty,
        OperationStatus = StatusOperationCode.Error,
    };
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
        return JsonConvert.SerializeObject(this);
    }
}
