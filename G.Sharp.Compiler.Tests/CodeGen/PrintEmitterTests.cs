using System.Reflection.Emit;
using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.CodeGen;

namespace G.Sharp.Compiler.Tests.CodeGen;
public class PrintEmitterTests
{
    [Fact]
    public void Should_Emit_PrintStatement_With_LiteralExpression()
    {
        var statement = new PrintStatement(new LiteralExpression(new StringValue("hi")));

        using var sw = new StringWriter();
        Console.SetOut(sw);

        var method = new DynamicMethod("Print", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        PrintEmitter.Emit(il, statement, new());
        il.Emit(OpCodes.Ret);

        var action = (Action)method.CreateDelegate(typeof(Action));
        action();

        var output = sw.ToString().Trim();
        output.Should().Be("hi");
    }

    [Fact]
    public void Should_Emit_PrintStatement_With_VariableExpression()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var method = new DynamicMethod("PrintVar", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(int));
        locals["num"] = local;

        il.Emit(OpCodes.Ldc_I4, 123);
        il.Emit(OpCodes.Stloc, local);

        var statement = new PrintStatement(new VariableExpression("num"));

        using var sw = new StringWriter();
        Console.SetOut(sw);

        PrintEmitter.Emit(il, statement, locals);
        il.Emit(OpCodes.Ret);

        var action = (Action)method.CreateDelegate(typeof(Action));
        action();

        var output = sw.ToString().Trim();
        output.Should().Be("123");
    }

    [Fact]
    public void Should_Throw_If_Expression_Is_Not_Supported_By_Emitter()
    {
        var statement = new PrintStatement(new FakeExpression());
        var il = new DynamicMethod("Fail", typeof(void), Type.EmptyTypes).GetILGenerator();

        var act = () => PrintEmitter.Emit(il, statement, new());

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported expression type: FakeExpression");
    }

    private sealed record FakeExpression : Expression;
}
