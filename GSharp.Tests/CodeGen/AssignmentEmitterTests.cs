using System.Reflection.Emit;
using FluentAssertions;
using GSharp.AST;
using GSharp.CodeGen;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class AssignmentEmitterTests
{
    [Fact]
    public void Should_Assign_IntValue_To_Local()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var statement = new AssignmentStatement("x", new LiteralExpression(new IntValue(42)));

        var method = new DynamicMethod("AssignInt", typeof(int), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(int));
        locals["x"] = local;

        AssignmentEmitter.Emit(il, statement, locals);
        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<int>)method.CreateDelegate(typeof(Func<int>));
        func().Should().Be(42);
    }

    [Fact]
    public void Should_Assign_FloatValue_To_Local()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var statement = new AssignmentStatement("x", new LiteralExpression(new FloatValue(3.14f)));

        var method = new DynamicMethod("AssignFloat", typeof(float), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(float));
        locals["x"] = local;

        AssignmentEmitter.Emit(il, statement, locals);
        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<float>)method.CreateDelegate(typeof(Func<float>));
        func().Should().BeApproximately(3.14f, 0.0001f);
    }

    [Fact]
    public void Should_Assign_DoubleValue_To_Local()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var statement = new AssignmentStatement("x", new LiteralExpression(new DoubleValue(2.5)));

        var method = new DynamicMethod("AssignDouble", typeof(double), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(double));
        locals["x"] = local;

        AssignmentEmitter.Emit(il, statement, locals);
        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<double>)method.CreateDelegate(typeof(Func<double>));
        func().Should().BeApproximately(2.5, 0.0001);
    }

    [Fact]
    public void Should_Assign_BoolValue_To_Local()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var statement = new AssignmentStatement("x", new LiteralExpression(new BooleanValue(true)));

        var method = new DynamicMethod("AssignBool", typeof(bool), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(bool));
        locals["x"] = local;

        AssignmentEmitter.Emit(il, statement, locals);
        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<bool>)method.CreateDelegate(typeof(Func<bool>));
        func().Should().BeTrue();
    }

    [Fact]
    public void Should_Assign_StringValue_To_Local()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var statement = new AssignmentStatement("x", new LiteralExpression(new StringValue("hello")));

        var method = new DynamicMethod("AssignString", typeof(string), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(string));
        locals["x"] = local;

        AssignmentEmitter.Emit(il, statement, locals);
        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var func = (Func<string>)method.CreateDelegate(typeof(Func<string>));
        func().Should().Be("hello");
    }

    [Fact]
    public void Should_Throw_If_Variable_Not_Defined()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var statement = new AssignmentStatement("x", new LiteralExpression(new IntValue(1)));
        var il = new DynamicMethod("Fail", typeof(void), Type.EmptyTypes).GetILGenerator();

        var act = () => AssignmentEmitter.Emit(il, statement, locals);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Variable 'x' is not defined.");
    }

    [Fact]
    public void Should_Throw_If_Expression_Type_Not_Supported()
    {
        var locals = new Dictionary<string, LocalBuilder> { ["x"] = null! };
        var statement = new AssignmentStatement("x", new FakeExpression());
        var il = new DynamicMethod("Fail", typeof(void), Type.EmptyTypes).GetILGenerator();

        var act = () => AssignmentEmitter.Emit(il, statement, locals);

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported expression: FakeExpression");
    }

    [Fact]
    public void Should_Throw_If_VariableExpression_Not_Found()
    {
        var locals = new Dictionary<string, LocalBuilder> { ["x"] = null! };
        var expr = new VariableExpression("y");
        var stmt = new AssignmentStatement("x", expr);

        var il = new DynamicMethod("Fail", typeof(void), Type.EmptyTypes).GetILGenerator();

        var act = () => AssignmentEmitter.Emit(il, stmt, locals);

        act.Should().Throw<Exception>()
            .WithMessage("Variable 'y' not found.");
    }

    [Fact]
    public void Should_Throw_If_Literal_Type_Not_Supported()
    {
        var locals = new Dictionary<string, LocalBuilder> { ["x"] = null! };
        var expr = new LiteralExpression(new FakeValue());
        var stmt = new AssignmentStatement("x", expr);

        var il = new DynamicMethod("Fail", typeof(void), Type.EmptyTypes).GetILGenerator();

        var act = () => AssignmentEmitter.Emit(il, stmt, locals);

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported literal type");
    }

    private sealed record FakeExpression : Expression { }

    private sealed record FakeValue : VariableValue
    {
        public override GType Type => new(GPrimitiveType.Int);
    }
} 
