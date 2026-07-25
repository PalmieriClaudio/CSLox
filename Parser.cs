using System;
using System.Collections.Generic;

namespace CSLox;

public class Parser
{
    private class ParseError:Exception {}

    private readonly List<Token> tokens;
    private int current = 0;

    internal Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    internal List<Stmt> Parse()
    {
        List<Stmt> statements = [];
        while (!IsAtEnd())
        {
            Stmt? stmt = Declaration();
            if (stmt is Stmt s) statements.Add(s);
        }

        return statements;
    }

    private Expr Expression()
    {
        return Assignment();
    }

    private Stmt Statement()
    {
        if (Match(TokenType.PRINT)) return PrintStatement();
        if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

        return ExpressionStatement();
    }

    private Stmt? Declaration()
    {
        try
        {
            if (Match(TokenType.VAR)) return VarDeclaration();

            return Statement();
        }
        catch (ParseError)
        {
            Syncronize();
            return null;
        }
    }

    private Stmt.Print PrintStatement()
    {
        Expr value = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after value.");
        return new Stmt.Print(value);
    }

    private List<Stmt> Block()
    {
        List<Stmt> statements = [];

        while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
        {
            Stmt? stmt = Declaration();
            if (stmt is Stmt s) statements.Add(s);
        }

        Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");
        return statements;
    }
 
    private Stmt.Var VarDeclaration()
    {
        Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

        Expr? initializer = null;
        if (Match(TokenType.EQUAL)) initializer = Expression();

        Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
        return new Stmt.Var(name, initializer);
    }

    private Stmt.Expression ExpressionStatement()
    {
        Expr expr = Expression();
        Consume(TokenType.SEMICOLON, "Expect ';' after expression.");
        return new Stmt.Expression(expr);
    }
    
    private Expr Assignment()
    {
        Expr expr = Equality();

        if (Match(TokenType.EQUAL))
        {
            Token equals = Previous();
            Expr value = Assignment();

            if (expr is Expr.Variable e)
            {
                Token name = e.name;
                return new Expr.Assign(name, value);
            }

            Error(equals, "Invalid assignment targhet.");
        }

        return expr;
    }

    private Expr Equality()
    {
        Expr expr = Comparison();

        while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
        {
            Token opr = Previous();
            Expr right = Comparison();
            expr = new Expr.Binary(expr, opr, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        Expr expr = Term();

        while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
        {
            Token opr = Previous();
            Expr right = Term();
            expr = new Expr.Binary(expr, opr, right);
        }

        return expr;
    }

    private Expr Term()
    {   
        Expr expr = Factor();

        while (Match(TokenType.MINUS, TokenType.PLUS))
        {
            Token opr = Previous();
            Expr right = Factor();
            expr = new Expr.Binary(expr, opr, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        Expr expr = Unary();

        while (Match(TokenType.SLASH, TokenType.STAR))
        {
            Token opr = Previous();
            Expr right = Unary();
            expr = new Expr.Binary(expr, opr, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(TokenType.BANG, TokenType.MINUS))
        {
            Token opr = Previous();
            Expr right = Unary();
            return new Expr.Unary(opr, right);
        }

        return Primary();
    }

    private Expr Primary()
    {
        if (Match(TokenType.FALSE)) return new Expr.Literal(false);
        if (Match(TokenType.TRUE)) return new Expr.Literal(true);
        if (Match(TokenType.NIL)) return new Expr.Literal(null);

        if (Match(TokenType.NUMBER, TokenType.STRING))
            return new Expr.Literal(Previous().literal);

        if (Match(TokenType.IDENTIFIER))
            return new Expr.Variable(Previous());

        if (Match(TokenType.LEFT_PAREN))
        {
            Expr expr = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after expression");
            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (TokenType type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;
        return Previous();
    }

    private bool IsAtEnd()
    {
        return Peek().type == TokenType.EOF;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }

    private static ParseError Error(Token token, string message)
    {
        CSLox.Error(token, message);
        return new ParseError();
    }

    private void Syncronize()
    {
        while (!IsAtEnd())
        {
            Advance(); // This is different than how the book does this part, in the book we advance before the loop and then again at the end, but I didn't see a reason to do it like that.
            if (Previous().type == TokenType.SEMICOLON) return;

            switch (Peek().type)
            {
                case TokenType.CLASS:
                case TokenType.FUN:
                case TokenType.VAR:
                case TokenType.FOR:
                case TokenType.IF:
                case TokenType.WHILE:
                case TokenType.PRINT:
                case TokenType.RETURN:
                    return;
            }
        }
    }
}
