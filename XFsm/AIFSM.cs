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
            Set(0x18, value.Address);
            NodeCount = value.Count;
        }
    }

    public unsafe void AddNode(AIFSMNode node, MtAllocator? allocator = null)
    {
        if (NodeCount == _nodeCapacity)
        {
            allocator ??= GetAllocator();

            _nodeCapacity += 32;
            var newNodes = new ObjectArray<AIFSMNode>(
                allocator.Alloc(8 * _nodeCapacity), 
                NodeCount + 1
            );
            
            NativeMemory.Copy(newNodes.Pointer, Nodes.Pointer, (nuint)NodeCount * 8);

            allocator.Free(Nodes.Address);
            Nodes = newNodes;
        }

        Nodes[NodeCount] = node;
        NodeCount++;
    }

    public unsafe AIFSMNode AddNode()
    {
        var node = (MtDti.Find("cAIFSMNode")?.CreateInstance<AIFSMNode>()) 
            ?? throw new NullReferenceException("Failed to create AIFSMNode instance");
        AddNode(node);
        return node;
    }

    public unsafe void RemoveNode(int index)
    {
        if (index < 0 || index >= NodeCount)
        {
            throw new IndexOutOfRangeException("Node index out of range");
        }

        Nodes[index].Destroy(true);
        for (var i = index; i < NodeCount - 1; i++)
        {
            Nodes[i] = Nodes[i + 1];
        }

        NodeCount--;
    }

    public unsafe void RemoveNode(AIFSMNode node)
    {
        for (var i = 0; i < NodeCount; i++)
        {
            if (Nodes[i].Instance == node.Instance)
            {
                RemoveNode(i);
                return;
            }
        }
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
    public unsafe ObjectArray<AIFSMLink> Links
    {
        get => new(Get<nint>(0x18), LinkCount);
        set
        {
            Set(0x18, value.Address);
            LinkCount = value.Count;
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

    public unsafe void AddLink(AIFSMLink link, MtAllocator? allocator = null)
    {
        if (LinkCount == _linkCapacity)
        {
            allocator ??= GetAllocator();

            _linkCapacity += 32;
            var newLinks = new ObjectArray<AIFSMLink>(
                allocator.Alloc(8 * _linkCapacity),
                LinkCount + 1
            );

            NativeMemory.Copy(newLinks.Pointer, Links.Pointer, (nuint)LinkCount * 8);

            allocator.Free(Links.Address);
            Links = newLinks;
        }

        Links[LinkCount] = link;
        LinkCount++;
    }

    public unsafe AIFSMLink AddLink()
    {
        var link = MtDti.Find("cAIFSMLink")?.CreateInstance<AIFSMLink>() 
            ?? throw new NullReferenceException("Failed to create AIFSMLink instance");
        AddLink(link);
        return link;
    }

    public unsafe void RemoveLink(int index)
    {
        if (index < 0 || index >= LinkCount)
        {
            throw new IndexOutOfRangeException("Link index out of range");
        }

        Links[index].Destroy(true);
        for (var i = index; i < LinkCount - 1; i++)
        {
            Links.Pointer[i] = Links.Pointer[i + 1];
        }

        LinkCount--;
    }

    public unsafe void RemoveLink(AIFSMLink link)
    {
        for (var i = 0; i < LinkCount; i++)
        {
            if (Links.Pointer[i] == link.Instance)
            {
                RemoveLink(i);
                return;
            }
        }
    }

    public static MtAllocator GetAllocator()
    {
        return InternalCalls.GetAllocator(MtDti.Find("cAIFSMNode"));
    }
}

public unsafe class AIFSMLink : MtObject
{
    public AIFSMLink(nint instance) : base(instance) { }
    public AIFSMLink() { }


    public ref int DestinationNodeId => ref GetRef<int>(0x8);
    public ref bool HasCondition => ref GetRef<bool>(0xC);
    public ref int ConditionId => ref GetRef<int>(0x10);
    public MtString* NamePtr => GetPtr<MtString>(0x18);
    public nint NamePPtr => (nint)NamePtr;

    public string Name
    {
        get => NamePtr->GetString();
        set => InternalCalls.MtStringAssign(NamePPtr, value);
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
