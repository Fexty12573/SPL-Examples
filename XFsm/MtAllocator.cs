using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core;

namespace XFsm;
public class MtAllocator : MtObject
{
    public MtAllocator(nint instance) : base(instance) { }
    public MtAllocator() : base() { }

    public unsafe nint Alloc(nint size, uint align = 16)
    {
        return new NativeFunction<nint, nint, uint, nint>(GetVirtualFunction(9)).Invoke(Instance, size, align);
    }

    public unsafe T Alloc<T>(MtDti dti, int count = 1) where T : MtObject, new()
    {
        return new T
        {
            Instance = Alloc((nint)dti.Size * count)
        };
    }

    public unsafe void Free(nint ptr)
    {
        new NativeAction<nint, nint>(GetVirtualFunction(13)).Invoke(Instance, ptr);
    }
}
