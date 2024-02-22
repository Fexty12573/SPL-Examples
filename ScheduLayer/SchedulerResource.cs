using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScheduLayer;

internal class SchedulerResource : Resource
{
    public SchedulerResource(nint instance) : base(instance, true) { }
    public SchedulerResource() : base() { }

    public unsafe ref SchedulerHeader Header => ref MemoryUtil.AsRef(GetPtr<SchedulerHeader>(0xA8));
}

[StructLayout(LayoutKind.Sequential, Size = 0x58)]
internal unsafe struct SchedulerHeader
{
    public uint Magic;
    public ushort Version;
    public ushort TrackCount;
    public uint Crc;
    private uint _flags;
    public int BaseTrackIndex;
    public nint MetaDataBuffer;
    public SchedulerTrack* Tracks;

    public int FrameCount
    {
        readonly get => (int)(_flags & 0x3FFFFF);
        set => _flags = ((_flags & 0xFFC00000) | ((uint)value & 0x3FFFFF));
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x10)]
internal unsafe struct SchedulerTrack
{
    public TrackType Type;
    private byte _propType;
    public ushort KeyCount;
    private uint _meta;
    private sbyte* _name;
    private nint _dti;
    public ulong UnitGroup;
    private nint _keyframes;
    private nint _values;

    public PropType PropType
    {
        readonly get => (PropType)_propType;
        set => _propType = (byte)value;
    }

    public int ParentIndex
    {
        readonly get => (int)(_meta & 0xFFFFFF);
        set => _meta = ((_meta & 0xFF000000) | ((uint)value & 0xFFFFFF));
    }

    public int MoveLine
    {
        readonly get => (int)((_meta >> 24) & 0xFF);
        set => _meta = ((_meta & 0xFFFFFF) | (((uint)value & 0xFF) << 24));
    }
    
    public readonly string Name => new(_name);

    public MtDti Dti
    {
        readonly get => new(_dti);
        set => _dti = value.Instance;
    }

    public readonly Span<SchedulerKeyframe> Keyframes => new((void*)_keyframes, KeyCount);

    /// <summary>
    /// Get the values of the track
    /// </summary>
    /// <typeparam name="T">The type of the values, should be according to <see cref="Type"/>.</typeparam>
    /// <returns>The values of the track</returns>
    /// <remarks>
    /// Mapping of <see cref="Type"/> to <typeparamref name="T"/>:
    /// <list type="table">
    ///     <listheader>
    ///         <term>Track Type</term>
    ///         <description><typeparamref name="T"/></description>
    ///     </listheader>
    ///     <item>
    ///         <term><see cref="TrackType.Int"/></term>
    ///         <description><see cref="int"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.Int64"/></term>
    ///         <description><see cref="long"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.Vector"/></term>
    ///         <description><see cref="System.Numerics.Vector4"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.Float"/></term>
    ///         <description><see cref="float"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.Double"/></term>
    ///         <description><see cref="double"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.Bool"/></term>
    ///         <description><see cref="bool"/></description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.Ref"/></term>
    ///         <description><see cref="int"/>, serves as an index</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.Resource"/></term>
    ///         <description><see cref="nint"/>, pointer to the resource</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.String"/></term>
    ///         <description><see cref="byte"/>, list of nullterminated strings</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.Event"/></term>
    ///         <description><see cref="uint"/>, parameter value for the event</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="TrackType.Matrix"/></term>
    ///         <description><see cref="System.Numerics.Matrix4x4"/></description>
    ///     </item>
    /// </list>
    /// </remarks>
    public Span<T> GetValues<T>() where T : unmanaged => new((void*)_values, KeyCount);
}

[StructLayout(LayoutKind.Sequential, Size = 0x4)]
public struct SchedulerKeyframe
{
    private uint _data;

    public int Frame
    {
        readonly get => (int)(_data & 0xFFFFFF);
        set => _data = ((_data & 0xFF000000) | ((uint)value & 0xFFFFFF));
    }

    public KeyMode Mode
    {
        readonly get => (KeyMode)((_data >> 24) & 0xFF);
        set => _data = ((_data & 0xFFFFFF) | (((uint)value & 0xFF) << 24));
    }
}

public enum TrackType : byte
{
    None,
    Root,
    Unit,
    System,
    Scheduler,
    Object,
    Int,
    Int64,
    Vector,
    Float,
    Double,
    Bool,
    Ref,
    Resource,
    String,
    Event,
    Matrix,
}

public enum KeyMode : byte
{
    Constant,
    Offset,
    Trigger,
    Linear,
    OffsetF,
    Hermite,
}
