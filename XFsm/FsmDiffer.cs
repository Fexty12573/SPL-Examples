using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using SharpPluginLoader.Core;

namespace XFsm;

internal static class FsmDiffer
{
    /// <summary>
    /// Diff two FSMs and return the differences.
    /// </summary>
    /// <param name="fsm1">The first FSM.</param>
    /// <param name="fsm2">The second FSM.</param>
    /// <returns>The differences between the two FSMs.</returns>
    public static string[] Diff(AIFSM fsm1, AIFSM fsm2)
    {
        Ensure.NotNull(fsm1.RootCluster);
        Ensure.NotNull(fsm2.RootCluster);
        Ensure.NotNull(fsm1.ConditionTree);
        Ensure.NotNull(fsm2.ConditionTree);

        List<string> differences = [];

        // Compare the nodes in the root cluster.
        differences.AddRange(DiffClusters(fsm1.RootCluster, fsm2.RootCluster));

        // Compare the condition trees.
        differences.AddRange(DiffConditionTrees(fsm1.ConditionTree, fsm2.ConditionTree));

        return [..differences];
    }

    private static string[] DiffClusters(AIFSMCluster cluster1, AIFSMCluster cluster2)
    {
        List<string> differences = [];

        // Check if any nodes are missing in either cluster.
        foreach (var node1 in cluster1.Nodes)
        {
            if (!cluster2.Nodes.Any(node2 => node2.BasicCompare(node1)))
            {
                differences.Add($"Node {node1.Name} from FSM 1 is missing in FSM 2");
            }
        }

        foreach (var node2 in cluster2.Nodes)
        {
            if (!cluster1.Nodes.Any(node1 => node1.BasicCompare(node2)))
            {
                differences.Add($"Node {node2.Name} from FSM 2 is missing in FSM 1");
            }
        }

        // Compare nodes with the same id
        foreach (var node1 in cluster1.Nodes)
        {
            var node2 = cluster2.Nodes.FirstOrDefault(n => n.Id == node1.Id);
            if (node2 is not null)
            {
                differences.AddRange(DiffNodes(node1, node2));
            }
        }

        return [..differences];
    }

    private static string[] DiffNodes(AIFSMNode node1, AIFSMNode node2)
    {
        List<string> differences = [];

        // Check if the node names are different.
        if (node1.Name != node2.Name)
        {
            differences.Add($"Node {node1.Id} in FSM 1 has a different name than node {node2.Id} in FSM 2");
        }

        // Check if the node types are different.
        if (node1.LinkCount != node2.LinkCount)
        {
            differences.Add($"Node {node1.Id} in FSM 1 has a different number of links than node {node2.Id} in FSM 2");
        }

        // Check if the node links are different.
        for (var i = 0; i < node1.LinkCount; i++)
        {
            var link1 = node1.Links[i];
            var link2 = node2.Links[i];

            if (link1 is null || link2 is null)
            {
                differences.Add($"Link {i} in node {node1.Id} in FSM 1 is missing in node {node2.Id} in FSM 2");
                continue;
            }

            if (link1.Name != link2.Name)
            {
                differences.Add($"Link {i} in node {node1.Id} in FSM 1 has a different name than link {i} in node {node2.Id} in FSM 2");
            }

            if (link1.ConditionId != link2.ConditionId)
            {
                differences.Add($"Link {i} in node {node1.Id} in FSM 1 has a different condition id than link {i} in node {node2.Id} in FSM 2");
            }

            if (link1.HasCondition && !link2.HasCondition)
            {
                differences.Add($"Link {i} in node {node1.Id} in FSM 1 has a condition while link {i} in node {node2.Id} in FSM 2 does not");
            }
            else if (!link1.HasCondition && link2.HasCondition)
            {
                differences.Add($"Link {i} in node {node1.Id} in FSM 1 does not have a condition while link {i} in node {node2.Id} in FSM 2 does");
            }

            if (link1.DestinationNodeId != link2.DestinationNodeId)
            {
                differences.Add($"Link {i} in node {node1.Id} in FSM 1 has a different destination node than link {i} in node {node2.Id} in FSM 2");
            }
        }

        // Check the node processes
        if (node1.ProcessCount != node2.ProcessCount)
        {
            differences.Add($"Node {node1.Id} in FSM 1 has a different number of processes than node {node2.Id} in FSM 2");
        }

        for (var i = 0; i < node1.ProcessCount; i++)
        {
            var process1 = node1.Processes[i];
            var process2 = node2.Processes[i];

            if (process1 is null || process2 is null)
            {
                differences.Add($"Process {i} in node {node1.Id} in FSM 1 is missing in node {node2.Id} in FSM 2");
                continue;
            }

            if (process1.ContainerName != process2.ContainerName)
            {
                differences.Add($"Process {i} in node {node1.Id} in FSM 1 has a different container name than process {i} in node {node2.Id} in FSM 2");
            }

            if (process1.CategoryName != process2.CategoryName)
            {
                differences.Add($"Process {i} in node {node1.Id} in FSM 1 has a different category name than process {i} in node {node2.Id} in FSM 2");
            }

            if (process1.Parameter?.GetDti()?.Name != process2.Parameter?.GetDti()?.Name)
            {
                differences.Add($"Process {i} in node {node1.Id} in FSM 1 has a different parameter name than process {i} in node {node2.Id} in FSM 2");
            }
        }

        return [..differences];
    }

