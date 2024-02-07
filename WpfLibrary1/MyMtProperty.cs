
using System.Runtime.InteropServices;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;

namespace FsmEditor;

[StructLayout(LayoutKind.Explicit, Size = 0x58)]
internal unsafe struct MyMtProperty
{
    [FieldOffset(0x00)] public sbyte* NamePtr;
    [FieldOffset(0x08)] public sbyte* CommentPtr;
    [FieldOffset(0x10)] public uint FlagsAndType;
    [FieldOffset(0x18)] public nint Owner;
    [FieldOffset(0x20)] public nint Get;
    [FieldOffset(0x20)] public nint Address;
    [FieldOffset(0x28)] public nint GetCount;
    [FieldOffset(0x28)] public nint Count;
    [FieldOffset(0x30)] public nint Set;
    [FieldOffset(0x38)] public nint SetCount;
    [FieldOffset(0x40)] public int Index;
    [FieldOffset(0x48)] public MyMtProperty* Prev;
    [FieldOffset(0x50)] public MyMtProperty* Next;

    public PropType Type
    {
        readonly get => (PropType)(FlagsAndType & 0xFFF);
        set => FlagsAndType = (FlagsAndType & ~0xFFFu) | (uint)value;
    }

    public uint Flags
    {
        readonly get => FlagsAndType >> 12;
        set => FlagsAndType = (FlagsAndType & 0xFFFu) | (value << 12);
    }
}

internal static class MtPropertyListExtensions
{
    public static unsafe void AddProperty(this MtPropertyList list, ref MyMtProperty property)
    {
        var head = list.GetPtr<MyMtProperty>(0x8);
        if (head != null)
        {
            head->Prev = MemoryUtil.AsPointer(ref property);
            property.Next = head;
        }

        list.SetPtr(0x8, MemoryUtil.AsPointer(ref property));
    }
}
