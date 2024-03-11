using System.Numerics;
using System.Drawing;
using ImGuiNET;
using SharpPluginLoader.Core.MtTypes;
using XFsm.ImGuiNodeEditor;
using static XFsm.ImGuiNodeEditor.InternalCalls;

namespace XFsm;

internal class NodeBuilder(nint texture = 0, uint textureWidth = 0, uint textureHeight = 0)
{
    public void Begin(nint nodeId)
    {
        _hasHeader = false;
        _headerMin = _headerMax = new Vector2();

        PushStyleVarVec4(StyleVar.NodePadding, new Vector4(8f, 4f, 8f, 8f));

        BeginNode(nodeId);

        ImGui.PushID(nodeId);
        _nodeId = nodeId;

        SetStage(Stage.Begin);
    }

    public unsafe void End()
    {
        SetStage(Stage.End);

        EndNode();

        if (ImGui.IsItemVisible())
        {
            var style = ImGui.GetStyle();
            var edStyle = GetStyle();

            var alpha = (byte)(255f * style.Alpha);
            var drawList = new ImDrawListPtr(GetNodeBackgroundDrawList(_nodeId));
            
            var halfBorderWidth = 0.5f * edStyle.NodeBorderWidth;
            var headerColor = new MtColor(_headerColor.R, _headerColor.G, _headerColor.B, alpha);
            if (_headerMax.X > _headerMin.X && _headerMax.Y > _headerMin.Y && texture != 0)
            {
                var uv = new Vector2(
                    (_headerMax.X - _headerMin.X) / (4f * textureWidth),
                    (_headerMax.Y - _headerMin.Y) / (4f * textureHeight)
                );

                drawList.AddImageRounded(
                    texture,
                    _headerMin - new Vector2(8f - halfBorderWidth, 4f - halfBorderWidth),
                    _headerMax + new Vector2(8f - halfBorderWidth, 0),
                    new Vector2(),
                    uv,
                    headerColor,
                    edStyle.NodeRounding,
                    ImDrawFlags.RoundCornersTop
                );

                if (_contentMin.Y > _headerMax.Y)
                {
                    drawList.AddLine(
                        new Vector2(_headerMin.X - (8f - halfBorderWidth), _headerMax.Y - 0.5f),
                        new Vector2(_headerMax.X + (8f - halfBorderWidth), _headerMax.Y - 0.5f),
                        new MtColor(255, 255, 255, (byte)(96 * alpha / (3 * 255))), 
                        1.0f
                    );
                }
            }
        }

        _nodeId = 0;

        ImGui.PopID();
        PopStyleVar();
        SetStage(Stage.Invalid);
    }

    public void Header(MtColor color)
    {
        _headerColor = color;
        SetStage(Stage.Header);
    }

    public void Header() => Header(Color.White);

    public void EndHeader()
    {
        SetStage(Stage.Content);
    }

    public void Input(nint pinId)
    {
        if (_currentStage == Stage.Begin)
            SetStage(Stage.Content);

        var applyPadding = _currentStage == Stage.Input;

        SetStage(Stage.Input);

        if (applyPadding)
            ImGui.Spring(0);

        Pin(pinId, PinKind.Input);

        ImGui.BeginHorizontal(pinId);
    }

    public void EndInput()
    {
        ImGui.EndHorizontal();

        EndPin();
    }

    public void Middle()
    {
        if (_currentStage == Stage.Begin)
            SetStage(Stage.Content);

        SetStage(Stage.Middle);
    }

    public void Output(nint pinId)
    {
        if (_currentStage == Stage.Begin)
            SetStage(Stage.Content);

        var applyPadding = _currentStage == Stage.Output;

        SetStage(Stage.Output);

        if (applyPadding)
            ImGui.Spring(0);

        Pin(pinId, PinKind.Output);

        ImGui.BeginHorizontal(pinId);
    }

    public void EndOutput()
    {
        ImGui.EndHorizontal();

        EndPin();
    }

    private bool SetStage(Stage stage)
    {
        if (stage == _currentStage)
            return false;

        var oldStage = _currentStage;
        _currentStage = stage;

        switch (oldStage)
        {
            case Stage.Invalid:
            case Stage.Begin:
                break;

            case Stage.Header:
                ImGui.EndHorizontal();
                _headerMin = ImGui.GetItemRectMin();
                _headerMax = ImGui.GetItemRectMax();

                ImGui.Spring(0, ImGui.GetStyle().ItemSpacing.Y * 2f);
                break;

            case Stage.Content:
                break;

            case Stage.Input:
                PopStyleVar(2);

                ImGui.Spring(1, 0);
                ImGui.EndVertical();
                break;

            case Stage.Output:
                PopStyleVar(2);

                ImGui.Spring(1, 0);
                ImGui.EndVertical();
                break;

            case Stage.Middle:
                ImGui.EndVertical();
                break;

            case Stage.End:
                break;

            default:
                throw new Exception("Invalid stage");
        }

        switch (stage)
        {
            case Stage.Invalid:
                break;

            case Stage.Begin:
                ImGui.BeginVertical("node");
                break;

            case Stage.Header:
                _hasHeader = true;
                ImGui.BeginHorizontal("header");
                break;

            case Stage.Content:
                if (oldStage == Stage.Begin)
                    ImGui.Spring(0);

                ImGui.BeginHorizontal("content");
                ImGui.Spring(0, 0);
                break;

            case Stage.Input:
                ImGui.BeginVertical("inputs", new Vector2(0, 0), 0f);

                PushStyleVarVec2(StyleVar.PivotAlignment, new Vector2(0, 0.5f));
                PushStyleVarVec2(StyleVar.PivotSize, new Vector2(0, 0));

                if (!_hasHeader)
                    ImGui.Spring(1, 0);
                break;

            case Stage.Output:
                if (oldStage is Stage.Middle or Stage.Input)
                    ImGui.Spring(1);
                else
                    ImGui.Spring(1, 0);

                ImGui.BeginVertical("outputs", new Vector2(0, 0), 1f);

                PushStyleVarVec2(StyleVar.PivotAlignment, new Vector2(1, 0.5f));
                PushStyleVarVec2(StyleVar.PivotSize, new Vector2(0, 0));

                if (!_hasHeader)
                    ImGui.Spring(1, 0);
                break;

            case Stage.Middle:
                ImGui.Spring(1);
                ImGui.BeginVertical("middle", new Vector2(0, 0), 1f);
                break;

            case Stage.End:
                if (oldStage == Stage.Input)
                    ImGui.Spring(1, 0);
                if (oldStage != Stage.Begin)
                    ImGui.EndHorizontal();

                _contentMin = ImGui.GetItemRectMin();
                _contentMax = ImGui.GetItemRectMax();

                ImGui.EndVertical();
                _nodeMin = ImGui.GetItemRectMin();
                _nodeMax = ImGui.GetItemRectMax();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(stage), stage, null);
        }

        return true;
    }

    private void Pin(nint pinId, PinKind kind)
    {
        BeginPin(pinId, kind);
    }

    private void EndPin()
    {
        InternalCalls.EndPin();
    }

    private enum Stage
    {
        Invalid,
        Begin,
        Header,
        Content,
        Input,
        Output,
        Middle,
        End,
    }

    private nint _nodeId;
    private Stage _currentStage = Stage.Invalid;
    private MtColor _headerColor;
    private Vector2 _nodeMin;
    private Vector2 _nodeMax;
    private Vector2 _headerMin;
    private Vector2 _headerMax;
    private Vector2 _contentMin;
    private Vector2 _contentMax;
    private bool _hasHeader;
}
