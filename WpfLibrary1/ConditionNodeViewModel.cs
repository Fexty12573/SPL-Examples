using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core.Memory;

namespace FsmEditor;

using ConditionNodeConnectorPair = (ConditionNodeInputConnectorViewModel, ConditionNodeOutputConnectorViewModel);

internal unsafe class ConditionNodeViewModel(ref AIConditionTreeInfo conditionTreeInfo) : INodeViewModel
{
    public uint Id => _conditionTreeInfo->Name.Id;
    public string Name { get; set; } = $"{conditionTreeInfo.Name.Id}: {conditionTreeInfo.Name.Name}";

    public ObservableCollection<ConditionNodeInputConnectorViewModel> InputConnectors { get; } = [];
    public ObservableCollection<ConditionNodeOutputConnectorViewModel> OutputConnectors { get; } = [];

    public ref AIConditionTreeInfo ConditionTreeInfo => ref *_conditionTreeInfo;

    public ConditionNodeOutputConnectorViewModel GetCorrespondingConnector(ConditionNodeInputConnectorViewModel input)
    {
        return OutputConnectors[InputConnectors.IndexOf(input)];
    }
    public ConditionNodeInputConnectorViewModel GetCorrespondingConnector(ConditionNodeOutputConnectorViewModel output)
    {
        return InputConnectors[OutputConnectors.IndexOf(output)];
    }

    private readonly AIConditionTreeInfo* _conditionTreeInfo = MemoryUtil.AsPointer(ref conditionTreeInfo);
}
