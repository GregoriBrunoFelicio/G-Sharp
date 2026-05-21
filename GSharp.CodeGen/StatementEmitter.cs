using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

// Central dispatch point for all statement types.
//
// Each statement type has its own dedicated emitter class.
// StatementEmitter just routes the call to the right one.
// This keeps the individual emitters focused and easy to find.
//
// Adding a new statement type:
//   1. Create a new XxxEmitter.cs with a static Emit method.
//   2. Add a case here that calls it.
public static class StatementEmitter
{
    public static void Emit(ILGenerator il, Statement stmt, EmitContext ctx)
    {
        switch (stmt)
        {
            case LetStatement letStmt:
                LetEmitter.Emit(il, letStmt, ctx);
                break;

            case PrintStatement printStmt:
                PrintEmitter.Emit(il, printStmt, ctx);
                break;

            case ForStatement forStmt:
                ForEmitter.Emit(il, forStmt, ctx);
                break;

            case WhileStatement whileStmt:
                WhileEmitter.Emit(il, whileStmt, ctx);
                break;

            case IfStatement ifStmt:
                IfEmitter.Emit(il, ifStmt, ctx);
                break;

            case ExpressionStatement exprStmt:
                // Expressions used as statements (function calls at top level,
                // or non-tail expressions inside a function body) leave a value
                // on the stack. We discard it with Pop because statement-level
                // expressions don't contribute a value to anything.
                ExpressionEmitter.EmitToStack(il, exprStmt.Expression, ctx);
                il.Emit(OpCodes.Pop);
                break;

            case FunctionDeclaration:
                // Function declarations are fully emitted in a dedicated pass
                // by the Compiler before Main is emitted. Nothing to do here.
                break;

            default:
                throw new NotSupportedException($"Unsupported statement: {stmt.GetType().Name}");
        }
    }
}
