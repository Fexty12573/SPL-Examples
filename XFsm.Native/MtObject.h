#pragma once

#include <cstdint>

class MtProperty;
class MtPropertyList;

class MtDti {
    void* m_vft;
    const char* m_name;
    MtDti* m_next;
    MtDti* m_child;
    MtDti* m_parent;
    MtDti* m_link;
    uint32_t m_size : 19;
    uint32_t m_flags : 13;
    uint32_t m_id;

public:
    const char* name() const { return m_name; }
    MtDti* next() const { return m_next; }
    MtDti* child() const { return m_child; }
    MtDti* parent() const { return m_parent; }
    MtDti* link() const { return m_link; }
    uint32_t size() const { return m_size << 2; }
    uint32_t flags() const { return m_flags; }
    uint32_t id() const { return m_id; }
};

class MtObject {
public:
    virtual ~MtObject() = 0;
    virtual void create_ui(MtProperty*) = 0;
    virtual bool is_enable_instance() = 0;
    virtual void create_property(MtPropertyList*) = 0;
    virtual MtDti* get_dti() = 0;
};
