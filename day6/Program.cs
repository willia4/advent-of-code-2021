
using CommonHelpers;

(int numberOfDays, int totalFish) SimulateBadly(IEnumerable<int> initialSchool, int numberOfDays)
{
    
    var school = initialSchool.ToList();
    
    for (var day = 0; day < numberOfDays; day++)
    {
        var schoolLength = school.Count;
        for (var i = 0; i < schoolLength; i++)
        {
            var fish = school[i] - 1;
            if (fish < 0)
            {
                fish = 6;
                school.Add(8);
            }

            school[i] = fish;
        }
    }

    return (numberOfDays, school.Count);
}

// thanks to https://www.reddit.com/r/adventofcode/comments/r9z49j/2021_day_6_solutions/hng9bcz/
(int numberOfDays, UInt64 totalFish) SimulateIntelligently(IEnumerable<int> initialSchool, int numberOfDays)
{
    // timers go from 0 to 8
    var timerToFishCounts = new Dictionary<int, UInt64>()
    {
        { 0, 0 },
        { 1, 0 },
        { 2, 0 },
        { 3, 0 },
        { 4, 0 },
        { 5, 0 },
        { 6, 0 },
        { 7, 0 },
        { 8, 0 }
    };

    foreach (var timer in initialSchool)
    {
        timerToFishCounts[timer]++;
    }
    
    // to avoid a bunch of allocations, make a single new temp dictionary; we will reuse this throughout
    var tempMap = new Dictionary<int, UInt64>()
    {
        { 0, 0 },
        { 1, 0 },
        { 2, 0 },
        { 3, 0 },
        { 4, 0 },
        { 5, 0 },
        { 6, 0 },
        { 7, 0 },
        { 8, 0 }
    };
    
    for (var day = 0; day < numberOfDays; day++)
    {
        Helpers.ClearDictionaryValuesInPlace(tempMap);
        for (int i = 1; i <= 8; i++)
        {
            tempMap[i - 1] = timerToFishCounts[i];
        }

        // all timers at 0 spawn new fish at 8
        tempMap[8] = timerToFishCounts[0];
        tempMap[6] = tempMap[6] + timerToFishCounts[0]; // all timers at one get added to timers at 6
        
        Helpers.ReplaceDictionaryKeys(timerToFishCounts, tempMap);
    }

    var count = timerToFishCounts.Values.Sum();
    return (numberOfDays, count);
}

void Part1(string path)
{
    var input = System.IO.File.ReadAllText(path)
        .Split(",")
        .Select(Helpers.Trim)
        .Where(Helpers.NotEmpty)
        .Select(Helpers.SafeParseInt);

    var dumbResults = SimulateBadly(input, 80);
    var goodResults = SimulateIntelligently(input, 80);
    
    Console.WriteLine( "");
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" | Naive: Total fish after {dumbResults.numberOfDays} days: {dumbResults.totalFish}");
    Console.WriteLine($" | Clever: Total fish after {goodResults.numberOfDays} days: {goodResults.totalFish}");
    Console.WriteLine($" └────────────");
}

void Part2(string path)
{
    var input = System.IO.File.ReadAllText(path)
        .Split(",")
        .Select(Helpers.Trim)
        .Where(Helpers.NotEmpty)
        .Select(Helpers.SafeParseInt);

    var results = SimulateIntelligently(input, 256);
    
    Console.WriteLine( "");
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" | Total fish after {results.numberOfDays} days: {results.totalFish}");
    Console.WriteLine($" └────────────");
}

Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");