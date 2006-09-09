/*
 * The BIOS of the Atari 7800.
 *
 * Copyright (C) 2004 Mike Murphy
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
	[Serializable]
	public sealed class Bios7800 : IDevice {
		private byte[] rom;
		private ushort mask;
		private ushort size;

		public Bios7800(MachineType machineType) {
			switch (machineType) {
				case MachineType.A7800NTSC:
					if (LoadBios("0763f1ffb006ddbe32e52d497ee848ae")) {
					}
					else if (LoadBios("b32526ea179dc9ab9b2e5f8a2662b298")) {
						Trace.WriteLine("WARNING: Using incorrect, but widely used, 7800 NTSC BIOS");
					}
					else {
						throw new Exception("7800 NTSC BIOS not found in ROMDirectory: " + EMU7800App.Instance.Settings.ROMDirectory);
					}
					break;
				case MachineType.A7800PAL:
					LoadBios("397bb566584be7b9764e7a68974c4263");
					break;
				default:
					throw new Exception("Invalid MachineType");
			}
		}

		public ushort Size {
			get { return size; }
			set { size = value; }
		}

		public bool RequestSnooping {
			get { return false; }
		}

		public void Reset() {
			// Nothing to do.
		}

		public void Map(AddressSpace adrspc) {
			// Nothing to do.
		}

		public byte this[ushort addr] {
			get { return rom[addr & mask]; }
			set {}
		}

		bool LoadBios(string md5) {
			rom = null;
			DirectoryInfo di = new DirectoryInfo(EMU7800App.Instance.Settings.ROMDirectory);
			string foundFullName = "";
			foreach (FileInfo fi in di.GetFiles()) {
				if (fi.Length != 4096 && fi.Length != 16384) {
					continue;
				}
				BinaryReader r = null;
				try {
					r = new BinaryReader(File.OpenRead(fi.FullName));
					rom = r.ReadBytes((int)fi.Length);
					if (md5 != MD5.ComputeMD5Digest(rom)) {
						rom = null;
					}
					else {
						foundFullName = fi.FullName;
					}
				}
				catch (Exception) {
				}
				finally {
					if (r != null) {
						r.Close();
					}
				}
				if (rom != null) {
					break;
				}
			}

			if (rom != null) {
				size = mask = (ushort) rom.Length;
				--mask;
				Trace.Write("Found 7800 BIOS: ");
				Trace.WriteLine(foundFullName);
			}

			return rom != null;
		}
	}
}
