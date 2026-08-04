using System.Collections.Generic;

namespace CSLox;

interface LoxCallable
{
    int Arity();
    object? Call(Interpreter interpreter, List<object> arguments);
}
