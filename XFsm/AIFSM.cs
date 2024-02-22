using System.Runtime.InteropServices;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Resources;
using XFsm.ImGuiNodeEditor;

namespace XFsm;

public class AIFSM : Resource
{
    public AIFSM(nint instance) : base(instance) { }
    public AIFSM() { }

    public string OwnerObjectName => GetRef<MtString>(0xA8).GetString();

    public AIFSMCluster? RootCluster => GetObject<AIFSMCluster>(0xB0);

    public AIConditionTree? ConditionTree => GetObject<AIConditionTree>(0xB8);
}

public class AIFSMCluster : AIFSMObject
{
    public AIFSMCluster(nint instance) : base(instance) 
    {
        _nodeCapacity = NodeCount;
    }

    public AIFSMCluster() { _nodeCapacity = 0; }

    private int _nodeCapacity;

    public ref uint OwnerObjectUniqueId => ref GetRef<uint>(0xC);
    public ref uint InitialStateId => ref GetRef<uint>(0x10);
    public ref int NodeCount => ref GetRef<int>(0x14);
    public ObjectArray<AIFSMNode> Nodes
    {
        get => new(Get<nint>(0x18), NodeCount);
        set
        {
            Set(0x18, value.Pointer);
            NodeCount = value.Count;
        }
    }

    public unsafe void AddNode(AIFSMNode node, MtAllocator allocator)
    {
        if (NodeCount == _nodeCapacity)
        {
            _nodeCapacity += 32;
            var newNodes = new ObjectArray<AIFSMNode>(
                allocator.Alloc((nint)node.GetDti()!.Size * _nodeCapacity), 
                NodeCount + 1
            );
            
            for (var i = 0; i < NodeCount; i++)
            {
                newNodes[i] = Nodes[i];
            }

            allocator.Free(Nodes.Pointer);
            Nodes = newNodes;
        }

        Nodes[NodeCount] = node;
        NodeCount++;
    }

    public unsafe AIFSMNode AddNode()
    {
        var node = (MtDti.Find("cAIFSMNode")?.CreateInstance<AIFSMNode>()) 
            ?? throw new NullReferenceException("Failed to create AIFSMNode instance");
        AddNode(node, GetAllocator());
        return node;
    }

    public static MtAllocator GetAllocator()
    {
        return InternalCalls.GetAllocator(MtDti.Find("cAIFSMCluster"));
    }
}

public class AIFSMNode : AIFSMObject
{
    public AIFSMNode(nint instance) : base(instance) { _linkCapacity = LinkCount; }
    public AIFSMNode() { }

    private int _linkCapacity = 0;

    public ref uint UniqueId => ref GetRef<uint>(0xC);
    public ref uint OwnerId => ref GetRef<uint>(0x10);
    public ref int LinkCount => ref GetRef<int>(0x14);
    public unsafe PointerArray<AIFSMLink> Links
    {
        get => new(Get<nint>(0x18), LinkCount);
        set
        {
            Set(0x18, (nint)value.Pointer);
            LinkCount = value.Length;
        }
    }
    public AIFSMCluster? SubCluster => GetObject<AIFSMCluster>(0x20);
    public ref int ProcessCount => ref GetRef<int>(0x28);
    public PointerArray<AIFSMNodeProcess> Processes => new(Get<nint>(0x30), ProcessCount);
    public ref uint Setting => ref GetRef<uint>(0x38);
    public ref uint UserAttribute => ref GetRef<uint>(0x3C);
    public ref uint UIPos => ref GetRef<uint>(0x40);
    public ref byte ColorType => ref GetRef<byte>(0x44);
    public ref bool ExistConditionTransitionFromAll => ref GetRef<bool>(0x48);
    public ref uint ConditionTransitionFromAllId => ref GetRef<uint>(0x4C);
    public unsafe string Name => GetPtr<MtString>(0x50)->GetString();

    public unsafe void AddLink(ref AIFSMLink link, MtAllocator allocator)
    {
        if (LinkCount == _linkCapacity)
        {
            _linkCapacity += 32;
            var newLinks = new PointerArray<AIFSMLink>(
                allocator.Alloc(sizeof(AIFSMLink) * _linkCapacity),
                LinkCount + 1
            );

            for (var i = 0; i < LinkCount; i++)
            {
                // Using .Pointer because we want to copy the pointer values, not the array itself
                newLinks.Pointer[i] = Links.Pointer[i];
            }

            allocator.Free((nint)Links.Pointer);
            Links = newLinks;
        }

        Links.Pointer[LinkCount] = MemoryUtil.AsPointer(ref link);
        LinkCount++;
    }

    public unsafe ref AIFSMLink AddLink()
    {
        var link = MtDti.Find("cAIFSMLink")?.CreateInstance<MtObject>() 
            ?? throw new NullReferenceException("Failed to create AIFSMLink instance");
        var linkPtr = (AIFSMLink*)link.Instance;
        AddLink(ref *linkPtr, GetAllocator());
        return ref *linkPtr; // AddLink does not copy the link itself, so we can return a reference to it
    }

    public static MtAllocator GetAllocator()
    {
        return InternalCalls.GetAllocator(MtDti.Find("cAIFSMNode"));
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public unsafe struct AIFSMLink
{
    [FieldOffset(0x00)] public nint VTable;
    [FieldOffset(0x08)] public int DestinationNodeId;
    [FieldOffset(0x0C)] public bool HasCondition;
    [FieldOffset(0x10)] public int ConditionId;
    [FieldOffset(0x18)] public MtString* NamePtr;

    public string Name
    {
        readonly get => NamePtr->GetString();
        set
        {
            var assign = new NativeAction<nint, string>(0x14031b0b0);
            fixed (MtString** namePtr = &NamePtr)
            {
                assign.Invoke((nint)namePtr, value);
            }
        }
    }

    public static MtAllocator GetAllocator()
    {
        return InternalCalls.GetAllocator(MtDti.Find("cAIFSMLink"));
    }
}

[StructLayout(LayoutKind.Sequential, Size = 0x50)]
public unsafe struct AIFSMNodeProcess
{
    public nint VTable;
    public MtString* ContainerNamePtr;
    public MtString* CategoryNamePtr;
    public nint Parameter;
    public nint UpdateProcess;
    public nint StateProcess;
    public nint ExitProcess;
    public nint StatusChangeProcess;
    public nint ExportProcess;
    public nint ImportProcess;

    public string ContainerName => ContainerNamePtr->GetString();
    public string CategoryName => CategoryNamePtr->GetString();

    public static MtAllocator GetAllocator()
    {
        return InternalCalls.GetAllocator(MtDti.Find("cAIFSMNodeProcess"));
    }
}

public class AIFSMObject : MtObject
{
    public AIFSMObject(nint instance) : base(instance) { }
    public AIFSMObject() { }

    public ref int Id => ref GetRef<int>(0x8);
}
