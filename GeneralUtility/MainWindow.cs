using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpPluginLoader.Core;
using SharpPluginLoader.Core.Actions;
using SharpPluginLoader.Core.Components;
using SharpPluginLoader.Core.Entities;
using SharpPluginLoader.Core.Memory;
using SharpPluginLoader.Core.MtTypes;

namespace GeneralUtility;
public partial class MainWindow : Form
{
    private readonly Plugin _plugin;
    private readonly Thread _updateThread;
    private readonly NativeFunction<nint, MonsterType, int, bool, nint> _spawnMonsterFunc;
    private readonly NativeFunction<int, int, nint, nint> _spawnGimmickFunc;
    private readonly Patch[] _coordUpdatePatches =
    [
        new Patch((nint)0x141fbb9d7, Enumerable.Repeat<byte>(0x90, 8).ToArray()),
        new Patch((nint)0x141fbb9e5, Enumerable.Repeat<byte>(0x90, 8).ToArray()),
        new Patch((nint)0x141fbb9ed, Enumerable.Repeat<byte>(0x90, 8).ToArray())
    ];
    private readonly Patch _plTransparencyResetPatch = new((nint)0x141F6D852, Enumerable.Repeat<byte>(0x90, 8).ToArray());
    private bool _isRecordingSpeed = false;
    private Vector3 _recordingStartPos = Vector3.Zero;
    private DateTime _recordingStartTime = DateTime.Now;

    public Monster? SelectedMonster => (Monster?)cbSelectedMonster.SelectedItem;

    public bool StickXLocked => cbLockStickX.Checked;
    public bool StickYLocked => cbLockStickY.Checked;
    public int StickX => (int)((float)tbStickX.Value * (32767f / 100f));
    public int StickY => (int)((float)tbStickY.Value * (32767f / 100f));
    public bool PlayerPosLocked => cbLockPlayerToEm.Checked;
    public Vector3 PlayerPosOffset => new(
        (float)tbPlLockOffsetX.Value,
        (float)tbPlLockOffsetY.Value,
        (float)tbPlLockOffsetZ.Value
    );

    public MainWindow(Plugin plugin)
    {
        _plugin = plugin;
        InitializeComponent();

        cbSelectedMonster.DataSource = _plugin.Monsters;
        cbSpawnMonster.DataSource = Enum.GetValues<MonsterType>();
        cbSpawnGimmick.DataSource = GimmickNames.All;

        var addresses = PatternScanner.Scan(Pattern.FromString("33 C0 48 8D 4F 38 0F 1F 00"));
        _spawnMonsterFunc = new NativeFunction<nint, MonsterType, int, bool, nint>(addresses.FirstOrDefault(0) - 119);
        if (_spawnMonsterFunc.NativePointer <= 0)
        {
            Log.Error("Failed to find spawn monster function.");
        }

        addresses = PatternScanner.Scan(Pattern.FromString("44 8B E2 8B E9 48 85 FF 74 30 33 F6 8B DE"));
        _spawnGimmickFunc = new NativeFunction<int, int, nint, nint>(addresses.FirstOrDefault(0) - 34);
        if (_spawnGimmickFunc.NativePointer <= 0)
        {
            Log.Error("Failed to find spawn gimmick function.");
        }

        _updateThread = new Thread(UpdateThread);
        _updateThread.Start();
    }

    public bool IsSelectedMonster(Entity entity)
    {
        if (cbSelectedMonster.SelectedItem is null)
            return false;

        return ((Monster)cbSelectedMonster.SelectedItem).Instance == entity.Instance;
    }

    public void SetActionAnimation(ActionInfo action, AnimationId animation)
    {
        tbMonsterAction.Invoke(() =>
        {
            tbMonsterAction.Text = $@"{action}";
            tbMonsterAnim.Text = $@"{animation}";
        });
    }

