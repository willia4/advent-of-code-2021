using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CommonHelpers;

IEnumerable<IEnumerable<string>> ChunkStringByEmptyLines(IEnumerable<string> lines)
{
    // get to the first empty line
    lines = lines.SkipWhile(string.IsNullOrWhiteSpace);

    var chunks = new List<List<string>>();
    var currentChunk = new List<string>();

    foreach (var line in lines)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            if (currentChunk.Any())
            {
                chunks.Add(currentChunk);
            }

            currentChunk = new List<string>();
        }
        else
        {
            currentChunk.Add(line);            
        }
    }

    if (currentChunk.Any())
    {
        chunks.Add(currentChunk);
    }

    return chunks;
}

async Task Part1(string path)
{
    IEnumerable<string> input = await System.IO.File.ReadAllLinesAsync(path);
    var calls = input.First().Split(",").Where(s => !string.IsNullOrWhiteSpace(s)).Select(Helpers.SafeParseInt);

    input = input.Skip(1);

    var boards = ChunkStringByEmptyLines(input)
        .Select(c => new BingoBoard(c))
        .Select(b => new { Board = b, WinningCalls = CallsToWin(b, calls)})
        .ToArray();

    var earliestWin = boards
        .OrderBy(item => item.WinningCalls.Count())
        .First();

    Console.WriteLine( "");
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Earliest winning call: { earliestWin.WinningCalls.Last() }");
    Console.WriteLine($" │ Winning calls: { string.Join(", ", earliestWin.WinningCalls) }");
    Console.WriteLine( " │ Winning board: ");
    var winningBoardLines = earliestWin.Board.ToString().Split("\n");
    foreach (var w in winningBoardLines)
    {
        Console.WriteLine($" │    {w}");
    }
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Winning board score: {earliestWin.Board.Score(earliestWin.WinningCalls)}");
    Console.WriteLine($" │ Winning board score * last call: {earliestWin.Board.Score(earliestWin.WinningCalls) * earliestWin.WinningCalls.Last() }");
    Console.WriteLine($" └────────────");
}

IEnumerable<int> CallsToWin(BingoBoard b, IEnumerable<int> allCalls)
{
    var currentCalls = new List<int>();
    foreach (var nextCall in allCalls)
    {
        currentCalls.Add(nextCall);
        if (b.IsWinner(currentCalls))
        {
            return currentCalls;
        }
    }

    return Enumerable.Empty<int>();
}

async Task Part2(string path)
{
    IEnumerable<string> input = await System.IO.File.ReadAllLinesAsync(path);
    var calls = input.First().Split(",").Where(s => !string.IsNullOrWhiteSpace(s)).Select(Helpers.SafeParseInt);

    input = input.Skip(1);

    var boards = ChunkStringByEmptyLines(input)
        .Select(c => new BingoBoard(c))
        .Select(b => new { Board = b, WinningCalls = CallsToWin(b, calls)})
        .ToArray();

    var latestWin = boards
        .OrderByDescending(item => item.WinningCalls.Count())
        .First();
    
    Console.WriteLine( "");
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    
    Console.WriteLine($" │ Latest winning call: {latestWin.WinningCalls.Last()}");
    Console.WriteLine($" | Calls to win: {string.Join(", ", latestWin.WinningCalls)}");
    Console.WriteLine( " │ Latest winning board: ");
    var latestWinningBoardLines = latestWin.Board.ToString().Split("\n");
    foreach (var w in latestWinningBoardLines)
    {
        Console.WriteLine($" │    {w}");
    }
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Latest Winning board score: { latestWin.Board.Score(latestWin.WinningCalls) }");
    Console.WriteLine($" │ Latest Winning board score * last call: {latestWin.Board.Score(latestWin.WinningCalls) * latestWin.WinningCalls.Last() }");

    Console.WriteLine($" └────────────");
}

Console.WriteLine("Part 1: ");
await Part1("test_input.txt");
await Part1("input.txt");

Console.WriteLine("Part 2: ");
await Part2("test_input.txt");
await Part2("input.txt");

class BingoEntry
{
    public static List<BingoEntry> FromInputLine(string line)
    {
        var entries = line.Split(" ").Where(s => !string.IsNullOrWhiteSpace(s));
        return entries.Select(e => new BingoEntry { Value =  Helpers.SafeParseInt(e) }).ToList();
    }

    public int Value { get; init; }
    
    public bool IsMarked(IEnumerable<int> calls)
    {
        return calls.Contains(Value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}

class BingoBoard
{
    public int NumColumns { get; }
    public int NumRows { get; }

    private readonly List<List<BingoEntry>> _columns = new List<List<BingoEntry>>();
    private readonly List<List<BingoEntry>> _rows = new List<List<BingoEntry>>();

    public IEnumerable<IEnumerable<BingoEntry>> Rows => _rows.Select(r => r.ToArray());
    public IEnumerable<IEnumerable<BingoEntry>> Columns => _columns.Select(c => c.ToArray());

    private BingoBoard()
    {
    }

    public BingoBoard(IEnumerable<string> dataLines)
    {
        dataLines = dataLines.Where(s => !string.IsNullOrWhiteSpace(s));
        if (!dataLines.Any()) { throw new InvalidOperationException("Cannot load an empty board"); }

        _rows = dataLines.Select(BingoEntry.FromInputLine).ToList();
        _columns = _rows.Transpose();

        NumColumns = _columns.Count;
        NumRows = _rows.Count;
    }

    public static bool IsWinner(IEnumerable<BingoEntry> rowOrColumn, IEnumerable<int> calls)
    {
        return rowOrColumn.All(e => e.IsMarked(calls));
    }

    public bool IsWinner(IEnumerable<int> calls)
    {
        return _rows.Any(r => IsWinner(r, calls)) || _columns.Any(c => IsWinner(c, calls));
    }

    public int Score(IEnumerable<int> calls)
    {
        var unmarked = _rows.SelectMany(r => r.Where(e => !e.IsMarked(calls)));
        return unmarked.Select(e => e.Value).Sum();
    }

    public override string ToString()
    {
        return _rows.MatrixToString();
    }
}