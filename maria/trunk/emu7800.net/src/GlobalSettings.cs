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
	public enum HostType { GDI = 0, SDL = 1 };

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

		string _OutputDirectory;
		public string OutputDirectory
		{
			get
			{
				return _OutputDirectory;
			}
		}

		string _ROMDirectory;
		public string ROMDirectory
		{
			get
			{
				return _ROMDirectory;
			}
			set
			{
				_ROMDirectory = value;
				Trace.Write("ROM Directory: ");
				Trace.WriteLine(ROMDirectory);
			}
		}

		HostType _HostSelect;
		public HostType HostSelect
		{
			get
			{
				return _HostSelect;
			}
			set
			{
				_HostSelect = value;
			}
		}

		int _FrameRateAdjust;
		public int FrameRateAdjust
		{
			get
			{
				return _FrameRateAdjust;
			}
			set
			{
				_FrameRateAdjust = value;
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

		bool _Skip7800BIOS;
		public bool Skip7800BIOS
		{
			get
			{
				return _Skip7800BIOS;
			}
			set
			{
				_Skip7800BIOS = value;
			}
		}

		bool _Use7800HSC;
		public bool Use7800HSC
		{
			get
			{
				return _Use7800HSC;
			}
			set
			{
				_Use7800HSC = value;
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

			HostSelect = (HostType)GetRegistryDWORD("HostSelect", 1);
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

		~GlobalSettings()
		{
			SetRegistrySZ("ROMDirectory", ROMDirectory);
			SetRegistryDWORD("HostSelect", (int)HostSelect);
			SetRegistryDWORD("FrameRateAdjust", FrameRateAdjust);
			SetRegistryDWORD("NumSoundBuffers", NumSoundBuffers);
			SetRegistryDWORD("Skip7800BIOS", Skip7800BIOS ? 1 : 0);
			SetRegistryDWORD("Use7800HSC", Use7800HSC ? 1 : 0);
			SetRegistryDWORD("DeactivateMouseInput", DeactivateMouseInput ? 1 : 0);
			SetRegistryDWORD("ControlPanelFormWidth", ControlPanelFormSize.Width);
			SetRegistryDWORD("ControlPanelFormHeight", ControlPanelFormSize.Height);
			SetRegistryDWORD("NOPRegisterDumping", NOPRegisterDumping ? 1 : 0);
			SetRegistryDWORD("JoyBTrigger", JoyBTrigger);
			SetRegistryDWORD("JoyBBooster", JoyBBooster);
			SetRegistryDWORD("CpuSpin", CpuSpin);
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

		void SetRegistryDWORD(string name, int val)
		{
			try
			{
				RegistryKey rk = GetSubKey();
				rk.SetValue(name, val);
			}
			catch { }
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

		void SetRegistrySZ(string name, string val)
		{
			try
			{
				RegistryKey rk = GetSubKey();
				rk.SetValue(name, val);
			}
			catch { }
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
