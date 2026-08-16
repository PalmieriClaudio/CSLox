using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSLox;

public class CSLox
{
    // cslox interpreter implementation
    // Implementation following craftinginterpreters.com from @Robert Nystrom

    private static readonly Interpreter interpreter = new Interpreter();
    internal static bool HadError { get; set; }
    internal static bool HadRuntimeError { get; set; }

    static int Main(string[] args)
    {
        try
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: cslox [script]");
                return 64; // UsageError FreeBSD sysexits
            }
            else if (args.Length == 1)
            {
                return RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }
        catch (IOException)
        {
            return 74; //IOException FreeBSD sysexits
        }
        return 0;
    }

    private static int RunFile(string path)
    {
        Run(File.ReadAllText(path, Encoding.UTF8)); //Need to figure out if "ReadAllText" is fine (does it return white space??)

        if (HadError) return 65;
        if (HadRuntimeError) return 70;

        return 0;
    }

    private static void RunPrompt()
    {
        while (true)
        {
            string? line;
            line = Console.ReadLine();
            if (line == null)
                break;
            Run(line);
        }
    }

    private static void Run(string source)
    {
        Scanner scanner = new Scanner(source);
        List<Token> tokens = scanner.ScanTokens();

        Parser parser = new Parser(tokens);
        List<Stmt> statements = parser.Parse();

        if (HadError) return;

        Resolver resolver = new Resolver(interpreter);
        resolver.Resolve(statements);

        if (HadError) return;

        interpreter.Interpret(statements);
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    internal static void Error(Token token, string message)
    {
        if (token.type == TokenType.EOF)
        {
            Report(token.line, " at end", message);
        }
        else
        {
            Report(token.line, " at '" + token.lexeme + "'", message);
        }
    }

    internal static void RuntimeError(RuntimeError error)
    {
        Console.WriteLine(error.Message + "\n[line " + error.token.line + "]");
        HadRuntimeError = true;
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine("[line " +line+ "] Error" + where + ": " + message);
        HadError = true;
    }
}
