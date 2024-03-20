#pragma once

#include "MtPropertyList.h"

void display_imgui_properties(MtObject* obj);

void display_prop(MtProperty* prop);
void display_prop_value(MtProperty* prop);
void display_prop_value_array(MtProperty* prop);
void display_prop_property(MtProperty* prop);
void display_prop_property_array(MtProperty* prop);

struct MtString {
    uint32_t RefCount;
    uint32_t Length;
    char Data[0];
};

typedef MtPropertyList* (*MtPropertyList_ctor)(MtPropertyList*);
typedef MtString** (*MtString_assign)(MtString**, const char*);

void set_callbacks(MtPropertyList_ctor ctor, MtString_assign assign);
