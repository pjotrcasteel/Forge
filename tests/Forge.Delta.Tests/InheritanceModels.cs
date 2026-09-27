using Forge.Delta;

namespace Forge.Delta.Tests;

internal abstract class EntityBase
{
    protected EntityBase(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}

[GenerateDelta]
internal sealed class ProvisionedService : EntityBase
{
    public ProvisionedService(Guid id, string state)
        : base(id)
    {
        State = state;
    }

    public string State { get; }
}

internal abstract class AuditedEntity
{
    protected AuditedEntity(string technicalState)
    {
        TechnicalState = technicalState;
    }

    public virtual string TechnicalState { get; }
}

[GenerateDelta]
internal sealed class BusinessEntity : AuditedEntity
{
    public BusinessEntity(string technicalState, string name)
        : base(technicalState)
    {
        Name = name;
    }

    [DeltaIgnore]
    public override string TechnicalState => base.TechnicalState;

    public string Name { get; }
}
