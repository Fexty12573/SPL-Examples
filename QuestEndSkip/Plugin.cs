using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;

namespace QuestEndSkip;

public class Plugin : IPlugin
{
    public string Name => "QuestEndSkip";
    public string Author => "Fexty";

    public PluginData OnLoad()
    {
        KeyBindings.AddKeybind("SkipQuestEnd", new Keybind<Key>(Key.S, [Key.LeftControl, Key.LeftAlt]));

        return new PluginData
        {
            OnUpdate = true
        };
    }

    public void OnUpdate(float dt)
    {
        if (Quest.QuestEndTimer.Time > 0f && KeyBindings.IsPressed("SkipQuestEnd"))
            Quest.QuestEndTimer.SetToEnd();
    }
}
