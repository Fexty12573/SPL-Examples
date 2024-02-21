using SharpPluginLoader.Core;
using SharpPluginLoader.InternalCallGenerator;

namespace ScheduLayer;

[InternalCallManager]
public partial class GameFunctions
{
    [InternalCall(Pattern = "48 3b ca 74 1e 48 85 c9 74 05", Offset = -23)]
    public static partial void SchedulerSetData(nint scheduler, nint data);

    [InternalCall(Pattern = "48 83 b9 38 01 00 00 00 74 19 8b c2 0f 57 c0")]
    public static partial void SchedulerSetFrame(nint scheduler, int frame);

    [InternalCall(Pattern = "38 91 88 01 00 00 74 1e f3 0f 10 81 78 01 00 00")]
    public static partial void SchedulerSetPause(nint scheduler, bool pause);
}
