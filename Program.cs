using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CSLox;
    
public class CSLox
{
    // cslox interpreter implementation
    // Implementation following craftinginterpreters.com from @Robert Nystrom

    internal static bool HadError { get; set; }

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
               RunFile(args[1]);
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

    private static void RunFile(string path)
    {
        Run(File.ReadAllText(path, Encoding.UTF8)); //Need to figure out if "ReadAllText" is fine (does it return white space??)
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

        foreach (Token token in tokens)
        {
            Console.WriteLine(token);
        }
    }

    internal static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    private static void Report(int line, string where, string message)
    {
        Console.Error.WriteLine("[line " +line+ "] Error" + where + ": " + message);
        HadError = true;
    }
}
