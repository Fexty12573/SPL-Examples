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

internal class DtiType
{
    public string Name { get; set; }
    public nint Vtable { get; set; }
    public MtDti Dti { get; }
    public List<DtiProperty> Properties { get; }

    public string GetInheritanceString()
    {
        var sb = new StringBuilder();
        sb.Append(Dti.Name);
        var parent = Dti.Parent;
        while (parent != null)
        {
            sb.Append(", ");
            sb.Append(parent.Name);
            parent = parent.Parent;
        }

        return sb.ToString();
    }

    public unsafe DtiType(MtDti dti, MtPropertyList* propList, nint vtable)
    {
        Name = dti.Name;
        Vtable = vtable;
        Dti = dti;
        Properties = [];

        var prop = propList->First;
        while (prop != null)
        {
            Properties.Add(new DtiProperty(prop));
            prop = prop->Next;
        }

        Properties.Sort((a, b) => a.Offset.CompareTo(b.Offset));
    }
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

    private readonly List<DtiType> _dtiTypes = [];

    private readonly HashSet<string> _dtiBlackList =
    [
        "MtNetServiceError",
        "nGUI::OutlineFontManager",
    ];

    public void OnLoad()
    {
        try
        {
            //Task.Run(DumpDti);
            Task.Run(DumpDtiPlainText);
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

    private void DumpDtiPlainText()
    {
        Log.Info("Dumping DTI classes (Plain Text)...");

        var dti = MtDti.Find("MtObject");
        if (dti is null)
        {
            Log.Error("Failed to find MtObject DTI");
            return;
        }

        DumpDtiPlainText(dti);

        _dtiTypes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));

        using var writer = new StreamWriter("dti_dump.h");
        writer.WriteLine($"// Fexty's MHW DTI Dump log [{DateTime.Now}] (Inspired by Ando's DTI Dumper)");
        writer.WriteLine();
        writer.WriteLine();

        foreach (var type in _dtiTypes)
        {
            writer.WriteLine($"// {type.Name} vftable:0x{type.Vtable:X}, Size:0x{type.Dti.Size:X}, CRC32:0x{type.Dti.Id:X}");

            var inheritance = type.GetInheritanceString();
            writer.WriteLine(!string.IsNullOrEmpty(inheritance)
                ? $"class {type.Name} /*: {inheritance}*/ {{"
                : $"class {type.Name} /**/ {{");

            foreach (var prop in type.Properties)
            {
                if (prop.Comment is not null)
                {
                    writer.WriteLine($"    // Comment: {prop.Comment}");
                }

                var typeAndName = $"{prop.GetTypeName()} '{prop.Name}'";
                var offset = prop.Offset;
                if (offset < 0 || offset > type.Dti.Size)
                {
                    offset = long.MaxValue;
                }

                if (prop.IsArray)
                {
                    if (prop.IsProperty)
                    {
                        var varString = $"{typeAndName}[*]";
                        writer.WriteLine($"    {varString,-50}; // Offset:0x{offset:X}, DynamicArray, Getter:0x{prop.Get:X}, Setter:0x{prop.SetData:X}, GetCount:0x{prop.GetCount:X}, Reallocate:0x{prop.SetCount:X}, CRC32:0x{prop.Hash:X}, Flags:0x{prop.Flags:X}");
                    }
                    else
                    {
                        var varString = $"{typeAndName}[{prop.Count}]";
                        writer.WriteLine($"    {varString,-50}; // Offset:0x{offset:X}, Array, CRC32:0x{prop.Hash:X}, Flags:0x{prop.Flags:X}");
                    }
                }
                else if (prop.IsProperty)
                {
                    writer.WriteLine($"    {typeAndName,-50}; // Offset:0x{offset:X}, PSEUDO-PROP, Getter:0x{prop.Get:X}, Setter:0x{prop.SetData:X}, CRC32:0x{prop.Hash:X}, Flags:0x{prop.Flags:X}");
                }
                else
                {
                    writer.WriteLine($"    {typeAndName,-50}; // Offset:0x{offset:X}, Var, CRC32:0x{prop.Hash:X}, Flags:0x{prop.Flags:X}");
                }
            }

            writer.WriteLine("};");
            writer.WriteLine();
        }

        writer.WriteLine("// END OF FILE");
        writer.Flush();

        Log.Info("Dumped DTI classes to dti_dump.h");
    }

    private unsafe void DumpDtiPlainText(MtDti dti)
    {
        Log.Info($"Dumping {dti.Name}");

        var type = DumpDataPlainText(dti);
        if (type is not null)
        {
            _dtiTypes.Add(type);
        }

        foreach (var childDti in dti.Children)
        {
            DumpDtiPlainText(childDti);
        }
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

    private unsafe DtiType? DumpDataPlainText(MtDti dti)
    {
        if (_dtiBlackList.Contains(dti.Name))
        {
            _failedDti.Add(dti.Name);
            return null;
        }

        var obj = InstantiateObject(dti, out var isSingleton);

        if (obj is null || obj.Instance == 0 || obj.Get<nint>(0) == 0)
        {
            _failedDti.Add(dti.Name);
            return null;
        }

        var vtable = obj.Get<nint>(0);

        using var propList = new MtPropertyList();
        try
        {
            if (!Helpers.PopulatePropertyList(obj.Instance, &propList, obj.GetVirtualFunction(3)))
            {
                throw new Exception("Access Violation thrown");
            }
        }
        catch (Exception e)
        {
            _failedDti.Add(dti.Name);
            Log.Error($"Failed to populate property list of {dti.Name}:");
            Log.Error(e.Message);

            if (!isSingleton)
            {
                obj.Destroy(false);
                NativeMemory.AlignedFree((void*)obj.Instance);
            }

            return null;
        }

        var type = new DtiType(dti, &propList, vtable);

        if (!isSingleton)
        {
            obj.Destroy(false);
            NativeMemory.AlignedFree((void*)obj.Instance);
        }

        return type;
    }

    private unsafe List<DtiField> DumpFields(MtDti dti)
    {
        List<DtiField> fields = [];

        if (_dtiBlackList.Contains(dti.Name))
        {
            _failedDti.Add(dti.Name);
            return fields;
        }
        
        var obj = InstantiateObject(dti, out var isSingleton);

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

    private static unsafe MtObject? InstantiateObject(MtDti dti, out bool isSingleton)
    {
        MtObject? obj;
        isSingleton = false;
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

        return obj;
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
