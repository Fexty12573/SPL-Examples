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
        Connections.Clear();

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
                target = new ConditionNodeInputConnectorViewModel(source, condition);

                Connect(source, target);
                
                var conditionSource = new ConditionNodeOutputConnectorViewModel(condition);
                var conditionTarget = targetNode?.InputConnector;

                condition.InputConnectors.Add((ConditionNodeInputConnectorViewModel)target);
                condition.OutputConnectors.Add(conditionSource);

                if (conditionTarget is null)
                {
                    Log.Warn($"Could not find node with id {destId}");
                    continue;
                }

                Connect(conditionSource, conditionTarget);
            }
        }
    }

    public bool TryConnect(IConnectorViewModel source, IConnectorViewModel target)
    {
        // There are 3 possible scenarios when connecting nodes:
        // 1. Connecting a node to another node
        // In this scenario, the following things must be done:
        // - All existing connections from the source node must be removed
        // - A new connection must be created between the source and target nodes
        // - The Link.DestinationNodeId must be updated to the target node's id
        // - Link.HasCondition must be set to false (because we are connecting to a node, not a condition)
        //
        // 2. Connecting a node to a condition
        // In this scenario, the following things must be done:
        // - All existing connections from the source node must be removed
        // - If the target condition's input connector already has a connection, the connection must be removed
        //      - Additionally, whichever node was previously connected to the condition must have its Link.HasCondition set to false,
        //        and there must be a new connection created between that node and its target node (no condition)
        //      - The target condition's output connector must also be disconnected from its target node
        // - A new connection must be created between the source node and the target condition
        // - The source node's Link.ConditionId must be updated to the target condition's id
        // - (Optionally) The target condition's output connector must be connected to the target node
        //   This depends on how natural that feels to the user. It might be better to let the user do this manually
        //
        // 3. Connecting a condition to a node
        // In this scenario, the following things must be done:
        // - If the source condition's output connector already has a connection, the connection must be removed
        // - The source condition's output connector must be connected to the target node
        // - The source condition's corresponding input connector must be used to obtain the source node/link
        // - The source node/link must have its Link.DestinationNodeId updated to the target node's id
        //
        // Connecting a condition to another condition is not allowed

        // TODO: Maybe remove this
        // Handle the opposite cases, where the source and target are swapped
        if ((target is NodeOutputConnectorViewModel && 
            (source is NodeInputConnectorViewModel or ConditionNodeInputConnectorViewModel)) ||
            (target is ConditionNodeOutputConnectorViewModel && source is NodeInputConnectorViewModel))
        {
            (source, target) = (target, source);
        }

        if (source is NodeOutputConnectorViewModel sourceNode)
        {
            if (target is NodeInputConnectorViewModel targetNode) // Scenario 1: Node to Node
            {
                if (sourceNode.IsConnected)
                {
                    // Remove all existing connections from the source node
                    DisconnectAllFrom(sourceNode);
                }

                sourceNode.IsConnected = true;
                targetNode.IsConnected = true;

                sourceNode.Link.DestinationNodeId = targetNode.Parent.Id;
                sourceNode.Link.HasCondition = false;

                Connect(sourceNode, targetNode);
                return true;
            }
            else if (target is ConditionNodeInputConnectorViewModel targetCondition) // Scenario 2: Node to Condition
            {
                if (sourceNode.IsConnected)
                {
                    // Remove all existing connections from the source node
                    DisconnectAllFrom(sourceNode);
                }

                if (targetCondition.IsConnected)
                {
                    RemoveCondition(targetCondition.Source);
                }

                sourceNode.IsConnected = true;
                targetCondition.IsConnected = true;
                targetCondition.Source = sourceNode;

                Connect(sourceNode, targetCondition);

                sourceNode.Link.HasCondition = true;
                sourceNode.Link.ConditionId = targetCondition.Parent.Id;

                // (Optionally) The target condition's output connector must be connected to the target node
                // This depends on how natural that feels to the user. It might be better to let the user do this manually

                var destinationNode = GetNodeById(sourceNode.Link.DestinationNodeId);
                if (destinationNode is not null)
                {
                    Connect(targetCondition.GetCorrespondingOutput(), destinationNode.InputConnector);
                }

                return true;
            }
        }
        else if (source is ConditionNodeOutputConnectorViewModel sourceCondition) // Scenario 3: Condition to Node
        {
            if (target is NodeInputConnectorViewModel targetNode)
            {
                if (sourceCondition.IsConnected)
                {
                    DisconnectAllFrom(sourceCondition);
                }

                sourceNode = sourceCondition.GetCorrespondingInput().Source;
                sourceNode.Link.DestinationNodeId = targetNode.Parent.Id;

                Connect(sourceCondition, targetNode);

                return true;
            }
        }

        return false;
    }

    private void Connect(IConnectorViewModel source, IConnectorViewModel target)
    {
        Connections.Add(new ConnectionViewModel(source, target));
    }

    private void DisconnectAllFrom(IConnectorViewModel source)
    {
        foreach (var connection in Connections.Where(c => c.Source == source).ToList())
        {
            Connections.Remove(connection);
        }
    }

    private void DisconnectAllTo(IConnectorViewModel target)
    {
        foreach (var connection in Connections.Where(c => c.Target == target).ToList())
        {
            Connections.Remove(connection);
        }
    }

    private void RemoveCondition(NodeOutputConnectorViewModel source)
    {
        if (!source.Link.HasCondition)
            return;

        var targetNode = GetNodeById(source.Link.DestinationNodeId);
        if (targetNode is null)
            return;

        var currentCondition = GetConditionById(source.Link.ConditionId);
        if (currentCondition is null)
            return;

        var connection = GetConnectionsFrom(source).FirstOrDefault();
        if (connection is null)
            return;

        source.Link.HasCondition = false;

        // Remove the connection from the source node to the condition
        Connections.Remove(connection);

        var conditionOutput = currentCondition
            .GetCorrespondingConnector((ConditionNodeInputConnectorViewModel)connection.Target);

        // Remove the connection from the condition to the target node
        DisconnectAllFrom(conditionOutput);

        // Connect the source node to the target node directly
        Connect(source, targetNode.InputConnector);
    }

    private IEnumerable<ConnectionViewModel> GetConnectionsFrom(IConnectorViewModel source)
    {
        return Connections.Where(c => c.Source == source);
    }

    public IEnumerable<ConnectionViewModel> GetConnectionsTo(IConnectorViewModel target)
    {
        return Connections.Where(c => c.Target == target);
    }

    public NodeViewModel? GetNodeById(uint id)
    {
        return Nodes.OfType<NodeViewModel>().FirstOrDefault(n => n.Id == id);
    }

    public ConditionNodeViewModel? GetConditionById(uint id)
    {
        return Nodes.OfType<ConditionNodeViewModel>().FirstOrDefault(n => n.Id == id);
    }
}
