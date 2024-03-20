using SharpPluginLoader.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core.Memory;

namespace XFsm;

[StructLayout(LayoutKind.Sequential, Size = 0x30)]
internal readonly unsafe struct FsmFunction
{
    private readonly sbyte* _namePtr;
    private readonly nint _parentDti;
    private readonly nint _paramDti;
    public readonly nint OnExecute;
    public readonly nint OnUpdate;
    public readonly nint OnEnd;

    public string Name => _namePtr != null ? new string(_namePtr) : string.Empty;
    public MtDti ParentDti => _parentDti != 0 ? new MtDti(_parentDti) : new MtDti();
    public MtDti? ParamDti => _paramDti != 0 ? new MtDti(_paramDti) : null;

    public bool IsEmpty => _namePtr == null;

    public static NativeArray<FsmFunction> CreateArray(nint functions)
    {
        var start = functions;
        var i = 0;
        while (MemoryUtil.Read<nint>(functions) != 0 && i < 500)
        {
            functions += sizeof(FsmFunction);
            i++;
        }

        return new NativeArray<FsmFunction>(start, i);
    }
}
