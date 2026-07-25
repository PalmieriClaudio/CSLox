using System;
using System.Collections.Generic;

namespace CSLox;

public class Interpreter:Expr.IVisitor<object?>, Stmt.IVisitor<object?> // This is different from the book, where they use the Void java type. C# doesn't have that, and I don't like the Unit concept.
{
    private Environment environment = new Environment();

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
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

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        return environment.Get(expr.name);
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
                return left is double l && right is double r ? l+r: left?.ToString() + right?.ToString();
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

        return obj?.ToString() ?? "nil"; //Cannot actually be null, just wanted to shut Roslyn up
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void Execute(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        Environment previous = this.environment;
        try
        {
            this.environment = environment;

            foreach (Stmt statement in statements)
            {
                Execute(statement);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.statements, new Environment(environment));
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.expression);
        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        object? value = Evaluate(stmt.expression);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        object? value = null;
        if (stmt.initializer is not null)
            value = Evaluate(stmt.initializer);

        environment.Define(stmt.name.lexeme, value);
        return null;
    }

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        object? value = Evaluate(expr.value);
        environment.Assign(expr.name, value);
        return value;
    }
}
