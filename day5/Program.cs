using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommonHelpers;

List<List<int>> BuildBoard(IEnumerable<LineSegment> lineSegments)
{
    var minX = 0;
    var minY = 0;
    var maxX = 0;
    var maxY = 0;
    foreach (var l in lineSegments)
    {
        minX = Helpers.Min(minX, l.P1.X, l.P2.X);
        minY = Helpers.Min(minY, l.P1.Y, l.P2.Y);
        maxX = Helpers.Max(maxX, l.P1.X, l.P2.X);
        maxY = Helpers.Max(maxY, l.P1.Y, l.P2.Y);
    }
    
    if (minX < 0 || minY < 0) { throw new InvalidOperationException("Unexpected negative origin"); }

    var board = new List<List<int>>(); 
    foreach (var _ in Enumerable.Range(0, maxY + 1)) // the coordinates are zero based so we need to add one to get a count
    {
        board.Add(new List<int>(Enumerable.Repeat(0, maxX + 1))); // the coordinates are zero based so we need to add one to get a count
    }

    foreach (var p in lineSegments.SelectMany(l => l.Points()))
    {
        // row-order so Y comes first
        board[p.Y][p.X]++;
    }

    return board;
}

async Task Part1(string path)
{
    var input = await System.IO.File.ReadAllLinesAsync(path);
    var lineSegments = input.Select(LineSegment.FromLineInput);

    lineSegments = lineSegments.Where(l => !l.IsDiagonal);
    var board = BuildBoard(lineSegments);
    var multipleOverlappingPoints = board.SelectMany(row => row).Count(c => c >= 2);
    
    Console.WriteLine( "");
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    if (board.Count < 20)
    {
        Console.WriteLine(" │ ");
        var boardString = board.MatrixToString("", (c => c == 0 ? "." : c.ToString()));
        foreach (var l in boardString.Lines())
        {
            Console.WriteLine($" │ {l}");
        }
    }

    Console.WriteLine( " │ ");
    Console.WriteLine($" | Multiple overlaps at {multipleOverlappingPoints} points");
    Console.WriteLine($" └────────────");
}

async Task Part2(string path)
{
    var input = await System.IO.File.ReadAllLinesAsync(path);
    var lineSegments = input.Select(LineSegment.FromLineInput);
    
    var board = BuildBoard(lineSegments);
    var multipleOverlappingPoints = board.SelectMany(row => row).Count(c => c >= 2);
    
    Console.WriteLine( "");
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    if (board.Count < 20)
    {
        Console.WriteLine(" │ ");
        var boardString = board.MatrixToString("", (c => c == 0 ? "." : c.ToString()));
        foreach (var l in boardString.Lines())
        {
            Console.WriteLine($" │ {l}");
        }
    }

    Console.WriteLine( " │ ");
    Console.WriteLine($" | Multiple overlaps at {multipleOverlappingPoints} points");
    Console.WriteLine($" └────────────");
}

await Part1("test_input.txt");
await Part1("input.txt");

await Part2("test_input.txt");
await Part2("input.txt");

public class LineSegment
{
    public Point P1 { get; }
    public Point P2 { get; }

    public int X1 => P1.X;
    public int X2 => P2.X;
    public int Y1 => P1.Y;
    public int Y2 => P2.Y;
    
    private static readonly Regex __regex = new Regex("^(?<x1>\\d+?),(?<y1>\\d+?) -> (?<x2>\\d+?),(?<y2>\\d+?)$"); 
    public static LineSegment FromLineInput(string input)
    {
        var match = __regex.Match(input.Trim());
        if (!match.Success)
        {
            throw new InvalidOperationException($"Could not parse line \"{input}\"");
        }

        return new LineSegment(
            x1: Helpers.SafeParseInt(match.Groups["x1"].Value),
            y1: Helpers.SafeParseInt(match.Groups["y1"].Value),
            x2: Helpers.SafeParseInt(match.Groups["x2"].Value),
            y2: Helpers.SafeParseInt(match.Groups["y2"].Value));
    }
    
    private LineSegment(int x1, int y1, int x2, int y2)
    {
        P1 = new Point(x1, y1);
        P2 = new Point(x2, y2);
    }
    
    public bool IsDiagonal => P1.X != P2.X && P1.Y != P2.Y;

    public IEnumerable<Point> Points()
    {
        int getDirection(int currentCoordinate, int finalCoordinate)
        {
            if (currentCoordinate < finalCoordinate) { return 1; }
            if (currentCoordinate > finalCoordinate) { return -1; }

            return 0;
        }

        var current = P1;
        while (current != P2)
        {
            yield return current;

            current = new Point(current.X + getDirection(current.X, P2.X), current.Y + getDirection(current.Y, P2.Y));
        }

        yield return P2;
    }

    public override string ToString()
    {
        return $"Line Segment {P1.X},{P1.Y} -> {P2.X},{P2.Y}";
    }
}

public record Point(int X, int Y);
