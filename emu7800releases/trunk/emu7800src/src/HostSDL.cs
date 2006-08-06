/*
 * HostSdl
 * 
 * An Sdl-based host
 * 
 * Copyright (c) 2004-2006 Mike Murphy
 * 
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800
{
	public class HostSdl : Host
	{
		const int WIDTH = 320, HEIGHT = 240;
		Machine M;

		uint[] FrameBuffer;
		byte[] AudioBuffer;
		int EffectiveFPS, SoundSampleRate;
		int VisiblePitch;
		bool Fullscreen;
		int ClipStart, LeftOffset;
		int PanX, PanY;

		const int LIGHTGUN_LATENCY_CLKS = 25;  // 2600-specific, 7800 still needs to be determined

		int KeyboardPlayerNo;  // Player # that keyboard/mouse maps to

		int SdlJoystickSwapper;

		int MouseX, MouseY;
		bool ShowMouseCursor;	// For lightgun emulation
		bool DeactivateMouseInput;

		FontRenderer FontRenderer;
		long TextExpireFrameCount;
		int MaxTextMsgLen;
		string _TextMsg;
		string TextMsg
		{
			get
			{
				return _TextMsg;
			}
			set
			{
				MaxTextMsgLen = value.Length > MaxTextMsgLen ? value.Length : MaxTextMsgLen;
				_TextMsg = value.PadRight(MaxTextMsgLen, ' ');
				if (_TextMsg != null)
				{
					TextExpireFrameCount = M.FrameNumber + 3 * M.FrameHZ;
				}
			}
		}

		bool Quit, Paused, Mute;

		double FPS, RunMachineTPF, RenderFrameTPF, SleepDurationTPF;
		int RunMachineTicks, RenderFrameTicks, SleepDurationTicks, OtherTicks;
		int FrameSamples;

		public static readonly HostSdl Instance = new HostSdl();
		private HostSdl()
		{
			Fullscreen = true;
		}

		public override void Run(Machine m)
		{
			M = m;
			M.H = this;

			try
			{
				Trace.Write("Simple DirectMedia Layer ");
				Trace.Write(SdlNativeMethods.Version);
				Trace.WriteLine(" detected");

				VisiblePitch = M.VisiblePitch;
				ClipStart = M.FirstScanline;
				LeftOffset = 0;
				FrameBuffer = new uint[VisiblePitch * M.Scanlines];
				FontRenderer = new FontRenderer(FrameBuffer, VisiblePitch, M.Palette, 0);
				EffectiveFPS = M.FrameHZ + EMU7800App.Instance.Settings.FrameRateAdjust;
				SdlNativeMethods.Open(WIDTH, HEIGHT, Fullscreen, EMU7800App.Title);

				SoundSampleRate = M.SoundSampleRate * EffectiveFPS / M.FrameHZ;
				WinmmNativeMethods.Open(SoundSampleRate, M.Scanlines << 1, EMU7800App.Instance.Settings.NumSoundBuffers);
				Run();
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				SdlNativeMethods.Close();
				WinmmNativeMethods.Close();
			}
		}

		void Run()
		{
			Quit = false;
			Paused = false;
			Mute = false;
			_TextMsg = "";
			TextExpireFrameCount = 0;

			FrameSamples = EffectiveFPS << 1;
			RunMachineTicks = FrameSamples * 1000 / EffectiveFPS;

			KeyboardPlayerNo = 0;  // Keyboard/mouse defaults to player #1
			DeactivateMouseInput = EMU7800App.Instance.Settings.DeactivateMouseInput;
			SdlJoystickSwapper = 0;

			SdlNativeMethods.Quit += new SdlNativeMethods.QuitEventHandler(OnQuit);
			SdlNativeMethods.Keyboard += new SdlNativeMethods.KeyboardEventHandler(OnKeyboard);
			SdlNativeMethods.JoyButton += new SdlNativeMethods.JoyButtonEventHandler(OnJoyButton);
			SdlNativeMethods.JoyAxis += new SdlNativeMethods.JoyAxisEventHandler(OnJoyAxis);
			SdlNativeMethods.MouseButton += new SdlNativeMethods.MouseButtonEventHandler(OnMouseButton);
			SdlNativeMethods.MouseMotion += new SdlNativeMethods.MouseMotionEventHandler(OnMouseMotion);

			foreach (SdlJoystickDevice dev in SdlNativeMethods.Joysticks)
			{
				if (dev.Name != null && dev.Name.Contains("Stelladaptor"))
				{
					Trace.WriteLine("Stelladaptor detected: disabling mouse input");
					DeactivateMouseInput = true;
				}
			}

			int startOfFrameTick, endOfRunMachineTick, endOfRenderFrameTick;

			while (!Quit && !M.MachineHalt)
			{
				startOfFrameTick = SdlNativeMethods.GetTicks();

				if (M.FrameNumber % FrameSamples == 0)
				{
					FPS = 1000.0 * FrameSamples / (RunMachineTicks + RenderFrameTicks + OtherTicks);
					RunMachineTPF = RunMachineTicks;
					RunMachineTPF /= FrameSamples;
					RenderFrameTPF = RenderFrameTicks;
					RenderFrameTPF /= FrameSamples;
					SleepDurationTPF = SleepDurationTicks;
					SleepDurationTPF /= FrameSamples;
					RunMachineTicks = 0;
					RenderFrameTicks = 0;
					SleepDurationTicks = 0;
					OtherTicks = 0;
				}

				SdlNativeMethods.PollAndDelegate();

				if (!Paused)
				{
					M.Run();
				}

				endOfRunMachineTick = SdlNativeMethods.GetTicks();

				RenderFrame();

				endOfRenderFrameTick = SdlNativeMethods.GetTicks();

				while (WinmmNativeMethods.Enqueue(AudioBuffer) > 0)
				{
					int duration = SdlNativeMethods.GetTicks() - startOfFrameTick;
					if (duration > EMU7800App.Instance.Settings.CpuSpin)
					{
						System.Threading.Thread.Sleep(duration);
						SleepDurationTicks += duration;
					}
				}

				RunMachineTicks -= startOfFrameTick;
				RunMachineTicks += endOfRunMachineTick;
				RenderFrameTicks -= endOfRunMachineTick;
				RenderFrameTicks += endOfRenderFrameTick;
				OtherTicks -= endOfRenderFrameTick;
				OtherTicks += SdlNativeMethods.GetTicks();
			}
		}

		unsafe void RenderFrame()
		{
			if (PanX < 0)
			{
				LeftOffset++;
				SdlNativeMethods.FillRect(Color.Black);
				if (LeftOffset > 300)
				{
					LeftOffset = 300;
				}
			}
			if (PanX > 0)
			{
				LeftOffset--;
				SdlNativeMethods.FillRect(Color.Black);
				if (LeftOffset < -300)
				{
					LeftOffset = -300;
				}
			}
			if (PanY < 0)
			{
				ClipStart++;
				SdlNativeMethods.FillRect(Color.Black);
				if (ClipStart > 300)
				{
					ClipStart = 300;
				}
			}
			if (PanY > 0)
			{
				ClipStart--;
				SdlNativeMethods.FillRect(Color.Black);
				if (ClipStart < -300)
				{
					ClipStart = -300;
				}
			}

			if (TextExpireFrameCount > M.FrameNumber)
			{
				FontRenderer.DrawText(TextMsg, 5, ClipStart + 4, 10, 0);
			}

			SdlNativeMethods.Lock();
			uint* tptr = (uint*)SdlNativeMethods.Pixels.ToPointer();
			int si = ClipStart * VisiblePitch + LeftOffset;
			if (tptr != null)
			{
				uint* xptr = tptr;
				uint* yptr;

				if (VisiblePitch == (WIDTH >> 1))
				{
					for (int i = 0; i < HEIGHT; i++)
					{
						while (si < 0)
						{
							si += FrameBuffer.Length;
						}
						yptr = xptr;
						for (int j = 0; j < (WIDTH >> 1); j++)
						{
							while (si >= FrameBuffer.Length)
							{
								si -= FrameBuffer.Length;
							}
							*yptr++ = *yptr++ = FrameBuffer[si++];
						}
						xptr += (SdlNativeMethods.Pitch >> 2);
					}
				}
				else if (VisiblePitch == WIDTH)
				{
					for (int i = 0; i < HEIGHT; i++)
					{
						while (si < 0)
						{
							si += FrameBuffer.Length;
						}
						yptr = xptr;
						for (int j = 0; j < WIDTH; j++)
						{
							while (si >= FrameBuffer.Length)
							{
								si -= FrameBuffer.Length;
							}
							*yptr++ = FrameBuffer[si++];
						}
						xptr += (SdlNativeMethods.Pitch >> 2);
					}
				}

				if (ShowMouseCursor)
				{
					tptr[MouseY * WIDTH + MouseX] = 0xffffff;
				}
			}
			SdlNativeMethods.Unlock();
			//SdlNativeMethods.UpdateRect(0, 0, WIDTH, HEIGHT);
			SdlNativeMethods.Flip();
		}

		void OnQuit()
		{
			Quit = true;
		}

		void OnJoyButton(int deviceno, int button, bool down)
		{
			InputAdapter ia = M.InputAdapter;
			int playerno = deviceno ^ SdlJoystickSwapper;

			if (Paused)
			{
				UnPause();
			}
			else if (ia.Controllers[playerno] == Controller.Paddles)
			{
				if (DeactivateMouseInput && button <= 1)
				{
					// Map Stelladaptor joystick button events
					// to paddle triggers when necessary
					button ^= SdlNativeMethods.Joysticks[deviceno].PaddlesSwapper;
					playerno = (playerno << 1) | button;
					ia[playerno, ControllerAction.Trigger] = down;
				}
			}
			else if (button == EMU7800App.Instance.Settings.JoyBTrigger)
			{
				ia[playerno, ControllerAction.Trigger] = down;
			}
			else if (button == EMU7800App.Instance.Settings.JoyBBooster)
			{
				ia[playerno, ControllerAction.Trigger2] = down;
			}
		}

		void OnJoyAxis(int deviceno, int axis, int val)
		{
			InputAdapter ia = M.InputAdapter;
			int playerno = deviceno ^ SdlJoystickSwapper;

			if (Paused)
			{
				UnPause();
			}
			else if (ia.Controllers[playerno] == Controller.Paddles)
			{
				if (DeactivateMouseInput && axis <= 1)
				{
					// Map Stelladaptor joystick axis events
					// to paddle knobs when necessary
					axis ^= SdlNativeMethods.Joysticks[deviceno].PaddlesSwapper;
					playerno = (playerno << 1) | axis;
					SetPaddleOhms(playerno, 0xfeff, 0x8000 + val);
				}
			}
			else if (axis == 0)
			{
				ia[playerno, ControllerAction.Left] = val < -8192;
				ia[playerno, ControllerAction.Right] = val > 8192;
			}
			else if (axis == 1)
			{
				ia[playerno, ControllerAction.Up] = val < -8192;
				ia[playerno, ControllerAction.Down] = val > 8192;
			}
		}

		void OnKeyboard(int deviceno, bool down, int scancode, SdlKey key, SdlMod mod)
		{
			InputAdapter ia = M.InputAdapter;

            if (Paused)
            {
                switch (key)
                {
                    case SdlKey.K_LCTRL:
                    case SdlKey.K_RCTRL:
                    case SdlKey.K_LALT:
                    case SdlKey.K_RALT:
                    case SdlKey.K_LEFT:
                    case SdlKey.K_UP:
                    case SdlKey.K_RIGHT:
                    case SdlKey.K_DOWN:
                    case SdlKey.K_KP0:
                    case SdlKey.K_KP1:
                    case SdlKey.K_KP2:
                    case SdlKey.K_KP3:
                    case SdlKey.K_KP4:
                    case SdlKey.K_KP5:
                    case SdlKey.K_KP6:
                    case SdlKey.K_KP7:
                    case SdlKey.K_KP8:
                    case SdlKey.K_KP9:
                    case SdlKey.K_KP_MULTIPLY:
                    case SdlKey.K_KP_DIVIDE:
                        key = SdlKey.K_UNKNOWN;
                        break;
                }
            }

			switch (key)
			{
				case SdlKey.K_ESCAPE:
					Quit = true;
					break;
				case SdlKey.K_LCTRL:
				case SdlKey.K_RCTRL:
					ia[KeyboardPlayerNo, ControllerAction.Trigger] = down;
					break;
				case SdlKey.K_LALT:
				case SdlKey.K_RALT:
					ia[KeyboardPlayerNo, ControllerAction.Trigger2] = down;
					break;
				case SdlKey.K_LEFT:
					ia[KeyboardPlayerNo, ControllerAction.Left] = down;
					break;
				case SdlKey.K_UP:
					ia[KeyboardPlayerNo, ControllerAction.Up] = down;
					break;
				case SdlKey.K_RIGHT:
					ia[KeyboardPlayerNo, ControllerAction.Right] = down;
					break;
				case SdlKey.K_DOWN:
					ia[KeyboardPlayerNo, ControllerAction.Down] = down;
					break;
				case SdlKey.K_KP7:
					ia[KeyboardPlayerNo, ControllerAction.Keypad7] = down;
					break;
				case SdlKey.K_KP8:
					ia[KeyboardPlayerNo, ControllerAction.Keypad8] = down;
					break;
				case SdlKey.K_KP9:
					ia[KeyboardPlayerNo, ControllerAction.Keypad9] = down;
					break;
				case SdlKey.K_KP4:
					ia[KeyboardPlayerNo, ControllerAction.Keypad4] = down;
					break;
				case SdlKey.K_KP5:
					ia[KeyboardPlayerNo, ControllerAction.Keypad5] = down;
					break;
				case SdlKey.K_KP6:
					ia[KeyboardPlayerNo, ControllerAction.Keypad6] = down;
					break;
				case SdlKey.K_KP1:
					ia[KeyboardPlayerNo, ControllerAction.Keypad1] = down;
					break;
				case SdlKey.K_KP2:
					ia[KeyboardPlayerNo, ControllerAction.Keypad2] = down;
					break;
				case SdlKey.K_KP3:
					ia[KeyboardPlayerNo, ControllerAction.Keypad3] = down;
					break;
				case SdlKey.K_KP_MULTIPLY:
					ia[KeyboardPlayerNo, ControllerAction.KeypadA] = down;
					break;
				case SdlKey.K_KP0:
					ia[KeyboardPlayerNo, ControllerAction.Keypad0] = down;
					break;
				case SdlKey.K_KP_DIVIDE:
					ia[KeyboardPlayerNo, ControllerAction.KeypadP] = down;
					break;
				case SdlKey.K_1:
					if (down)
					{
						ia[ConsoleSwitch.LDifficultyA] = !ia[ConsoleSwitch.LDifficultyA];
						TextMsg = "Left Difficulty: "  + (ia[ConsoleSwitch.LDifficultyA] ? "A (Pro)" : "B (Novice)");
					}
					break;
				case SdlKey.K_2:
					if (down)
					{
						ia[ConsoleSwitch.RDifficultyA] = !ia[ConsoleSwitch.RDifficultyA];
						TextMsg = "Right Difficulty: " + (ia[ConsoleSwitch.RDifficultyA] ? "A (Pro)" : "B (Novice)");
					}
					break;
				case SdlKey.K_p:
					if (down && !Paused)
					{
						Paused = true;
						TextMsg = "Paused";
                        for (int i = 0; i < AudioBuffer.Length; i++)
                        {
                            AudioBuffer[i] = 0;
                        }
					}
					break;
				case SdlKey.K_F1:
					if (down)
					{
						SetKeyboardToPlayerNo(0);
					}
					break;
				case SdlKey.K_F2:
					if (down)
					{
						SetKeyboardToPlayerNo(1);
					}
					break;
				case SdlKey.K_F3:
					if (down)
					{
						SetKeyboardToPlayerNo(2);
					}
					break;
				case SdlKey.K_F4:
					if (down)
					{
						SetKeyboardToPlayerNo(3);
					}
					break;
				case SdlKey.K_F5:
					PanX = down ? -1 : 0;
					break;
				case SdlKey.K_F6:
					PanX = down ? 1 : 0;
					break;
				case SdlKey.K_F7:
					PanY = down ? -1 : 0;
					break;
				case SdlKey.K_F8:
					PanY = down ? 1 : 0;
					break;
				case SdlKey.K_F11:
					if (down)
					{
						string fn = String.Format("machinestate{0:yyyy}.{0:MM}-{0:dd}.{0:HH}{0:mm}{0:ss}.emu", DateTime.Now);
						try
						{
							M.Serialize(Path.Combine(EMU7800App.Instance.Settings.OutputDirectory, fn));
							Trace.WriteLine("machine state saved successfully.");
							TextMsg = "Machine State Saved";
						}
						catch (Exception e)
						{
							Trace.Write("machine state save error: ");
							Trace.WriteLine(e);
							TextMsg = "Error Saving Machine State";
						}
					}
					break;
				case SdlKey.K_F12:
					if (down)
					{
						string fn = String.Format("screenshot {0:yyyy} {0:MM}-{0:dd} {0:HH}{0:mm}{0:ss}.bmp", DateTime.Now);
						string rmsg = SdlNativeMethods.SaveBMP(Path.Combine(EMU7800App.Instance.Settings.OutputDirectory, fn));
						Trace.WriteLine(rmsg);
						TextMsg = "Screenshot taken";
					}
					break;
				case SdlKey.K_c:
					if (down)
					{
						ia[ConsoleSwitch.GameBW] = !ia[ConsoleSwitch.GameBW];
						TextMsg = ia[ConsoleSwitch.GameBW] ? "B/W" : "Color";
					}
					break;
				case SdlKey.K_f:
					if (down)
					{
						TextMsg = String.Format("{0}/{1} FPS {2:0.0} {3:0.0} {4:0.0} {5:0.0}", Math.Round(FPS, 0), EffectiveFPS, RunMachineTPF, RenderFrameTPF, SleepDurationTPF, 1000.0 / EffectiveFPS);
					}
					break;
				case SdlKey.K_m:
					if (down)
					{
						Mute = !Mute;
						TextMsg = Mute ? "Mute On" : "Mute Off";
					}
					break;
				case SdlKey.K_r:
					ia[ConsoleSwitch.GameReset] = down;
					break;
				case SdlKey.K_s:
					ia[ConsoleSwitch.GameSelect] = down;
					break;
				case SdlKey.K_q:
					if (down)
					{
						SwapPaddles(0);
					}
					break;
				case SdlKey.K_w:
					if (down)
					{
						SdlJoystickSwapper ^= 1;
						TextMsg = String.Format("P1/P2 Game Controllrs {0}wapped", SdlJoystickSwapper == 0 ? "Uns" : "S");
					}
					break;
				case SdlKey.K_e:
					if (down)
					{
						SwapPaddles(1);
					}
					break;
				default:
					if (down && Paused)
					{
						UnPause();
					}
					break;
			}
		}

		void OnMouseButton(int deviceno, bool down, SdlMouseButton button, int x, int y)
		{
            if (Paused)
            {
                UnPause();
            } else if (!DeactivateMouseInput)
			{
				M.InputAdapter[KeyboardPlayerNo, ControllerAction.Trigger] = down;
			}
		}

		void OnMouseMotion(bool down, int x, int y, int xrel, int yrel)
		{
			// Record last known mouse position for drawing things
			// like possible mouse cursors
			MouseX = x;
			MouseY = y;

			if (!DeactivateMouseInput)
			{
				if (KeyboardPlayerNo <= 1)
				{
					int scanline = (MouseY + ClipStart) % M.Scanlines;
					int hpos = MouseX * VisiblePitch / WIDTH + LIGHTGUN_LATENCY_CLKS;
					M.InputAdapter.SetLightgunPos(KeyboardPlayerNo, scanline, hpos);
				}
				SetPaddleOhms(KeyboardPlayerNo, WIDTH, MouseX);
			}
		}

        void UnPause()
        {
            TextMsg = "Resumed";
            Paused = false;
        }

        void SwapPaddles(int deviceno)
        {
            if (SdlNativeMethods.Joysticks[deviceno].Opened)
            {
                SdlNativeMethods.Joysticks[deviceno].PaddlesSwapper ^= 1;
                TextMsg = String.Format("P{0} Stelladptr Paddles {1}wapped",
                    deviceno + 1,
                    SdlNativeMethods.Joysticks[deviceno].PaddlesSwapper == 0 ? "Uns" : "S");
            }
        }

		void SetKeyboardToPlayerNo(int playerno)
		{
			ClearPlayerInput(KeyboardPlayerNo);
			KeyboardPlayerNo = playerno;
			ShowMouseCursor = (playerno <= 1 && M.InputAdapter.Controllers[playerno] == Controller.Lightgun);
			TextMsg = String.Format("Keyboard/Mouse to Player {0}", KeyboardPlayerNo + 1);
		}

		void SetPaddleOhms(int playerno, int val_max, int val)
		{
			int ohms = InputAdapter.PADDLEOHM_MAX
				 - (InputAdapter.PADDLEOHM_MAX - InputAdapter.PADDLEOHM_MIN)
				 / val_max * val;
			M.InputAdapter.SetOhms(playerno, ohms);
		}

		void ClearPlayerInput(int playerno)
		{
			InputAdapter ia = M.InputAdapter;
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

		public override void UpdateDisplay(byte[] buf, int scanline, int xstart, int len)
		{
			int i = scanline * VisiblePitch + xstart;
			int x = xstart;
			if (i + len < FrameBuffer.Length)
			{
				while (len-- > 0)
				{
					FrameBuffer[i++] = (uint)M.Palette[buf[x++]] | (uint)0xff000000;
				}
			}
		}

		public override void UpdateSound(byte[] buf)
		{
			if (Mute)
			{
				for (int i = 0; i < buf.Length; i++)
				{
					buf[i] = 0;
				}
			}
			AudioBuffer = buf;
		}
	}
}