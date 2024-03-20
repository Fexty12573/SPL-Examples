#pragma once

#include <format>

#include "MtObject.h"

#include <vector>

#define SIZE_ASSERT(TYPE, SIZE) static_assert(sizeof(TYPE) == SIZE, "Size of " #TYPE " is not " #SIZE " bytes.");
#define OFFSET_ASSERT(TYPE, MEMBER, OFFSET) static_assert(offsetof(TYPE, MEMBER) == OFFSET, "Offset of " #MEMBER " in " #TYPE " is not " #OFFSET " bytes.");

enum class PropType {
    Undefined,
    Class,
    ClassRef,
    Bool,
    U8,
    U16,
    U32,
    U64,
    S8,
    S16,
    S32,
    S64,
    F32,
    F64,
    String,
    Color,
    Point,
    Size,
    Rect,
    Matrix44,
    Vector3,
    Vector4,
    Quaternion,
    Property,
    Event,
    Group,
    PageBegin,
    PageEnd,
    Event32,
    Array,
    PropertyList,
    GroupEnd,
    CString,
    Time,
    Float2,
    Float3,
    Float4,
    Float3X3,
    Float4X3,
    Float4X4,
    Easecurve,
    Line,
    Linesegment,
    Ray,
    Plane,
    Sphere,
    Capsule,
    Aabb,
    Obb,
    Cylinder,
    Triangle,
    Cone,
    Torus,
    Ellipsoid,
    Range,
    RangeF,
    RangeU16,
    Hermitecurve,
    Enumlist,
    Float3X4,
    LineSegment4,
    Aabb4,
    Oscillator,
    Variable,
    Vector2,
    Matrix33,
    Rect3dXz,
    Rect3d,
    Rect3dCollision,
    PlaneXz,
    RayY,
    PointF,
    SizeF,
    RectF,
    Event64,
    Bool2,
    End
};

class MtProperty {
    const char* m_name;
    const char* m_comment;
    uint32_t m_type : 12;
    uint32_t : 5;
    uint32_t m_is_array : 1;
    uint32_t : 1;
    uint32_t m_is_property : 1;
    uint32_t : 12;
    MtObject* m_owner;
    union {
        void* m_data;
        void* m_array;
        void* m_get;
    };
    union {
        uint32_t m_count;
        void* m_get_count;
    };
    void* m_set;
    void* m_set_count;
    uint32_t m_index;
    MtProperty* m_prev;
    MtProperty* m_next;

public:
    const char* name() const { return m_name; }
    const char* comment() const { return m_comment; }
    const char* hash_name() const { return m_comment ? m_comment : m_name; }
    PropType type() const { return static_cast<PropType>(m_type); }
    bool is_array() const { return m_is_array; }
    bool is_property() const { return m_is_property; }
    MtObject* owner() const { return m_owner; }
    void* data() const { return m_data; }
    template<typename T> T* data() const { return reinterpret_cast<T*>(m_data); }
    void* array() const { return m_array; }
    uint32_t count() const { return m_count; }
    
    std::string display_name() const { 
        return m_comment
            ? std::format("{} ({})", m_name, m_comment).c_str()
            : m_name;
    }

    template<typename FuncT> FuncT* getter() const { return reinterpret_cast<FuncT*>(m_get); }
    template<typename FuncT> FuncT* count_getter() const { return reinterpret_cast<FuncT*>(m_get_count); }
    template<typename FuncT> FuncT* setter() const { return reinterpret_cast<FuncT*>(m_set); }
    template<typename FuncT> FuncT* count_setter() const { return reinterpret_cast<FuncT*>(m_set_count); }

    template<typename T> T get() const { return getter<T(MtObject*)>()(m_owner); }
    template<typename T> void set(T value) const { setter<void(MtObject*, T)>()(m_owner, value); }
    template<typename T> T get(int index) const { return getter<T(MtObject*, int)>()(m_owner, index); }
    template<typename T> void set(int index, T value) const { setter<void(MtObject*, int, T)>()(m_owner, index, value); }
    uint32_t get_count() const { return count_getter<uint32_t(MtObject*)>()(m_owner); }
    void set_count(uint32_t value) const { count_setter<void(MtObject*, uint32_t)>()(m_owner, value); }

    MtProperty* next() const { return m_next; }
    MtProperty* prev() const { return m_prev; }
};
SIZE_ASSERT(MtProperty, 0x58);
