using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

byte[] StringToBitArray(string s)
{
    return s.Trim().Select(c => c == '1' ? (byte)1 : (byte)0).ToArray();
}

(int[] zeros, int[] ones) CountBits(int bitLength, IEnumerable<byte[]> bitArrays)
{
    var zeros = new int[bitLength];
    var ones = new int[bitLength];
    
    foreach (var bits in bitArrays)
    {
        for (var i = 0; i < bitLength; i++)
        {
            var bit = bits[i];
            if (bit == 1)
            {
                ones[i]++;
            }
            else
            {
                zeros[i]++;
            }
        }
    }

    return (zeros: zeros, ones: ones);
}

int BitsToInt(IEnumerable<byte> reversedBits)
{
    // Linq doesn't have a reduce function that supplies an index to the aggregator, so we wouldn't know how much to shift by
    // do it the old fashioned way
    int result = 0;
    var bits = reversedBits.ToArray();
    for (var i = 0; i < bits.Length; i++)
    {
        result += (bits[i] << i);
    }

    return result;
}
int MakePart1Number(int bitLength, int[] reversedZeros, int[] reversedOnes, Func<int, int, byte> makeBit)
{
    var bits = reversedZeros.Zip(reversedOnes).Select((bitCounts, bitIndex) =>
    {
        var (zeroCount, oneCount) = bitCounts;
        return makeBit(zeroCount, oneCount);
    });
    
    return BitsToInt(bits);
}

int MakePart2Number(int bitLength, IEnumerable<byte[]> reversedInputs, Func<int, int, int> makeFilter)
{
    var inputs = reversedInputs;
    
    // because the inputs are reversed, we need to go backwards so we start with the MSB
    for (var i = (bitLength - 1); i >= 0; i--)
    {
        if (inputs.Count() <= 1)
        {
            break;
        }

        var bitIndex = i;
        var (zeros, ones) = CountBits(bitLength, inputs);
        var filter = makeFilter(zeros[bitIndex], ones[bitIndex]);

        inputs = inputs.Where(bits => bits[bitIndex] == filter);
    }

    return BitsToInt(inputs.First());
}

async Task Part1(string path)
{
    var input = (await System.IO.File.ReadAllLinesAsync(path)).Select(StringToBitArray);
    var bitLength = input.First().Length;

    var (zeros, ones) = CountBits(bitLength, input);

    // because shifting is the opposite order from array indexing, this is easier if we reverse the array
    // then when we read the LSB with [0], we can shift it << 0 to set it as the LSB 
    var reversedZeros = zeros.Reverse().ToArray();
    var reversedOnes = ones.Reverse().ToArray();
    
    var gamma = MakePart1Number(bitLength, reversedZeros, reversedOnes, (zeroCount, oneCount) => oneCount > zeroCount ? (byte) 1 : (byte) 0);
    var epsilon = MakePart1Number(bitLength, reversedZeros, reversedOnes, (zeroCount, oneCount) => oneCount > zeroCount ? (byte) 0 : (byte) 1);
    
    Console.WriteLine("");
    Console.WriteLine( "│ ");
    Console.WriteLine( "│ Part 1: ");
    Console.WriteLine($"│ Bit Length: {bitLength}");
    Console.WriteLine($"│ Gamma: {gamma}");
    Console.WriteLine($"│ Epsilon: {epsilon}");
    Console.WriteLine($"│ ");
    Console.WriteLine($"│ Power (gamma * epsilon): {gamma * epsilon}");
    Console.WriteLine($"└────");
    
}

async Task Part2(string path)
{
    var input = (await System.IO.File.ReadAllLinesAsync(path));
    var bitLength = input.First().Length;
    
    var reversedInput = input.Select(StringToBitArray).Select(bits => bits.Reverse().ToArray());

    var o2 = MakePart2Number(bitLength, reversedInput, (zerosCount, onesCount) => zerosCount > onesCount ? 0 : 1);
    var co2 = MakePart2Number(bitLength, reversedInput, (zerosCount, onesCount) => zerosCount > onesCount ? 1 : 0);
    
    Console.WriteLine("");
    Console.WriteLine( "│ ");
    Console.WriteLine( "│ Part 2: ");
    Console.WriteLine($"│ Bit Length: {bitLength}");
    Console.WriteLine($"│ O2 Generator: {o2}");
    Console.WriteLine($"│ CO2 Scrubber: {co2}");
    Console.WriteLine($"│ ");
    Console.WriteLine($"│ Life Support (O2 * CO2): {o2 * co2}");
    Console.WriteLine($"└────");
}

Console.WriteLine("Test Input: ");
await Part1("test_input.txt");
await Part2("test_input.txt");

Console.WriteLine("Real Input: ");
await Part1("input.txt");
await Part2("input.txt");