using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpPluginLoader.Core.Entities;

namespace GeneralUtility;
public class Config
{
    public Dictionary<MonsterType, ActionChain> ActionChains { get; set; } = [];

    public ActionChain GetActionChain(MonsterType monsterType)
    {
        var chain = ActionChains.GetValueOrDefault(monsterType);
        if (chain is null)
        {
            chain = new ActionChain([]);
            ActionChains.Add(monsterType, chain);
        }

        return chain;
    }
}

public class ConfigJson
{
    public ActionChainJson[] ActionChains { get; set; } = [];
}
