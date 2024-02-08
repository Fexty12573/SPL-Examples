using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FsmEditor;

internal class NodeDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? ConditionNodeTemplate { get; set; }
    public DataTemplate? ActionNodeTemplate { get; set; }

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        return item switch
        {
            ConditionNodeViewModel => ConditionNodeTemplate,
            NodeViewModel => ActionNodeTemplate,
            _ => base.SelectTemplate(item, container)
        };
    }
}
