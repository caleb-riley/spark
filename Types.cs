namespace Types;

public class TypeSymbol
{
    public virtual string Name { get; protected set; }

    public TypeSymbol(string name)
    {
        Name = name;
    }

    public static TypeSymbol String => new("string");
    public static TypeSymbol Float => new("float");
    public static TypeSymbol Boolean => new("bool");
    public static TypeSymbol Void => new("void");
    public static TypeSymbol Object => new("object");

    public static FunctionTypeSymbol Function(TypeSymbol returnType, List<TypeSymbol> parameterTypes)
    {
        return new(returnType, parameterTypes);
    }

    public static ArrayTypeSymbol Array(TypeSymbol elementType)
    {
        return new(elementType);
    }

    public bool Matches(TypeSymbol targetType)
    {
        if (targetType.Name == "object")
            return true;

        if (Name != targetType.Name)
            return false;

        if (this is FunctionTypeSymbol functionValueType && targetType is FunctionTypeSymbol functionTargetType)
        {
            if (functionValueType.ParameterTypes.Count != functionTargetType.ParameterTypes.Count)
                return false;

            for (int i = 0; i < functionTargetType.ParameterTypes.Count; i++)
            {
                if (!functionValueType.ParameterTypes[i].Matches(functionTargetType.ParameterTypes[i]))
                    return false;
            }

            if (!functionValueType.ReturnType.Matches(functionTargetType.ReturnType))
                return false;
        }

        if (this is ArrayTypeSymbol arrayValueType && targetType is ArrayTypeSymbol arrayTargetType)
            return arrayValueType.ElementType.Matches(arrayTargetType.ElementType);

        return true;
    }

    public static TypeSymbol FromSyntax(TypeSyntax type)
    {
        if (type is FunctionTypeSyntax functionType)
        {
            TypeSymbol returnType = FromSyntax(functionType.ReturnType);
            List<TypeSymbol> parameterTypeSymbols = new();

            foreach (TypeSyntax parameterTypeSyntax in functionType.ParameterTypes)
                parameterTypeSymbols.Add(FromSyntax(parameterTypeSyntax));

            return new FunctionTypeSymbol(returnType, parameterTypeSymbols);
        }

        if (type is ArrayTypeSyntax arrayType)
        {
            TypeSymbol elementType = FromSyntax(arrayType.InnerType);

            return new ArrayTypeSymbol(elementType);
        }

        if (type is not ObjectTypeSyntax objectType)
            throw new Exception("Invalid type syntax.");

        return objectType.Identifier.Text switch
        {
            "float" => Float,
            "string" => String,
            "bool" => Boolean,
            "void" => Void,
            _ => throw new Exception("Invalid object type."),
        };
    }
}

public sealed class FunctionTypeSymbol : TypeSymbol
{
    public TypeSymbol ReturnType { get; }
    public List<TypeSymbol> ParameterTypes { get; }

    public FunctionTypeSymbol(TypeSymbol returnType, List<TypeSymbol> parameterTypes) : base("Function")
    {
        ReturnType = returnType;
        ParameterTypes = parameterTypes;
    }
}

public sealed class ArrayTypeSymbol : TypeSymbol
{
    public TypeSymbol ElementType { get; }

    public ArrayTypeSymbol(TypeSymbol elementType) : base("Array")
    {
        ElementType = elementType;
    }
}

public abstract class TypeSyntax { }

public class ArrayTypeSyntax : TypeSyntax
{
	public TypeSyntax InnerType { get; }

	public ArrayTypeSyntax(TypeSyntax innerType)
	{
		InnerType = innerType;
	}
}

public sealed class FunctionTypeSyntax : TypeSyntax
{
	public TypeSyntax ReturnType { get; }
	public List<TypeSyntax> ParameterTypes { get; }

	public FunctionTypeSyntax(TypeSyntax returnType, List<TypeSyntax> parameterTypes)
	{
		ReturnType = returnType;
		ParameterTypes = parameterTypes;
	}
}

public sealed class ObjectTypeSyntax : TypeSyntax
{
	public SyntaxToken Identifier { get; }

	public ObjectTypeSyntax(SyntaxToken identifier)
	{
		Identifier = identifier;
	}
}

public sealed class InferredTypeSyntax : TypeSyntax { }
