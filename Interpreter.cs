using System;

namespace CSLox;

public class Interpreter:Expr.IVisitor<object?>
{
    public void Interpret(Expr expression)
    {
        try
        {
            object? value = Evaluate(expression);
            Console.WriteLine(Stringify(value));
        }
        catch (RuntimeError error)
        {
            CSLox.RuntimeError(error);
        }
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.value;
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        object? right = Evaluate(expr.right);

        switch (expr.opr.type)
        {
            case TokenType.BANG:
                return !IsTruthy(right);
            case TokenType.MINUS:
                CheckNumberOperand(expr.opr, right);
                return -(double?)right;
        }

        return null;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr.expression);
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        object? left = Evaluate(expr.left);
        object? right = Evaluate(expr.right);

        switch (expr.opr.type)
        {
            case TokenType.MINUS:
                CheckNumberOperands(expr.opr, left, right);
                return (double?)left - (double?)right;
            case TokenType.SLASH:
                CheckNumberOperands(expr.opr, left, right);
                return (double?)left / (double?)right;
            case TokenType.STAR:
                CheckNumberOperands(expr.opr, left, right);
                return (double?)left * (double?)right;
            case TokenType.PLUS:
                return left is double l && right is double r ? l+r: left?.ToString() + right?.ToString();//(string?)left + (string?)right;
            case TokenType.GREATER:
                CheckNumberOperands(expr.opr, left, right);
                return (double?)left > (double?)right;
            case TokenType.GREATER_EQUAL:
                CheckNumberOperands(expr.opr, left, right);
                return (double?)left >= (double?)right;
            case TokenType.LESS:
                CheckNumberOperands(expr.opr, left, right);
                return (double?)left < (double?)right;
            case TokenType.LESS_EQUAL:
                CheckNumberOperands(expr.opr, left, right);
                return (double?)left <= (double?)right;
            case TokenType.BANG_EQUAL:
                return !IsEqual(left, right);
            case TokenType.EQUAL_EQUAL:
                return IsEqual(left, right);
            default:
                return null;
        }
    }

    private static void CheckNumberOperand(Token opr, object? operand)
    {
        if (operand is double) return;
        throw new RuntimeError(opr, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token opr, object? left, object? right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(opr, "Operands must be numbers.");
    }

    private static bool IsTruthy(object? obj)
    {
        if (obj == null) return false;
        if (obj is bool b) return b;
        return true;
    }

    private static bool IsEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null) return false;

        return a.Equals(b);
    }

    private static string Stringify(object? obj)
    {
        if (obj is null) return "nil";

        if (obj is double d)
        {
            string text = d.ToString();
            if (text.EndsWith(".0"))
            {
                text = text[..^2];
            }
            return text;
        }

        return obj?.ToString() ?? ""; //Cannot actually be null, just wanted to shut Roslyn up
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }
}
