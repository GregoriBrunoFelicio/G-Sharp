using System.Reflection.Emit;
using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.CodeGen;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class ExpressionEmitterTests
{
    [Fact]
    public void Should_Emit_And_Store_IntValue()
    {
        var il = CreateIl<int>(out var method);
        var local = ExpressionEmitter.Emit(il, new LiteralExpression(new IntValue(42)), new());

        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<int>)method.CreateDelegate(typeof(Func<int>));
        func().Should().Be(42);
    }

    [Fact]
    public void Should_Emit_And_Store_DecimalValue()
    {
        var il = CreateIl<decimal>(out var method);
        var local = ExpressionEmitter.Emit(il, new LiteralExpression(new DecimalValue(10.5m)), new());

        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<decimal>)method.CreateDelegate(typeof(Func<decimal>));
        func().Should().Be(10.5m);
    }

    [Fact]
    public void Should_Emit_And_Store_FloatValue()
    {
        var il = CreateIl<float>(out var method);
        var local = ExpressionEmitter.Emit(il, new LiteralExpression(new FloatValue(3.14f)), new());

        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<float>)method.CreateDelegate(typeof(Func<float>));
        func().Should().BeApproximately(3.14f, 0.0001f);
    }

    [Fact]
    public void Should_Emit_And_Store_DoubleValue()
    {
        var il = CreateIl<double>(out var method);
        var local = ExpressionEmitter.Emit(il, new LiteralExpression(new DoubleValue(2.718)), new());

        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<double>)method.CreateDelegate(typeof(Func<double>));
        func().Should().BeApproximately(2.718, 0.0001);
    }

    [Fact]
    public void Should_Emit_And_Store_BoolValue()
    {
        var il = CreateIl<bool>(out var method);
        var local = ExpressionEmitter.Emit(il, new LiteralExpression(new BooleanValue(true)), new());

        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<bool>)method.CreateDelegate(typeof(Func<bool>));
        func().Should().BeTrue();
    }

    [Fact]
    public void Should_Emit_And_Store_StringValue()
    {
        var il = CreateIl<string>(out var method);
        var local = ExpressionEmitter.Emit(il, new LiteralExpression(new StringValue("yo")), new());

        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<string>)method.CreateDelegate(typeof(Func<string>));
        func().Should().Be("yo");
    }

    [Fact]
    public void Should_Emit_Value_From_Declared_Variable()
    {
        var il = CreateIl<int>(out var method);
        var local = il.DeclareLocal(typeof(int));
        var locals = new Dictionary<string, LocalBuilder> { ["x"] = local };

        il.Emit(OpCodes.Ldc_I4, 7);
        il.Emit(OpCodes.Stloc, local);

        var returned = ExpressionEmitter.Emit(il, new VariableExpression("x"), locals);
        il.Emit(OpCodes.Ldloc, returned); 
        il.Emit(OpCodes.Ret);

        var func = (Func<int>)method.CreateDelegate(typeof(Func<int>));
        func().Should().Be(7);
    }

    [Fact]
    public void Should_Throw_If_Emit_Unsupported_Expression()
    {
        var il = CreateIl<object>(out _);
        var act = () => ExpressionEmitter.Emit(il, new FakeExpression(), new());

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported expression type: FakeExpression");
    }

    [Fact]
    public void Should_Throw_If_EmitToStack_Unsupported_Expression()
    {
        var il = CreateIl<object>(out _);
        var act = () => ExpressionEmitter.EmitToStack(il, new FakeExpression(), new());

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported expression type: FakeExpression");
    }

    [Fact]
    public void Should_Throw_If_Variable_Not_Found()
    {
        var il = CreateIl<object>(out _);
        var act = () => ExpressionEmitter.EmitToStack(il, new VariableExpression("notfound"), new());

        act.Should().Throw<Exception>()
            .WithMessage("Variable 'notfound' not found.");
    }

    [Fact]
    public void Should_Emit_Array_To_Stack()
    {
        var array = new ArrayValue([
            new IntValue(1),
            new IntValue(2),
            new IntValue(3)
        ], new GType(GPrimitiveType.Int));

        var il = CreateIl<object>(out var method);
        ExpressionEmitter.EmitToStack(il, new LiteralExpression(array), new());
        il.Emit(OpCodes.Ret);

        var func = (Func<object>)method.CreateDelegate(typeof(Func<object>));
        func().Should().BeAssignableTo<int[]>().Which.Should().BeEquivalentTo([1, 2, 3]);
    }

    [Fact]
    public void Should_Throw_When_EmitLiteralToStack_With_UnsupportedType()
    {
        var il = CreateIl<object>(out _);
        var act = () => ExpressionEmitter.EmitToStack(il, new LiteralExpression(new FakeValue()), new());

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported literal type: FakeValue");
    }

    private static ILGenerator CreateIl<T>(out DynamicMethod method)
    {
        method = new DynamicMethod("Test", typeof(T), Type.EmptyTypes);
        return method.GetILGenerator();
    }

    private sealed record FakeExpression : Expression { }
    private sealed record FakeValue : VariableValue
    {
        public override GType Type => new(GPrimitiveType.Int);
    }
}
