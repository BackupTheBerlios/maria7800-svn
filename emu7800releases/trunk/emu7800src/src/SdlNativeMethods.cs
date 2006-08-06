/*
 * SdlNativeMethods
 * 
 * Native methods et al. to the Simple Direct Media Layer (SDL)
 * 
 * Copyright (c) 2006 Mike Murphy
 * 
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800
{
	internal enum SdlKey
	{
		K_UNKNOWN = 0,
		K_FIRST = 0,
		K_BACKSPACE = 8,
		K_TAB = 9,
		K_CLEAR = 12,
		K_RETURN = 13,
		K_PAUSE = 19,
		K_ESCAPE = 27,
		K_SPACE = 32,
		K_EXCLAIM = 33,
		K_QUOTEDBL = 34,
		K_HASH = 35,
		K_DOLLAR = 36,
		K_AMPERSAND = 38,
		K_QUOTE = 39,
		K_LEFTPAREN = 40,
		K_RIGHTPAREN = 41,
		K_ASTERISK = 42,
		K_PLUS = 43,
		K_COMMA = 44,
		K_MINUS = 45,
		K_PERIOD = 46,
		K_SLASH = 47,
		K_0 = 48,
		K_1 = 49,
		K_2 = 50,
		K_3 = 51,
		K_4 = 52,
		K_5 = 53,
		K_6 = 54,
		K_7 = 55,
		K_8 = 56,
		K_9 = 57,
		K_COLON = 58,
		K_SEMICOLON = 59,
		K_LESS = 60,
		K_EQUALS = 61,
		K_GREATER = 62,
		K_QUESTION = 63,
		K_AT = 64,
		K_LEFTBRACKET = 91,
		K_BACKSLASH = 92,
		K_RIGHTBRACKET = 93,
		K_CARET = 94,
		K_UNDERSCORE = 95,
		K_BACKQUOTE = 96,
		K_a = 97,
		K_b = 98,
		K_c = 99,
		K_d = 100,
		K_e = 101,
		K_f = 102,
		K_g = 103,
		K_h = 104,
		K_i = 105,
		K_j = 106,
		K_k = 107,
		K_l = 108,
		K_m = 109,
		K_n = 110,
		K_o = 111,
		K_p = 112,
		K_q = 113,
		K_r = 114,
		K_s = 115,
		K_t = 116,
		K_u = 117,
		K_v = 118,
		K_w = 119,
		K_x = 120,
		K_y = 121,
		K_z = 122,
		K_DELETE = 127,
		K_KP0 = 256,
		K_KP1 = 257,
		K_KP2 = 258,
		K_KP3 = 259,
		K_KP4 = 260,
		K_KP5 = 261,
		K_KP6 = 262,
		K_KP7 = 263,
		K_KP8 = 264,
		K_KP9 = 265,
		K_KP_PERIOD = 266,
		K_KP_DIVIDE = 267,
		K_KP_MULTIPLY = 268,
		K_KP_MINUS = 269,
		K_KP_PLUS = 270,
		K_KP_ENTER = 271,
		K_KP_EQUALS = 272,
		K_UP = 273,
		K_DOWN = 274,
		K_RIGHT = 275,
		K_LEFT = 276,
		K_INSERT = 277,
		K_HOME = 278,
		K_END = 279,
		K_PAGEUP = 280,
		K_PAGEDOWN = 281,
		K_F1 = 282,
		K_F2 = 283,
		K_F3 = 284,
		K_F4 = 285,
		K_F5 = 286,
		K_F6 = 287,
		K_F7 = 288,
		K_F8 = 289,
		K_F9 = 290,
		K_F10 = 291,
		K_F11 = 292,
		K_F12 = 293,
		K_F13 = 294,
		K_F14 = 295,
		K_F15 = 296,
		K_NUMLOCK = 300,
		K_CAPSLOCK = 301,
		K_SCROLLOCK = 302,
		K_RSHIFT = 303,
		K_LSHIFT = 304,
		K_RCTRL = 305,
		K_LCTRL = 306,
		K_RALT = 307,
		K_LALT = 308,
		K_RMETA = 309,
		K_LMETA = 310,
		K_LSUPER = 311,
		K_RSUPER = 312,
		K_MODE = 313,
		K_COMPOSE = 314,
		K_HELP = 315,
		K_PRINT = 316,
		K_SYSREQ = 317,
		K_BREAK = 318,
		K_MENU = 319,
		K_POWER = 320,
		K_EURO = 321,
		K_UNDO = 322
	}

	internal enum SdlMod
	{
		None = 0x0000,
		LShift = 0x0001,
		RShift = 0x0002,
		LCtrl = 0x0040,
		RCtrl = 0x0080,
		LAlt = 0x0100,
		RAlt = 0x0200,
		LMeta = 0x0400,
		RMeta = 0x0800,
		Num = 0x1000,
		Caps = 0x2000,
		Mode = 0x4000
	}

	internal enum SdlMouseButton
	{
		Left = 1,
		Middle = 2,
		Right = 3,
		WheelUp = 4,
		WheelDown = 5
	}

	internal struct SdlJoystickDevice
	{
		internal bool Opened;		// Device opened?
		internal string Name;		// Name of the device
		internal IntPtr Device;		// SDL device handle
		internal int PaddlesSwapper;
	}

	internal unsafe sealed class SdlNativeMethods
	{
		static SDL_Surface* FrontSurface;

		internal delegate void KeyboardEventHandler(int deviceno, bool down, int scancode, SdlKey key, SdlMod mod);
		internal delegate void JoyButtonEventHandler(int deviceno, int button, bool down);
		internal delegate void JoyAxisEventHandler(int deviceno, int axis, int val);
		internal delegate void MouseButtonEventHandler(int deviceno, bool down, SdlMouseButton button, int x, int y);
		internal delegate void MouseMotionEventHandler(bool down, int x, int y, int xrel, int yrel);
		internal delegate void QuitEventHandler();

		internal static event KeyboardEventHandler Keyboard;
		internal static event JoyButtonEventHandler JoyButton;
		internal static event JoyAxisEventHandler JoyAxis;
		internal static event MouseButtonEventHandler MouseButton;
		internal static event MouseMotionEventHandler MouseMotion;
		internal static event QuitEventHandler Quit;

		enum Init
		{
			Timer = 0x00000001,
			Audio = 0x00000010,
			Video = 0x00000020,
			Cdrom = 0x00000100,
			Joystick = 0x00000200
		}

		enum Video
		{
			SWSurface = 0x00000000,
			HWSurface = 0x00000001,
			AsyncBlit = 0x00000004,
			AnyFormat = 0x10000000,
			HWPallete = 0x20000000,
			DoubleBuf = 0x40000000,
			FullScreen = -0x7FFFFFFF,
			OpenGL = 0x00000002,
			OpenGLBlit = 0x0000000A,
			Resizable = 0x00000010,
			NoFrame = 0x00000020,
			RLEAccel = 0x00004000
		}

		enum Event
		{
			NOEVENT = 0,
			ACTIVEEVENT,
			KEYDOWN,
			KEYUP,
			MOUSEMOTION,
			MOUSEBUTTONDOWN,
			MOUSEBUTTONUP,
			JOYAXISMOTION,
			JOYBALLMOTION,
			JOYHATMOTION,
			JOYBUTTONDOWN,
			JOYBUTTONUP,
			QUIT,
			SYSWMEVENT,
			EVENT_RESERVEDA,
			EVENT_RESERVEDB,
			VIDEORESIZE,
			VIDEOEXPOSE,
			EVENT_RESERVED2,
			EVENT_RESERVED3,
			EVENT_RESERVED4,
			EVENT_RESERVED5,
			EVENT_RESERVED6,
			EVENT_RESERVED7,
			USEREVENT = 24,
			NUMEVENTS = 32
		}

		enum Enable
		{
			Query = -1,
			Ignore = 0,
			Disable = 0,
			Enable = 1
		}

		enum State
		{
			Pressed = 1,
			Released = 0
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_Rect
		{
			internal SDL_Rect(System.Drawing.Rectangle r)
			{
				x = (short)r.X;
				y = (short)r.Y;
				w = (ushort)r.Width;
				h = (ushort)r.Height;
			}
			internal short x, y;
			internal ushort w, h;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_PixelFormat
		{
			internal IntPtr palette;
			internal byte BitsPerPixel;
			internal byte BytesPerPixel;
			internal byte Rloss;
			internal byte Gloss;
			internal byte Bloss;
			internal byte Aloss;
			internal byte Rshift;
			internal byte Gshift;
			internal byte Bshift;
			internal byte Ashift;
			internal uint Rmask;
			internal uint Gmask;
			internal uint Bmask;
			internal uint Amask;
			internal uint colorkey;
			internal byte alpha;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_Surface
		{
			internal uint flags;
			internal SDL_PixelFormat* format;
			internal int w, h;
			internal ushort pitch;
			internal IntPtr pixels;
			internal int offset;
			internal IntPtr hwdata;
			internal SDL_Rect clip_rect;
			internal uint unused1;
			internal uint locked;
			internal IntPtr map;
			internal uint format_version;
			internal int refcount;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_keysym
		{
			internal byte scancode;
			internal int sym;
			internal int mod;
			internal ushort unicode;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_Event
		{
			internal byte type;
			internal IntPtr d1;
			internal IntPtr d2;
			internal IntPtr d3;
			internal IntPtr d4;
			internal IntPtr d5;
			internal IntPtr d6;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_KeyboardEvent
		{
			internal byte type;
			internal byte which;
			internal byte state;
			internal SDL_keysym keysym;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_MouseMotionEvent
		{
			internal byte type;
			internal byte which;
			internal byte state;
			internal ushort x, y;
			internal short xrel;
			internal short yrel;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_MouseButtonEvent
		{
			internal byte type;
			internal byte which;
			internal byte button;
			internal byte state;
			internal ushort x, y;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_JoyAxisEvent
		{
			internal byte type;
			internal byte which;
			internal byte axis;
			internal short val;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_JoyButtonEvent
		{
			internal byte type;
			internal byte which;
			internal byte button;
			internal byte state;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		internal struct SDL_version
		{
			internal byte major;
			internal byte minor;
			internal byte patch;
		}

		internal static SdlJoystickDevice[] Joysticks = new SdlJoystickDevice[2];
		static bool IsSdlOpened = false;

		internal static bool Open(int w, int h, bool fullscreen, string caption)
		{
			if (IsSdlOpened)
			{
				AbormalTermination("SDL subsystem already open");
			}

			if (SDL_Init(0) == -1 || SDL_InitSubSystem((int)Init.Video) != 0)
			{
				AbormalTermination(SDL_GetError());
			}

			int flags = (int)(Video.HWSurface);
			if (fullscreen)
			{
				flags |= (int)(Video.FullScreen);
			}

			FrontSurface = SDL_SetVideoMode(w, h, 32, flags);
			if (FrontSurface == null)
			{
				AbormalTermination(SDL_GetError());
			}

			SDL_WM_SetCaption(caption, null);
			SDL_ShowCursor((int)Enable.Disable);

			if (SDL_InitSubSystem((int)Init.Joystick) != 0)
			{
				AbormalTermination(SDL_GetError());
			}

			OpenJoystickDevice(0);
			OpenJoystickDevice(1);

			SdlNativeMethods.FillRect(Color.Black);

			IsSdlOpened = true;
			return IsSdlOpened;
		}

		static void OpenJoystickDevice(int deviceno)
		{
			if (deviceno < SDL_NumJoysticks())
			{
				Joysticks[deviceno].Opened = true;
				Joysticks[deviceno].Name = SDL_JoystickName(deviceno);
				Joysticks[deviceno].Device = SDL_JoystickOpen(deviceno);
				Joysticks[deviceno].PaddlesSwapper = 0;

				Trace.Write("Opened joystick deviceno ");
				Trace.Write(deviceno);
				Trace.Write(": ");
				Trace.WriteLine(Joysticks[deviceno].Name);
			}
			else
			{
				Joysticks[deviceno].Opened = false;
				Joysticks[deviceno].Name = null;
			}
		}

		internal static int Flip()
		{
			return SDL_Flip(FrontSurface);
		}

		internal static void UpdateRect(int x, int y, int width, int height)
		{
			SDL_UpdateRect(FrontSurface, x, y, width, height);
		}

		internal static int Width
		{
			get
			{
				return (int)FrontSurface->w;
			}
		}

		internal static int Height
		{
			get
			{
				return (int)FrontSurface->h;
			}
		}

		internal static int Lock()
		{
			return MustLock ? SDL_LockSurface(FrontSurface) : 0;
		}

		internal static IntPtr Pixels
		{
			get
			{
				return FrontSurface->pixels;
			}
		}

		internal static int Unlock()
		{
			return MustLock ? SDL_UnlockSurface(FrontSurface) : 0;
		}

		internal static bool MustLock
		{
			get
			{
				return (FrontSurface->offset != 0 || ((FrontSurface->flags & (int)(Video.HWSurface | Video.AsyncBlit | Video.RLEAccel)) != 0));
			}
		}

		internal static string SaveBMP(string file)
		{
			string rmsg = "";

			if (SDL_SaveBMP_RW(FrontSurface, SDL_RWFromFile(file, "wb"), 1) < 0)
			{
				rmsg = "screenshot save failure: " + SDL_GetError();
			}
			else
			{
				rmsg = "screenshot saved successfully";
			}

			return rmsg;
		}

		internal static int BytesPerPixel
		{
			get
			{
				return FrontSurface->format->BytesPerPixel;
			}
		}

		internal static int Pitch
		{
			get
			{
				return FrontSurface->pitch;
			}
		}

		internal static int FillRect(Color color)
		{
			SDL_Rect sdlrect = new SDL_Rect(new Rectangle(0, 0, Width, Height));
			uint pf = SDL_MapRGBA(FrontSurface->format, color.R, color.G, color.B, color.A);
			return SDL_FillRect(FrontSurface, &sdlrect, pf);
		}

		internal static string Version
		{
			get
			{
				SDL_version ver = *SDL_Linked_Version();
				return String.Format("v{0}.{1}.{2}", ver.major, ver.minor, ver.patch);
			}
		}

		internal static void PollAndDelegate()
		{
			SDL_Event ev;

			while (true)
			{
				int ret = SDL_PollEvent(&ev);
				if (ret < 0)
				{
					AbormalTermination(SDL_GetError());
				}
				else if (ret == 0)
				{
					break;
				}
				switch ((Event)ev.type)
				{
					case Event.JOYBUTTONDOWN:
					case Event.JOYBUTTONUP:
						if (JoyButton != null)
						{
							SDL_JoyButtonEvent* jev = (SDL_JoyButtonEvent*)&ev;
							JoyButton(jev->which, jev->button, ev.type == (int)Event.JOYBUTTONDOWN);
						}
						break;
					case Event.JOYAXISMOTION:
						if (JoyAxis != null)
						{
							SDL_JoyAxisEvent* jev = (SDL_JoyAxisEvent*)&ev;
							JoyAxis(jev->which, jev->axis, jev->val);
						}
						break;
					case Event.KEYDOWN:
					case Event.KEYUP:
						if (Keyboard != null)
						{
							SDL_KeyboardEvent* kev = (SDL_KeyboardEvent*)&ev;
							Keyboard(kev->which,
								(int)kev->state == (int)State.Pressed,
								kev->keysym.scancode,
								(SdlKey)kev->keysym.sym,
								(SdlMod)kev->keysym.mod);
						}
						break;
					case Event.MOUSEBUTTONDOWN:
					case Event.MOUSEBUTTONUP:
						if (MouseButton != null)
						{
							SDL_MouseButtonEvent* mbev = (SDL_MouseButtonEvent*)&ev;
							MouseButton(mbev->which,
								(int)mbev->state == (int)State.Pressed,
								(SdlMouseButton)mbev->button,
								mbev->x, mbev->y);
						}
						break;
					case Event.MOUSEMOTION:
						if (MouseMotion != null)
						{
							SDL_MouseMotionEvent* mmev = (SDL_MouseMotionEvent*)&ev;
							MouseMotion((int)mmev->state == (int)State.Pressed,
								mmev->x, mmev->y, mmev->xrel, mmev->yrel);
						}
						break;
					case Event.QUIT:
						if (Quit != null)
						{
							Quit();
						}
						break;
					default:
						break;
				}
			}
		}

		internal static int GetTicks()
		{
			return (int)SdlNativeMethods.SDL_GetTicks();
		}

		internal static void Close()
		{
			Quit = null;
			Keyboard = null;
			JoyButton = null;
			JoyAxis = null;
			MouseButton = null;
			MouseMotion = null;

			for (int i=0; i < Joysticks.Length; i++)
			{
				if (Joysticks[i].Opened)
				{
					SDL_JoystickClose(Joysticks[i].Device);
					Joysticks[i].Opened = false;
				}
			}

			if (FrontSurface != null)
			{
				SDL_FreeSurface(FrontSurface);
			}
			SDL_Quit();

			IsSdlOpened = false;
		}

		static void AbormalTermination(string message)
		{
			Close();
			throw new Exception(message);
		}

		// General
		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_Init(int flags);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_InitSubSystem(int flags);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_Quit();

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern string SDL_GetError();

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern uint SDL_GetTicks();

		// Video
		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern SDL_Surface* SDL_SetVideoMode(int width, int height, int bpp, int flags);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern void SDL_FreeSurface(SDL_Surface* surface);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_Flip(SDL_Surface* screen);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_FillRect(SDL_Surface* surface, SDL_Rect* rect, uint color);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern uint SDL_MapRGBA(SDL_PixelFormat* fmt, byte r, byte g, byte b, byte a);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_ShowCursor(int toggle);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern SDL_Surface* SDL_CreateRGBSurface(int flags, int width, int height, int depth, uint Rmask, uint Gmask, uint Bmask, uint Amask);

		[DllImport("SDL", EntryPoint = "SDL_UpperBlit"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_BlitSurface(SDL_Surface* src, SDL_Rect* srcrect, SDL_Surface* dst, SDL_Rect* dstrect);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_LockSurface(SDL_Surface* surface);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_UnlockSurface(SDL_Surface* surface);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_SaveBMP_RW(SDL_Surface* surface, IntPtr dst, int freedst);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern SDL_Surface* SDL_ConvertSurface(SDL_Surface* src, SDL_PixelFormat* fmt, int flags);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern void SDL_UpdateRect(SDL_Surface* surface, int x, int y, int width, int height);

		// RWops
		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern IntPtr SDL_RWFromFile(string file, string mode);

		// Events
		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_PollEvent(SDL_Event* evt);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_WaitEvent(SDL_Event* evt);

		// WM
		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern void SDL_WM_SetCaption(string title, string icon);

		// Joystick
		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_NumJoysticks();

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern string SDL_JoystickName(int index);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern IntPtr SDL_JoystickOpen(int index);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_JoystickOpened(int index);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_JoystickIndex(IntPtr joystick);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_JoystickNumAxes(IntPtr joystick);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_JoystickNumBalls(IntPtr joystick);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_JoystickNumHats(IntPtr joystick);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern int SDL_JoystickNumButtons(IntPtr joystick);

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern void SDL_JoystickClose(IntPtr joystick);

		// Audio
		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern void SDL_CloseAudio();

		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern void SDL_PauseAudio(int pause_on);

		// SDL Version
		[DllImport("SDL"), SuppressUnmanagedCodeSecurity]
		static extern SDL_version* SDL_Linked_Version();
	}
}