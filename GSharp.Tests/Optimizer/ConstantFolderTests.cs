using FluentAssertions;
using GSharp.Compiler.AST;
using GSharp.Compiler.Optimizer;
using GSharp.Compiler.TypeChecker;

namespace G.Sharp.Compiler.Tests.Optimizer;

public class ConstantFolderTests
{
    private static List<Expression> Fold(string source)
    {
        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = new GSharp.Compiler.Parser.Parser(tokens).Parse();
        return ConstantFolder.FoldAll(expressions);
    }

    private static Expression BoundValue(string source) =>
        ((BindingExpression)Fold(source)[0]).Value;

    [Fact]
    public void Folds_Int_Addition()
    {
        BoundValue("y -> 1 + 1").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(2);
    }

    [Fact]
    public void Folds_Arithmetic_Inside_A_Comparison()
    {
        BoundValue("y -> 2 * 3 == 6").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(true);
    }

    [Fact]
    public void Folds_Nested_Binary_Expressions_Bottom_Up()
    {
        BoundValue("y -> (1 + 2) + 3").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(6);
    }

    [Fact]
    public void Folds_Mixed_Int_And_Double_With_Promotion()
    {
        BoundValue("y -> 1 + 1.5").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(2.5);
    }

    [Fact]
    public void Folds_String_Concatenation()
    {
        BoundValue("y -> \"a\" + \"b\"").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be("ab");
    }

    [Fact]
    public void Does_Not_Fold_Division_By_Zero_So_It_Still_Fails_At_Runtime()
    {
        BoundValue("y -> 10 / 0").Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void Short_Circuits_False_And_Without_Needing_The_Right_Side_To_Be_Literal()
    {
        BoundValue("y -> false and x").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(false);
    }

    [Fact]
    public void Short_Circuits_True_Or_Without_Needing_The_Right_Side_To_Be_Literal()
    {
        BoundValue("y -> true or x").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(true);
    }

    [Fact]
    public void Leaves_Non_Literal_Operands_Unfolded()
    {
        var value = BoundValue("y -> x + 1");

        value.Should().BeOfType<BinaryExpression>()
            .Which.Left.Should().BeOfType<IdentifierExpression>();
    }

    [Fact]
    public void Folds_Unary_Minus_On_A_Literal()
    {
        BoundValue("y -> -5").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(-5);
    }

    [Fact]
    public void Folds_Not_On_A_Literal()
    {
        BoundValue("y -> not true").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(false);
    }

    [Fact]
    public void Folds_Unary_Minus_Over_A_Nested_Binary_Bottom_Up()
    {
        BoundValue("y -> -(1 + 2)").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(-3);
    }

    [Fact]
    public void Leaves_Unary_Minus_On_A_Non_Literal_Operand_Unfolded()
    {
        BoundValue("y -> -x").Should().BeOfType<UnaryExpression>()
            .Which.Operand.Should().BeOfType<IdentifierExpression>();
    }

    [Fact]
    public void Folds_The_Println_Argument_Directly()
    {
        var printExpression = (PrintExpression)Fold("println 10 + 10")[0];

        printExpression.Value.Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(20);
    }

    [Fact]
    public void Folds_A_Chain_Of_Same_Precedence_Multiply_And_Divide_After_A_Parenthesized_Subtraction()
    {
        BoundValue("num -> (1 - 20) * 20 / 1").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(-380);
    }

    [Fact]
    public void Equality_Does_Not_Promote_Across_Numeric_Types()
    {
        // Mirrors RuntimeHelpers.EqualEqual: boxed int and boxed double are never Equals-equal,
        // even though the pair type-checks fine.
        BoundValue("y -> 1 == 1.0").Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(false);
    }

    [Fact]
    public void Folds_A_Literal_Arithmetic_Expression_Inside_A_Lambda_Body()
    {
        var lambda = (LambdaExpression)BoundValue("f -> n => 1 + 1");

        lambda.Body.Should().ContainSingle()
            .Which.Should().BeOfType<LiteralExpression>()
            .Which.Value.Should().Be(2);
    }

    [Fact]
    public void Folded_Program_Compiles_And_Runs_With_The_Same_Output_As_Before_Folding()
    {
        var source = "println 1 + 1\nprintln 2 * 3 == 6";

        var tokens = new GSharp.Compiler.Lexer.Lexer(source).Tokenize();
        var expressions = ConstantFolder.FoldAll(new GSharp.Compiler.Parser.Parser(tokens).Parse());
        var typeMap = new TypeInferrer().Infer(expressions);

        var originalOut = Console.Out;
        var captured = new StringWriter();
        Console.SetOut(captured);
        try
        {
            new GSharp.Compiler.CodeGen.Compiler().CompileAndRun(expressions, typeMap: typeMap);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        captured.ToString().Replace("\r\n", "\n").Trim().Should().Be("2\nTrue");
    }
}
