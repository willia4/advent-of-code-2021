namespace day10;

public enum Symbol
{
    ParenOpen,
    ParenClose,
    
    BraceOpen,
    BraceClose,
    
    SquareBracketOpen,
    SquareBracketClose,
    
    AngleBracketOpen,
    AngleBracketClose
}

public static class SymbolHelper
{
    public static Symbol MatchingSymbol(this Symbol symbol)
    {
        return symbol switch
        {
            Symbol.ParenClose => Symbol.ParenOpen,
            Symbol.ParenOpen => Symbol.ParenClose,

            Symbol.SquareBracketClose => Symbol.SquareBracketOpen,
            Symbol.SquareBracketOpen => Symbol.SquareBracketClose,

            Symbol.AngleBracketClose => Symbol.AngleBracketOpen,
            Symbol.AngleBracketOpen => Symbol.AngleBracketClose,

            Symbol.BraceClose => Symbol.BraceOpen,
            Symbol.BraceOpen => Symbol.BraceClose,

            _ => throw new InvalidOperationException($"Unexpected input symbol {symbol}")
        };
    }
    
    public static bool IsOpenSymbol(this Symbol symbol)
    {
        return symbol is Symbol.BraceOpen or Symbol.ParenOpen or Symbol.AngleBracketOpen or Symbol.SquareBracketOpen;
    }

    public static bool IsCloseSymbol(this Symbol symbol)
    {
        return !symbol.IsOpenSymbol();
    }
}