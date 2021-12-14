using System.Text.RegularExpressions;
using CommonHelpers;

uint ToLookup(char b1, char b2) => (((uint)b1) << 8) + b2;
    
(Form initialForm, IEnumerable<Rule> rules) ReadInput(string path)
{
    StringPair ToStringPair(string s)
    {
        return new StringPair(s.Substring(0, 1), s.Substring(1, 1));
    }

    var chunks = System.IO.File.ReadAllLines(path).ChunkBySeparator(Helpers.IsEmpty).ToArray();
    var initialFormPairs = chunks[0].First().Strings().AsSlidingWindow(2).Select(pair => string.Join("", pair));
    var initialFormCounts = 
        initialFormPairs
            .GroupBy(pair => pair)
            .ToDictionary(
                pair => ToStringPair(pair.Key), 
                pair => (long) pair.Count());

    var rules = chunks[1].Select(line =>
    {
        var m = Regex.Match(line, "^(.)(.) -> (.)");
        if (!m.Success)
        {
            throw new InvalidOperationException($"Could not parse line {line}");
        }

        return new Rule(new StringPair(m.Groups[1].Value, m.Groups[2].Value), m.Groups[3].Value);
    });

    var firstFormPair = ToStringPair(initialFormPairs.First());
    return (new Form(firstFormPair.Left, initialFormCounts), rules);
}

Form ProcessStep(Form currentForm, IEnumerable<Rule> rules)
{
    var output = currentForm.Counts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    
    foreach (var rule in rules.Where(r => currentForm.Counts.GetValueOrDefault(r.From) > 0))
    {
        var pairsInCurrentForm = currentForm.Counts.GetValueOrDefault(rule.From);
        output[rule.From] = output.GetValueOrDefault(rule.From) - pairsInCurrentForm;

        var leftNewPair = new StringPair(rule.From.Left, rule.To);
        var rightNewPair = new StringPair(rule.To, rule.From.Right);
        
        output[leftNewPair] = (output.GetValueOrDefault(leftNewPair, 0) + pairsInCurrentForm);
        output[rightNewPair] = (output.GetValueOrDefault(rightNewPair, 0) + pairsInCurrentForm);
    }

    return new Form(currentForm.FirstCharacter, output);
}

(long processedLength, (long count, string value) mostCommon, (long count, string value) leastCommon) RunProgram(string path, int stepCount)
{
    var input = ReadInput(path);

    var processed = Enumerable.Range(0, stepCount).Aggregate(input.initialForm, (s, i) => ProcessStep(s, input.rules));

    long processedLength = 0;
    var frequencyTable = new Dictionary<string, long>();
    foreach (var s in processed.Counts)
    {
        var pair = s.Key;
        var count = s.Value;
        
        frequencyTable[pair.Right] = frequencyTable.GetValueOrDefault(pair.Right, 0) + count;

        processedLength += count;
    }

    frequencyTable[processed.FirstCharacter] = frequencyTable.GetValueOrDefault(processed.FirstCharacter, 0) + 1;

    var mostCommon = frequencyTable.Aggregate((count: (long)-1, value: ""), (acc, kvp) => kvp.Value > acc.count ? (kvp.Value, kvp.Key) : acc);
    var leastCommon = frequencyTable.Aggregate((count: long.MaxValue, value: ""), (acc, kvp) => kvp.Value < acc.count ? (kvp.Value, kvp.Key) : acc);

    return (processedLength + 1, mostCommon, leastCommon);
}

void Part1(string path)
{

    var (processedLength, mostCommon, leastCommon) = RunProgram(path, 10);

    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Processed Length: { processedLength }");
    Console.WriteLine($" │ Most common: ({mostCommon.value}, {mostCommon.count})");
    Console.WriteLine($" │ Least common: ({leastCommon.value}, {leastCommon.count})");
    Console.WriteLine($" │ Answer: {mostCommon.count - leastCommon.count}");
    Console.WriteLine($" └────────────");
    
}

void Part2(string path)
{

    var (processedLength, mostCommon, leastCommon) = RunProgram(path, 40);

    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Processed Length: { processedLength }");
    Console.WriteLine($" │ Most common: ({mostCommon.value}, {mostCommon.count})");
    Console.WriteLine($" │ Least common: ({leastCommon.value}, {leastCommon.count})");
    Console.WriteLine($" │ Answer: {mostCommon.count - leastCommon.count}");
    Console.WriteLine($" └────────────");
    
}

Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");

public record Rule (StringPair From, string To);
public record StringPair(string Left, string Right);
public record Form(string FirstCharacter, Dictionary<StringPair, long> Counts);