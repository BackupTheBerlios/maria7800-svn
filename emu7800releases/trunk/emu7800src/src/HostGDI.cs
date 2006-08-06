/*
 * HostGDI
 *
 * A GDI-based Host. 
 *
 * Copyright (c) 2003-2006 Mike Murphy
 *
 */
using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Reflection;
using System.Windows.Forms;

namespace EMU7800
{
	public class HostGDIForm : Form
	{
		Machine M;
		Rectangulator Rectangulator;
		bool ReqRefresh;
		uint[] FrameBuffer;
		byte[] AudioBuffer;

		bool Paused, Mute;

		double FPS;
		int RunEmuTicks, SleepDurationTicks, OtherTicks;
		double RunEmuTPF, SleepDurationTPF;
		int FrameSamples;

		string _TextMsg;
		string TextMsg 
		{
			get
			{
				return _TextMsg;
			}
			set
			{
				_TextMsg = value;
				if (_TextMsg != null)
				{
					TextExpireFrameCount = M.FrameNumber + 3*M.FrameHZ;
				}
				ClearTextMsg();
				ShowTextMsg();
			}
		}
		long TextExpireFrameCount;
		
		static Point SavedLocation = new Point(-1, -1);
		Font TextFont;
		SolidBrush TextBrush;
		Graphics RGraphics;
		SolidBrush SBrush = new SolidBrush(Color.Black);
		Bitmap FrameBitmap;

		int KeyboardPlayerNo;
		int[] PaddleOhms = new int[4];
		int[] PaddleVels = new int[4];

		int VisiblePitch, EffectiveFPS, SoundSampleRate, NumSoundBuffers;

		public HostGDIForm() { }

		public void Run(Machine m, Host h)
		{
			M = m;
			M.H = h;

			VisiblePitch = M.VisiblePitch;
			NumSoundBuffers = EMU7800App.Instance.Settings.NumSoundBuffers;
			EffectiveFPS = M.FrameHZ + EMU7800App.Instance.Settings.FrameRateAdjust;
			SoundSampleRate = M.SoundSampleRate*EffectiveFPS/M.FrameHZ;

			FrameSamples = EffectiveFPS << 1;

			Rectangulator r = new Rectangulator(VisiblePitch, M.Scanlines);
			r.UpdateRect += new UpdateRectHandler(OnUpdateRect);
			r.Palette = M.Palette;
			r.PixelAspectXRatio = 320/VisiblePitch;
			r.OffsetLeft = 0;
			r.ClipTop = M.FirstScanline;
			r.ClipHeight = 240;
			r.UpdateTransformationParameters();
			Rectangulator = r;
			FrameBitmap = null;
			FrameBuffer = null;

			Text = EMU7800App.Title;
			Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("EMU7800.Icon1.ico"));

			ShowInTaskbar = true;
			FormBorderStyle = FormBorderStyle.Sizable;
			CenterToScreen();

			ClientSize = new Size(640, 480);
			MinimumSize = new Size(320 + 8, 240 + 27);
		
			// Enable double-buffering to avoid flicker
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.DoubleBuffer, true);

			TextFont = new Font("Courier New", 18);
			TextBrush = new SolidBrush(Color.White);

			Paint += new PaintEventHandler(OnPaint);
			Layout += new LayoutEventHandler(OnLayout);
			LocationChanged += new EventHandler(OnLocationChanged);
		
			KeyDown += new KeyEventHandler(OnKeyDown);
			KeyUp   += new KeyEventHandler(OnKeyUp);
			KeyPreview = true;

			if (SavedLocation.X >= 0)
			{
				Location = SavedLocation;
			}

			Show();

			Paused = false;
			ReqRefresh = true;

			KeyboardPlayerNo = 0;