    private static string[] DiffConditionTrees(AIConditionTree tree1, AIConditionTree tree2)
    {
        List<string> differences = [];

        // Check if any conditions are missing in either tree.
        var conds1 = tree1.TreeList;
        var conds2 = tree2.TreeList;

        foreach (var cond1 in conds1)
        {
            if (!conds2.Any(cond2 => cond2.BasicCompare(cond1)))
            {
                differences.Add($"Condition {cond1.Name.Id} from tree 1 is missing in tree 2");
            }
        }

        foreach (var cond2 in conds2)
        {
            if (!conds1.Any(cond1 => cond1.BasicCompare(cond2)))
            {
                differences.Add($"Condition {cond2.Name.Id} from tree 2 is missing in tree 1");
            }
        }

        // Compare conditions with the same id
        foreach (var cond1 in conds1)
        {
            var cond2 = conds2.FirstOrDefault(c => c.Name.Id == cond1.Name.Id);
            if (cond2 is not null)
            {
                differences.AddRange(DiffConditions(cond1, cond2));
            }
        }

        return [..differences];
    }

    private static string[] DiffConditions(AIConditionTreeInfo cond1, AIConditionTreeInfo cond2)
    {
        List<string> differences = [];

        var root1 = cond1.RootNode;
        var root2 = cond2.RootNode;

        if (root1 is null || root2 is null)
        {
            differences.Add($"Condition {cond1.Name.Id} in FSM 1 is missing a root node or condition {cond2.Name.Id} in FSM 2 is missing a root node");
            return [..differences];
        }

        differences.AddRange(DiffConditionNodes(root1, root2, cond1.Name.Id, cond2.Name.Id));

        return [..differences];
    }

    private static string[] DiffConditionNodes(AIConditionTreeNode node1, AIConditionTreeNode node2, int id1, int id2)
    {
        List<string> differences = [];

        var isRoot = node1.Parent is null && node2.Parent is null;
        var nodeKind = isRoot ? "root node" : "child node";

        if (node1.Type != node2.Type)
        {
            differences.Add($"{nodeKind} of condition {id1} in FSM 1 has a different type than {nodeKind} of condition {id2} in FSM 2");

            // If the types are different, there is no point in comparing them further.
            return [.. differences];
        }

        // Compare the children of the nodes.
        if (node1.ChildCount != node2.ChildCount)
        {
            differences.Add($"{nodeKind} of condition {id1} in FSM 1 has a different number of children than {nodeKind} of condition {id2} in FSM 2");
        }

        for (var i = 0; i < node1.ChildCount; i++)
        {
            var child1 = node1.Children[i];
            var child2 = node2.Children[i];

            if (child1 is null || child2 is null)
            {
                differences.Add($"Child {i} of {nodeKind} of condition {id1} in FSM 1 is missing or child {i} of {nodeKind} of condition {id2} in FSM 2 is missing");
                continue;
            }

            differences.AddRange(DiffConditionNodes(child1, child2, id1, id2));
        }

        return [..differences];
    }
}

internal static class AIFSMExtensions
{
    public static bool BasicCompare(this AIFSMNode node1, AIFSMNode node2)
    {
        return node1.Name == node2.Name;
    }

    public static bool BasicCompare(this AIConditionTreeInfo cond1, AIConditionTreeInfo cond2)
    {
        return cond1.Name.Id == cond2.Name.Id;
    }
}
