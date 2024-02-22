using ImGuiNET;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Rendering;
using System.Numerics;
using System.Runtime.CompilerServices;
using SharpPluginLoader.Core.Memory;

namespace ScheduLayer;

public unsafe class ScheduLayerPlugin : IPlugin
{
    public string Name => "ScheduLayer";
    public string Author => "Fexty";

    private Scheduler? _scheduler;
    private SchedulerResource? _resource;
    private SchedulerTrackWork* _selectedTrack;
    private SchedulerKeyframe* _selectedKeyframe;
    private int _selectedKeyframeIndex;
    private readonly Dictionary<string, bool> _groupExpandedMap = [];
    private Hook<ClearSchedulerDelegate> _clearSchedulerHook = null!;

    private delegate void ClearSchedulerDelegate(nint scheduler);

    public void OnLoad()
    {
        _clearSchedulerHook = Hook.Create<ClearSchedulerDelegate>(0x14221b390, scheduler =>
        {
            if (_scheduler == scheduler)
            {
                _scheduler = null;
                _resource = null;
                _selectedTrack = null;
                _selectedKeyframe = null;
                _selectedKeyframeIndex = -1;
            }

            _clearSchedulerHook.Original(scheduler);
        });
    }
    
    public void OnUpdate(float deltaTime)
    {
        if (Input.IsDown(Key.LeftAlt) && Input.IsPressed(Key.L))
        {
            LogLines();
        }
    }

