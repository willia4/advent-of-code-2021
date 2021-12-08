using System.Collections.Immutable;
using CommonHelpers;

IEnumerable<DataRecord> ReadData(string path)
{
    var inputLines = System.IO.File.ReadAllLines(path);
    return inputLines.Select(line =>
    {
        var splits = line.Split("|").Select(Helpers.Trim).ToArray();
        var inputs = splits[0].Split().Select(Helpers.Trim).Select(Helpers.ToUpper).Select(SortLettersInString);
        var outputs = splits[1].Split().Select(Helpers.Trim).Select(Helpers.ToUpper).Select(SortLettersInString);
        
        return new DataRecord(Helpers.MakeImmutableList(inputs), Helpers.MakeImmutableList(outputs), line);
    });
}

string SortLettersInString(string s)
{
    return String.Join("", s.Strings().OrderBy(c => c));
}

Dictionary<int, List<string>> StandardDigits()
{
    return new Dictionary<int, List<string>>()
    {
        { 0, new List<string> { "a", "b", "c", "e", "f", "g" } },
        { 1, new List<string> { "c", "f" } },
        { 2, new List<string> { "a", "c", "d", "e", "g" } },
        { 3, new List<string> { "a", "c", "d", "f", "g" } },
        { 4, new List<string> { "b", "c", "d", "f" } },
        { 5, new List<string> { "a", "b", "d", "f", "g" } },
        { 6, new List<string> { "a", "b", "d", "e", "f", "g" } },
        { 7, new List<string> { "a", "c", "f" } },
        { 8, new List<string> { "a", "b", "c", "d", "e", "f", "g" } }, 
        { 9, new List<string> { "a", "b", "c", "d", "f", "g" } }
    };
}

int ScreenToDigit(string screen)
{
    var screenToDigit = StandardDigits().ToDictionary(
        kvp => string.Join("", kvp.Value.OrderBy(s => s)),
        kvp => kvp.Key);
    screen = string.Join("", screen.Strings().OrderBy(s => s));
    return screenToDigit[screen];
}

IEnumerable<int> DigitsByUniqueSegmentCount()
{
    var digits = StandardDigits();
    var counts = Enumerable.Range(1, 7)
        .ToDictionary(
            i => i,
            i => digits.Where(kvp => kvp.Value.Count == i).Select(kvp => kvp.Key).ToList()
        );

    return counts.Where(kvp => kvp.Value.Count == 1).Select(kvp => kvp.Value.First()).OrderBy(i => i).ToList();
}

void PrintDictionarySortedByValues<K, V>(Dictionary<K, V> d)
{
    Console.WriteLine("{");
    foreach (var kvp in d.AsEnumerable().OrderBy(kvp => kvp.Value))
    {
        Console.WriteLine($"   \"{kvp.Key}\": \"{kvp.Value}\"");
    }
    Console.WriteLine("}");
}

Dictionary<string, string> DecodeData(DataRecord data)
{
    var patterns = data.InputPatterns.Concat(data.OutputValues).Distinct().OrderBy(p => p.Length).ToList();

    var decoder = new Dictionary<string, string>();

    string? ReverseDecode(string decodedValue)
    {
        return decoder?.Where(kvp => kvp.Value == decodedValue).Select(kvp => kvp.Key).FirstOrDefault();
    }

    // the symbols for 1 contains two segments ("c" and "f"), the symbols for 7 contains those two segments and one other, "a". 
    // so we know that the symbol in 7 that is not in 1 will be "a"
    var oneSymbols = patterns.First(o => o.Length == 2).Strings().ToList();
    var sevenSymbols = patterns.First(o => o.Length == 3).Strings().ToList();

    decoder[(sevenSymbols.First(s => !oneSymbols.Contains(s)))] = "a";

    // the symbols for "4" (also the only one to contain four segments) has "b", "c", "d", and "f". 
    // Since "c" and "f" are in "1", we know that "b" and "d" are the symbols not in "1". 
    // The three six-count numbers ("0", "6", and "9") contain "b" and "f". ALL of them contain b. One of them does not contain d. 
    var fourSymbols = patterns.First(o => o.Length == 4).Strings().ToList();
    var sixers = patterns.Where(o => o.Length == 6).ToList();
    var bd = fourSymbols.Where(s => !oneSymbols.Contains(s)).ToList();
    
    decoder[bd.First(v => sixers.All(s => s.Strings().Contains(v)))] = "b";
    decoder[bd.First(v => !sixers.All(s => s.Strings().Contains(v)))] = "d";
    
    // there's one 5-symbol segment that contains both b and d. It's "5". 
    var fivers = patterns.Where(o => o.Length == 5).ToList();
    var encodedB = ReverseDecode("b") ?? throw new InvalidOperationException("Did not have b");
    var encodedD = ReverseDecode("d") ?? throw new InvalidOperationException("Did not have b");

    var fiveSymbols = fivers.First(f => f.Strings().Contains(encodedB) && f.Strings().Contains(encodedD)).Strings().ToList();
    
    // five has a, b, d, f, g. We know a, b, d. If we remove those, we're left with f and g. One contains f. 
    // So the letter that's in one that isn't in "f and g" must be "c". 
    // The letter that's in five that isn't in one must be "g". 
    var fg = fiveSymbols.Where(f => !decoder.Keys.Contains(f)).ToList();

    decoder[oneSymbols.First(s => !fg.Contains(s))] = "c";
    decoder[fg.First(s => !oneSymbols.Contains(s))] = "g";
    
    // since we know "c" now, the only symbol in one that we don't know must be "f". 
    decoder[oneSymbols.First(s => !decoder.Keys.Contains(s))] = "f";
    
    // We now know a,b,c,d,g,f. 
    // we now know 6 segments. The only segment we don't know must be e. 
    // The number 8 has all 7 segments so we can use it to get a list of all possible symbols for convenience 
    var eightSymbols = patterns.First(o => o.Length == 7).Strings().ToList();
    decoder[eightSymbols.First(s => !decoder.Keys.Contains(s))] = "e";

    return decoder;
}

int DecodeDigits(IEnumerable<string> screens, Dictionary<string, string> decoder)
{
    string DecodeString(string s)
    {
        return string.Join("", s.Strings().Select(e => decoder[e]));
    }

    screens = screens.Select(DecodeString);
    var digits = screens.Select(ScreenToDigit).ToList();
    var number = string.Join("", digits);
    return Helpers.SafeParseInt(number);
}

void Part1(string path)
{
    var data = ReadData(path);

    var uniqueDigits = DigitsByUniqueSegmentCount();
    var uniqueDigitLengths = uniqueDigits.Select(d => StandardDigits()[d].Count);

    var uniqueOutputs = data
        .SelectMany(i => i.OutputValues)
        .Where(v => uniqueDigitLengths.Contains(v.Length));
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Unique digits: { string.Join(", ", uniqueDigits)}");
    Console.WriteLine($" │ Count of outputs of unique digits: { uniqueOutputs.Count() }");
    Console.WriteLine($" └────────────");
}

void Part2(string path)
{
    var outputSum = ReadData(path).Select(d => DecodeDigits(d.OutputValues, DecodeData(d))).Sum();
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │ Output sum: { outputSum }");
    Console.WriteLine($" └────────────");
}

Part1("test_input.txt");
Part1("input.txt");

Part2("test_input.txt");
Part2("input.txt");
public record DataRecord(ImmutableList<string> InputPatterns, ImmutableList<string> OutputValues, string OriginalLine);