using System.Runtime.InteropServices;
using SharpPluginLoader.Core;
using XFsm.ImGuiNodeEditor;

namespace XFsm;

public class AIConditionTree : MtObject
{
    public AIConditionTree(nint instance) : base(instance) { }
    public AIConditionTree() { }

    public ref int TreeInfoCount => ref GetRef<int>(0xA8);
    public ObjectArray<AIConditionTreeInfo> TreeList => new(Get<nint>(0xB0), TreeInfoCount);
}

public class AIConditionTreeInfo : MtObject
{
    public AIConditionTreeInfo(nint instance) : base(instance) { }
    public AIConditionTreeInfo() { }

    public ref AIDEnum Name => ref GetRefInline<AIDEnum>(0x8);
    public AIConditionTreeNode? RootNode
    {
        get => new(Get<nint>(0x20));
        set
        {
            RootNode?.Destroy(true);
            Set(0x20, value?.Instance ?? 0);
        }
    }
}

public enum ConditionTreeNodeType
{
    ConstEnumNode,
    ConstF32Node,
    ConstF64Node,
    ConstS32Node,
    ConstS64Node,
    ConstStringNode,
    OperationNode,
    StateNode,
    VariableNode,

    None = -1
}

public class AIConditionTreeNode : MtObject
{
    public AIConditionTreeNode(nint instance) : base(instance) 
    {
        Type = GetDti()?.Name switch
        {
            "rAIConditionTree::ConstEnumNode" => ConditionTreeNodeType.ConstEnumNode,
            "rAIConditionTree::ConstF32Node" => ConditionTreeNodeType.ConstF32Node,
            "rAIConditionTree::ConstF64Node" => ConditionTreeNodeType.ConstF64Node,
            "rAIConditionTree::ConstS32Node" => ConditionTreeNodeType.ConstS32Node,
            "rAIConditionTree::ConstS64Node" => ConditionTreeNodeType.ConstS64Node,
            "rAIConditionTree::ConstStringNode" => ConditionTreeNodeType.ConstStringNode,
            "rAIConditionTree::OperationNode" => ConditionTreeNodeType.OperationNode,
            "rAIConditionTree::StateNode" => ConditionTreeNodeType.StateNode,
            "rAIConditionTree::VariableNode" => ConditionTreeNodeType.VariableNode,
            _ => throw new InvalidOperationException("Invalid condition tree node type.")
        };
        _capacity = ChildCount;
    }
    public AIConditionTreeNode() { _capacity = 0; }

    public ConditionTreeNodeType Type { get; init; }
    private int _capacity;

    public ref int ChildCount => ref GetRef<int>(0x8);
    public ObjectArray<AIConditionTreeNode> Children
    {
        get => new(Get<nint>(0x10), ChildCount, ptr => new AIConditionTreeNode(ptr));
        set
        {
            Set(0x10, value.Address);
            ChildCount = value.Count;
        }
    }
    public AIConditionTreeNode? Parent => GetObject<AIConditionTreeNode>(0x38);
    public ref T Value<T>(int extraOffset = 0) where T : unmanaged => ref GetRef<T>(0x40 + extraOffset);
    public T? Object<T>(int extraOffset = 0) where T : MtObject, new() => GetObject<T>(0x40 + extraOffset);

    public static AIConditionTreeNode Create(ConditionTreeNodeType type)
    {
        var node = GetDti(type)?.CreateInstance<AIConditionTreeNode>()
            ?? throw new NullReferenceException("Failed to create AIConditionTreeNode instance");
        return node;
    }

    public void SetChild(int index, AIConditionTreeNode node)
    {
        if (index < 0 || index >= ChildCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        Children[index].Destroy(true);
        Children[index] = node;
    }

    public void SetChild(int index, ConditionTreeNodeType type)
    {
        SetChild(index, Create(type));
    }

    public unsafe void AddChild(AIConditionTreeNode node, MtAllocator? allocator = null)
    {
        if (ChildCount >= _capacity)
        {
            allocator ??= GetAllocator();

            _capacity += 8;
            var newChildren = new ObjectArray<AIConditionTreeNode>(
                allocator.Alloc(8 * _capacity),
                ChildCount + 1,
                ptr => new AIConditionTreeNode(ptr)
            );

            NativeMemory.Copy(newChildren.Pointer, Children.Pointer, (nuint)ChildCount * 8);
            newChildren[ChildCount] = node;

            allocator.Free(Children.Address);
            Children = newChildren;
        }
        else
        {
            Children[ChildCount] = node;
            ChildCount++;
        }
    }

    public AIConditionTreeNode AddChild(ConditionTreeNodeType type)
    {
        var node = Create(type);
        AddChild(node);
        return node;
    }

    public void RemoveChild(int index)
    {
        if (index < 0 || index >= ChildCount)
            throw new ArgumentOutOfRangeException(nameof(index));

        var children = Children; // Avoid multiple instatiation of ObjectArray
        children[index].Destroy(true);

        for (var i = index; i < ChildCount - 1; i++)
        {
            children[i] = children[i + 1];
        }

        ChildCount--;
    }

    public void RemoveChild(AIConditionTreeNode node)
    {
        for (var i = 0; i < ChildCount; i++)
        {
            if (Children[i].Instance == node.Instance)
            {
                RemoveChild(i);
                return;
            }
        }
    }

    private static MtAllocator GetAllocator()
    {
        return InternalCalls.GetAllocator(MtDti.Find("rAIConditionTreeNode"));
    }

    private static MtDti? GetDti(ConditionTreeNodeType type)
    {
        return MtDti.Find($"rAIConditionTree::{type}");
    }
}

public class AIConditionTreeConstEnumNode : AIConditionTreeNode
{
    public AIConditionTreeConstEnumNode(nint instance) : base(instance) { }
    public AIConditionTreeConstEnumNode() { }

