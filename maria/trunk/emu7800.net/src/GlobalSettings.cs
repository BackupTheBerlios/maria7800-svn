/*
 * GlobalSettings
 *
 * Retains settings across program invocations.
 *
 * Copyright (c) 2004, 2005 Mike Murphy
 *
 */
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Microsoft.Win32;

namespace EMU7800
{
	public class GlobalSettings
	{
		string _RootDirectory;
		public string RootDirectory
		{
			get
			{
				return _RootDirectory;
			}
		}

		int _NumSoundBuffers;
		public int NumSoundBuffers
		{
			get
			{
				return _NumSoundBuffers;
			}
			set
			{
				_NumSoundBuffers = value;
			}
		}

		bool _DeactivateMouseInput;
		public bool DeactivateMouseInput
		{
			get
			{
				return _DeactivateMouseInput;
			}
			set
			{
				_DeactivateMouseInput = value;
			}
		}

		Size _ControlPanelFormSize;
		public Size ControlPanelFormSize
		{
			get
			{
				return _ControlPanelFormSize;
			}
			set
			{
				_ControlPanelFormSize = value;
			}
		}

		bool _NOPRegisterDumping;
		public bool NOPRegisterDumping
		{
			get
			{
				return _NOPRegisterDumping;
			}
			set
			{
				_NOPRegisterDumping = value;
			}
		}

		int _JoyBTrigger;
		public int JoyBTrigger
		{
			get
			{
				return _JoyBTrigger;
			}
			set
			{
				_JoyBTrigger = value;
			}
		}

		int _JoyBBooster;
		public int JoyBBooster
		{
			get
			{
				return _JoyBBooster;
			}
			set
			{
				_JoyBBooster = value;
			}
		}

		int _CpuSpin;
		public int CpuSpin
		{
			get
			{
				return _CpuSpin;
			}
			set
			{
				_CpuSpin = (value < 0) ? 0 : value;
			}
		}

		public GlobalSettings()
		{
			_RootDirectory = Directory.GetCurrentDirectory();
			Trace.Write("Root Directory: ");
			Trace.WriteLine(RootDirectory);

			_OutputDirectory = Path.Combine(RootDirectory, "outdir");
			if (!Directory.Exists(OutputDirectory))
			{
				_OutputDirectory = RootDirectory;
			}
			Trace.Write("Output Directory: ");
			Trace.WriteLine(OutputDirectory);

			ROMDirectory = GetRegistrySZ("ROMDirectory", Path.Combine(RootDirectory, "roms"));
			if (!Directory.Exists(ROMDirectory))
			{
				ROMDirectory = RootDirectory;
			}

			FrameRateAdjust = GetRegistryDWORD("FrameRateAdjust", 0);
			NumSoundBuffers = GetRegistryDWORD("NumSoundBuffers", 10);
			Skip7800BIOS = GetRegistryDWORD("Skip7800BIOS", 0) == 1;
			Use7800HSC = GetRegistryDWORD("Use7800HSC", 0) == 1;
			DeactivateMouseInput = GetRegistryDWORD("DeactivateMouseInput", 0) == 1;
			int w = GetRegistryDWORD("ControlPanelFormWidth", 0);
			int h = GetRegistryDWORD("ControlPanelFormHeight", 0);
			if (w < 500)
			{
				w = 500;
			}
			if (h < 500)
			{
				h = 500;
			}
			ControlPanelFormSize = new Size(w, h);
			NOPRegisterDumping = GetRegistryDWORD("NOPRegisterDumping", 0) == 1;
			JoyBTrigger = GetRegistryDWORD("JoyBTrigger", 0);
			JoyBBooster = GetRegistryDWORD("JoyBBooster", 1);
			CpuSpin = GetRegistryDWORD("CpuSpin", 1);
			SetRegistrySZ("Version", EMU7800App.Version);
		}

		int GetRegistryDWORD(string name, int defaultVal)
		{
			RegistryKey rk = GetSubKey();
			object obj = rk.GetValue(name);
			int val;
			if (obj == null)
			{
				val = defaultVal;
			}
			else
			{
				try
				{
					val = (int)obj;
				}
				catch
				{
					val = defaultVal;
				}
			}
			return val;
		}

		string GetRegistrySZ(string name, string defaultVal)
		{
			RegistryKey rk = GetSubKey();
			object obj = rk.GetValue(name);
			string val;
			if (obj == null)
			{
				val = defaultVal;
			}
			else
			{
				try
				{
					val = (string)obj;
				}
				catch
				{
					val = defaultVal;
				}
			}
			return val;
		}

		static RegistryKey GetSubKey()
		{
			string subkey = @"Software\EMU7800";
			RegistryKey rk = Registry.CurrentUser.OpenSubKey(subkey, true);
			if (rk == null)
			{
				rk = Registry.CurrentUser.CreateSubKey(subkey);
			}
			return rk;
		}
	}
}
