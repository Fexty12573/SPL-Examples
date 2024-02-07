using System.Runtime.InteropServices;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Resources;

namespace FsmEditor;

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
    public AIFSMCluster(nint instance) : base(instance) { }
    public AIFSMCluster() { }

    public ref uint OwnerObjectUniqueId => ref GetRef<uint>(0xC);
    public ref uint InitialStateId => ref GetRef<uint>(0x10);
    public ref int NodeCount => ref GetRef<int>(0x14);
    public ObjectArray<AIFSMNode> Nodes => new(Get<nint>(0x18), NodeCount);
}

public class AIFSMNode : AIFSMObject
{
    public AIFSMNode(nint instance) : base(instance) { }
    public AIFSMNode() { }

    public ref uint UniqueId => ref GetRef<uint>(0xC);
    public ref uint OwnerId => ref GetRef<uint>(0x10);
    public ref int LinkCount => ref GetRef<int>(0x14);
    public PointerArray<AIFSMLink> Links => new(Get<nint>(0x18), LinkCount);
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
}

[StructLayout(LayoutKind.Explicit, Size = 0x18)]
public unsafe struct AIFSMLink
{
    [FieldOffset(0x00)] public nint VTable;
    [FieldOffset(0x08)] public uint DestinationNodeId;
    [FieldOffset(0x0C)] public bool HasCondition;
    [FieldOffset(0x10)] public uint ConditionId;
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
}

public class AIFSMObject : MtObject
{
    public AIFSMObject(nint instance) : base(instance) { }
    public AIFSMObject() { }

    public ref uint Id => ref GetRef<uint>(0x8);
}
