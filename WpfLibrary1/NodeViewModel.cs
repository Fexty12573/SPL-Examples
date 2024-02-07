﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FsmEditor;

internal class NodeViewModel : INodeViewModel
{
    public AIFSMNode BackingNode { get; }

    public uint Id => BackingNode.Id;
    public string Name { get; set; }
    public uint UniqueId
    {
        get => BackingNode.UniqueId;
        set => BackingNode.UniqueId = value;
    }

    public NodeInputConnectorViewModel InputConnector { get; } = new("Input");
    public ObservableCollection<NodeOutputConnectorViewModel> OutputConnectors { get; } = [];

    public NodeViewModel(AIFSMNode backingNode)
    {
        BackingNode = backingNode;
        Name = BackingNode.Name;

        foreach (ref var link in BackingNode.Links)
        {
            OutputConnectors.Add(new NodeOutputConnectorViewModel(ref link));
        }
    }
}
