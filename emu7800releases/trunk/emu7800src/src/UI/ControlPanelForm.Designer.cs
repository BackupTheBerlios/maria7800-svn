namespace EMU7800
{
	partial class ControlPanelForm
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlPanelForm));
			this.btnStart = new System.Windows.Forms.Button();
			this.btnResume = new System.Windows.Forms.Button();
			this.btnQuit = new System.Windows.Forms.Button();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tpgGamePrograms = new System.Windows.Forms.TabPage();
			this.gpbGameTitle = new System.Windows.Forms.GroupBox();
			this.lblGameTitle = new System.Windows.Forms.Label();
			this.pgbROMCount = new System.Windows.Forms.ProgressBar();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.cmbROMDir = new System.Windows.Forms.ComboBox();
			this.lblROMDir = new System.Windows.Forms.Label();
			this.lblROMCount = new System.Windows.Forms.Label();
			this.tvwROMList = new System.Windows.Forms.TreeView();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.tpgSettings = new System.Windows.Forms.TabPage();
			this.gpb7800SpecificSettings = new System.Windows.Forms.GroupBox();
			this.cbxHSC7800 = new System.Windows.Forms.CheckBox();
			this.cbxSkip7800BIOS = new System.Windows.Forms.CheckBox();
			this.nudSoundVolume = new System.Windows.Forms.NumericUpDown();
			this.nudNumSoundBuffers = new System.Windows.Forms.NumericUpDown();
			this.nudFrameRateAdjust = new System.Windows.Forms.NumericUpDown();
			this.cmbHostVideoSelect = new System.Windows.Forms.ComboBox();
			this.btnLoadMachineState = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.lblSkip7800BIOS = new System.Windows.Forms.Label();
			this.lblFrameRateAdjust = new System.Windows.Forms.Label();
			this.lblHostSelect = new System.Windows.Forms.Label();
			this.tpgConsole = new System.Windows.Forms.TabPage();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.inpBox = new System.Windows.Forms.TextBox();
			this.outBox = new System.Windows.Forms.TextBox();
			this.tpgHelp = new System.Windows.Forms.TabPage();
			this.lklGameHelp = new System.Windows.Forms.LinkLabel();
			this.lklReadMe = new System.Windows.Forms.LinkLabel();
			this.webHelpBrowser = new System.Windows.Forms.WebBrowser();
			this.tabControl.SuspendLayout();
			this.tpgGamePrograms.SuspendLayout();
			this.gpbGameTitle.SuspendLayout();
			this.tpgSettings.SuspendLayout();
			this.gpb7800SpecificSettings.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudSoundVolume)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudNumSoundBuffers)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.nudFrameRateAdjust)).BeginInit();
			this.tpgConsole.SuspendLayout();
			this.tpgHelp.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnStart.Location = new System.Drawing.Point(4, 436);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 0;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnResume
			// 
			this.btnResume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnResume.Location = new System.Drawing.Point(85, 436);
			this.btnResume.Name = "btnResume";
			this.btnResume.Size = new System.Drawing.Size(75, 23);
			this.btnResume.TabIndex = 1;
			this.btnResume.Text = "Resume";
			this.btnResume.UseVisualStyleBackColor = true;
			this.btnResume.Click += new System.EventHandler(this.btnResume_Click);
			// 
			// btnQuit
			// 
			this.btnQuit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnQuit.Location = new System.Drawing.Point(166, 436);
			this.btnQuit.Name = "btnQuit";
			this.btnQuit.Size = new System.Drawing.Size(75, 23);
			this.btnQuit.TabIndex = 2;
			this.btnQuit.Text = "Quit";
			this.btnQuit.UseVisualStyleBackColor = true;
			this.btnQuit.Click += new System.EventHandler(this.btnQuit_Click);
			// 
			// tabControl
			// 
			this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControl.Controls.Add(this.tpgGamePrograms);
			this.tabControl.Controls.Add(this.tpgSettings);
			this.tabControl.Controls.Add(this.tpgConsole);
			this.tabControl.Controls.Add(this.tpgHelp);
			this.tabControl.Location = new System.Drawing.Point(4, 3);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(486, 427);
			this.tabControl.TabIndex = 3;
			// 
			// tpgGamePrograms
			// 
			this.tpgGamePrograms.Controls.Add(this.gpbGameTitle);
			this.tpgGamePrograms.Controls.Add(this.pgbROMCount);
			this.tpgGamePrograms.Controls.Add(this.btnBrowse);
			this.tpgGamePrograms.Controls.Add(this.cmbROMDir);
			this.tpgGamePrograms.Controls.Add(this.lblROMDir);
			this.tpgGamePrograms.Controls.Add(this.lblROMCount);
			this.tpgGamePrograms.Controls.Add(this.tvwROMList);
			this.tpgGamePrograms.Location = new System.Drawing.Point(4, 22);
			this.tpgGamePrograms.Name = "tpgGamePrograms";
			this.tpgGamePrograms.Size = new System.Drawing.Size(478, 401);
			this.tpgGamePrograms.TabIndex = 0;
			this.tpgGamePrograms.Text = "Game Programs";
			this.tpgGamePrograms.UseVisualStyleBackColor = true;
			// 
			// gpbGameTitle
			// 
			this.gpbGameTitle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.gpbGameTitle.Controls.Add(this.lblGameTitle);
			this.gpbGameTitle.Location = new System.Drawing.Point(7, 46);
			this.gpbGameTitle.Name = "gpbGameTitle";
			this.gpbGameTitle.Size = new System.Drawing.Size(464, 47);
			this.gpbGameTitle.TabIndex = 7;
			this.gpbGameTitle.TabStop = false;
			this.gpbGameTitle.Text = "Selected Game Program";
			// 
			// lblGameTitle
			// 
			this.lblGameTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lblGameTitle.Font = new System.Drawing.Font("Lucida Console", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblGameTitle.Location = new System.Drawing.Point(29, 16);
			this.lblGameTitle.Name = "lblGameTitle";
			this.lblGameTitle.Size = new System.Drawing.Size(420, 22);
			this.lblGameTitle.TabIndex = 0;
			this.lblGameTitle.Text = "Text";
			this.lblGameTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// pgbROMCount
			// 
			this.pgbROMCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.pgbROMCount.Location = new System.Drawing.Point(4, 381);
			this.pgbROMCount.Name = "pgbROMCount";
			this.pgbROMCount.Size = new System.Drawing.Size(471, 17);
			this.pgbROMCount.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.pgbROMCount.TabIndex = 6;
			// 
			// btnBrowse
			// 
			this.btnBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnBrowse.Location = new System.Drawing.Point(396, 19);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.Size = new System.Drawing.Size(75, 23);
			this.btnBrowse.TabIndex = 5;
			this.btnBrowse.Text = "Browse";
			this.btnBrowse.UseVisualStyleBackColor = true;
			this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
			// 
			// cmbROMDir
			// 
			this.cmbROMDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbROMDir.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbROMDir.FormattingEnabled = true;
			this.cmbROMDir.Location = new System.Drawing.Point(7, 19);
			this.cmbROMDir.Name = "cmbROMDir";
			this.cmbROMDir.Size = new System.Drawing.Size(383, 21);
			this.cmbROMDir.TabIndex = 4;
			this.cmbROMDir.SelectedValueChanged += new System.EventHandler(this.cmbROMDir_SelectedValueChanged);
			// 
			// lblROMDir
			// 
			this.lblROMDir.AutoSize = true;
			this.lblROMDir.Location = new System.Drawing.Point(4, 3);
			this.lblROMDir.Name = "lblROMDir";
			this.lblROMDir.Size = new System.Drawing.Size(114, 13);
			this.lblROMDir.TabIndex = 3;
			this.lblROMDir.Text = "Current ROM Directory";
			// 
			// lblROMCount
			// 
			this.lblROMCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.lblROMCount.AutoSize = true;
			this.lblROMCount.Location = new System.Drawing.Point(1, 388);
			this.lblROMCount.Name = "lblROMCount";
			this.lblROMCount.Size = new System.Drawing.Size(0, 13);
			this.lblROMCount.TabIndex = 1;
			// 
			// tvwROMList
			// 
			this.tvwROMList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tvwROMList.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tvwROMList.ImageIndex = 0;
			this.tvwROMList.ImageList = this.imageList1;
			this.tvwROMList.Location = new System.Drawing.Point(7, 99);
			this.tvwROMList.Name = "tvwROMList";
			this.tvwROMList.SelectedImageIndex = 0;
			this.tvwROMList.Size = new System.Drawing.Size(464, 276);
			this.tvwROMList.TabIndex = 0;
			this.tvwROMList.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvwROMList_NodeMouseDoubleClick);
			this.tvwROMList.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.tvwROMList_NodeMouseClick);
			this.tvwROMList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tvwROMList_KeyDown);
			// 
			// imageList1
			// 
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "CLSDFOLD.BMP");
			this.imageList1.Images.SetKeyName(1, "OPENFOLD.BMP");
			this.imageList1.Images.SetKeyName(2, "ROM.BMP");
			// 
			// tpgSettings
			// 
			this.tpgSettings.Controls.Add(this.gpb7800SpecificSettings);
			this.tpgSettings.Controls.Add(this.nudSoundVolume);
			this.tpgSettings.Controls.Add(this.nudNumSoundBuffers);
			this.tpgSettings.Controls.Add(this.nudFrameRateAdjust);
			this.tpgSettings.Controls.Add(this.cmbHostVideoSelect);
			this.tpgSettings.Controls.Add(this.btnLoadMachineState);
			this.tpgSettings.Controls.Add(this.label4);
			this.tpgSettings.Controls.Add(this.lblSkip7800BIOS);
			this.tpgSettings.Controls.Add(this.lblFrameRateAdjust);
			this.tpgSettings.Controls.Add(this.lblHostSelect);
			this.tpgSettings.Location = new System.Drawing.Point(4, 22);
			this.tpgSettings.Name = "tpgSettings";
			this.tpgSettings.Size = new System.Drawing.Size(478, 401);
			this.tpgSettings.TabIndex = 1;
			this.tpgSettings.Text = "Settings";
			this.tpgSettings.UseVisualStyleBackColor = true;
			// 
			// gpb7800SpecificSettings
			// 
			this.gpb7800SpecificSettings.Controls.Add(this.cbxHSC7800);
			this.gpb7800SpecificSettings.Controls.Add(this.cbxSkip7800BIOS);
			this.gpb7800SpecificSettings.Location = new System.Drawing.Point(20, 153);
			this.gpb7800SpecificSettings.Name = "gpb7800SpecificSettings";
			this.gpb7800SpecificSettings.Size = new System.Drawing.Size(160, 71);
			this.gpb7800SpecificSettings.TabIndex = 6;
			this.gpb7800SpecificSettings.TabStop = false;
			this.gpb7800SpecificSettings.Text = "7800 Specific";
			// 
			// cbxHSC7800
			// 
			this.cbxHSC7800.AutoSize = true;
			this.cbxHSC7800.Location = new System.Drawing.Point(15, 42);
			this.cbxHSC7800.Name = "cbxHSC7800";
			this.cbxHSC7800.Size = new System.Drawing.Size(123, 17);
			this.cbxHSC7800.TabIndex = 1;
			this.cbxHSC7800.Text = "Use High Score Cart";
			this.cbxHSC7800.UseVisualStyleBackColor = true;
			// 
			// cbxSkip7800BIOS
			// 
			this.cbxSkip7800BIOS.AutoSize = true;
			this.cbxSkip7800BIOS.Location = new System.Drawing.Point(15, 19);
			this.cbxSkip7800BIOS.Name = "cbxSkip7800BIOS";
			this.cbxSkip7800BIOS.Size = new System.Drawing.Size(112, 17);
			this.cbxSkip7800BIOS.TabIndex = 0;
			this.cbxSkip7800BIOS.Text = "Skip BIOS Startup";
			this.cbxSkip7800BIOS.UseVisualStyleBackColor = true;
			// 
			// nudSoundVolume
			// 
			this.nudSoundVolume.Location = new System.Drawing.Point(198, 118);
			this.nudSoundVolume.Name = "nudSoundVolume";
			this.nudSoundVolume.ReadOnly = true;
			this.nudSoundVolume.Size = new System.Drawing.Size(47, 20);
			this.nudSoundVolume.TabIndex = 4;
			// 
			// nudNumSoundBuffers
			// 
			this.nudNumSoundBuffers.Location = new System.Drawing.Point(198, 91);
			this.nudNumSoundBuffers.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
			this.nudNumSoundBuffers.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
			this.nudNumSoundBuffers.Name = "nudNumSoundBuffers";
			this.nudNumSoundBuffers.ReadOnly = true;
			this.nudNumSoundBuffers.Size = new System.Drawing.Size(47, 20);
			this.nudNumSoundBuffers.TabIndex = 3;
			this.nudNumSoundBuffers.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
			// 
			// nudFrameRateAdjust
			// 
			this.nudFrameRateAdjust.Location = new System.Drawing.Point(198, 65);
			this.nudFrameRateAdjust.Maximum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.nudFrameRateAdjust.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            -2147483648});
			this.nudFrameRateAdjust.Name = "nudFrameRateAdjust";
			this.nudFrameRateAdjust.ReadOnly = true;
			this.nudFrameRateAdjust.Size = new System.Drawing.Size(47, 20);
			this.nudFrameRateAdjust.TabIndex = 2;
			this.nudFrameRateAdjust.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
			// 
			// cmbHostVideoSelect
			// 
			this.cmbHostVideoSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbHostVideoSelect.FormattingEnabled = true;
			this.cmbHostVideoSelect.Items.AddRange(new object[] {
            "Windows GDI",
            "Simple DirectMedia Layer (SDL)"});
			this.cmbHostVideoSelect.Location = new System.Drawing.Point(16, 29);
			this.cmbHostVideoSelect.Name = "cmbHostVideoSelect";
			this.cmbHostVideoSelect.Size = new System.Drawing.Size(229, 21);
			this.cmbHostVideoSelect.TabIndex = 0;
			this.cmbHostVideoSelect.SelectedIndexChanged += new System.EventHandler(this.cmbHostVideoSelect_SelectedIndexChanged);
			// 
			// btnLoadMachineState
			// 
			this.btnLoadMachineState.Location = new System.Drawing.Point(16, 246);
			this.btnLoadMachineState.Name = "btnLoadMachineState";
			this.btnLoadMachineState.Size = new System.Drawing.Size(164, 23);
			this.btnLoadMachineState.TabIndex = 7;
			this.btnLoadMachineState.Text = "Load Machine State";
			this.btnLoadMachineState.UseVisualStyleBackColor = true;
			this.btnLoadMachineState.Click += new System.EventHandler(this.btnLoadMachineState_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(17, 118);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(79, 13);
			this.label4.TabIndex = 3;
			this.label4.Text = "Sound Volume:";
			// 
			// lblSkip7800BIOS
			// 
			this.lblSkip7800BIOS.AutoSize = true;
			this.lblSkip7800BIOS.Location = new System.Drawing.Point(17, 91);
			this.lblSkip7800BIOS.Name = "lblSkip7800BIOS";
			this.lblSkip7800BIOS.Size = new System.Drawing.Size(112, 13);
			this.lblSkip7800BIOS.TabIndex = 2;
			this.lblSkip7800BIOS.Text = "Sound Queue Length:";
			// 
			// lblFrameRateAdjust
			// 
			this.lblFrameRateAdjust.AutoSize = true;
			this.lblFrameRateAdjust.Location = new System.Drawing.Point(17, 65);
			this.lblFrameRateAdjust.Name = "lblFrameRateAdjust";
			this.lblFrameRateAdjust.Size = new System.Drawing.Size(120, 13);
			this.lblFrameRateAdjust.TabIndex = 1;
			this.lblFrameRateAdjust.Text = "Frame Rate Adjust (+/-):";
			// 
			// lblHostSelect
			// 
			this.lblHostSelect.AutoSize = true;
			this.lblHostSelect.Location = new System.Drawing.Point(15, 13);
			this.lblHostSelect.Name = "lblHostSelect";
			this.lblHostSelect.Size = new System.Drawing.Size(65, 13);
			this.lblHostSelect.TabIndex = 0;
			this.lblHostSelect.Text = "Host Select:";
			// 
			// tpgConsole
			// 
			this.tpgConsole.Controls.Add(this.label2);
			this.tpgConsole.Controls.Add(this.label1);
			this.tpgConsole.Controls.Add(this.inpBox);
			this.tpgConsole.Controls.Add(this.outBox);
			this.tpgConsole.Location = new System.Drawing.Point(4, 22);
			this.tpgConsole.Name = "tpgConsole";
			this.tpgConsole.Size = new System.Drawing.Size(478, 401);
			this.tpgConsole.TabIndex = 2;
			this.tpgConsole.Text = "Console";
			this.tpgConsole.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(10, 358);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(100, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Command-line Input";
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 12);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(106, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Output Message Log";
			// 
			// inpBox
			// 
			this.inpBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.inpBox.BackColor = System.Drawing.Color.Black;
			this.inpBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.inpBox.Font = new System.Drawing.Font("Lucida Sans Typewriter", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.inpBox.ForeColor = System.Drawing.Color.Lime;
			this.inpBox.Location = new System.Drawing.Point(13, 375);
			this.inpBox.Name = "inpBox";
			this.inpBox.Size = new System.Drawing.Size(459, 23);
			this.inpBox.TabIndex = 0;
			this.inpBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.inpBox_KeyPress);
			// 
			// outBox
			// 
			this.outBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.outBox.BackColor = System.Drawing.Color.Black;
			this.outBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.outBox.Font = new System.Drawing.Font("Lucida Sans Typewriter", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.outBox.ForeColor = System.Drawing.Color.Lime;
			this.outBox.Location = new System.Drawing.Point(13, 29);
			this.outBox.Multiline = true;
			this.outBox.Name = "outBox";
			this.outBox.ReadOnly = true;
			this.outBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.outBox.Size = new System.Drawing.Size(459, 326);
			this.outBox.TabIndex = 0;
			this.outBox.TabStop = false;
			this.outBox.WordWrap = false;
			this.outBox.VisibleChanged += new System.EventHandler(this.outBox_VisibleChanged);
			// 
			// tpgHelp
			// 
			this.tpgHelp.Controls.Add(this.lklGameHelp);
			this.tpgHelp.Controls.Add(this.lklReadMe);
			this.tpgHelp.Controls.Add(this.webHelpBrowser);
			this.tpgHelp.Location = new System.Drawing.Point(4, 22);
			this.tpgHelp.Name = "tpgHelp";
			this.tpgHelp.Size = new System.Drawing.Size(478, 401);
			this.tpgHelp.TabIndex = 3;
			this.tpgHelp.Text = "Help";
			this.tpgHelp.UseVisualStyleBackColor = true;
			// 
			// lklGameHelp
			// 
			this.lklGameHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lklGameHelp.AutoSize = true;
			this.lklGameHelp.Location = new System.Drawing.Point(74, 378);
			this.lklGameHelp.Name = "lklGameHelp";
			this.lklGameHelp.Size = new System.Drawing.Size(105, 13);
			this.lklGameHelp.TabIndex = 1;
			this.lklGameHelp.TabStop = true;
			this.lklGameHelp.Text = "Selected Game Help";
			this.lklGameHelp.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.lklGameHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lklGameHelp_LinkClicked);
			// 
			// lklReadMe
			// 
			this.lklReadMe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.lklReadMe.AutoSize = true;
			this.lklReadMe.Location = new System.Drawing.Point(3, 378);
			this.lklReadMe.Name = "lklReadMe";
			this.lklReadMe.Size = new System.Drawing.Size(53, 13);
			this.lklReadMe.TabIndex = 0;
			this.lklReadMe.TabStop = true;
			this.lklReadMe.Text = "README";
			this.lklReadMe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.lklReadMe.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lklReadMe_Clicked);
			// 
			// webHelpBrowser
			// 
			this.webHelpBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.webHelpBrowser.Location = new System.Drawing.Point(-4, 3);
			this.webHelpBrowser.MinimumSize = new System.Drawing.Size(20, 20);
			this.webHelpBrowser.Name = "webHelpBrowser";
			this.webHelpBrowser.ScriptErrorsSuppressed = true;
			this.webHelpBrowser.Size = new System.Drawing.Size(476, 372);
			this.webHelpBrowser.TabIndex = 0;
			this.webHelpBrowser.TabStop = false;
			this.webHelpBrowser.Url = new System.Uri("", System.UriKind.Relative);
			this.webHelpBrowser.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webHelpBrowser_DocumentCompleted);
			// 
			// ControlPanelForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(492, 466);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.btnQuit);
			this.Controls.Add(this.btnResume);
			this.Controls.Add(this.btnStart);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(500, 500);
			this.Name = "ControlPanelForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "EMU7800 Control Panel";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ControlPanelForm_FormClosing);
			this.Load += new System.EventHandler(this.ControlPanelForm_Load);
			this.tabControl.ResumeLayout(false);
			this.tpgGamePrograms.ResumeLayout(false);
			this.tpgGamePrograms.PerformLayout();
			this.gpbGameTitle.ResumeLayout(false);
			this.tpgSettings.ResumeLayout(false);
			this.tpgSettings.PerformLayout();
			this.gpb7800SpecificSettings.ResumeLayout(false);
			this.gpb7800SpecificSettings.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.nudSoundVolume)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudNumSoundBuffers)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.nudFrameRateAdjust)).EndInit();
			this.tpgConsole.ResumeLayout(false);
			this.tpgConsole.PerformLayout();
			this.tpgHelp.ResumeLayout(false);
			this.tpgHelp.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnResume;
		private System.Windows.Forms.Button btnQuit;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tpgGamePrograms;
		private System.Windows.Forms.TreeView tvwROMList;
		private System.Windows.Forms.Label lblROMCount;
		private System.Windows.Forms.Label lblROMDir;
		private System.Windows.Forms.ComboBox cmbROMDir;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TabPage tpgSettings;
		private System.Windows.Forms.TabPage tpgConsole;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label lblSkip7800BIOS;
		private System.Windows.Forms.Label lblFrameRateAdjust;
		private System.Windows.Forms.Label lblHostSelect;
		private System.Windows.Forms.Button btnLoadMachineState;
        private System.Windows.Forms.ComboBox cmbHostVideoSelect;
		private System.Windows.Forms.NumericUpDown nudSoundVolume;
		private System.Windows.Forms.NumericUpDown nudNumSoundBuffers;
		private System.Windows.Forms.NumericUpDown nudFrameRateAdjust;
		private System.Windows.Forms.CheckBox cbxHSC7800;
		private System.Windows.Forms.CheckBox cbxSkip7800BIOS;
		private System.Windows.Forms.GroupBox gpb7800SpecificSettings;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox inpBox;
		private System.Windows.Forms.TextBox outBox;
		private System.Windows.Forms.ProgressBar pgbROMCount;
		private System.Windows.Forms.GroupBox gpbGameTitle;
		private System.Windows.Forms.Label lblGameTitle;
		private System.Windows.Forms.TabPage tpgHelp;
		private System.Windows.Forms.WebBrowser webHelpBrowser;
		private System.Windows.Forms.LinkLabel lklReadMe;
		private System.Windows.Forms.LinkLabel lklGameHelp;
		private System.Windows.Forms.Button btnStart;
	}
}