#include "pch.h"

#include "SPL/InternalCall.h"

#include "imgui.h"
#include "imgui-node-editor/imgui_node_editor.h"
#include "imgui-node-editor/imgui_node_editor_internal.h"

namespace SPLNative = SharpPluginLoader::Native;

SPL_INTERNAL_CALL int get_internal_call_count() {
    return 115;
}

SPL_INTERNAL_CALL void collect_internal_calls(SPLNative::InternalCall* icalls) {
    using ax::NodeEditor::PinId;
    using ax::NodeEditor::NodeId;

    *(icalls++) = { "SetCurrentEditor", ax::NodeEditor::SetCurrentEditor };
    *(icalls++) = { "GetCurrentEditor", ax::NodeEditor::GetCurrentEditor };
    *(icalls++) = { "CreateEditor", ax::NodeEditor::CreateEditor };
    *(icalls++) = { "DestroyEditor", ax::NodeEditor::DestroyEditor };
    *(icalls++) = { "GetConfig", ax::NodeEditor::GetConfig };
    *(icalls++) = { "GetStyle", ax::NodeEditor::GetStyle };
    *(icalls++) = { "GetStyleColorName", ax::NodeEditor::GetStyleColorName };
    *(icalls++) = { "PushStyleColor", ax::NodeEditor::PushStyleColor };
    *(icalls++) = { "PopStyleColor", ax::NodeEditor::PopStyleColor };
    *(icalls++) = { "PushStyleVarFloat", static_cast<void(*)(ax::NodeEditor::StyleVar, float)>(ax::NodeEditor::PushStyleVar) };
    *(icalls++) = { "PushStyleVarVec2", static_cast<void(*)(ax::NodeEditor::StyleVar, const ImVec2&)>(ax::NodeEditor::PushStyleVar) };
    *(icalls++) = { "PushStyleVarVec4", static_cast<void(*)(ax::NodeEditor::StyleVar, const ImVec4&)>(ax::NodeEditor::PushStyleVar) };
    *(icalls++) = { "PopStyleVar", ax::NodeEditor::PopStyleVar };
    *(icalls++) = { "Begin", ax::NodeEditor::Begin };
    *(icalls++) = { "End", ax::NodeEditor::End };
    *(icalls++) = { "BeginNode", ax::NodeEditor::BeginNode };
    *(icalls++) = { "BeginPin", ax::NodeEditor::BeginPin };
    *(icalls++) = { "PinRect", ax::NodeEditor::PinRect };
    *(icalls++) = { "PinPivotRect", ax::NodeEditor::PinPivotRect };
    *(icalls++) = { "PinPivotSize", ax::NodeEditor::PinPivotSize };
    *(icalls++) = { "PinPivotScale", ax::NodeEditor::PinPivotScale };
    *(icalls++) = { "PinPivotAlignment", ax::NodeEditor::PinPivotAlignment };
    *(icalls++) = { "EndPin", ax::NodeEditor::EndPin };
    *(icalls++) = { "Group", ax::NodeEditor::Group };
    *(icalls++) = { "EndNode", ax::NodeEditor::EndNode };
    *(icalls++) = { "BeginGroupHint", ax::NodeEditor::BeginGroupHint };
    *(icalls++) = { "GetGroupMin", ax::NodeEditor::GetGroupMin };
    *(icalls++) = { "GetGroupMax", ax::NodeEditor::GetGroupMax };
    *(icalls++) = { "GetHintForegroundDrawList", ax::NodeEditor::GetHintForegroundDrawList };
    *(icalls++) = { "GetHintBackgroundDrawList", ax::NodeEditor::GetHintBackgroundDrawList };
    *(icalls++) = { "EndGroupHint", ax::NodeEditor::EndGroupHint };
    *(icalls++) = { "GetNodeBackgroundDrawList", ax::NodeEditor::GetNodeBackgroundDrawList };
    *(icalls++) = { "Link", ax::NodeEditor::Link };
    *(icalls++) = { "Flow", ax::NodeEditor::Flow };
    *(icalls++) = { "BeginCreate", ax::NodeEditor::BeginCreate };
    *(icalls++) = { "QueryNewLink", static_cast<bool(*)(PinId*, PinId*)>(ax::NodeEditor::QueryNewLink) };
    *(icalls++) = { "QueryNewLinkEx", static_cast<bool(*)(PinId*, PinId*, const ImVec4&, float)>(ax::NodeEditor::QueryNewLink) };
    *(icalls++) = { "QueryNewNode", static_cast<bool(*)(PinId*)>(ax::NodeEditor::QueryNewNode) };
    *(icalls++) = { "QueryNewNodeEx", static_cast<bool(*)(PinId*, const ImVec4&, float)>(ax::NodeEditor::QueryNewNode) };
    *(icalls++) = { "AcceptNewItem", static_cast<bool(*)()>(ax::NodeEditor::AcceptNewItem) };
    *(icalls++) = { "AcceptNewItemEx", static_cast<bool(*)(const ImVec4&, float)>(ax::NodeEditor::AcceptNewItem) };
    *(icalls++) = { "RejectNewItem", static_cast<void(*)()>(ax::NodeEditor::RejectNewItem) };
    *(icalls++) = { "RejectNewItemEx", static_cast<void(*)(const ImVec4&, float)>(ax::NodeEditor::RejectNewItem) };
    *(icalls++) = { "EndCreate", ax::NodeEditor::EndCreate };
    *(icalls++) = { "BeginDelete", ax::NodeEditor::BeginDelete };
    *(icalls++) = { "QueryDeletedLink", ax::NodeEditor::QueryDeletedLink };
    *(icalls++) = { "QueryDeletedNode", ax::NodeEditor::QueryDeletedNode };
    *(icalls++) = { "AcceptDeletedItem", ax::NodeEditor::AcceptDeletedItem };
    *(icalls++) = { "RejectDeletedItem", ax::NodeEditor::RejectDeletedItem };
    *(icalls++) = { "EndDelete", ax::NodeEditor::EndDelete };
    *(icalls++) = { "SetNodePosition", ax::NodeEditor::SetNodePosition };
    *(icalls++) = { "SetGroupSize", ax::NodeEditor::SetGroupSize };
    *(icalls++) = { "GetNodePosition", ax::NodeEditor::GetNodePosition };
    *(icalls++) = { "GetNodeSize", ax::NodeEditor::GetNodeSize };
    *(icalls++) = { "CenterNodeOnScreen", ax::NodeEditor::CenterNodeOnScreen };
    *(icalls++) = { "SetNodeZPosition", ax::NodeEditor::SetNodeZPosition };
    *(icalls++) = { "GetNodeZPosition", ax::NodeEditor::GetNodeZPosition };
    *(icalls++) = { "RestoreNodeState", ax::NodeEditor::RestoreNodeState };
    *(icalls++) = { "Suspend", ax::NodeEditor::Suspend };
    *(icalls++) = { "Resume", ax::NodeEditor::Resume };
    *(icalls++) = { "IsSuspended", ax::NodeEditor::IsSuspended };
    *(icalls++) = { "IsActive", ax::NodeEditor::IsActive };
    *(icalls++) = { "HasSelectionChanged", ax::NodeEditor::HasSelectionChanged };
    *(icalls++) = { "GetSelectedObjectCount", ax::NodeEditor::GetSelectedObjectCount };
    *(icalls++) = { "GetSelectedNodes", ax::NodeEditor::GetSelectedNodes };
    *(icalls++) = { "GetSelectedLinks", ax::NodeEditor::GetSelectedLinks };
    *(icalls++) = { "IsNodeSelected", ax::NodeEditor::IsNodeSelected };
    *(icalls++) = { "IsLinkSelected", ax::NodeEditor::IsLinkSelected };
    *(icalls++) = { "ClearSelection", ax::NodeEditor::ClearSelection };
    *(icalls++) = { "SelectNode", ax::NodeEditor::SelectNode };
    *(icalls++) = { "SelectLink", ax::NodeEditor::SelectLink };
    *(icalls++) = { "DeselectNode", ax::NodeEditor::DeselectNode };
    *(icalls++) = { "DeselectLink", ax::NodeEditor::DeselectLink };
    *(icalls++) = { "DeleteNode", ax::NodeEditor::DeleteNode };
    *(icalls++) = { "DeleteLink", ax::NodeEditor::DeleteLink };
    *(icalls++) = { "PinHasAnyLinks", static_cast<bool(*)(PinId)>(ax::NodeEditor::HasAnyLinks) }; 
    *(icalls++) = { "NodeHasAnyLinks", static_cast<bool(*)(NodeId)>(ax::NodeEditor::HasAnyLinks) }; 
    *(icalls++) = { "PinBreakLinks", static_cast<int(*)(PinId)>(ax::NodeEditor::BreakLinks) };
    *(icalls++) = { "NodeBreakLinks", static_cast<int(*)(NodeId)>(ax::NodeEditor::BreakLinks) };
    *(icalls++) = { "NavigateToContent", ax::NodeEditor::NavigateToContent };
    *(icalls++) = { "NavigateToSelection", ax::NodeEditor::NavigateToSelection };
    *(icalls++) = { "ShowNodeContextMenu", ax::NodeEditor::ShowNodeContextMenu };
    *(icalls++) = { "ShowPinContextMenu", ax::NodeEditor::ShowPinContextMenu };
    *(icalls++) = { "ShowLinkContextMenu", ax::NodeEditor::ShowLinkContextMenu };
    *(icalls++) = { "ShowBackgroundContextMenu", ax::NodeEditor::ShowBackgroundContextMenu };
    *(icalls++) = { "EnableShortcuts", ax::NodeEditor::EnableShortcuts };
    *(icalls++) = { "AreShortcutsEnabled", ax::NodeEditor::AreShortcutsEnabled };
    *(icalls++) = { "BeginShortcut", ax::NodeEditor::BeginShortcut };
    *(icalls++) = { "AcceptCut", ax::NodeEditor::AcceptCut };
    *(icalls++) = { "AcceptCopy", ax::NodeEditor::AcceptCopy };
    *(icalls++) = { "AcceptPaste", ax::NodeEditor::AcceptPaste };
    *(icalls++) = { "AcceptDuplicate", ax::NodeEditor::AcceptDuplicate };
    *(icalls++) = { "AcceptCreateNode", ax::NodeEditor::AcceptCreateNode };
    *(icalls++) = { "GetActionContextSize", ax::NodeEditor::GetActionContextSize };
    *(icalls++) = { "GetActionContextNodes", ax::NodeEditor::GetActionContextNodes };
    *(icalls++) = { "GetActionContextLinks", ax::NodeEditor::GetActionContextLinks };
    *(icalls++) = { "EndShortcut", ax::NodeEditor::EndShortcut };
    *(icalls++) = { "GetCurrentZoom", ax::NodeEditor::GetCurrentZoom };
    *(icalls++) = { "GetHoveredNode", ax::NodeEditor::GetHoveredNode };
    *(icalls++) = { "GetHoveredPin", ax::NodeEditor::GetHoveredPin };
    *(icalls++) = { "GetHoveredLink", ax::NodeEditor::GetHoveredLink };
    *(icalls++) = { "GetDoubleClickedNode", ax::NodeEditor::GetDoubleClickedNode };
    *(icalls++) = { "GetDoubleClickedPin", ax::NodeEditor::GetDoubleClickedPin };
    *(icalls++) = { "GetDoubleClickedLink", ax::NodeEditor::GetDoubleClickedLink };
    *(icalls++) = { "IsBackgroundClicked", ax::NodeEditor::IsBackgroundClicked };
    *(icalls++) = { "IsBackgroundDoubleClicked", ax::NodeEditor::IsBackgroundDoubleClicked };
    *(icalls++) = { "GetBackgroundClickButtonIndex", ax::NodeEditor::GetBackgroundClickButtonIndex };
    *(icalls++) = { "GetBackgroundDoubleClickButtonIndex", ax::NodeEditor::GetBackgroundDoubleClickButtonIndex };
    *(icalls++) = { "GetLinkPins", ax::NodeEditor::GetLinkPins };
    *(icalls++) = { "PinHadAnyLinks", ax::NodeEditor::PinHadAnyLinks };
    *(icalls++) = { "GetScreenSize", ax::NodeEditor::GetScreenSize };
    *(icalls++) = { "ScreenToCanvas", ax::NodeEditor::ScreenToCanvas };
    *(icalls++) = { "CanvasToScreen", ax::NodeEditor::CanvasToScreen };
    *(icalls++) = { "GetNodeCount", ax::NodeEditor::GetNodeCount };
    *(icalls++) = { "GetOrderedNodeIds", ax::NodeEditor::GetOrderedNodeIds };
}


BOOL APIENTRY DllMain(HMODULE hModule, DWORD dwReason, LPVOID lpReserved) {
    return TRUE;
}
