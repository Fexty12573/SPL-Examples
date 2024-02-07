using System.Text;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Resources;

namespace FsmEditor;

public unsafe class Plugin : IPlugin
{
    public string Name => "WpfLibrary1";
    public string Author => "Fexty";

    private static readonly MainWindow Window = new();
    private Thread _wpfThread = null!;

    #region Property Extensions

    public delegate void MtObjectDtorDelegate(nint obj, uint flags);
    public delegate void PopulatePropertyListDelegate(nint obj, nint list);
    public delegate nint DtiNewDelegate(nint dti);

    private NativeFunction<nint, nint> _newProperty;
    private unsafe sbyte* _mNameString = null;

    #region AIFSMNode Name Property

    private Patch[]? _aifsmNodeAllocationPatches;
    private Hook<MtObjectDtorDelegate> _aifsmNodeDtorHook = null!;
    private Hook<PopulatePropertyListDelegate> _aifsmNodePopulatePropertyListHook = null!;
    private Hook<DtiNewDelegate> _aifsmNodeDtiNewHook = null!;

    #endregion
    #region AIFSMLink Name Property

    private Patch[]? _aifsmLinkAllocationPatches;
    private Hook<MtObjectDtorDelegate> _aifsmLinkDtorHook = null!;
    private Hook<PopulatePropertyListDelegate> _aifsmLinkPopulatePropertyListHook = null!;
    private Hook<DtiNewDelegate> _aifsmLinkDtiNewHook = null!;

    #endregion

    #endregion

    public PluginData Initialize()
    {
        return new PluginData { OnUpdate = true };
    }

    public void OnLoad()
    {
        InitializePropertyExtensions();

        _wpfThread = new Thread(() =>
        {
            Window.ShowDialog();
        });

        _wpfThread.SetApartmentState(ApartmentState.STA);
        _wpfThread.Start();
    }

    public void OnUpdate(float deltaTime)
    {
        if (Window.LoadFsm)
        {
            Window.LoadFsm = false;

            var testFsm = ResourceManager.GetResource<Resource>(@"hm\wp\wp00\wp00_action", MtDti.Find("rAIFSM")!);
            Ensure.NotNull(testFsm);

            var fsm = new AIFSM(testFsm.Instance);
            Ensure.NotNull(fsm.RootCluster);
            Log.Info($"{fsm.FilePath} Nodes:");
            foreach (var node in fsm.RootCluster.Nodes)
            {
                Log.Info(node.Name);
            }
        }
    }

