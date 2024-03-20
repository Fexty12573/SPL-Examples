#pragma once

#include "MtProperty.h"

struct MtPropertyIterator {
    using value_type = MtProperty*;
    using pointer = MtProperty**;
    using reference = MtProperty*&;
    using difference_type = std::ptrdiff_t;
    using iterator_category = std::bidirectional_iterator_tag;

    MtPropertyIterator(MtProperty* first) : m_current(first) {}

    MtPropertyIterator& operator++() {
        m_current = m_current->next();
        return *this;
    }

    MtPropertyIterator operator++(int) {
        MtPropertyIterator tmp = *this;
        m_current = m_current->next();
        return tmp;
    }

    MtPropertyIterator& operator--() {
        m_current = m_current->prev();
        return *this;
    }

    MtPropertyIterator operator--(int) {
        MtPropertyIterator tmp = *this;
        m_current = m_current->prev();
        return tmp;
    }

    value_type operator*() {
        return m_current;
    }

    pointer operator->() {
        return &m_current;
    }

    bool operator==(const MtPropertyIterator& other) const {
        return m_current == other.m_current;
    }

    bool operator!=(const MtPropertyIterator& other) const {
        return m_current != other.m_current;
    }

private:
    MtProperty* m_current;
};

class MtPropertyList {
    MtProperty* m_first;
    MtProperty** m_pool;
    uint32_t m_pool_pos;

public:
    virtual ~MtPropertyList() {}

    MtPropertyList(MtPropertyList* (*ctor)(MtPropertyList*)) {
        ctor(this);
    }

    MtPropertyIterator begin() {
        return MtPropertyIterator(m_first);
    }

    MtPropertyIterator end() {
        return MtPropertyIterator(nullptr);
    }
};
SIZE_ASSERT(MtPropertyList, 0x20);
