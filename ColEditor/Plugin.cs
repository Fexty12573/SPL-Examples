using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using ImGuiNET;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Components;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core.Geometry;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Models;
using SharpPluginLoader.Core.MtTypes;
using SharpPluginLoader.Core.Rendering;
using SharpPluginLoader.Core.Resources;
using SharpPluginLoader.Core.Resources.Collision;
using SharpPluginLoader.Core.View;

namespace ColEditor;

using ActiveNode = (CollisionComponent.Node node, bool visible);
using CollIdentifier = (int bankId, uint uniqueId, nint clidPtr);
using ActiveShell = (Unit shell, MtArray<CollisionComponent.Node> nodes);

public partial class Plugin : IPlugin
{
    public string Name => "Collision Editor";
    public string Author => "Fexty";

    #region Selected Resources

    private Model? _selectedModel;
    private CollisionComponent? _selectedColComponent;
    private ObjCollision? _selectedCollision;
    private CollIndexResource? _selectedClid;
    private CollNodeResource? _selectedClnd;
    private AttackParamResource? _selectedAtk;
    private Resource? _selectedOap;
    private int _selectedBankId;
    private readonly Dictionary<nint, ActiveNode> _nodeMap = [];
    private string _savePath = "";
    private readonly Queue<CollIdentifier> _nodesToCreate = [];
    private Viewport _mainViewport = null!;

    private float _textSize = 16f;
    private Vector4 _textColor = new(0, 1, 1, 1);
    private Vector4 _defaultJointColor = new(1, 1, 1, 0.5f);
    private float _jointRadius = 1f;

#if DEBUG
    private ImGuiDti _imGuiDti = null!;
    private string _singletonName = "sMhScene";
    private string _propFilter = "";
#endif

    private MoveLine _moveLine = null!;
    private readonly Dictionary<nint, ActiveShell> _activeShells = [];

    #endregion

    #region Cached Dti

    private MtDti _colComponentDti = null!;
    private MtDti _collGeomDti = null!;

    #endregion

    #region Enum Names

    private readonly string[] _collGeomShapes = Enum.GetNames(typeof(CollGeomShape));
    private readonly string[] _impactTypes = Enum.GetNames(typeof(ImpactType));
    private readonly string[] _elementTypes = Enum.GetNames(typeof(ElementType));
    private readonly string[] _knockbackTypes = Enum.GetNames(typeof(KnockbackType));
    private readonly string[] _guardTypes = Enum.GetNames(typeof(GuardType));

    #endregion

    #region Functions/Hooks

    private NativeFunction<nint, string, nint> _mtStringSet;
    private NativeAction<nint, int, uint, bool, bool> _createNodeFromUid;
    private Hook<CreateNodeDelegate> _createNodeHook = null!;
    private Hook<CreateShellDelegate> _createShellHook = null!;
    private Hook<DestroyShellDelegate> _destroyShellHook = null!;
    private Hook<InitShellDelegate> _initShellHook = null!;

    private delegate nint CreateNodeDelegate(nint colComponent, nint col, nint atk, nint clnd, nint catk, nint clid,
        bool motSync, uint explicitUid);

    private delegate nint CreateShellDelegate(nint shlp, nint parent1, nint parent2, nint param, int shllIndex,
        int shlpIndex, byte register);

    private delegate void InitShellDelegate(nint shell);

    private delegate void DestroyShellDelegate(nint shell);

    #endregion

    public PluginData Initialize() => new() { ImGuiWrappedInTreeNode = false };

