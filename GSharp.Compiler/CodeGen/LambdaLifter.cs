using GSharp.Compiler.AST;

namespace GSharp.Compiler.CodeGen;

public record LambdaLiftResult(
    List<FunctionDeclaration> LiftedFunctions,
    Dictionary<LambdaExpression, string> LambdaNames);

// Lambdas are non-capturing: a LambdaExpression can only see its own parameters, never an
// enclosing function's locals/parameters (codegen has no support for that at all — every
// function gets a brand-new, empty EmitContext). So every LambdaExpression found anywhere in
// the AST is "lifted" into an independent synthetic top-level FunctionDeclaration and fed
// through the exact same two-pass DefineFunction/EmitFunction machinery as a named function.
// This is a pure collection pass — it never rewrites the tree, so every node it walks keeps
// the same reference identity the TypeInferrer's TypeMap was already built against.
public static class LambdaLifter
{
    public static LambdaLiftResult Lift(List<Expression> expressions, string prefix = "")
    {
        var lifted = new List<FunctionDeclaration>();
        var names = new Dictionary<LambdaExpression, string>(ReferenceEqualityComparer.Instance);
        var counter = 0;

        foreach (var expression in expressions)
            Walk(expression, prefix, lifted, names, ref counter);

        return new LambdaLiftResult(lifted, names);
    }

    private static void Walk(
        Expression expression,
        string prefix,
        List<FunctionDeclaration> lifted,
        Dictionary<LambdaExpression, string> names,
        ref int counter)
    {
        switch (expression)
        {
            case LambdaExpression lambda:
                var name = $"__lambda_{++counter}";
                names[lambda] = prefix + name;
                lifted.Add(new FunctionDeclaration(name, lambda.Parameters, lambda.Body));
                WalkBody(lambda.Body, prefix, lifted, names, ref counter);
                break;

            case BindingExpression binding:
                Walk(binding.Value, prefix, lifted, names, ref counter);
                break;

            case PrintExpression print:
                Walk(print.Value, prefix, lifted, names, ref counter);
                break;

            case IfExpression ifExpression:
                Walk(ifExpression.Condition, prefix, lifted, names, ref counter);
                WalkBody(ifExpression.ThenBody, prefix, lifted, names, ref counter);
                if (ifExpression.ElseBody is not null)
                    WalkBody(ifExpression.ElseBody, prefix, lifted, names, ref counter);
                break;

            case ForExpression forExpression:
                Walk(forExpression.Iterable, prefix, lifted, names, ref counter);
                WalkBody(forExpression.Body, prefix, lifted, names, ref counter);
                break;

            case FunctionDeclaration functionDeclaration:
                WalkBody(functionDeclaration.Body, prefix, lifted, names, ref counter);
                break;

            case CallExpression call:
                WalkBody(call.Arguments, prefix, lifted, names, ref counter);
                break;

            case ModuleCallExpression moduleCall:
                WalkBody(moduleCall.Arguments, prefix, lifted, names, ref counter);
                break;

            case BinaryExpression binary:
                Walk(binary.Left, prefix, lifted, names, ref counter);
                Walk(binary.Right, prefix, lifted, names, ref counter);
                break;

            case UnaryExpression unary:
                Walk(unary.Operand, prefix, lifted, names, ref counter);
                break;
        }
    }

    private static void WalkBody(
        List<Expression> body,
        string prefix,
        List<FunctionDeclaration> lifted,
        Dictionary<LambdaExpression, string> names,
        ref int counter)
    {
        foreach (var expression in body)
            Walk(expression, prefix, lifted, names, ref counter);
    }
}
