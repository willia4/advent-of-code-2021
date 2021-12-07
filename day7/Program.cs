
using CommonHelpers;

IEnumerable<int> ReadInputs(string path)
{
    return File
        .ReadAllText(path)
        .Trim()
        .Split(",")
        .Select(Helpers.Trim)
        .Select(Helpers.SafeParseInt);
}

IEnumerable<int> FindFuelCostsForMovingToPosition(IEnumerable<int> currentPositions, int desiredPosition, Func<int, int, int> calculateFuelCost)
{
    foreach (var p in currentPositions)
    {
        yield return calculateFuelCost(p, desiredPosition);
    }
}

(int position, int totalFuelCosts) MinimizeFuelCosts(IEnumerable<int> currentPositions, Func<int, int, int> calculateFuelCost)
{
    var max = currentPositions.Max();
    var min = currentPositions.Min();

    var winningPosition = int.MaxValue;
    var winningFuelCost = int.MaxValue;
    
    for (var testPosition = min; testPosition <= max; testPosition++)
    {
        var fuel = FindFuelCostsForMovingToPosition(
            currentPositions, 
            testPosition, 
            calculateFuelCost
        ).Sum();

        if (fuel < winningFuelCost)
        {
            winningPosition = testPosition;
            winningFuelCost = fuel;
        }
    }

    return (winningPosition, winningFuelCost);
}

void Part1(string path)
{
    var positions = ReadInputs(path);

    var (winningPosition, winningFuelCost) = MinimizeFuelCosts(positions, (current, desired) => Math.Abs(current - desired));
    
    Console.WriteLine( "");
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" | Winning position: {winningPosition}");
    Console.WriteLine($" | Winning fuel cost: {winningFuelCost}");
    Console.WriteLine($" └────────────");
}

void Part2(string path)
{
    var positions = ReadInputs(path);

    var (winningPosition, winningFuelCost) = MinimizeFuelCosts(positions, (current, desired) =>
    {
        double delta = Math.Abs(current - desired);
        return (int) Math.Ceiling((delta * (delta + 1)) / 2);
    });
    
    Console.WriteLine( "");
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" | Winning position: {winningPosition}");
    Console.WriteLine($" | Winning fuel cost: {winningFuelCost}");
    Console.WriteLine($" └────────────");
}

Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");