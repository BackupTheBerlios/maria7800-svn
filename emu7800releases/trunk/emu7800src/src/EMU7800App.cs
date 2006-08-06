/*
 * EMU7800App.cs
 * 
 * Main application class for EMU7800.
 * 
 * Copyright (c) 2004-2005 Mike Murphy
 * 
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Windows.Forms;

[assembly: AssemblyTitle("EMU7800")]
[assembly: AssemblyProduct("EMU7800")]
[assembly: AssemblyDescription("An Atari 2600/7800 .NET-based Emulator")]
[assembly: AssemblyCopyright("Copyright © 2003-2006 Mike Murphy")]
[assembly: AssemblyVersion("0.71.0.0")]
[assembly: CLSCompliant(false)]

namespace EMU7800
{
	public class EMU7800App
	{
		public static string Title
		{
			get
			{
				Assembly myAss = Assembly.GetExecutingAssembly();
				object[] obj = myAss.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				AssemblyTitleAttribute attr = (AssemblyTitleAttribute)obj[0];
				return attr.Title;
			}
		}

		public static string Version
		{
			get
			{
				Assembly myAss = Assembly.GetExecutingAssembly();
				return myAss.GetName().Version.ToString();
			}
		}

		public static string Copyright
		{
			get
			{
				Assembly myAss = Assembly.GetExecutingAssembly();
				object[] obj = myAss.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				AssemblyCopyrightAttribute attr = (AssemblyCopyrightAttribute)obj[0];
				return attr.Copyright;
			}
		}

		public Version ClrVersion
		{
			get
			{
				return Environment.Version;
			}
		}
		public OperatingSystem OSVersion
		{
			get
			{
				return Environment.OSVersion;
			}
		}

		GlobalSettings _Settings;
		public GlobalSettings Settings
		{
			get
			{
				return _Settings;
			}
		}

		ROMProperties _ROMProperties;
		public ROMProperties ROMProperties
		{
			get
			{
				return _ROMProperties;
			}
		}

		Machine _M;
		public Machine M
		{
			get
			{
				return _M;
			}
			set
			{
				_M = value;
			}
		}

		public void RunMachine(GameSettings gs, InputAdapter ia)
		{
			if (ia == null)
			{
				ia = new InputAdapter();
			}
			M = Machine.New(gs, Cart.New(gs), ia);
			M.Reset();
			RunMachine();
			M.Done();
		}

		public void RunMachine()
		{
			Host.New(Settings.HostSelect).Run(M);
		}

		public static readonly EMU7800App Instance = new EMU7800App();
		private EMU7800App()
		{
			EMUTraceListener.Instance.Start();

			Trace.Write(Title);
			Trace.Write(" v");
			Trace.Write(Version);
			Debug.Write(" DEBUG");
			Trace.WriteLine("");
			Trace.WriteLine(Copyright);

			Trace.Write("CLR Version: ");
			Trace.WriteLine(ClrVersion);

			Trace.Write("OS Version: ");
			Trace.WriteLine(OSVersion);

			_Settings = new GlobalSettings();
			_ROMProperties = new ROMProperties();
		}

		void LaunchGUI()
		{
			Trace.WriteLine("Launching GUI interface");
			Application.Run(new ControlPanelForm());
		}

		void LaunchFromCL(string[] args)
		{
			try
			{
				FileInfo fi = new FileInfo(args[0]);
				GameSettings gs = ROMProperties.GetGameSettings(MD5.ComputeMD5Digest(fi));
				gs.FileInfo = fi;
				RunMachine(gs, null);
			} 
			catch (Exception ex)
			{
				Trace.Write("Error running machine: ");
				Trace.WriteLine(ex);
				EMU7800App.Instance.LaunchGUI();
			}
		}

		[STAThread]
		public static void Main(string[] args)
		{
			if (args.Length > 0)
			{
				EMU7800App.Instance.LaunchFromCL(args);
			}
			else
			{
				EMU7800App.Instance.LaunchGUI();
			}
		}
	}
}