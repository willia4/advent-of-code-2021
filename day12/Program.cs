using System.Collections.Immutable;
using Graph = System.Collections.Generic.IEnumerable<(string a, string b)>;
using Path = System.Collections.Generic.IEnumerable<string>;

const string StartNode = "start";
const string EndNode = "end";

Graph ReadInput(string path)
{
    return ImmutableArray<(string a, string b)>.Empty.AddRange(
        System.IO.File.ReadAllLines(path)
            .Select(line =>
            {
                var nodes = line.Split("-");
                return (nodes[0], nodes[1]);
            }));
}

IEnumerable<string> Nodes(Graph g) => g.SelectMany(e => new string[] {e.a, e.b}).Distinct();

IEnumerable<string> ConnectionsToNode(Graph g, string n)
{
    var aConnections = g.Where(e => e.a == n).Select(e => e.b);
    var bConnections = g.Where(e => e.b == n).Select(e => e.a);
    return aConnections.Concat(bConnections).Distinct();
}

IEnumerable<Path> FindPaths(Graph g, HashSet<string> mayRevisit, HashSet<string>? mayRevisitOnce = null)
{
    mayRevisitOnce ??= new HashSet<string>();

    bool IsAllowed(Path currentPath, string currentNode)
    {
        if (!currentPath.Contains(currentNode)) { return true; } // if we haven't visited this node, it's fair game
        if (mayRevisit.Contains(currentNode)) { return true; } // if we're allowed to visit as many times as we'd like, go for it
        
        // if we can't revisit as many times as we like and we can't even revisit once, deny
        if (!mayRevisitOnce.Contains(currentNode)) { return false; }
        
        // we've visited this node before and we know we can only revisit it once. This will only be allowed if it's our second 
        // visit which means that the current path can only contain a single previous visit to allow it
        return currentPath.Count(n => n == currentNode) == 1;
    }

    IEnumerable<Path> Visit(List<Path> allPaths, Path currentPath, string currentNode)
    {
        // we're not allowed to visit this node. Don't add it to the current path, but make sure the current path is 
        // included in our total list of paths and return.
        if (!IsAllowed(currentPath, currentNode))
        {
            allPaths.Add(currentPath);
            return allPaths;
        }

        currentPath = currentPath.Append(currentNode);
        if (currentNode == EndNode)
        {
            allPaths.Add(currentPath);
            return allPaths;
        }

        foreach (var n in ConnectionsToNode(g, currentNode))
        {
            Visit(allPaths, currentPath, n);
        }

        return allPaths;
    }

    return 
        Visit(new List<Path>(), Enumerable.Empty<string>(), StartNode)
            .Where(p => p.Last() == EndNode)
            .Distinct(new PathComparer());
}

void Part1(string path)
{
    var g = ReadInput(path);

    var bigNodes = Nodes(g).Where(n => n == n.ToUpperInvariant());
    var mayRevist = new HashSet<string>(bigNodes);

    var paths = FindPaths(g, mayRevist).ToList();
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Paths from start to end: { paths.Count }");
    Console.WriteLine($" └────────────");
}

void Part2(string path)
{
    var g = ReadInput(path);

    var bigNodes = Nodes(g).Where(n => n == n.ToUpperInvariant());
    var smallNodes = Nodes(g).Where(n => n != n.ToUpperInvariant())
                                            .Except(new string[] { StartNode, EndNode });

    var mayAlwaysRevisit = new HashSet<string>(bigNodes);
    var paths = smallNodes.SelectMany(singleSmallNode =>
        {
            var mayRevisitOnce = new HashSet<string>(new string[] { singleSmallNode });
            return FindPaths(g, mayAlwaysRevisit, mayRevisitOnce).Distinct(new PathComparer());
        })
        .Distinct(new PathComparer())
        .ToList();

    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Paths from start to end: { paths.Count }");
    Console.WriteLine($" └────────────");
}

Part1("test_input1.txt");
Part1("test_input2.txt");
Part1("test_input3.txt");
Part1("input.txt");

Part2("test_input1.txt");
Part2("test_input2.txt");
Part2("test_input3.txt");
Part2("input.txt");

public class PathComparer : IEqualityComparer<IEnumerable<string>>
{
    public bool Equals(IEnumerable<string>? x, IEnumerable<string>? y)
    {
        if (x == null && y == null) { return true; }
        if (x == null || y == null) { return false; }

        return x.Zip(y).All(t => t.First == t.Second);
    }

    public int GetHashCode(IEnumerable<string> obj)
    {
        return obj.Aggregate(new HashCode(), (acc, next) =>
        {
            acc.Add(next);
            return acc;
        }).ToHashCode();
    }
}