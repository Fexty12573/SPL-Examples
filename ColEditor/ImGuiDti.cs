
using System.Numerics;
using ImGuiNET;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Rendering;

namespace ColEditor;

public class ImGuiDti
{
    private readonly List<Property> _properties = [];
    private MtObject _object;

    public MtObject Object
    {
        get => _object;
        set
        {
            _object = value;
            _properties.Clear();

            try
            {
                var propList = _object.GetProperties();
                foreach (var prop in propList)
                {
                    if (prop.IsArray || prop.IsProperty)
                        continue;

                    _properties.Add(new Property
                    {
                        Name = prop.Name,
                        Type = prop.Type,
                        Get = prop.Get
                    });
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw;
            }
        }
    }

    public ImGuiDti(MtObject obj)
    {
        _object = obj;
        Object = obj;
    }

    public void Draw(string filter)
    {
        foreach (var prop in _properties)
        {
            if (!string.IsNullOrEmpty(filter) && !prop.Name.Contains(filter))
                continue;

            switch (prop.Type)
            {
                case PropType.Bool:
                    ImGui.Checkbox(prop.Name, ref MemoryUtil.GetRef<bool>(prop.Get));
                    break;
                case PropType.U8:
                    ImGuiExtensions.InputScalar(prop.Name, ref MemoryUtil.GetRef<byte>(prop.Get));
                    break;
                case PropType.U16:
                    ImGuiExtensions.InputScalar(prop.Name, ref MemoryUtil.GetRef<ushort>(prop.Get));
                    break;
                case PropType.U32:
                    ImGuiExtensions.InputScalar(prop.Name, ref MemoryUtil.GetRef<uint>(prop.Get));
                    break;
                case PropType.U64:
                    ImGui.InputScalar(prop.Name, ImGuiDataType.U64, prop.Get);
                    break;
                case PropType.S8:
                    ImGuiExtensions.InputScalar(prop.Name, ref MemoryUtil.GetRef<sbyte>(prop.Get));
                    break;
                case PropType.S16:
                    ImGuiExtensions.InputScalar(prop.Name, ref MemoryUtil.GetRef<short>(prop.Get));
                    break;
                case PropType.S32:
                    ImGuiExtensions.InputScalar(prop.Name, ref MemoryUtil.GetRef<int>(prop.Get));
                    break;
                case PropType.S64:
                    ImGui.InputScalar(prop.Name, ImGuiDataType.S64, prop.Get);
                    break;
                case PropType.F32:
                    ImGui.DragFloat(prop.Name, ref MemoryUtil.GetRef<float>(prop.Get));
                    break;
                case PropType.F64:
                    ImGui.DragScalar(prop.Name, ImGuiDataType.Double, prop.Get);
                    break;
                case PropType.Color:
                    ImGuiExtensions.InputScalar(prop.Name, ref MemoryUtil.GetRef<uint>(prop.Get), format: "%08X");
                    break;
                case PropType.Matrix44:
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get));
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get + 0x10));
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get + 0x20));
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get + 0x30));
                    break;
                case PropType.Vector3:
                    ImGui.DragFloat3(prop.Name, ref MemoryUtil.GetRef<Vector3>(prop.Get));
                    break;
                case PropType.Vector4:
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get));
                    break;
                case PropType.Quaternion:
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get));
                    break;
                case PropType.Float2:
                    ImGui.DragFloat2(prop.Name, ref MemoryUtil.GetRef<Vector2>(prop.Get));
                    break;
                case PropType.Float3:
                    ImGui.DragFloat3(prop.Name, ref MemoryUtil.GetRef<Vector3>(prop.Get));
                    break;
                case PropType.Float4:
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get));
                    break;
                case PropType.Float3X3:
                    ImGui.DragFloat3(prop.Name, ref MemoryUtil.GetRef<Vector3>(prop.Get));
                    ImGui.DragFloat3(prop.Name, ref MemoryUtil.GetRef<Vector3>(prop.Get + 0xC));
                    ImGui.DragFloat3(prop.Name, ref MemoryUtil.GetRef<Vector3>(prop.Get + 0x18));
                    break;
                case PropType.Float4X3:
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get));
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get + 0x10));
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get + 0x20));
                    break;
                case PropType.Float4X4:
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get));
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get + 0x10));
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get + 0x20));
                    ImGui.DragFloat4(prop.Name, ref MemoryUtil.GetRef<Vector4>(prop.Get + 0x30));
                    break;
                case PropType.Vector2:
                    ImGui.DragFloat2(prop.Name, ref MemoryUtil.GetRef<Vector2>(prop.Get));
                    break;
                case PropType.Matrix33:
                    ImGui.DragFloat3(prop.Name, ref MemoryUtil.GetRef<Vector3>(prop.Get));
                    ImGui.DragFloat3(prop.Name, ref MemoryUtil.GetRef<Vector3>(prop.Get + 0xC));
                    ImGui.DragFloat3(prop.Name, ref MemoryUtil.GetRef<Vector3>(prop.Get + 0x18));
                    break;
            }
        }
    }


    private readonly struct Property
    {
        public required string Name { get; init; }
        public required PropType Type { get; init; }
        public required nint Get { get; init; }
    }
}
