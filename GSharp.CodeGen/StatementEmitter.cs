using System.Reflection.Emit;
using GSharp.AST;

namespace GSharp.CodeGen;

public static class StatementEmitter
{
    public static void Emit(ILGenerator il, Statement stmt, Dictionary<string, LocalBuilder> variables)
    {
        switch (stmt)
        {
            case LetStatement letStmt:
                LetEmitter.Emit(il, letStmt, variables);
                break;

            case AssignmentStatement assignStmt:
                AssignmentEmitter.Emit(il, assignStmt, variables);
                break;

            case PrintStatement printStmt:
                PrintEmitter.Emit(il, printStmt, variables);
                break;

            case ForStatement forStmt:
                ForEmitter.Emit(il, forStmt, variables);
                break;
            
            case IfStatement ifStmt:
                IfEmitter.Emit(il, ifStmt, variables);
                break;

            default:
                throw new NotSupportedException($"Unsupported statement: {stmt.GetType().Name}");
        }
    }
}