using System.Numerics;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Rendering;
using ImGuiNET;
using SharpPluginLoader.Core.MtTypes;
using System.IO;
using System.Text;
using SharpPluginLoader.Core.Memory;

namespace AssetBrowser;

public unsafe class Plugin : IPlugin
{
    public string Name => "AssetBrowser";
    public string Author => "Fexty";

    private DirectoryInfo? _baseDirectory;
    private DirectoryInfo? _currentDirectory;

    private bool _firstRender = true;
    private bool _isAnyItemHovered = false;
    private string _searchString = "";

    private nint _currentDragSource = 0;
    private uint _currentDragSourceLength = 0;
    private string _currentDragSourceString = "";

    private const float CellSize = 128f;
    private const float CellPadding = 2f;
    private const float ThumbnailSize = 128f;


    private const string PluginBaseDir = "nativePC/plugins/CSharp/AssetBrowser/";
    private const string AssetDir = "Assets";

    private const string RootDirectory = "nativePC";
    private static readonly string RootDirectoryAbsolute = Path.GetFullPath(RootDirectory);

    public void OnLoad()
    {
        _baseDirectory = new DirectoryInfo("nativePC");
        _currentDirectory = _baseDirectory;
    }

    public void OnImGuiFreeRender()
    {
        if (_firstRender)
        {
            IconRepository.Initialize(PluginBaseDir + AssetDir);
            _firstRender = false;
        }

        if (!Renderer.MenuShown)
            return;

        if (!ImGui.Begin("Asset Browser", ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoScrollbar))
        {
            ImGui.End();
            return;
        }

        var styleStack = new ScopedStyleStack(
            (ImGuiStyleVar.ItemSpacing, new Vector2(8f, 8f)),
            (ImGuiStyleVar.FramePadding, new Vector2(4f, 4f)),
            (ImGuiStyleVar.CellPadding, new Vector2(10f, 2f))
        );

        var tableFlags = ImGuiTableFlags.Resizable
                         | ImGuiTableFlags.SizingFixedFit
                         | ImGuiTableFlags.BordersInnerV;

        if (ImGui.BeginTable("##AssetBrowserTable", 2, tableFlags, new Vector2()))
        {
            ImGui.TableSetupColumn("Outline", ImGuiTableColumnFlags.None, 300f);
            ImGui.TableSetupColumn("Directory Structure", ImGuiTableColumnFlags.WidthStretch);

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.BeginChild("##folders_common");
            {
                using var spacing = new ScopedStyle(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
                using var itemBg = new ScopedColorStack(
                    (ImGuiCol.Header, new Vector4(0, 0, 0, 1)),
                    (ImGuiCol.HeaderActive, new Vector4(0, 0, 0, 1))
                );

                if (_baseDirectory is not null)
                {
                    foreach (var dir in _baseDirectory.EnumerateDirectories())
                    {
                        RenderDirectoryHierarchy(dir);
                    }
                }
            }
            ImGui.EndChild();

            ImGui.TableSetColumnIndex(1);

            var avail = ImGui.GetContentRegionAvail();

            const float topBarHeight = 26f;
            const float bottomBarHeight = 32f;
            var size = new Vector2(
                avail.X,
                ImGui.GetWindowHeight() - topBarHeight - bottomBarHeight
            );

            ImGui.BeginChild("##directory_structure", size);
            {
                using (var border = new ScopedStyle(ImGuiStyleVar.FrameBorderSize, 0f))
                    RenderTopBar(topBarHeight);

                ImGui.Separator();

                ImGui.BeginChild("Scrolling");
                {
                    {
                        using var colorStack = new ScopedColorStack(
                            (ImGuiCol.Button, new Vector4()),
                            (ImGuiCol.ButtonHovered, new Vector4(0.3f, 0.3f, 0.3f, 0.35f))
                        );

                        const float paddingForOutline = 2f;
                        var scrollbarOffset = 20f + ImGui.GetStyle().ScrollbarSize;
                        var panelWidth = avail.X - scrollbarOffset;
                        var cellSize = CellSize + paddingForOutline + CellPadding;
                        int columns = (int)(panelWidth / cellSize);

                        if (columns < 1)
                            columns = 1;

                        {
                            const float rowSpacing = 12f;
                            using var spacing = new ScopedStyle(
                                ImGuiStyleVar.ItemSpacing,
                                new Vector2(paddingForOutline, rowSpacing)
                            );
                            ImGui.Columns(columns, "##AssetBrowserColumns", false);

                            using var border = new ScopedStyle(ImGuiStyleVar.FrameBorderSize, 0f);
                            using var padding = new ScopedStyle(ImGuiStyleVar.CellPadding, new Vector2(CellPadding, CellPadding));
                            RenderItems();

                            ImGui.EndColumns();
                        }

                        if (ImGui.IsWindowFocused() && !ImGui.IsMouseDragging(ImGuiMouseButton.Left))
                            UpdateInput();
                    }

                    RenderDeleteDialogue();
                }
                ImGui.EndChild();
            }
            ImGui.EndChild();

            RenderBottomBar(bottomBarHeight);

            ImGui.EndTable();
        }

        styleStack.Dispose();

        ImGui.End();
    }

    private void RenderTopBar(float height)
    {
        ImGui.BeginChild("##top_bar", new Vector2(0, height));
        ImGui.BeginHorizontal("##top_bar", ImGui.GetWindowSize());
        {
            const float edgeOffset = 4f;

            {
                using var spacing = new ScopedStyle(ImGuiStyleVar.ItemSpacing, new Vector2(2, 0));

                var isDisabled = _currentDirectory?.Name == RootDirectory;
                if (isDisabled)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0.5f, 0.5f, 0.8f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.5f, 0.5f, 0.8f));
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.5f, 0.5f, 0.5f, 0.8f));
                }

                if (AssetBrowserButton("##up", IconRepository.UpArrow) && !isDisabled)
                {
                    ChangeDirectory(_currentDirectory?.Parent);
                }

                if (isDisabled)
                {
                    ImGui.PopStyleVar();
                    ImGui.PopStyleColor(3);
                }
                else
                {
                    ShowTooltip("Go up one level");
                }

                ImGui.Spring(-1, edgeOffset * 2);
            }

            {
                ImGui.SetNextItemWidth(200f);

                ImGui.InputTextWithHint("##search", "Search...", ref _searchString, 260);
                // TODO: Implement search functionality
            }

            {
                List<DirectoryInfo> breadCrumbs = [];
                var current = _currentDirectory;
                while (current is not null)
                {
                    breadCrumbs.Add(current);
                    if (current.Name == RootDirectory)
                        break;

                    current = current.Parent;
                }

                breadCrumbs.Reverse();
                var textPadding = ImGui.GetStyle().FramePadding.Y;
                using var rounding = new ScopedStyle(ImGuiStyleVar.FrameRounding, 3f);

                foreach (var dir in breadCrumbs)
                {
                    var textSize = ImGui.CalcTextSize(dir.Name);
                    if (ImGui.Selectable(dir.Name, false, ImGuiSelectableFlags.None,
                            textSize with { Y = textSize.Y + textPadding }))
                    {
                        ChangeDirectory(dir);
                    }

                    ImGui.Text("/");
                }
            }
        }

        ImGui.EndHorizontal();
        ImGui.EndChild();

        return;

        bool AssetBrowserButton(string id, TextureHandle icon)
        {
            const float iconPadding = 3f;
            var iconSize = MathF.Min(24f, height) - iconPadding;
            return ImGui.ImageButton(
                id,
                icon,
                new Vector2(iconSize, iconSize)
            );
        }
    }

    private void RenderDirectoryHierarchy(DirectoryInfo directory)
    {
        var open = ImGui.TreeNodeEx(directory.Name, ImGuiTreeNodeFlags.SpanFullWidth);

        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            ChangeDirectory(directory);
        }

        if (open)
        {
            foreach (var dir in directory.EnumerateDirectories())
            {
                RenderDirectoryHierarchy(dir);
            }

            ImGui.TreePop();
        }
    }

    private void RenderItems()
    {
        _isAnyItemHovered = false;

        if (_currentDirectory is null)
            return;

        var items = _currentDirectory.EnumerateFileSystemInfos();

        var hoveredCol = *ImGui.GetStyleColorVec4(ImGuiCol.ButtonHovered);

        using var colors = new ScopedColorStack(
            (ImGuiCol.Button, new Vector4()),
            (ImGuiCol.ButtonHovered, hoveredCol with { W = 0.2f }),
            (ImGuiCol.ButtonActive, (hoveredCol * 0.8f) with { W = 0.5f })
        );

        foreach (var item in items)
        {
            if (_searchString.Length > 0 && !item.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
                continue;

            if (item is DirectoryInfo dir)
            {
                if (dir.Name.StartsWith('.'))
                    continue;

                RenderDirectory(dir);
            }
            else if (item is FileInfo file)
            {
                RenderFile(file);
            }

            // Allow dragging items
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.SourceAllowNullID))
            {
                if (_currentDragSource == 0)
                {
                    Log.Info("Allocating drag source");

                    var relPath = Path.GetRelativePath(RootDirectoryAbsolute, item.FullName);
                    var pathNoExt = Path.Combine(Path.GetDirectoryName(relPath)!, Path.GetFileNameWithoutExtension(relPath));

                    // The plugin that handles the drop will deallocate the payload
                    var length = Encoding.UTF8.GetByteCount(pathNoExt);
                    var payload = MemoryUtil.Alloc<byte>(length + 1);
                    Encoding.UTF8.GetBytes(pathNoExt, new Span<byte>(payload, length));
                    payload[length] = 0;

                    _currentDragSource = (nint)payload;
                    _currentDragSourceLength = (uint)length;
                    _currentDragSourceString = item.FullName;
                }

                ImGui.SetDragDropPayload("AssetBrowser_Item", _currentDragSource, _currentDragSourceLength);

                ImGui.Text(item.FullName);
                ImGui.EndDragDropSource();
            }
            else
            {
                if (_currentDragSourceString == item.FullName && _currentDragSource != 0)
                {
                    Log.Info("Freeing drag source");

                    MemoryUtil.Free((byte*)_currentDragSource);
                    _currentDragSource = 0;
                    _currentDragSourceLength = 0;
                    _currentDragSourceString = "";
                }
            }

            ImGui.NextColumn();
        }
    }

    private void RenderDirectory(DirectoryInfo directory)
    {
        var isHovered = ImGui.IsItemHovered();
        _isAnyItemHovered |= isHovered;

        using var _ = new ScopedId(directory.FullName);

        ImGui.BeginGroup();
        {
            var text = directory.Name;
            var drawList = ImGui.GetWindowDrawList();
            var pos = ImGui.GetCursorScreenPos();
            var textSize = ImGui.CalcTextSize(text);
            var buttonSize = new Vector2(ThumbnailSize, ThumbnailSize + textSize.Y);
            var clipRect = new ImRect
            {
                Min = pos + new Vector2(CellPadding),
                Max = pos + buttonSize - new Vector2(CellPadding)
            };

            if (textSize.X > ThumbnailSize)
                textSize.X = ThumbnailSize;

            ImGui.AlignTextToFramePadding();
            if (ImGui.Button("##dir_button", new Vector2(ThumbnailSize, ThumbnailSize + textSize.Y)))
            {
                ChangeDirectory(directory);
            }

            ShowTooltip(text);

            drawList.AddImage(IconRepository.Folder, pos, pos + new Vector2(ThumbnailSize, ThumbnailSize));

            ImGui.PushClipRect(clipRect.Min, clipRect.Max, true);

            pos += new Vector2(0, ThumbnailSize);
            drawList.AddText(
                pos + new Vector2(ThumbnailSize / 2 - textSize.X / 2, -textSize.Y * 0.5f),
                ImGui.GetColorU32(ImGuiCol.Text),
                text
            );

            ImGui.PopClipRect();
        }
        ImGui.EndGroup();
    }

    private void RenderFile(FileInfo file)
    {
        var isHovered = ImGui.IsItemHovered();
        _isAnyItemHovered |= isHovered;

        using var _ = new ScopedId(file.FullName);

        ImGui.BeginGroup();
        {
            var text = file.Name;
            var drawList = ImGui.GetWindowDrawList();
            var pos = ImGui.GetCursorScreenPos();
            var textSize = ImGui.CalcTextSize(text);
            var buttonSize = new Vector2(ThumbnailSize, ThumbnailSize + textSize.Y);
            var clipRect = new ImRect
            {
                Min = pos + new Vector2(CellPadding),
                Max = pos + buttonSize - new Vector2(CellPadding)
            };

            if (textSize.X > ThumbnailSize)
                textSize.X = ThumbnailSize;

            ImGui.AlignTextToFramePadding();
            if (ImGui.Button("##file_button", buttonSize))
            {
                
            }

            ShowTooltip(text);

            drawList.AddImage(IconRepository.File, pos, pos + new Vector2(ThumbnailSize, ThumbnailSize));

            ImGui.PushClipRect(clipRect.Min, clipRect.Max, true);
            
            pos += new Vector2(0, ThumbnailSize);
            drawList.AddText(
                pos + new Vector2(ThumbnailSize / 2 - textSize.X / 2, -textSize.Y * 0.5f),
                ImGui.GetColorU32(ImGuiCol.Text),
                text
            );

            ImGui.PopClipRect();
        }
        ImGui.EndGroup();
    }

    private void UpdateInput()
    {

    }

    private void RenderDeleteDialogue()
    {

    }

    private void RenderBottomBar(float height)
    {

    }

    private void ChangeDirectory(DirectoryInfo? directory)
    {
        if (directory is null)
            return;

        _currentDirectory = directory;
    }

    static void ShowTooltip(string text)
    {
        if (ImGui.BeginItemTooltip())
        {
            ImGui.Text(text);
            ImGui.EndTooltip();
        }
    }
}
