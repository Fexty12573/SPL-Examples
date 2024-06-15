using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.InternalCallGenerator;

namespace BinaryDtiDumper;

using DtiField = (int StringTableOffset, PropType Type);

internal class DtiClass(string name, List<DtiField> fields, MtDti instance)
{
    public string Name { get; } = name;
    public List<DtiField> Fields { get; } = fields;
    public nint Instance { get; } = instance.Instance;

    // Indices in _dtiList
    public int Parent { get; set; } = -1;
    public List<int> Children { get; } = [];

    public nint ParentPointer { get; } = instance.Parent?.Instance ?? 0;
    public List<nint> ChildrenPointers { get; } = [];
}

public class Plugin : IPlugin
{
    public string Name => "Binary DTI Dumper";
    public string Author => "Fexty";

    private const string OutputPath = "nativePC/plugins/CSharp/dti.bin";

    private readonly List<DtiClass> _dtiList = [];
    private readonly Dictionary<nint, int> _stringOffsetMap = [];
    private readonly List<byte> _stringTable = [];
    private readonly List<string> _failedDti = [];

    private readonly HashSet<string> _dtiBlackList =
    [
        "MtNetServiceError",
        "nGUI::OutlineFontManager",
    ];

    public void OnLoad()
    {
        try
        {
            Task.Run(DumpDti);
        }
        catch (Exception e)
        {
            Log.Error(e.Message);
        }
    }

    private void DumpDti()
    {
        Log.Info("Dumping DTI classes...");

        var dti = MtDti.Find("MtObject");
        if (dti is null)
        {
            Log.Error("Failed to find MtObject DTI");
            return;
        }

        DumpDti(dti);
        ResolveHierarchy();

        using var writer = new BinaryWriter(File.Open(OutputPath, FileMode.Create));
        writer.Write("DTI\0"u8);
        writer.Write(_dtiList.Count);
        writer.Write(_stringTable.Count);
        writer.Write(_stringTable.GetInternalArray().AsSpan(0, _stringTable.Count));
        foreach (var dtiClass in _dtiList)
        {
            //Log.Info($"Writing {dtiClass.Name}");
            writer.Write(Encoding.UTF8.GetBytes(dtiClass.Name));
            writer.Write((byte)0);
            writer.Write((short)dtiClass.Parent);

            writer.Write((short)dtiClass.Children.Count);
            foreach (var child in dtiClass.Children)
            {
                writer.Write((short)child);
            }

            writer.Write(dtiClass.Fields.Count);
            foreach (var field in dtiClass.Fields)
            {
                writer.Write((byte)field.Type);
                writer.Write(field.StringTableOffset + 12); // Offset + 12 to skip the header
            }
        }

        Log.Info($"Dumped {_dtiList.Count} DTI classes to {OutputPath}");
        if (_failedDti.Count > 0)
        {
            Log.Warn($"Failed to dump {_failedDti.Count} DTI classes");
            Log.Warn("Failed DTI classes:");
            foreach (var dtiName in _failedDti)
            {
                Log.Warn(dtiName);
            }
        }

        _dtiList.Clear();
        _stringOffsetMap.Clear();
        _stringTable.Clear();
    }

    private void ResolveHierarchy()
    {
        foreach (var dtiClass in _dtiList)
        {
            dtiClass.Parent = Find(dtiClass.ParentPointer);
            dtiClass.Children.AddRange(dtiClass.ChildrenPointers.Select(Find).Where(x => x != -1));
        }

        return;

        int Find(nint dti)
        {
            return dti != 0 ? _dtiList.FindIndex(x => x.Instance == dti) : -1;
        }
    }

    private void DumpDti(MtDti dti)
    {
        var name = dti.Name;
        Log.Info($"Dumping {name}");

        var fields = DumpFields(dti);
        var dtiClass = new DtiClass(name, fields, dti);
        _dtiList.Add(dtiClass);

        foreach (var childDti in dti.Children)
        {
            dtiClass.ChildrenPointers.Add(childDti.Instance);
            DumpDti(childDti);
        }
    }

