using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using CommonHelpers;

const int PacketVersionLength = 3;
const int PacketTypeIdLength = 3;
const int PacketHeaderLength = PacketVersionLength + PacketTypeIdLength;

int AlignToFour(int i) => ((int)Math.Ceiling(((double)i) / 4.0)) * 4;

IEnumerable<byte> HexStringToBits(string s)
{
    static IEnumerable<byte> FourBits(int v)
    {
        return new byte[]
        {
            (byte)((v & 0b1000) >> 3),
            (byte)((v & 0b0100) >> 2),
            (byte)((v & 0b0010) >> 1),
            (byte)((v & 0b0001) >> 0)
        };
    }
    
    foreach (var c in s.Strings())
    {
        var n = Int32.Parse(c, System.Globalization.NumberStyles.HexNumber);
        foreach (var d in FourBits(n))
        {
            if (d == 0 || d == 1)
            {
                yield return d;
            }
            else
            {
                throw new InvalidOperationException($"Could not parse character {c}");
            }
        }
    }
}

(IList<byte> bits, string hexSTring) ReadInputBits(string path)
{
    var s = System.IO.File.ReadAllText(path).Trim();
    return (HexStringToBits(s).ToList(), s);
}

int BitsToInt(IList<byte> bits, int length = -1)
{
    if (length == -1)
    {
        length = bits.Count;
    }

    int r = 0;
    var reverseB = bits.Take(length).Reverse().ToList();
    for (int i = 0; i < reverseB.Count; i++)
    {
        r = r + (reverseB[i] << i);
    }

    return r;
}

int PacketVersion(IList<byte> packet) => BitsToInt(packet, PacketVersionLength);
int PacketType(IList<byte> packet) => BitsToInt(packet.Skip(PacketVersionLength).ToList(), PacketTypeIdLength);

bool IsLiteralPacket(IList<byte> packet) => PacketType(packet) == 4;
bool IsOperatorPacket(IList<byte> packet) => !IsLiteralPacket(packet);

(LiteralValuePacket, int bitsConsumed) DecodeLiteralValue(IList<byte> packet)
{
    if (!IsLiteralPacket(packet)) { throw new InvalidOperationException(); }

    var packetLength = LiteralPacketLength(packet);
    var packetVersion = PacketVersion(packet);
    
    var data = packet
        .Take(packetLength) // only look at the bits for this packet
        .Skip(PacketHeaderLength)
        .ToArray(); // skip the header

    var numberBits = new List<byte>();
    foreach (var chunk in data.Chunk(5))
    {
        var shouldContinue = chunk.First() == 1;
        numberBits.AddRange(chunk.Skip(1));
        
        if (!shouldContinue) { break; }
    }

    var decodedPacket = new LiteralValuePacket(packetVersion, BitsToInt(numberBits, numberBits.Count));
    return (decodedPacket, packetLength);
}

int LiteralPacketLength(IEnumerable<byte> packet)
{
    var c =  PacketVersionLength + PacketTypeIdLength;
    var data = packet.Skip(c);

    while (data.First() == 1)
    {
        c += 5;
        data = data.Skip(5);
    }

    c += 5;

    // c will be padded to a 4-bit boundary so we need to do the same
    //return AlignToFour(c);
    return c;
}

//
// int OperatorPacketLength(IEnumerable<byte> packet)
// {
//     int packetLength = 0; 
//     var data = packet.Skip(PacketHeaderLength);
//     
//     var lengthType = data.First();
//     data = data.Skip(1); // skip the length type
//     if (lengthType == 0)
//     {
//         var length = data.Take(15);
//         packetLength = BitsToInt(length, length.Count()) + 15;
//     }
//     else
//     {
//         var numberOfSubPackets = BitsToInt(data.Take(11), 11);
//         data = data.Skip(11);
//         
//         for (var i = 0; i < numberOfSubPackets; i++)
//         {
//             
//         }
//     }
//
//     return AlignToFour(packetLength + PacketHeaderLength + 1);
// }
//
// // returns the length of the first packet in bits, including the length of any subpackets 
// int PacketLength(IEnumerable<byte> packet)
// {
//     if (IsLiteralPacket(packet))
//     {
//         return LiteralPacketLength(packet);
//     }
//     else
//     {
//         return OperatorPacketLength(packet);
//     }
// }

