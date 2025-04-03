using System.Reflection.Emit;
using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.CodeGen;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class EmitTests
{
    [Theory]
    [InlineData(10)]
    [InlineData(0)]
    [InlineData(-42)]
    public void EmitInt_Should_Declare_Int_Local(int value)
    {
        var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = EmitNumber.EmitInt(il, value);

        local.LocalType.Should().Be(typeof(int));
    }

    [Theory]
    [InlineData(1.0f)]
    [InlineData(0.0f)]
    [InlineData(-9.9f)]
    public void EmitFloat_Should_Declare_Float_Local(float value)
    {
        var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = EmitNumber.EmitFloat(il, value);

        local.LocalType.Should().Be(typeof(float));
    }

    [Theory]
    [InlineData(2.0)]
    [InlineData(0.0)]
    [InlineData(-123.456)]
    public void EmitDouble_Should_Declare_Double_local(double value)
    {
        var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = EmitNumber.EmitDouble(il, value);

        local.LocalType.Should().Be(typeof(double));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(123.456)]
    [InlineData(-9876.54321)]
    [InlineData(0.0000000000000000000000000001)]
    public void EmitDecimal_Should_Declare_Decimal_Local_(decimal value)
    {
        var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = EmitDecimal.Emit(il, value);

        local.LocalType.Should().Be(typeof(decimal));
    }

    public static IEnumerable<object[]> ArrayTestData => new List<object[]>
    {
        new object[]
        {
            new ArrayValue(
                new List<VariableValue> { new IntValue(1), new IntValue(2), new IntValue(3) },
                new GType(GPrimitiveType.Number)
            ),
            typeof(int[])
        },
        new object[]
        {
            new ArrayValue(
                new List<VariableValue> { new StringValue("a"), new StringValue("b") },
                new GType(GPrimitiveType.String)
            ),
            typeof(string[])
        },
        new object[]
        {
            new ArrayValue(
                new List<VariableValue> { new BooleanValue(true), new BooleanValue(false) },
                new GType(GPrimitiveType.Boolean)
            ),
            typeof(bool[])
        }
    };

    [Theory]
    [MemberData(nameof(ArrayTestData))]
    public void EmitArray_Should_Declare_Local_OfExpectedArrayType(ArrayValue array, Type expectedArrayType)
    {
        var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = EmitArray.Emit(il, array);

        local.LocalType.Should().Be(expectedArrayType);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(-10)]
    public void EmitAssignment_Should_Assign_Int_Value_To_Local(int value)
    {
        var statement = new AssignmentStatement("x", new IntValue(value));
        var method = new DynamicMethod("AssignInt", typeof(int), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(int));
        var locals = new Dictionary<string, LocalBuilder> { ["x"] = local };

        EmitAssignment.Emit(il, statement, locals);
        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var result = ((Func<int>)method.CreateDelegate(typeof(Func<int>)))();
        result.Should().Be(value);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("")]
    public void EmitAssignment_Should_Assign_String_Value_To_Local(string value)
    {
        var statement = new AssignmentStatement("x", new StringValue(value));
        var method = new DynamicMethod("AssignString", typeof(string), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(string));
        var locals = new Dictionary<string, LocalBuilder> { ["x"] = local };

        EmitAssignment.Emit(il, statement, locals);
        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var result = ((Func<string>)method.CreateDelegate(typeof(Func<string>)))();
        result.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EmitAssignment_Should_Assign_Bool_Value_To_Local(bool value)
    {
        var statement = new AssignmentStatement("x", new BooleanValue(value));
        var method = new DynamicMethod("AssignBool", typeof(bool), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(bool));
        var locals = new Dictionary<string, LocalBuilder> { ["x"] = local };

        EmitAssignment.Emit(il, statement, locals);
        il.Emit(OpCodes.Ldloc, local);
        il.Emit(OpCodes.Ret);

        var result = ((Func<bool>)method.CreateDelegate(typeof(Func<bool>)))();
        result.Should().Be(value);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EmitBoolean_Should_Declare_Local_And_Assign_Value(bool value)
    {
        var method = new DynamicMethod("test", typeof(bool), Type.EmptyTypes);
        var il = method.GetILGenerator();

        EmitBoolean.Emit(il, value);
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Ret);

        var func = (Func<bool>)method.CreateDelegate(typeof(Func<bool>));
        var result = func();

        result.Should().Be(value);
    }

    [Theory]
    [InlineData(10, typeof(int))]
    [InlineData(3.14f, typeof(float))]
    [InlineData(2.718, typeof(double))]
    public void EmitLet_Should_Declare_Local_And_Assign_NumericValue(object rawValue, Type expectedType)
    {
        var value = rawValue switch
        {
            int i => new IntValue(i) as VariableValue,
            float f => new FloatValue(f),
            double d => new DoubleValue(d),
            _ => throw new NotSupportedException()
        };

        var statement = new LetStatement("x", value);
        var method = new DynamicMethod("TestLet", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();
        var locals = new Dictionary<string, LocalBuilder>();

        EmitLet.Emit(il, statement, locals);

        locals.Should().ContainKey("x");
        locals["x"].LocalType.Should().Be(expectedType);
    }

    [Theory]
    [InlineData("hello")]
    [InlineData("")]
    public void EmitLet_Should_Declare_Local_And_Assign_String(string str)
    {
        var statement = new LetStatement("msg", new StringValue(str));
        var method = new DynamicMethod("TestLetString", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();
        var locals = new Dictionary<string, LocalBuilder>();

        EmitLet.Emit(il, statement, locals);

        locals.Should().ContainKey("msg");
        locals["msg"].LocalType.Should().Be(typeof(string));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void EmitLet_Should_Declare_Local_And_Assign_Bool(bool value)
    {
        var statement = new LetStatement("flag", new BooleanValue(value));
        var method = new DynamicMethod("TestLetBool", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();
        var locals = new Dictionary<string, LocalBuilder>();

        EmitLet.Emit(il, statement, locals);

        locals.Should().ContainKey("flag");
        locals["flag"].LocalType.Should().Be(typeof(bool));
    }

    [Fact]
    public void EmitPrint_Should_Throw_When_Variable_Not_Defined()
    {
        var statement = new PrintStatement("missing");
        var method = new DynamicMethod("TestPrint", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();
        var locals = new Dictionary<string, LocalBuilder>();

        var act = () => EmitPrint.Emit(il, statement, locals);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Variable 'missing' is not defined.");
    }

    [Theory]
    [InlineData(42, "42")]
    [InlineData(-1, "-1")]
    public void EmitPrint_Should_Print_Int_Value(int value, string expectedOutput)
    {
        var statement = new PrintStatement("x");
        var method = new DynamicMethod("TestPrint", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(int));
        il.Emit(OpCodes.Ldc_I4, value);
        il.Emit(OpCodes.Stloc, local);

        var locals = new Dictionary<string, LocalBuilder> { ["x"] = local };
        EmitPrint.Emit(il, statement, locals);
        il.Emit(OpCodes.Ret);

        var func = (Action)method.CreateDelegate(typeof(Action));
        using var sw = new StringWriter();
        Console.SetOut(sw);
        func();
        sw.ToString().Trim().Should().Be(expectedOutput);
    }

    [Theory]
    [InlineData("hello", "hello")]
    [InlineData("", "")]
    public void EmitPrint_Should_Print_String_Value(string value, string expectedOutput)
    {
        var statement = new PrintStatement("x");
        var method = new DynamicMethod("TestPrint", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var local = il.DeclareLocal(typeof(string));
        il.Emit(OpCodes.Ldstr, value);
        il.Emit(OpCodes.Stloc, local);

        var locals = new Dictionary<string, LocalBuilder> { ["x"] = local };
        EmitPrint.Emit(il, statement, locals);
        il.Emit(OpCodes.Ret);

        var func = (Action)method.CreateDelegate(typeof(Action));
        using var sw = new StringWriter();
        Console.SetOut(sw);
        func();
        sw.ToString().Trim().Should().Be(expectedOutput);
    }
    
    [Fact]
    public void EmitPrint_Should_Throw_When_Variable_Is_Not_Defined()
    {
        var statement = new PrintStatement("undefinedVar");
        var method = new DynamicMethod("Test", typeof(void), Type.EmptyTypes);
        var il = method.GetILGenerator();
        var locals = new Dictionary<string, LocalBuilder>();

        var act = () => EmitPrint.Emit(il, statement, locals);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Variable 'undefinedVar' is not defined.");
    }
}