    private unsafe List<DtiField> DumpFields(MtDti dti)
    {
        List<DtiField> fields = [];

        if (_dtiBlackList.Contains(dti.Name))
        {
            _failedDti.Add(dti.Name);
            return fields;
        }
        
        MtObject? obj;
        var isSingleton = false;
        if (dti.InheritsFrom("cSystem"))
        {
            obj = GetSingletonInstance(dti);
            isSingleton = true;
        }
        else
        {
            try
            {
                obj = new MtObject((nint)NativeMemory.AlignedAlloc(dti.Size, 0x10));
                if (obj.Instance == 0)
                {
                    obj = null;
                }

                if (obj is not null)
                {
                    NativeMemory.Clear((void*)obj.Instance, dti.Size);
                    var result = Helpers.TryInstantiateObject(obj.Instance, dti.Instance, dti.GetVirtualFunction(2));
                    if (result == 0)
                    {
                        NativeMemory.AlignedFree((void*)obj.Instance);
                        obj = null;
                    }
                }
            }
            catch (Exception e)
            {
                obj = null;
                Log.Error($"Failed to create instance of {dti.Name}:");
                Log.Error(e.Message);
            }
        }

        if (obj is null || obj.Instance == 0)
        {
            _failedDti.Add(dti.Name);
            return fields;
        }

        using var propList = new MtPropertyList();
        try
        {
            if (!Helpers.PopulatePropertyList(obj.Instance, &propList, obj.GetVirtualFunction(3)))
            {
                throw new Exception("Access Violation thrown");
            }
            //var populatePropList = new NativeAction<nint, nint>(obj.GetVirtualFunction(3));
            //populatePropList.Invoke(obj.Instance, (nint)(&propList));
        }
        catch (Exception e)
        {
            _failedDti.Add(dti.Name);
            Log.Error($"Failed to populate property list of {dti.Name}:");
            Log.Error(e.Message);
            return fields;
        }

        var propCount = 0;
        var prop = propList.First;
        while (prop != null)
        {
            var name = prop->HashNamePtr;
            if (!_stringOffsetMap.TryGetValue((nint)name, out var offset))
            {
                offset = _stringTable.Count;
                _stringOffsetMap[(nint)name] = offset;
                var strLen = MemoryUtil.StringLength(name);
                if (strLen > 0)
                {
                    _stringTable.AddRange(MemoryUtil.ReadBytes((nint)name, strLen));
                }
                else
                {
                    Log.Warn($"Failed to read string at {(nint)name:X}, skipped property {propCount} for {dti.Name}");
                }

                _stringTable.Add(0);
            }
            
            fields.Add((offset, prop->Type));
            propCount++;

            prop = prop->Next;
        }

        if (!isSingleton)
        {
            obj.Destroy(false);
            NativeMemory.AlignedFree((void*)obj.Instance);
        }

        return fields;
    }

    private static MtObject? GetSingletonInstance(MtDti dti)
    {
        var obj = SingletonManager.GetSingleton(dti.Name);
        if (obj is not null)
        {
            return obj;
        }

        foreach (var childDti in dti.AllChildren)
        {
            obj = SingletonManager.GetSingleton(childDti.Name);
            if (obj is not null)
            {
                return obj;
            }
        }

        return null;
    }
}

public static class ListExtensions
{
    private static class ArrayAccessor<T>
    {
        public static readonly Func<List<T>, T[]> Getter;

        static ArrayAccessor()
        {
            var dm = new DynamicMethod("get", MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard, typeof(T[]), new Type[] { typeof(List<T>) }, typeof(ArrayAccessor<T>), true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0); // Load List<T> argument
            il.Emit(OpCodes.Ldfld, typeof(List<T>).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)!); // Replace argument by field
            il.Emit(OpCodes.Ret); // Return field
            Getter = (Func<List<T>, T[]>)dm.CreateDelegate(typeof(Func<List<T>, T[]>));
        }
    }

    public static T[] GetInternalArray<T>(this List<T> list)
    {
        return ArrayAccessor<T>.Getter(list);
    }
}

[InternalCallManager]
public static partial class Helpers
{
    [InternalCall]
    public static partial nint TryInstantiateObject(nint obj, nint dti, nint ctor);

    [InternalCall]
    public static unsafe partial bool PopulatePropertyList(nint obj, void* propList, nint populate);
}
