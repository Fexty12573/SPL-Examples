#include "pch.h"
#include "SPL/InternalCall.h"

namespace SPLNative = SharpPluginLoader::Native;

namespace {
void* try_instantiate_object(void* object, void* dti, void(*instantiate)(void* pdti, void* pobj)) {
    __try {
        instantiate(dti, object);
    } __except (EXCEPTION_EXECUTE_HANDLER) {
        return nullptr;
    }

    return object;
}

bool populate_property_list(void* object, void* prop_list, void(*populate)(void* pobj, void* plist)) {
    __try {
        populate(object, prop_list);
    } __except (EXCEPTION_EXECUTE_HANDLER) {
        return false;
    }

    return true;
}

}

SPL_INTERNAL_CALL int get_internal_call_count() {
    return 2;
}

SPL_INTERNAL_CALL void collect_internal_calls(SPLNative::InternalCall* icalls) {
    icalls[0] = SPLNative::InternalCall{ "TryInstantiateObject", &try_instantiate_object };
    icalls[1] = SPLNative::InternalCall{ "PopulatePropertyList", &populate_property_list };
}

BOOL APIENTRY DllMain(HMODULE, DWORD, LPVOID) {
    return TRUE;
}

