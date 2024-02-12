using SharpPluginLoader.Core;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Interfaces;
using TwitchLib.Communication.Models;

namespace TwitchLibTest;

public class Plugin : IPlugin
{
    public string Name => "TwitchLibTest";
    private TwitchClient? _client;

    public PluginData Initialize()
    {
        return new PluginData();
    }

    public void OnLoad()
    {
        Log.Info("TwitchLibTest loaded");
        var credentials = new ConnectionCredentials("fexty12573", "");
        Log.Info("Created Credentials");
        var clientOptions = new ClientOptions
        {
            MessagesAllowedInPeriod = 750,
            ThrottlingPeriod = TimeSpan.FromSeconds(30)
        };
        Log.Info("Created ClientOptions");
        WebSocketClient customClient = new WebSocketClient(clientOptions);
        Log.Info("Created WebSocketClient");
        _client = new TwitchClient(customClient);
        _client.Initialize(credentials, "channel");

        Log.Info("Initialized TwitchClient");
        _client.Connect();
    }

}
