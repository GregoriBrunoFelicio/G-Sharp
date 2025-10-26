namespace GSharp.AST;

public enum GPrimitiveType
{
    Number,
    Int,
    Float,
    Double,
    Decimal,
    String, // I know, I know that string is not primitive type, I`ll fix it later :/
    Boolean
}

public abstract class GType
{
    public abstract string Name { get; }

    public abstract Type GetClrType();

    public override string ToString() => Name;
}

public sealed class GNumberType : GType
{
    public override string Name => "number";
    
    public override Type GetClrType() => typeof(int);
}

public sealed class GStringType : GType
{
    public override string Name => "string";

    public override Type GetClrType() => typeof(string);
}

public sealed class GBooleanType : GType
{
    public override string Name => "boolean";
    public override Type GetClrType() => typeof(bool);
}

public sealed class GArrayType : GType
{
    public GType ElementType { get; }

    public GArrayType(GType elementType)
    {
        ElementType = elementType; 
    }

    public override string Name => $"{ElementType.Name}[]";

    public override Type GetClrType() => ElementType.GetClrType().MakeArrayType();
}
