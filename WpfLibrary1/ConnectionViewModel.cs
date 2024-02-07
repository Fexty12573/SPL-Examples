using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FsmEditor;

internal class ConnectionViewModel
{
    public IConnectorViewModel Source { get; set; }
    public IConnectorViewModel Target { get; set; }

    public ConnectionViewModel(IConnectorViewModel source, IConnectorViewModel target)
    {
        Source = source;
        Target = target;

        Source.IsConnected = true;
        Target.IsConnected = true;
    }
}
