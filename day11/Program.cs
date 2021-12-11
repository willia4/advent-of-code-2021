using CommonHelpers;

List<List<int>> ReadInput(string path)
{
    return System.IO.File.ReadAllLines(path)
        .Select(
            line => line.Strings().Select(Helpers.SafeParseInt).ToList())
        .ToList();
}

IEnumerable<Point> AdjacentPoints(List<List<int>> data, Point p)
{
    var minX = 0;
    var minY = 0;
    var maxX = data[0].Count - 1;
    var maxY = data.Count - 1;

    bool IsValid(Point p) => p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY;

    var possibilities = new Point[]
    {
        p with { X = p.X - 1 },
        p with { X = p.X - 1, Y = p.Y - 1 },
        p with { Y = p.Y - 1 },
        p with { X = p.X + 1, Y = p.Y - 1 },
        p with { X = p.X + 1 },
        p with { X = p.X + 1, Y = p.Y + 1 },
        p with { Y = p.Y + 1 },
        p with { X = p.X - 1, Y = p.Y + 1 },
    };
    
    return possibilities.Where(IsValid).ToArray();
}

long Step(List<List<int>> data)
{
    var rowCount = data.Count;
    var colCount = data[0].Count;

    var mustFlash = false;
    long flashCount = 0;

    for (var y = 0; y < rowCount; y++)
    {
        for (var x = 0; x < colCount; x++)
        {
            var v = data[y][x];
            v++;
            mustFlash = mustFlash || v > 9;
            data[y][x] = v;
        }

    }

    while (mustFlash)
    {
        mustFlash = false;

        for (var y = 0; y < rowCount; y++)
        {
            for (var x = 0; x < colCount; x++)
            {
                var v = data[y][x];
                if (v > 9)
                {
                    data[y][x] = 0;
                    flashCount++;

                    foreach (var adj in AdjacentPoints(data, new Point(x, y)))
                    {
                        var o = data[adj.Y][adj.X];
                        if (o == 0) { continue; } // if o has already flashed, stop

                        o++;
                        mustFlash = mustFlash || o > 9;
                        data[adj.Y][adj.X] = o;
                    }
                }
            }
        }
    }

    return flashCount;
}

var IntFormatter = (int v) =>
{
    if (v == 0)
    {
        return $"\x1b[97m{v}\x1b[0m";
    }

    return $"\x1b[90m{v}\x1b[0m";
};

void Part1(string path)
{
    var data = ReadInput(path);
    long flashCount = 0;
    for (var i = 0; i < 100; i++)
    {
        flashCount += Step(data);
    }
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Flash Count: { flashCount }");
    Console.WriteLine($" └────────────");
}

void Part2(string path)
{
    var data = ReadInput(path);
    var step = 0;
    while (true)
    {
        step++;
        Step(data);
        if (data.SelectMany(r => r).All(i => i == 0)){ break; }
    }
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Synchronized Step: { step }");
    Console.WriteLine($" └────────────");
}

Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");

public record Point( int X, int Y );