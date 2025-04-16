using System.Reflection.Emit;
using FluentAssertions;
using G.Sharp.Compiler.AST;
using G.Sharp.Compiler.CodeGen;

namespace G.Sharp.Compiler.Tests.CodeGen;

public class LetEmitterTests
{
    [Fact]
    public void Should_Emit_LetStatement_And_Store_Local()
    {
        var locals = new Dictionary<string, LocalBuilder>();
        var statement = new LetStatement("x", new LiteralExpression(new IntValue(123)));

        var method = new DynamicMethod("LetEmit", typeof(int), Type.EmptyTypes);
        var il = method.GetILGenerator();

        LetEmitter.Emit(il, statement, locals);

        il.Emit(OpCodes.Ldloc, locals["x"]);
        il.Emit(OpCodes.Ret);

        var func = (Func<int>)method.CreateDelegate(typeof(Func<int>));
        func().Should().Be(123);
    }

    [Fact]
    public void Should_Replace_Local_If_Already_Exists()
    {
        var method = new DynamicMethod("LetEmitReplace", typeof(int), Type.EmptyTypes);
        var il = method.GetILGenerator();

        var oldLocal = il.DeclareLocal(typeof(int));
        var locals = new Dictionary<string, LocalBuilder> { ["x"] = oldLocal };

        var statement = new LetStatement("x", new LiteralExpression(new IntValue(99)));

        LetEmitter.Emit(il, statement, locals);

        il.Emit(OpCodes.Ldloc, locals["x"]);
        il.Emit(OpCodes.Ret);

        var func = (Func<int>)method.CreateDelegate(typeof(Func<int>));
        func().Should().Be(99);
    }
}