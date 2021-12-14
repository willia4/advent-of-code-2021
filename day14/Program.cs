using System.Text.RegularExpressions;
using CommonHelpers;
using Microsoft.VisualBasic;

uint ToLookup(byte b1, byte b2) => (((uint)b1) << 8) + b2;
    
(LinkedList<byte> initialForm, Dictionary<uint, byte> rules) ReadInput(string path)
{
    byte ToByte(string s)
    {
        return System.Text.Encoding.UTF8.GetBytes(s)[0];
    }

    var chunks = System.IO.File.ReadAllLines(path).ChunkBySeparator(Helpers.IsEmpty).ToArray();
    var initialForm = new LinkedList<byte>(chunks[0].First().Strings().Select(ToByte));

    var rules = chunks[1].Select(line =>
    {
        var m = Regex.Match(line, "^(..) -> (.)");
        if (!m.Success)
        {
            throw new InvalidOperationException($"Could not parse line {line}");
        }

        var from = ToLookup(ToByte(m.Groups[1].Value.Substring(0, 1)), ToByte(m.Groups[1].Value.Substring(1, 1)))  ;
        var to = ToByte(m.Groups[2].Value);

        return (from: from, to: to);
    }).ToDictionary(t => t.from, t => t.to);

    
    return (initialForm, rules);
}

LinkedList<byte> ProcessStep(LinkedList<byte> s, Dictionary<uint, byte> rules)
{
    var pairFirst = s.First!;
    var pairSecond = pairFirst.Next;
    
    while (pairSecond != null)
    {
        var lookup = ToLookup(pairFirst.Value, pairSecond.Value);
        if (rules.ContainsKey(lookup))
        {
            s.AddAfter(pairFirst, rules[lookup]);
        }

        pairFirst = pairSecond;
        pairSecond = pairSecond.Next;
    }

    return s;
}

(long processedLength, (long count, byte value) mostCommon, (long count, byte value) leastCommon) RunProgram(string path, int stepCount)
{
    var input = ReadInput(path);

    var processed = Enumerable.Range(0, stepCount).Aggregate(input.initialForm, (s, i) => ProcessStep(s, input.rules));

    var frequencyTable = new Dictionary<byte, long>();
    foreach (var s in processed)
    {
        frequencyTable[s] = frequencyTable.GetValueOrDefault(s, 0) + 1;
    }

    var mostCommon = frequencyTable.Aggregate((count: (long)-1, value: (byte) 0), (acc, kvp) => kvp.Value > acc.count ? (kvp.Value, kvp.Key) : acc);
    var leastCommon = frequencyTable.Aggregate((count: long.MaxValue, value: (byte) 0), (acc, kvp) => kvp.Value < acc.count ? (kvp.Value, kvp.Key) : acc);

    return (processed.Count, mostCommon, leastCommon);
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
