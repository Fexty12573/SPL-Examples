
using ImGuiNET;
using SharpPluginLoader.Core.Rendering;

namespace ColEditor;

public partial class Plugin
{
    private readonly HashSet<byte> _highlightedBones = [];
    private byte _selectedBone;
    private readonly List<byte> _bonesToRemove = [];

    public void InitializeBoneManager()
    {
        _highlightedBones.EnsureCapacity(255);
        _bonesToRemove.EnsureCapacity(255);
    }

    public void DrawBoneManager()
    {
        if (_selectedModel is null)
            return;

        if (ImGui.Begin("Bone Manager"))
        {
            ImGuiExtensions.InputScalar("Bone ID", ref _selectedBone);
            ImGui.SameLine();
            if (ImGui.Button("Add Bone"))
            {
                _highlightedBones.Add(_selectedBone);
            }

            ImGui.Separator();
            ImGui.Text("Highlighted Bones (Click to Remove)");

            _bonesToRemove.Clear();
            foreach (var bone in _highlightedBones)
            {
                if (ImGui.Selectable($"Bone {bone}", false, ImGuiSelectableFlags.SpanAllColumns))
                    _bonesToRemove.Add(bone);
            }

            foreach (var bone in _bonesToRemove)
                _highlightedBones.Remove(bone);
        }

        ImGui.End();
    }
}
