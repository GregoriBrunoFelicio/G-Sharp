using System.Reflection.Emit;
using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.CodeGen;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class ArrayEmitterTests
{
    [Fact]
    public void Should_Emit_Array_Of_Strings_And_Return_As_Object()
    {
        var array = new ArrayValue(
            [
                new StringValue("one"),
                new StringValue("two"),
                new StringValue("three")
            ],
            new GType(GPrimitiveType.String)
        );

        var method = new DynamicMethod(
            "CreateArray",
            typeof(object),
            Type.EmptyTypes
        );

        var il = method.GetILGenerator();

        ArrayEmitter.EmitToStack(il, array);
        il.Emit(OpCodes.Ret);

        var func = (Func<object>)method.CreateDelegate(typeof(Func<object>));
        var result = func();

        result.Should().BeOfType<string[]>();
        result.As<string[]>().Should().BeEquivalentTo("one", "two", "three");
    }

    [Fact]
    public void Should_Emit_Array_Of_Ints_And_Return_As_Object()
    {
        var array = new ArrayValue(
            [
                new IntValue(1),
                new IntValue(2),
                new IntValue(3)
            ],
            new GType(GPrimitiveType.Int)
        );

        var method = new DynamicMethod(
            "CreateIntArray",
            typeof(object),
            Type.EmptyTypes
        );

        var il = method.GetILGenerator();

        ArrayEmitter.EmitToStack(il, array);
        il.Emit(OpCodes.Ret);

        var func = (Func<object>)method.CreateDelegate(typeof(Func<object>));
        var result = func();

        result.Should().BeOfType<int[]>();
        result.As<int[]>().Should().BeEquivalentTo([1, 2, 3]);
    }
    
    [Fact]
    public void Should_Emit_Array_Of_Bools_And_Return_As_Object()
    {
        var array = new ArrayValue(
            [
                new BooleanValue(true),
                new BooleanValue(false),
                new BooleanValue(true)
            ],
            new GType(GPrimitiveType.Boolean)
        );

        var method = new DynamicMethod(
            "CreateBoolArray",
            typeof(object),
            Type.EmptyTypes
        );

        var il = method.GetILGenerator();

        ArrayEmitter.EmitToStack(il, array);
        il.Emit(OpCodes.Ret);

        var func = (Func<object>)method.CreateDelegate(typeof(Func<object>));
        var result = func();

        result.Should().BeOfType<bool[]>();
        result.As<bool[]>().Should().BeEquivalentTo([true, false, true]);
    }
    
    [Fact]
    public void Should_Emit_Array_Of_Floats_And_Return_As_Object()
    {
        var array = new ArrayValue(
            [ new FloatValue(1.1f), new FloatValue(2.2f), new FloatValue(3.3f) ],
            new GType(GPrimitiveType.Float)
        );

        var method = new DynamicMethod("CreateFloatArray", typeof(object), Type.EmptyTypes);
        var il = method.GetILGenerator();

        ArrayEmitter.EmitToStack(il, array);
        il.Emit(OpCodes.Ret);

        var func = (Func<object>)method.CreateDelegate(typeof(Func<object>));
        var result = func();

        result.Should().BeOfType<float[]>();
        result.As<float[]>().Should().BeEquivalentTo([1.1f, 2.2f, 3.3f]);
    }
    
    [Fact]
    public void Should_Emit_Array_Of_Doubles_And_Return_As_Object()
    {
        var array = new ArrayValue(
            [ new DoubleValue(1.5), new DoubleValue(2.5), new DoubleValue(3.5) ],
            new GType(GPrimitiveType.Double)
        );

        var method = new DynamicMethod("CreateDoubleArray", typeof(object), Type.EmptyTypes);
        var il = method.GetILGenerator();

        ArrayEmitter.EmitToStack(il, array);
        il.Emit(OpCodes.Ret);

        var func = (Func<object>)method.CreateDelegate(typeof(Func<object>));
        var result = func();

        result.Should().BeOfType<double[]>();
        result.As<double[]>().Should().BeEquivalentTo([1.5, 2.5, 3.5]);
    }
    
    [Fact]
    public void Should_Throw_When_Emitting_Array_Of_Decimal()
    {
        var array = new ArrayValue(
            [ new DecimalValue(1.1m), new DecimalValue(2.2m) ],
            new GType(GPrimitiveType.Decimal)
        );

        var method = new DynamicMethod("CreateDecimalArray", typeof(object), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var act = () =>
        {
            ArrayEmitter.EmitToStack(il, array);
            il.Emit(OpCodes.Ret);

            var func = (Func<object>)method.CreateDelegate(typeof(Func<object>));
            func();
        };

        act.Should().Throw<NotSupportedException>()
            .WithMessage("Unsupported array element: DecimalValue");
    }
}