    public void OnImGuiRender()
    {
        if (ImGui.BeginCombo("Scheduler", _resource?.FilePath ?? "None"))
        {
            var line = UnitManager.GetLine(2); // Schedulers are always the second line
            if (line is null)
                return;

            foreach (var scheduler in line.Units.Where(u => u.Is("uScheduler")).Select(u => u.As<Scheduler>()))
            {
                var resource = scheduler.Resource;
                if (resource is null || resource.Instance == 0)
                    continue;

                if (ImGui.Selectable(resource.FilePath, _scheduler == scheduler))
                {
                    _scheduler = scheduler;
                    _resource = resource;
                }
            }
            ImGui.EndCombo();
        }

        if (_scheduler is null || _resource is null)
            return;

        Span<float> keyframes = stackalloc float[100]; // Hopefully there are no SDLs with more than 100 keyframes

        ImGui.Text($"Scheduler: {_scheduler.Name}");

        ImGui.Separator();

        ImGui.Checkbox("Paused", ref _scheduler.Pause);
        ImGui.SameLine();
        ImGui.Checkbox("Loop", ref _scheduler.Loop);
        ImGui.SliderFloat("Speed", ref _scheduler.Speed, 0.1f, 10.0f);

        ImGui.BeginChild("##sidebar", new Vector2(), ImGuiChildFlags.ResizeX | ImGuiChildFlags.Border);

        if (_selectedTrack != null)
        {
            ImGui.Text($"Track: {_selectedTrack->Track.Name}");
            ImGui.Separator();

            ImGui.Text($"Has {_selectedTrack->Track.KeyCount} Keyframes");
            ImGui.SeparatorText("Keyframe");
            ImGui.Text($"Frame: {_selectedKeyframe->Frame}");
            ImGui.SameLine();
            ImGui.Text($"Mode: {_selectedKeyframe->Mode}");

            switch (_selectedTrack->Type)
            {
                case TrackType.Float:
                {
                    ref var value = ref _selectedTrack->Track.GetValues<float>()[_selectedKeyframeIndex];
                    ImGui.DragFloat("Value", ref value, 0.1f);
                    break;
                }
                case TrackType.Double:
                {
                    ref var value = ref _selectedTrack->Track.GetValues<double>()[_selectedKeyframeIndex];
                    ImGui.DragScalar("Value", ImGuiDataType.Double, MemoryUtil.AddressOf(ref value), 0.1f);
                    break;
                }
                case TrackType.Bool:
                {
                    ref var value = ref _selectedTrack->Track.GetValues<bool>()[_selectedKeyframeIndex];
                    ImGui.Checkbox("Value", ref value);
                    break;
                }
                case TrackType.Ref:
                {
                    ref var value = ref _selectedTrack->Track.GetValues<int>()[_selectedKeyframeIndex];
                    ImGui.DragInt("Value", ref value, 1);

                    if (value >= 0 && value < _scheduler.Tracks.Length)
                    {
                        // This is a reference to another track
                        var obj = _scheduler.Tracks[value].Object;
                        if (obj is not null)
                        {
                            ImGui.Text($"Object: {obj}");
                        }
                    }
                    break;
                }
                case TrackType.Resource:
                {
                    ref var value = ref _selectedTrack->Track.GetValues<nint>()[_selectedKeyframeIndex];
                    ImGui.Text($"Value: 0x{value:X}");
                    break;
                }
                case TrackType.String:
                {
                    var value = _selectedTrack->Track.GetValues<byte>()[_selectedKeyframeIndex];
                    ImGui.Text($"Value: {value}");
                    break;
                }
                case TrackType.Event:
                {
                    ref var value = ref _selectedTrack->Track.GetValues<uint>()[_selectedKeyframeIndex];
                    ImGuiExtensions.InputScalar("Value", ref value);
                    break;
                }
                case TrackType.Matrix:
                {
                    var value = _selectedTrack->Track.GetValues<Matrix4x4>()[_selectedKeyframeIndex];
                    ImGui.DragFloat4("Value", ref Unsafe.AsRef<Vector4>(&value.M11));
                    ImGui.DragFloat4("#value1", ref Unsafe.AsRef<Vector4>(&value.M21));
                    ImGui.DragFloat4("#value2", ref Unsafe.AsRef<Vector4>(&value.M31));
                    ImGui.DragFloat4("#value3", ref Unsafe.AsRef<Vector4>(&value.M41));
                    _selectedTrack->Track.GetValues<Matrix4x4>()[_selectedKeyframeIndex] = value;
                    break;
                }
                case TrackType.Int:
                {
                    ref var value = ref _selectedTrack->Track.GetValues<int>()[_selectedKeyframeIndex];
                    ImGui.DragInt("Value", ref value, 1);
                    break;
                }
                case TrackType.Int64:
                {
                    ref var value = ref _selectedTrack->Track.GetValues<long>()[_selectedKeyframeIndex];
                    ImGui.DragScalar("Value", ImGuiDataType.S64, MemoryUtil.AddressOf(ref value), 1);
                    break;
                }
                case TrackType.Vector:
                {
                    ref var value = ref _selectedTrack->Track.GetValues<Vector4>()[_selectedKeyframeIndex];
                    ImGui.DragFloat4("Value", ref value);
                    break;
                }
            }
        }

        ImGui.EndChild();
        ImGui.SameLine();
        ImGui.BeginChild("##timeline");

        if (ImGuiExtensions.BeginTimeline(_scheduler.Name, 0, _resource.Header.FrameCount,
            ref _scheduler.Frame, ImGuiTimelineFlags.EnableSnapping | ImGuiTimelineFlags.ExtendFramePointer))
        {
            var groupExpanded = false;
            foreach (ref var trackWork in _scheduler.Tracks)
            {
                if (Unsafe.IsNullRef(ref trackWork.Track))
                    continue;

                switch (trackWork.Type)
                {
                    case TrackType.Root or TrackType.Scheduler:
                        continue;
                    case TrackType.Unit or TrackType.System or TrackType.Object:
                    {
                        if (groupExpanded)
                        {
                            ImGuiExtensions.EndTimelineGroup();
                        }

                        var trackName = trackWork.Track.Name;
                    
                        groupExpanded = _groupExpandedMap.GetValueOrDefault(trackName, false);
                        ImGuiExtensions.BeginTimelineGroup(trackName, ref groupExpanded);
                        _groupExpandedMap[trackName] = groupExpanded;
                        break;
                    }
                    default:
                    {
                        if (groupExpanded)
                        {
                            var sourceKeyframes = trackWork.Track.Keyframes;
                            for (var i = 0; i < trackWork.Track.KeyCount; i++)
                            {
                                keyframes[i] = sourceKeyframes[i].Frame;
                            }

                            if (ImGuiExtensions.TimelineTrack(trackWork.Track.Name, keyframes, 
                                    out var selectedKeyframe, trackWork.Track.KeyCount))
                            {
                                for (var i = 0; i < trackWork.Track.KeyCount; i++)
                                {
                                    sourceKeyframes[i].Frame = (int)keyframes[i];
                                }
                            }

                            if (selectedKeyframe != -1)
                            {
                                _selectedKeyframeIndex = selectedKeyframe;
                                _selectedKeyframe = MemoryUtil.AsPointer(ref trackWork.Track.Keyframes[_selectedKeyframeIndex]);
                                _selectedTrack = MemoryUtil.AsPointer(ref trackWork);
                            }
                        }

                        break;
                    }
                }
            }

            if (groupExpanded)
            {
                ImGuiExtensions.EndTimelineGroup();
            }

            ImGuiExtensions.EndTimeline();
        }

        ImGui.EndChild();
    }

    private static void LogLines()
    {
        for (var i = 0; i < UnitManager.LineCount; i++)
        {
            var line = UnitManager.GetLine(i);
            if (line is null)
                continue;

            Log.Info($"Line {i}: {line.Name}, {line.UnitCount} units:");
            foreach (var unit in line.Units)
            {
                Log.Info($"  {unit} ({unit.Name})");
            }
        }
    }
}
