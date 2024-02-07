
using System.Runtime.CompilerServices;
using SharpPluginLoader.Core.IO;

namespace ColEditor;

public class MtByteWriter(MtStream stream)
{
    public void Write(byte[] bytes)
    {
        stream.Write(bytes);
    }

    public void Write(ReadOnlySpan<byte> bytes)
    {
        stream.Write(bytes);
    }

    public void Write(bool value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(uint value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(ushort value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(byte value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(sbyte value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(short value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(int value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(float value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(double value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(long value)
    {
        stream.Write(AsBytes(value));
    }

    public void Write(ulong value)
    {
        stream.Write(AsBytes(value));
    }

    private static unsafe byte[] AsBytes<T>(T value) where T : unmanaged
    {
        var bytes = new byte[sizeof(T)];
        Unsafe.As<byte, T>(ref bytes[0]) = value;
        return bytes;
    }
}
