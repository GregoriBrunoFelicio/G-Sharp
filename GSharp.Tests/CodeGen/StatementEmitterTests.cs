using System.Reflection.Emit;
using FluentAssertions;
using GSharp.AST;
using GSharp.CodeGen;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class StatementEmitterTests
{
    [Fact]
    public void Should_Emit_LetStatement()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var statement = new LetStatement("a", new LiteralExpression(new IntValue(10)));

        var method = new DynamicMethod("EmitLet", typeof(int), Type.EmptyTypes);
        var il = method.GetILGenerator();

        StatementEmitter.Emit(il, statement, locals);

        il.Emit(OpCodes.Ldloc, locals["a"]);
        il.Emit(OpCodes.Ret);

        var func = (Func<int>)method.CreateDelegate(typeof(Func<int>));
        func().Should().Be(10);
    }

    [Fact]
    public void Should_Emit_AssignmentStatement()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var method = new DynamicMethod("EmitAssign", typeof(int), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(int));
        locals["x"] = local;

        il.Emit(OpCodes.Ldc_I4_1);
        il.Emit(OpCodes.Stloc, local);

        var assign = new AssignmentStatement("x", new LiteralExpression(new IntValue(99)));
        StatementEmitter.Emit(il, assign, locals);

        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<int>)method.CreateDelegate(typeof(Func<int>));
        func().Should().Be(99);
    }

    [Fact]
    public void Should_Emit_PrintStatement()
    {
        var statement = new PrintStatement(new LiteralExpression(new StringValue("hey")));
        using var sw = new StringWriter();
        Console.SetOut(sw);

        var method = new DynamicMethod("EmitPrint", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        StatementEmitter.Emit(il, statement, new());
        il.Emit(OpCodes.Ret);

        var action = (Action)method.CreateDelegate(typeof(Action));
        action();

        var output = sw.ToString().Trim();
        output.Should().Be("hey");
    }

    [Fact]
    public void Should_Throw_If_Statement_Not_Supported()
    {
        var il = new DynamicMethod("EmitUnknown", typeof(void), Type.EmptyTypes).GetILGenerator();
        var statement = new UnknownStatement();

        var act = () => StatementEmitter.Emit(il, statement, new());

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported statement: UnknownStatement");
    }

    private sealed record UnknownStatement : Statement;
}