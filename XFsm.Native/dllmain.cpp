
#include "SPL/InternalCall.h"
#include "renderdoc_app.h"

#include <graphviz/gvc.h>

#define IMGUI_DISABLE_OBSOLETE_KEYIO
#define IMGUI_DISABLE_OBSOLETE_FUNCTIONS
#define IMGUI_DEFINE_MATH_OPERATORS

#include <format>
#include <functional>
#include <Windows.h>

#include <imgui.h>
#include <imgui_internal.h>
#include <imgui-node-editor/imgui_node_editor.h>
#include <imgui-node-editor/imgui_node_editor_internal.h>
#include <imgui-node-editor/examples/blueprints-example/utilities/widgets.h>

namespace SPLNative = SharpPluginLoader::Native;

namespace ax::NodeEditor {
void ShowStyleEditor(bool* show = nullptr);
}

RENDERDOC_API_1_6_0* rdoc_api = nullptr;
bool inject_render_doc(const wchar_t* dll_path);
void render_doc_trigger_capture() {
    if (rdoc_api) {
        rdoc_api->TriggerCapture();
    }
}

struct XFsmGvcNode {
    int Id;
    char* Name;
    float X, Y;
};

struct XFsmGvcEdge {
    int SourceId;
    int TargetId;
};

void layout_nodes(GVC_t* gv, XFsmGvcNode* nodes, int node_count, const XFsmGvcEdge* edges, int edge_count) {
    Agdesc_t desc{};
    desc.directed = true;
    desc.maingraph = true;
    agseterr(AGWARN);
    graph_t* g = agopen((char*)"G", desc, nullptr);

    std::unordered_map<int, Agnode_t*> node_map;

    for (int i = 0; i < node_count; i++) {
        
        const auto node = agnode(g, nodes[i].Name, 1);
        node_map[nodes[i].Id] = node;
    }

    for (int i = 0; i < edge_count; i++) {
        auto edge = edges[i];
        agedge(g, node_map[edge.SourceId], node_map[edge.TargetId], nullptr, 1);
    }

    const auto result = gvLayout(gv, g, "dot");
    if (result != 0) {
        agclose(g);
        return;
    }

    for (int i = 0; i < node_count; i++) {
        const auto node = node_map[nodes[i].Id];
        nodes[i].X = ND_coord(node).x;
        nodes[i].Y = ND_coord(node).y;
    }

    agclose(g);
}

SPL_INTERNAL_CALL int get_internal_call_count() {
    return 137;
}

