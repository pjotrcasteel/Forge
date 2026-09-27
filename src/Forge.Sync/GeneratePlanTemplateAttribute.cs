namespace Forge.Sync;

/// <summary>Generates a strongly typed plan-template instantiation entry point for a partial class.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class GeneratePlanTemplateAttribute : Attribute
{
    public GeneratePlanTemplateAttribute(Type contextType, Type operationType, Type keyType)
    {
        ContextType = contextType;
        OperationType = operationType;
        KeyType = keyType;
    }

    public Type ContextType { get; }
    public Type OperationType { get; }
    public Type KeyType { get; }
}
