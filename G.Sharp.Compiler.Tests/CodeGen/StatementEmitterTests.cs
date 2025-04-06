using System.Reflection.Emit;
using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.CodeGen;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class StatementEmitterTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(0)]
    [InlineData(-42)]
    public void StatementEmitter_Should_Emit_Let_Int_Local(int value)
    {
        var statement = new LetStatement("x", new IntValue(value));
        var method = new DynamicMethod("TestLet", typeof(int), Type.EmptyTypes);
        var il = method.GetILGenerator();
        var locals = new Dictionary<string, LocalBuilder>();

        StatementEmitter.Emit(il, statement, locals);
        il.Emit(OpCodes.Ldloc, locals["x"]);
        il.Emit(OpCodes.Ret);

        var result = ((Func<int>)method.CreateDelegate(typeof(Func<int>)))();
        result.Should().Be(value);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(-1)]
    public void StatementEmitter_Should_Emit_Assignment_To_Int_Local(int value)
    {
        var method = new DynamicMethod("AssignInt", typeof(int), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(int));
        var locals = new Dictionary<string, LocalBuilder> { ["x"] = local };

        var statement = new AssignmentStatement("x", new IntValue(value));
        StatementEmitter.Emit(il, statement, locals);

        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var result = ((Func<int>)method.CreateDelegate(typeof(Func<int>)))();
        result.Should().Be(value);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("dotnet")]
    public void StatementEmitter_Should_Emit_Print_String(string value)
    {
        var statement = new PrintStatement("x");
        var method = new DynamicMethod("Print", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(string));
        il.Emit(OpCodes.Ldstr, value);
        il.Emit(OpCodes.Stloc, local);

        var locals = new Dictionary<string, LocalBuilder> { ["x"] = local };

        StatementEmitter.Emit(il, statement, locals);
        il.Emit(OpCodes.Ret);

        var func = (Action)method.CreateDelegate(typeof(Action));
        using var sw = new StringWriter();
        Console.SetOut(sw);
        func();
        sw.ToString().Trim().Should().Be(value);
    }

    [Theory]
    [InlineData(123)]
    [InlineData(-99)]
    public void StatementEmitter_Should_Emit_Print_Int(int value)
    {
        var statement = new PrintStatement("x");
        var method = new DynamicMethod("PrintInt", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4, value);
        il.Emit(OpCodes.Stloc, local);

        var locals = new Dictionary<string, LocalBuilder> { ["x"] = local };

        StatementEmitter.Emit(il, statement, locals);
        il.Emit(OpCodes.Ret);

        var func = (Action)method.CreateDelegate(typeof(Action));
        using var sw = new StringWriter();
        Console.SetOut(sw);
        func();
        sw.ToString().Trim().Should().Be(value.ToString());
    }

    [Fact]
    public void StatementEmitter_Should_Throw_When_Statement_Is_Not_Supported()
    {
        var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();
        var locals = new Dictionary<string, LocalBuilder>();

        var stmt = new FakeStatement();

        var act = () => StatementEmitter.Emit(il, stmt, locals);

        act.Should().Throw<NotSupportedException>()
            .WithMessage("*Unsupported statement*");
    }

    private record FakeStatement : Statement;
}