    public ref int Value => ref GetRef<int>(0x48);
    public ref bool IsBitNo => ref GetRef<bool>(0x70);
}

public class AIConditionTreeConstF32Node : AIConditionTreeNode
{
    public AIConditionTreeConstF32Node(nint instance) : base(instance) { }
    public AIConditionTreeConstF32Node() { }

    public ref float Value => ref GetRef<float>(0x40);
}

public class AIConditionTreeConstF64Node : AIConditionTreeNode
{
    public AIConditionTreeConstF64Node(nint instance) : base(instance) { }
    public AIConditionTreeConstF64Node() { }

    public ref double Value => ref GetRef<double>(0x40);
}

public class AIConditionTreeConstS32Node : AIConditionTreeNode
{
    public AIConditionTreeConstS32Node(nint instance) : base(instance) { }
    public AIConditionTreeConstS32Node() { }

    public ref int Value => ref GetRef<int>(0x40);
}

public class AIConditionTreeConstS64Node : AIConditionTreeNode
{
    public AIConditionTreeConstS64Node(nint instance) : base(instance) { }
    public AIConditionTreeConstS64Node() { }

    public ref long Value => ref GetRef<long>(0x40);
}

public class AIConditionTreeConstStringNode : AIConditionTreeNode
{
    public AIConditionTreeConstStringNode(nint instance) : base(instance) { }
    public AIConditionTreeConstStringNode() { }

    public string Value
    {
        get => GetRef<MtString>(0x40).GetString();
        set => InternalCalls.MtStringAssign(Instance + 0x40, value);
    }
}

public class AIConditionTreeOperationNode : AIConditionTreeNode
{
    public AIConditionTreeOperationNode(nint instance) : base(instance) { }
    public AIConditionTreeOperationNode() { }

    public ref OperatorType Operator => ref GetRef<OperatorType>(0x40);
}

public class AIConditionTreeStateNode : AIConditionTreeNode
{
    public AIConditionTreeStateNode(nint instance) : base(instance) { }
    public AIConditionTreeStateNode() { }

    public ref int StateId => ref GetRef<int>(0x40);
}

public class AIConditionTreeVariableNode : AIConditionTreeNode
{
    public AIConditionTreeVariableNode(nint instance) : base(instance) { }
    public AIConditionTreeVariableNode() { }

    public ref VariableInfo Variable => ref GetRefInline<VariableInfo>(0x40);
    public ref bool IsBitNo => ref GetRef<bool>(0x60);
    public ref bool IsArray => ref GetRef<bool>(0x61);
    public ref bool IsDynamicIndex => ref GetRef<bool>(0x62);
    public ref int Index => ref GetRef<int>(0x64);
    public ref VariableInfo IndexVariable => ref GetRefInline<VariableInfo>(0x68);
    public ref bool UseIndexEnum => ref GetRef<bool>(0x88);
    public ref EnumProp IndexEnum => ref GetRefInline<EnumProp>(0x90);


    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    public unsafe struct VariableInfo
    {
        public nint VTable;
        public MtString* PropertyNamePtr;
        public MtString* OwnerNamePtr;
        public bool IsSingletonOwner;

        public string PropertyName
        {
            get => PropertyNamePtr->GetString();
            set
            {
                fixed (MtString** ptr = &PropertyNamePtr)
                    InternalCalls.MtStringAssign((nint)ptr, value);
            }
        }
        public string OwnerName
        {
            get => OwnerNamePtr->GetString();
            set
            {
                fixed (MtString** ptr = &OwnerNamePtr)
                    InternalCalls.MtStringAssign((nint)ptr, value);
            }
        }
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
public unsafe struct EnumProp
{
    public nint VTable;
    public int Value;
    public MtString* NamePtr;
    public MtString* EnumNamePtr;
    public uint NameCrc;
    public uint EnumNameCrc;

    public string Name
    {
        get => NamePtr->GetString();
        set
        {
            fixed (MtString** ptr = &NamePtr)
                InternalCalls.MtStringAssign((nint)ptr, value);
        }
    }

    public string EnumName
    {
        get => EnumNamePtr->GetString();
        set
        {
            fixed (MtString** ptr = &EnumNamePtr)
                InternalCalls.MtStringAssign((nint)ptr, value);
        }
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public unsafe struct AIDEnum
{
    public nint VTable;
    public MtString* NamePtr;
    public int Id;

    public string Name
    {
        get => NamePtr != null ? NamePtr->GetString() : "N/A";
        set
        {
            fixed (MtString** ptr = &NamePtr)
                InternalCalls.MtStringAssign((nint)ptr, value);
        }
    }
}

public enum OperatorType : int
{
    None = 0,
    IsTrue = 1,
    IsFalse = 2,
    Equal = 3,
    NotEqual = 4,
    LessThan = 5,
    LessThanOrEqual = 6,
    GreaterThan = 7,
    GreaterThanOrEqual = 8,
    BitAnd = 9,
    BitOr = 10,
    And = 16,
    Or = 17
}
