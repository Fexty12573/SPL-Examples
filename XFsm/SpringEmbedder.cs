using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core;

namespace XFsm;

public static class SpringEmbedder
{
    /// <summary>
    /// Fruchterman-Reingold algorithm for graph layout
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="links"></param>
    /// <param name="l"></param>
    /// <param name="epsilon"></param>
    public static unsafe bool FruchtermanReingold(List<XFsmNode> nodes, Span<XFsmLink> links, float lR, float lA, float epsilon = 1f)
    {
        var disp = stackalloc Vector2[nodes.Count];

        // Repulsive forces
        Parallel.ForEach(nodes, nodeA =>
        {
            foreach (var nodeB in nodes)
            {
                if (nodeA.Id == nodeB.Id)
                    continue;

                var delta = nodeA.Position - nodeB.Position;
                var distance = Math.Max((nodeA.Position - nodeB.Position).Length(), 0.1f);
                var direction = delta / distance;
                disp[nodeA.Id] += direction * (lR * lR / distance);
            }
        });

        // Attractive forces
        foreach (var link in links)
        {
            var nodeA = link.Source.Parent;
            var nodeB = link.Target.Parent;

            var delta = nodeA.Position - nodeB.Position;
            var distanceSquared = Math.Max((nodeA.Position - nodeB.Position).LengthSquared(), 0.1f);
            var direction = delta / MathF.Sqrt(distanceSquared);
            var d = direction * (distanceSquared / lA);
            disp[nodeA.Id] -= d;
            disp[nodeB.Id] += d;
        }

        var totalDisp = new Vector2(0, 0);

        // Apply Displacements
        for (var i = 0; i < nodes.Count; i++)
        {
            totalDisp += disp[i];
            nodes[i].Position += disp[i];
        }

        return true;
        //return totalDisp.LengthSquared() > epsilon;
    }

    /// <summary>
    /// Customized force-directed layout algorithm
    /// </summary>
    /// <param name="nodes">The nodes to layout</param>
    /// <param name="links">The links connecting the nodes</param>
    /// <param name="rF">Repulsive force factor</param>
    /// <param name="l">Spring length</param>
    /// <param name="sF">Spring force factor</param>
    /// <param name="epsilon">Equilibrium threshold</param>
    /// <returns>True if the layout is in equilibrium</returns>
    public static bool Layout(List<XFsmNode> nodes, Span<XFsmLink> links, float rF, float l, float sF, float epsilon = 0.1f)
    {
        var equilibrium = true;

        // Calculate repulsive forces between nodes
        Parallel.ForEach(nodes, nodeA =>
        {
            var repulsiveForce = Vector2.Zero;
            foreach (var nodeB in nodes)
            {
                if (nodeA.Id != nodeB.Id)
                {
                    var delta = nodeA.Position - nodeB.Position;
                    var distance = delta.Length();
                    if (distance < 1) distance = 1; // Avoid division by zero or very large forces
                    var force = rF / (distance * distance);

                    repulsiveForce += Vector2.Normalize(delta) * force;
                }
            }

            nodeA.Position += repulsiveForce;
        });

        // Calculate attractive forces for links (springs)
        foreach (var link in links)
        {
            var source = link.Source.Parent;
            var target = link.Target.Parent;

            var delta = target.Position - source.Position;
            var distance = delta.Length() - l;
            var force = delta * (distance * sF);

            source.Position += force;
            target.Position -= force;

            // Check for equilibrium in a basic way, you might want to refine this
            if (force.Length() > epsilon) equilibrium = false;
        }

        return equilibrium;
    }
}
