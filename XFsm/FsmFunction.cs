using SharpPluginLoader.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XFsm;

[StructLayout(LayoutKind.Sequential)]
internal readonly unsafe struct FsmFunction
{
    private readonly sbyte* _namePtr;
    private readonly nint _parentDti;
    private readonly nint _paramDti;
    public readonly nint OnExecute;
    public readonly nint OnUpdate;
    public readonly nint OnEnd;

    public string Name => new(_namePtr);
    public MtDti ParentDti => new(_parentDti);
    public MtDti ParamDti => new(_paramDti);

    public bool IsEmpty => _namePtr == null;
}

[StructLayout(LayoutKind.Sequential)]
internal readonly unsafe struct FsmFunctionList
{
    private readonly NativeArray<FsmFunction> _functions;

    public ref FsmFunction this[int index] => ref _functions[index];
    public int Count => _functions.Length;

    public IEnumerator<FsmFunction> GetEnumerator()
    {
        foreach (var function in _functions)
        {
            yield return function;
        }
    }

    public FsmFunctionList(FsmFunction* functions)
    {
        for (int i = 0; i < int.MaxValue; i++)
        {
            if (functions[i].IsEmpty)
            {
                _functions = new NativeArray<FsmFunction>((nint)functions, i);
            }
        }
    }

    public FsmFunctionList(nint functions)
    {
        var pfunctions = (FsmFunction*)functions;
        
        for (int i = 0; i < int.MaxValue; i++)
        {
            if (pfunctions[i].IsEmpty)
            {
                _functions = new NativeArray<FsmFunction>(functions, i);
            }
        }
    }
}
