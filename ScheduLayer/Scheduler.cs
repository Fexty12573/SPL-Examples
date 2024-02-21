using SharpPluginLoader.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ScheduLayer;

internal class Scheduler : Unit
{
    public Scheduler(nint instance) : base(instance) { }
    public Scheduler() : base() { }

    public SchedulerResource? Resource => new(Get<nint>(0x138)); // Not using GetObject because we want a WeakRef

    public int TrackCount => Get<int>(0x168);

    public ref float Frame => ref GetRef<float>(0x178);

    public ref float PreviousFrame => ref GetRef<float>(0x17C);

    public ref float Speed => ref GetRef<float>(0x180);

    public ref bool Pause => ref GetRef<bool>(0x188);

    public ref bool Loop => ref GetRef<bool>(0x189);

    public unsafe Span<SchedulerTrackWork> Tracks => new(GetPtr(0x140), TrackCount);

    public unsafe PointerArray<SchedulerTrackWork> UnitTracks => new(Get<nint>(0x148), TrackCount);
}

[StructLayout(LayoutKind.Explicit, Size = 0x78)]
internal unsafe struct SchedulerTrackWork
{
    [FieldOffset(0x00)] private SchedulerTrack* _trackResource;
    [FieldOffset(0x08)] private nint _object;
    [FieldOffset(0x68)] private uint _trackType;
    [FieldOffset(0x6C)] public int Key;
    [FieldOffset(0x74)] public int Index;

    public ref SchedulerTrack Track => ref *_trackResource;

    public MtObject? Object
    {
        readonly get => _object != 0 ? new(_object) : null;
        set => _object = value?.Instance ?? 0;
    }

    public TrackType Type
    {
        readonly get => (TrackType)_trackType;
        set => _trackType = (uint)value;
    }
}
