using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;

namespace XFsm.PropertyInjector;

public class Plugin : IPlugin
{
    public string Name => "XFsm.PropertyInjector";
    public string Author => "Fexty";

    #region Property Extensions

    public delegate void MtObjectDtorDelegate(nint obj, uint flags);
    public delegate void PopulatePropertyListDelegate(nint obj, nint list);
    public delegate nint DtiNewDelegate(nint dti);

    private NativeFunction<nint, nint> _newProperty;
    private unsafe sbyte* _mNameString = null;
    private static nint _stringAllocator;

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

    public void OnLoad()
    {
        InjectProperties();
        Log.Info("[XFsm.PropertyInjector] Finished.");
    }

    private unsafe void InjectProperties()
    {
        Log.Debug("Property Injection");

        // MtPropertyList::newElement
        var results = PatternScanner.Scan(Pattern.FromString("48 8B 49 10 8B C0 48 6B C0 58 48 03 01 48 83 C4 28 C3"));
        Ensure.IsTrue(results.Count > 0);
        _newProperty = new NativeFunction<nint, nint>(results[0] - 21);

        Log.Debug($"Found MtPropertyList::newElement at 0x{_newProperty.NativePointer:X}");

        // "mName" string
        var nameStr = "mName\0"u8;
        _mNameString = MemoryUtil.Alloc<sbyte>(nameStr.Length);
        MemoryUtil.WriteBytes((nint)_mNameString, nameStr.ToArray());

        Log.Debug("cAIFSMNode Setup");

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

        var dummyInstance = dti.CreateInstance<MtObject>();
        var vtable = *(nint**)dummyInstance.Instance;
        dummyInstance.Destroy(true);

        Log.Debug("cAIFSMNode Functions:");
        Log.Debug($"  dtiNew: 0x{dtiNew:X}");
        Log.Debug($"  vtable: 0x{(nint)vtable:X}");
        Log.Debug($"  dtor: 0x{vtable[0]:X}");
        Log.Debug($"  populatePropertyList: 0x{vtable[3]:X}");

        _aifsmNodeDtorHook = Hook.Create<MtObjectDtorDelegate>(vtable[0], (obj, flags) =>
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
        _aifsmNodePopulatePropertyListHook = Hook.Create<PopulatePropertyListDelegate>(vtable[3], (obj, plist) =>
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

        Log.Debug("cAIFSMLink Setup");

        // cAIFSMLink setup
        dti = MtDti.Find("cAIFSMLink");
        Ensure.NotNull(dti);

        dummyInstance = dti.CreateInstance<MtObject>();
        vtable = *(nint**)dummyInstance.Instance;
        dummyInstance.Destroy(true);

        dtiNew = dti.GetVirtualFunction(1);
        _aifsmLinkAllocationPatches =
        [
            new Patch(dtiNew + 55, [0x20], true), // Change allocation size to 0x20
        ];

        Log.Debug("cAIFSMLink Functions:");
        Log.Debug($"  dtiNew: 0x{dtiNew:X}");
        Log.Debug($"  vtable: 0x{(nint)vtable:X}");
        Log.Debug($"  dtor: 0x{vtable[0]:X}");
        Log.Debug($"  populatePropertyList: 0x{vtable[3]:X}");

        _aifsmLinkDtorHook = Hook.Create<MtObjectDtorDelegate>(vtable[0], (obj, flags) =>
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
        _aifsmLinkPopulatePropertyListHook = Hook.Create<PopulatePropertyListDelegate>(vtable[3], (obj, plist) =>
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

        var stringFunc = PatternScanner.FindFirst(Pattern.FromString("48 8B D9 48 8B FA 48 8B 09 48 8D 41 08 48 85 C9 75 07"));
        Ensure.IsTrue(stringFunc != 0);
        var readInstr = stringFunc + 49;
        var readOffset = MemoryUtil.Read<int>(readInstr + 3);
        _stringAllocator = readInstr + 7 + readOffset;

        Log.Debug($"String Allocator found at 0x{_stringAllocator:X}");

        return;

        static unsafe void DeallocateString(MtString* str)
        {
            var refCount = str->RefCount;
            str->RefCount -= 1;

            if (refCount == 1)
            {
                var allocator = new MtAllocator(MemoryUtil.Read<nint>(_stringAllocator));
                allocator.Free((nint)str);
            }
        }
    }
}
