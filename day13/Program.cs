using System.Collections.Generic;
using System;
using System.Collections.Immutable;
using CommonHelpers;

InputRecord ReadInput(string path)
{
    var chunks = System.IO.File.ReadAllLines(path).ChunkBySeparator(Helpers.IsEmpty).ToList();

    var points = chunks[0].Select(line =>
    {
        var coords = line.Split(",");
        return new Point(Helpers.SafeParseInt(coords[0]), Helpers.SafeParseInt(coords[1]));
    });

    //var folders = Enumerable.Empty<Func<IList<IList<bool>>, IList<IList<bool>>>>();

    var folders = chunks[1].Select(line =>
    {
        line = line.Replace("fold along ", "");
        var instructions = line.Split("=");
        var coord = Helpers.SafeParseInt(instructions[1]);
        return instructions[0] == "x" ? BuildHorizontalFolder(coord) : BuildVerticalFolder(coord);
    });

    return new InputRecord(points, folders);
}

IList<IList<bool>> BuildMatrix(IEnumerable<Point> points)
{
    var pointsSet = new HashSet<Point>(points);
    var o = ImmutableArray<bool>.Empty;
    var (maxX, maxY) = points.Aggregate((0, 0), (acc, p) => 
        (Math.Max(acc.Item1, p.X), Math.Max(acc.Item2, p.Y)));

    var rows = Enumerable.Range(0, (maxY + 1)).Select(y =>
    {
        return ImmutableArray<bool>.Empty.AddRange(
            Enumerable.Range(0, (maxX + 1)).Select(
                x => pointsSet.Contains(new Point(x, y)))) as IList<bool>;
    });
    
    return ImmutableArray<IList<bool>>.Empty.AddRange(rows);
}

IList<IList<bool>> OverlayMatrixes(IList<IList<bool>> a, IList<IList<bool>> b)
{
    IEnumerable<bool> OverlayRows(IEnumerable<bool> a, IEnumerable<bool> b)
    {
        return a.Zip(b).Select(points => points.First || points.Second);
    }

    return a.Zip(b).Select(rows => OverlayRows(rows.First, rows.Second)).ToImmutableMatrix();
}

Func<IList<IList<bool>>, IList<IList<bool>>> BuildHorizontalFolder(int x)
{
    return (matrix =>
    {
        var transposed = matrix.Transpose().ToImmutableMatrix();
        var folder = BuildVerticalFolder(x);

        return folder(transposed).Transpose().ToImmutableMatrix();
    });
}

Func<IList<IList<bool>>, IList<IList<bool>>> BuildVerticalFolder(int y)
{
    return (matrix =>
    {
        if (y <= 1 || y >= (matrix.Count - 1)) return matrix;

        var top = matrix.Take(y).ToImmutableArray();
        var bottom = matrix.Skip(y + 1).ToImmutableArray();

        var flippedBottom = new IList<bool>[bottom.Length];

        for (var i = 0; i < bottom.Length / 2; i++)
        {
            var j = (bottom.Length - 1) - i;
            flippedBottom[i] = bottom[j].ToImmutableArray();
            flippedBottom[j] = bottom[i].ToImmutableArray();
        }

        if (bottom.Length % 2 != 0)
        {
            flippedBottom[bottom.Length / 2] = bottom[bottom.Length / 2];
        }

        return OverlayMatrixes(top, flippedBottom);
    });
}

string PrintMatrix(IEnumerable<IEnumerable<bool>> matrix)
{
    return matrix.MatrixToString("", (v => v ? "█" : " "));
}

void Part1(string path)
{
    var input = ReadInput(path);
    var matrix = BuildMatrix(input.Points);
    var result = input.Folders.First()(matrix);

    var pointCount = result.SelectMany(row => row).Count(b => b);
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Total starting points: { input.Points.Count() }");
    Console.WriteLine($" │ Total folding instructions: { input.Folders.Count() }");
    Console.WriteLine($" │ Points after 1 fold: { pointCount }");
    Console.WriteLine($" └────────────");
}

void Part2(string path)
{
    var input = ReadInput(path);
    var matrix = BuildMatrix(input.Points);

    var foldedMatrix = input.Folders.Aggregate(matrix, (acc, nextFolder) => nextFolder(acc)); 
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Total starting points: { input.Points.Count() }");
    Console.WriteLine($" │ Total folding instructions: { input.Folders.Count() }");
    Console.WriteLine( " │ ");

    foreach (var line in PrintMatrix(foldedMatrix).Lines())
    {
        Console.WriteLine($" │ {line}");
    }
    Console.WriteLine( " │ ");    
    Console.WriteLine($" └────────────");
}

Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");

public record Point(int X, int Y);
public record InputRecord (IEnumerable<Point> Points, IEnumerable<Func<IList<IList<bool>>, IList<IList<bool>>>> Folders);