#pragma once

#include "MtPropertyList.h"

void display_imgui_properties(MtObject* obj, const char* name = nullptr, const char* comment = nullptr);

void display_prop(const MtProperty* prop);
void display_prop_value(const MtProperty* prop);
void display_prop_value_array(const MtProperty* prop);
void display_prop_property(const MtProperty* prop);
void display_prop_property_array(const MtProperty* prop);

struct MtString {
    uint32_t RefCount;
    uint32_t Length;
    char Data[0];
};

typedef MtPropertyList* (*MtPropertyList_ctor)(MtPropertyList*);
typedef MtString** (*MtString_assign)(MtString**, const char*);

void set_callbacks(MtPropertyList_ctor ctor, MtString_assign assign);
