
using System.Collections.Concurrent;
using System.Collections.Immutable;

Rectangle ReadInput(string path)
{
    var regex = new System.Text.RegularExpressions.Regex
        (@"^target area: x=(?<x1>-?\d+?)\.\.(?<x2>-?\d+?), y=(?<y1>-?\d+?)\.\.(?<y2>-?\d+?)$");
    var match = regex.Match(System.IO.File.ReadAllText(path).Trim());
    
    if (!match.Success) { throw new InvalidOperationException(); }

    var x1 = int.Parse(match.Groups["x1"].Value);
    var x2 = int.Parse(match.Groups["x2"].Value);
    var y1 = int.Parse(match.Groups["y1"].Value);
    var y2 = int.Parse(match.Groups["y2"].Value);

    var xMin = Math.Min(x1, x2);
    var xMax = Math.Max(x1, x2);
    var yMin = Math.Min(y1, y2);
    var yMax = Math.Max(y1, y2);
    
    return new Rectangle(xMin, xMax, yMin, yMax);
}

TargetComparison CompareYToTarget(long y, Rectangle target)
{
    return CompareValueToRange(y, target.YMin, target.YMax);
}

TargetComparison CompareXToTarget(long x, Rectangle target)
{
    return CompareValueToRange(x, target.XMin, target.XMax);
}

TargetComparison CompareValueToRange(long value, long t1, long t2)
{
    var min = Math.Min(t1, t2);
    var max = Math.Max(t2, t2);

    if (value < min) return TargetComparison.LessThanRange;
    if (value > max) return TargetComparison.GreaterThanRange;
    return TargetComparison.InRange;
}

Shot Step(Shot s)
{
    var newPos = new Point(X: s.X + s.XVelocity, Y: s.Y + s.YVelocity);
    var newVelocity = new Point(
        X: s.XVelocity > 0 ? s.XVelocity - 1 : (s.XVelocity < 0 ? s.XVelocity + 1 : 0),
        Y: s.YVelocity - 1);
     
    return s with
    {
        Position = newPos,
        Velocity = newVelocity,
        HighestY = Math.Max(s.HighestY, newPos.Y)
    };
}

bool InTarget(Shot s)
{
    return CompareXToTarget(s.X, s.Target) == TargetComparison.InRange &&
           CompareYToTarget(s.Y, s.Target) == TargetComparison.InRange;
}

Shot? ShotIsSuccessful(Shot shot)
{
    do
    {
        if (InTarget(shot)) return shot;
        
        var next = Step(shot);
        if (next.XVelocity == 0)
        {
            if (CompareXToTarget(next.X, next.Target) != TargetComparison.InRange)
            {
                return null;
            }
        }

        if (CompareYToTarget(next.Y, next.Target) == TargetComparison.LessThanRange)
        {
            return null;
        }

        shot = next;
    } while (true);
}

IEnumerable<long> ReasonableXVelocities(Rectangle target)
{
    // we know we start at x = 0 and the x velocity will never reverse 
    // so if we build a field that has its x coordinates bounded by 0 and the target sides, that will be the 
    // only values it ever makes sense to fire x in 
    var coords = new long[] { 0, target.XMin, target.XMax };
    var min = coords.Min();
    var max = coords.Max();
    
    for (var i = min; i <= max; i++)
    {
        yield return i;
    }
}

IEnumerable<long> ReasonableYVelocities(Rectangle target)
{
    // there should be a clever way to figure out a reasonable range for y. 
    // I could not come up with it. 
    // +-1,000 gave the wrong answer. +-10,000 gave the right answer. 
    for (var i = -10000L; i <= 10000L; i++)
    {
        yield return i;
    }
}

Dictionary<Rectangle, IEnumerable < Shot >> _cache = new Dictionary<Rectangle, IEnumerable<Shot>>();
IEnumerable<Shot> FindSuccessfulShots(Rectangle target)
{
    if (!_cache.ContainsKey(target))
    {
        var hits = new ConcurrentBag<Shot>();

        var guesses = ReasonableXVelocities(target).SelectMany(x => ReasonableYVelocities(target).Select(y => (x, y)));

        Parallel.ForEach(guesses, guess =>
        {
            var (xVelocity, yVelocity) = guess;
            var success = ShotIsSuccessful(new Shot(xVelocity, yVelocity, target));
            if (success != null)
            {
                hits.Add(success);
            }
        });

        _cache[target] = hits.ToImmutableList();
    }

    return _cache[target];
}

void Part1(string path)
{
    var target = ReadInput(path);
    var successes = FindSuccessfulShots(target);
    var best = successes.MaxBy(s => s.HighestY)!;
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Best Y: { best.HighestY }");
    Console.WriteLine($" └────────────");
}

void Part2(string path)
{
    var target = ReadInput(path);
    var successes = FindSuccessfulShots(target);
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Total Successes: { successes.Count() }");
    Console.WriteLine($" └────────────");
}


Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");

public record Point(long X, long Y);

public record Shot
{
    public Shot(long xVelocity, long yVelocity, Rectangle target)
    {
        Position = new Point(0, 0);
        Velocity = new Point(xVelocity, yVelocity);
        HighestY = 0;
        Target = target;

        InitialPosition = Position;
        InitialVelocity = Velocity;
    }

    public Shot(Point position, Point velocity, long highestY, Rectangle target)
    {
        Position = position;
        Velocity = velocity;
        HighestY = highestY;
        Target = target;

        InitialPosition = Position;
        InitialVelocity = Velocity;
    }

    public Rectangle Target;
    public Point Position;
    public Point Velocity;
    public long HighestY;
    public Point InitialPosition;
    public Point InitialVelocity;
    
    public long X => Position.X;
    public long Y => Position.Y;
    public long XVelocity => Velocity.X;
    public long YVelocity => Velocity.Y;
}

public record Rectangle (long XMin, long XMax, long YMin, long YMax);

public enum TargetComparison
{
    LessThanRange,
    InRange,
    GreaterThanRange
}