using System;
using System.Collections.Generic;

namespace CSLox;

public class Environment
{
    internal readonly Environment? enclosing;
    private readonly Dictionary<string, object?> values = [];

    public Environment()
    {
        this.enclosing = null;
    }

    public Environment(Environment enclosing)
    {
        this.enclosing = enclosing;
    }

    internal void Define(string name, object? value)
    {
        values[name] = value;
    }

    Environment Ancestor(int distance)
    {
        Environment environment = this;
        for (int i = 0; i < distance; i++)
        {
            environment = environment.enclosing ?? throw new InvalidOperationException($"No environment found at distance {i}");
        }

        return environment;
    }

    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance).values[name];
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance).values[name.lexeme] = value;
    }

    internal object? Get(Token name)
    {
        if (values.TryGetValue(name.lexeme, out object? val))
            return val;

        if (enclosing is not null) return enclosing.Get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }

    internal void Assign(Token name, object? value)
    {
        if (values.ContainsKey(name.lexeme))
        {
            values[name.lexeme] = value;
            return;
        }

        if (enclosing is not null)
        {
            enclosing.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
    }
}