OperatorPacketLengthType DecodeOperatorPacketLengthType(IList<byte> bits)
{
    var lengthTypeId = bits.Skip(PacketHeaderLength).First();
    return lengthTypeId switch
    {
        0 => OperatorPacketLengthType.BitLength,
        1 => OperatorPacketLengthType.PacketLength,
        _ => throw new InvalidOperationException($"Invalid packet length type {lengthTypeId}")
    };
}

(IEnumerable<Packet> packets, int consumedBits) DecodeBitLengthSubPackets(IList<byte> parentBits)
{
    if (DecodeOperatorPacketLengthType(parentBits) != OperatorPacketLengthType.BitLength)
    {
        return (Enumerable.Empty<Packet>(), 0);
    }

    var parentHeaderLength = PacketHeaderLength + 1;
    var lengthHeaderLength = 15;
    // skip the header, skip the packet length type, and the length is the next 15 bits
    var lengthBits = BitsToInt(parentBits.Skip(parentHeaderLength).Take(lengthHeaderLength).ToList());

    var totalConsumed = parentHeaderLength + lengthHeaderLength; 

    var subPackets = new List<Packet>();
    var subPacketBits = parentBits.Skip(parentHeaderLength + lengthHeaderLength).Take(lengthBits).ToList();
    while (subPacketBits.Any())
    {
        var (nextPacket, nextLength) = DecodePacket(subPacketBits);
        subPackets.Add(nextPacket);
        
        subPacketBits = subPacketBits.Skip(nextLength).ToList();
        totalConsumed += nextLength;

    }

    return (subPackets, totalConsumed);
}

(IEnumerable<Packet> packets, int consumedBits) DecodePacketLengthSubPackets(IList<byte> parentBits)
{
    if (DecodeOperatorPacketLengthType(parentBits) != OperatorPacketLengthType.PacketLength)
    {
        return (Enumerable.Empty<Packet>(), 0);
    }
    
    var parentHeaderLength = PacketHeaderLength + 1;
    var lengthHeaderLength = 11;
    var subPacketCount = BitsToInt(parentBits.Skip(parentHeaderLength).Take(lengthHeaderLength).ToList());

    var totalConsumed = parentHeaderLength + lengthHeaderLength;
    
    var subPackets = new List<Packet>();
    var subPacketBits = parentBits.Skip(parentHeaderLength + lengthHeaderLength).ToList();
    
    for (var i = 0; i < subPacketCount; i++)
    {
        var (nextPacket, nextLength) = DecodePacket(subPacketBits);
        subPackets.Add(nextPacket);
        
        subPacketBits = subPacketBits.Skip(nextLength).ToList();
        totalConsumed += nextLength;
    }
    return (subPackets, totalConsumed);
}

(Packet packet, int consumedBits) DecodePacket(IList<byte> bits)
{
    if (IsLiteralPacket(bits))
    {
        return DecodeLiteralValue(bits);
    }
    else if (IsOperatorPacket(bits))
    {
        var (subPackets, consumedBits) = DecodeOperatorPacketLengthType(bits) switch
        {
            OperatorPacketLengthType.BitLength => DecodeBitLengthSubPackets(bits),
            OperatorPacketLengthType.PacketLength => DecodePacketLengthSubPackets(bits),
            _ => throw new InvalidOperationException("Could not determine operator packet length")
        };

        return (new OperatorPacket(
            version: PacketVersion(bits),
            type: PacketType(bits),
            subPackets: subPackets.ToList()), consumedBits);
    }
    else
    {
        throw new InvalidOperationException($"Could not decode packet with type {PacketType(bits)}");
    }
}

int VersionSum(Packet packet)
{
    if (packet is LiteralValuePacket v)
    {
        return v.Version;
    }
    else if (packet is OperatorPacket o)
    {
        return o.Version + o.SubPackets.Select(VersionSum).Sum();
    }
    else
    {
        throw new InvalidOperationException($"Could not find version sum for packet {packet}");
    }
}

