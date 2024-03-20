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

void display_imgui_properties(MtObject* obj) {
    if (!property_list_ctor) {
        ImGui::TextColored(ImVec4(1, 0, 0, 1), "PropertyList constructor not set");
        return;
    }

    MtPropertyList list(property_list_ctor);
    obj->create_property(&list);

    for (auto prop : list) {
        display_prop(prop);
    }
}

void display_prop(MtProperty* prop) {
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
static s8 s8_step = 1;
static s8 s8_step_fast = 10;
static u8 u8_step = 1;
static u8 u8_step_fast = 10;
static s16 s16_step = 1;
static s16 s16_step_fast = 10;
static u16 u16_step = 1;
static u16 u16_step_fast = 10;
static s32 s32_step = 1;
static s32 s32_step_fast = 100;
static u32 u32_step = 1;
static u32 u32_step_fast = 100;
static s64 s64_step = 1;
static s64 s64_step_fast = 100;
static u64 u64_step = 1;
static u64 u64_step_fast = 100;
#pragma endregion

void display_prop_value(MtProperty* prop) {
    switch (prop->type()) {
    case PropType::Undefined:
        break;
    case PropType::Class:
        display_imgui_properties(prop->data<MtObject>());
        break;
    case PropType::ClassRef:
        display_imgui_properties(*prop->data<MtObject*>());
        break;
    case PropType::Bool:
        ImGui::Checkbox(prop->hash_name(), prop->data<bool>());
        break;
    case PropType::U8:
        ImGui::InputScalar(prop->hash_name(), ImGuiDataType_U8, prop->data<u8>(), &u8_step, &u8_step_fast);
        break;
    case PropType::U16:
        ImGui::InputScalar(prop->hash_name(), ImGuiDataType_U16, prop->data<u16>(), &u16_step, &u16_step_fast);
        break;
    case PropType::U32:
        ImGui::InputScalar(prop->hash_name(), ImGuiDataType_U32, prop->data<u32>(), &u32_step, &u32_step_fast);
        break;
    case PropType::U64:
        ImGui::InputScalar(prop->hash_name(), ImGuiDataType_U64, prop->data<u64>(), &u64_step, &u64_step_fast);
        break;
    case PropType::S8:
        ImGui::InputScalar(prop->hash_name(), ImGuiDataType_S8, prop->data<s8>(), &s8_step, &s8_step_fast);
        break;
    case PropType::S16:
        ImGui::InputScalar(prop->hash_name(), ImGuiDataType_S16, prop->data<s16>(), &s16_step, &s16_step_fast);
        break;
    case PropType::S32:
        ImGui::InputScalar(prop->hash_name(), ImGuiDataType_S32, prop->data<s32>(), &s32_step, &s32_step_fast);
        break;
    case PropType::S64:
        ImGui::InputScalar(prop->hash_name(), ImGuiDataType_S64, prop->data<s64>(), &s64_step, &s64_step_fast);
        break;
    case PropType::F32:
        ImGui::DragFloat(prop->hash_name(), prop->data<f32>());
        break;
    case PropType::F64:
        ImGui::DragScalar(prop->hash_name(), ImGuiDataType_Double, prop->data<f64>());
        break;
    case PropType::String: {
        MtString** str = prop->data<MtString*>();
        std::string s((*str)->Data, (*str)->Length);
        if (ImGui::InputText(prop->hash_name(), &s)) {
            mt_string_assign(str, s.c_str());
        }
    } break;
    case PropType::Color: {
        ImVec4 color = ImGui::ColorConvertU32ToFloat4(*prop->data<u32>());
        ImGui::ColorEdit4(prop->hash_name(), &color.x);
        *prop->data<u32>() = ImGui::ColorConvertFloat4ToU32(color);
    } break;
    case PropType::Point:
        ImGui::InputInt2(prop->hash_name(), prop->data<s32>());
        break;
    case PropType::Size:
        ImGui::InputInt2(prop->hash_name(), prop->data<s32>());
        break;
    case PropType::Rect:
        ImGui::InputInt4(prop->hash_name(), prop->data<s32>());
        break;
    case PropType::Matrix44:
    case PropType::Float4X4:
        ImGui::DragFloat4(std::format("{}[0]", prop->hash_name()).c_str(), prop->data<f32>(), 0.1f);
        ImGui::DragFloat4(std::format("{}[1]", prop->hash_name()).c_str(), prop->data<f32>() + 4, 0.1f);
        ImGui::DragFloat4(std::format("{}[2]", prop->hash_name()).c_str(), prop->data<f32>() + 8, 0.1f);
        ImGui::DragFloat4(std::format("{}[3]", prop->hash_name()).c_str(), prop->data<f32>() + 12, 0.1f);
        break;
    case PropType::Vector3:
        ImGui::DragFloat3(prop->hash_name(), prop->data<f32>(), 0.1f);
        break;
    case PropType::Vector4:
        ImGui::DragFloat4(prop->hash_name(), prop->data<f32>(), 0.1f);
        break;
    case PropType::Quaternion:
        ImGui::DragFloat4(prop->hash_name(), prop->data<f32>(), 0.1f);
        break;
    case PropType::CString:
        ImGui::InputText(prop->hash_name(), *prop->data<char*>(), std::strlen(*prop->data<const char*>()));
        break;
    case PropType::Time:
        ImGui::InputScalar(prop->hash_name(), ImGuiDataType_U64, prop->data<u64>(), &u64_step, &u64_step_fast);
        break;
    case PropType::Float2:
        ImGui::DragFloat2(prop->hash_name(), prop->data<f32>(), 0.1f);
        break;
    case PropType::Float3:
        ImGui::DragFloat3(prop->hash_name(), prop->data<f32>(), 0.1f);
        break;
    case PropType::Float4:
        ImGui::DragFloat4(prop->hash_name(), prop->data<f32>(), 0.1f);
        break;
    case PropType::Float3X3:
        ImGui::DragFloat3(std::format("{}[0]", prop->hash_name()).c_str(), prop->data<f32>(), 0.1f);
        ImGui::DragFloat3(std::format("{}[1]", prop->hash_name()).c_str(), prop->data<f32>() + 3, 0.1f);
        ImGui::DragFloat3(std::format("{}[2]", prop->hash_name()).c_str(), prop->data<f32>() + 6, 0.1f);
        break;
    case PropType::Float4X3:
        ImGui::DragFloat3(std::format("{}[0]", prop->hash_name()).c_str(), prop->data<f32>(), 0.1f);
        ImGui::DragFloat3(std::format("{}[1]", prop->hash_name()).c_str(), prop->data<f32>() + 3, 0.1f);
        ImGui::DragFloat3(std::format("{}[2]", prop->hash_name()).c_str(), prop->data<f32>() + 6, 0.1f);
        ImGui::DragFloat3(std::format("{}[3]", prop->hash_name()).c_str(), prop->data<f32>() + 9, 0.1f);
        break;
    default:
        ImGui::Text("%s: Unsupported type: %d", prop->hash_name(), (int)prop->type());
        break;
    }
}

void display_prop_value_array(MtProperty* prop) {
    switch (prop->type()) {
    case PropType::Class: {
        auto size = prop->data<MtObject>()->get_dti()->size();
        for (int i = 0; i < prop->count(); ++i) {
            display_imgui_properties((MtObject*)(prop->data<u8>() + i * size));
        }
    } break;
    case PropType::ClassRef:
        for (int i = 0; i < prop->count(); ++i) {
            display_imgui_properties(prop->data<MtObject*>()[i]);
        }
        break;
    case PropType::Bool:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::Checkbox(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<bool>() + i);
        }
        break;
    case PropType::U8:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_U8, prop->data<u8>() + i, &u8_step, &u8_step_fast);
        }
        break;
    case PropType::U16:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_U16, prop->data<u16>() + i, &u16_step, &u16_step_fast);
        }
        break;
    case PropType::U32:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_U32, prop->data<u32>() + i, &u32_step, &u32_step_fast);
        }
        break;
    case PropType::U64:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_U64, prop->data<u64>() + i, &u64_step, &u64_step_fast);
        }
        break;
    case PropType::S8:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_S8, prop->data<s8>() + i, &s8_step, &s8_step_fast);
        }
        break;
    case PropType::S16:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_S16, prop->data<s16>() + i, &s16_step, &s16_step_fast);
        }
        break;
    case PropType::S32:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_S32, prop->data<s32>() + i, &s32_step, &s32_step_fast);
        }
        break;
    case PropType::S64:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_S64, prop->data<s64>() + i, &s64_step, &s64_step_fast);
        }
        break;
    case PropType::F32:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<f32>() + i, 0.1f);
        }
        break;
    case PropType::F64:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::DragScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_Double, prop->data<f64>() + i);
        }
        break;
    case PropType::String:
        for (int i = 0; i < prop->count(); ++i) {
            MtString** str = prop->data<MtString*>() + i;
            std::string s((*str)->Data, (*str)->Length);
            if (ImGui::InputText(std::format("{}[{}]", prop->hash_name(), i).c_str(), &s)) {
                mt_string_assign(str, s.c_str());
            }
        }
        break;
    case PropType::Color:
        for (int i = 0; i < prop->count(); ++i) {
            ImVec4 color = ImGui::ColorConvertU32ToFloat4(prop->data<u32>()[i]);
            ImGui::ColorEdit4(std::format("{}[{}]", prop->hash_name(), i).c_str(), &color.x);
            prop->data<u32>()[i] = ImGui::ColorConvertFloat4ToU32(color);
        }
        break;
    case PropType::Point:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputInt2(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<s32>() + i * 2);
        }
        break;
    case PropType::Matrix44:
    case PropType::Float4X4:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat4(std::format("{}[{}][0]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 16, 0.1f);
            ImGui::DragFloat4(std::format("{}[{}][1]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 16 + 4, 0.1f);
            ImGui::DragFloat4(std::format("{}[{}][2]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 16 + 8, 0.1f);
            ImGui::DragFloat4(std::format("{}[{}][3]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 16 + 12, 0.1f);
        }
        break;
    case PropType::Vector3:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat3(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 3, 0.1f);
        }
        break;
    case PropType::Vector4:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat4(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 4, 0.1f);
        }
        break;
    case PropType::Quaternion:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat4(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 4, 0.1f);
        }
        break;
    case PropType::CString:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputText(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<char*>()[i], std::strlen(prop->data<const char*>()[i]));
        }
        break;
    case PropType::Time:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::InputScalar(std::format("{}[{}]", prop->hash_name(), i).c_str(), ImGuiDataType_U64, prop->data<u64>() + i, &u64_step, &u64_step_fast);
        }
        break;
    case PropType::Float2:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat2(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 2, 0.1f);
        }
        break;
    case PropType::Float3:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat3(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 3, 0.1f);
        }
        break;
    case PropType::Float4:
        for (int i = 0; i < prop->count(); ++i) {
            ImGui::DragFloat4(std::format("{}[{}]", prop->hash_name(), i).c_str(), prop->data<f32>() + i * 4, 0.1f);
        }
        break;
    default:
        ImGui::Text("Unsupported type: %d", (int)prop->type());
        break;
    }
}

void display_prop_property(MtProperty* prop) {
    switch (prop->type()) {
    case PropType::Class:
    case PropType::ClassRef:
        display_imgui_properties(prop->get<MtObject*>());
        break;
    case PropType::String:
        ImGui::InputText(prop->hash_name(), prop->get<char*>(), 12);
        break;
    case PropType::U32: {
        auto val = prop->get<u32>();
        if (ImGui::InputScalar(prop->hash_name(), ImGuiDataType_U32, &val, &u32_step, &u32_step_fast)) {
            prop->set(val);
        }
    } break;
    default:
        ImGui::Text("%s: Unsupported type for display_prop_property: %d", prop->hash_name(), (int)prop->type());
        break;
    }
}

void display_prop_property_array(MtProperty* prop) {
    ImGui::Text("%s: Array of properties not supported yet", prop->hash_name());
}

