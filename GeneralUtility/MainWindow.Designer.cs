﻿namespace GeneralUtility;

partial class MainWindow
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
        label1 = new Label();
        cbSelectedMonster = new ComboBox();
        label2 = new Label();
        tbLoadedMonsters = new TextBox();
        label4 = new Label();
        tbMonsterCoords = new TextBox();
        tbMonsterAction = new TextBox();
        label5 = new Label();
        tbMonsterAnim = new TextBox();
        cbCoordsLocked = new CheckBox();
        cbMonsterFrozen = new CheckBox();
        cbDisableSpeedReset = new CheckBox();
        groupBox1 = new GroupBox();
        btnSetToCurrent = new Button();
        btnPasteCoords = new Button();
        btnCopyCoords = new Button();
        btnTpPlToEm = new Button();
        btnTpEmToPl = new Button();
        btnSetXYZ = new Button();
        btnSetZ = new Button();
        btnSetY = new Button();
        btnSetX = new Button();
        label7 = new Label();
        label6 = new Label();
        label3 = new Label();
        tbMonsterZ = new NumericUpDown();
        tbMonsterY = new NumericUpDown();
        tbMonsterX = new NumericUpDown();
        groupBox2 = new GroupBox();
        groupBox3 = new GroupBox();
        tbPlayerCoords = new TextBox();
        label11 = new Label();
        btnPlSetToCurrent = new Button();
        btnPastePlCoords = new Button();
        btnCopyPlCoords = new Button();
        btnSetPlXYZ = new Button();
        tbPlayerZ = new NumericUpDown();
        btnSetPlZ = new Button();
        tbPlayerX = new NumericUpDown();
        btnSetPlY = new Button();
        tbPlayerY = new NumericUpDown();
        btnSetPlX = new Button();
        label10 = new Label();
        label8 = new Label();
        label9 = new Label();
        groupBox4 = new GroupBox();
        label19 = new Label();
        btnApplySpeed = new Button();
        tbMonsterSpeed = new NumericUpDown();
        btnPasteTargetCoords = new Button();
        btnCopyTargetCoords = new Button();
        cbLockTargetCoords = new CheckBox();
        label18 = new Label();
        label15 = new Label();
        btnSaveActionChainToCfg = new Button();
        label16 = new Label();
        btnCancelActionChain = new Button();
        label17 = new Label();
        btnLoadChainFromCfg = new Button();
        tbTargetZ = new NumericUpDown();
        tbTargetY = new NumericUpDown();
        label14 = new Label();
        tbTargetX = new NumericUpDown();
        tbActionChainLoopCount = new NumericUpDown();
        btnExecActionChain = new Button();
        label13 = new Label();
        tbActionChain = new TextBox();
        cbLockActions = new CheckBox();
        btnOverrideAction = new Button();
        btnForceAction = new Button();
        nudMonsterAction = new NumericUpDown();
        label12 = new Label();
        btnReloadConfig = new Button();
        groupBox5 = new GroupBox();
        btnSpawnGimmick = new Button();
        cbSpawnGimmick = new ComboBox();
        label22 = new Label();
        btnSpawnMonster = new Button();
        label21 = new Label();
        label20 = new Label();
        nudSpawnSubId = new NumericUpDown();
        cbSpawnMonster = new ComboBox();
        groupBox6 = new GroupBox();
        btnUnenrageMonster = new Button();
        btnEnrageMonster = new Button();
        btnDespawnMonster = new Button();
        btnKillMonster = new Button();
        btnGetMonsterHealth = new Button();
        btnSetMonsterHealth = new Button();
        label23 = new Label();
        nudMonsterHealth = new NumericUpDown();
        groupBox1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)tbMonsterZ).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbMonsterY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbMonsterX).BeginInit();
        groupBox2.SuspendLayout();
        groupBox3.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)tbPlayerZ).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbPlayerX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbPlayerY).BeginInit();
        groupBox4.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)tbMonsterSpeed).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbTargetZ).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbTargetY).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbTargetX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)tbActionChainLoopCount).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudMonsterAction).BeginInit();
        groupBox5.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudSpawnSubId).BeginInit();
        groupBox6.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudMonsterHealth).BeginInit();
        SuspendLayout();
        // 
        // label1
        // 
        label1.AutoSize = true;
        label1.Location = new Point(12, 9);
        label1.Name = "label1";
        label1.Size = new Size(86, 15);
        label1.TabIndex = 0;
        label1.Text = "Target Monster";
        // 
        // cbSelectedMonster
        // 
        cbSelectedMonster.DisplayMember = "Name";
        cbSelectedMonster.DropDownStyle = ComboBoxStyle.DropDownList;
        cbSelectedMonster.FormattingEnabled = true;
        cbSelectedMonster.Location = new Point(12, 27);
        cbSelectedMonster.Name = "cbSelectedMonster";
        cbSelectedMonster.Size = new Size(169, 23);
        cbSelectedMonster.TabIndex = 1;
        cbSelectedMonster.ValueMember = "Instance";
        cbSelectedMonster.SelectedIndexChanged += cbSelectedMonster_SelectedIndexChanged;
        // 
        // label2
        // 
        label2.AutoSize = true;
        label2.Location = new Point(524, 9);
        label2.Name = "label2";
        label2.Size = new Size(98, 15);
        label2.TabIndex = 2;
        label2.Text = "Loaded Monsters";
        // 
        // tbLoadedMonsters
        // 
        tbLoadedMonsters.Location = new Point(524, 27);
        tbLoadedMonsters.Multiline = true;
        tbLoadedMonsters.Name = "tbLoadedMonsters";
        tbLoadedMonsters.ReadOnly = true;
        tbLoadedMonsters.ScrollBars = ScrollBars.Vertical;
        tbLoadedMonsters.Size = new Size(324, 418);
        tbLoadedMonsters.TabIndex = 3;
        // 
        // label4
        // 
        label4.AutoSize = true;
        label4.Location = new Point(6, 21);
        label4.Name = "label4";
        label4.Size = new Size(45, 15);
        label4.TabIndex = 5;
        label4.Text = "Coords";
        // 
        // tbMonsterCoords
        // 
        tbMonsterCoords.Location = new Point(88, 18);
        tbMonsterCoords.Name = "tbMonsterCoords";
        tbMonsterCoords.ReadOnly = true;
        tbMonsterCoords.Size = new Size(308, 23);
        tbMonsterCoords.TabIndex = 6;
        // 
        // tbMonsterAction
        // 
        tbMonsterAction.Location = new Point(88, 47);
        tbMonsterAction.Name = "tbMonsterAction";
        tbMonsterAction.ReadOnly = true;
        tbMonsterAction.Size = new Size(148, 23);
        tbMonsterAction.TabIndex = 7;
        // 
        // label5
        // 
        label5.AutoSize = true;
        label5.Location = new Point(6, 50);
        label5.Name = "label5";
        label5.Size = new Size(76, 15);
        label5.TabIndex = 8;
        label5.Text = "Action/Anim";
        // 
        // tbMonsterAnim
        // 
        tbMonsterAnim.Location = new Point(242, 47);
        tbMonsterAnim.Name = "tbMonsterAnim";
        tbMonsterAnim.ReadOnly = true;
        tbMonsterAnim.Size = new Size(154, 23);
        tbMonsterAnim.TabIndex = 9;
        // 
        // cbCoordsLocked
        // 
        cbCoordsLocked.AutoSize = true;
        cbCoordsLocked.Location = new Point(402, 20);
        cbCoordsLocked.Name = "cbCoordsLocked";
        cbCoordsLocked.Size = new Size(51, 19);
        cbCoordsLocked.TabIndex = 10;
        cbCoordsLocked.Text = "Lock";
        cbCoordsLocked.UseVisualStyleBackColor = true;
        cbCoordsLocked.CheckedChanged += cbCoordsLocked_CheckedChanged;
        // 
        // cbMonsterFrozen
        // 
        cbMonsterFrozen.AutoSize = true;
        cbMonsterFrozen.Location = new Point(88, 76);
        cbMonsterFrozen.Name = "cbMonsterFrozen";
        cbMonsterFrozen.Size = new Size(108, 19);
        cbMonsterFrozen.TabIndex = 11;
        cbMonsterFrozen.Text = "Monster Frozen";
        cbMonsterFrozen.UseVisualStyleBackColor = true;
        cbMonsterFrozen.CheckedChanged += cbMonsterFrozen_CheckedChanged;
        // 
        // cbDisableSpeedReset
        // 
        cbDisableSpeedReset.AutoSize = true;
        cbDisableSpeedReset.Location = new Point(202, 76);
        cbDisableSpeedReset.Name = "cbDisableSpeedReset";
        cbDisableSpeedReset.Size = new Size(182, 19);
        cbDisableSpeedReset.TabIndex = 12;
        cbDisableSpeedReset.Text = "Speed Reset Disabled (Global)";
        cbDisableSpeedReset.UseVisualStyleBackColor = true;
        cbDisableSpeedReset.CheckedChanged += cbDisableSpeedReset_CheckedChanged;
        // 
        // groupBox1
        // 
        groupBox1.Controls.Add(btnSetToCurrent);
        groupBox1.Controls.Add(btnPasteCoords);
        groupBox1.Controls.Add(btnCopyCoords);
        groupBox1.Controls.Add(btnTpPlToEm);
        groupBox1.Controls.Add(btnTpEmToPl);
        groupBox1.Controls.Add(btnSetXYZ);
        groupBox1.Controls.Add(btnSetZ);
        groupBox1.Controls.Add(btnSetY);
        groupBox1.Controls.Add(btnSetX);
        groupBox1.Controls.Add(label7);
        groupBox1.Controls.Add(label6);
        groupBox1.Controls.Add(label3);
        groupBox1.Controls.Add(tbMonsterZ);
        groupBox1.Controls.Add(tbMonsterY);
        groupBox1.Controls.Add(tbMonsterX);
        groupBox1.Location = new Point(12, 162);
        groupBox1.Name = "groupBox1";
        groupBox1.Size = new Size(506, 116);
        groupBox1.TabIndex = 13;
        groupBox1.TabStop = false;
        groupBox1.Text = "Coordinate Options";
        // 
        // btnSetToCurrent
        // 
        btnSetToCurrent.Location = new Point(390, 24);
        btnSetToCurrent.Name = "btnSetToCurrent";
        btnSetToCurrent.Size = new Size(110, 23);
        btnSetToCurrent.TabIndex = 24;
        btnSetToCurrent.Text = "Set to Current";
        btnSetToCurrent.UseVisualStyleBackColor = true;
        btnSetToCurrent.Click += btnSetToCurrent_Click;
        // 
        // btnPasteCoords
        // 
        btnPasteCoords.Location = new Point(316, 80);
        btnPasteCoords.Name = "btnPasteCoords";
        btnPasteCoords.Size = new Size(68, 23);
        btnPasteCoords.TabIndex = 23;
        btnPasteCoords.Text = "Paste";
        btnPasteCoords.UseVisualStyleBackColor = true;
        btnPasteCoords.Click += btnPaseCoords_Click;
        // 
        // btnCopyCoords
        // 
        btnCopyCoords.Location = new Point(242, 80);
        btnCopyCoords.Name = "btnCopyCoords";
        btnCopyCoords.Size = new Size(68, 23);
        btnCopyCoords.TabIndex = 22;
        btnCopyCoords.Text = "Copy";
        btnCopyCoords.UseVisualStyleBackColor = true;
        btnCopyCoords.Click += btnCopyCoords_Click;
        // 
        // btnTpPlToEm
        // 
        btnTpPlToEm.Location = new Point(242, 51);
        btnTpPlToEm.Name = "btnTpPlToEm";
        btnTpPlToEm.Size = new Size(142, 23);
        btnTpPlToEm.TabIndex = 21;
        btnTpPlToEm.Text = "TP Player to Monster";
        btnTpPlToEm.UseVisualStyleBackColor = true;
        btnTpPlToEm.Click += btnTpPlToEm_Click;
        // 
        // btnTpEmToPl
        // 
        btnTpEmToPl.Location = new Point(242, 24);
        btnTpEmToPl.Name = "btnTpEmToPl";
        btnTpEmToPl.Size = new Size(142, 23);
        btnTpEmToPl.TabIndex = 20;
        btnTpEmToPl.Text = "TP Monster to Player";
        btnTpEmToPl.UseVisualStyleBackColor = true;
        btnTpEmToPl.Click += btnTpEmToPl_Click;
        // 
        // btnSetXYZ
        // 
        btnSetXYZ.Location = new Point(202, 22);
        btnSetXYZ.Name = "btnSetXYZ";
        btnSetXYZ.Size = new Size(34, 81);
        btnSetXYZ.TabIndex = 19;
        btnSetXYZ.Text = "Set All";
        btnSetXYZ.UseVisualStyleBackColor = true;
        btnSetXYZ.Click += btnSetXYZ_Click;
        // 
        // btnSetZ
        // 
        btnSetZ.Location = new Point(152, 80);
        btnSetZ.Name = "btnSetZ";
        btnSetZ.Size = new Size(44, 23);
        btnSetZ.TabIndex = 18;
        btnSetZ.Text = "Set";
        btnSetZ.UseVisualStyleBackColor = true;
        btnSetZ.Click += btnSetZ_Click;
        // 
        // btnSetY
        // 
        btnSetY.Location = new Point(152, 51);
        btnSetY.Name = "btnSetY";
        btnSetY.Size = new Size(44, 23);
        btnSetY.TabIndex = 17;
        btnSetY.Text = "Set";
        btnSetY.UseVisualStyleBackColor = true;
        btnSetY.Click += btnSetY_Click;
        // 
        // btnSetX
        // 
        btnSetX.Location = new Point(152, 22);
        btnSetX.Name = "btnSetX";
        btnSetX.Size = new Size(44, 23);
        btnSetX.TabIndex = 16;
        btnSetX.Text = "Set";
        btnSetX.UseVisualStyleBackColor = true;
        btnSetX.Click += btnSetX_Click;
        // 
        // label7
        // 
        label7.AutoSize = true;
        label7.Location = new Point(6, 82);
        label7.Name = "label7";
        label7.Size = new Size(14, 15);
        label7.TabIndex = 15;
        label7.Text = "Z";
        // 
        // label6
        // 
        label6.AutoSize = true;
        label6.Location = new Point(6, 53);
        label6.Name = "label6";
        label6.Size = new Size(14, 15);
        label6.TabIndex = 4;
        label6.Text = "Y";
        // 
        // label3
        // 
        label3.AutoSize = true;
        label3.Location = new Point(6, 24);
        label3.Name = "label3";
        label3.Size = new Size(14, 15);
        label3.TabIndex = 3;
        label3.Text = "X";
        // 
        // tbMonsterZ
        // 
        tbMonsterZ.DecimalPlaces = 3;
        tbMonsterZ.Location = new Point(26, 80);
        tbMonsterZ.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
        tbMonsterZ.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
        tbMonsterZ.Name = "tbMonsterZ";
        tbMonsterZ.Size = new Size(120, 23);
        tbMonsterZ.TabIndex = 2;
        // 
        // tbMonsterY
        // 
        tbMonsterY.DecimalPlaces = 3;
        tbMonsterY.Location = new Point(26, 51);
        tbMonsterY.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
        tbMonsterY.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
        tbMonsterY.Name = "tbMonsterY";
        tbMonsterY.Size = new Size(120, 23);
        tbMonsterY.TabIndex = 1;
        // 
        // tbMonsterX
        // 
        tbMonsterX.DecimalPlaces = 3;
        tbMonsterX.Location = new Point(26, 22);
        tbMonsterX.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
        tbMonsterX.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
        tbMonsterX.Name = "tbMonsterX";
        tbMonsterX.Size = new Size(120, 23);
        tbMonsterX.TabIndex = 0;
        // 
        // groupBox2
        // 
        groupBox2.Controls.Add(label4);
        groupBox2.Controls.Add(cbDisableSpeedReset);
        groupBox2.Controls.Add(label5);
        groupBox2.Controls.Add(cbMonsterFrozen);
        groupBox2.Controls.Add(tbMonsterCoords);
        groupBox2.Controls.Add(cbCoordsLocked);
        groupBox2.Controls.Add(tbMonsterAction);
        groupBox2.Controls.Add(tbMonsterAnim);
        groupBox2.Location = new Point(12, 56);
        groupBox2.Name = "groupBox2";
        groupBox2.Size = new Size(506, 100);
        groupBox2.TabIndex = 14;
        groupBox2.TabStop = false;
        groupBox2.Text = "General Info";
        // 
        // groupBox3
        // 
        groupBox3.Controls.Add(tbPlayerCoords);
        groupBox3.Controls.Add(label11);
        groupBox3.Controls.Add(btnPlSetToCurrent);
        groupBox3.Controls.Add(btnPastePlCoords);
        groupBox3.Controls.Add(btnCopyPlCoords);
        groupBox3.Controls.Add(btnSetPlXYZ);
        groupBox3.Controls.Add(tbPlayerZ);
        groupBox3.Controls.Add(btnSetPlZ);
        groupBox3.Controls.Add(tbPlayerX);
        groupBox3.Controls.Add(btnSetPlY);
        groupBox3.Controls.Add(tbPlayerY);
        groupBox3.Controls.Add(btnSetPlX);
        groupBox3.Controls.Add(label10);
        groupBox3.Controls.Add(label8);
        groupBox3.Controls.Add(label9);
        groupBox3.Location = new Point(524, 451);
        groupBox3.Name = "groupBox3";
        groupBox3.Size = new Size(324, 222);
        groupBox3.TabIndex = 15;
        groupBox3.TabStop = false;
        groupBox3.Text = "Player";
        // 
        // tbPlayerCoords
        // 
        tbPlayerCoords.Location = new Point(62, 112);
        tbPlayerCoords.Name = "tbPlayerCoords";
        tbPlayerCoords.ReadOnly = true;
        tbPlayerCoords.Size = new Size(256, 23);
        tbPlayerCoords.TabIndex = 13;
        // 
        // label11
        // 
        label11.AutoSize = true;
        label11.Location = new Point(11, 115);
        label11.Name = "label11";
        label11.Size = new Size(45, 15);
        label11.TabIndex = 37;
        label11.Text = "Coords";
        // 
        // btnPlSetToCurrent
        // 
        btnPlSetToCurrent.Location = new Point(247, 79);
        btnPlSetToCurrent.Name = "btnPlSetToCurrent";
        btnPlSetToCurrent.Size = new Size(71, 23);
        btnPlSetToCurrent.TabIndex = 36;
        btnPlSetToCurrent.Text = "Current";
        btnPlSetToCurrent.UseVisualStyleBackColor = true;
        btnPlSetToCurrent.Click += btnPlSetToCurrent_Click;
        // 
        // btnPastePlCoords
        // 
        btnPastePlCoords.Location = new Point(247, 50);
        btnPastePlCoords.Name = "btnPastePlCoords";
        btnPastePlCoords.Size = new Size(71, 23);
        btnPastePlCoords.TabIndex = 35;
        btnPastePlCoords.Text = "Paste";
        btnPastePlCoords.UseVisualStyleBackColor = true;
        btnPastePlCoords.Click += btnPastePlCoords_Click;
        // 
        // btnCopyPlCoords
        // 
        btnCopyPlCoords.Location = new Point(247, 21);
        btnCopyPlCoords.Name = "btnCopyPlCoords";
        btnCopyPlCoords.Size = new Size(71, 23);
        btnCopyPlCoords.TabIndex = 34;
        btnCopyPlCoords.Text = "Copy";
        btnCopyPlCoords.UseVisualStyleBackColor = true;
        btnCopyPlCoords.Click += btnCopyPlCoords_Click;
        // 
        // btnSetPlXYZ
        // 
        btnSetPlXYZ.Location = new Point(207, 21);
        btnSetPlXYZ.Name = "btnSetPlXYZ";
        btnSetPlXYZ.Size = new Size(34, 81);
        btnSetPlXYZ.TabIndex = 33;
        btnSetPlXYZ.Text = "Set All";
        btnSetPlXYZ.UseVisualStyleBackColor = true;
        btnSetPlXYZ.Click += btnSetPlXYZ_Click;
        // 
        // tbPlayerZ
        // 
        tbPlayerZ.DecimalPlaces = 3;
        tbPlayerZ.Location = new Point(31, 79);
        tbPlayerZ.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
        tbPlayerZ.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
        tbPlayerZ.Name = "tbPlayerZ";
        tbPlayerZ.Size = new Size(120, 23);
        tbPlayerZ.TabIndex = 26;
        // 
        // btnSetPlZ
        // 
        btnSetPlZ.Location = new Point(157, 79);
        btnSetPlZ.Name = "btnSetPlZ";
        btnSetPlZ.Size = new Size(44, 23);
        btnSetPlZ.TabIndex = 32;
        btnSetPlZ.Text = "Set";
        btnSetPlZ.UseVisualStyleBackColor = true;
        btnSetPlZ.Click += btnSetPlZ_Click;
        // 
        // tbPlayerX
        // 
        tbPlayerX.DecimalPlaces = 3;
        tbPlayerX.Location = new Point(31, 21);
        tbPlayerX.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
        tbPlayerX.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
        tbPlayerX.Name = "tbPlayerX";
        tbPlayerX.Size = new Size(120, 23);
        tbPlayerX.TabIndex = 24;
        // 
        // btnSetPlY
        // 
        btnSetPlY.Location = new Point(157, 50);
        btnSetPlY.Name = "btnSetPlY";
        btnSetPlY.Size = new Size(44, 23);
        btnSetPlY.TabIndex = 31;
        btnSetPlY.Text = "Set";
        btnSetPlY.UseVisualStyleBackColor = true;
        btnSetPlY.Click += btnSetPlY_Click;
        // 
        // tbPlayerY
        // 
        tbPlayerY.DecimalPlaces = 3;
        tbPlayerY.Location = new Point(31, 50);
        tbPlayerY.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
        tbPlayerY.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
        tbPlayerY.Name = "tbPlayerY";
        tbPlayerY.Size = new Size(120, 23);
        tbPlayerY.TabIndex = 25;
        // 
        // btnSetPlX
        // 
        btnSetPlX.Location = new Point(157, 21);
        btnSetPlX.Name = "btnSetPlX";
        btnSetPlX.Size = new Size(44, 23);
        btnSetPlX.TabIndex = 30;
        btnSetPlX.Text = "Set";
        btnSetPlX.UseVisualStyleBackColor = true;
        btnSetPlX.Click += btnSetPlX_Click;
        // 
        // label10
        // 
        label10.AutoSize = true;
        label10.Location = new Point(11, 23);
        label10.Name = "label10";
        label10.Size = new Size(14, 15);
        label10.TabIndex = 27;
        label10.Text = "X";
        // 
        // label8
        // 
        label8.AutoSize = true;
        label8.Location = new Point(11, 81);
        label8.Name = "label8";
        label8.Size = new Size(14, 15);
        label8.TabIndex = 29;
        label8.Text = "Z";
        // 
        // label9
        // 
        label9.AutoSize = true;
        label9.Location = new Point(11, 52);
        label9.Name = "label9";
        label9.Size = new Size(14, 15);
        label9.TabIndex = 28;
        label9.Text = "Y";
        // 
        // groupBox4
        // 
        groupBox4.Controls.Add(label19);
        groupBox4.Controls.Add(btnApplySpeed);
        groupBox4.Controls.Add(tbMonsterSpeed);
        groupBox4.Controls.Add(btnPasteTargetCoords);
        groupBox4.Controls.Add(btnCopyTargetCoords);
        groupBox4.Controls.Add(cbLockTargetCoords);
        groupBox4.Controls.Add(label18);
        groupBox4.Controls.Add(label15);
        groupBox4.Controls.Add(btnSaveActionChainToCfg);
        groupBox4.Controls.Add(label16);
        groupBox4.Controls.Add(btnCancelActionChain);
        groupBox4.Controls.Add(label17);
        groupBox4.Controls.Add(btnLoadChainFromCfg);
        groupBox4.Controls.Add(tbTargetZ);
        groupBox4.Controls.Add(tbTargetY);
        groupBox4.Controls.Add(label14);
        groupBox4.Controls.Add(tbTargetX);
        groupBox4.Controls.Add(tbActionChainLoopCount);
        groupBox4.Controls.Add(btnExecActionChain);
        groupBox4.Controls.Add(label13);
        groupBox4.Controls.Add(tbActionChain);
        groupBox4.Controls.Add(cbLockActions);
        groupBox4.Controls.Add(btnOverrideAction);
        groupBox4.Controls.Add(btnForceAction);
        groupBox4.Controls.Add(nudMonsterAction);
        groupBox4.Controls.Add(label12);
        groupBox4.Location = new Point(12, 284);
        groupBox4.Name = "groupBox4";
        groupBox4.Size = new Size(506, 234);
        groupBox4.TabIndex = 16;
        groupBox4.TabStop = false;
        groupBox4.Text = "Actions";
        // 
        // label19
        // 
        label19.AutoSize = true;
        label19.Location = new Point(308, 173);
        label19.Name = "label19";
        label19.Size = new Size(39, 15);
        label19.TabIndex = 35;
        label19.Text = "Speed";
        // 
        // btnApplySpeed
        // 
        btnApplySpeed.Location = new Point(434, 191);
        btnApplySpeed.Name = "btnApplySpeed";
        btnApplySpeed.Size = new Size(66, 23);
        btnApplySpeed.TabIndex = 34;
        btnApplySpeed.Text = "Apply";
        btnApplySpeed.UseVisualStyleBackColor = true;
        btnApplySpeed.Click += btnApplySpeed_Click;
        // 
        // tbMonsterSpeed
        // 
        tbMonsterSpeed.DecimalPlaces = 2;
        tbMonsterSpeed.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
        tbMonsterSpeed.Location = new Point(308, 191);
        tbMonsterSpeed.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
        tbMonsterSpeed.Name = "tbMonsterSpeed";
        tbMonsterSpeed.Size = new Size(120, 23);
        tbMonsterSpeed.TabIndex = 33;
        tbMonsterSpeed.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // btnPasteTargetCoords
        // 
        btnPasteTargetCoords.Location = new Point(152, 189);
        btnPasteTargetCoords.Name = "btnPasteTargetCoords";
        btnPasteTargetCoords.Size = new Size(68, 23);
        btnPasteTargetCoords.TabIndex = 25;
        btnPasteTargetCoords.Text = "Paste";
        btnPasteTargetCoords.UseVisualStyleBackColor = true;
        btnPasteTargetCoords.Click += btnPasteTargetCoords_Click;
        // 
        // btnCopyTargetCoords
        // 
        btnCopyTargetCoords.Location = new Point(152, 160);
        btnCopyTargetCoords.Name = "btnCopyTargetCoords";
        btnCopyTargetCoords.Size = new Size(68, 23);
        btnCopyTargetCoords.TabIndex = 25;
        btnCopyTargetCoords.Text = "Copy";
        btnCopyTargetCoords.UseVisualStyleBackColor = true;
        btnCopyTargetCoords.Click += btnCopyTargetCoords_Click;
        // 
        // cbLockTargetCoords
        // 
        cbLockTargetCoords.AutoSize = true;
        cbLockTargetCoords.Location = new Point(152, 132);
        cbLockTargetCoords.Name = "cbLockTargetCoords";
        cbLockTargetCoords.Size = new Size(51, 19);
        cbLockTargetCoords.TabIndex = 32;
        cbLockTargetCoords.Text = "Lock";
        cbLockTargetCoords.UseVisualStyleBackColor = true;
        cbLockTargetCoords.CheckedChanged += cbLockTargetCoords_CheckedChanged;
        // 
        // label18
        // 
        label18.AutoSize = true;
        label18.Location = new Point(6, 108);
        label18.Name = "label18";
        label18.Size = new Size(106, 15);
        label18.TabIndex = 31;
        label18.Text = "Target Coordinates";
        // 
        // label15
        // 
        label15.AutoSize = true;
        label15.Location = new Point(6, 191);
        label15.Name = "label15";
        label15.Size = new Size(14, 15);
        label15.TabIndex = 30;
        label15.Text = "Z";
        // 
        // btnSaveActionChainToCfg
        // 
        btnSaveActionChainToCfg.Location = new Point(308, 104);
        btnSaveActionChainToCfg.Name = "btnSaveActionChainToCfg";
        btnSaveActionChainToCfg.Size = new Size(142, 23);
        btnSaveActionChainToCfg.TabIndex = 12;
        btnSaveActionChainToCfg.Text = "Save to Config";
        btnSaveActionChainToCfg.UseVisualStyleBackColor = true;
        btnSaveActionChainToCfg.Click += btnSaveActionChainToCfg_Click;
        // 
        // label16
        // 
        label16.AutoSize = true;
        label16.Location = new Point(6, 162);
        label16.Name = "label16";
        label16.Size = new Size(14, 15);
        label16.TabIndex = 29;
        label16.Text = "Y";
        // 
        // btnCancelActionChain
        // 
        btnCancelActionChain.Location = new Point(365, 17);
        btnCancelActionChain.Name = "btnCancelActionChain";
        btnCancelActionChain.Size = new Size(135, 23);
        btnCancelActionChain.TabIndex = 11;
        btnCancelActionChain.Text = "Cancel Active Chain";
        btnCancelActionChain.UseVisualStyleBackColor = true;
        btnCancelActionChain.Click += btnCancelActionChain_Click;
        // 
        // label17
        // 
        label17.AutoSize = true;
        label17.Location = new Point(6, 133);
        label17.Name = "label17";
        label17.Size = new Size(14, 15);
        label17.TabIndex = 28;
        label17.Text = "X";
        // 
        // btnLoadChainFromCfg
        // 
        btnLoadChainFromCfg.Location = new Point(308, 75);
        btnLoadChainFromCfg.Name = "btnLoadChainFromCfg";
        btnLoadChainFromCfg.Size = new Size(142, 23);
        btnLoadChainFromCfg.TabIndex = 10;
        btnLoadChainFromCfg.Text = "Load from Config";
        btnLoadChainFromCfg.UseVisualStyleBackColor = true;
        btnLoadChainFromCfg.Click += btnLoadChainFromCfg_Click;
        // 
        // tbTargetZ
        // 
        tbTargetZ.DecimalPlaces = 3;
        tbTargetZ.Location = new Point(26, 189);
        tbTargetZ.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
        tbTargetZ.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
        tbTargetZ.Name = "tbTargetZ";
        tbTargetZ.Size = new Size(120, 23);
        tbTargetZ.TabIndex = 27;
        tbTargetZ.ValueChanged += tbTargetZ_ValueChanged;
        // 
        // tbTargetY
        // 
        tbTargetY.DecimalPlaces = 3;
        tbTargetY.Location = new Point(26, 160);
        tbTargetY.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
        tbTargetY.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
        tbTargetY.Name = "tbTargetY";
        tbTargetY.Size = new Size(120, 23);
        tbTargetY.TabIndex = 26;
        tbTargetY.ValueChanged += tbTargetY_ValueChanged;
        // 
        // label14
        // 
        label14.AutoSize = true;
        label14.Location = new Point(456, 50);
        label14.Name = "label14";
        label14.Size = new Size(44, 15);
        label14.TabIndex = 9;
        label14.Text = "Loop #";
        // 
        // tbTargetX
        // 
        tbTargetX.DecimalPlaces = 3;
        tbTargetX.Location = new Point(26, 131);
        tbTargetX.Maximum = new decimal(new int[] { 999999999, 0, 0, 0 });
        tbTargetX.Minimum = new decimal(new int[] { 999999999, 0, 0, int.MinValue });
        tbTargetX.Name = "tbTargetX";
        tbTargetX.Size = new Size(120, 23);
        tbTargetX.TabIndex = 25;
        tbTargetX.ValueChanged += tbTargetX_ValueChanged;
        // 
        // tbActionChainLoopCount
        // 
        tbActionChainLoopCount.Location = new Point(375, 46);
        tbActionChainLoopCount.Name = "tbActionChainLoopCount";
        tbActionChainLoopCount.Size = new Size(75, 23);
        tbActionChainLoopCount.TabIndex = 8;
        tbActionChainLoopCount.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // btnExecActionChain
        // 
        btnExecActionChain.Location = new Point(308, 46);
        btnExecActionChain.Name = "btnExecActionChain";
        btnExecActionChain.Size = new Size(61, 23);
        btnExecActionChain.TabIndex = 7;
        btnExecActionChain.Text = "Execute";
        btnExecActionChain.UseVisualStyleBackColor = true;
        btnExecActionChain.Click += btnExecActionChain_Click;
        // 
        // label13
        // 
        label13.AutoSize = true;
        label13.Location = new Point(6, 49);
        label13.Name = "label13";
        label13.Size = new Size(38, 15);
        label13.TabIndex = 6;
        label13.Text = "Chain";
        // 
        // tbActionChain
        // 
        tbActionChain.Location = new Point(50, 46);
        tbActionChain.Multiline = true;
        tbActionChain.Name = "tbActionChain";
        tbActionChain.Size = new Size(252, 52);
        tbActionChain.TabIndex = 5;
        // 
        // cbLockActions
        // 
        cbLockActions.AutoSize = true;
        cbLockActions.Location = new Point(308, 20);
        cbLockActions.Name = "cbLockActions";
        cbLockActions.Size = new Size(51, 19);
        cbLockActions.TabIndex = 4;
        cbLockActions.Text = "Lock";
        cbLockActions.UseVisualStyleBackColor = true;
        cbLockActions.CheckedChanged += cbLockActions_CheckedChanged;
        // 
        // btnOverrideAction
        // 
        btnOverrideAction.Location = new Point(242, 17);
        btnOverrideAction.Name = "btnOverrideAction";
        btnOverrideAction.Size = new Size(60, 23);
        btnOverrideAction.TabIndex = 3;
        btnOverrideAction.Text = "Do Next";
        btnOverrideAction.UseVisualStyleBackColor = true;
        btnOverrideAction.Click += btnOverrideAction_Click;
        // 
        // btnForceAction
        // 
        btnForceAction.Location = new Point(176, 17);
        btnForceAction.Name = "btnForceAction";
        btnForceAction.Size = new Size(60, 23);
        btnForceAction.TabIndex = 2;
        btnForceAction.Text = "Force";
        btnForceAction.UseVisualStyleBackColor = true;
        btnForceAction.Click += btnForceAction_Click;
        // 
        // nudMonsterAction
        // 
        nudMonsterAction.Location = new Point(50, 17);
        nudMonsterAction.Name = "nudMonsterAction";
        nudMonsterAction.Size = new Size(120, 23);
        nudMonsterAction.TabIndex = 1;
        // 
        // label12
        // 
        label12.AutoSize = true;
        label12.Location = new Point(6, 20);
        label12.Name = "label12";
        label12.Size = new Size(18, 15);
        label12.TabIndex = 0;
        label12.Text = "ID";
        // 
        // btnReloadConfig
        // 
        btnReloadConfig.Location = new Point(422, 27);
        btnReloadConfig.Name = "btnReloadConfig";
        btnReloadConfig.Size = new Size(96, 23);
        btnReloadConfig.TabIndex = 17;
        btnReloadConfig.Text = "Reload Config";
        btnReloadConfig.UseVisualStyleBackColor = true;
        // 
        // groupBox5
        // 
        groupBox5.Controls.Add(btnSpawnGimmick);
        groupBox5.Controls.Add(cbSpawnGimmick);
        groupBox5.Controls.Add(label22);
        groupBox5.Controls.Add(btnSpawnMonster);
        groupBox5.Controls.Add(label21);
        groupBox5.Controls.Add(label20);
        groupBox5.Controls.Add(nudSpawnSubId);
        groupBox5.Controls.Add(cbSpawnMonster);
        groupBox5.Location = new Point(12, 524);
        groupBox5.Name = "groupBox5";
        groupBox5.Size = new Size(264, 149);
        groupBox5.TabIndex = 18;
        groupBox5.TabStop = false;
        groupBox5.Text = "Spawner";
        // 
        // btnSpawnGimmick
        // 
        btnSpawnGimmick.Location = new Point(177, 101);
        btnSpawnGimmick.Name = "btnSpawnGimmick";
        btnSpawnGimmick.Size = new Size(75, 23);
        btnSpawnGimmick.TabIndex = 7;
        btnSpawnGimmick.Text = "Spawn";
        btnSpawnGimmick.UseVisualStyleBackColor = true;
        btnSpawnGimmick.Click += btnSpawnGimmick_Click;
        // 
        // cbSpawnGimmick
        // 
        cbSpawnGimmick.FormattingEnabled = true;
        cbSpawnGimmick.Location = new Point(50, 101);
        cbSpawnGimmick.Name = "cbSpawnGimmick";
        cbSpawnGimmick.Size = new Size(121, 23);
        cbSpawnGimmick.TabIndex = 6;
        // 
        // label22
        // 
        label22.AutoSize = true;
        label22.Location = new Point(50, 83);
        label22.Name = "label22";
        label22.Size = new Size(55, 15);
        label22.TabIndex = 5;
        label22.Text = "Gimmick";
        // 
        // btnSpawnMonster
        // 
        btnSpawnMonster.Location = new Point(176, 22);
        btnSpawnMonster.Name = "btnSpawnMonster";
        btnSpawnMonster.Size = new Size(75, 52);
        btnSpawnMonster.TabIndex = 4;
        btnSpawnMonster.Text = "Spawn";
        btnSpawnMonster.UseVisualStyleBackColor = true;
        btnSpawnMonster.Click += btnSpawnMonster_Click;
        // 
        // label21
        // 
        label21.AutoSize = true;
        label21.Location = new Point(6, 53);
        label21.Name = "label21";
        label21.Size = new Size(27, 15);
        label21.TabIndex = 3;
        label21.Text = "Sub";
        // 
        // label20
        // 
        label20.AutoSize = true;
        label20.Location = new Point(6, 25);
        label20.Name = "label20";
        label20.Size = new Size(31, 15);
        label20.TabIndex = 2;
        label20.Text = "Type";
        // 
        // nudSpawnSubId
        // 
        nudSpawnSubId.Location = new Point(50, 51);
        nudSpawnSubId.Maximum = new decimal(new int[] { 99, 0, 0, 0 });
        nudSpawnSubId.Name = "nudSpawnSubId";
        nudSpawnSubId.Size = new Size(121, 23);
        nudSpawnSubId.TabIndex = 1;
        // 
        // cbSpawnMonster
        // 
        cbSpawnMonster.FormattingEnabled = true;
        cbSpawnMonster.Location = new Point(50, 22);
        cbSpawnMonster.Name = "cbSpawnMonster";
        cbSpawnMonster.Size = new Size(121, 23);
        cbSpawnMonster.TabIndex = 0;
        // 
        // groupBox6
        // 
        groupBox6.Controls.Add(btnUnenrageMonster);
        groupBox6.Controls.Add(btnEnrageMonster);
        groupBox6.Controls.Add(btnDespawnMonster);
        groupBox6.Controls.Add(btnKillMonster);
        groupBox6.Controls.Add(btnGetMonsterHealth);
        groupBox6.Controls.Add(btnSetMonsterHealth);
        groupBox6.Controls.Add(label23);
        groupBox6.Controls.Add(nudMonsterHealth);
        groupBox6.Location = new Point(282, 524);
        groupBox6.Name = "groupBox6";
        groupBox6.Size = new Size(236, 149);
        groupBox6.TabIndex = 19;
        groupBox6.TabStop = false;
        groupBox6.Text = "Miscellaneous";
        // 
        // btnUnenrageMonster
        // 
        btnUnenrageMonster.Location = new Point(129, 80);
        btnUnenrageMonster.Name = "btnUnenrageMonster";
        btnUnenrageMonster.Size = new Size(101, 23);
        btnUnenrageMonster.TabIndex = 7;
        btnUnenrageMonster.Text = "Unenrage";
        btnUnenrageMonster.UseVisualStyleBackColor = true;
        btnUnenrageMonster.Click += btnUnenrageMonster_Click;
        // 
        // btnEnrageMonster
        // 
        btnEnrageMonster.Location = new Point(35, 80);
        btnEnrageMonster.Name = "btnEnrageMonster";
        btnEnrageMonster.Size = new Size(88, 23);
        btnEnrageMonster.TabIndex = 6;
        btnEnrageMonster.Text = "Enrage";
        btnEnrageMonster.UseVisualStyleBackColor = true;
        btnEnrageMonster.Click += btnEnrageMonster_Click;
        // 
        // btnDespawnMonster
        // 
        btnDespawnMonster.Location = new Point(129, 51);
        btnDespawnMonster.Name = "btnDespawnMonster";
        btnDespawnMonster.Size = new Size(101, 23);
        btnDespawnMonster.TabIndex = 5;
        btnDespawnMonster.Text = "Despawn";
        btnDespawnMonster.UseVisualStyleBackColor = true;
        btnDespawnMonster.Click += btnDespawnMonster_Click;
        // 
        // btnKillMonster
        // 
        btnKillMonster.Location = new Point(35, 51);
        btnKillMonster.Name = "btnKillMonster";
        btnKillMonster.Size = new Size(88, 23);
        btnKillMonster.TabIndex = 4;
        btnKillMonster.Text = "Kill";
        btnKillMonster.UseVisualStyleBackColor = true;
        btnKillMonster.Click += btnKillMonster_Click;
        // 
        // btnGetMonsterHealth
        // 
        btnGetMonsterHealth.Location = new Point(182, 22);
        btnGetMonsterHealth.Name = "btnGetMonsterHealth";
        btnGetMonsterHealth.Size = new Size(48, 23);
        btnGetMonsterHealth.TabIndex = 3;
        btnGetMonsterHealth.Text = "Get";
        btnGetMonsterHealth.UseVisualStyleBackColor = true;
        btnGetMonsterHealth.Click += btnGetMonsterHealth_Click;
        // 
        // btnSetMonsterHealth
        // 
        btnSetMonsterHealth.Location = new Point(129, 22);
        btnSetMonsterHealth.Name = "btnSetMonsterHealth";
        btnSetMonsterHealth.Size = new Size(48, 23);
        btnSetMonsterHealth.TabIndex = 2;
        btnSetMonsterHealth.Text = "Set";
        btnSetMonsterHealth.UseVisualStyleBackColor = true;
        btnSetMonsterHealth.Click += btnSetMonsterHealth_Click;
        // 
        // label23
        // 
        label23.AutoSize = true;
        label23.Location = new Point(6, 25);
        label23.Name = "label23";
        label23.Size = new Size(23, 15);
        label23.TabIndex = 1;
        label23.Text = "HP";
        // 
        // nudMonsterHealth
        // 
        nudMonsterHealth.Location = new Point(35, 22);
        nudMonsterHealth.Maximum = new decimal(new int[] { -727379969, 232, 0, 0 });
        nudMonsterHealth.Name = "nudMonsterHealth";
        nudMonsterHealth.Size = new Size(88, 23);
        nudMonsterHealth.TabIndex = 0;
        // 
        // MainWindow
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(860, 685);
        Controls.Add(groupBox6);
        Controls.Add(groupBox5);
        Controls.Add(btnReloadConfig);
        Controls.Add(groupBox4);
        Controls.Add(groupBox3);
        Controls.Add(groupBox1);
        Controls.Add(tbLoadedMonsters);
        Controls.Add(label2);
        Controls.Add(cbSelectedMonster);
        Controls.Add(label1);
        Controls.Add(groupBox2);
        Name = "MainWindow";
        Text = "MainWindow";
        groupBox1.ResumeLayout(false);
        groupBox1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)tbMonsterZ).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbMonsterY).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbMonsterX).EndInit();
        groupBox2.ResumeLayout(false);
        groupBox2.PerformLayout();
        groupBox3.ResumeLayout(false);
        groupBox3.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)tbPlayerZ).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbPlayerX).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbPlayerY).EndInit();
        groupBox4.ResumeLayout(false);
        groupBox4.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)tbMonsterSpeed).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbTargetZ).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbTargetY).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbTargetX).EndInit();
        ((System.ComponentModel.ISupportInitialize)tbActionChainLoopCount).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudMonsterAction).EndInit();
        groupBox5.ResumeLayout(false);
        groupBox5.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudSpawnSubId).EndInit();
        groupBox6.ResumeLayout(false);
        groupBox6.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)nudMonsterHealth).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private Label label1;
    private Label label2;
    private TextBox tbLoadedMonsters;
    private Label label4;
    private Label label5;
    internal TextBox tbMonsterCoords;
    internal TextBox tbMonsterAction;
    internal ComboBox cbSelectedMonster;
    internal TextBox tbMonsterAnim;
    private CheckBox cbCoordsLocked;
    private CheckBox cbMonsterFrozen;
    private CheckBox cbDisableSpeedReset;
    private GroupBox groupBox1;
    private Button btnSetZ;
    private Button btnSetY;
    private Button btnSetX;
    private Label label7;
    private Label label6;
    private Label label3;
    private NumericUpDown tbMonsterZ;
    private NumericUpDown tbMonsterY;
    private NumericUpDown tbMonsterX;
    private GroupBox groupBox2;
    private Button btnSetXYZ;
    private Button btnTpPlToEm;
    private Button btnTpEmToPl;
    private Button btnPasteCoords;
    private Button btnCopyCoords;
    private GroupBox groupBox3;
    private Button btnSetPlXYZ;
    private NumericUpDown tbPlayerZ;
    private Button btnSetPlZ;
    private NumericUpDown tbPlayerX;
    private Button btnSetPlY;
    private NumericUpDown tbPlayerY;
    private Button btnSetPlX;
    private Label label10;
    private Label label8;
    private Label label9;
    private Button btnSetToCurrent;
    private Button btnPastePlCoords;
    private Button btnCopyPlCoords;
    internal TextBox tbPlayerCoords;
    private Label label11;
    private Button btnPlSetToCurrent;
    private GroupBox groupBox4;
    private Button btnOverrideAction;
    private Button btnForceAction;
    private NumericUpDown nudMonsterAction;
    private Label label12;
    private CheckBox cbLockActions;
    private Label label13;
    private TextBox tbActionChain;
    private Label label14;
    private NumericUpDown tbActionChainLoopCount;
    private Button btnExecActionChain;
    private Button btnLoadChainFromCfg;
    private Button btnReloadConfig;
    private Button btnCancelActionChain;
    private Button btnSaveActionChainToCfg;
    private Label label18;
    private Label label15;
    private Label label16;
    private Label label17;
    private NumericUpDown tbTargetZ;
    private NumericUpDown tbTargetY;
    private NumericUpDown tbTargetX;
    private Button btnPasteTargetCoords;
    private Button btnCopyTargetCoords;
    private CheckBox cbLockTargetCoords;
    private Label label19;
    private Button btnApplySpeed;
    private NumericUpDown tbMonsterSpeed;
    private GroupBox groupBox5;
    private ComboBox cbSpawnMonster;
    private Button btnSpawnMonster;
    private Label label21;
    private Label label20;
    private NumericUpDown nudSpawnSubId;
    private Label label22;
    private ComboBox cbSpawnGimmick;
    private Button btnSpawnGimmick;
    private GroupBox groupBox6;
    private Button btnDespawnMonster;
    private Button btnKillMonster;
    private Button btnGetMonsterHealth;
    private Button btnSetMonsterHealth;
    private Label label23;
    private NumericUpDown nudMonsterHealth;
    private Button btnUnenrageMonster;
    private Button btnEnrageMonster;
}