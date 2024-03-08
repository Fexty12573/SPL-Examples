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

    public unsafe string OwnerObjectName => GetPtr<MtString>(0xA8)->GetString();

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
        get => new(Get<nint>(0x18), NodeCount, ptr => new AIFSMNode(Instance));
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
                NodeCount + 1,
                ptr => new AIFSMNode(Instance)
            );
            
            NativeMemory.Copy(newNodes.Pointer, Nodes.Pointer, (nuint)NodeCount * 8);
            newNodes[NodeCount] = node;

            allocator.Free(Nodes.Address);
            Nodes = newNodes;
        }
        else
        {
            Nodes[NodeCount] = node;
            NodeCount++;
        }
    }

    public AIFSMNode AddNode(string name)
    {
        var node = (MtDti.Find("cAIFSMNode")?.CreateInstance<AIFSMNode>()) 
            ?? throw new NullReferenceException("Failed to create AIFSMNode instance");

        AddNode(node);
        node.Name = name;

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
    public AIFSMNode(nint instance) : base(instance)
    {
        _linkCapacity = LinkCount;
        _processCapacity = ProcessCount;
    }
    public AIFSMNode() { }

    private int _linkCapacity = 0;
    private int _processCapacity = 0;

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
    public ObjectArray<AIFSMNodeProcess> Processes
    {
        get => new(Get<nint>(0x30), ProcessCount);
        set
        {
            Set(0x30, value.Address);
            ProcessCount = value.Count;
        }
    }
    public ref uint Setting => ref GetRef<uint>(0x38);
    public ref uint UserAttribute => ref GetRef<uint>(0x3C);
    public ref uint UIPos => ref GetRef<uint>(0x40);
    public ref byte ColorType => ref GetRef<byte>(0x44);
    public ref bool ExistConditionTransitionFromAll => ref GetRef<bool>(0x48);
    public ref uint ConditionTransitionFromAllId => ref GetRef<uint>(0x4C);

    public unsafe string Name
    {
        get => GetPtr<MtString>(0x50)->GetString();
        set => InternalCalls.MtStringAssign(Instance + 0x50, value);
    }

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
            newLinks[LinkCount] = link;

            allocator.Free(Links.Address);
            Links = newLinks;
        }
        else
        {
            Links[LinkCount] = link;
            LinkCount++;
        }
    }

    public AIFSMLink AddLink(string name)
    {
        var link = MtDti.Find("cAIFSMLink")?.CreateInstance<AIFSMLink>() 
            ?? throw new NullReferenceException("Failed to create AIFSMLink instance");

        AddLink(link);
        link.Name = name;

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

    public unsafe void AddProcess(AIFSMNodeProcess process, MtAllocator? allocator = null)
    {
        if (ProcessCount == _processCapacity)
        {
            allocator ??= GetAllocator();

            _processCapacity += 32;
            var newProcesses = new ObjectArray<AIFSMNodeProcess>(
                allocator.Alloc(8 * _processCapacity),
                ProcessCount + 1
            );

            NativeMemory.Copy(newProcesses.Pointer, Processes.Pointer, (nuint)ProcessCount * 8);
            newProcesses[ProcessCount] = process;

            allocator.Free(Processes.Address);
            Processes = newProcesses;
        }
        else
        {
            Processes[ProcessCount] = process;
            ProcessCount++;
        }
    }

    public AIFSMNodeProcess AddProcess(string containerName)
    {
        var process = MtDti.Find("cAIFSMNodeProcess")?.CreateInstance<AIFSMNodeProcess>() 
            ?? throw new NullReferenceException("Failed to create AIFSMNodeProcess instance");

        AddProcess(process);
        process.ContainerName = containerName;

        return process;
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
    public nint NamePPtr => Instance + 0x18;

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

public unsafe class AIFSMNodeProcess : MtObject
{
    public AIFSMNodeProcess(nint instance) : base(instance) { }
    public AIFSMNodeProcess() { }

    public string ContainerName
    {
        get => GetPtr<MtString>(0x8)->GetString();
        set => InternalCalls.MtStringAssign(Instance + 0x8, value);
    }

    public string CategoryName
    {
        get => GetPtr<MtString>(0x10)->GetString();
        set => InternalCalls.MtStringAssign(Instance + 0x10, value);
    }

    public MtObject? Parameter
    {
        get => GetObject<MtObject>(0x18);
        set => Set(0x18, value?.Instance ?? 0);
    }

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