long PacketValue(Packet packet)
{
    if (packet is LiteralValuePacket v)
    {
        return v.Value;
    }
    else if (packet is OperatorPacket o)
    {
        return o.Type switch
        {
            0 => o.SubPackets.Select(PacketValue).Sum(), //Sum
            1 => o.SubPackets.Select(PacketValue).Aggregate((acc, n) => acc * n), //Product
            2 => o.SubPackets.Select(PacketValue).Min(), //Minimum
            3 => o.SubPackets.Select(PacketValue).Max() , //Maximum
            5 => PacketValue(o.SubPackets[0]) > PacketValue(o.SubPackets[1]) ? 1 : 0, //Greater Than
            6 => PacketValue(o.SubPackets[0]) < PacketValue(o.SubPackets[1]) ? 1 : 0, //Less Than
            7 => PacketValue(o.SubPackets[0]) == PacketValue(o.SubPackets[1]) ? 1 : 0, //Equals To
            _ => throw new InvalidOperationException($"Unexpected packet type for packet {packet}")
        };
    }
    else
    {
        throw new InvalidOperationException($"Could not determine the value for packet {packet}");
    }
}

void Part1(string path)
{
    var (packetBits, hex) = ReadInputBits(path);
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 1: ");
    Console.WriteLine($" │ {path}");
    Console.WriteLine( " │ ");
    //Console.WriteLine($" │Hex: {hex}");
    //Console.WriteLine($" │Bits: {string.Join("", packetBits)}");

    var (decoded, decodedLength) = DecodePacket(packetBits);
    Console.WriteLine($" │Packet Length: {decodedLength}");
    Console.WriteLine($" │Packet Type: {decoded.Type}");
    Console.WriteLine($" │Packet Version: {decoded.Version}");
    //Console.WriteLine($" │Packet Value: {decoded}");
    Console.WriteLine($" │Version Sum: {VersionSum(decoded)}");
    Console.WriteLine($" └────────────");
    Console.WriteLine();
    Console.WriteLine();
}

void Part2(string hexString)
{
    var packetBits = HexStringToBits(hexString).ToList();
    
    Console.WriteLine( " │ ");
    Console.WriteLine( " │ Part 2: ");
    Console.WriteLine( " │ ");
    Console.WriteLine($" │Hex: {hexString}");
    //Console.WriteLine($" │Bits: {string.Join("", packetBits)}");

    var (decoded, decodedLength) = DecodePacket(packetBits);
    Console.WriteLine($" │Packet Length: {decodedLength}");
    Console.WriteLine($" │Packet Type: {decoded.Type}");
    Console.WriteLine($" │Packet Version: {decoded.Version}");
    //Console.WriteLine($" │Packet Value: {decoded}");
    Console.WriteLine($" │Value: {PacketValue(decoded)}");
    Console.WriteLine($" └────────────");
    Console.WriteLine();
    Console.WriteLine();
}

IList<byte> packetBits = Array.Empty<byte>();
Packet decoded = null;
int decodedLength = 0;
string hex = "";

// Part1("test_input_val.txt");
//
// Part1("test_input_op1.txt");
// Part1("test_input_op2.txt");
// Part1("test_input_op3.txt");
// Part1("test_input_op4.txt");
// Part1("test_input_op5.txt");
// Part1("test_input_op6.txt");
Part1("input.txt");
//
//
// Part2("C200B40A82");
// Part2("04005AC33890");
// Part2("880086C3E88112");
// Part2("CE00C43D881120");
// Part2("D8005AC2A8F0");
// Part2("F600BC2D8F");
// Part2("9C005AC2F8F0");
//Part2("9C0141080250320F1802104A08");

Part2(System.IO.File.ReadAllText("input.txt").Trim());
public abstract class Packet
{
    public int Type { get; protected set; } = 0;
    public int Version { get; protected set; } = 0;
}

public class LiteralValuePacket : Packet
{
    public LiteralValuePacket(int version, long value)
    {
        Type = 4;
        Version = version;
        Value = value;
    }

    public long Value { get; protected set; }

    public override string ToString()
    {
        return $"<Version: {Version}, Value: {Value}>";
    }
}

public class OperatorPacket : Packet
{
    public OperatorPacket(int version, int type, IList<Packet> subPackets)
    {
        Type = type;
        Version = version;
        SubPackets = ImmutableList<Packet>.Empty.AddRange(subPackets);
    }

    public IList<Packet> SubPackets { get; protected set; }

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append($"<Version: {Version}, Type: {Type}, SubPackets: [");

        sb.Append(String.Join(" ", SubPackets.Select(s => s.ToString())));
        sb.Append("]>");
        return sb.ToString();
    }
}

public class UnknownPacket : Packet
{
    public UnknownPacket(int type, int version)
    {
        Type = type;
        Version = version;
    }
}

public enum OperatorPacketLengthType
{
    BitLength,
    PacketLength
}