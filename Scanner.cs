using System.Collections.Generic;

namespace CSLox;

class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = [];
    private int start = 0;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> keywords =
        new()
        {
            {"and", TokenType.AND},
            {"class", TokenType.CLASS},
            {"else", TokenType.ELSE},
            {"false", TokenType.FALSE},
            {"for", TokenType.FOR},
            {"fun", TokenType.FUN},
            {"if", TokenType.IF},
            {"nil", TokenType.NIL},
            {"or", TokenType.OR},
            {"print", TokenType.PRINT},
            {"return", TokenType.RETURN},
            {"super", TokenType.SUPER},
            {"this", TokenType.THIS},
            {"true", TokenType.TRUE},
            {"var", TokenType.VAR},
            {"while", TokenType.WHILE}
        };

    internal Scanner(string source)
    {
        this.source = source;
    }

    internal List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(TokenType.EOF, "", null, line));
        return tokens;
    }

    private void ScanToken()
    {
        char c = Advance();
        switch (c)
        {
            case '(': AddToken(TokenType.LEFT_PAREN); break;
            case ')': AddToken(TokenType.RIGHT_PAREN); break;
            case '{': AddToken(TokenType.LEFT_BRACE); break;
            case '}': AddToken(TokenType.RIGHT_BRACE); break;
            case ',': AddToken(TokenType.COMMA); break;
            case '.': AddToken(TokenType.DOT); break;
            case '-': AddToken(TokenType.MINUS); break;
            case '+': AddToken(TokenType.PLUS); break;
            case ';': AddToken(TokenType.SEMICOLON); break;
            case '*': AddToken(TokenType.STAR); break;
            case '!': AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG); break;
            case '=': AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL); break;
            case '<': AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS); break;
            case '>': AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER); break;
            case '/':
                      if (Match('/'))
                      {
                          while (Peek() != '\n' && !IsAtEnd()) Advance();
                      }
                      else
                      {
                          AddToken(TokenType.SLASH);
                      }
                      break;
            case ' ':
            case '\r':
            case '\t':
                      break;
            case '\n':
                      line++;
                      break;
            case '"': String(); break;
            default:
                      // Here we skipped writing the utility the book writes, because as far as I can tell the C# char.IsDigit method is a good fit and doesn't involve the issues the Java one has.
                      if (char.IsDigit(c))
                      {
                          Number();
                      }
                      else if (char.IsAsciiLetter(c))
                      {
                          Identifier();
                      }
                      else
                      {
                          CSLox.Error(line, "Unexpected character.");
                      }
                      break;
        }
    }

    private void Identifier()
    {
        // Same concept as with IsDigit, we're skipping the utilities since the char methods seem appropriate.
        while (char.IsAsciiLetterOrDigit(Peek())) Advance();

        string text = source[start..current];

        if (!keywords.TryGetValue(text, out TokenType type))
            type = TokenType.IDENTIFIER;
        AddToken(type);
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd()) // We're managing multiline strings with the default string symbol. I don't like that, but it's kept the same for the purpose of being faithfull to docs.
        {
            if (Peek() == '\n') line++;
            Advance();
        }

        if (IsAtEnd())
        {
            CSLox.Error(line, "Unterminated string.");
            return;
        }

        Advance();

        string value = source[(start + 1)..(current - 1)];
        AddToken(TokenType.STRING, value);
    }

    private void Number()
    {
        while (char.IsDigit(Peek())) Advance();

        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance();

            while (char.IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.NUMBER, double.Parse(source[start..current]));
    }

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (source[current] != expected) return false;

        current++;
        return true;
    }

    private char Peek()
    {
        if (IsAtEnd()) return '\0'; // Null byte, will exit on !IsAtEnd
        return source[current];
    }

    private char PeekNext()
    {
        if (current + 1 >= source.Length) return '\0';
        return source[current + 1];
    }

    private char Advance() => source[current++];

    private void AddToken(TokenType type) => AddToken(type, null);

    private void AddToken(TokenType type, object? literal)
    {
        string text = source[start..current];
        tokens.Add(new Token(type, text, literal, line));
    }

    private bool IsAtEnd() => current >= source.Length;
} 