    		WinmmNativeMethods.Open(SoundSampleRate, M.Scanlines << 1, NumSoundBuffers);
            Run();
		}

		void Run()
		{
			int doEventsCounter = 0;
			int startOfFrameTick, endOfRunEmuTick;

			while (true)
			{
				startOfFrameTick = Environment.TickCount;

				if (M.FrameNumber % FrameSamples == 0)
				{
					FPS = 1000.0 * FrameSamples / (RunEmuTicks + OtherTicks);
					RunEmuTPF = RunEmuTicks;
					RunEmuTPF /= FrameSamples;
					SleepDurationTPF = SleepDurationTicks;
					SleepDurationTPF /= FrameSamples;
					RunEmuTicks = 0;
					SleepDurationTicks = 0;
					OtherTicks = 0;
				}

				if (Created && --doEventsCounter <= 0)
				{
					Application.DoEvents();
					doEventsCounter = 3;
				}

                if (!Created)
                {
                    break;
                }

				if (M.MachineHalt)
				{
					Close();
				}

				if (ReqRefresh)
				{
					if (Rectangulator != null)
					{
						RGraphics.Clear(Color.Black);
						Rectangulator.DrawEntireFrame(Paused);
						ShowTextMsg();
					}
					ReqRefresh = false;
				}

				UpdatePaddles();

				if (!Paused)
				{
					if (Rectangulator != null)
					{
						Rectangulator.StartFrame();
						M.Run();
						Rectangulator.EndFrame();
					}
					else
					{
						M.Run();
						OnUpdateFrame(FrameBuffer);
					}
				}

				if (TextExpireFrameCount <= M.FrameNumber && TextMsg != null)
				{
					ClearTextMsg();
					TextMsg = null;
				}

				endOfRunEmuTick = Environment.TickCount;

				while (WinmmNativeMethods.Enqueue(AudioBuffer) > 0)
				{
					int duration = Environment.TickCount - startOfFrameTick;
					if (duration > EMU7800App.Instance.Settings.CpuSpin)
					{
						System.Threading.Thread.Sleep(duration);
						SleepDurationTicks += duration;
					}	
				}

				RunEmuTicks -= startOfFrameTick;
				RunEmuTicks += endOfRunEmuTick;
				OtherTicks -= endOfRunEmuTick;
				OtherTicks += Environment.TickCount;
			}
		}

		void ClearPlayerInput(int playerno)
		{
			InputAdapter ia = M.InputAdapter;

			PaddleVels[playerno] = 0;
			ia[playerno, ControllerAction.Trigger] = false;
			ia[playerno, ControllerAction.Trigger2] = false;
			ia[playerno, ControllerAction.Left] = false;
			ia[playerno, ControllerAction.Up] = false;
			ia[playerno, ControllerAction.Right] = false;
			ia[playerno, ControllerAction.Down] = false;
			ia[playerno, ControllerAction.Keypad7] = false;
			ia[playerno, ControllerAction.Keypad8] = false;
			ia[playerno, ControllerAction.Keypad9] = false;
			ia[playerno, ControllerAction.Keypad4] = false;
			ia[playerno, ControllerAction.Keypad5] = false;
			ia[playerno, ControllerAction.Keypad6] = false;
			ia[playerno, ControllerAction.Keypad1] = false;
			ia[playerno, ControllerAction.Keypad2] = false;
			ia[playerno, ControllerAction.Keypad3] = false;
			ia[playerno, ControllerAction.KeypadA] = false;
			ia[playerno, ControllerAction.Keypad0] = false;
			ia[playerno, ControllerAction.KeypadP] = false;
		}

		void OnKeyDown(Object sender, KeyEventArgs e)
		{
			OnKeyPress(e, true);
		}
	
		void OnKeyUp(Object sender, KeyEventArgs e)
		{
			OnKeyPress(e, false);
		}

		void OnKeyPress(KeyEventArgs e, bool down)
		{
			InputAdapter ia = M.InputAdapter;
			Keys key = e.KeyCode;

			e.Handled = true;

			if (Paused)
			{
				switch (key)
				{
					case Keys.ControlKey:
					case Keys.Menu:
					case Keys.Left:
					case Keys.Right:
					case Keys.Up:
					case Keys.Down:
					case Keys.NumPad0:
					case Keys.NumPad1:
					case Keys.NumPad2:
					case Keys.NumPad3:
					case Keys.NumPad4:
					case Keys.NumPad5:
					case Keys.NumPad6:
					case Keys.NumPad7:
					case Keys.NumPad8:
					case Keys.NumPad9:
					case Keys.Multiply:
					case Keys.Divide:
						key = Keys.Space;
						break;
				}
			}

			switch (key)
			{
				case Keys.Escape:
					if (down)
					{
						Close();
					}
					break;
				case Keys.P:
					if (down && !Paused)
					{
						TextMsg = "Paused";
						Paused = true;
						ReqRefresh = true;
                        for (int i = 0; i < AudioBuffer.Length; i++)
                        {
                            AudioBuffer[i] = 0;
                        }
					}
					break;
				case Keys.ControlKey:
					ia[KeyboardPlayerNo, ControllerAction.Trigger] = down;
					break;
				case Keys.Menu:  // Alt key
					ia[KeyboardPlayerNo, ControllerAction.Trigger2] = down;
					break;
				case Keys.Left:
					ia[KeyboardPlayerNo, ControllerAction.Left] = down;
					if (down)
					{
						ia[KeyboardPlayerNo, ControllerAction.Right] = false;
					}
					PaddleVels[KeyboardPlayerNo] += down ? 1 : -1;
					if (PaddleVels[KeyboardPlayerNo] != 0)
					{
						PaddleVels[KeyboardPlayerNo] /= Math.Abs(PaddleVels[KeyboardPlayerNo]);
					}
					break;
				case Keys.Up:
					ia[KeyboardPlayerNo, ControllerAction.Up] = down;
					if (down)
					{
						ia[KeyboardPlayerNo, ControllerAction.Down] = false;
					}
					break;
				case Keys.Right:
					ia[KeyboardPlayerNo, ControllerAction.Right] = down;
					if (down)
					{
						ia[KeyboardPlayerNo, ControllerAction.Left] = false;
					}
					PaddleVels[KeyboardPlayerNo] += down ? -1 : 1;
					if (PaddleVels[KeyboardPlayerNo] != 0)
					{
						PaddleVels[KeyboardPlayerNo] /= Math.Abs(PaddleVels[KeyboardPlayerNo]);
					}
					break;
				case Keys.Down:
					ia[KeyboardPlayerNo, ControllerAction.Down] = down;
					if (down)
					{
						ia[KeyboardPlayerNo, ControllerAction.Up] = false;
					}
					break;
				case Keys.NumPad7:
					ia[KeyboardPlayerNo, ControllerAction.Keypad7] = down;
					break;
				case Keys.NumPad8:
					ia[KeyboardPlayerNo, ControllerAction.Keypad8] = down;
					break;
				case Keys.NumPad9:
					ia[KeyboardPlayerNo, ControllerAction.Keypad9] = down;
					break;
				case Keys.NumPad4:
					ia[KeyboardPlayerNo, ControllerAction.Keypad4] = down;
					break;
				case Keys.NumPad5:
					ia[KeyboardPlayerNo, ControllerAction.Keypad5] = down;
					break;
				case Keys.NumPad6:
					ia[KeyboardPlayerNo, ControllerAction.Keypad6] = down;
					break;
				case Keys.NumPad1:
					ia[KeyboardPlayerNo, ControllerAction.Keypad1] = down;
					break;
				case Keys.NumPad2:
					ia[KeyboardPlayerNo, ControllerAction.Keypad2] = down;
					break;
				case Keys.NumPad3:
					ia[KeyboardPlayerNo, ControllerAction.Keypad3] = down;
					break;
				case Keys.Multiply:
					ia[KeyboardPlayerNo, ControllerAction.KeypadA] = down;
					break;
				case Keys.NumPad0:
					ia[KeyboardPlayerNo, ControllerAction.Keypad0] = down;
					break;
				case Keys.Divide:
					ia[KeyboardPlayerNo, ControllerAction.KeypadP] = down;
					break;
				case Keys.R:
					ia[ConsoleSwitch.GameReset] = down;
					break;
				case Keys.S:
					ia[ConsoleSwitch.GameSelect] = down;
					break;
				case Keys.C:
					if (down)
					{
						ia[ConsoleSwitch.GameBW] = !ia[ConsoleSwitch.GameBW];
						TextMsg = ia[ConsoleSwitch.GameBW] ? "B/W" : "Color";
					}
					break;
				case Keys.D1:
					if (down)
					{
						ia[ConsoleSwitch.LDifficultyA] = !ia[ConsoleSwitch.LDifficultyA];
						TextMsg = "Left Difficulty: " + (ia[ConsoleSwitch.LDifficultyA] ? "A (Pro)" : "B (Novice)");
					}
					break;
				case Keys.D2:
					if (down)
					{
						ia[ConsoleSwitch.RDifficultyA] = !ia[ConsoleSwitch.RDifficultyA];
						TextMsg = "Right Difficulty: " + (ia[ConsoleSwitch.RDifficultyA] ? "A (Pro)" : "B (Novice)");
					}
					break;
				case Keys.F1:
					if (down)
					{
						SetKeyboardToPlayerNo(0);
					}
					break;
				case Keys.F2:
					if (down)
					{
						SetKeyboardToPlayerNo(1);
					}
					break;
				case Keys.F3:
					if (down)
					{
						SetKeyboardToPlayerNo(2);
					}
					break;
				case Keys.F4:
					if (down)
					{
						SetKeyboardToPlayerNo(3);
					}
					break;
				case Keys.F:
					if (down)
					{
						TextMsg = String.Format("{0}/{1} FPS  {2:0.0} {3:0.0} {4:0.0} ms", Math.Round(FPS, 0), EffectiveFPS, RunEmuTPF, SleepDurationTPF, 1000.0 / EffectiveFPS);
					}
					break;
				case Keys.M:
					if (down)
					{
						Mute = !Mute;
						TextMsg = Mute ? "Mute" : "Mute Off";
					}
					break;
				case Keys.F5:
					if (down && Rectangulator != null)
					{
						Rectangulator.OffsetLeft++;
						Rectangulator.UpdateTransformationParameters();
						ReqRefresh = true;
					}
					break;
				case Keys.F6:
					if (down && Rectangulator != null)
					{
						Rectangulator.OffsetLeft--;
						Rectangulator.UpdateTransformationParameters();
						ReqRefresh = true;
					}
					break;
				case Keys.F7:
					if (down && Rectangulator != null)
					{
						Rectangulator.ClipTop++;
						Rectangulator.UpdateTransformationParameters();
						ReqRefresh = true;
					}
					break;
				case Keys.F8:
					if (down && Rectangulator != null)
					{
						Rectangulator.ClipTop--;
						Rectangulator.UpdateTransformationParameters();
						ReqRefresh = true;
					}
					break;
				default:
					if (down && Paused)
					{
						Paused = false;
						ReqRefresh = true;
						TextMsg = "Resumed";
					}
					break;
			}
		}

		void SetKeyboardToPlayerNo(int playerno)
		{
			ClearPlayerInput(KeyboardPlayerNo);
			KeyboardPlayerNo = playerno;
			TextMsg = String.Format("Keyboard to Player {0}", KeyboardPlayerNo + 1);
		}

		void UpdatePaddles()
		{
			for (int i=0; i < 4; i++)
			{
				int ohms = PaddleOhms[i];
				ohms += 16384*PaddleVels[i];
				if (ohms < InputAdapter.PADDLEOHM_MIN)
					ohms = InputAdapter.PADDLEOHM_MIN;
				if (ohms > InputAdapter.PADDLEOHM_MAX)
					ohms = InputAdapter.PADDLEOHM_MAX;
				PaddleOhms[i] = ohms;
				M.InputAdapter.SetOhms(i, ohms);
			}
		}

		void ShowTextMsg()
		{
			TextBrush.Color = Color.White;
			RGraphics = CreateGraphics();
			RGraphics.TextRenderingHint = TextRenderingHint.SystemDefault;
			RGraphics.DrawString(TextMsg, TextFont, TextBrush, 0, 0);
		}

		void ClearTextMsg()
		{
			TextBrush.Color = Color.Black;
			RGraphics.FillRectangle(TextBrush, 0, 0, ClientSize.Width, 30);
		}

		void OnLayout(object sender, LayoutEventArgs e)
		{
			RGraphics = CreateGraphics();
			RGraphics.CompositingMode = CompositingMode.SourceCopy;
			RGraphics.CompositingQuality = CompositingQuality.Invalid;
			RGraphics.SmoothingMode = SmoothingMode.None;
			RGraphics.InterpolationMode = InterpolationMode.NearestNeighbor;

			if (Rectangulator != null)
			{
				Rectangulator.ViewPortSize = ClientSize;
				Rectangulator.UpdateTransformationParameters();
			}
			ReqRefresh = true;
		}

		void OnLocationChanged(object sender, EventArgs e)
		{
			SavedLocation = Location;
		}

		void OnPaint(object sender, PaintEventArgs e)
		{
			ReqRefresh = true;
		}

		public void UpdateDisplay(byte[] buf, int scanline, int xstart, int len)
		{
			if (Rectangulator != null)
			{
				Rectangulator.InputScanline(buf, scanline, xstart, len);
			} 
			else 
			{
				int i = scanline*VisiblePitch + xstart;
				int x = xstart;
				while (len-- > 0) 
				{
					FrameBuffer[i++] = (uint)M.Palette[buf[x++]] | (uint)0xff000000;
				}
			}
		}

		void OnUpdateRect(DisplRect r)
		{
			SBrush.Color = Color.FromArgb(r.Argb);
			RGraphics.FillRectangle(SBrush, r.Rectangle);
		}

		void OnUpdateFrame(uint[] fb)
		{
			BitmapData d = FrameBitmap.LockBits(
				new Rectangle(new Point(), FrameBitmap.Size),
				ImageLockMode.WriteOnly,
				FrameBitmap.PixelFormat);
			unsafe
			{
				uint *tptr = (uint *)d.Scan0.ToPointer();
				for (int i=0; i < fb.Length; i++) 
				{
					*tptr++ = fb[i];
				}
			}
			FrameBitmap.UnlockBits(d);

			RGraphics.DrawImage(FrameBitmap, 0, 0, 320, 240);
		}

		public void UpdateSound(byte[] buf)
		{
			if (Mute)
			{
				for (int i=0; i < buf.Length; i++)
				{
					buf[i] = 0;
				}
			}

			AudioBuffer = buf;
		}
	}

    public class HostGDI : Host
    {
        HostGDIForm F;

        public static readonly HostGDI Instance = new HostGDI();
        private HostGDI() { }

        public override void Run(Machine m)
        {
            F = new HostGDIForm();
            F.Run(m, this);
        }
        public override void UpdateDisplay(byte[] buf, int scanline, int start, int len)
        {
            F.UpdateDisplay(buf, scanline, start, len);
        }
        public override void UpdateSound(byte[] buf)
        {
            F.UpdateSound(buf);
        }
    }
}