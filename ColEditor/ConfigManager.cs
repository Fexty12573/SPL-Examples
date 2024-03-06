using System.Drawing;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.IO;
using System.Numerics;
using ImGuiNET;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.MtTypes;
using SharpPluginLoader.Core.Rendering;

namespace ColEditor;

public partial class Plugin
{
    private Config _config = null!;
    private string _currentConfig = string.Empty;
    private uint _boneToAdd = 0;
    private BonePreset? _presetToAddTo = null;
    private Settings _settings = null!;

    private const string PluginDirectory = "nativePC/plugins/CSharp/ColEditor/";
    private const string DefaultConfig = "$Default.Config.json";
    private const string SettingsFile = $"{PluginDirectory}Settings.json";
    private static JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new MtColorJsonConverter()
        }
    };

    private void InitializeConfig()
    {
        if (!Directory.Exists(PluginDirectory))
            Directory.CreateDirectory(PluginDirectory);

        var configs = GetAvailableConfigs();
        if (configs.Length > 0)
        {
            Config? config = null;
            foreach (var file in configs)
            {
                config = LoadConfig(file);
                if (config is not null)
                {
                    _config = config;
                    _currentConfig = Path.GetFileNameWithoutExtension(file);
                    break;
                }
            }

            if (config is null)
            {
                _config = new Config();
                SaveConfig(PluginDirectory + DefaultConfig, _config);
            }
        }
        else
        {
            _config = new Config();
            SaveConfig(PluginDirectory + DefaultConfig, _config);
        }

        _settings = new Settings();
        if (File.Exists(SettingsFile))
        {
            using var fs = File.OpenRead(SettingsFile);
            var settings = JsonSerializer.Deserialize<Settings>(fs, JsonOptions);
            if (settings is not null)
                _settings = settings;
        }
        else
        {
            using var fs = File.Create(SettingsFile);
            JsonSerializer.Serialize(fs, _settings, JsonOptions);
        }
    }

    private void DrawPresetManager()
    {
        if (ImGui.Begin("Preset Manager"))
        {
            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.45f);
            if (ImGui.BeginCombo("Configs", _currentConfig))
            {
                foreach (var file in GetAvailableConfigs())
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    if (ImGui.Selectable(name, name == _currentConfig))
                    {
                        var config = LoadConfig(file);
                        if (config is not null)
                        {
                            _config = config;
                            _currentConfig = name;
                        }
                    }
                }

                ImGui.EndCombo();
            }
            ImGui.PopItemWidth();

            ImGui.SameLine();
            if (ImGui.Button("Save"))
            {
                SaveConfig(PluginDirectory + _currentConfig + ".json", _config);
            }

            ImGui.SameLine();
            if (ImGui.Button("Reload"))
            {
                var config = LoadConfig($"{PluginDirectory}{_currentConfig}");
                if (config is not null)
                    _config = config;
            }

            ImGui.SameLine();
            if (ImGui.Button("Add Preset"))
            {
                _config.BonePresets.Add(new BonePreset { Name = $"BonePreset {_config.BonePresets.Count}"});
            }

            ImGui.Separator();

            var i = 0;
            foreach (var preset in _config.BonePresets)
            {
                ImGui.PushID(i);

                var color = preset.Color.ToVector4();
                ImGui.ColorEdit4("##color", ref color,
                    ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel);
                preset.Color = MtColor.FromVector4(color);
                ImGui.SameLine();

                var open = ImGui.CollapsingHeader("##header");
                ImGui.SameLine();
                ImGui.Text(preset.Name);

                if (open)
                {
                    if (ImGui.Button("+##Add bone", new Vector2(30, 30)))
                    {
                        _presetToAddTo = preset;
                        ImGui.OpenPopup("AddNewBonePopup");
                    }

                    ImGui.SameLine();
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, new MtColor(23, 28, 30, 255));
                    ImGui.InputText("Name", ref preset.RefName, 256);
                    ImGui.PopStyleColor();

                    ImGui.Separator();
                    ImGui.Indent();

                    foreach (var bone in preset.Bones)
                    {
                        if (ImGui.Selectable($"Bone {bone}", false, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            preset.Bones.Remove(bone);
                        }
                    }

                    ImGui.Unindent();
                    ImGui.Separator();
                }

                if (ImGui.BeginPopup("AddNewBonePopup"))
                {
                    Ensure.NotNull(_presetToAddTo);

                    ImGui.Text("Add New Bone");
                    ImGuiExtensions.InputScalar("Bone ID", ref _boneToAdd);
                    ImGui.SameLine();
                    if (ImGui.Button("Add"))
                    {
                        _presetToAddTo.Bones.Add(_boneToAdd);
                        ImGui.CloseCurrentPopup();
                    }

                    ImGui.EndPopup();
                }

                ImGui.PopID();
                i += 1;
            }
        }

        ImGui.End();
    }

    private static string[] GetAvailableConfigs()
    {
        return Directory.EnumerateFiles(PluginDirectory, "*.Config.json").ToArray();
    }

    private static Config? LoadConfig(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        using var fs = File.OpenRead(filePath);
        try
        {
            return JsonSerializer.Deserialize<Config>(fs, JsonOptions);
        }
        catch (Exception e)
        {
            ImGuiExtensions.NotificationError("Failed to load Config file. Check the log for more info.", 4000);
            Log.Error(e.ToString());
            return null;
        }
    }

    private static void SaveConfig(string filePath, Config config)
    {
        using var fs = File.Create(filePath);
        JsonSerializer.Serialize(fs, config, JsonOptions);
    }

    private void SaveSettings()
    {
        using var fs = File.Create(SettingsFile);
        JsonSerializer.Serialize(fs, _settings, JsonOptions);
    }
}
