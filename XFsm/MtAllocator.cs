using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core;

namespace XFsm;
internal class MtAllocator : MtObject
{
    public MtAllocator(nint instance) : base(instance) { }
    public MtAllocator() : base() { }

    public unsafe void Free(nint ptr)
    {
        new NativeAction<nint, nint>(GetVirtualFunction(13)).Invoke(Instance, ptr);
    }
}