    public void SetMonsterCoords(Vector3 coords)
    {
        tbMonsterCoords.Invoke(() =>
        {
            tbMonsterCoords.Text = $@"X: {coords.X,-12:.02f} | Y: {coords.Y,-12:.02f} | Z: {coords.Z,-12:.02f}";
        });
    }

    public void SetPlayerCoords(Vector3 coords)
    {
        tbPlayerCoords.Invoke(() =>
        {
            tbPlayerCoords.Text = $@"X: {coords.X,-12:.02f} | Y: {coords.Y,-12:.02f} | Z: {coords.Z,-12:.02f}";
        });
    }

    public void UpdateLoadedMonsters()
    {
        tbLoadedMonsters.Invoke(() =>
        {
            cbSelectedMonster.DataSource = null;
            cbSelectedMonster.DataSource = _plugin.Monsters;

            var sb = new StringBuilder(2048);
            foreach (var monster in _plugin.Monsters)
            {
                sb.AppendLine($"{monster.Name} ID: {(int)monster.Type} Variant: {monster.Variant} @ 0x{monster.Instance:X}");
            }

            tbLoadedMonsters.Text = sb.ToString();
        });
    }

    private void UpdateThread()
    {
        var lastUpdateTime = DateTime.Now;
        while (true)
        {
            var deltaTime = (float)(DateTime.Now - lastUpdateTime).TotalMilliseconds;
            if (deltaTime > 100)
            {
                lastUpdateTime = DateTime.Now;
                var monster = SelectedMonster;
                if (monster is null)
                    continue;

                var action = monster.GetCurrentAction();
                var animation = monster.CurrentAnimation;

                SetActionAnimation(action, animation);
                SetMonsterCoords(monster.Position);

                var player = Player.MainPlayer;
                if (player is null)
                    continue;

                SetPlayerCoords(player.Position);

                //if (cbLockPlayerToEm.Checked)
                //{
                //    var offset = new Vector3(
                //        (float)tbPlLockOffsetX.Value,
                //        (float)tbPlLockOffsetY.Value,
                //        (float)tbPlLockOffsetZ.Value
                //    );

                //    player.Teleport(monster.Position + offset);
                //}
            }

            Thread.Sleep(10);
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    private void cbCoordsLocked_CheckedChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        if (cbCoordsLocked.Checked)
        {
            _plugin.LockCoordinates(SelectedMonster);
        }
        else
        {
            _plugin.UnlockCoordinates(SelectedMonster);
        }
    }

    private void cbMonsterFrozen_CheckedChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        SelectedMonster.Frozen = cbMonsterFrozen.Checked;
    }

