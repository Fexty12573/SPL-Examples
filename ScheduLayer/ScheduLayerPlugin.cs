using ImGuiNET;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Rendering;
using System.Numerics;

namespace ScheduLayer;

public unsafe class ScheduLayerPlugin : IPlugin
{
    public string Name => "ScheduLayer";
    public string Author => "Fexty";

    private Scheduler? _scheduler;
    private SchedulerResource? _resource;
    private SchedulerTrackWork* _selectedTrack;
    private readonly Dictionary<string, bool> _groupExpandedMap = [];
    
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
                if (resource is null)
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
        ImGui.SliderFloat("Speed", ref _scheduler.Speed, 0.1f, 3.0f);

        if (ImGuiExtensions.BeginTimeline(_scheduler.Name, 0, _resource.Header.FrameCount,
            ref _scheduler.Frame, ImGuiTimelineFlags.EnableSnapping | ImGuiTimelineFlags.ExtendFramePointer))
        {
            bool groupExpanded = false;
            foreach (ref var trackWork in _scheduler.Tracks)
            {
                if (trackWork.Type is TrackType.Root or TrackType.Scheduler)
                    continue;

                if (trackWork.Type is TrackType.Unit or TrackType.System or TrackType.Object)
                {
                    if (groupExpanded)
                    {
                        ImGuiExtensions.EndTimelineGroup();
                        groupExpanded = false;
                    }

                    var trackName = trackWork.Track.Name;
                    
                    groupExpanded = _groupExpandedMap.GetValueOrDefault(trackName, false);
                    ImGuiExtensions.BeginTimelineGroup(trackName, ref groupExpanded);
                    _groupExpandedMap[trackName] = groupExpanded;
                }
                else if (groupExpanded)
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
                        _scheduler.Frame = keyframes[selectedKeyframe];
                    }
                }
            }

            if (groupExpanded)
            {
                ImGuiExtensions.EndTimelineGroup();
            }

            ImGuiExtensions.EndTimeline();
        }
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
