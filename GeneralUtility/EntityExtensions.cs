using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core.Memory;

namespace GeneralUtility;
public static class EntityExtensions
{
    public static ref ActionInfo GetCurrentAction(this Entity entity)
    {
        return ref MemoryUtil.GetRef<ActionInfo>(entity.Instance + 0x61C8 + 0xAC);
    }
}
