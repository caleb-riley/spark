using Types;

public sealed class Environment
{
    public Environment? Parent { get; }
    private readonly Dictionary<string, Variable> _variables = new();

    public Environment(Environment? parent)
    {
        Parent = parent;
    }

    public void Declare(string name, TypeSymbol targetType, ValueObject value, bool isConstant = false)
    {
        if (!value.Type.Matches(targetType))
            throw new Exception("Incompatible types.");

        if (_variables.ContainsKey(name))
            throw new Exception($"Variable '{name}' has already been declared in this scope.");

        _variables[name] = new Variable(value, targetType, isConstant);
    }

    public Variable Resolve(string name)
    {
        if (_variables.ContainsKey(name))
            return _variables[name];

        if (Parent is not null)
            return Parent.Resolve(name);

        throw new Exception($"Could not resolve variable '{name}'.");
    }

    public void Set(string name, ValueObject value)
    {
        Variable variable = Resolve(name);

        if (!value.Type.Matches(variable.Type))
            throw new Exception("Incompatible types.");

        if (variable.IsConstant)
            throw new Exception("Cannot change the value of a constant variable.");

        variable.Set(value);
    }

    public ValueObject Get(string name)
    {
        Variable variable = Resolve(name);

        return variable.Value;
    }
}