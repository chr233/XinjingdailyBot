namespace XinjingdailyBot.Infrastructure.Extensions;

/// <summary>
/// 扩展BinaryReader
/// </summary>
public static class BinaryReaderExtension
{
    public static int ReadInt32BE(this BinaryReader br)
    {
        var bytes = br.ReadBytes(4);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt32(bytes, 0);
    }

    public static int ReadInt32LE(BinaryReader br)
    {
        var bytes = br.ReadBytes(4);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt32(bytes, 0);
    }

    public static short ReadInt16LE(this BinaryReader br)
    {
        var bytes = br.ReadBytes(2);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToInt16(bytes, 0);
    }

    public static ushort ReadUInt16LE(BinaryReader br)
    {
        var bytes = br.ReadBytes(2);
        if (!BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return BitConverter.ToUInt16(bytes, 0);
    }
}
