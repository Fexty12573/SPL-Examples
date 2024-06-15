
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using SharpPluginLoader.Core;

namespace BinaryDtiDumper;

[StructLayout(LayoutKind.Explicit)]
internal readonly unsafe struct MtProperty
{
    [FieldOffset(0x00)] public readonly byte* Name;
    [FieldOffset(0x08)] public readonly byte* Comment;
    [FieldOffset(0x10)] public readonly uint Flags;
    [FieldOffset(0x18)] public readonly nint Owner;
    [FieldOffset(0x20)] public readonly void* Data;
    [FieldOffset(0x20)] public readonly void* Array;
    [FieldOffset(0x20)] public readonly void* Get;
    [FieldOffset(0x28)] public readonly uint Count;
    [FieldOffset(0x28)] public readonly void* GetCount;
    [FieldOffset(0x30)] public readonly void* SetData;
    [FieldOffset(0x38)] public readonly void* SetCount;
    [FieldOffset(0x40)] public readonly uint Index;
    [FieldOffset(0x48)] public readonly MtProperty* Prev;
    [FieldOffset(0x50)] public readonly MtProperty* Next;

    public string GetHashName() => Utf8StringMarshaller.ConvertToManaged(HashNamePtr) ?? string.Empty;
    public PropType Type => (PropType)(Flags & 0xFFF);
    public byte* HashNamePtr => Comment != null ? Comment : Name;
}
