#include "PropertyDisplay.h"
#include "imgui.h"
#include "misc/cpp/imgui_stdlib.h"
#include <string>
#include <format>

typedef signed char s8;
typedef unsigned char u8;
typedef signed short s16;
typedef unsigned short u16;
typedef signed int s32;
typedef unsigned int u32;
typedef signed long long s64;
typedef unsigned long long u64;
typedef float f32;
typedef double f64;

MtPropertyList_ctor property_list_ctor = nullptr;
MtString_assign mt_string_assign = nullptr;
void set_callbacks(MtPropertyList_ctor ctor, MtString_assign assign) {
    property_list_ctor = ctor;
    mt_string_assign = assign;
}

void display_imgui_properties(MtObject* obj, const char* name, const char* comment) {
    if (!property_list_ctor) {
        ImGui::TextColored(ImVec4(1, 0, 0, 1), "PropertyList constructor not set");
        return;
    }

    MtPropertyList list(property_list_ctor);
    obj->create_property(&list);

    bool open;
    if (name != nullptr) {
        if (comment != nullptr) {
            open = ImGui::TreeNode(obj, "%s %s (%s)", obj->get_dti()->name(), name, comment);
        } else {
            open = ImGui::TreeNode(obj, "%s %s", obj->get_dti()->name(), name);
        }
    } else {
        open = ImGui::TreeNode(obj, "%s", obj->get_dti()->name());
    }

    if (open) {
        for (const auto prop : list) {
            display_prop(prop);
        }
        ImGui::TreePop();
    }
}

void display_prop(const MtProperty* prop) {
    if (prop->is_property()) {
        if (prop->is_array()) {
            display_prop_property_array(prop);
        } else {
            display_prop_property(prop);
        }
    } else {
        if (prop->is_array()) {
            display_prop_value_array(prop);
        } else {
            display_prop_value(prop);
        }
    }
}

#pragma region ImGui Step Sizes
namespace {
constexpr s8 s8_step = 1;
constexpr s8 s8_step_fast = 10;
constexpr u8 u8_step = 1;
constexpr u8 u8_step_fast = 10;
constexpr s16 s16_step = 1;
constexpr s16 s16_step_fast = 10;
constexpr u16 u16_step = 1;
constexpr u16 u16_step_fast = 10;
constexpr s32 s32_step = 1;
constexpr s32 s32_step_fast = 100;
constexpr u32 u32_step = 1;
constexpr u32 u32_step_fast = 100;
constexpr s64 s64_step = 1;
constexpr s64 s64_step_fast = 100;
constexpr u64 u64_step = 1;
constexpr u64 u64_step_fast = 100;
}
#pragma endregion