SPL_INTERNAL_CALL void collect_internal_calls(SPLNative::InternalCall* icalls) {
    using ax::NodeEditor::PinId;
    using ax::NodeEditor::NodeId;

    IMGUI_CHECKVERSION();

    *icalls++ = { "RenderDocTriggerCapture", render_doc_trigger_capture };
    *icalls++ = { "InjectRenderDoc", inject_render_doc };
    *icalls++ = { "ShowStyleEditor", ax::NodeEditor::ShowStyleEditor };
    *icalls++ = { "SetCurrentImGuiContext", ImGui::SetCurrentContext };
    *icalls++ = { "SetCurrentEditor", ax::NodeEditor::SetCurrentEditor };
    *icalls++ = { "GetCurrentEditor", ax::NodeEditor::GetCurrentEditor };
    *icalls++ = { "CreateEditor", ax::NodeEditor::CreateEditor };
    *icalls++ = { "DestroyEditor", ax::NodeEditor::DestroyEditor };
    *icalls++ = { "GetConfig", ax::NodeEditor::GetConfig };
    *icalls++ = { "GetStyle", ax::NodeEditor::GetStyle };
    *icalls++ = { "GetStyleColorName", ax::NodeEditor::GetStyleColorName };
    *icalls++ = { "PushStyleColor", ax::NodeEditor::PushStyleColor };
    *icalls++ = { "PopStyleColor", ax::NodeEditor::PopStyleColor };
    *icalls++ = { "PushStyleVarFloat", static_cast<void(*)(ax::NodeEditor::StyleVar, float)>(ax::NodeEditor::PushStyleVar) };
    *icalls++ = { "PushStyleVarVec2", static_cast<void(*)(ax::NodeEditor::StyleVar, const ImVec2&)>(ax::NodeEditor::PushStyleVar) };
    *icalls++ = { "PushStyleVarVec4", static_cast<void(*)(ax::NodeEditor::StyleVar, const ImVec4&)>(ax::NodeEditor::PushStyleVar) };
    *icalls++ = { "PopStyleVar", ax::NodeEditor::PopStyleVar };
    *icalls++ = { "Begin", ax::NodeEditor::Begin };
    *icalls++ = { "End", ax::NodeEditor::End };
    *icalls++ = { "BeginNode", ax::NodeEditor::BeginNode };
    *icalls++ = { "BeginPin", ax::NodeEditor::BeginPin };
    *icalls++ = { "PinRect", ax::NodeEditor::PinRect };
    *icalls++ = { "PinPivotRect", ax::NodeEditor::PinPivotRect };
    *icalls++ = { "PinPivotSize", ax::NodeEditor::PinPivotSize };
    *icalls++ = { "PinPivotScale", ax::NodeEditor::PinPivotScale };
    *icalls++ = { "PinPivotAlignment", ax::NodeEditor::PinPivotAlignment };
    *icalls++ = { "EndPin", ax::NodeEditor::EndPin };
    *icalls++ = { "Group", ax::NodeEditor::Group };
    *icalls++ = { "EndNode", ax::NodeEditor::EndNode };
    *icalls++ = { "BeginGroupHint", ax::NodeEditor::BeginGroupHint };
    *icalls++ = { "GetGroupMin", ax::NodeEditor::GetGroupMin };
    *icalls++ = { "GetGroupMax", ax::NodeEditor::GetGroupMax };
    *icalls++ = { "GetHintForegroundDrawList", ax::NodeEditor::GetHintForegroundDrawList };
    *icalls++ = { "GetHintBackgroundDrawList", ax::NodeEditor::GetHintBackgroundDrawList };
    *icalls++ = { "EndGroupHint", ax::NodeEditor::EndGroupHint };
    *icalls++ = { "GetNodeBackgroundDrawList", ax::NodeEditor::GetNodeBackgroundDrawList };
    *icalls++ = { "Link", ax::NodeEditor::Link };
    *icalls++ = { "Flow", ax::NodeEditor::Flow };
    *icalls++ = { "BeginCreate", ax::NodeEditor::BeginCreate };
    *icalls++ = { "QueryNewLink", static_cast<bool(*)(PinId*, PinId*)>(ax::NodeEditor::QueryNewLink) };
    *icalls++ = { "QueryNewLinkEx", static_cast<bool(*)(PinId*, PinId*, const ImVec4&, float)>(ax::NodeEditor::QueryNewLink) };
    *icalls++ = { "QueryNewNode", static_cast<bool(*)(PinId*)>(ax::NodeEditor::QueryNewNode) };
    *icalls++ = { "QueryNewNodeEx", static_cast<bool(*)(PinId*, const ImVec4&, float)>(ax::NodeEditor::QueryNewNode) };
    *icalls++ = { "AcceptNewItem", static_cast<bool(*)()>(ax::NodeEditor::AcceptNewItem) };
    *icalls++ = { "AcceptNewItemEx", static_cast<bool(*)(const ImVec4&, float)>(ax::NodeEditor::AcceptNewItem) };
    *icalls++ = { "RejectNewItem", static_cast<void(*)()>(ax::NodeEditor::RejectNewItem) };
    *icalls++ = { "RejectNewItemEx", static_cast<void(*)(const ImVec4&, float)>(ax::NodeEditor::RejectNewItem) };
    *icalls++ = { "EndCreate", ax::NodeEditor::EndCreate };
    *icalls++ = { "BeginDelete", ax::NodeEditor::BeginDelete };
    *icalls++ = { "QueryDeletedLink", ax::NodeEditor::QueryDeletedLink };
    *icalls++ = { "QueryDeletedNode", ax::NodeEditor::QueryDeletedNode };
    *icalls++ = { "AcceptDeletedItem", ax::NodeEditor::AcceptDeletedItem };
    *icalls++ = { "RejectDeletedItem", ax::NodeEditor::RejectDeletedItem };
    *icalls++ = { "EndDelete", ax::NodeEditor::EndDelete };
    *icalls++ = { "SetNodePosition", ax::NodeEditor::SetNodePosition };
    *icalls++ = { "SetGroupSize", ax::NodeEditor::SetGroupSize };
    *icalls++ = { "GetNodePosition", ax::NodeEditor::GetNodePosition };
    *icalls++ = { "GetNodeSize", ax::NodeEditor::GetNodeSize };
    *icalls++ = { "CenterNodeOnScreen", ax::NodeEditor::CenterNodeOnScreen };
    *icalls++ = { "SetNodeZPosition", ax::NodeEditor::SetNodeZPosition };
    *icalls++ = { "GetNodeZPosition", ax::NodeEditor::GetNodeZPosition };
    *icalls++ = { "RestoreNodeState", ax::NodeEditor::RestoreNodeState };
    *icalls++ = { "Suspend", ax::NodeEditor::Suspend };
    *icalls++ = { "Resume", ax::NodeEditor::Resume };
    *icalls++ = { "IsSuspended", ax::NodeEditor::IsSuspended };
    *icalls++ = { "IsActive", ax::NodeEditor::IsActive };
    *icalls++ = { "HasSelectionChanged", ax::NodeEditor::HasSelectionChanged };
    *icalls++ = { "GetSelectedObjectCount", ax::NodeEditor::GetSelectedObjectCount };
    *icalls++ = { "GetSelectedNodes", ax::NodeEditor::GetSelectedNodes };
    *icalls++ = { "GetSelectedLinks", ax::NodeEditor::GetSelectedLinks };
    *icalls++ = { "IsNodeSelected", ax::NodeEditor::IsNodeSelected };
    *icalls++ = { "IsLinkSelected", ax::NodeEditor::IsLinkSelected };
    *icalls++ = { "ClearSelection", ax::NodeEditor::ClearSelection };
    *icalls++ = { "SelectNode", ax::NodeEditor::SelectNode };
    *icalls++ = { "SelectLink", ax::NodeEditor::SelectLink };
    *icalls++ = { "DeselectNode", ax::NodeEditor::DeselectNode };
    *icalls++ = { "DeselectLink", ax::NodeEditor::DeselectLink };
    *icalls++ = { "DeleteNode", ax::NodeEditor::DeleteNode };
    *icalls++ = { "DeleteLink", ax::NodeEditor::DeleteLink };
    *icalls++ = { "PinHasAnyLinks", static_cast<bool(*)(PinId)>(ax::NodeEditor::HasAnyLinks) }; 
    *icalls++ = { "NodeHasAnyLinks", static_cast<bool(*)(NodeId)>(ax::NodeEditor::HasAnyLinks) }; 
    *icalls++ = { "PinBreakLinks", static_cast<int(*)(PinId)>(ax::NodeEditor::BreakLinks) };
    *icalls++ = { "NodeBreakLinks", static_cast<int(*)(NodeId)>(ax::NodeEditor::BreakLinks) };
    *icalls++ = { "NavigateToContent", ax::NodeEditor::NavigateToContent };
    *icalls++ = { "NavigateToSelection", ax::NodeEditor::NavigateToSelection };
    *icalls++ = { "ShowNodeContextMenu", ax::NodeEditor::ShowNodeContextMenu };
    *icalls++ = { "ShowPinContextMenu", ax::NodeEditor::ShowPinContextMenu };
    *icalls++ = { "ShowLinkContextMenu", ax::NodeEditor::ShowLinkContextMenu };
    *icalls++ = { "ShowBackgroundContextMenu", ax::NodeEditor::ShowBackgroundContextMenu };
    *icalls++ = { "EnableShortcuts", ax::NodeEditor::EnableShortcuts };
    *icalls++ = { "AreShortcutsEnabled", ax::NodeEditor::AreShortcutsEnabled };
    *icalls++ = { "BeginShortcut", ax::NodeEditor::BeginShortcut };
    *icalls++ = { "AcceptCut", ax::NodeEditor::AcceptCut };
    *icalls++ = { "AcceptCopy", ax::NodeEditor::AcceptCopy };
    *icalls++ = { "AcceptPaste", ax::NodeEditor::AcceptPaste };
    *icalls++ = { "AcceptDuplicate", ax::NodeEditor::AcceptDuplicate };
    *icalls++ = { "AcceptCreateNode", ax::NodeEditor::AcceptCreateNode };
    *icalls++ = { "GetActionContextSize", ax::NodeEditor::GetActionContextSize };
    *icalls++ = { "GetActionContextNodes", ax::NodeEditor::GetActionContextNodes };
    *icalls++ = { "GetActionContextLinks", ax::NodeEditor::GetActionContextLinks };
    *icalls++ = { "EndShortcut", ax::NodeEditor::EndShortcut };
    *icalls++ = { "GetCurrentZoom", ax::NodeEditor::GetCurrentZoom };
    *icalls++ = { "GetHoveredNode", ax::NodeEditor::GetHoveredNode };
    *icalls++ = { "GetHoveredPin", ax::NodeEditor::GetHoveredPin };
    *icalls++ = { "GetHoveredLink", ax::NodeEditor::GetHoveredLink };
    *icalls++ = { "GetDoubleClickedNode", ax::NodeEditor::GetDoubleClickedNode };
    *icalls++ = { "GetDoubleClickedPin", ax::NodeEditor::GetDoubleClickedPin };
    *icalls++ = { "GetDoubleClickedLink", ax::NodeEditor::GetDoubleClickedLink };
    *icalls++ = { "IsBackgroundClicked", ax::NodeEditor::IsBackgroundClicked };
    *icalls++ = { "IsBackgroundDoubleClicked", ax::NodeEditor::IsBackgroundDoubleClicked };
    *icalls++ = { "GetBackgroundClickButtonIndex", ax::NodeEditor::GetBackgroundClickButtonIndex };
    *icalls++ = { "GetBackgroundDoubleClickButtonIndex", ax::NodeEditor::GetBackgroundDoubleClickButtonIndex };
    *icalls++ = { "GetLinkPins", ax::NodeEditor::GetLinkPins };
    *icalls++ = { "PinHadAnyLinks", ax::NodeEditor::PinHadAnyLinks };
    *icalls++ = { "GetScreenSize", ax::NodeEditor::GetScreenSize };
    *icalls++ = { "ScreenToCanvas", ax::NodeEditor::ScreenToCanvas };
    *icalls++ = { "CanvasToScreen", ax::NodeEditor::CanvasToScreen };
    *icalls++ = { "GetNodeCount", ax::NodeEditor::GetNodeCount };
    *icalls++ = { "GetOrderedNodeIds", ax::NodeEditor::GetOrderedNodeIds };
    *icalls++ = { "Icon", ax::Widgets::Icon };
    *icalls++ = { "DockBuilderDockWindow", ImGui::DockBuilderDockWindow };
    *icalls++ = { "DockBuilderGetNode", ImGui::DockBuilderGetNode };
    *icalls++ = { "DockBuilderGetCentralNode", ImGui::DockBuilderGetCentralNode };
    *icalls++ = { "DockBuilderAddNode", ImGui::DockBuilderAddNode };
    *icalls++ = { "DockBuilderRemoveNode", ImGui::DockBuilderRemoveNode };
    *icalls++ = { "DockBuilderRemoveNodeDockedWindows", ImGui::DockBuilderRemoveNodeDockedWindows };
    *icalls++ = { "DockBuilderRemoveNodeChildNodes", ImGui::DockBuilderRemoveNodeChildNodes };
    *icalls++ = { "DockBuilderSetNodePos", ImGui::DockBuilderSetNodePos };
    *icalls++ = { "DockBuilderSetNodeSize", ImGui::DockBuilderSetNodeSize };
    *icalls++ = { "DockBuilderSplitNode", ImGui::DockBuilderSplitNode };
    *icalls++ = { "DockBuilderCopyDockSpace", ImGui::DockBuilderCopyDockSpace };
    *icalls++ = { "DockBuilderCopyNode", ImGui::DockBuilderCopyNode };
    *icalls++ = { "DockBuilderCopyWindowSettings", ImGui::DockBuilderCopyWindowSettings };
    *icalls++ = { "DockBuilderFinish", ImGui::DockBuilderFinish };
    *icalls++ = { "GvContext", gvContext };
    *icalls++ = { "GvFreeContext", gvFreeContext };
    *icalls++ = { "GvLayout", layout_nodes };
}