    public void OnLoad()
    {
        InitializeConfig();
        InitializeBoneManager();
        
        _mainViewport = CameraSystem.MainViewport;
        _colComponentDti = MtDti.Find("cpObjCollision")!;
        _collGeomDti = MtDti.Find("cCollGeom")!;
        Ensure.NotNull(_colComponentDti);
        Ensure.NotNull(_collGeomDti);
        
        var address = FindFunction("48 8B D9 48 8B FA 48 8B 09 48 8D 41 08 48 85 C9 75 07");
        Ensure.IsTrue(address != 0);
        _mtStringSet = new NativeFunction<nint, string, nint>(address - 10);

        address = FindFunction("48 8B 94 F9 D0 00 00 00 48 85 D2 74 11");
        Ensure.IsTrue(address != 0);
        _createNodeFromUid = new NativeAction<nint, int, uint, bool, bool>(address - 24);

        address = FindFunction("8B 9C 24 A8 01 00 00 4C 8B E1 48 8B 89 A8 01 00 00");
        Ensure.IsTrue(address != 0);
        _createNodeHook = Hook.Create<CreateNodeDelegate>(address - 18,
            (component, col, atk, clnd, catk, clid, sync, uid) =>
            {
                var node = _createNodeHook.Original(component, col, atk, clnd, catk, clid, sync, uid);
                if (_selectedColComponent is null
                    || _selectedColComponent.Instance != component
                    || _selectedColComponent.Resources[_selectedBankId] != col)
                    return node;

                if (node != 0)
                    _nodeMap.TryAdd(clid, (new CollisionComponent.Node(node), false));

                return node;
            });

#if DEBUG
        _imGuiDti = new ImGuiDti(SingletonManager.GetSingleton("sMhScene")!);
#endif

        var line = UnitManager.GetLine(16);
        Ensure.NotNull(line);

        _moveLine = line;

        address = FindFunction("48 83 EC 20 48 8B D9 48 81 C1 F0 2F 00 00");
        Ensure.IsTrue(address != 0);

        _destroyShellHook = Hook.Create<DestroyShellDelegate>(address - 20, shell =>
        {
            _activeShells.Remove(shell);
            _destroyShellHook.Original(shell);
        });

        var temp = MtDti.Find("uShellBase")?.CreateInstance<MtObject>();
        if (temp is not null)
        {
            _initShellHook = Hook.Create<InitShellDelegate>(temp.GetVirtualFunction(5), shell =>
            {
                _initShellHook.Original(shell);
                if (shell == 0)
                    return;

                var shellObj = new Unit(shell);
                var colComponent = shellObj.ComponentManager.Find("cpObjCollision")?.As<CollisionComponent>();
                if (colComponent is null)
                {
                    Log.Warn($"Failed to find cpObjCollision on {shellObj}");
                    return;
                }

                _activeShells.Add(shell, (shellObj, colComponent.Nodes));
            });

            temp.Destroy(true);
        }
        else
        {
            Log.Error("Failed to find uShellBase and create an instance of it");
        }
    }

    public void OnRender()
    {
        // Draw Bones
        if (_selectedModel is not null && _settings.ShowBones)
        {
            var joints = _selectedModel.GetJoints();
            var defaultColor = _config.DefaultBoneColor.ToVector4();

            foreach (ref var joint in joints)
            {
                if (joint.ParentIndex == 0xFF)
                    continue;

                ref var parent = ref joints[joint.ParentIndex];
                var capsule = new MtCapsule
                {
                    Point1 = joint.Position,
                    Point2 = parent.Position,
                    Radius = _jointRadius
                };

                var color = defaultColor;
                foreach (var preset in _config.BonePresets)
                {
                    if (preset.Bones.Contains(joint.Id))
                    {
                        color = preset.Color.ToVector4();
                        break;
                    }
                }

                Primitives.RenderCapsule(capsule, color);
            }
        }

        // Draw Colliders
        foreach (var (node, visible) in _nodeMap.Values)
        {
            if ((!visible && !_settings.ShowAllColliders) || node is null)
                continue;

            RenderNode(node);
        }

        // Draw Shell Colliders
        if (_settings.DrawShellColliders)
        {
            foreach (var (_, nodes) in _activeShells.Values)
            {
                foreach (var node in nodes)
                {
                    RenderNode(node);
                }
            }
        }

        return;

        void RenderNode(CollisionComponent.Node node)
        {
            foreach (var geometry in node.Geometries)
            {
                if (geometry.Geom is null) continue;

                MtColor color;
                if (node.IsActive)
                {
                    color = geometry.Geom.Type switch
                    {
                        GeometryType.Sphere => _settings.ActiveColliderSphereColor,
                        GeometryType.Capsule => _settings.ActiveColliderCapsuleColor,
                        GeometryType.Obb => _settings.ActiveColliderObbColor,
                        _ => new MtColor(255, 255, 255, 64)
                    };

                    if (node.Get<nint>(0x90) == 0) // AttackParam
                        color = _settings.NonAttackColliderColor;
                }
                else
                {
                    color = _settings.InactiveColliderColor;
                }

                switch (geometry.Geom.Type)
                {
                    case GeometryType.Sphere:
                        Primitives.RenderSphere(geometry.Geom.Get<MtSphere>(0x20), color);
                        break;
                    case GeometryType.Capsule:
                        Primitives.RenderCapsule(geometry.Geom.Get<MtCapsule>(0x20), color);
                        break;
                    case GeometryType.Obb:
                        Primitives.RenderObb(geometry.Geom.Get<MtObb>(0x20), color);
                        break;
                }
            }
        }
    }

