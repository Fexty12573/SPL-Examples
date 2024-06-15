using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using SharpPluginLoader.Core;
using ImGuiNET;
using Microsoft.Win32;
using SharpPluginLoader.Core.Components;
using SharpPluginLoader.Core.IO;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.Rendering;
using XFsm.ImGuiNodeEditor;
using SharpPluginLoader.Core.Entities;

namespace XFsm;

public partial class Plugin : IPlugin
{
    public string Name => "XFsm";
    public string Author => "Fexty";

    private XFsmEditor _editor = null!;
    private MtDti _fsmDti = null!;
    private string _fsmPath = "";
    private string _lastOpenedPath = "";
    private string _saveToPath = "";
    private AIFSM? _pendingFsm;

    [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16)]
    private static partial nint AddDllDirectory(string dir);

    [LibraryImport("kernel32.dll", EntryPoint = "SetEnvironmentVariableW", StringMarshalling = StringMarshalling.Utf16)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetEnvironmentVariable(string name, string value);

    public Plugin()
    {
        AddDllDirectory(Path.GetFullPath("./nativePC/plugins/CSharp/XFsm"));
        AddDllDirectory(Path.GetFullPath("./nativePC/plugins/CSharp/XFsm/lib"));
        SetEnvironmentVariable("GVBINDIR", Path.GetFullPath("./nativePC/plugins/CSharp/XFsm/lib/"));
    }

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

        ImGui.BeginGroup();

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

            ImGui.SameLine();
        }

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

                        var serializeBinary = new NativeFunction<nint, nint, ushort, nint, SerializerMode, nint, bool>(0x14219c120);
                        unsafe
                        {
                            var serializerPtr = &serializer;
                            InternalCalls.SerializeBinary(
                                (nint)serializerPtr,
                                fs.Instance,
                                0xE05,
                                file.Instance,
                                SerializerMode.State,
                                0
                            );
                            //serializeBinary.Invoke(
                            //    (nint)serializerPtr,
                            //    fs.Instance,
                            //    0xE05,
                            //    file.Instance,
                            //    SerializerMode.State,
                            //    0
                            //);
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

        ImGui.SameLine();

        var player = Player.MainPlayer;
        if (ImGui.Button("Reload Player FSM") && player is not null)
        {
            _editor.ApplyEditorToObject();
            var currentWeaponType = player.CurrentWeaponType;
            var currentWeaponId = player.CurrentWeapon!.Get<int>(0x9FC);
            var newWeaponType = currentWeaponType == WeaponType.GreatSword ? WeaponType.LongSword : WeaponType.GreatSword;
            
            InternalCalls.ChangeWeapon(newWeaponType, 0);
            InternalCalls.ChangeWeapon(currentWeaponType, currentWeaponId);
        }

        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text("This will force-reload the player's FSM. It might result in a small freeze.");
            ImGui.EndTooltip();
        }

        _editor.Render();

        ImGui.EndGroup();

        if (ImGui.BeginDragDropTarget())
        {
            if (ImGui.AcceptDragDropPayload("AssetBrowser_Item") != 0)
            {
                var payload = ImGui.GetDragDropPayload();
                string path;
                unsafe { path = Encoding.UTF8.GetString((byte*)payload.Data, payload.DataSize); }
                var file = ResourceManager.GetResource<AIFSM>(path, _fsmDti);
                if (file is not null)
                {
                    if (_editor.HasFsm)
                    {
                        _pendingFsm = file;
                        _saveToPath = _lastOpenedPath;
                        ImGui.OpenPopup("Unsaved Changes");
                    }
                    else
                    {
                        _editor.SetFsm(file);
                        _lastOpenedPath = $@".\nativePC\{path}.fsm";
                    }
                }
                else
                {
                    Log.Error("Failed to load FSM file.");
                }
            }

            ImGui.EndDragDropTarget();
        }

        if (ImGui.BeginPopupModal("Unsaved Changes"))
        {
            ImGui.TextWrapped("Warning: You have unsaved changes. Do you want to save them?" +
                              " Your changes will be saved to the following file:");
            ImGui.InputText("##path", ref _saveToPath, 260);

            if (ImGui.Button("Yes"))
            {
                _editor.ApplyEditorToObject();

                using var fs = MtFileStream.FromPath(_saveToPath, OpenMode.Write);
                Ensure.NotNull(fs);

                var serializer = new MtSerializer();

                if (Path.GetExtension(_saveToPath) == ".fsm")
                {
                    serializer.SerializeBinary(fs, _editor.Fsm!, 0xE05);
                }
                else if (Path.GetExtension(_saveToPath) == ".xml")
                {
                    serializer.SerializeXml(fs, _editor.Fsm!, _editor.Fsm!.OwnerObjectName);
                }

                _editor.SetFsm(_pendingFsm!);
                _lastOpenedPath = $@".\nativePC\{_pendingFsm!.FilePath}.fsm";
                _pendingFsm = null;

                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();

            if (ImGui.Button("No"))
            {
                _editor.SetFsm(_pendingFsm!);
                _lastOpenedPath = $@".\nativePC\{_pendingFsm!.FilePath}.fsm";
                _pendingFsm = null;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

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
        return Path.GetRelativePath(Path.GetFullPath("./nativePC"), pathNoExt);
    }
}
