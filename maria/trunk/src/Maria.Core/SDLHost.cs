/*
 * SDL-based host
 *
 * Copyright (C) 2004-2006 Mike Murphy
 * Copyright (C) 2006 Thomas Mathys  (tom42@users.berlios.de)
 *
 * This file is part of Maria.
 *
 * Maria is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Maria is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Maria; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */
using System;
using System.Threading;
using Tao.Sdl;

namespace Maria.Core {
	public class SDLHost : IHost {
		private const int WIDTH = 320;
		private const int HEIGHT = 240;
		private const bool FULLSCREEN = false;
		private const int LIGHTGUN_LATENCY_CLKS = 25; // 2600-specific, 7800 still needs to be determined
		private Machine machine;
		private uint[] FrameBuffer;
		private byte[] AudioBuffer;
		private int EffectiveFPS;
		private int SoundSampleRate;
		private int VisiblePitch;
		private int ClipStart;
		private int LeftOffset;
		private int PanX;
		private int PanY;
		private int KeyboardPlayerNo; // Player # that keyboard/mouse maps to
		private int SdlJoystickSwapper;
		private int MouseX, MouseY;
		private bool ShowMouseCursor; // For lightgun emulation
		private bool DeactivateMouseInput;
		private FontRenderer FontRenderer;
		private long TextExpireFrameCount;
		private int MaxTextMsgLen;
		private string textMsg;
		private bool Quit, Paused, Mute;
		private double FPS, RunMachineTPF, RenderFrameTPF, SleepDurationTPF;
		private int RunMachineTicks, RenderFrameTicks, SleepDurationTicks, OtherTicks;
		private int FrameSamples;

		string TextMsg {
			get { return textMsg; }
			set {
				MaxTextMsgLen = value.Length > MaxTextMsgLen ? value.Length : MaxTextMsgLen;
				textMsg = value.PadRight(MaxTextMsgLen, ' ');
				if (textMsg != null) {
					TextExpireFrameCount = machine.FrameNumber + 3 * machine.FrameHZ;
				}
			}
		}

		public void Run(Machine m) {
			machine = m;
			machine.Host = this;
			try {
				VisiblePitch = machine.VisiblePitch;
				ClipStart = machine.FirstScanline;
				LeftOffset = 0;
				FrameBuffer = new uint[VisiblePitch * machine.Scanlines];
				FontRenderer = new FontRenderer(FrameBuffer, VisiblePitch, machine.Palette, 0);
				EffectiveFPS = machine.FrameHZ + EMU7800App.Instance.Settings.FrameRateAdjust;
				if (Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO | Sdl.SDL_INIT_AUDIO | Sdl.SDL_INIT_JOYSTICK) < 0) {
					throw new MariaCoreException("Couldn't initialize SDL: " + Sdl.SDL_GetError());
				}
				int flags = Sdl.SDL_HWSURFACE;
				if (FULLSCREEN)
					flags |= Sdl.SDL_FULLSCREEN;
				if (IntPtr.Zero == Sdl.SDL_SetVideoMode(WIDTH, HEIGHT, 32, flags)) {
					throw new MariaCoreException("Couldn't set video mode: " + Sdl.SDL_GetError());
				}
				Sdl.SDL_WM_SetCaption("Maria", "Maria"); // TODO : unhardcode ?
				Sdl.SDL_ShowCursor(FULLSCREEN ? Sdl.SDL_DISABLE : Sdl.SDL_ENABLE);

				// TODO : SdlNativeMethods.Open does some more shit...:
				//OpenJoystickDevice(0);
				//OpenJoystickDevice(1);
				//SdlNativeMethods.FillRect(Color.Black);

				// TODO : need to initialize sound somehow...
				/*SoundSampleRate = M.SoundSampleRate * EffectiveFPS / M.FrameHZ;
				WinmmNativeMethods.Open(
					SoundSampleRate,
					M.Scanlines << 1,
					EMU7800App.Instance.Settings.NumSoundBuffers
				);*/

				Run();
			}
			finally {
				// TODO : also clean up sound, joystick, whatnot.
				Sdl.SDL_Quit();
			}
		}

		public void UpdateDisplay(byte[] buf, int scanline, int xstart, int len) {
			int i = scanline * VisiblePitch + xstart;
			int x = xstart;
			if (i + len < FrameBuffer.Length) {
				while (len-- > 0) {
					FrameBuffer[i++] = (uint) machine.Palette[buf[x++]] | (uint)0xff000000;
				}
			}
		}

		public void UpdateSound(byte[] buf) {
			if (Mute) {
				for (int i = 0; i < buf.Length; i++) {
					buf[i] = 0;
				}
			}
			AudioBuffer = buf;
		}

		private void Run() {
			Quit = false;
			Paused = false;
			Mute = false;
			textMsg = "";
			TextExpireFrameCount = 0;
			FrameSamples = EffectiveFPS << 1;
			RunMachineTicks = FrameSamples * 1000 / EffectiveFPS;
			KeyboardPlayerNo = 0; // Keyboard/mouse defaults to player #1
			DeactivateMouseInput = EMU7800App.Instance.Settings.DeactivateMouseInput;
			SdlJoystickSwapper = 0;

			// TODO : this can be removed. we do the event handling business all in one class
//			SdlNativeMethods.Quit += new SdlNativeMethods.QuitEventHandler(OnQuit);
//			SdlNativeMethods.Keyboard += new SdlNativeMethods.KeyboardEventHandler(OnKeyboard);
//			SdlNativeMethods.JoyButton += new //SdlNativeMethods.JoyButtonEventHandler(OnJoyButton);
//			SdlNativeMethods.JoyAxis += new SdlNativeMethods.JoyAxisEventHandler(OnJoyAxis);
//			SdlNativeMethods.MouseButton += new //SdlNativeMethods.MouseButtonEventHandler(OnMouseButton);
//			SdlNativeMethods.MouseMotion += new //SdlNativeMethods.MouseMotionEventHandler(OnMouseMotion);

			// TODO : enable for joystick/stelladaptor support. or whatever it's good for.
			/*foreach (SdlJoystickDevice dev in SdlNativeMethods.Joysticks) {
				if (dev.Name != null && dev.Name.Contains("Stelladaptor")) {
					Trace.WriteLine("Stelladaptor detected: disabling mouse input");
					DeactivateMouseInput = true;
				}
			}*/

			int startOfFrameTick, endOfRunMachineTick, endOfRenderFrameTick;

		}
		// TODO : continue implementing the nonsense below...
/*
		void Run() {
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
*/
		private void SetKeyboardToPlayerNo(int playerno) {
			ClearPlayerInput(KeyboardPlayerNo);
			KeyboardPlayerNo = playerno;
			ShowMouseCursor = (playerno <= 1 && machine.InputAdapter.Controllers[playerno] == Controller.Lightgun);
			TextMsg = String.Format("Keyboard/Mouse to Player {0}", KeyboardPlayerNo + 1);
		}

		private void SetPaddleOhms(int playerno, int val_max, int val) {
			int ohms = InputAdapter.PADDLEOHM_MAX -
				(InputAdapter.PADDLEOHM_MAX - InputAdapter.PADDLEOHM_MIN) / val_max * val;
			machine.InputAdapter.SetOhms(playerno, ohms);
		}

		private void ClearPlayerInput(int playerno) {
			InputAdapter ia = machine.InputAdapter;
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
	}
}

// TODO : enable the stuff below...
/*
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

	}
}
*/
