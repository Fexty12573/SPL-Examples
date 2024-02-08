using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

    public NodeInputConnectorViewModel InputConnector { get; }
    public ObservableCollection<NodeOutputConnectorViewModel> OutputConnectors { get; } = [];

    public NodeViewModel(AIFSMNode backingNode)
    {
        BackingNode = backingNode;
        Name = BackingNode.Name;

        InputConnector = new NodeInputConnectorViewModel("In", this);

        foreach (ref var link in BackingNode.Links)
        {
            OutputConnectors.Add(new NodeOutputConnectorViewModel(ref link, this));
        }
    }
}
