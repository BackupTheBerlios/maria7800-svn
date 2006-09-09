/*
 * The 7800 High Score cartridge--courtesy of Matthias <matthias@atari8bit.de>.
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
	[Serializable]	public sealed class HSC7800 : IDevice {
		private byte[] rom;
		private ushort mask;
		private ushort size;
		RAM6116 sram = new RAM6116();

		public HSC7800() {
			LoadROM("c8a73288ab97226c52602204ab894286");
		}

		public void Reset() {
			LoadRAM();
		}

		public bool RequestSnooping {
			get { return false; }
		}

		public void Map(AddressSpace mem) {}

		public ushort Size {
			get { return size; }
			set { size = value; }
		}

		public byte this[ushort addr] {
			get { return rom[addr & mask]; }
			set {}
		}

		public RAM6116 SRAM {
			get { return sram; }
		}

		string BackingStoreFullName {
			get {
				return Path.Combine(EMU7800App.Instance.Settings.OutputDirectory, "scores.hsc");
			}
		}

		public void SaveSRAM() {
			Trace.Write("Saving Highscore Cartridge RAM: ");
			Trace.Write(BackingStoreFullName);
			Trace.Write("... ");
			BinaryWriter bw = null;
			try {
				bw = new BinaryWriter(File.Create(BackingStoreFullName));
				for (ushort addr = 0; addr < 0x0800; addr++) {
					bw.Write(SRAM[addr]);
				}
				bw.Flush();
				Trace.WriteLine("ok");
			}
			catch (Exception ex) {
				Trace.WriteLine("error");
				Trace.WriteLine(ex.Message);
			}
			finally {
				if (bw != null) {
					bw.Close();
				}
			}
		}

		void LoadROM(string md5) {
			rom = null;
			DirectoryInfo di = new DirectoryInfo(EMU7800App.Instance.Settings.ROMDirectory);
			Trace.Write("Loading Highscore Cartridge ROM: ");
			Trace.Write("... ");
			foreach (FileInfo fi in di.GetFiles()) {
				if (fi.Length == 4096) {
					BinaryReader br = null;
					try {
						br = new BinaryReader(File.OpenRead(fi.FullName));
						rom = br.ReadBytes((int)fi.Length);
					}
					catch {
						rom = null;
					}
					finally {
						if (br != null) {
							br.Close();
						}
					}
					if (md5 != MD5.ComputeMD5Digest(rom)) {
						rom = null;
					}
					else if (rom != null) {
						break;
					}
				}
			}

			if (rom == null) {
				throw new Exception("7800 Highscore BIOS not found in ROM directory: " + di.FullName);
			}
			else {
				Trace.WriteLine("ok, found " + di.FullName);
				size = mask = (ushort) rom.Length;
				--mask;
			}
		}

		void LoadRAM() {
			Trace.Write("Loading Highscore Cartridge RAM: ");
			Trace.Write(BackingStoreFullName);
			Trace.Write("... ");
			BinaryReader br = null;
			byte[] data = null;
			try {
				br = new BinaryReader(File.OpenRead(BackingStoreFullName));
				data = br.ReadBytes(0x0800);
				Trace.WriteLine("ok");
			}
			catch (Exception ex) {
				data = new byte[0x0800];
				Trace.WriteLine("error");
				Trace.WriteLine(ex.Message);
			}
			finally {
				if (br != null) {
					br.Close();
				}
			}
			for (ushort addr = 0; data != null && addr < data.Length; addr++) {
				SRAM[addr] = data[addr];
			}
		}
	}
}
