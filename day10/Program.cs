// See https://aka.ms/new-console-template for more information

using System.Collections.Immutable;
using CommonHelpers;
using day10;

List<List<Symbol>> ReadInput(string path)
{
    return System.IO.File.ReadLines(path)
        .Select(
            line => line.Strings().Select(ParseSymbol).ToList()
        ).ToList();
}

Symbol ParseSymbol(string symbol)
{
    return symbol switch
    {
        "(" => Symbol.ParenOpen,
        ")" => Symbol.ParenClose,

        "[" => Symbol.SquareBracketOpen,
        "]" => Symbol.SquareBracketClose,

        "<" => Symbol.AngleBracketOpen,
        ">" => Symbol.AngleBracketClose,

        "{" => Symbol.BraceOpen,
        "}" => Symbol.BraceClose,

        _ => throw new InvalidOperationException($"Unexpected input symbol {symbol}")
    };
}

ParseResult ParseLine(List<Symbol> line)
{
    var stack = new Stack<Symbol>();

    foreach (var s in line)
    {
        if (s.IsOpenSymbol())
        {
            stack.Push(s);
        }
        else if (s.IsCloseSymbol())
        {
            var expected = stack.Pop().MatchingSymbol();
            if (expected != s)
            {
                return new CorruptParse(s);
            }
        }
    }

    return stack.Count > 0 ? new IncompleteParse(stack) : new ParseSuccess();
}

void Part1(string path)
{
    var input = ReadInput(path);
    var corrupted = input.Select(ParseLine).OfType<CorruptParse>();
    var score = corrupted.Select(c => c.Score).Sum();
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Syntax score: { score }");
    Console.WriteLine($" └────────────");
}

void Part2(string path)
{
    var input = ReadInput(path);
    var incomplete = input.Select(ParseLine).OfType<IncompleteParse>();
    var scores = incomplete.Select(i => i.Score).OrderBy(s => s).ToList();
    var midScore = scores[(scores.Count - 1) / 2];
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Syntax score: { midScore }");
    Console.WriteLine($" └────────────");
}

Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");

abstract class ParseResult
{
}

class ParseSuccess : ParseResult
{
}

class IncompleteParse : ParseResult
{
    public ImmutableList<Symbol> CompletionString { get; }
    public IncompleteParse(Stack<Symbol> completionStack)
    {
        CompletionString = ImmutableList<Symbol>.Empty.AddRange(completionStack.ToList().Select(s => s.MatchingSymbol()));
    }

    private UInt64? _score = null;
    public UInt64 Score
    {
        get
        {
            if (!_score.HasValue)
            {
                UInt64 s = 0;
                foreach (var c in CompletionString)
                {
                    s *= 5;
                    s += (UInt64) (c switch
                    {
                        Symbol.ParenClose => 1,
                        Symbol.SquareBracketClose => 2,
                        Symbol.BraceClose => 3,
                        Symbol.AngleBracketClose => 4,
                        _ => throw new InvalidOperationException($"Could not get score for {c:G}")
                    });
                }
                _score = s;
            }

            return _score.Value;
        }
    }
}

class CorruptParse : ParseResult
{
    public Symbol FirstIncorrectSymbol { get;  }
    public CorruptParse(Symbol firstIncorrectSymbol)
    {
        FirstIncorrectSymbol = firstIncorrectSymbol;
    }

    public int Score => FirstIncorrectSymbol switch
    {
        Symbol.ParenClose => 3,
        Symbol.SquareBracketClose => 57,
        Symbol.BraceClose => 1197,
        Symbol.AngleBracketClose => 25137,
        _ => throw new InvalidOperationException($"Could not find score for {FirstIncorrectSymbol:G}")
    };
}
