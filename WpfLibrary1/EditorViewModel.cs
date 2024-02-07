using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core;

namespace FsmEditor;

internal class EditorViewModel
{
    public ObservableCollection<INodeViewModel> Nodes { get; } = [];
    public ObservableCollection<ConnectionViewModel> Connections { get; } = [];
    public PendingConnectionViewModel PendingConnection { get; }
    private AIFSM? _fsm;

    public EditorViewModel()
    {
        PendingConnection = new PendingConnectionViewModel(this);
    }

    public void LoadFsm(AIFSM fsm)
    {
        _fsm = fsm;
        Nodes.Clear();

        var rootCluster = fsm.RootCluster;
        if (rootCluster is null)
            return;

        foreach (var node in rootCluster.Nodes)
        {
            Nodes.Add(new NodeViewModel(node));
        }

        if (fsm.ConditionTree is null)
            return;

        foreach (ref var treeInfo in fsm.ConditionTree.TreeList)
        {
            Nodes.Add(new ConditionNodeViewModel(ref treeInfo));
        }

        var nodes = Nodes.OfType<NodeViewModel>().ToList();
        var conditionNodes = Nodes.OfType<ConditionNodeViewModel>().ToList();
        foreach (var node in nodes)
        {
            var backingNode = node.BackingNode;
            for (var i = 0; i < backingNode.LinkCount; i++)
            {
                NodeOutputConnectorViewModel? source;
                IConnectorViewModel? target;

                ref var link = ref backingNode.Links[i];
                var destId = link.DestinationNodeId;
                var targetNode = nodes.Find(n => n.Id == backingNode.Links[i].DestinationNodeId);

                if (!link.HasCondition)
                {
                    source = node.OutputConnectors[i];
                    target = targetNode?.InputConnector;

                    if (target is not null)
                    {
                        Connections.Add(new ConnectionViewModel(source, target));
                    }
                    else
                    {
                        Log.Warn($"Could not find node with id {destId}");
                    }

                    continue;
                }

                var condId = link.ConditionId;
                var condition = conditionNodes.Find(n => n.Id == condId);
                if (condition is null)
                {
                    Log.Warn($"Could not find condition with id {condId}");
                    continue;
                }

                source = node.OutputConnectors[i];
                target = new ConditionNodeInputConnectorViewModel(source);
                var conditionSource = new ConditionNodeOutputConnectorViewModel();
                var conditionTarget = targetNode?.InputConnector;

                if (conditionTarget is null)
                {
                    Log.Warn($"Could not find node with id {destId}");
                    continue;
                }

                condition.InputConnectors.Add((ConditionNodeInputConnectorViewModel)target);
                condition.OutputConnectors.Add(conditionSource);

                Connections.Add(new ConnectionViewModel(source, target));
                Connections.Add(new ConnectionViewModel(conditionSource, conditionTarget));
            }
        }
    }

    public void TryConnect(IConnectorViewModel source, IConnectorViewModel target)
    {
        // TODO: Implement
    }
}