BOOL APIENTRY DllMain(HMODULE hModule, DWORD dwReason, LPVOID lpReserved) {
    return TRUE;
}

void ax::NodeEditor::ShowStyleEditor(bool* show) {
    namespace ed = ax::NodeEditor;
    if (!ImGui::Begin("Style", show))
    {
        ImGui::End();
        return;
    }

    auto paneWidth = ImGui::GetContentRegionAvail().x;

    auto& editorStyle = ed::GetStyle();
    ImGui::Spacing();
    ImGui::DragFloat4("Node Padding", &editorStyle.NodePadding.x, 0.1f, 0.0f, 40.0f);
    ImGui::DragFloat("Node Rounding", &editorStyle.NodeRounding, 0.1f, 0.0f, 40.0f);
    ImGui::DragFloat("Node Border Width", &editorStyle.NodeBorderWidth, 0.1f, 0.0f, 15.0f);
    ImGui::DragFloat("Hovered Node Border Width", &editorStyle.HoveredNodeBorderWidth, 0.1f, 0.0f, 15.0f);
    ImGui::DragFloat("Hovered Node Border Offset", &editorStyle.HoverNodeBorderOffset, 0.1f, -40.0f, 40.0f);
    ImGui::DragFloat("Selected Node Border Width", &editorStyle.SelectedNodeBorderWidth, 0.1f, 0.0f, 15.0f);
    ImGui::DragFloat("Selected Node Border Offset", &editorStyle.SelectedNodeBorderOffset, 0.1f, -40.0f, 40.0f);
    ImGui::DragFloat("Pin Rounding", &editorStyle.PinRounding, 0.1f, 0.0f, 40.0f);
    ImGui::DragFloat("Pin Border Width", &editorStyle.PinBorderWidth, 0.1f, 0.0f, 15.0f);
    ImGui::DragFloat("Link Strength", &editorStyle.LinkStrength, 1.0f, 0.0f, 500.0f);
    //ImVec2  SourceDirection;
    //ImVec2  TargetDirection;
    ImGui::DragFloat("Scroll Duration", &editorStyle.ScrollDuration, 0.001f, 0.0f, 2.0f);
    ImGui::DragFloat("Flow Marker Distance", &editorStyle.FlowMarkerDistance, 1.0f, 1.0f, 200.0f);
    ImGui::DragFloat("Flow Speed", &editorStyle.FlowSpeed, 1.0f, 1.0f, 2000.0f);
    ImGui::DragFloat("Flow Duration", &editorStyle.FlowDuration, 0.001f, 0.0f, 5.0f);
    //ImVec2  PivotAlignment;
    //ImVec2  PivotSize;
    //ImVec2  PivotScale;
    //float   PinCorners;
    //float   PinRadius;
    //float   PinArrowSize;
    //float   PinArrowWidth;
    ImGui::DragFloat("Group Rounding", &editorStyle.GroupRounding, 0.1f, 0.0f, 40.0f);
    ImGui::DragFloat("Group Border Width", &editorStyle.GroupBorderWidth, 0.1f, 0.0f, 15.0f);

    ImGui::Separator();

    static ImGuiColorEditFlags edit_mode = ImGuiColorEditFlags_DisplayRGB;
    ImGui::TextUnformatted("Filter Colors");
    ImGui::SameLine();
    ImGui::RadioButton("RGB", &edit_mode, ImGuiColorEditFlags_DisplayRGB);
    ImGui::SameLine();
    ImGui::RadioButton("HSV", &edit_mode, ImGuiColorEditFlags_DisplayHSV);
    ImGui::SameLine();
    ImGui::RadioButton("HEX", &edit_mode, ImGuiColorEditFlags_DisplayHex);

    static ImGuiTextFilter filter;
    filter.Draw("##filter", paneWidth);

    ImGui::Spacing();

    ImGui::PushItemWidth(-160);
    for (int i = 0; i < ed::StyleColor_Count; ++i)
    {
        auto name = ed::GetStyleColorName((ed::StyleColor)i);
        if (!filter.PassFilter(name))
            continue;

        ImGui::ColorEdit4(name, &editorStyle.Colors[i].x, edit_mode);
    }
    ImGui::PopItemWidth();

    ImGui::End();
}

bool inject_render_doc(const wchar_t* dll_path) {
    if (rdoc_api) {
        return true;
    }

    const HMODULE renderdoc = LoadLibraryW(dll_path);
    if (!renderdoc) {
        return false;
    }

    const auto get_api = (pRENDERDOC_GetAPI)GetProcAddress(renderdoc, "RENDERDOC_GetAPI");
    if (!get_api) {
        FreeLibrary(renderdoc);
        return false;
    }

    if (get_api(eRENDERDOC_API_Version_1_6_0, (void**)&rdoc_api) != 1) {
        FreeLibrary(renderdoc);
        return false;
    }

    if (!rdoc_api->SetCaptureOptionU32(eRENDERDOC_Option_CaptureCallstacks, 1)) {
        MessageBoxA(nullptr, "Failed to set capture callstacks option", "RenderDoc", MB_OK | MB_ICONERROR);
    }

    return true;
}
