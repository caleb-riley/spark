using Types;

public class ValueObject
{
    public object Value { get; }
    public TypeSymbol Type { get; }

    public ValueObject(object value, TypeSymbol type)
    {
        Value = value;
        Type = type;
    }

    public static ValueObject Void => new("void", TypeSymbol.Void);
    public static ValueObject Float(double value) => new(value, TypeSymbol.Float);
    public static ValueObject String(string value) => new(value, TypeSymbol.String);
    public static ValueObject Boolean(bool value) => new(value, TypeSymbol.Boolean);
    public static ValueObject Function(Func<List<ValueObject>, Result?> value, FunctionTypeSymbol functionType) => new(value, functionType);
    public static ValueObject Array(List<ValueObject> elements, ArrayTypeSymbol arrayType) => new ValueObject(elements, arrayType);

    public static ValueObject Infer(object value)
    {
        if (value is string s)
            return String(s);
        else if (value is int d)
            return Float(d);
        else if (value is bool b)
            return Boolean(b);

        throw new Exception("Could not infer the type.");
    }
}