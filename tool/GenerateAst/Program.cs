namespace GenerateAst;

public class GenerateAst
{
    public static int Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.Error.WriteLine("Usage: generate_ast <output directory>");
            return 64;
        } 

        string outputDir = args[0];
        DefineAst(outputDir, "Expr", new List<string> 
                {
                "Binary     : Expr left, Token opr, Expr right",
                "Grouping   : Expr expression",
                "Literal    : object value",
                "Unary      : Token opr, Expr right"
                });

        return 0;
    }

    private static void DefineAst(string outputDir, string baseName, List<string> types)
    {
        string path = Path.Combine(outputDir, baseName + ".cs");

        List<string> lines = 
           ["namespace CSLox;",
            "",
            "public abstract class " + baseName,
            "{",
            ""];

        foreach (string type in types)
        {
            string className = type.Split(":")[0].Trim();
            string fields = type.Split(":")[1].Trim();
            DefineType(lines, baseName, className, fields);
        }

        lines.Add("}");

        File.WriteAllLines(path, lines);
    }

    private static void DefineType(List<string> lines, string baseName, string className, string fields)
    {
        lines.Add(" public class " + className + ":" + baseName);
        lines.Add(" {");
        lines.Add("     public " + className + "(" + fields + ")");
        lines.Add("     {");

        foreach (var field in fields.Split(", "))
        {
            string fieldName = field.Split(" ")[1];
            lines.Add("         this." + fieldName + " = " + fieldName + ";");
        }

        lines.Add("     }");
        lines.Add("");

        foreach (var field in fields.Split(", "))
        {
            lines.Add("     public readonly " + field + ";");
        }

        lines.Add(" }");
    }

}
