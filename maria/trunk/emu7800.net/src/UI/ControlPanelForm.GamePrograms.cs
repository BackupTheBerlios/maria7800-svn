using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EMU7800
{
	partial class ControlPanelForm
	{
		TreeNode tnTitle, tnUnknown;
		bool DoubleClickReady;
		int ROMFileCount;

		void cmbROMDir_SelectedValueChanged(object sender, EventArgs e)
		{
			if (cmbROMDir.Text != EMU7800App.Instance.Settings.ROMDirectory)
			{
				EMU7800App.Instance.Settings.ROMDirectory = cmbROMDir.Text;
				LoadTreeView();
			}
		}

		void btnBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofdROMSelect = new OpenFileDialog();
			ofdROMSelect.Title = "Select ROM File";
			ofdROMSelect.Filter = "ROMs (*.bin)|*.bin|A78 ROMs (*.a78)|*.a78";
			ofdROMSelect.FilterIndex = 1;

			ofdROMSelect.InitialDirectory
				= Directory.Exists(cmbROMDir.Text)
				? cmbROMDir.Text : EMU7800App.Instance.Settings.RootDirectory;

			if (ofdROMSelect.ShowDialog() == DialogResult.OK)
			{
				Application.DoEvents();
				GameSelectByFileName(ofdROMSelect.FileName);
			}
		}

		bool GameSelectByFileName(string fn)
		{
			FileInfo fi = new FileInfo(fn);

			if (!cmbROMDir.Items.Contains(fi.DirectoryName))
			{
				cmbROMDir.Items.Add(fi.DirectoryName);
			}

			// this will reload the TreeView via cmbROMDir_SelectedValueChanged
			cmbROMDir.SelectedItem = fi.DirectoryName;

			bool recog = SelectTitle(fi.FullName);
			if (!recog)
			{
				MessageBox.Show("Use Console Tab to specify custom attributes.",
					"ROM Not Recognized",
					MessageBoxButtons.OK,
					MessageBoxIcon.Exclamation);
				GameSettings gs = EMU7800App.Instance.ROMProperties.GetGameSettings(MD5.ComputeMD5Digest(fi));
				gs.FileInfo = fi;
				Trace.WriteLine("-------");
				Trace.WriteLine("Unrecognized ROM: prefilled GameSettings fields as follows:");
				Trace.WriteLine(gs);
				CurrGameSettings = gs;
				Update_lblGameTitle();
			}

			return recog;
		}

		void tvwROMList_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				e.Handled = true;
				SetSelection(tvwROMList.SelectedNode);
				tvwROMList_NodeMouseDoubleClick(sender, null);
			}
		}

		void tvwROMList_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			SetSelection(e.Node);
		}

		void tvwROMList_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (DoubleClickReady && CurrGameSettings.FileInfo != null)
			{
				btnStart_Click(sender, e);
			}
			DoubleClickReady = false;
		}

		void SetSelection(TreeNode n)
		{
			DoubleClickReady = false;
			if (n == null || n.Tag == null)
			{
			}
			else
			{
				DoubleClickReady = true;

				GameSettings gs = n.Tag as GameSettings;
				if (gs != CurrGameSettings)
				{
					CurrGameSettings = gs;
					Trace.WriteLine(CurrGameSettings.ToString());
				}

				Update_lblGameTitle();
				StartButtonEnabled = true;
				ResumeButtonEnabled = false;
			}
		}

		void LoadTreeView()
		{
			Cursor = Cursors.WaitCursor;

			ROMFileCount = 0;
			lblROMCount.Text = "Examining ROM Directory...";

			tvwROMList.BeginUpdate();
			tvwROMList.Nodes.Clear();

			tnTitle = new TreeNode("Title", 0, 1);
			tvwROMList.Nodes.Add(tnTitle);

			Hashtable manuIndex = AddTreeSubRoot(tvwROMList, "Manufacturer", new string[] {
			"Absolute",
			"Activision",
			"Apollo",
			"Atari",
			"Avalon Hill",
			"Bitcorp",
			"Bomb",
			"CBS Electronics",
			"CCE",
			"Coleco",
			"CommaVid",
			"Data Age",
			"Epyx",
			"Exus",
			"Froggo",
			"HomeVision",
			"Hozer Video Games",
			"Imagic",
			"ITT Family Games",
			"Konami",
			"Mattel",
			"Milton Bradley",
			"Mystique",
			"Mythicon",
			"Panda",
			"Parker Bros",
			"Playaround",
			"Sears",
			"Sega",
			"Spectravideo",
			"Starsoft",
			"Suntek",
			"Telegames",
			"Telesys",
			"Tigervision",
			"US Games",
			"Video Gems",
			"Xonox",
			"Zellers",
			"Zimag",
			"20th Century Fox"});

			ArrayList al = new ArrayList();
			for (int i = 1977; i <= System.DateTime.Today.Year; i++)
			{
				al.Add(i.ToString());
			}
			Hashtable yearIndex = AddTreeSubRoot(tvwROMList, "Year", (string[])al.ToArray(typeof(System.String)));

			Hashtable rareIndex = AddTreeSubRoot(tvwROMList, "Rarity", new string[] {
			"Common", "Uncommon", "Rare", "Extremely Rare",
			"Unbelievably Rare", "Prototype", "Unreleased Prototype"});

			Hashtable machIndex = AddTreeSubRoot(tvwROMList, "Machine Type", new string[] {
				GetMachineTypeString(MachineType.A2600NTSC, true),
				GetMachineTypeString(MachineType.A2600PAL, true),
				GetMachineTypeString(MachineType.A7800NTSC, true),
				GetMachineTypeString(MachineType.A7800PAL, true)});

			Hashtable contIndex = AddTreeSubRoot(tvwROMList, "LController", new string[] {
			"Joystick", "ProLineJoystick", "Paddles", "Driving", "Keypad", "Lightgun", "BoosterGrip"});
#if DEBUG
			string[] cartList = Enum.GetNames(typeof(CartType));
			Hashtable cartIndex = AddTreeSubRoot(tvwROMList, "Cartridge Type", cartList);
#endif
			tnUnknown = new TreeNode("Unknown", 0, 1);
			tvwROMList.Nodes.Add(tnUnknown);

			Application.DoEvents();

			FileInfo[] romFiles;
			if (!Directory.Exists(EMU7800App.Instance.Settings.ROMDirectory))
			{
				EMU7800App.Instance.Settings.ROMDirectory = Directory.GetCurrentDirectory();
			}
			romFiles = new DirectoryInfo(EMU7800App.Instance.Settings.ROMDirectory).GetFiles();

			pgbROMCount.Minimum = 0;
			pgbROMCount.Value = 0;
			pgbROMCount.Maximum = romFiles.Length;
			lblROMCount.Visible = false;
			pgbROMCount.Visible = true;

			foreach (FileInfo fi in romFiles)
			{
				GameSettings gs = EMU7800App.Instance.ROMProperties.GetGameSettingsFromFile(fi);
				if (gs == null)
				{
					continue;
				}

				ROMFileCount++;
				pgbROMCount.Value = ROMFileCount;

				TreeNode tn = new TreeNode(BuildTitle(gs.Title, gs.Manufacturer, GetMachineTypeString(gs.MachineType, false), gs.Year), 2, 2);
				tn.Tag = gs;
				tnTitle.Nodes.Add(tn);

				AddTreeNode(manuIndex, gs, gs.Manufacturer, gs.Title, GetMachineTypeString(gs.MachineType, false), gs.Year);
				AddTreeNode(yearIndex, gs, gs.Year, gs.Title, gs.Manufacturer, GetMachineTypeString(gs.MachineType, false));
				AddTreeNode(rareIndex, gs, gs.Rarity, gs.Title, gs.Manufacturer, GetMachineTypeString(gs.MachineType, false), gs.Year);
				AddTreeNode(machIndex, gs, GetMachineTypeString(gs.MachineType, true), gs.Title, gs.Manufacturer, GetMachineTypeString(gs.MachineType, false), gs.Year);
				AddTreeNode(contIndex, gs, gs.LController.ToString(), gs.Title, gs.Manufacturer, GetMachineTypeString(gs.MachineType, false), gs.Year);
#if DEBUG
				AddTreeNode(cartIndex, gs, gs.CartType.ToString(), gs.Title, gs.Manufacturer, GetMachineTypeString(gs.MachineType, false), gs.Year);
#endif
				Application.DoEvents();
			}

			for (int i = 0; i < tvwROMList.Nodes.Count; )
			{
				TreeNode c = tvwROMList.Nodes[i];
				if (PruneTree(c))
				{
					c.Remove();
				}
				else
				{
					i++;
				}
			}

			tvwROMList.Sorted = true;
			tvwROMList.EndUpdate();
			tvwROMList.Update();
			pgbROMCount.Visible = false;

			lblROMCount.Text = String.Format("{0} ROM file{1} recognized", ROMFileCount, (ROMFileCount != 1 ? "s" : ""));
			lblROMCount.Visible = true;

			Cursor = Cursors.Arrow;
		}

		// Remove TreeNodes that have no dependencies
		bool PruneTree(TreeNode p)
		{
			int score = 0;
			if (p.Nodes.Count > 0)
			{
				for (int i = 0; i < p.Nodes.Count; )
				{
					TreeNode c = p.Nodes[i];
					if (PruneTree(c))
					{
						c.Remove();
					}
					else
					{
						score++;
						i++;
					}
				}
			}
			return (score == 0 && p.Tag == null);
		}

		bool SelectTitle(string fullName)
		{
			FileInfo fi = new FileInfo(fullName);
			string md5 = MD5.ComputeMD5Digest(fi);
			if (md5 == null)
			{
				return false;
			}
			foreach (TreeNode tn in tnTitle.Nodes)
			{
				if (tn.Tag != null && md5 == ((GameSettings)tn.Tag).MD5)
				{
					tvwROMList.SelectedNode = tn;
					return true;
				}
			}
			foreach (TreeNode tn in tnUnknown.Nodes)
			{
				if (tn.Tag != null && md5 == ((GameSettings)tn.Tag).MD5)
				{
					tvwROMList.SelectedNode = tn;
					return true;
				}
			}
			return false;
		}

		static Hashtable AddTreeSubRoot(TreeView root, string label, string[] subList)
		{
			TreeNode tnparent = new TreeNode(label, 0, 1);
			root.Nodes.Add(tnparent);
			Hashtable index = new Hashtable();
			TreeNode tn;
			foreach (string s in subList)
			{
				tn = new TreeNode(s, 0, 1);
				tnparent.Nodes.Add(tn);
				index.Add(s, tn);
			}
			tn = new TreeNode("Other", 0, 1);
			tnparent.Nodes.Add(tn);
			index.Add("Other", tn);
			return index;
		}

		void AddTreeNode(Hashtable index, GameSettings gs, string key, params string[] titlebits)
		{
			TreeNode tn = new TreeNode(BuildTitle(titlebits), 2, 2);
			tn.Tag = gs;
			if (key == null || !index.ContainsKey(key))
			{
				key = "Other";
			}
			((TreeNode)index[key]).Nodes.Add(tn);
		}

		string BuildTitle(params string[] titlebits)
		{
			StringBuilder title = new StringBuilder();

			for (int i = 0; i < titlebits.Length; i++)
			{
				if (titlebits[i] != null && titlebits[i].Length > 0)
				{
					if (i > 0)
					{
						title.Append(", ");
					}
					title.Append(titlebits[i]);
				}
			}
			return title.ToString();
		}

		string GetMachineTypeString(MachineType machineType, bool verbose)
		{
			string mts = "";
				
			switch (machineType)
			{
				case MachineType.A2600NTSC:
					mts = verbose ? "VCS (2600) NTSC (N.American)" : "VCS";
					break;
				case MachineType.A2600PAL:
					mts = verbose ? "VCS (2600) PAL (European)" : "VCS PAL";
					break;
				case MachineType.A7800NTSC:
					mts = verbose ? "ProSystem (7800) NTSC (N.American)" : "ProSystem";
					break;
				case MachineType.A7800PAL:
					mts = verbose ? "ProSystem (7800) PAL (European)" : "ProSystem PAL";
					break;
			}

			return mts;
		}
	}
}

