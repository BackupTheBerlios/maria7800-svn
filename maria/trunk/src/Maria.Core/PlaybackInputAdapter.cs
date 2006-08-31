/*
 * An InputAdapter extension providing input playback support 
 * TODO: Lightgun needs to be included
 *
 * Copyright (C) 2003, 2004 Mike Murphy 
 * Copyright (C) 2006 Thomas Mathys (tom42@users.berlios.de)
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
using System.Diagnostics;
using System.IO;

namespace Maria.Core {
	public delegate void PlaybackEofListener();
	
	public class PlaybackInputAdapter : InputAdapter {
		public event PlaybackEofListener EofListeners;
		private string cartMD5;
		private long nextPlaybackFrameNumber;
		private BinaryReader BR;

		public string CartMD5 {
			get { return cartMD5; }
		}

		public override string ToString() {
			return "PlaybackInputAdapter";
		}

		void LogPlaybackStatusMsg(string msg) {
			Trace.Write(this);
			Trace.Write(": ");
			Trace.WriteLine(msg);
		}

		protected override void DoCheckPoint(long frameNumber) {
			if (BR != null) {
				try {
					while (nextPlaybackFrameNumber <= frameNumber) {
						ConsoleSwitches = BR.ReadByte();
						KeypadState = BR.ReadByte();
						for (int i = 0; i < 4; i++) {
							ControllerActions[i] = BR.ReadInt32();
						}
						for (int i = 0; i < 4; i++) {
							Ohms[i] = BR.ReadInt32();
						}
						for (int i = 0; i < 2; i++) {
							DrivingCounters[i] = BR.ReadByte();
						}
						nextPlaybackFrameNumber = BR.ReadInt64();
					}
				}
				catch (EndOfStreamException) {
					StopPlayback();
					if (EofListeners != null) {
						EofListeners();
					}
				}
				catch (Exception ex) {
					LogPlaybackStatusMsg(ex.Message);
					StopPlayback();
				}
			}
		}

		public void StopPlayback() {
			if (BR != null) {
				BR.Close();
				BR = null;
				IgnoreInput = false;
				ClearAllInput();
				LogPlaybackStatusMsg("Stopped");
			}
		}

		public PlaybackInputAdapter(string fn) {
			IgnoreInput = true;
			try {
				BR = new BinaryReader(File.OpenRead(fn));
				string fver = BR.ReadString();
				if (fver != "1") {
					throw new Exception("bad file format");
				}
				cartMD5 = BR.ReadString();
				if (BR.ReadString() != "EMU7800") {
					throw new Exception("bad file format");
				}
				nextPlaybackFrameNumber = BR.ReadInt64();
				LogPlaybackStatusMsg("Started");
			}
			catch (Exception ex) {
				LogPlaybackStatusMsg(ex.Message);
				StopPlayback();
			}
		}
	}
}
