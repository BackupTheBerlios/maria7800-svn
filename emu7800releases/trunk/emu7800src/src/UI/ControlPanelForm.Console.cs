using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EMU7800
{
	partial class ControlPanelForm
	{
		void outBox_VisibleChanged(object sender, EventArgs e)
		{
			if (outBox.Visible)
			{
				outBox.AppendText(EMUTraceListener.Instance.GetMsgs());
			}
			inpBox.Text = null;
			inpBox.Focus();
		}

		void inpBox_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)
			{
				e.Handled = true;
				IssueInpBoxCommand();
			}
		}

		void IssueInpBoxCommand()
		{
			string commandline = inpBox.Text;
			inpBox.Text = "";
			Trace.WriteLine(String.Format(">{0}", commandline));
			ExecuteCommandLine(new CommandLine(commandline));
			outBox.AppendText(EMUTraceListener.Instance.GetMsgs());
			inpBox.Focus();
		}

		void ExecuteCommandLine(CommandLine cl)
		{
			bool recog = false;
			GameSettings gs;

			if (EMU7800App.Instance.M != null && EMU7800App.Instance.M.ExecuteCommandLine(cl))
			{
				recog = true;
			}

			switch (cl.Verb)
			{
				case "clear":
					outBox.Clear();
					Trace.WriteLine("log cleared");
					break;
				case "cpunop":
					if (cl.Parms.Length > 0)
					{
						EMU7800App.Instance.Settings.NOPRegisterDumping =
								(cl.Parms[0].StrValue.ToUpper() == "ON");
					}
					Trace.Write("CPU NOP register dumping: ");
					Trace.WriteLine(EMU7800App.Instance.Settings.NOPRegisterDumping ? "ON" : "OFF");
					break;
				case "disablemouse":
					if (!cl.CheckParms("s"))
					{
						Trace.WriteLine("need on/off parm");
					}
					else
					{
						EMU7800App.Instance.Settings.DeactivateMouseInput = (cl.Parms[0].StrValue.ToUpper() == "ON");
					}
					Trace.Write("Disable mouse: ");
					Trace.WriteLine(EMU7800App.Instance.Settings.DeactivateMouseInput ? "ON" : "OFF");
					break;
				case "dumpromprops":
					EMU7800App.Instance.ROMProperties.Dump();
					break;
				case "gs":
					gs = CurrGameSettings;
					if (cl.Parms.Length > 0)
					{
						string var = cl.Parms[0].StrValue.ToLower();
						string val = "";
						for (int i = 1; i < cl.Parms.Length; i++)
						{
							if (i > 1)
								val += " ";
							val += cl.Parms[i].StrValue;
						}
						switch (var)
						{
							case "title": gs.Title = val; break;
							case "manufacturer": gs.Manufacturer = val; break;
							case "year": gs.Year = val; break;
							case "modelno": gs.ModelNo = val; break;
							case "rarity": gs.Rarity = val; break;
							case "carttype":
								try
								{
									gs.CartType = (CartType)Enum.Parse(typeof(CartType), val, true);
								}
								catch
								{
									Trace.WriteLine("Valid CartTypes:");
									foreach (string typ in Enum.GetNames(typeof(CartType)))
									{
										Trace.Write(typ);
										Trace.Write(" ");
									}
									Trace.WriteLine("");
								}
								break;
							case "machinetype":
								try
								{
									gs.MachineType = (MachineType)Enum.Parse(typeof(MachineType), val, true);
								}
								catch
								{
									Trace.WriteLine("Valid MachineTypes:");
									foreach (string typ in Enum.GetNames(typeof(MachineType)))
									{
										Trace.Write(typ);
										Trace.Write(" ");
									}
									Trace.WriteLine("");
								}
								break;
							case "lcontroller":
							case "rcontroller":
								try
								{
									Controller c = (Controller)Enum.Parse(typeof(Controller), val, true);
									if (var.Substring(0, 1) == "l")
									{
										gs.LController = c;
									}
									else
									{
										gs.RController = c;
									}
								}
								catch
								{
									Trace.WriteLine("Valid Controllers:");
									foreach (string typ in Enum.GetNames(typeof(Controller)))
									{
										Trace.Write(typ);
										Trace.Write(" ");
									}
									Trace.WriteLine("");
								}
								break;
							case "helpuri": gs.HelpUri= val; break;
							default:
								Trace.WriteLine("unrecognized GameSettings field");
								break;
						}
					}
					else
					{
						Trace.WriteLine(gs);
					}
					CurrGameSettings = gs;
					break;
				case "outdir":
					Trace.Write("outdir: ");
					Trace.WriteLine(EMU7800App.Instance.Settings.OutputDirectory);
					break;
				case "ls":
					Trace.WriteLine("files in outdir:");
					Trace.WriteLine(EMU7800App.Instance.Settings.OutputDirectory);
					FileInfo[] files;
					try
					{
						files = new DirectoryInfo(EMU7800App.Instance.Settings.OutputDirectory).GetFiles();
						Trace.Write("FileName".PadRight(40, ' '));
						Trace.WriteLine(" Size");
						Trace.WriteLine("".PadRight(45, '-'));
						foreach (FileInfo fi in files)
						{
							Trace.Write(fi.Name.PadRight(40, ' '));
							Trace.Write(" ");
							Trace.Write(fi.Length / 1024);
							Trace.WriteLine("k");
						}
					}
					catch (DirectoryNotFoundException)
					{
						Trace.Write("directory does not exist: ");
						Trace.WriteLine(EMU7800App.Instance.Settings.OutputDirectory);
					}
					break;
				case "rec":
					if (!cl.CheckParms("s"))
					{
						Trace.WriteLine("need filename");
					}
					else if (CurrGameSettings == null)
					{
						Trace.WriteLine("no game selected");
					}
					else
					{
						string fn = GenerateOutFileName(Path.Combine(EMU7800App.Instance.Settings.OutputDirectory, cl.Parms[0].StrValue), ".rec");
						Record(fn);
					}
					break;
				case "pb":
					if (cl.Parms.Length == 1 && !cl.CheckParms("s"))
					{
						Trace.WriteLine("need filename");
					}
					else if (cl.Parms.Length > 1 && !cl.Parms[1].StrValue.Equals("loop"))
					{
						Trace.WriteLine("bad optional parameter");
					}
					else
					{
						string fn = Path.Combine(EMU7800App.Instance.Settings.OutputDirectory, cl.Parms[0].StrValue);
						Playback(fn, cl.Parms.Length > 1);
					}
					break;
				case "run":
					if (CurrGameSettings.FileInfo == null)
					{
						Trace.WriteLine("GameSettings incomplete");
					}
					else
					{
						outBox_VisibleChanged(this, null);
						btnStart_Click(this, null);
					}
					break;
				case "opacity":
					if (cl.Parms.Length == 0)
					{
						Trace.Write(Opacity);
					}
					else if (!cl.CheckParms("i"))
					{
						Trace.WriteLine("bad parm");
					}
					else
					{
						int op = cl.Parms[0].IntValue;
						if (op > 100)
						{
							op = 100;
							Trace.WriteLine("opacity set to 100%");
						}
						else if (op < 25)
						{
							op = 25;
							Trace.WriteLine("opacity set to 25%");
						}
						else
						{
							Trace.WriteLine("ok");
						}
						Opacity = (double)op / 100.0;
					}
					break;
				case "joybuttons":
					if (cl.Parms.Length == 0)
					{
						Trace.WriteLine(String.Format("joybuttons: trigger:{0} booster:{1}\n", EMU7800App.Instance.Settings.JoyBTrigger, EMU7800App.Instance.Settings.JoyBBooster));
					}
					else if (cl.CheckParms("ii"))
					{
						EMU7800App.Instance.Settings.JoyBTrigger = cl.Parms[0].IntValue;
						EMU7800App.Instance.Settings.JoyBBooster = cl.Parms[1].IntValue;
						Trace.WriteLine("joystick button bindings set");
					}
					else
					{
						Trace.WriteLine("bad parms");
						Trace.WriteLine("usage: joybuttons [trigger#] [booster#] [select#] [reset#] [swap#]");
					}
					break;
				case "fps":
					if (cl.Parms.Length == 0)
					{
						Trace.WriteLine(EMU7800App.Instance.Settings.FrameRateAdjust);
					}
					else if (cl.CheckParms("i"))
					{
						EMU7800App.Instance.Settings.FrameRateAdjust = cl.Parms[0].IntValue;
						Trace.WriteLine("ok");
					}
					else if (cl.Parms.Length > 0)
					{
						Trace.WriteLine("bad parm");
					}
					break;
				case "cpuspin":
					if (cl.Parms.Length == 0)
					{
						Trace.WriteLine(EMU7800App.Instance.Settings.CpuSpin);
					}
					else if (!cl.CheckParms("i"))
					{
						Trace.WriteLine("bad parm");
					}
					else
					{
						EMU7800App.Instance.Settings.CpuSpin = cl.Parms[0].IntValue;
						Trace.WriteLine("ok");
					}
					break;
				case "sb":
					if (cl.CheckParms("i"))
					{
						EMU7800App.Instance.Settings.NumSoundBuffers = cl.Parms[0].IntValue;
						Trace.WriteLine("ok");
					}
					else if (cl.Parms.Length > 0)
					{
						Trace.WriteLine("bad parm");
					}
					break;
				case "save":
					if (cl.CheckParms("s"))
					{
						try
						{
							string fn = GenerateOutFileName(Path.Combine(EMU7800App.Instance.Settings.OutputDirectory, cl.Parms[0].StrValue), ".emu");
							EMU7800App.Instance.M.Serialize(fn);
							Trace.Write("machine state saved to ");
							Trace.WriteLine(Path.GetFileName(fn));
						}
						catch (Exception ex)
						{
							Trace.WriteLine("error saving machine state: ");
							Trace.WriteLine(ex);
						}
					}
					else
					{
						Trace.WriteLine("bad parm");
					}
					break;
				case "restore":
					if (cl.CheckParms("s"))
					{
						try
						{
							string fn = Path.Combine(EMU7800App.Instance.Settings.OutputDirectory, cl.Parms[0].StrValue);
							EMU7800App.Instance.M = Machine.Deserialize(fn);
							Reset_lblGameTitle();
							StartButtonEnabled = false;
							ResumeButtonEnabled = true;
							CurrGameSettings = null;
							Trace.WriteLine("machine state restored");
						}
						catch (Exception ex)
						{
							Trace.WriteLine(ex);
						}
					}
					else
					{
						Trace.WriteLine("bad argument");
					}
					break;
				case "help":
				case "h":
				case "?":
					Trace.Write("** General Commands **\n"
						+ " ?: this help menu\n"
						+ " clear: clear log messages (64k limit)\n"
						+ " cpunop [on]|[off]: turn on/off cpu NOP register dumping\n"
						+ " disablemouse [on]|[off]: disable mouse input\n"
						+ " fps [ratedelta]: adj. frames per second\n"
						+ " gs [attribute] [value]: show/set game settings\n"
						+ " halt: stop running machine\n"
						+ " joybuttons <trigger#> <booster#>\n"
						+ " ls: show files in outdir\n"
						+ " cpuspin [millisecs]: per frame time cpu will spin wait for next frame\n"
						+ " opacity [25-100]\n"
						+ " outdir [newdir]: show/update output directory\n"
						+ " pb <filenm> [loop]: replay recording\n"
						+ " rec: start recording input\n"
						+ " restore <filenm>: restore machine state\n"
						+ " resume: resume stopped machine\n"
						+ " run: run ROM specified in GameSettings\n"
						+ " sb <num>: number of sound buffers\n"
						+ " save <filenm>: save machine state\n"
					);
					break;
				default:
					if (!recog)
					{
						Trace.WriteLine("Unrecognized command");
					}
					break;
			}
		}

		string GenerateOutFileName(string fn, string ext)
		{
			string stem = Path.Combine(Path.GetDirectoryName(fn),Path.GetFileNameWithoutExtension(fn));
			int i = 0;
			string u = "";
			string outFileName = "";
			while (true)
			{
				outFileName = stem + u + ext;
				if (!File.Exists(outFileName))
				{
					break;
				}
				u = (++i).ToString();
			}
			return outFileName;
		}

		void Record(string fn)
		{
			EMU7800App.Instance.Settings.Skip7800BIOS = true;
			cbxSkip7800BIOS.Checked = true;
			Trace.Write("recording to ");
			Trace.WriteLine(Path.GetFileName(fn));
			outBox_VisibleChanged(this, null);
			RecordInputAdapter ria = new RecordInputAdapter(fn, CurrGameSettings.MD5);
			Start(ria);
			ria.StopRecording();
		}

		void Playback(string fn, bool doLooping)
		{
			bool playbackLoopingEofSeen = false;
			EMU7800App.Instance.Settings.Skip7800BIOS = true;
			cbxSkip7800BIOS.Checked = true;
			while (true)
			{
				PlaybackInputAdapter pia = new PlaybackInputAdapter(fn);
				playbackLoopingEofSeen = false;
				if (doLooping)
				{
					pia.EofListeners += delegate()
					{
						EMU7800App.Instance.M.MachineHalt = true;
						playbackLoopingEofSeen = true;
					};
				}
				if (pia.CartMD5 == null)
				{
					Trace.WriteLine("error starting playback");
				}
				else
				{
					GameSettings gs = EMU7800App.Instance.ROMProperties.GetGameSettings(pia.CartMD5);
					if (gs == null || gs.FileInfo == null)
					{
						Trace.WriteLine("rom not found in current rom directory");
					}
					else
					{
						outBox_VisibleChanged(this, null);
						Reset_lblGameTitle();
						CurrGameSettings = gs;
						Start(pia);
						pia.StopPlayback();
					}
				}

				if (!doLooping || !playbackLoopingEofSeen)
				{
					break;
				}
			}
		}
	}
}
