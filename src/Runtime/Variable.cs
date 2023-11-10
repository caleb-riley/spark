namespace Language.Runtime;

public sealed class Variable
{
    public ValueObject Value { get; private set; }
    public TypeSymbol Type { get; }
    public bool IsConstant { get; }

    public Variable(ValueObject value, TypeSymbol type, bool isConstant = false)
    {
        Type = type;
        Value = value;
        IsConstant = isConstant;
    }

    public void Set(ValueObject value)
    {
        Value = value;
    }
}
