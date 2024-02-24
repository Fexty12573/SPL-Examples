using FuzzySharp;

namespace XFsm;

public interface IXFsmNodeFilter
{
    public bool IsVisible(XFsmNode node);
}

public interface IXFsmLinkFilter
{
    public bool IsVisible(XFsmLink link);
}

#region Node Filters

public class DefaultNodeFilter : IXFsmNodeFilter
{
    public bool IsVisible(XFsmNode node)
    {
        return true;
    }
}

public class NodeIdFilter(nint nodeId) : IXFsmNodeFilter
{
    public bool IsVisible(XFsmNode node)
    {
        return node.Id == nodeId;
    }
}

public class NodeNameFilter(string nodeName) : IXFsmNodeFilter
{
    public bool IsVisible(XFsmNode node)
    {
        return node.Name == nodeName;
    }
}

public class NodeNameFuzzyFilter(string nodeName, int threshold = 65) : IXFsmNodeFilter
{
    public bool IsVisible(XFsmNode node)
    {
        return Fuzz.PartialRatio(node.Name, nodeName) >= threshold;
    }
}

public class NodeHasLinkWithNameFilter(string linkName) : IXFsmNodeFilter
{
    public bool IsVisible(XFsmNode node)
    {
        return node.BackingNode.Links.Any(link => link.Name == linkName);
    }
}

#endregion

#region Link Filters

public class DefaultLinkFilter : IXFsmLinkFilter
{
    public bool IsVisible(XFsmLink link)
    {
        return true;
    }
}

public class LinkNameFilter(string linkName) : IXFsmLinkFilter
{
    public bool IsVisible(XFsmLink link)
    {
        return link.Name == linkName;
    }
}

public class LinkNameFuzzyFilter(string linkName, int threshold = 65) : IXFsmLinkFilter
{
    public bool IsVisible(XFsmLink link)
    {
        return Fuzz.PartialRatio(link.Name, linkName) >= threshold;
    }
}

public class LinkSourceFilter(nint sourceId) : IXFsmLinkFilter
{
    public bool IsVisible(XFsmLink link)
    {
        return link.Source.Id == sourceId;
    }
}

public class LinkTargetFilter(nint targetId) : IXFsmLinkFilter
{
    public bool IsVisible(XFsmLink link)
    {
        return link.Target.Id == targetId;
    }
}

public class LinkHasConditionFilter : IXFsmLinkFilter
{
    public bool IsVisible(XFsmLink link)
    {
        return link.BackingLink.HasCondition;
    }
}

#endregion
