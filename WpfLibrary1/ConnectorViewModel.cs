using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SharpPluginLoader.Core.Memory;

namespace FsmEditor;

internal interface IConnectorViewModel
{
    public bool IsConnected { get; set; }
    public Point Anchor { get; set; }
    public string Name { get; }
    public INodeViewModel Parent { get; }
}

internal class NodeInputConnectorViewModel(string name, NodeViewModel parent) : IConnectorViewModel, INotifyPropertyChanged
{
    public string Name { get; } = name;

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
        }
    }

    public Point Anchor
    {
        get => _anchor;
        set
        {
            _anchor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Anchor)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public NodeViewModel Parent { get; } = parent;
    INodeViewModel IConnectorViewModel.Parent => Parent;

    private bool _isConnected;
    private Point _anchor;
}

internal unsafe class NodeOutputConnectorViewModel(ref AIFSMLink link, NodeViewModel parent) : IConnectorViewModel, INotifyPropertyChanged
{
    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
        }
    }

    public Point Anchor
    {
        get => _anchor;
        set
        {
            _anchor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Anchor)));
        }
    }

    public ref AIFSMLink Link => ref *_link;

    public event PropertyChangedEventHandler? PropertyChanged;

    public NodeViewModel Parent { get; } = parent;
    INodeViewModel IConnectorViewModel.Parent => Parent;

    private string _name = link.Name;
    private Point _anchor;
    private bool _isConnected;
    private readonly AIFSMLink* _link = MemoryUtil.AsPointer(ref link);
}

internal class ConditionNodeInputConnectorViewModel(NodeOutputConnectorViewModel source, ConditionNodeViewModel parent)
    : IConnectorViewModel, INotifyPropertyChanged
{
    public string Name => "In";
    public Brush Color { get; set; } = Brushes.Green;

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
        }
    }

    public Point Anchor
    {
        get => _anchor;
        set
        {
            _anchor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Anchor)));
        }
    }

    public ConditionNodeOutputConnectorViewModel GetCorrespondingOutput() => Parent.GetCorrespondingConnector(this);

    public NodeOutputConnectorViewModel Source { get; set; } = source;
    public ConditionNodeViewModel Parent { get; } = parent;
    INodeViewModel IConnectorViewModel.Parent => Parent;

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isConnected;
    private Point _anchor;
}

internal class ConditionNodeOutputConnectorViewModel(ConditionNodeViewModel parent) : IConnectorViewModel, INotifyPropertyChanged
{
    public string Name => "Out";
    public Brush Color { get; set; } = Brushes.Red;

    public bool IsConnected
    {
        get => _isConnected;
        set
        {
            _isConnected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
        }
    }

    public Point Anchor
    {
        get => _anchor;
        set
        {
            _anchor = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Anchor)));
        }
    }

    public ConditionNodeInputConnectorViewModel GetCorrespondingInput() => Parent.GetCorrespondingConnector(this);

    public ConditionNodeViewModel Parent { get; } = parent;
    INodeViewModel IConnectorViewModel.Parent => Parent;

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool _isConnected;
    private Point _anchor;
}
