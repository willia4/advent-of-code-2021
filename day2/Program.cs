using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NavigationCommand = System.Func<Position, Position>;
using CommonHelpers;

async IAsyncEnumerable<string> ReadDataAsync(string path)
{
    using var reader = File.OpenText(path);
    while (!reader.EndOfStream)
    {
        var line = await reader.ReadLineAsync();
        if (line != null)
        {
            yield return line;
        }
    }
}

async IAsyncEnumerable<NavigationCommand> ParseCommands(IAsyncEnumerable<string> lines, Func<string, NavigationCommand?> parser)
{
    await foreach (var line in lines)
    {
        var cmd = parser(line);
        if (cmd != null)
        {
            yield return cmd;            
        }
    }
}

static BasicCommand? ParseBasicCommand(string command)
{
    var parseCommandRegex = new Regex("(?<direction>\\w+?) (?<amount>\\d+?)");
    var match = parseCommandRegex.Match(command);
    return match.Success 
        ? new BasicCommand(match.Groups["direction"].Value, Helpers.SafeParseInt(match.Groups["amount"].Value))
        : null;
}

static NavigationCommand? ParseCommandPart1(string command)
{
    var parsed = ParseBasicCommand(command);
    if (parsed is var (direction, amount))
    {
        return direction switch
        {
            "forward" => (p => p with {Horizontal = p.Horizontal + amount }),
            "down" => (p => p with { Depth = p.Depth + amount }),
            "up" => (p => p with { Depth = p.Depth - amount }),
            _ => null
        };
    }

    return null;
}

static NavigationCommand? ParseCommandPart2(string command)
{
    var parsed = ParseBasicCommand(command);

    if (parsed is var (direction, amount))
    {
        
        return direction switch
        {
            "down" => (p => p with { Aim = p.Aim + amount}),
            "up" => (p => p with { Aim = p.Aim - amount}),
            "forward" => (p => p with
            {
                Horizontal = p.Horizontal + amount,
                Depth = p.Depth + (p.Aim * amount)
            }),
            _ => null
        };
    }

    return null;
}

async Task Part1(string path)
{
    var p = new Position(0,0,0);
    await foreach (var cmd in ParseCommands(ReadDataAsync(path), ParseCommandPart1))
    {
        p = cmd(p);
    }
    
    Console.WriteLine($"Part 1: The final position is (Depth: {p.Depth} * Horizontal: {p.Horizontal}) = {p.Depth * p.Horizontal}");
}

async Task Part2(string path)
{
    var p = new Position(0,0,0);
    await foreach (var cmd in ParseCommands(ReadDataAsync(path), ParseCommandPart2))
    {
        p = cmd(p);
    }
    
    Console.WriteLine($"Part 1: The final position is (Depth: {p.Depth} * Horizontal: {p.Horizontal}) = {p.Depth * p.Horizontal}");
}

Console.WriteLine("Test Input: ");
await Part1("test_input.txt");
await Part2("test_input.txt");

Console.WriteLine();
Console.WriteLine("---");
Console.WriteLine("Real Input: ");
await Part1("input.txt");
await Part2("input.txt");

public record Position(int Depth, int Horizontal, int Aim);
public record BasicCommand(string Direction, int Amount);