    private void InitializePropertyExtensions()
    {
        // MtPropertyList::newElement
        var results = PatternScanner.Scan(Pattern.FromString("48 8B 49 10 8B C0 48 6B C0 58 48 03 01 48 83 C4 28 C3"));
        Ensure.IsTrue(results.Count > 0);
        _newProperty = new NativeFunction<nint, nint>(results[0] - 21);

        // "mName" string
        var nameStr = "mName\0"u8;
        _mNameString = MemoryUtil.Alloc<sbyte>(nameStr.Length);
        MemoryUtil.WriteBytes((nint)_mNameString, nameStr.ToArray());

        // cAIFSMNode setup
        var dti = MtDti.Find("cAIFSMNode");
        Ensure.NotNull(dti);

        var dtiNew = dti.GetVirtualFunction(1);
        _aifsmNodeAllocationPatches =
        [
            //new Patch(dtiNew + 22, [0x0F, 0x57, 0xC0, 0x90, 0x90, 0x90, 0x90], true), // xorps xmm0, xmm0
            new Patch(dtiNew + 55, [0x58], true), // Change allocation size to 0x58
            //new Patch(dtiNew + 128, [0x0F, 0x29, 0x43, 0x48], true), // movaps [rbx+0x48], xmm0
        ];
        _aifsmNodeDtorHook = Hook.Create<MtObjectDtorDelegate>(0x142455540, (obj, flags) =>
        {
            if ((flags & 1) != 0) // Check if the object is being deleted
            {
                var str = MemoryUtil.ReadPointer<MtString>(obj + 0x50);
                if (str != null)
                {
                    DeallocateString(str);
                }
            }

            _aifsmNodeDtorHook.Original(obj, flags);
        });
        _aifsmNodePopulatePropertyListHook = Hook.Create<PopulatePropertyListDelegate>(0x1424565a0, (obj, plist) =>
        {
            _aifsmNodePopulatePropertyListHook.Original(obj, plist);

            var list = new MtPropertyList(plist);
            ref var nameProperty = ref *(MyMtProperty*)_newProperty.Invoke(list.Instance);
            nameProperty.NamePtr = _mNameString;
            nameProperty.CommentPtr = null;
            nameProperty.Type = PropType.String;
            nameProperty.Flags = 0;
            nameProperty.Owner = obj;
            nameProperty.Address = obj + 0x50;
            nameProperty.GetCount = 0;
            nameProperty.Set = 0;
            nameProperty.SetCount = 0;
            nameProperty.Index = 0;
            nameProperty.Prev = null;
            nameProperty.Next = null;

            list.AddProperty(ref nameProperty);
        });
        _aifsmNodeDtiNewHook = Hook.Create<DtiNewDelegate>(dtiNew, dtiObj =>
        {
            var result = _aifsmNodeDtiNewHook.Original(dtiObj);
            MemoryUtil.GetRef<nint>(result + 0x50) = 0;

            return result;
        });

        // cAIFSMLink setup
        dti = MtDti.Find("cAIFSMLink");
        Ensure.NotNull(dti);

        dtiNew = dti.GetVirtualFunction(1);
        _aifsmLinkAllocationPatches =
        [
            new Patch(dtiNew + 55, [0x20], true), // Change allocation size to 0x20
        ];
        _aifsmLinkDtorHook = Hook.Create<MtObjectDtorDelegate>(0x1424554d0, (obj, flags) =>
        {
            if ((flags & 1) != 0) // Check if the object is being deleted
            {
                var str = MemoryUtil.ReadPointer<MtString>(obj + 0x18);
                if (str != null)
                {
                    DeallocateString(str);
                }
            }

            _aifsmLinkDtorHook.Original(obj, flags);
        });
        _aifsmLinkPopulatePropertyListHook = Hook.Create<PopulatePropertyListDelegate>(0x1424563b0, (obj, plist) =>
        {
            _aifsmLinkPopulatePropertyListHook.Original(obj, plist);

            var list = new MtPropertyList(plist);
            ref var nameProperty = ref *(MyMtProperty*)_newProperty.Invoke(list.Instance);
            nameProperty.NamePtr = _mNameString;
            nameProperty.CommentPtr = null;
            nameProperty.Type = PropType.String;
            nameProperty.Flags = 0;
            nameProperty.Owner = obj;
            nameProperty.Address = obj + 0x18;
            nameProperty.GetCount = 0;
            nameProperty.Set = 0;
            nameProperty.SetCount = 0;
            nameProperty.Index = 0;
            nameProperty.Prev = null;
            nameProperty.Next = null;

            list.AddProperty(ref nameProperty);
        });
        _aifsmLinkDtiNewHook = Hook.Create<DtiNewDelegate>(dtiNew, dtiObj =>
        {
            var result = _aifsmLinkDtiNewHook.Original(dtiObj);
            MemoryUtil.GetRef<nint>(result + 0x18) = 0;

            return result;
        });

        return;

        static unsafe void DeallocateString(MtString* str)
        {
            var refCount = str->RefCount;
            str->RefCount -= 1;

            if (refCount == 1)
            {
                var allocator = new MtAllocator(MemoryUtil.Read<nint>(0x143fdca50));
                allocator.Free((nint)str);
            }
        }
    }
}

