using System;
using SharpPluginLoader.Core;
using Microsoft.UI.Xaml;

namespace ClassLibrary1;

public class Plugin : IPlugin
{
    public string Name => "WinUI Plugin";
    public string Author => "Fexty";

    private MainWindow _mainWindow = null!;

    public PluginData Initialize()
    {
        return new PluginData();
        //return new PluginData
        //{
        //    OnImGuiRender = true,
        //    OnMonsterCreate = true,
        //    OnMonsterDestroy = true,
        //    OnMonsterAction = true,
        //    OnEntityAnimation = true,
        //};
    }

    public void OnLoad()
    {
        try
        {
            _mainWindow = new MainWindow();
            _mainWindow.Activate();
        }
        catch (Exception e)
        {
            Log.Error(e.ToString());
            throw;
        }
    }
}
