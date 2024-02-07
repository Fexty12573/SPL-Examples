using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace ColEditor;

internal static class ImGuiEx
{
    public static bool CollapsingHeader(nint id, string label)
    {
        var open = ImGui.CollapsingHeader(id.ToString());
        ImGui.SameLine();
        ImGui.Text(label);

        return open;
    }
}
