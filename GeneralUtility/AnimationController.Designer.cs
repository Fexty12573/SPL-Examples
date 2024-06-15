namespace GeneralUtility;

partial class AnimationController
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        groupBox1 = new GroupBox();
        cbAnimationLocked = new CheckBox();
        label5 = new Label();
        label4 = new Label();
        label3 = new Label();
        label2 = new Label();
        tbStartSpeed = new NumericUpDown();
        tbStartFrame = new NumericUpDown();
        tbInterpFrames = new NumericUpDown();
        tbAnimId = new NumericUpDown();
        label1 = new Label();
        tbLmtId = new NumericUpDown();
        groupBox2 = new GroupBox();
        btnExecuteAnim = new Button();
        label6 = new Label();
        tbForceStartFrame = new NumericUpDown();
        label7 = new Label();
        tbForceLmtId = new NumericUpDown();
        label8 = new Label();
        label10 = new Label();
        label9 = new Label();
        tbForceAnimId = new NumericUpDown();
        tbForceStartSpeed = new NumericUpDown();
        tbForceInterpFrames = new NumericUpDown();
        contextMenuStrip1 = new ContextMenuStrip(components);
        menuStrip1 = new MenuStrip();
        helpToolStripMenuItem = new ToolStripMenuItem();
        AboutToolStripMenuItem = new ToolStripMenuItem();
        cbOverrideAll = new CheckBox();
        groupBox1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)tbStartSpeed).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbStartFrame).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbInterpFrames).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbAnimId).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbLmtId).BeginInit();
        groupBox2.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)tbForceStartFrame).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbForceLmtId).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbForceAnimId).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbForceStartSpeed).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbForceInterpFrames).BeginInit();
        menuStrip1.SuspendLayout();
        SuspendLayout();
        // 
        // groupBox1
        // 
        groupBox1.Controls.Add(cbOverrideAll);
        groupBox1.Controls.Add(cbAnimationLocked);
        groupBox1.Controls.Add(label5);
        groupBox1.Controls.Add(label4);
        groupBox1.Controls.Add(label3);
        groupBox1.Controls.Add(label2);
        groupBox1.Controls.Add(tbStartSpeed);
        groupBox1.Controls.Add(tbStartFrame);
        groupBox1.Controls.Add(tbInterpFrames);
        groupBox1.Controls.Add(tbAnimId);
        groupBox1.Controls.Add(label1);
        groupBox1.Controls.Add(tbLmtId);
        groupBox1.Location = new Point(12, 27);
        groupBox1.Name = "groupBox1";
        groupBox1.Size = new Size(472, 258);
        groupBox1.TabIndex = 0;
        groupBox1.TabStop = false;
        groupBox1.Text = "Animation Override";
        // 
        // cbAnimationLocked
        // 
        cbAnimationLocked.AutoSize = true;
        cbAnimationLocked.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        cbAnimationLocked.Location = new Point(16, 204);
        cbAnimationLocked.Name = "cbAnimationLocked";
        cbAnimationLocked.Size = new Size(154, 25);
        cbAnimationLocked.TabIndex = 10;
        cbAnimationLocked.Text = "Animation Locked";
        cbAnimationLocked.UseVisualStyleBackColor = true;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new Point(170, 153);
        label5.Name = "label5";
        label5.Size = new Size(66, 15);
        label5.TabIndex = 9;
        label5.Text = "Start Speed";
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new Point(170, 124);
        label4.Name = "label4";
        label4.Size = new Size(67, 15);
        label4.TabIndex = 8;
        label4.Text = "Start Frame";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(170, 95);
        label3.Name = "label3";
        label3.Size = new Size(116, 15);
        label3.TabIndex = 7;
        label3.Text = "Interpolation Frames";
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(170, 66);
        label2.Name = "label2";
        label2.Size = new Size(77, 15);
        label2.TabIndex = 6;
        label2.Text = "Animation ID";
        // 
        // tbStartSpeed
        // 
        tbStartSpeed.DecimalPlaces = 2;
        tbStartSpeed.Location = new Point(16, 150);
        tbStartSpeed.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        tbStartSpeed.Name = "tbStartSpeed";
        tbStartSpeed.Size = new Size(148, 23);
        tbStartSpeed.TabIndex = 5;
        tbStartSpeed.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // tbStartFrame
        // 
        tbStartFrame.DecimalPlaces = 2;
        tbStartFrame.Location = new Point(16, 121);
        tbStartFrame.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        tbStartFrame.Name = "tbStartFrame";
        tbStartFrame.Size = new Size(148, 23);
        tbStartFrame.TabIndex = 4;
        // 
        // tbInterpFrames
        // 
        tbInterpFrames.DecimalPlaces = 2;
        tbInterpFrames.Location = new Point(16, 92);
        tbInterpFrames.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        tbInterpFrames.Name = "tbInterpFrames";
        tbInterpFrames.Size = new Size(148, 23);
        tbInterpFrames.TabIndex = 3;
        // 
        // tbAnimId
        // 
        tbAnimId.Location = new Point(16, 63);
        tbAnimId.Maximum = new decimal(new int[] { 4095, 0, 0, 0 });
        tbAnimId.Name = "tbAnimId";
        tbAnimId.Size = new Size(148, 23);
        tbAnimId.TabIndex = 2;
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(170, 37);
        label1.Name = "label1";
        label1.Size = new Size(44, 15);
        label1.TabIndex = 1;
        label1.Text = "LMT ID";
        // 
        // tbLmtId
        // 
        tbLmtId.Location = new Point(16, 34);
        tbLmtId.Maximum = new decimal(new int[] { 15, 0, 0, 0 });
        tbLmtId.Name = "tbLmtId";
        tbLmtId.Size = new Size(148, 23);
        tbLmtId.TabIndex = 0;
        // 
        // groupBox2
        // 
        groupBox2.Controls.Add(btnExecuteAnim);
        groupBox2.Controls.Add(label6);
        groupBox2.Controls.Add(tbForceStartFrame);
        groupBox2.Controls.Add(label7);
        groupBox2.Controls.Add(tbForceLmtId);
        groupBox2.Controls.Add(label8);
        groupBox2.Controls.Add(label10);
        groupBox2.Controls.Add(label9);
        groupBox2.Controls.Add(tbForceAnimId);
        groupBox2.Controls.Add(tbForceStartSpeed);
        groupBox2.Controls.Add(tbForceInterpFrames);
        groupBox2.Location = new Point(12, 291);
        groupBox2.Name = "groupBox2";
        groupBox2.Size = new Size(472, 258);
        groupBox2.TabIndex = 1;
        groupBox2.TabStop = false;
        groupBox2.Text = "Animation Forcer";
        // 
        // btnExecuteAnim
        // 
        btnExecuteAnim.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        btnExecuteAnim.Location = new Point(16, 195);
        btnExecuteAnim.Name = "btnExecuteAnim";
        btnExecuteAnim.Size = new Size(148, 40);
        btnExecuteAnim.TabIndex = 21;
        btnExecuteAnim.Text = "Execute";
        btnExecuteAnim.UseVisualStyleBackColor = true;
        btnExecuteAnim.Click += btnExecuteAnim_Click;
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Location = new Point(170, 149);
        label6.Name = "label6";
        label6.Size = new Size(66, 15);
        label6.TabIndex = 20;
        label6.Text = "Start Speed";
        // 
        // tbForceStartFrame
        // 
        tbForceStartFrame.DecimalPlaces = 2;
        tbForceStartFrame.Location = new Point(16, 117);
        tbForceStartFrame.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        tbForceStartFrame.Name = "tbForceStartFrame";
        tbForceStartFrame.Size = new Size(148, 23);
        tbForceStartFrame.TabIndex = 15;
        // 
        // label7
        // 
        label7.AutoSize = true;
        label7.Location = new Point(170, 120);
        label7.Name = "label7";
        label7.Size = new Size(67, 15);
        label7.TabIndex = 19;
        label7.Text = "Start Frame";
        // 
        // tbForceLmtId
        // 
        tbForceLmtId.Location = new Point(16, 30);
        tbForceLmtId.Maximum = new decimal(new int[] { 15, 0, 0, 0 });
        tbForceLmtId.Name = "tbForceLmtId";
        tbForceLmtId.Size = new Size(148, 23);
        tbForceLmtId.TabIndex = 11;
        // 
        // label8
        // 
        label8.AutoSize = true;
        label8.Location = new Point(170, 91);
        label8.Name = "label8";
        label8.Size = new Size(116, 15);
        label8.TabIndex = 18;
        label8.Text = "Interpolation Frames";
        // 
        // label10
        // 
        label10.AutoSize = true;
        label10.Location = new Point(170, 33);
        label10.Name = "label10";
        label10.Size = new Size(44, 15);
        label10.TabIndex = 12;
        label10.Text = "LMT ID";
        // 
        // label9
        // 
        label9.AutoSize = true;
        label9.Location = new Point(170, 62);
        label9.Name = "label9";
        label9.Size = new Size(77, 15);
        label9.TabIndex = 17;
        label9.Text = "Animation ID";
        // 
        // tbForceAnimId
        // 
        tbForceAnimId.Location = new Point(16, 59);
        tbForceAnimId.Maximum = new decimal(new int[] { 4095, 0, 0, 0 });
        tbForceAnimId.Name = "tbForceAnimId";
        tbForceAnimId.Size = new Size(148, 23);
        tbForceAnimId.TabIndex = 13;
        // 
        // tbForceStartSpeed
        // 
        tbForceStartSpeed.DecimalPlaces = 2;
        tbForceStartSpeed.Location = new Point(16, 146);
        tbForceStartSpeed.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        tbForceStartSpeed.Name = "tbForceStartSpeed";
        tbForceStartSpeed.Size = new Size(148, 23);
        tbForceStartSpeed.TabIndex = 16;
        tbForceStartSpeed.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // tbForceInterpFrames
        // 
        tbForceInterpFrames.DecimalPlaces = 2;
        tbForceInterpFrames.Location = new Point(16, 88);
        tbForceInterpFrames.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        tbForceInterpFrames.Name = "tbForceInterpFrames";
        tbForceInterpFrames.Size = new Size(148, 23);
        tbForceInterpFrames.TabIndex = 14;
        // 
        // contextMenuStrip1
        // 
        contextMenuStrip1.Name = "contextMenuStrip1";
        contextMenuStrip1.Size = new Size(61, 4);
        // 
        // menuStrip1
        // 
        menuStrip1.Items.AddRange(new ToolStripItem[] { helpToolStripMenuItem });
        menuStrip1.Location = new Point(0, 0);
        menuStrip1.Name = "menuStrip1";
        menuStrip1.Size = new Size(496, 24);
        menuStrip1.TabIndex = 2;
        menuStrip1.Text = "menuStrip1";
        // 
        // helpToolStripMenuItem
        // 
        helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { AboutToolStripMenuItem });
        helpToolStripMenuItem.Name = "helpToolStripMenuItem";
        helpToolStripMenuItem.Size = new Size(44, 20);
        helpToolStripMenuItem.Text = "Help";
        // 
        // AboutToolStripMenuItem
        // 
        AboutToolStripMenuItem.Name = "AboutToolStripMenuItem";
        AboutToolStripMenuItem.Size = new Size(107, 22);
        AboutToolStripMenuItem.Text = "About";
        AboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
        // 
        // cbOverrideAll
        // 
        cbOverrideAll.AutoSize = true;
        cbOverrideAll.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
        cbOverrideAll.Location = new Point(210, 204);
        cbOverrideAll.Name = "cbOverrideAll";
        cbOverrideAll.Size = new Size(128, 25);
        cbOverrideAll.TabIndex = 11;
        cbOverrideAll.Text = "Hard Override";
        cbOverrideAll.UseVisualStyleBackColor = true;
        // 
        // AnimationController
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(496, 561);
        Controls.Add(menuStrip1);
        Controls.Add(groupBox2);
        Controls.Add(groupBox1);
        MainMenuStrip = menuStrip1;
        MaximizeBox = false;
        Name = "AnimationController";
        Text = "Animation Controller";
        groupBox1.ResumeLayout(false);
        groupBox1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)tbStartSpeed).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbStartFrame).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbInterpFrames).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbAnimId).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbLmtId).EndInit();
        groupBox2.ResumeLayout(false);
        groupBox2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)tbForceStartFrame).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbForceLmtId).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbForceAnimId).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbForceStartSpeed).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbForceInterpFrames).EndInit();
        menuStrip1.ResumeLayout(false);
        menuStrip1.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private GroupBox groupBox1;
    private NumericUpDown tbLmtId;
    private Label label1;
    private NumericUpDown tbAnimId;
    private Label label5;
    private Label label4;
    private Label label3;
    private Label label2;
    private NumericUpDown tbStartSpeed;
    private NumericUpDown tbStartFrame;
    private NumericUpDown tbInterpFrames;
    private CheckBox cbAnimationLocked;
    private GroupBox groupBox2;
    private Button btnExecuteAnim;
    private Label label6;
    private NumericUpDown tbForceStartFrame;
    private Label label7;
    private NumericUpDown tbForceLmtId;
    private Label label8;
    private Label label10;
    private Label label9;
    private NumericUpDown tbForceAnimId;
    private NumericUpDown tbForceStartSpeed;
    private NumericUpDown tbForceInterpFrames;
    private ContextMenuStrip contextMenuStrip1;
    private MenuStrip menuStrip1;
    private ToolStripMenuItem helpToolStripMenuItem;
    private ToolStripMenuItem AboutToolStripMenuItem;
    private CheckBox cbOverrideAll;
}