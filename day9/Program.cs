using System.Collections.Immutable;
using CommonHelpers;

IList<IList<int>> ReadInput(string path)
{
    return ImmutableList<IList<int>>.Empty.AddRange(
        File.ReadAllLines(path)
            .Select(line => ImmutableList<int>.Empty.AddRange(
                line
                .Strings()
                .Select(Helpers.SafeParseInt))));
    
}

IEnumerable<(int value, int x, int y)> AdjacentPoints(IList<IList<int>> data, int x, int y)
{
    if ((y - 1) >= 0)
    {
        yield return (data[y - 1][x], x, y - 1);
    }

    if ((y + 1) < data.Count)
    {
        yield return (data[y + 1][x], x, y + 1);
    }

    if ((x - 1) >= 0)
    {
        yield return (data[y][x - 1], x - 1, y);
    }

    if ((x + 1) < (data[y].Count))
    {
        yield return (data[y][x + 1], x + 1, y);
    }
}

IEnumerable<(int value, int x, int y)> FindLowPoints(IList<IList<int>> data)
{
    for (var y = 0; y < data.Count; y++)
    {
        for (var x = 0; x < data[y].Count; x++)
        {
            var currentValue = data[y][x];
            if (AdjacentPoints(data, x, y).All(adj => adj.value > currentValue))
            {
                yield return (currentValue, x, y);
            }
        }
    }
}

IEnumerable<(int value, int x, int y)> FindBasinForLowPoint(IList<IList<int>> data, int x, int y)
{
    var visited = new HashSet<(int value, int x, int y)>();
    var toVisit = new Stack<(int value, int x, int y)>();
    toVisit.Push((data[y][x], x, y));

    while (toVisit.Count > 0)
    {
        var next = toVisit.Pop();
        if (!visited.Add(next)) { continue; }

        yield return next;

        var adjacents = AdjacentPoints(data, next.x, next.y).Where(adj => adj.value != 9);

        foreach (var adj in adjacents) { toVisit.Push(adj); }
    }
}

IEnumerable<IEnumerable<(int value, int x, int y)>> FindBasins(IList<IList<int>> data)
{
    var lowPoints = FindLowPoints(data);
    return lowPoints.Select(lp => FindBasinForLowPoint(data, lp.x, lp.y));
}

void Part1(string path)
{
    var data = ReadInput(path);

    var riskLevels = FindLowPoints(data).Select(pt => pt.value + 1);
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Sum of risk levels {riskLevels.Sum()}");
    Console.WriteLine($" └────────────");
}

void Part2(string path)
{
    var data = ReadInput(path);
    var basins = FindBasins(data);

    var biggestBasins = basins.OrderByDescending(b => b.Count()).Take(3);
    var biggestSizes = biggestBasins.Select(b => b.Count());

    var productOfSizes = biggestSizes.Aggregate(1, (acc, next) => acc * next);

    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Found Basins: {basins.Count()}");
    Console.WriteLine($" │ Multiplication of top 3 basins {productOfSizes}");
    Console.WriteLine($" └────────────");
}

Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");