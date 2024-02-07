using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core.Entities;

namespace GeneralUtility;
public class ActionChain(IEnumerable<int> actions)
{
    public bool IsActive { get; private set; }

    public int[] Actions { get; private set; } = actions.ToArray();

    public bool GetNextAction(out int action)
    {
        if (!IsActive)
        {
            action = -1;
            return false;
        }

        if (_index >= Actions.Length)
        {
            if (_loopCount > 0)
            {
                _loopCount--;
                _index = 0;
            }
            else
            {
                action = -1;
                IsActive = false;
                return false;
            }
        }

        action = Actions[_index++];
        return true;
    }

    public void Start(int loopCount = 0)
    {
        _loopCount = loopCount;
        IsActive = true;
        _index = 0;
    }

    public void Stop()
    {
        IsActive = false;
    }

    public void UpdateActions(IEnumerable<int> newActions)
    {
        Actions = newActions.ToArray();
    }

    private int _index;
    private int _loopCount;
}

public class ActionChainJson
{
    public int MonsterId { get; set; }
    public int[] Actions { get; set; } = [];

    public ActionChain ToActionChain() => new(Actions);
}