void display_prop_value(const MtProperty* prop) {
    const auto display_name = prop->display_name();

    switch (prop->type()) {
    case PropType::Undefined:
        break;
    case PropType::Class:
        display_imgui_properties(prop->data<MtObject>(), prop->name(), prop->comment());
        break;
    case PropType::ClassRef:
        display_imgui_properties(*prop->data<MtObject*>(), prop->name(), prop->comment());
        break;
    case PropType::Bool:
        ImGui::Checkbox(display_name.c_str(), prop->data<bool>());
        break;
    case PropType::U8:
        ImGui::InputScalar(display_name.c_str(), ImGuiDataType_U8, prop->data<u8>(), &u8_step, &u8_step_fast);
        break;
    case PropType::U16:
        ImGui::InputScalar(display_name.c_str(), ImGuiDataType_U16, prop->data<u16>(), &u16_step, &u16_step_fast);
        break;
    case PropType::U32:
        ImGui::InputScalar(display_name.c_str(), ImGuiDataType_U32, prop->data<u32>(), &u32_step, &u32_step_fast);
        break;
    case PropType::U64:
        ImGui::InputScalar(display_name.c_str(), ImGuiDataType_U64, prop->data<u64>(), &u64_step, &u64_step_fast);
        break;
    case PropType::S8:
        ImGui::InputScalar(display_name.c_str(), ImGuiDataType_S8, prop->data<s8>(), &s8_step, &s8_step_fast);
        break;
    case PropType::S16:
        ImGui::InputScalar(display_name.c_str(), ImGuiDataType_S16, prop->data<s16>(), &s16_step, &s16_step_fast);
        break;
    case PropType::S32:
        ImGui::InputScalar(display_name.c_str(), ImGuiDataType_S32, prop->data<s32>(), &s32_step, &s32_step_fast);
        break;
    case PropType::S64:
        ImGui::InputScalar(display_name.c_str(), ImGuiDataType_S64, prop->data<s64>(), &s64_step, &s64_step_fast);
        break;
    case PropType::F32:
        ImGui::DragFloat(display_name.c_str(), prop->data<f32>());
        break;
    case PropType::F64:
        ImGui::DragScalar(display_name.c_str(), ImGuiDataType_Double, prop->data<f64>());
        break;
    case PropType::String: {
        MtString** str = prop->data<MtString*>();
        std::string s((*str)->Data, (*str)->Length);
        if (ImGui::InputText(display_name.c_str(), &s)) {
            mt_string_assign(str, s.c_str());
        }
    } break;
    case PropType::Color: {
        ImVec4 color = ImGui::ColorConvertU32ToFloat4(*prop->data<u32>());
        ImGui::ColorEdit4(display_name.c_str(), &color.x);
        *prop->data<u32>() = ImGui::ColorConvertFloat4ToU32(color);
    } break;
    case PropType::Point: [[fallthrough]];
    case PropType::Size:
        ImGui::InputInt2(display_name.c_str(), prop->data<s32>());
        break;
    case PropType::Rect:
        ImGui::InputInt4(display_name.c_str(), prop->data<s32>());
        break;
    case PropType::Matrix44:
    case PropType::Float4X4:
        ImGui::DragFloat4(std::format("{}[0]", display_name).c_str(), prop->data<f32>(), 0.1f);
        ImGui::DragFloat4(std::format("{}[1]", display_name).c_str(), prop->data<f32>() + 4, 0.1f);
        ImGui::DragFloat4(std::format("{}[2]", display_name).c_str(), prop->data<f32>() + 8, 0.1f);
        ImGui::DragFloat4(std::format("{}[3]", display_name).c_str(), prop->data<f32>() + 12, 0.1f);
        break;
    case PropType::Vector3:
        ImGui::DragFloat3(display_name.c_str(), prop->data<f32>(), 0.1f);
        break;
    case PropType::Vector4: [[fallthrough]];
    case PropType::Quaternion:
        ImGui::DragFloat4(display_name.c_str(), prop->data<f32>(), 0.1f);
        break;
    case PropType::CString:
        ImGui::InputText(display_name.c_str(), *prop->data<char*>(), std::strlen(*prop->data<const char*>()));
        break;
    case PropType::Time:
        ImGui::InputScalar(display_name.c_str(), ImGuiDataType_U64, prop->data<u64>(), &u64_step, &u64_step_fast);
        break;
    case PropType::Float2:
        ImGui::DragFloat2(display_name.c_str(), prop->data<f32>(), 0.1f);
        break;
    case PropType::Float3:
        ImGui::DragFloat3(display_name.c_str(), prop->data<f32>(), 0.1f);
        break;
    case PropType::Float4:
        ImGui::DragFloat4(display_name.c_str(), prop->data<f32>(), 0.1f);
        break;
    case PropType::Float3X3:
        ImGui::DragFloat3(std::format("{}[0]", display_name).c_str(), prop->data<f32>(), 0.1f);
        ImGui::DragFloat3(std::format("{}[1]", display_name).c_str(), prop->data<f32>() + 3, 0.1f);
        ImGui::DragFloat3(std::format("{}[2]", display_name).c_str(), prop->data<f32>() + 6, 0.1f);
        break;
    case PropType::Float4X3:
        ImGui::DragFloat3(std::format("{}[0]", display_name).c_str(), prop->data<f32>(), 0.1f);
        ImGui::DragFloat3(std::format("{}[1]", display_name).c_str(), prop->data<f32>() + 3, 0.1f);
        ImGui::DragFloat3(std::format("{}[2]", display_name).c_str(), prop->data<f32>() + 6, 0.1f);
        ImGui::DragFloat3(std::format("{}[3]", display_name).c_str(), prop->data<f32>() + 9, 0.1f);
        break;
    default:
        ImGui::Text("%s: Unsupported type: %d", display_name.c_str(), (int)prop->type());
        break;
    }
}

