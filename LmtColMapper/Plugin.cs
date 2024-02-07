using System;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Entities;
using ImGuiNET;
using SharpPluginLoader.Core.Components;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Resources;
using SharpPluginLoader.Core.View;

namespace LmtColMapper;

public unsafe class Plugin : IPlugin
{
    public string Name => "LmtColMapper";
    public string Author => "Fexty";

    private readonly List<Monster> _monsters = [];
    private Monster? _selectedMonster;
    private int _currentAction = -1;
    private bool _doingActions = false;

    private Patch _distPatch;
    private delegate void SetCameraDelegate(nint cameraA, nint unkn, nint cameraB, nint data);
    private Hook<SetCameraDelegate> _setCameraHook = null!;
    private bool _shouldChangeCameraDistance = true;

    private readonly uint[] _bankMemberHashes =
    [
        0xBF9000B3,
        0xA68B31F2,
        0x8DA66231,
        0x94BD5370,
        0xDBFCC5B7,
        0xC2E7F4F6,
        0xE9CAA735,
        0xF0D19674,
    ];

    private readonly uint[] _nodeIdMemberHashes =
    [
        0xB296CA66,
        0xAB8DFB27,
        0x80A0A8E4,
        0x99BB99A5,
        0xD6FA0F62,
        0xCFE13E23,
        0xE4CC6DE0,
        0xFDD75CA1,
    ];

    public PluginData Initialize()
    {
        return new PluginData
        {
            OnImGuiRender = true,
            OnMonsterCreate = true,
            OnMonsterDestroy = true,
            OnMonsterAction = true,
            OnEntityAnimation = true,
        };
    }

    public void OnLoad()
    {
        var nopBytes = Enumerable.Repeat<byte>(0x90, 15).ToArray();
        _distPatch = new Patch((nint)0x141fa6504, nopBytes, true);

        _setCameraHook = Hook.Create<SetCameraDelegate>((nint)0x141f7d3c0, (a, unkn, b, data) =>
        {
            var viewportIndex = MemoryUtil.Read<int>(data);
            var x = MemoryUtil.Read<float>(data + 4);

            if (viewportIndex != 0)
            {
                _setCameraHook.Original(a, unkn, b, data);
                return;
            }

            Camera camera;
            if (x > 0.0f)
            {
                camera = new Camera(a);
                if (camera.Is("uInterpolationCamera")) // Exiting quest board
                {
                    _distPatch.Enable();
                    _shouldChangeCameraDistance = true;
                }
            }
            else
            {
                camera = new Camera(b);
                if (camera.Is("uMhSimpleCamera")) // Entering quest board
                {
                    _distPatch.Disable();
                    _shouldChangeCameraDistance = false;
                }
            }

            _setCameraHook.Original(a, unkn, b, data);
        });
    }

    public void OnImGuiRender()
    {
        var camera = CameraSystem.MainViewport.Camera;
        if (camera is not null && _shouldChangeCameraDistance)
        {
            ImGui.DragFloat("Camera Distance", ref camera.GetRef<float>(0x748));
        }

        lock (_monsters)
        {
            if (ImGui.BeginCombo("Monster", _selectedMonster?.Name ?? "N/A"))
            {
                foreach (var monster in _monsters)
                {
                    var isSelected = monster == _selectedMonster;
                    if (ImGui.Selectable(monster.Name, isSelected))
                        _selectedMonster = monster;

                    if (isSelected)
                        ImGui.SetItemDefaultFocus();
                }

                ImGui.EndCombo();
            }
        }

        ImGui.Separator();

        if (ImGui.Button("Do All Actions") && _selectedMonster is not null)
        {
            Log.Info("Mapping Actions for " + _selectedMonster?.Name ?? "N/A");
            _doingActions = true;
            _currentAction = 0;
        }
    }

    public void OnMonsterCreate(Monster monster)
    {
        lock (_monsters)
            _monsters.Add(monster);
    }

    public void OnMonsterDestroy(Monster monster)
    {
        lock (_monsters)
            _monsters.Remove(monster);

        if (_selectedMonster == monster)
            _selectedMonster = null;
    }

    public void OnMonsterAction(Monster monster, ref int action)
    {
        if (monster != _selectedMonster || !_doingActions)
            return;

        var actions = monster.ActionController.GetActionList(1);
        if (_currentAction >= actions.Count)
        {
            _doingActions = false;
            return;
        }

        _currentAction += 1;
        action = _currentAction;

        var actionObj = actions[action];
        if (actionObj is null || actionObj.Instance == 0)
            return;

        Log.Info($"    Action: {action} ({actionObj.Name})");
    }

    public void OnEntityAnimation(Entity entity, ref AnimationId animationId, ref float startFrame, ref float interFrame)
    {
        if (!_doingActions || entity != _selectedMonster)
            return;

        Log.Info($"        Animation: {animationId}");

        var animLayer = entity.AnimationLayer;
        var colComponent = entity.CollisionComponent;
        Ensure.NotNull(animLayer);
        Ensure.NotNull(colComponent);

        var cols = new Span<nint>(colComponent.GetPtrInline(0xD0), 8);

        var lmts = new Span<nint>(animLayer.GetPtrInline(0xE120), 16);
        var lmt = lmts[(int)animationId.Lmt] != 0 ? new MotionList(lmts[(int)animationId.Lmt]) : null;
        Ensure.NotNull(lmt);

        Ensure.IsTrue(lmt.Header.HasMotion((int)animationId.Id));
        ref var anim = ref lmt.Header.GetMotion((int)animationId.Id);
        if (!anim.HasMetadata)
            return;

        foreach (ref var param in anim.Metadata.Params)
        {
            if (param.Dti?.Name == "nTimelineParam::CollisionTimelineObject")
            {
                var bank = -1;
                var nodeId = -1;
                foreach (ref var member in param.Members)
                {
                    if (_bankMemberHashes.Contains(member.Hash))
                        bank = member.GetKeyframe(0).IntValue;
                    else if (_nodeIdMemberHashes.Contains(member.Hash))
                        nodeId = member.GetKeyframe(0).IntValue;
                }


                if (bank == -1)
                {
                    if (nodeId == -1)
                        continue;

                    Log.Info($"            ColLink: -1:{nodeId} (Unknown Link)");
                    continue;
                }

                var colPtr = cols[bank];
                if (colPtr == 0)
                    continue;

                var col = new ObjCollision(colPtr);
                Ensure.NotNull(col.CollIndex);
                ref var node = ref col.CollIndex.Indices[nodeId];
                Log.Info($"            ColLink: {bank}:{nodeId} ({node.Name})");
            }
        }
    }
}
