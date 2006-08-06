/*
 * ControlPanelForm.cs
 * 
 * The main user interface form.
 * 
 * Copyright (c) 2005 Mike Murphy
 * 
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace EMU7800
{
	public partial class ControlPanelForm : Form
	{
		GameSettings CurrGameSettings;
		string ReadMeUri;

		void Reset_lblGameTitle()
		{
			gpbGameTitle.Text = "";
			lblGameTitle.Text = EMU7800App.Copyright;
			lklGameHelp.Visible = false;
		}

		void Update_lblGameTitle()
		{
			GameSettings gs = CurrGameSettings;
			if (gs != null && (gs.Title ?? "").Trim().Length > 0)
			{
				gpbGameTitle.Text = "Selected Game Program";
				lblGameTitle.Text = gs.Title.Trim();
				lklGameHelp.Text = lblGameTitle.Text + " Game Help";
				lklGameHelp.Visible = gs.HelpUri != null;
			}
		}

		bool StartButtonEnabled
		{
			get
			{
				return btnStart.Enabled;
			}
			set
			{
				btnStart.Enabled = value && CurrGameSettings != null;
			}
		}

		bool ResumeButtonEnabled
		{
			get
			{
				return btnResume.Enabled;
			}
			set
			{
				btnResume.Enabled = value && EMU7800App.Instance.M != null;
			}
		}

		public ControlPanelForm()
		{
			InitializeComponent();

			cmbROMDir.Items.Add(EMU7800App.Instance.Settings.ROMDirectory);
			cmbROMDir.SelectedIndex = 0;

			Reset_lblGameTitle();

			// Game Programs TabPage
			gpbGameTitle.Text = "";
			lblGameTitle.Text = EMU7800App.Copyright;
			StartButtonEnabled = true;
			ResumeButtonEnabled = true;

			// Settings TabPage
			cmbHostVideoSelect.SelectedIndex = (int)EMU7800App.Instance.Settings.HostSelect;
			nudFrameRateAdjust.DataBindings.Add("Value", EMU7800App.Instance.Settings, "FrameRateAdjust");
			nudNumSoundBuffers.DataBindings.Add("Value", EMU7800App.Instance.Settings, "NumSoundBuffers");
			nudSoundVolume.DataBindings.Add("Value", EMU7800App.Instance.Settings, "SoundVolume");
			cbxSkip7800BIOS.DataBindings.Add("Checked", EMU7800App.Instance.Settings, "Skip7800BIOS");
			cbxHSC7800.DataBindings.Add("Checked", EMU7800App.Instance.Settings, "Use7800HSC");

			// Help TabPage
			string fn = Path.Combine(Directory.GetCurrentDirectory(), "README\\README.html");
			ReadMeUri = File.Exists(fn) ? fn : null;
			lklReadMe.Enabled = ReadMeUri != null;
			if (ReadMeUri != null)
			{
				lklReadMe_Clicked(this, null);
			}
			else
			{
				Trace.Write("README not found at: ");
				Trace.WriteLine(fn);
			}
		}

		void ControlPanelForm_Load(object sender, EventArgs e)
		{
			Size = EMU7800App.Instance.Settings.ControlPanelFormSize;
			Show();
			LoadTreeView();
		}

		void btnStart_Click(object sender, EventArgs e)
		{
			Start(null);
		}

		void Start(InputAdapter ia)
		{
			Hide();
			try
			{
				EMU7800App.Instance.RunMachine(CurrGameSettings, ia);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Machine/Host Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				Trace.WriteLine(ex.Message);
				Trace.WriteLine(ex.StackTrace);
			}
			Show();
			StartButtonEnabled = true;
			ResumeButtonEnabled = true;
		}

		void btnResume_Click(object sender, EventArgs e)
		{
			Resume();
		}

		void Resume()
		{
			Hide();
			try
			{
				EMU7800App.Instance.RunMachine();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Host Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				Trace.WriteLine(ex.Message);
				Trace.WriteLine(ex.StackTrace);
			}
			Show();
			StartButtonEnabled = true;
			ResumeButtonEnabled = true;
		}

		void btnQuit_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		void lklReadMe_Clicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			webHelpBrowser.Navigate(ReadMeUri);
		}

		void lklGameHelp_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			lklGameHelp.Enabled = false;
			webHelpBrowser.Navigate(CurrGameSettings.HelpUri);
		}

		void ControlPanelForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			EMU7800App.Instance.Settings.ControlPanelFormSize = Size;
		}

		private void webHelpBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
			lklGameHelp.Enabled = true;
		}
	}
}