using System.Collections.Generic;

namespace CSLox;

public class Resolver: Expr.IVisitor<object?>, Stmt.IVisitor<object?>
{
    private readonly Interpreter interpreter;
    private readonly List<Dictionary<string, bool>> scopes = [];
    private EFunctionType currentFunction = EFunctionType.NONE;

    private enum EFunctionType
    {
        NONE,
        FUNCTION,
    }

    public Resolver(Interpreter interpreter)
    {
        this.interpreter = interpreter;
    }

    public object? VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.statements);
        EndScope();
        return null;
    }

    public object? VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.expression);
        return null;
    }

    public object? VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.condition);
        Resolve(stmt.thenBranch);
        if (stmt.elseBranch is not null) Resolve(stmt.elseBranch);

        return null;
    }

    public object? VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.expression);
        return null;
    }

    public object? VisitReturnStmt(Stmt.Return stmt)
    {
        if (currentFunction == EFunctionType.NONE)
            CSLox.Error(stmt.keyword, "Can't return from top-level code.");

        if (stmt.value is not null)
        {
            Resolve(stmt.value);
        }
        return null;
    }

    public object? VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.name);
        Define(stmt.name);

        ResolveFunction(stmt, EFunctionType.FUNCTION);
        return null;
    }

    public object? VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.name);
        if (stmt.initializer is not null)
        {
            Resolve(stmt.initializer);
        }
        return null;
    }

    public object? VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.condition);
        Resolve(stmt.body);
        return null;
    }

    public object? VisitAssignExpr(Expr.Assign expr)    
    {
        Resolve(expr.value);
        ResolveLocal(expr, expr.name);
        return null;
    }

    public object? VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.left);
        Resolve(expr.right);
        return null;
    }

    public object? VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.callee);

        foreach (Expr argument in expr.arguments)
        {
            Resolve(argument);
        }

        return null;
    }

    public object? VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.expression);
        return null;
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return null;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.left);
        Resolve(expr.right);
        return null;
    }

    public object? VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.right);
        return null;
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        if (!(scopes.Count == 0) && scopes[^1][expr.name.lexeme] == false)
        {
            CSLox.Error(expr.name, "Can't read local variable in it's own initializer");
        }

        ResolveLocal(expr, expr.name);
        return null;
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (Stmt statement in statements)
        {
            Resolve(statement);
        }
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void ResolveFunction(Stmt.Function function, EFunctionType type)
    {
        EFunctionType enclosingFunction = currentFunction;
        currentFunction = type;

        BeginScope();
        foreach (Token parameter in function.parameters)
        {
            Declare(parameter);
            Define(parameter);
        }
        Resolve(function.body);
        EndScope();
        currentFunction = enclosingFunction;
    }

    private void BeginScope()
    {
        scopes.Add([]);
    }

    private void EndScope()
    {
        scopes.RemoveAt(scopes.Count - 1);
    }

    private void Declare(Token name)
    {
        if (scopes.Count == 0) return;

        if (scopes[^1].ContainsKey(name.lexeme))
        {
            CSLox.Error(name, "There's already a variable with this name in scope.");
        }
        scopes[^1].Add(name.lexeme, false);

    }

    private void Define(Token name)
    {
        if (scopes.Count == 0) return;

        scopes[^1].Add(name.lexeme, true);
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes[i].ContainsKey(name.lexeme))
            {
                interpreter.Resolve(expr, scopes.Count - 1 - i);
                return;
            }
        }
    }
}
