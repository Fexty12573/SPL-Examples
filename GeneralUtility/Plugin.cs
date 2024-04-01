using System.Collections.Concurrent;
using System.Numerics;
using System.Text.Json;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Components;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Models;
using SharpPluginLoader.Core.MtTypes;

namespace GeneralUtility;

public class Plugin : IPlugin
{
    public string Name => "General Utility";
    public string Author => "Fexty";

    public const string ConfigPath = "./nativePC/plugins/CSharp/GeneralUtility/Config.json";

    public List<Monster> Monsters { get; } = [];
    public int OverrideActionId = 0;
    public bool LockOverrideAction = false;

    private Vector3 _lastPos = Vector3.Zero;
    private int _elapsedFrames = 0;
    private float _elapsedTime = 0;
    private Queue<float> _lastSpeeds = [];

    public Config Config { get; private set; } = null!;
    public Dictionary<MonsterType, ActionChain> ActionChains => [];
    public ConcurrentDictionary<Monster, Vector3> LockedCoordinates { get; } = [];
    public ConcurrentDictionary<Monster, Vector3> LockedTargetCoordinates { get; } = [];

    private static MainWindow _mainWindow = null!;
    private Thread? _winformsThread;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private Hook<SetTargetPositionDelegate> _setTargetPositionHook = null!;
    private Hook<InputUpdateDelegate> _inputUpdateHook = null!;

    public void OnLoad()
    {
        LoadConfig();

        _setTargetPositionHook = Hook.Create<SetTargetPositionDelegate>(0x141393ba0, SetTargetPositionHook);
        _inputUpdateHook = Hook.Create<InputUpdateDelegate>(0x141b15af0, InputUpdateHook);
        _mainWindow = new MainWindow(this);

        Log.Info("Starting WinForms Thread");
        _winformsThread = new Thread(WinformsRun);
        _winformsThread.SetApartmentState(ApartmentState.STA);
        _winformsThread.Start();
    }

    public void OnUpdate(float deltaTime)
    {
        foreach (var (monster, coords) in LockedCoordinates)
        {
            monster.Position = coords;
        }

        var selectedMonster = _mainWindow.SelectedMonster;
        if (selectedMonster is null)
            return;

        if (_elapsedFrames > 10)
        {
            var deltaPos = selectedMonster.Position - _lastPos;
            var speed = deltaPos.Length() / _elapsedTime;
            _lastSpeeds.Enqueue(speed);
            if (_lastSpeeds.Count > 10)
                _lastSpeeds.Dequeue();

            _mainWindow.UpdateMonsterSpeed(_lastSpeeds.Average());

            _lastPos = selectedMonster.Position;
            _elapsedFrames = 0;
            _elapsedTime = 0;
        }
        else
        {
            _elapsedTime += deltaTime;
            _elapsedFrames++;
        }

        if (!_mainWindow.PlayerPosLocked)
            return;

        var player = Player.MainPlayer;
        if (player is null)
            return;

        player.Teleport(selectedMonster.Position + _mainWindow.PlayerPosOffset);
    }

    public void OnMonsterCreate(Monster monster)
    {
        Monsters.Add(monster);
        _mainWindow.UpdateLoadedMonsters();
    }

    public void OnMonsterDestroy(Monster monster)
    {
        Monsters.Remove(monster);
        LockedCoordinates.TryRemove(monster, out _);
        LockedTargetCoordinates.TryRemove(monster, out _);
        _mainWindow.UpdateLoadedMonsters();
    }

    public void OnMonsterAction(Monster monster, ref int actionId)
    {
        if (!_mainWindow.IsSelectedMonster(monster))
            return;

        var actionChain = GetActionChain(monster.Type);
        if (actionChain is not null && actionChain.GetNextAction(out var newActionId))
        {
            Log.Info($"Setting action for {monster} to {newActionId} (Action Chain)");
            actionId = newActionId;
        }
        else if (OverrideActionId != 0)
        {
            Log.Info($"Setting action for {monster} to {OverrideActionId} (Override)");
            actionId = OverrideActionId;

            if (!LockOverrideAction)
                OverrideActionId = 0;
        }

        _mainWindow.SetActionAnimation(new ActionInfo(1, actionId), monster.CurrentAnimation);
    }

    public void OnEntityAnimation(Entity entity, ref AnimationId animationId, ref float startFrame, ref float interFrame)
    {
        if (!entity.Is("uEnemy"))
            return;

        var monster = entity.As<Monster>();
        if (!_mainWindow.IsSelectedMonster(monster))
            return;

        _mainWindow.SetActionAnimation(monster.GetCurrentAction(), animationId);
    }

    public void LockCoordinates(Monster monster)
    {
        LockedCoordinates[monster] = monster.Position;
    }

    public void LockCoordinates(Monster monster, Vector3 coords)
    {
        LockedCoordinates[monster] = coords;
    }

    public void UnlockCoordinates(Monster monster)
    {
        LockedCoordinates.TryRemove(monster, out _);
    }

    public void LoadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            var tempConfig = new ConfigJson();
            using var fileStream = File.CreateText(ConfigPath);
            JsonSerializer.Serialize(fileStream.BaseStream, tempConfig, _jsonSerializerOptions);
        }

        var configJson = JsonSerializer.Deserialize<ConfigJson>(File.ReadAllText(ConfigPath), _jsonSerializerOptions);
        if (configJson is null)
            return;

        Config = new Config();
        foreach (var chain in configJson.ActionChains)
        {
            Config.ActionChains[(MonsterType)chain.MonsterId] = chain.ToActionChain();
        }
    }

    public ActionChain? GetActionChain(MonsterType monsterType)
    {
        return ActionChains.GetValueOrDefault(monsterType);
    }

    public void SetTargetPositionHook(nint actionParams, int index, ref Vector3 pos)
    {
        if (_mainWindow.SelectedMonster?.Instance != actionParams - 0x1D700)
        {
            _setTargetPositionHook.Original(actionParams, index, ref pos);
            return;
        }

        var monster = new Monster(actionParams - 0x1D700);
        if (LockedTargetCoordinates.TryGetValue(monster, out var coords))
        {
            Log.Info($"Setting target position for {monster} to {coords}");
            pos = coords;
        }

        _setTargetPositionHook.Original(actionParams, index, ref pos);
    }

    public void InputUpdateHook(nint pad)
    {
        _inputUpdateHook?.Original(pad);
        ref var axis = ref MemoryUtil.GetRef<Point>(pad + 0x1B8);

        if (_mainWindow.StickXLocked)
            axis.X = _mainWindow.StickX;
        if (_mainWindow.StickYLocked)
            axis.Y = _mainWindow.StickY;
    }

    [STAThread]
    public static void WinformsRun()
    {
        Ensure.NotNull(_mainWindow);
        
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.Run(_mainWindow);
    }

    public delegate void SetTargetPositionDelegate(nint actionParams, int index, ref Vector3 pos);
    public delegate void InputUpdateDelegate(nint pad);
}
