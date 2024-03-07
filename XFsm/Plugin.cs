﻿using System.IO;
using System.Numerics;
using System.Text;
using SharpPluginLoader.Core;
using ImGuiNET;
using Microsoft.Win32;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Rendering;

namespace XFsm;

public partial class Plugin : IPlugin
{
    public string Name => "XFsm";
    public string Author => "Fexty";

    private XFsmEditor _editor = null!;
    private MtDti _fsmDti = null!;
    private string _fsmPath = "";
    private string _lastOpenedPath = "";

    public void OnLoad()
    {
        _editor = new XFsmEditor();
        _fsmDti = MtDti.Find("rAIFSM")!;
        Ensure.NotNull(_fsmDti);
    }

    public void OnImGuiFreeRender()
    {
        if (!Renderer.MenuShown)
            return;
        
        if (!ImGui.Begin("XFsm", ImGuiWindowFlags.DockNodeHost))
            goto Exit;

        if (ImGui.Button("Open.."))
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".fsm",
                Filter = "FSM Files|*.fsm;*.xml",
            };

            try
            {
                if (dlg.ShowDialog() == true)
                {
                    var file = TryLoadFsm(dlg.FileName, out var isNativePc);
                    if (file is not null)
                    {
                        _editor.SetFsm(file);
                        _lastOpenedPath = isNativePc 
                            ? $@".\nativePC\{GetGameCompatiblePath(dlg.FileName)}{Path.GetExtension(dlg.FileName)}" 
                            : dlg.FileName;
                    }
                    else
                    {
                        Log.Error("Failed to load FSM file.");
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        ImGui.SameLine();

        ImGui.InputText("##nativePC path", ref _fsmPath, 260);

        ImGui.SameLine();

        if (ImGui.Button("Load from NativePC"))
        {
            var fsm = ResourceManager.GetResource<AIFSM>(_fsmPath, _fsmDti);
            if (fsm is not null)
            {
                _editor.SetFsm(fsm);
                _lastOpenedPath = $@".\nativePC\{_fsmPath}.fsm";
            }
            else
            {
                Log.Error("Failed to load FSM file.");
            }
        }

        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("""
                       Loads the specified FSM file into the context of the game.
                       If the file is already loaded, that loaded instance will be used.
                       """);

            ImGui.EndTooltip();
        }

        if (_editor.HasFsm)
        {
            ImGui.SameLine();

            if (ImGui.Button("Save"))
            {
                _editor.ApplyEditorToObject();

                using var fs = MtFileStream.FromPath(_lastOpenedPath, OpenMode.Write);
                Ensure.NotNull(fs);

                var serializer = new MtSerializer();

                if (Path.GetExtension(_lastOpenedPath) == ".fsm")
                {
                    serializer.SerializeBinary(fs, _editor.Fsm!, 0xE05);
                }
                else if (Path.GetExtension(_lastOpenedPath) == ".xml")
                {
                    serializer.SerializeXml(fs, _editor.Fsm!, _editor.Fsm!.OwnerObjectName);
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Save As..."))
            {
                _editor.ApplyEditorToObject();

                var dlg = new SaveFileDialog
                {
                    DefaultExt = ".fsm",
                    Filter = "FSM Files|*.fsm;*.xml",
                };

                if (dlg.ShowDialog() == true)
                {
                    using var fs = MtFileStream.FromPath(dlg.FileName, OpenMode.Write);
                    Ensure.NotNull(fs);

                    var serializer = new MtSerializer();

                    if (Path.GetExtension(dlg.FileName) == ".fsm")
                    {
                        serializer.SerializeBinary(fs, _editor.Fsm!, 0xE05);
                    }
                    else if (Path.GetExtension(dlg.FileName) == ".xml")
                    {
                        serializer.SerializeXml(fs, _editor.Fsm!, _editor.Fsm!.OwnerObjectName);
                    }
                }
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Fix..."))
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".fsm",
                Filter = "XML Files|*.fsm",
            };

            if (dlg.ShowDialog() == true)
            {
                var file = TryLoadFsm(dlg.FileName, out _);
                if (file is not null)
                {
                    var saveDlg = new SaveFileDialog
                    {
                        DefaultExt = ".fsm",
                        Filter = "FSM Files|*.fsm"
                    };

                    if (saveDlg.ShowDialog() == true)
                    {
                        using var fs = MtFileStream.FromPath(saveDlg.FileName, OpenMode.Write);
                        Ensure.NotNull(fs);
                        var serializer = new MtSerializer();
                        //serializer.SerializeBinary(fs, file, 0xE05);

                        var serializeBinary = new NativeFunction<nint, nint, ushort, nint, SerializerMode, nint, bool>(0x14219c0c0);
                        unsafe
                        {
                            var serializerPtr = &serializer;
                            serializeBinary.Invoke(
                                (nint)serializerPtr,
                                fs.Instance,
                                0xE05,
                                file.Instance,
                                SerializerMode.State,
                                0
                            );
                        }
                    }
                }
            }
        }

        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("""
                       Lets you "fix" an FSM file that can't be converted by the JSON converter.
                       """);

            ImGui.EndTooltip();
        }

        _editor.Render();

        Exit:
        ImGui.End();
    }

    private AIFSM? TryLoadFsm(string path, out bool isNativePc)
    {
        var existing = ResourceManager.GetResource<AIFSM>(
            GetGameCompatiblePath(path),
            _fsmDti,
            0x10
        );

        if (existing is not null)
        {
            isNativePc = true;
            return existing;
        }

        isNativePc = false;

        using var fs = MtFileStream.FromPath(path, OpenMode.Read);
        Ensure.NotNull(fs);

        var file = _fsmDti.CreateInstance<AIFSM>();
        if (file.Instance != 0)
        {
            if (Path.GetExtension(path) == ".fsm")
            {
                if (!file.LoadFrom(fs))
                {
                    Log.Error("Failed to load FSM file.");
                    return null;
                }
            }
            else if (Path.GetExtension(path) == ".xml")
            {
                var deserializer = new MtSerializer();
                file = deserializer.DeserializeXml(fs, "whatever", file);
                if (file is null)
                {
                    Log.Error("Failed to load FSM file.");
                    return null;
                }
            }
        }

        return file;
    }

    private static string GetPathWithoutExtension(string path)
    {
        return Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path));
    }
    private static string GetGameCompatiblePath(string path)
    {
        var pathNoExt = GetPathWithoutExtension(path);
        return Path.GetRelativePath("./nativePC", pathNoExt);
    }
}
