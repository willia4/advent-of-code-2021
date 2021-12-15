using CommonHelpers;
using System.Collections.Immutable;

int[][] ReadInput(string path)
{
    var o = File.ReadAllLines(path)
                                        .Select(line => line.Strings().Select(Helpers.SafeParseInt).ToArray()).ToArray();

    return o;
}

T[][] MakeEmptyArray<T>(int height, int width)
{
    return Enumerable.Range(0, height).Select(_ => Enumerable.Range(0, width).Select(_ => default(T)).ToArray()).ToArray();
}

T[][] TileMatrix<T>(T[][] tile, int tilesWidth, int tilesHeight, Func<T, T> valueTransform)
{
    var map = MakeEmptyArray<T>(tile.Length * 5, tile[0].Length * 5);
    
    void CopyTileToMap(T[][] tile, int tileX, int tileY, Func<T, T> valueTransform)
    {
        var tileHeight = tile.Length;
        var tileWidth = tile[0].Length;
        
        for (var y = 0; y < tileHeight; y++)
        {
            for (var x = 0; x < tileWidth; x++)
            {
                map[(tileY * tileHeight) + y][(tileX * tileWidth) + x] = valueTransform(tile[y][x]);
            }
        }
    }

    T[][] ExtractTile(int tileX, int tileY)
    {
        var tileHeight = tile.Length;
        var tileWidth = tile[0].Length;

        var r = MakeEmptyArray<T>(tileHeight, tileWidth);
        for (var y = 0; y < tileHeight; y++)
        {
            for (var x = 0; x < tileWidth; x++)
            {
                r[y][x] = map[(tileY * tileHeight) + y][(tileX * tileWidth) + x];
            }
        }

        return r;
    }

    CopyTileToMap(tile, 0, 0, Helpers.Identity);

    for (int x = 1; x < tilesWidth; x++)
    {
        CopyTileToMap(ExtractTile(x - 1, 0), x, 0, valueTransform);
    }
    
    for (int y = 1; y < tilesHeight; y++)
    {
        CopyTileToMap(ExtractTile(0, y - 1), 0, y, valueTransform);
        for (int x = 1; x < tilesWidth; x++)
        {
            CopyTileToMap(ExtractTile(x - 1, y), x, y, valueTransform);
        }
    }
    
    // CopyTileToMap(ExtractTile(0, 0), 1, 0, valueTransform);
    // CopyTileToMap(ExtractTile(1, 0), 2, 0, valueTransform);
    // CopyTileToMap(ExtractTile(2, 0), 3, 0, valueTransform);
    // CopyTileToMap(ExtractTile(3, 0), 4, 0, valueTransform);
    return map;
}

IEnumerable<(int x, int y)> Neighbors<T>(T[][] m, (int x, int y) pt)
{
    bool IsValid((int x, int y) pt_)
    {
        var (x, y) = pt_;
        return (y >= 0 && y < m.Length) && (x >= 0 && x < m[y].Length);
    }

    return (new (int x, int y)[]
    {
        (pt.x - 1, pt.y), (pt.x, pt.y -1), (pt.x + 1, pt.y), (pt.x, pt.y + 1)
    }).Where(IsValid).ToArray();
}

IList<(int x, int y)> FindPath(int[][] m)
{
    var allPoints = Enumerable.Range(0, m.Length).SelectMany(y => Enumerable.Range(0, m[0].Length).Select(x => (x, y))).ToArray();

    var visited = new HashSet<(int x, int y)>();
    var distances = Enumerable.Range(0, m.Length).Select(_ => Enumerable.Range(0, m[0].Length).Select(_ => int.MaxValue).ToArray()).ToArray();
    distances[0][0] = m[0][0];

    IEnumerable<(int x, int y)> UnvisitedNodes()
    {
        return allPoints.Where(pt => !visited.Contains(pt));
    }

    IEnumerable<(int x, int y, int d)> NodeDistances(IEnumerable<(int x, int y)> nodes)
    {
        return nodes.Select(n => (n.x, n.y, distances[n.y][n.x])).ToArray();
    }
    
    IEnumerable<(int x, int y)> UnvisitedNeighbors((int x, int y) pt)
    {
        return Neighbors(m, pt).Where(pt => !visited.Contains(pt));
    }

    (int x, int y) startNode = (0, 0);
    (int x, int y) endNode = (m[0].Length - 1, m.Length - 1);

    var currentNode = startNode;
    while (currentNode != endNode)
    {
        var nodes = UnvisitedNeighbors(currentNode);
        var currentNodeDistance = distances[currentNode.y][currentNode.x];
        foreach (var n in nodes)
        {
            distances[n.y][n.x] = Math.Min(distances[n.y][n.x], currentNodeDistance + m[n.y][n.x]);
        }

        visited.Add(currentNode);
        var next = NodeDistances(UnvisitedNodes()).OrderBy(n => n.d).First();
        currentNode = (next.x, next.y);
    }

    var reversePath = new List<(int x, int y)>();
    currentNode = endNode;
    while (currentNode != startNode)
    {
        reversePath.Add(currentNode);
        var neighbors = NodeDistances(Neighbors(distances, currentNode));
        currentNode = neighbors.OrderBy(n => n.d).Select(n => (n.x, n.y)).First();
    }

    reversePath.Add(startNode);
    reversePath.Reverse();
    return reversePath;
}

void PrintMatrixAndPath(int[][] matrix, IList<(int x, int y)> path)
{
    var s = matrix.MatrixToString("", (x, y, v) =>
    {
        if (path.Contains((x, y)))
        {
            return $"\x1b[97m{v}\x1b[0m";
        }
        else
        {
            return $"\x1b[90m{v}\x1b[0m";
        }
    });
    
    Console.WriteLine(s);
}

void Part1(string path)
{
    var m = ReadInput(path);
    var shortestPath = FindPath(m);
    var totalRisk = shortestPath.Skip(1).Select(pt => m[pt.y][pt.x]).Sum();

    if (path.Contains("test_input.txt"))
    {
        PrintMatrixAndPath(m, shortestPath);
    }
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Shortest Path Length: { shortestPath.Count }");
    Console.WriteLine($" │ Total Risk: {totalRisk}");
    Console.WriteLine($" └────────────");
    
}

void Part2(string path)
{
    var m = ReadInput(path);
    var fullMap = TileMatrix(m, 5, 5, (v) => v >= 9 ? 1 : v + 1);

    var shortestPath = FindPath(fullMap);
    var totalRisk = shortestPath.Skip(1).Select(pt => fullMap[pt.y][pt.x]).Sum();


    if (path.Contains("test_input.txt"))
    {
        PrintMatrixAndPath(fullMap, shortestPath);
    }

    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Shortest Path Length: { shortestPath.Count }");
    Console.WriteLine($" │ Total Risk: {totalRisk}");
    Console.WriteLine($" └────────────");
}
// Part1("test_input.txt");
// Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");

