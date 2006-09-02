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

namespace EMU7800
{
	public class EMU7800App
	{
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
			Host.New(Settings.HostSelect).Run(M);
			M.Done();
		}

		public static readonly EMU7800App Instance = new EMU7800App();
		private EMU7800App()
		{
			EMUTraceListener.Instance.Start();
			_Settings = new GlobalSettings();
			_ROMProperties = new ROMProperties();
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
				// this branch used to launch the gui.
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
				throw new Exception("We don't care about this branch. Really.");
			}
		}
	}
}
