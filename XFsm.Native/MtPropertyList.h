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
        const MtPropertyIterator tmp = *this;
        m_current = m_current->next();
        return tmp;
    }

    MtPropertyIterator& operator--() {
        m_current = m_current->prev();
        return *this;
    }

    MtPropertyIterator operator--(int) {
        const MtPropertyIterator tmp = *this;
        m_current = m_current->prev();
        return tmp;
    }

    value_type operator*() const {
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
    MtProperty* m_first = nullptr;
    MtProperty** m_pool = nullptr;
    uint32_t m_pool_pos = 0;

public:
    virtual ~MtPropertyList() = default;

    explicit MtPropertyList(MtPropertyList* (*ctor)(MtPropertyList*)) {
        ctor(this);
    }

    MtPropertyIterator begin() {
        return { m_first };
    }

    MtPropertyIterator end() {
        return { nullptr };
    }
};
SIZE_ASSERT(MtPropertyList, 0x20);
