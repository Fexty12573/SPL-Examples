using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpPluginLoader.Core.Components;
using SharpPluginLoader.Core.Entities;

namespace GeneralUtility;
public partial class AnimationController : Form
{
    private readonly MainWindow _parent;
    private readonly Plugin _plugin;

    public bool LockAnimation => cbAnimationLocked.Checked;
    public bool HardOverride => cbOverrideAll.Checked;
    public AnimationId OverrideAnimationId => new((uint)tbLmtId.Value, (uint)tbAnimId.Value);
    public float OverrideInterpolationFrames => (float)tbInterpFrames.Value;
    public float OverrideStartFrame => (float)tbStartFrame.Value;
    public float OverrideStartSpeed => (float)tbStartSpeed.Value;

    private const string AboutMessage = """
                                        This tool allows you to override the current animation of a monster, or force a specific animation to play.

                                        The "Override Animation" section allows you to specify the LMT ID and Animation ID to play. Once you set the values, select the "Animation Locked" checkbox to start the override. Following that, all subsequent animations will be replaced with the specified one.

                                        The "Animation Forcer" section allows you to force a specific animation to play. Once you set the values, click the "Execute" button to force the animation.

                                        Enabling the "Hard Override" options will force-override even transitions and other "special" animations. This can cause some weird behavior, so use it with caution.
                                        
                                        Value Description:
                                        
                                        LMT ID: The LMT ID of the animation to play. The main window displays animation IDs in the format "lmt.anim". For example, the animation ID 4 of the LMT ID 1 is displayed as "1.4".

                                        Animation ID: The animation ID of the animation to play. This is the number after the dot in the "lmt.anim" format.

                                        Interpolation Frames: The number of frames to use for interpolating between the previous animation and the new one. This value is used to smoothly transition between animations. A value of 0 will cause the animation to instantly switch to the new one without a transition.

                                        Start Frame: The frame to start the new animation at. This value is used to start the new animation at a specific frame. To play the full animation, set this value to 0.

                                        Start Speed: The speed to start the new animation at. Note that the animation itself can modify its own speed, so this value may not apply to the entire animation. A value of 1 is the default speed.
                                        """;

    public AnimationController(MainWindow parent, Plugin plugin)
    {
        InitializeComponent();
        _plugin = plugin;
        _parent = parent;

        Icon = _parent.Icon;
    }

    private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        MessageBox.Show(AboutMessage, @"About", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

#pragma warning disable IDE1006 // Naming Styles

    private void btnExecuteAnim_Click(object sender, EventArgs e)
    {
        var monster = _parent.SelectedMonster;
        if (monster is null)
        {
            MessageBox.Show(@"No monster selected!", @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        var animId = new AnimationId((uint)tbForceLmtId.Value, (uint)tbForceAnimId.Value);
        var interpFrames = (uint)tbForceInterpFrames.Value;
        var startFrame = (uint)tbForceStartFrame.Value;
        var startSpeed = (uint)tbForceStartSpeed.Value;

        monster.AnimationLayer?.DoAnimationSafe(animId, interpFrames, startFrame, startSpeed);
    }

#pragma warning restore IDE1006 // Naming Styles
}
