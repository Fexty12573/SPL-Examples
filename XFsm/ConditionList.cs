using System.Collections.Frozen;
using System.Collections.Immutable;
using System.IO;
using System.Text.Json;
using SharpPluginLoader.Core.Entities;

namespace XFsm;

using ConditionPair = (string Original, string Translated);

internal static class ConditionList
{
    public static IReadOnlyList<ConditionPair> Generic { get; }
    public static IReadOnlyDictionary<WeaponType, IReadOnlyList<ConditionPair>> WeaponSpecific { get; }


    private const string ConditionListPath = "nativePC/plugins/CSharp/XFsm/Assets/ConditionList.json";
    static ConditionList()
    {
        var json = File.ReadAllText(ConditionListPath);
        var conditions = JsonSerializer.Deserialize<Dictionary<string, ConditionSet>>(json) 
                         ?? throw new InvalidOperationException("Failed to deserialize condition list.");

        var generic = conditions["Generic"];
        Generic = generic.Original.Zip(generic.Translated, (o, t) => (o, t)).ToImmutableArray();

        Dictionary<WeaponType, IReadOnlyList<ConditionPair>> weaponConditions = [];
        foreach (var (key, value) in conditions)
        {
            if (key == "Generic")
                continue;

            if (!Enum.TryParse(key, out WeaponType weaponType))
                throw new InvalidOperationException($"Failed to parse weapon type: {key}.");

            weaponConditions[weaponType] = value.Original.Zip(value.Translated, (o, t) => (o, t)).ToImmutableArray();
        }

        WeaponSpecific = weaponConditions.ToFrozenDictionary();
    }
}

file class ConditionSet
{
    public List<string> Original { get; set; } = [];
    public List<string> Translated { get; set; } = [];
}