    public void OnImGuiFreeRender()
    {

#if DEBUG
        if (_selectedModel is not null)
        {
            var bgDrawList = ImGui.GetBackgroundDrawList();
            var font = ImGui.GetFont();
            foreach (ref var joint in _selectedModel!.GetJoints())
            {
                if (_mainViewport.WorldToScreen(joint.Position, out var screenPos))
                {
                    var pos = new Vector2(screenPos.X + 10, screenPos.Y);
                    bgDrawList.AddText(font, _textSize, pos, MtColor.FromVector4(_textColor), joint.Id.ToString());
                }
            }

            if (_selectedModel.Is("uCharacterModel"))
            {
                var stageAdjustCollision = _selectedModel.Instance + 0xA40;
                ref var bb = ref MemoryUtil.GetRef<MtAabb>(stageAdjustCollision + 0x20);

                // Draw the bounding box as a series of lines
                var min = MemoryUtil.Read<Vector3>(stageAdjustCollision + 0x80);
                var max = MemoryUtil.Read<Vector3>(stageAdjustCollision + 0x70);

                Span<Vector3> corners =
                [
                    new Vector3(min.X, min.Y, min.Z),
                    new Vector3(max.X, min.Y, min.Z),
                    new Vector3(max.X, min.Y, max.Z),
                    new Vector3(min.X, min.Y, max.Z),
                    new Vector3(min.X, max.Y, min.Z),
                    new Vector3(max.X, max.Y, min.Z),
                    new Vector3(max.X, max.Y, max.Z),
                    new Vector3(min.X, max.Y, max.Z)
                ];

                Span<Vector2> screenCorners = stackalloc Vector2[8];
                for (var i = 0; i < corners.Length; ++i)
                {
                    if (_mainViewport.WorldToScreen(corners[i], out var screenPos))
                        screenCorners[i] = screenPos;
                }

                for (var i = 0; i < 4; i++)
                {
                    bgDrawList.AddLine(screenCorners[i], screenCorners[(i + 1) % 4], new MtColor(255, 0, 0, 255), 2);
                    bgDrawList.AddLine(screenCorners[i + 4], screenCorners[(i + 1) % 4 + 4], new MtColor(255, 0, 0, 255), 2);
                    bgDrawList.AddLine(screenCorners[i], screenCorners[i + 4], new MtColor(255, 0, 0, 255), 2);
                }
            }
        }
#endif

        if (!Renderer.MenuShown)
            return;

        DrawBoneManager();
        DrawPresetManager();

        if (!ImGui.Begin("Collision Editor", ImGuiWindowFlags.MenuBar))
        {
            goto Exit;
        }

#if DEBUG
        if (ImGui.CollapsingHeader("Singleton Stuff"))
        {
            if (ImGui.InputText("Singleton Name", ref _singletonName, 0x80))
            {
                var singleton = SingletonManager.GetSingleton(_singletonName);
                if (singleton is not null)
                    _imGuiDti.Object = singleton;
            }

            ImGui.InputText("Filter", ref _propFilter, 0x80);

            _imGuiDti.Draw(_propFilter);
        }
#endif

        var models = GetModels();
        if (_selectedModel is not null && !models.Contains(_selectedModel))
        {
            _selectedModel = null;
            _selectedColComponent = null;
            _selectedCollision = null;
            _selectedClid = null;
            _selectedClnd = null;
            _selectedAtk = null;
            _selectedOap = null;
        }

        if (ImGui.BeginMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                _textSize = _config.TextSize;
                _textColor = _config.TextColor.ToVector4();
                _jointRadius = _config.BoneRadius;
                _defaultJointColor = _config.DefaultBoneColor.ToVector4();
                var showBones = _settings.ShowBones;

                var showAllColliders = _settings.ShowAllColliders;
                var inactiveColliderColor = _settings.InactiveColliderColor.ToVector4();
                var drawShellColliders = _settings.DrawShellColliders;
                var activeColliderSphereColor = _settings.ActiveColliderSphereColor.ToVector4();
                var activeColliderCapsuleColor = _settings.ActiveColliderCapsuleColor.ToVector4();
                var activeColliderObbColor = _settings.ActiveColliderObbColor.ToVector4();
                var nonAttackColliderColor = _settings.NonAttackColliderColor.ToVector4();

                ImGui.DragFloat("Text Size", ref _textSize, 0.2f);
                ImGui.ColorEdit4("Text Color", ref _textColor);
                ImGui.DragFloat("Bone Radius", ref _jointRadius);
                ImGui.ColorEdit4("Default Joint Color", ref _defaultJointColor);

                ImGui.SeparatorText("Global Settings");

                ImGui.Checkbox("Show Bones", ref showBones);
                ImGui.Checkbox("Show All Colliders", ref showAllColliders);
                ImGui.Checkbox("Show Shell Colliders", ref drawShellColliders);
                ImGui.ColorEdit4("Inactive Collider Color", ref inactiveColliderColor);
                ImGui.ColorEdit4("Active Collider Sphere Color", ref activeColliderSphereColor);
                ImGui.ColorEdit4("Active Collider Capsule Color", ref activeColliderCapsuleColor);
                ImGui.ColorEdit4("Active Collider Obb Color", ref activeColliderObbColor);
                ImGui.ColorEdit4("Non-Attack Collider Color", ref nonAttackColliderColor);

                _config.TextSize = _textSize;
                _config.TextColor = MtColor.FromVector4(_textColor);
                _config.BoneRadius = _jointRadius;
                _config.DefaultBoneColor = MtColor.FromVector4(_defaultJointColor);
                _settings.ShowBones = showBones;

                _settings.ShowAllColliders = showAllColliders;
                _settings.DrawShellColliders = drawShellColliders;
                _settings.InactiveColliderColor = MtColor.FromVector4(inactiveColliderColor);
                _settings.ActiveColliderSphereColor = MtColor.FromVector4(activeColliderSphereColor);
                _settings.ActiveColliderCapsuleColor = MtColor.FromVector4(activeColliderCapsuleColor);
                _settings.ActiveColliderObbColor = MtColor.FromVector4(activeColliderObbColor);
                _settings.NonAttackColliderColor = MtColor.FromVector4(nonAttackColliderColor);

                ImGui.Separator();
                if (ImGui.Button("Save"))
                {
                    SaveSettings();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenuBar();
        }

        ImGui.DragFloat("Joint Radius", ref _jointRadius, 0.2f);

        if (ImGui.BeginCombo("Entity", _selectedModel?.ToString() ?? "None"))
        {
            foreach (var model in models)
            {
                if (ImGui.Selectable(model.ToString(), _selectedModel == model))
                {
                    if (_selectedModel != model)
                    {
                        _selectedCollision = null;
                        _selectedClid = null;
                        _selectedClnd = null;
                        _selectedAtk = null;
                        _selectedOap = null;
                    }

                    _selectedModel = model;
                    _selectedColComponent = model
                        .ComponentManager
                        .Find(_colComponentDti)?
                        .As<CollisionComponent>();
                }
            }

            ImGui.EndCombo();
        }

        if (_selectedColComponent is null)
        {
            goto Exit;
        }

        CreatePendingNodes(_selectedColComponent);

        ImGui.Separator();

        if (ImGui.BeginCombo("Collision File", _selectedCollision?.FilePath ?? "None"))
        {
            var collisions = _selectedColComponent.Resources;
            for (var i = 0; i < collisions.Length; ++i)
            {
                if (collisions[i] == 0)
                    continue;

                var collision = new ObjCollision(collisions[i]);
                if (ImGui.Selectable(collision.FilePath, _selectedCollision == collision))
                {
                    _selectedCollision = collision;
                    _selectedBankId = i;
                    _selectedClid = _selectedCollision.CollIndex;
                    _selectedClnd = _selectedCollision.CollNode;
                    _selectedAtk = _selectedCollision.AttackParam;
                    _selectedOap = _selectedCollision.ObjAppendParam;
                    _savePath = $"{collision.FilePath}.modified.{collision.FileExtension}";

                    if (_selectedClid is not null)
                    {
                        // Build CLID -> Node map
                        _nodeMap.Clear();
                        var indices = _selectedClid.Indices;
                        foreach (var node in _selectedColComponent.Nodes)
                        {
                            var clid = node.GetCollIndex();
                            if (indices.FindIndex((ref CollIndex x) => MemoryUtil.AddressOf(ref x) == clid) != -1)
                                _nodeMap[clid] = (node, false);
                        }
                    }
                }
            }

            ImGui.EndCombo();
        }

        if (_selectedClid is null)
            goto Exit;

        ImGui.Separator();

        ImGui.InputText("Save Path", ref _savePath, 260);
        ImGui.SameLine();
        if (ImGui.Button("Save"))
        {
            using var fs = MtFileStream.FromPath(_savePath, OpenMode.Write);
            if (fs is not null)
            {
                _selectedCollision!.SerializeTo(fs);
            }
            else
            {
                Log.Error($"Failed to open file '{_savePath}' for writing");
            }
        }

        ImGui.Separator();

        foreach (ref var clid in _selectedClid.Indices)
            ImGuiCollIndex(ref clid);

        Exit:
        ImGui.End();
    }

    private static List<Model> GetModels()
    {
        List<Model> entities = [];
        var player = Player.MainPlayer;
        if (player is not null)
        {
            entities.Add(player);
            var weapon = player.CurrentWeapon;
            if (weapon is not null)
                entities.Add(weapon);

            var claw = player.GetObject<Model>(0x8918);
            if (claw is not null)
                entities.Add(claw);
        }

        entities.AddRange(Monster.GetAllMonsters());

        return entities;
    }

    private void ImGuiCollIndex(ref CollIndex clid)
    {
        if (Unsafe.IsNullRef(ref clid))
        {
            ImGui.Text("Null CLID");
            return;
        }

        var name = clid.Name;
        var clidPtr = MemoryUtil.AddressOf(ref clid);
        var pushedStyle = false;

        ImGui.PushID(clidPtr);

        if (_nodeMap.TryGetValue(clidPtr, out var vp))
        {
            if (vp.node.IsActive && vp.node.Get<nint>(0x90) != 0) // Has AttackParam
            {
                pushedStyle = true;
                ImGui.PushStyleColor(ImGuiCol.Header, new MtColor(184, 78, 66, 255));
                ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new MtColor(227, 121, 109, 255));
                ImGui.PushStyleColor(ImGuiCol.HeaderActive, new MtColor(212, 54, 36, 255));
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new MtColor(184, 78, 66, 255));
                ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, new MtColor(227, 121, 109, 255));
                ImGui.PushStyleColor(ImGuiCol.FrameBgActive, new MtColor(212, 54, 36, 255));
            }
        }

        if (ImGui.Checkbox("##Visible", ref vp.visible))
        {
            if (vp is { node: null, visible: true })
            {
                Ensure.NotNull(_selectedColComponent);
                
                var node = CreateNode(_selectedColComponent, _selectedBankId, clid.UniqueId);
                if (node is not null)
                {
                    node.IsActive = false;
                    vp.node = node;

                    _nodeMap[clidPtr] = vp;
                }
                else
                {
                    ImGuiExtensions.NotificationError($"Failed to create node for CLID {clid.UniqueId}");
                }
            }
            else
            {
                _nodeMap[clidPtr] = vp;
            }
        }

        ImGui.SameLine();

        var open = ImGuiEx.CollapsingHeader(clidPtr, name != "" ? name : $"{clid.UniqueId}: No Name");

        if (pushedStyle) ImGui.PopStyleColor(6);
        if (!open)
        {
            ImGui.PopID();
            return;
        }

        if (ImGui.InputText("Name", ref name, 260))
        {
            unsafe
            {
                var namePtr = (nint)clid.NamePtr;
                _mtStringSet.Invoke(MemoryUtil.AddressOf(ref namePtr), name);
                clid.NamePtr = (SharpPluginLoader.Core.MtString*)namePtr;
            }
        }

        ImGuiExtensions.InputScalar("Link Id", ref clid.LinkId);
        ImGuiExtensions.InputScalar("Unique Id", ref clid.UniqueId);
        ImGui.Checkbox("Link Top", ref clid.LinkTop);

        ImGui.Separator();

        if (ImGui.TreeNode("CLND"))
        {
            var nodeIndex = clid.NodeIndex;
            if (nodeIndex == -1 || _selectedClnd is null)
            {
                ImGui.Text("No CLND");
            }
            else
            {
                var clnd = _selectedClnd.Nodes[nodeIndex];
                ImGuiCollNode(clnd, ref clid);
            }

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("ATK"))
        {
            var atkIndex = clid.AttackParamIndex;
            if (atkIndex == -1 || _selectedAtk is null)
            {
                ImGui.Text("No ATK");
            }
            else
            {
                var atk = _selectedAtk.AttackParams[atkIndex];
                ImGuiAttackParam(atk);
            }

            ImGui.TreePop();
        }

        ImGui.PopID();
    }

    private unsafe void ImGuiCollNode(CollNode? clnd, ref CollIndex clid)
    {
        if (clnd is null)
        {
            ImGui.Text("CLND not found");
            return;
        }

        Ensure.NotNull(_selectedColComponent);

        ImGuiExtensions.InputScalar("Flags", ref clnd.CollNodeFlags, 1, 10, "%X");
        ImGuiExtensions.InputScalar("Hit Collision Flags", ref clnd.HitCollisionFlags, 1, 10, "%X");
        ImGuiExtensions.InputScalar("Attributes", ref clnd.Attr, 1, 10, "%X");
        var useSphereCapsuleGeom = HasBit(clnd.Attr, 9);
        ImGui.Checkbox("Use Sphere/Capsule Geometry", ref useSphereCapsuleGeom);
        SetBit(ref clnd.Attr, 9, useSphereCapsuleGeom);

        var isEm = clnd.Is("cCollNodeEm");
        if (isEm)
        {
            var clndEm = clnd.As<CollNodeEm>();
            
            ImGuiExtensions.InputScalar("Base Damage Group", ref clndEm.BaseDamageGroup);
            ImGuiExtensions.InputScalar("Parts Damage Group", ref clndEm.PartsDamageGroup);
            ImGuiExtensions.InputScalar("Base Damage Group For Changed", ref clndEm.BaseDamageGroupForChanged);
            ImGuiExtensions.InputScalar("Parts Damage Group For Changed", ref clndEm.PartsDamageGroupForChanged);
            ImGuiExtensions.InputScalar("Enemy Attr", ref clndEm.EmAttr, 1, 16, "%X");
            ImGuiExtensions.InputScalar("Ride Parts", ref clndEm.RideParts);
            ImGuiExtensions.InputScalar("Parts Group", ref clndEm.PartsGroup);
            ImGuiExtensions.InputScalar("Parts Tag", ref clndEm.PartsTag);
        }

        var clgm = clnd.Geometry;
        if (clgm is null)
        {
            ImGui.Text("No Geometry");
            return;
        }

        ImGui.Separator();

        if (ImGui.CollapsingHeader("Geometry"))
        {
            if (ImGui.Button("Add New Geometry"))
            {
                var geom = _collGeomDti.CreateInstance<MtObject>();
                MemoryUtil.AsRef((CollGeom*)geom.Instance).Index = (ushort)clgm.Geometries.Length;

                try
                {
                    if (geom.Instance != 0)
                    {
                        clgm.GetGeometryArray().Push(geom);
                        var clidPtr = MemoryUtil.AddressOf(ref clid);
                        
                        if (_nodeMap.Remove(clidPtr))
                            _nodesToCreate.Enqueue((_selectedBankId, clid.UniqueId, clidPtr));

                        _selectedColComponent.Nodes
                            .Find(node => node.GetCollIndex() == clidPtr)?
                            .RequestKill();
                    }
                    else
                    {
                        Log.Error("Failed to create new geometry");
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }

            foreach (ref var geom in clgm.Geometries)
            {
                var addr = MemoryUtil.AddressOf(ref geom);
                if (ImGui.TreeNode(addr, $"{geom.Index} | Use: {geom.Use} | {geom.Shape} | Joints:[{geom.Joint0},{geom.Joint1}]"))
                {
                    ImGuiCollGeom(ref geom);
                    ImGui.TreePop();
                }
            }
        }
    }

    private void ImGuiCollGeom(ref CollGeom clgm)
    {
        if (Unsafe.IsNullRef(ref clgm))
        {
            ImGui.Text("Null CLGM");
            return;
        }

        ImGui.Checkbox("Use", ref clgm.Use);
        var shape = (int)clgm.Shape;
        ImGui.Combo("Shape", ref shape, _collGeomShapes, _collGeomShapes.Length);
        clgm.Shape = (CollGeomShape)shape;

        ImGui.Separator();

        ImGui.DragFloat("Radius", ref clgm.Radius);
        ImGui.DragFloat3("Extents", ref clgm.Extent);
        ImGui.DragFloat4("Offset 1", ref clgm.Offset0);
        ImGui.DragFloat4("Offset 2", ref clgm.Offset1);
        ImGui.DragFloat2("Angle 1", ref clgm.Angle0);
        ImGui.DragFloat2("Angle 2", ref clgm.Angle1);

        ImGui.NewLine();

        ImGuiExtensions.InputScalar("Joint 1", ref clgm.Joint0);
        ImGuiExtensions.InputScalar("Joint 2", ref clgm.Joint1);

        ImGui.NewLine();

        ImGuiExtensions.InputScalar("Priority", ref clgm.Priority);
        ImGuiExtensions.InputScalar("Region", ref clgm.Region);
        ImGuiExtensions.InputScalar("Layer", ref clgm.Layer);
        ImGuiExtensions.InputScalar("Range Check Base Joint", ref clgm.RangeCheckBaseJoint);

        ImGui.NewLine();

        ImGuiExtensions.InputScalar("Option", ref clgm.Option, format: "%X");
        ImGuiExtensions.InputScalar("Scale Option", ref clgm.ScaleOption, format: "%X");

        uint flags = clgm.Option;
        ImGui.CheckboxFlags("Capsule: Use Radius as Offset", ref flags, 0x1);
        ImGui.CheckboxFlags("Capsule: Use same XZ Pos", ref flags, 0x40);
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("If checked, the capsule will use the same XZ position for both joints.");
            ImGui.Text("This effectively results in the capsule facing perfectly upwards.");
            ImGui.EndTooltip();
        }

        ImGui.CheckboxFlags("Sphere: J1-J2 Average Pos", ref flags, 0x2);
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("If checked, the sphere will be centered between the two joints.");
            ImGui.EndTooltip();
        }

        ImGui.CheckboxFlags("Sphere: J1-Child Average Pos", ref flags, 0x4);
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("If checked, the sphere will be centered between joint 1 and its child joint.");
            ImGui.EndTooltip();
        }

        ImGui.CheckboxFlags("No Scaling", ref flags, 0x8);
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("If checked, the geometry will not be scaled.");
            ImGui.EndTooltip();
        }

        clgm.Option = (byte)(flags & 0xFF);
        
        ImGui.NewLine();

        flags = clgm.ScaleOption;

        ImGui.CheckboxFlags("Scale by Model X", ref flags, 0x1);
        ScaleTooltip('X');
        ImGui.CheckboxFlags("Scale by Model Y", ref flags, 0x2);
        ScaleTooltip('Y');
        ImGui.CheckboxFlags("Scale by Model Z", ref flags, 0x4);
        ScaleTooltip('Z');

        if ((flags & 0b10000111) == 0)
            ImGui.Text("Geometry will be scaled by X axis");

        ImGui.CheckboxFlags("No Scaling 2", ref flags, 0x80);

        clgm.ScaleOption = (byte)(flags & 0xFF);

        return;

        static void ScaleTooltip(char axis)
        {
            if (ImGui.BeginItemTooltip())
            {
                ImGui.Text($"If checked, the geometry will be scaled by the model's {axis} scale.");
                ImGui.EndTooltip();
            }
        }
    }

    private void ImGuiAttackParam(AttackParam? atk)
    {
        if (atk is null)
        {
            ImGui.Text("ATK not found");
            return;
        }

        ImGuiExtensions.InputScalar("Target Hit Group", ref atk.TargetHitGroup);

        var impactType = (int)atk.ImpactType;
        ImGui.Combo("Impact Type", ref impactType, _impactTypes, _impactTypes.Length);
        atk.ImpactType = (ImpactType)impactType;

        ImGui.InputFloat("Motion Value", ref atk.Attack);
        ImGui.InputFloat("Fixed Damage", ref atk.FixedAttack);
        ImGui.InputFloat("Part Break Rate", ref atk.PartBreakRate);
        
        var elementType = (int)atk.ElementType;
        ImGui.Combo("Element Type", ref elementType, _elementTypes, _elementTypes.Length);
        atk.ElementType = (ElementType)elementType;

        ImGuiExtensions.InputScalar("Element Level", ref atk.AttackAttrLevel);
        ImGui.InputFloat("Element Damage", ref atk.AttackAttrDamage);

        ImGuiExtensions.InputScalar("Status Level", ref atk.ConditionLevel);
    	ImGui.InputFloat("Status Rise Rate", ref atk.BadConditionRiseRate);

        ImGui.Separator();

        ImGui.InputFloat("Poison Damage", ref atk.PoisonDamage);
        ImGui.InputFloat("Deadly Poison Damage", ref atk.DeadlyPoisonDamage);
        ImGui.InputFloat("Paralysis Damage", ref atk.ParalysisDamage);
        ImGui.InputFloat("Sleep Damage", ref atk.SleepDamage);
        ImGui.InputFloat("Blast Damage", ref atk.BlastDamage);
        ImGui.InputFloat("Myxomy Blast Damage", ref atk.MyxomyBlastDamage);
        ImGui.InputFloat("Stun Damage", ref atk.StunDamage);
        ImGui.InputFloat("Exhaust Damage", ref atk.ExhaustDamage);
        ImGui.InputFloat("Bleed Damage", ref atk.BleedDamage);
        ImGui.InputFloat("Syouki Damage", ref atk.SyoukiDamage);

        ImGui.Separator();

        ImGui.Checkbox("Defense Down S", ref atk.DefenseDownS);
        ImGui.Checkbox("Defense Down L", ref atk.DefenseDownL);
        ImGui.Checkbox("Element Resist Down S", ref atk.ElementResistDownS);
        ImGui.Checkbox("Element Resist Down L", ref atk.ElementResistDownL);
        
        ImGui.Separator();

        ImGui.InputFloat("Stage Damage L", ref atk.StageDamageL);
        ImGui.InputFloat("Stage Damage M", ref atk.StageDamageM);
        ImGui.InputFloat("Stage Damage S", ref atk.StageDamageS);
        ImGui.InputFloat("Stage Damage SS", ref atk.StageDamageSS);
        ImGui.InputFloat("Stage Damage XS", ref atk.StageDamageXS);
        ImGui.InputFloat("Stage Damage Accumulate", ref atk.StageDamageAccumulate);

        ImGui.Separator();

        var knockbackType = (int)atk.KnockbackType;
        ImGui.Combo("Knockback Type", ref knockbackType, _knockbackTypes, _knockbackTypes.Length);
        atk.KnockbackType = (KnockbackType)knockbackType;

        ImGuiExtensions.InputScalar("Knockback Level", ref atk.KnockbackLevel);
        ImGuiExtensions.InputScalar("Damage Angle", ref atk.DamageAngle);
        ImGui.InputFloat("Power", ref atk.Power);
        
        var guardType = (int)atk.GuardType;
        ImGui.Combo("Guard Type", ref guardType, _guardTypes, _guardTypes.Length);
        atk.GuardType = (GuardType)guardType;

        ImGui.Separator();

        ImGui.Checkbox("Is Multi Hit", ref atk.IsMultiHit);
        ImGui.InputFloat("Tick Interval", ref atk.MultiHitTickInterval);
        
        ImGui.Separator();

        ImGuiExtensions.InputScalar("Hit Effect", ref atk.HitEffect);
        ImGuiExtensions.InputScalar("Disable Hit Effect", ref atk.DisableHitEffect);
        ImGui.InputFloat("Hit Effect Angle", ref atk.HitEffectAngle);
        ImGui.InputFloat("Hit Effect Angle X", ref atk.HitEffectAngleX);
        ImGuiExtensions.InputScalar("Player Skill Affect", ref atk.PlSkillAffect);
        ImGui.InputFloat("Custom 1", ref atk.Custom1);
        ImGui.InputFloat("Custom 2", ref atk.Custom2);
        ImGuiExtensions.InputScalar("Damage UI Type", ref atk.DamageUiType);

        switch (atk)
        {
            case AttackParamEm atkEm:
                ImGuiExtensions.InputScalar("Enemy Flinch Type", ref atkEm.EmFlinchType);
                break;
            case AttackParamPl atkPl:
                ImGui.InputFloat("Tenderize Damage", ref atkPl.TenderizeDamage);
                ImGui.InputFloat("Element Motion Value", ref atkPl.ElementMotionValue);
                ImGui.InputFloat("Status Motion Value", ref atkPl.StatusMotionValue);
                ImGui.InputFloat("Mount Damage", ref atkPl.MountDamage);
                ImGuiHitDelay(ref atkPl.HitDelayS, "Hit Delay S");
                ImGuiHitDelay(ref atkPl.HitDelayL, "Hit Delay L");
                ImGui.Checkbox("Use Minds Eye", ref atkPl.UseMindsEye);
                ImGui.InputFloat("Weapon Custom 1", ref atkPl.WeaponCustom1);
                ImGui.InputFloat("Weapon Custom 2", ref atkPl.WeaponCustom2);
                ImGui.InputFloat("Weapon Custom 3", ref atkPl.WeaponCustom3);
                break;
            case AttackParamPlShell:
            case AttackParamOt:
            case AttackParam:
                break;
        }
    }

    private void CreatePendingNodes(CollisionComponent colComponent)
    {
        while (_nodesToCreate.Count > 0)
        {
            var (bankId, uniqueId, clidPtr) = _nodesToCreate.Dequeue();
            var node = CreateNode(colComponent, bankId, uniqueId);
            if (node is not null)
            {
                node.IsActive = false;
                _nodeMap[clidPtr] = (node, true);
            }
        }
    }

    private unsafe CollisionComponent.Node? CreateNode(CollisionComponent colComponent, int bankId, uint uniqueId)
    {
        _createNodeFromUid.Invoke(
            colComponent.Instance,
            bankId,
            uniqueId,
            true,
            false
        );
        
        return colComponent.Nodes.Find(x => x.GetCollIndexObj().UniqueId == uniqueId);
    }

    private static void ImGuiHitDelay(ref HitDelay hitDelay, string name)
    {
        if (Unsafe.IsNullRef(ref hitDelay))
        {
            ImGui.Text("Null HitDelay");
            return;
        }

        if (!ImGui.TreeNode(name))
            return;

        ImGui.InputFloat("Timescale", ref hitDelay.Timescale);
        ImGui.InputFloat("Frame Advance", ref hitDelay.FrameAdvance);
        ImGui.InputFloat("Scaled Delay", ref hitDelay.ScaledDelay);
        ImGui.InputFloat("Scale Transition Delay", ref hitDelay.ScaleTransitionDelay);
        ImGui.InputFloat("Unscaled Delay", ref hitDelay.UnscaledDelay);

        ImGui.TreePop();
    }

    private static bool HasBit(uint value, int bit) => ((value >> bit) & 1) != 0;
    private static void SetBit(ref uint value, int bit, bool set) => value = (value & ~(1u << bit)) | (set ? 1u << bit : 0);

    private static nint FindFunction(string patternString)
    {
        var pattern = Pattern.FromString(patternString);
        return PatternScanner.FindFirst(pattern);
    }
}
