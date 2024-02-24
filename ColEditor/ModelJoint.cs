using System.Numerics;
using System.Runtime.InteropServices;
using SharpPluginLoader.Core.MtTypes;

namespace ColEditor;


[StructLayout(LayoutKind.Explicit, Size = 0xC0)]
public struct ModelJoint
{
    [FieldOffset(0x08)] public nint Constraint;
    [FieldOffset(0x10)] public float Length;
    [FieldOffset(0x14)] public uint Depth;
    [FieldOffset(0x20)] public Matrix4x4 Transform;
    [FieldOffset(0x60)] public Vector3 Offset;
    [FieldOffset(0x70)] public MtQuaternion Rotation;
    [FieldOffset(0x80)] public Vector3 Scale;
    [FieldOffset(0x90)] public Vector3 Translation;
    [FieldOffset(0xA0)] public uint Id;
    [FieldOffset(0xA4)] public byte ParentIndex;
    [FieldOffset(0xA5)] public byte SymmetryIndex;
    [FieldOffset(0xA6)] public byte Type;
    [FieldOffset(0xA7)] public byte ChildCount;
    [FieldOffset(0xA8)] public byte Attributes;
    [FieldOffset(0xB0)] public nint Owner;

    public Vector3 Position
    {
        readonly get => new(Transform.M41, Transform.M42, Transform.M43);
        set
        {
            Transform.M41 = value.X;
            Transform.M42 = value.Y;
            Transform.M43 = value.Z;
        }
    }
}
