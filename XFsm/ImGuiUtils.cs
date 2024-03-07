using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;

namespace XFsm;

internal readonly struct ScopedId : IDisposable
{
    public ScopedId(int id) => ImGui.PushID(id);
    public ScopedId(string id) => ImGui.PushID(id);
    public ScopedId(nint id) => ImGui.PushID(id);

    public void Dispose() => ImGui.PopID();
}
