using System;
using System.Collections.Generic;

namespace CSLox;

public class NativeFunction: LoxCallable
{
    private readonly int arity;
    private readonly Func<Interpreter, List<object>, object> call;

    public NativeFunction(int arity, Func<Interpreter, List<object>, object> call)
    {
        this.arity = arity;
        this.call = call;
    }

    public int Arity() => arity;
    public object Call(Interpreter interpreter, List<object> arguments) => call(interpreter, arguments);
    public override string ToString() => "<native fn>";
}
