using CommonHelpers;
using System.Collections.Immutable;
using System.Security;

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
    (int x, int y) startNode = (0, 0);
    (int x, int y) endNode = (m[0].Length - 1, m.Length - 1);

    var openSet = new PriorityQueue<(int x, int y), int>();
    var openSetHash = new HashSet<(int x, int y)>();

    var cameFrom = new Dictionary<(int x, int y), (int x, int y)>();
    var gScore = new Dictionary<(int x, int y), int>();

    foreach (var p in m.AllCoordinates())
    {
        gScore[p] = Int32.MaxValue;
    }

    openSet.Enqueue(startNode, m[startNode.y][startNode.x]);
    openSetHash.Add(startNode);

    gScore[startNode] = m[startNode.y][startNode.x];

    IList<(int x, int y)> ReconstructPath((int x, int y) current)
    {
        var path = new List<(int x, int y)> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
    
    while (openSet.Count > 0)
    {
        var current = openSet.Dequeue();
        if (current == endNode)
        {
            return ReconstructPath(current);
        }

        foreach (var neighbor in Neighbors(m, current))
        {
            var tentative_gScore = gScore[current] + m[neighbor.y][neighbor.x];
            if (tentative_gScore < gScore[neighbor])
            {
                cameFrom[neighbor] = current;
                gScore[neighbor] = tentative_gScore;

                if (!openSetHash.Contains(neighbor))
                {
                    openSetHash.Add(neighbor);
                    openSet.Enqueue(neighbor, gScore[neighbor]);
                }
            }
        }
    }

    throw new InvalidOperationException("Could not construct path");
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
Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");

