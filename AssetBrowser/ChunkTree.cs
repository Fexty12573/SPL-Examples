using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core;

namespace AssetBrowser;

internal class ChunkTree
{
    public static ChunkTree Parse(IEnumerable<string> paths)
    {
        var tree = new ChunkTree();

        foreach (var path in paths)
        {
            tree.InsertPath(path);
        }

        return tree;
    }

    public ChunkNode Root { get; } = new("chunk", null);

    public void InsertPath(string path)
    {
        var current = Root;
        string? lastPart = null;

        foreach (var part in path.Split('\\'))
        {
            if (part.Length == 0)
                continue;

            if (!current.Children.TryGetValue(part, out var value))
            {
                value = new ChunkNode(part, current);
                current.Children[part] = value;
            }

            current = value;
            lastPart = part;
        }

        if (lastPart is not null && lastPart.Contains('.'))
            current.IsFile = true;
    }

    public void Print(Action<string>? sink = null)
    {
        Print(Root, "", true, sink ?? Log.Info);
    }

    private void Print(ChunkNode chunkNode, string indent, bool last, Action<string> sink)
    {
        if (chunkNode == Root)
        {
            sink("/");
        }
        else
        {
            sink(indent + (last ? "└── " : "├── ") + chunkNode.Name);
        }

        foreach (var (name , child) in chunkNode.Children)
        {
            Print(child, indent + (last ? "    " : "│   "), name == chunkNode.Children.Last().Key, sink);
        }
    }
}

internal class ChunkNode(string name, ChunkNode? parent)
{
    public string Name { get; } = name;
    public Dictionary<string, ChunkNode> Children { get; } = [];
    public ChunkNode? Parent { get; } = parent;
    public bool IsFile { get; set; }

    public string FullName => Parent is null ? Name : Parent.FullName + "\\" + Name;
}
