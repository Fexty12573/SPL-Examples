
using System.Text.Json.Serialization;
using SharpPluginLoader.Core.MtTypes;

namespace ColEditor;

public class Config
{
    public float TextSize { get; set; } = 16f;
    public float BoneRadius { get; set; } = 1f;
    public MtColor TextColor { get; set; } = 0xFFFFFFFF;
    public MtColor DefaultBoneColor { get; set; } = 0xFF00FF00;

    public List<BonePreset> BonePresets { get; set; } = [];
}

public class BonePreset
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => _name = value;
    }

    public MtColor Color { get; set; }
    public HashSet<uint> Bones { get; set; } = [];

    [JsonIgnore]
    public ref string RefName => ref _name;
}

public class Settings
{
    public bool ShowBones { get; set; } = true;
    public bool ShowAllColliders { get; set; } = false;
    public bool DrawShellColliders { get; set; } = true;

    public MtColor InactiveColliderColor { get; set; } = new(128, 128, 128, 64);
    public MtColor ActiveColliderSphereColor { get; set; } = new(255, 0, 0, 64);
    public MtColor ActiveColliderCapsuleColor { get; set; } = new(0, 255, 0, 64);
    public MtColor ActiveColliderObbColor { get; set; } = new(0, 0, 255, 64);
    public MtColor NonAttackColliderColor { get; set; } = new(0, 42, 110, 64);
}
