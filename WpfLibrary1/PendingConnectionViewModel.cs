using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace FsmEditor;

internal class PendingConnectionViewModel
{
    private readonly EditorViewModel _editor;
    private IConnectorViewModel? _source;

    public PendingConnectionViewModel(EditorViewModel editor)
    {
        _editor = editor;

        StartCommand = new DelegateCommand<IConnectorViewModel>(source => _source = source);
        EndCommand = new DelegateCommand<IConnectorViewModel>(target =>
        {
            if (_source is null)
                return;

            _editor.TryConnect(_source, target);
        });
    }

    public ICommand StartCommand { get; }
    public ICommand EndCommand { get; }
}
