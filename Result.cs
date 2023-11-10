public abstract class Result { }

public sealed class Return : Result
{
    public ValueObject Value { get; }

    public Return(ValueObject value)
    {
        Value = value;
    }
}

public sealed class Break : Result { }
