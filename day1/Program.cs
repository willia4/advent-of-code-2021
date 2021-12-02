using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

async IAsyncEnumerable<int> ReadDataAsync(string path)
{
    using var reader = File.OpenText(path);
    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync();
        if (int.TryParse(line, out var value))
        {
            yield return value;
        }
    }
}

async Task<int> CountIncreasesAsync(IAsyncEnumerable<int> numbers)
{
    int? previous = null;
    int increases = 0;

    await foreach(var current in numbers)
    {
        if (previous.HasValue && current > previous.Value)
        {
            increases++;
        }
        previous = current;
    }

    return increases;
}

async IAsyncEnumerable<IEnumerable<T>> ChunkAsyncEnumerable<T>(IAsyncEnumerable<T> items, int chunkSize)
{
    var r = new List<T>(chunkSize);
    await foreach (var item in items)
    {
        if (r.Count == chunkSize)
        {
            yield return r.ToList();
            r.RemoveAt(0);
        }
        r.Add(item);
    }

    if (r.Count == 3)
    {
        yield return r;
    }
}

async IAsyncEnumerable<int> SumsOfChunksAsync(IAsyncEnumerable<IEnumerable<int>> chunks)
{
    await foreach (var chunk in chunks)
    {
        yield return chunk.Sum();
    }
}

async Task Part1(string path)
{
    var increases = await CountIncreasesAsync(ReadDataAsync(path));

    Console.WriteLine($"Part 1: There were {increases} increases");
}

async Task Part2(string path)
{
    var triplets = ChunkAsyncEnumerable(ReadDataAsync(path), 3);
    var increases = await CountIncreasesAsync(SumsOfChunksAsync(triplets));
    
    Console.WriteLine($"Part 2: There were {increases} chunked increases");
}

await Part1("./input.txt");
await Part2("./input.txt");