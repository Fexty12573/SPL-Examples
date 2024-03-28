using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Models;

namespace GeneralUtility;
public static class EntityExtensions
{
    public static ref ActionInfo GetCurrentAction(this Entity entity)
    {
        return ref MemoryUtil.GetRef<ActionInfo>(entity.Instance + 0x61C8 + 0xAC);
    }

    public static float GetTransparency(this Model model)
    {
        return model.Get<float>(0x314);
    }

    public static void SetTransparency(this Model model, float value)
    {
        model.Set(0x314, value);
    }
}
