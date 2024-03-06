using System.Runtime.InteropServices;
using SharpPluginLoader.Core;

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
    public ref AIDEnum Name => ref GetRefInline<AIDEnum>(0x8);
    public AIConditionTreeNode? RootNode => GetObject<AIConditionTreeNode>(0x20);
}

public class AIConditionTreeNode : MtObject
{
    public AIConditionTreeNode(nint instance) : base(instance) { }
    public AIConditionTreeNode() { }

    public ref int ChildCount => ref GetRef<int>(0x8);
    public ObjectArray<AIConditionTreeNode> Children => new(Get<nint>(0x10), ChildCount);
    public AIConditionTreeNode? Parent => GetObject<AIConditionTreeNode>(0x38);
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

    public string Value => GetRef<MtString>(0x40).GetString();
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

        public string PropertyName => PropertyNamePtr->GetString();
        public string OwnerName => OwnerNamePtr->GetString();
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
}

[StructLayout(LayoutKind.Sequential, Size = 0x18)]
public unsafe struct AIDEnum
{
    public nint VTable;
    public MtString* NamePtr;
    public int Id;

    public string Name => NamePtr->GetString();
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
