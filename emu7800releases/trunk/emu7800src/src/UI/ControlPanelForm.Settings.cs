using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace EMU7800
{
	partial class ControlPanelForm
	{
		void btnLoadMachineState_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofdFileSelect = new OpenFileDialog();
			ofdFileSelect.Title = "Select EMU Machine State";
			ofdFileSelect.Filter = "Machine States (*.emu)|*.emu";
			ofdFileSelect.FilterIndex = 1;

			ofdFileSelect.InitialDirectory = EMU7800App.Instance.Settings.OutputDirectory;
			if (ofdFileSelect.ShowDialog() == DialogResult.OK)
			{
				try
				{
					EMU7800App.Instance.M = Machine.Deserialize(ofdFileSelect.FileName);
					Reset_lblGameTitle();
					StartButtonEnabled = false;
					ResumeButtonEnabled = true;
					CurrGameSettings = null;
					Trace.WriteLine("machine state restored");
				}
				catch (Exception ex)
				{
					StartButtonEnabled = false;
					ResumeButtonEnabled = false;
					Trace.Write("error restoring machine state: ");
					Trace.WriteLine(ex);
				}
			}
		}

		void cmbHostVideoSelect_SelectedIndexChanged(object sender, EventArgs e)
		{
			EMU7800App.Instance.Settings.HostSelect = (HostType)cmbHostVideoSelect.SelectedIndex;
		}
	}
}