void display_prop_value_array(const MtProperty* prop) {
    const auto display_name = prop->display_name();
    switch (prop->type()) {
    case PropType::Class: {
        const auto size = prop->data<MtObject>()->get_dti()->size();
        for (u64 i = 0; i < prop->count(); ++i) {
            display_imgui_properties((MtObject*)(prop->data<u8>() + i * size), prop->name(), prop->comment());
        }
    } break;
    case PropType::ClassRef:
        for (u64 i = 0; i < prop->count(); ++i) {
            display_imgui_properties(prop->data<MtObject*>()[i], prop->name(), prop->comment());
        }
        break;
    case PropType::Bool:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::Checkbox(std::format("{}[{}]", display_name, i).c_str(), prop->data<bool>() + i);
        }
        break;
    case PropType::U8:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_U8, prop->data<u8>() + i, &u8_step, &u8_step_fast);
        }
        break;
    case PropType::U16:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_U16, prop->data<u16>() + i, &u16_step, &u16_step_fast);
        }
        break;
    case PropType::U32:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_U32, prop->data<u32>() + i, &u32_step, &u32_step_fast);
        }
        break;
    case PropType::U64:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_U64, prop->data<u64>() + i, &u64_step, &u64_step_fast);
        }
        break;
    case PropType::S8:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_S8, prop->data<s8>() + i, &s8_step, &s8_step_fast);
        }
        break;
    case PropType::S16:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_S16, prop->data<s16>() + i, &s16_step, &s16_step_fast);
        }
        break;
    case PropType::S32:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_S32, prop->data<s32>() + i, &s32_step, &s32_step_fast);
        }
        break;
    case PropType::S64:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_S64, prop->data<s64>() + i, &s64_step, &s64_step_fast);
        }
        break;
    case PropType::F32:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat(std::format("{}[{}]", display_name, i).c_str(), prop->data<f32>() + i, 0.1f);
        }
        break;
    case PropType::F64:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::DragScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_Double, prop->data<f64>() + i);
        }
        break;
    case PropType::String:
        for (u64 i = 0; i < prop->count(); ++i) {
            MtString** str = prop->data<MtString*>() + i;
            std::string s((*str)->Data, (*str)->Length);
            if (ImGui::InputText(std::format("{}[{}]", display_name, i).c_str(), &s)) {
                mt_string_assign(str, s.c_str());
            }
        }
        break;
    case PropType::Color:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImVec4 color = ImGui::ColorConvertU32ToFloat4(prop->data<u32>()[i]);
            ImGui::ColorEdit4(std::format("{}[{}]", display_name, i).c_str(), &color.x);
            prop->data<u32>()[i] = ImGui::ColorConvertFloat4ToU32(color);
        }
        break;
    case PropType::Point:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputInt2(std::format("{}[{}]", display_name, i).c_str(), prop->data<s32>() + i * 2);
        }
        break;
    case PropType::Matrix44:
    case PropType::Float4X4:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat4(std::format("{}[{}][0]", display_name, i).c_str(), prop->data<f32>() + i * 16, 0.1f);
            ImGui::DragFloat4(std::format("{}[{}][1]", display_name, i).c_str(), prop->data<f32>() + i * 16 + 4, 0.1f);
            ImGui::DragFloat4(std::format("{}[{}][2]", display_name, i).c_str(), prop->data<f32>() + i * 16 + 8, 0.1f);
            ImGui::DragFloat4(std::format("{}[{}][3]", display_name, i).c_str(), prop->data<f32>() + i * 16 + 12, 0.1f);
        }
        break;
    case PropType::Vector3:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat3(std::format("{}[{}]", display_name, i).c_str(), prop->data<f32>() + i * 3, 0.1f);
        }
        break;
    case PropType::Vector4:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat4(std::format("{}[{}]", display_name, i).c_str(), prop->data<f32>() + i * 4, 0.1f);
        }
        break;
    case PropType::Quaternion:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat4(std::format("{}[{}]", display_name, i).c_str(), prop->data<f32>() + i * 4, 0.1f);
        }
        break;
    case PropType::CString:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputText(std::format("{}[{}]", display_name, i).c_str(), prop->data<char*>()[i], std::strlen(prop->data<const char*>()[i]));
        }
        break;
    case PropType::Time:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", display_name, i).c_str(), ImGuiDataType_U64, prop->data<u64>() + i, &u64_step, &u64_step_fast);
        }
        break;
    case PropType::Float2:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat2(std::format("{}[{}]", display_name, i).c_str(), prop->data<f32>() + i * 2, 0.1f);
        }
        break;
    case PropType::Float3:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat3(std::format("{}[{}]", display_name, i).c_str(), prop->data<f32>() + i * 3, 0.1f);
        }
        break;
    case PropType::Float4:
        for (u64 i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat4(std::format("{}[{}]", display_name, i).c_str(), prop->data<f32>() + i * 4, 0.1f);
        }
        break;
    default:
        ImGui::Text("Unsupported type: %d", (int)prop->type());
        break;
    }
}

void display_prop_property(const MtProperty* prop) {
    const auto display_name = prop->display_name();
    switch (prop->type()) {
    case PropType::Class:
    case PropType::ClassRef:
        display_imgui_properties(prop->get<MtObject*>(), prop->name(), prop->comment());
        break;
    case PropType::String:
        if (std::strcmp(display_name.c_str(), "TagStr") == 0) {
            ImGui::InputText(display_name.c_str(), prop->get<char*>(), 15);
        } else if (std::strcmp(display_name.c_str(), "TextLabelID") == 0) {
            ImGui::InputText(display_name.c_str(), prop->get<char*>(), 255);
        } else {
            ImGui::InputText(display_name.c_str(), prop->get<char*>(), 12);
            if (ImGui::BeginItemTooltip()) {
                ImGui::Text("Unknown Property, String length limited to 12 bytes.");
                ImGui::EndTooltip();
            }
        }
        break;
    case PropType::U32: {
        auto val = prop->get<u32>();
        if (ImGui::InputScalar(display_name.c_str(), ImGuiDataType_U32, &val, &u32_step, &u32_step_fast)) {
            prop->set(val);
        }
    } break;
    default:
        ImGui::Text("%s: Unsupported type for display_prop_property: %d", display_name.c_str(), (int)prop->type());
        break;
    }
}

void display_prop_property_array(const MtProperty* prop) {
    const auto display_name = prop->display_name();
    if (prop->type() == PropType::Bool) {
        for (u32 i = 0; i < prop->get_count(); ++i) {
            auto val = prop->get<bool>(i);
            ImGui::Checkbox(std::format("{}[{}]", display_name, i).c_str(), &val);
            prop->set(val, i);
        }
    } else {
        ImGui::Text("%s: Array of properties not supported yet", display_name.c_str());
    }
}