    private void cbDisableSpeedReset_CheckedChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        if (cbDisableSpeedReset.Checked)
        {
            Monster.DisableSpeedReset();
        }
        else
        {
            Monster.EnableSpeedReset();
        }
    }

    private void btnSetX_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        var coords = SelectedMonster.Position;
        coords.X = (float)tbMonsterX.Value;
        SelectedMonster.Position = coords;

        if (cbCoordsLocked.Checked)
            _plugin.LockCoordinates(SelectedMonster, coords);
    }

    private void btnSetY_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        var coords = SelectedMonster.Position;
        coords.Y = (float)tbMonsterY.Value;
        SelectedMonster.Position = coords;

        if (cbCoordsLocked.Checked)
            _plugin.LockCoordinates(SelectedMonster, coords);
    }

    private void btnSetZ_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        var coords = SelectedMonster.Position;
        coords.Z = (float)tbMonsterZ.Value;
        SelectedMonster.Position = coords;

        if (cbCoordsLocked.Checked)
            _plugin.LockCoordinates(SelectedMonster, coords);
    }

    private void btnSetXYZ_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        var coords = new Vector3((float)tbMonsterX.Value, (float)tbMonsterY.Value, (float)tbMonsterZ.Value);
        SelectedMonster.Teleport(coords);

        if (cbCoordsLocked.Checked)
            _plugin.LockCoordinates(SelectedMonster, coords);
    }

    private void btnTpEmToPl_Click(object sender, EventArgs e)
    {
        var player = Player.MainPlayer;
        if (player is null)
            return;

        SelectedMonster?.Teleport(player.Position);
    }

    private void btnTpPlToEm_Click(object sender, EventArgs e)
    {
        var player = Player.MainPlayer;
        if (player is null || SelectedMonster is null)
            return;

        player.Teleport(SelectedMonster.Position);
    }

    private void btnCopyCoords_Click(object sender, EventArgs e)
    {
        Clipboard.SetText($@"{(float)tbMonsterX.Value},{(float)tbMonsterY.Value},{(float)tbMonsterZ.Value}");
        SystemSounds.Beep.Play();
    }

    private void btnPaseCoords_Click(object sender, EventArgs e)
    {
        var coords = Clipboard.GetText().Split(',');
        if (coords.Length != 3)
            return;

        if (float.TryParse(coords[0], out var x) &&
            float.TryParse(coords[1], out var y) &&
            float.TryParse(coords[2], out var z))
        {
            tbMonsterX.Value = (decimal)x;
            tbMonsterY.Value = (decimal)y;
            tbMonsterZ.Value = (decimal)z;
        }
    }

    private void btnSetToCurrent_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        var coords = SelectedMonster.Position;
        tbMonsterX.Value = (decimal)coords.X;
        tbMonsterY.Value = (decimal)coords.Y;
        tbMonsterZ.Value = (decimal)coords.Z;
    }

    private void btnSetPlX_Click(object sender, EventArgs e)
    {
        var player = Player.MainPlayer;
        if (player is null)
            return;

        player.Position.X = (float)tbPlayerX.Value;
    }

    private void btnSetPlY_Click(object sender, EventArgs e)
    {
        var player = Player.MainPlayer;
        if (player is null)
            return;

        player.Position.Y = (float)tbPlayerY.Value;
    }

    private void btnSetPlZ_Click(object sender, EventArgs e)
    {
        var player = Player.MainPlayer;
        if (player is null)
            return;

        player.Position.Z = (float)tbPlayerZ.Value;
    }

    private void btnSetPlXYZ_Click(object sender, EventArgs e)
    {
        Player.MainPlayer?.Teleport(new Vector3(
            (float)tbPlayerX.Value,
            (float)tbPlayerY.Value,
            (float)tbPlayerZ.Value
        ));
    }

    private void btnCopyPlCoords_Click(object sender, EventArgs e)
    {
        var player = Player.MainPlayer;
        if (player is null)
            return;

        Clipboard.SetText($@"{player.Position.X},{player.Position.Y},{player.Position.Z}");
    }

    private void btnPastePlCoords_Click(object sender, EventArgs e)
    {
        var player = Player.MainPlayer;
        if (player is null)
            return;

        var coords = Clipboard.GetText().Split(',');
        if (coords.Length != 3)
            return;

        if (float.TryParse(coords[0], out var x) &&
            float.TryParse(coords[1], out var y) &&
            float.TryParse(coords[2], out var z))
        {
            tbPlayerX.Value = (decimal)x;
            tbPlayerY.Value = (decimal)y;
            tbPlayerZ.Value = (decimal)z;
        }
    }

    private void btnForceAction_Click(object sender, EventArgs e)
    {
        SelectedMonster?.ForceAction((int)nudMonsterAction.Value);
    }

    private void btnOverrideAction_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        _plugin.OverrideActionId = (int)nudMonsterAction.Value;
    }

    private void cbLockActions_CheckedChanged(object sender, EventArgs e)
    {
        _plugin.LockOverrideAction = cbLockActions.Checked;
    }

    private void btnExecActionChain_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        IEnumerable<int> actions;
        try
        {
            actions = tbActionChain.Text
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(int.Parse);
        }
        catch (Exception)
        {
            MessageBox.Show(@"Invalid action chain format.", null, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var actionChain = _plugin.GetActionChain(SelectedMonster.Type);
        if (actionChain is null)
        {
            actionChain = new ActionChain(actions);
            _plugin.ActionChains[SelectedMonster.Type] = actionChain;
        }
        else
        {
            actionChain.UpdateActions(actions);
        }

        actionChain.Start((int)tbActionChainLoopCount.Value);
    }

    private void btnLoadChainFromCfg_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        tbActionChain.Text = string.Join(", ", _plugin.Config.GetActionChain(SelectedMonster.Type).Actions);
    }

    private void btnCancelActionChain_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        _plugin.GetActionChain(SelectedMonster.Type)?.Stop();
    }

    private void btnSaveActionChainToCfg_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        int[] actions;
        try
        {
            actions = tbActionChain.Text.Split(',').Select(int.Parse).ToArray();
        }
        catch (Exception)
        {
            MessageBox.Show(@"Invalid action chain format.", null, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _plugin.ActionChains[SelectedMonster.Type] = new ActionChain(actions);
        _plugin.Config.ActionChains[SelectedMonster.Type] = new ActionChain(actions);
    }

    private void cbSelectedMonster_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        var monster = SelectedMonster;
        var actionChain = _plugin.GetActionChain(monster.Type);
        tbActionChain.Text = actionChain is null ? "" : string.Join(", ", actionChain.Actions);

        if (_plugin.LockedCoordinates.TryGetValue(monster, out var coords))
        {
            cbCoordsLocked.Checked = true;
            tbMonsterX.Value = (decimal)coords.X;
            tbMonsterY.Value = (decimal)coords.Y;
            tbMonsterZ.Value = (decimal)coords.Z;
        }
        else
        {
            cbCoordsLocked.Checked = false;
            tbMonsterX.Value = (decimal)monster.Position.X;
            tbMonsterY.Value = (decimal)monster.Position.Y;
            tbMonsterZ.Value = (decimal)monster.Position.Z;
        }

        if (_plugin.LockedTargetCoordinates.TryGetValue(monster, out var targetCoords))
        {
            cbLockTargetCoords.Checked = true;
            tbTargetX.Value = (decimal)targetCoords.X;
            tbTargetY.Value = (decimal)targetCoords.Y;
            tbTargetZ.Value = (decimal)targetCoords.Z;
        }
        else
        {
            cbLockTargetCoords.Checked = false;
            tbTargetX.Value = (decimal)monster.Position.X;
            tbTargetY.Value = (decimal)monster.Position.Y;
            tbTargetZ.Value = (decimal)monster.Position.Z;
        }

        cbMonsterFrozen.Checked = monster.Frozen;
        tbMonsterSpeed.Value = (decimal)monster.Speed;

        cbCoordsLocked.Checked = _plugin.LockedCoordinates.ContainsKey(monster);
        cbLockTargetCoords.Checked = _plugin.LockedTargetCoordinates.ContainsKey(monster);

        tbMonsterTransparency.Value = (decimal)(SelectedMonster.GetTransparency() * 100);
        nudMonsterHealth.Value = (decimal)SelectedMonster.Health;

        _isRecordingSpeed = false;
        btnRecordSpeed.Text = @"Record Speed";
        tbSpeedRecordingResult.Text = "";
    }

    private void cbLockTargetCoords_CheckedChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        if (cbLockTargetCoords.Checked)
        {
            _plugin.LockedTargetCoordinates[SelectedMonster] = new Vector3(
                (float)tbTargetX.Value,
                (float)tbTargetY.Value,
                (float)tbTargetZ.Value
            );
        }
        else
        {
            _plugin.LockedTargetCoordinates.TryRemove(SelectedMonster, out _);
        }
    }

    private void btnCopyTargetCoords_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        Clipboard.SetText($@"{tbTargetX.Value},{tbTargetY.Value},{tbTargetZ.Value}");
    }

    private void btnPasteTargetCoords_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        var coords = Clipboard.GetText().Split(',');
        if (coords.Length != 3)
            return;

        if (float.TryParse(coords[0], out var x) &&
            float.TryParse(coords[1], out var y) &&
            float.TryParse(coords[2], out var z))
        {
            tbTargetX.Value = (decimal)x;
            tbTargetY.Value = (decimal)y;
            tbTargetZ.Value = (decimal)z;
        }
    }

    private void tbTargetX_ValueChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null || !cbLockTargetCoords.Checked)
            return;

        if (_plugin.LockedTargetCoordinates.TryGetValue(SelectedMonster, out var coords))
        {
            coords.X = (float)tbTargetX.Value;
            _plugin.LockedTargetCoordinates[SelectedMonster] = coords;
        }
        else
        {
            _plugin.LockedTargetCoordinates[SelectedMonster] = new Vector3(
                (float)tbTargetX.Value,
                (float)tbTargetY.Value,
                (float)tbTargetZ.Value
            );
        }
    }

    private void tbTargetY_ValueChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null || !cbLockTargetCoords.Checked)
            return;

        if (_plugin.LockedTargetCoordinates.TryGetValue(SelectedMonster, out var coords))
        {
            coords.Y = (float)tbTargetY.Value;
            _plugin.LockedTargetCoordinates[SelectedMonster] = coords;
        }
        else
        {
            _plugin.LockedTargetCoordinates[SelectedMonster] = new Vector3(
                (float)tbTargetX.Value,
                (float)tbTargetY.Value,
                (float)tbTargetZ.Value
            );
        }
    }

    private void tbTargetZ_ValueChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null || !cbLockTargetCoords.Checked)
            return;

        if (_plugin.LockedTargetCoordinates.TryGetValue(SelectedMonster, out var coords))
        {
            coords.Z = (float)tbTargetZ.Value;
            _plugin.LockedTargetCoordinates[SelectedMonster] = coords;
        }
        else
        {
            _plugin.LockedTargetCoordinates[SelectedMonster] = new Vector3(
                (float)tbTargetX.Value,
                (float)tbTargetY.Value,
                (float)tbTargetZ.Value
            );
        }
    }

    private void btnPlSetToCurrent_Click(object sender, EventArgs e)
    {
        var player = Player.MainPlayer;
        if (player is null)
            return;

        tbPlayerX.Value = (decimal)player.Position.X;
        tbPlayerY.Value = (decimal)player.Position.Y;
        tbPlayerZ.Value = (decimal)player.Position.Z;
    }

    private void btnApplySpeed_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        SelectedMonster.Speed = (float)tbMonsterSpeed.Value;
    }

    private unsafe void btnSpawnMonster_Click(object sender, EventArgs e)
    {
        if (_spawnMonsterFunc.NativePointer <= 0)
        {
            MessageBox.Show(@"Failed to find spawn monster function.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var sEnemy = SingletonManager.GetSingleton("sEnemy");
        if (sEnemy is null)
        {
            MessageBox.Show(@"Failed to find sEnemy singleton.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var monsterType = (MonsterType)cbSpawnMonster.SelectedIndex;
        var variant = (int)nudSpawnSubId.Value;

        _spawnMonsterFunc.Invoke(sEnemy.Instance, monsterType, variant, true);
    }

    private unsafe void btnSpawnGimmick_Click(object sender, EventArgs e)
    {
        if (_spawnGimmickFunc.NativePointer <= 0)
        {
            MessageBox.Show(@"Failed to find spawn gimmick function.", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _spawnGimmickFunc.Invoke(cbSpawnGimmick.SelectedIndex, 0, 0);
    }

    private void btnSetMonsterHealth_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        SelectedMonster.Health = (int)nudMonsterHealth.Value;
    }

    private void btnGetMonsterHealth_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        nudMonsterHealth.Value = (decimal)SelectedMonster.Health;
    }

    private void btnKillMonster_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        SelectedMonster.Health = 0;
    }

    private void btnDespawnMonster_Click(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        SelectedMonster.DespawnTime = 0f;
    }

    private void btnEnrageMonster_Click(object sender, EventArgs e)
    {
        SelectedMonster?.Enrage();
    }

    private void btnUnenrageMonster_Click(object sender, EventArgs e)
    {
        SelectedMonster?.Unenrage();
    }

    private void tbMonsterTransparency_ValueChanged(object sender, EventArgs e)
    {
        SelectedMonster?.SetTransparency((float)tbMonsterTransparency.Value / 100f);
    }

    private void tbMonsterSize_ValueChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        var scale = (float)tbMonsterSize.Value;
        SelectedMonster.Size = new Vector3(scale, scale, scale);
    }

    private void tbMonsterRot_ValueChanged(object sender, EventArgs e)
    {
        if (SelectedMonster is null)
            return;

        if (sender == tbMonsterRotX)
        {
            SelectedMonster.Rotation.X = (float)tbMonsterRotX.Value;
        }
        else if (sender == tbMonsterRotY)
        {
            SelectedMonster.Rotation.Y = (float)tbMonsterRotY.Value;
        }
        else if (sender == tbMonsterRotZ)
        {
            SelectedMonster.Rotation.Z = (float)tbMonsterRotZ.Value;
        }
        else if (sender == tbMonsterRotW)
        {
            SelectedMonster.Rotation.W = (float)tbMonsterRotW.Value;
        }
    }

    private void cbPlInvisible_CheckedChanged(object sender, EventArgs e)
    {
        if (cbPlInvisible.Checked)
        {
            _plTransparencyResetPatch.Enable();
        }
        else
        {
            _plTransparencyResetPatch.Disable();
        }

        Player.MainPlayer?.SetTransparency(cbPlInvisible.Checked ? 0f : 1f);
    }

    private void cbLockPlayerToEm_CheckedChanged(object sender, EventArgs e)
    {
        if (cbLockPlayerToEm.Checked)
        {
            _coordUpdatePatches[0].Enable();
            _coordUpdatePatches[1].Enable();
            _coordUpdatePatches[2].Enable();
        }
        else
        {
            _coordUpdatePatches[0].Disable();
            _coordUpdatePatches[1].Disable();
            _coordUpdatePatches[2].Disable();
        }
    }

    public void UpdateMonsterSpeed(float speed)
    {
        tbMonsterSpeed.Invoke(() =>
        {
            // Convert speed from cm/s to km/h
            var kmh = speed * 0.036f;
            lblMonsterSpeed.Text = $@"{kmh:000.00} km/h | {speed:.##} u/s";
        });
    }

    private void btnRecordSpeed_Click(object sender, EventArgs e)
    {
        var selectedMonster = SelectedMonster;
        if (selectedMonster is null)
            return;

        _isRecordingSpeed = !_isRecordingSpeed;
        if (_isRecordingSpeed)
        {
            _recordingStartPos = selectedMonster.Position;
            _recordingStartTime = DateTime.Now;
            btnRecordSpeed.Text = @"Stop Recording";
            tbSpeedRecordingResult.Text = "";
        }
        else
        {
            var deltaPos = selectedMonster.Position - _recordingStartPos;
            // Calculate speed in cm/s (1 unit = 1 cm)
            var speed = deltaPos.Length() / (float)(DateTime.Now - _recordingStartTime).TotalSeconds;
            var kmh = speed * 0.036f;
            tbSpeedRecordingResult.Text = $@"{kmh:000.00} km/h | {speed:.##} u/s";
            btnRecordSpeed.Text = @"Record Speed";
        }
    }

#pragma warning restore IDE1006 // Naming Styles
}
