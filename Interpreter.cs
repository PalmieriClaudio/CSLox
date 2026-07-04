namespace CSLox;

public class Interpreter:Expr.IVisitor<object?>
{
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
        object? left = expr.left;
        object? right = expr.right;

        return expr.opr.type switch
        {
            TokenType.MINUS => (double?)left - (double?)right,
            TokenType.SLASH => (double?)left / (double?)right,
            TokenType.STAR => (double?)left * (double?)right,
            TokenType.PLUS => left is double l && right is double r ? l+r: (string)left + (string)right,
            TokenType.GREATER => (double)left > (double)right,
            TokenType.GREATER_EQUAL => (double)left >= (double)right,
            TokenType.LESS => (double)left < (double)right,
            TokenType.LESS_EQUAL => (double)left <= (double)right,
            TokenType.BANG_EQUAL => !IsEqual(left, right),
            TokenType.EQUAL_EQUAL => IsEqual(left, right),
            _ => null,
        };
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

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }
}
