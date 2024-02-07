using System;
using ImGuiNET;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Resources;

namespace FsmSerializer;

public class Plugin : IPlugin
{
    public string Name => "FsmSerializer";
    public string Author => "Fexty";

    private string _filePath = "";
    private string _className = "rAIFSM";
    private string _outputPath = "";

    public PluginData OnLoad()
    {
        return new PluginData
        {
            OnImGuiRender = true
        };
    }

    public void OnImGuiRender()
    {
        ImGui.InputText("Path", ref _filePath, 260);
        ImGui.InputText("Class Name", ref _className, 80);
        ImGui.InputText("Output Path", ref _outputPath, 260);

        if (ImGui.Button("Convert to XML"))
        {
            var outPath = _outputPath == "" ? _filePath + ".xml" : _outputPath;
            var dti = MtDti.Find(_className);

            if (dti is null)
            {
                Log.Error("Class not found");
                return;
            }

            // Load the resource from file normally
            var resource = ResourceManager.GetResource<Resource>(_filePath, dti);
            if (resource is null)
            {
                Log.Error("Resource not found");
                return;
            }

            // Create a file stream to the output file
            using var fs = MtFileStream.FromPath(outPath, OpenMode.Write);
            if (fs is null)
            {
                Log.Error("Failed to open file");
                return;
            }

            // Serialize the resource to XML
            var serializer = new MtSerializer();
            serializer.SerializeXml(fs, resource, resource.FilePath);

            Log.Info($"File written to {outPath}");
        }
    }
}