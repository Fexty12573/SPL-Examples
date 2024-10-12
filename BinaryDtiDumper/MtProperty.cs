
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
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
    public long GetFieldOffset() => IsProperty ? long.MaxValue : (long)((ulong)Data - (ulong)Owner);
    public PropType Type => (PropType)(Flags & 0xFFF);
    public byte* HashNamePtr => Comment != null ? Comment : Name;
    public bool IsArray => (Flags & 0x20000) != 0;
    public bool IsProperty => (Flags & 0x80000) != 0;
}

internal unsafe class DtiProperty(MtProperty* property)
{
    public string Name { get; } = Utf8StringMarshaller.ConvertToManaged(property->Name) ?? string.Empty;
    public string? Comment { get; } = property->Comment != null ? Utf8StringMarshaller.ConvertToManaged(property->Comment) : null;
    public uint Hash { get; } = Utility.Crc32(property->GetHashName());
    public PropType Type { get; } = property->Type;
    public bool IsArray { get; } = property->IsArray;
    public bool IsProperty { get; } = property->IsProperty;
    public uint Flags { get; } = property->Flags;
    public nint Owner { get; } = property->Owner;
    public nint Data { get; } = (nint)property->Data;
    public nint Array { get; } = (nint)property->Array;
    public nint Get { get; } = (nint)property->Get;
    public uint Count { get; } = property->Count;
    public nint GetCount { get; } = (nint)property->GetCount;
    public nint SetData { get; } = (nint)property->SetData;
    public nint SetCount { get; } = (nint)property->SetCount;
    public uint Index { get; } = property->Index;
    public long Offset { get; } = property->GetFieldOffset();

    public long GetFieldOffsetFrom(ulong baseAddr)
    {
        if (IsProperty)
            return long.MaxValue;

        return (long)((ulong)Data - baseAddr);
    }

    public string GetTypeName()
    {
        return Type switch
        {
            PropType.Bool2 => "bool",
            (PropType)0x80 => "custom",
            _ => Type.ToString().ToLowerInvariant()
        };
    }
}
