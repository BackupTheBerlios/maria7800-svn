// TODO : sooner than later get rid of this class.
// It has only been kept to get the emu running as quickly
// as possible...
using System;
using System.IO;

namespace Maria.Core {
	public class EMU7800App {
		public static readonly EMU7800App Instance = new EMU7800App();		
		private GlobalSettings _Settings;
		private ROMProperties _ROMProperties;
		private Machine _M;
		
		public GlobalSettings Settings {
			get { return _Settings; }
		}

		public ROMProperties ROMProperties {
			get { return _ROMProperties; }
		}

		public Machine M {
			get { return _M; }
			set { _M = value; }
		}

		public void RunMachine(GameSettings gs, InputAdapter ia) {
			if (ia == null) {
				ia = new InputAdapter();
			}
			M = Machine.New(gs, Cart.New(gs), ia);
			M.Reset();
			// TODO : need to create real host here ?
			//Host.New(Settings.HostSelect).Run(M);
			M.Done();
		}

		private EMU7800App() {
			_Settings = new GlobalSettings();
			_ROMProperties = new ROMProperties();
		}

		void LaunchFromCL(string[] args) {
			FileInfo fi = new FileInfo(args[0]);
			GameSettings gs = ROMProperties.GetGameSettings(MD5.ComputeMD5Digest(fi));
			gs.FileInfo = fi;
			RunMachine(gs, null);
		}
	}
}
