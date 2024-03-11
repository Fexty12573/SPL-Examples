using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace AssetBrowser;

using ColorPair = (ImGuiCol Index, Vector4 Color);
using StylePairF = (ImGuiStyleVar Var, float Value);
using StylePairV = (ImGuiStyleVar Var, Vector2 Value);

internal readonly struct ScopedStyle : IDisposable
{
    public ScopedStyle(ImGuiStyleVar style, Vector2 value) => ImGui.PushStyleVar(style, value);
    public ScopedStyle(ImGuiStyleVar style, float value) => ImGui.PushStyleVar(style, value);
    public void Dispose() => ImGui.PopStyleVar();
}

internal readonly struct ScopedColor : IDisposable
{
    public ScopedColor(ImGuiCol color, Vector4 value) => ImGui.PushStyleColor(color, value);
    public ScopedColor(ImGuiCol color, uint value) => ImGui.PushStyleColor(color, value);
    public void Dispose() => ImGui.PopStyleColor();
}

internal readonly struct ScopedId : IDisposable
{
    public ScopedId(int id) => ImGui.PushID(id);
    public ScopedId(nint id) => ImGui.PushID(id);
    public ScopedId(string id) => ImGui.PushID(id);
    public void Dispose() => ImGui.PopID();
}

internal readonly struct ScopedColorStack : IDisposable
{
    private readonly int _count;

    public ScopedColorStack(ColorPair firstPair, params ColorPair[] pairs)
    {
        _count = pairs.Length + 1;
        ImGui.PushStyleColor(firstPair.Index, firstPair.Color);
        foreach (var (index, color) in pairs)
            ImGui.PushStyleColor(index, color);
    }

    public void Dispose() => ImGui.PopStyleColor(_count);
}

internal readonly struct ScopedStyleStack : IDisposable
{
    private readonly int _count;

    public ScopedStyleStack(StylePairF firstPair, params StylePairF[] pairs)
    {
        _count = pairs.Length + 1;
        ImGui.PushStyleVar(firstPair.Var, firstPair.Value);
        foreach (var (var, value) in pairs)
            ImGui.PushStyleVar(var, value);
    }

    public ScopedStyleStack(StylePairV firstPair, params StylePairV[] pairs)
    {
        _count = pairs.Length + 1;
        ImGui.PushStyleVar(firstPair.Var, firstPair.Value);
        foreach (var (var, value) in pairs)
            ImGui.PushStyleVar(var, value);
    }

    public void Dispose() => ImGui.PopStyleVar(_count);
}
