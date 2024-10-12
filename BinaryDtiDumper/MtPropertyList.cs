
using System.Runtime.InteropServices;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;

namespace BinaryDtiDumper;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct MtPropertyList : IDisposable
{
    private readonly nint _vtable;
    public readonly MtProperty* First;
    public readonly MtProperty** Pool;
    public readonly uint PoolPt;

    public MtPropertyList()
    {
        Ctor.Invoke(MemoryUtil.AddressOf(ref this));
    }

    public void Dispose()
    {
        Dtor.Invoke(MemoryUtil.AddressOf(ref this));
    }

    private static readonly NativeAction<nint> Ctor = new(0x142171920);
    private static readonly NativeAction<nint> Dtor = new(0x142171940);
}
