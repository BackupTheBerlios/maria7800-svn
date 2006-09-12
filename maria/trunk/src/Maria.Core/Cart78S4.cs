/*
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
using System.IO;

namespace Maria.Core {
	/*
	 * Atari 7800 SuperGame S4 bankswitched cartridge
	 *
	 * Cart Format                Mapping to ROM Address Space
	 * Bank0: 0x00000:0x4000      0x0000:0x4000
	 * Bank1: 0x04000:0x4000      0x4000:0x4000  Bank2
	 * Bank2: 0x08000:0x4000      0x8000:0x4000  Bank0 (0 on startup)
	 * Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank3
	 *
	 * Banks 0-3 are the same as banks 4-7
	 */
	[Serializable]
	public sealed class Cart78S4 : Cart {
		private int[] Bank = new int[4];
		private int BankNo;
		private byte[] RAM;

		public Cart78S4(BinaryReader br, bool needRAM) {
			if (br == null) {
				throw new ArgumentNullException("br");
			}
			if (needRAM) {
				RAM = new byte[0x4000];
			}

			int size = (int)(br.BaseStream.Length - br.BaseStream.Position);
			LoadRom(br, size);

			Bank[1] = 6 % (ROM.Length/0x4000);
			Bank[2] = 0 % (ROM.Length/0x4000);
			Bank[3] = 7 % (ROM.Length/0x4000);
		}

		public override void Reset() {}

		public override byte this[ushort addr] {
			get {
				BankNo = addr >> 14;
				if (RAM != null && BankNo == 1) {
					return RAM[addr & 0x3fff];
				}
				else {
					return ROM[ (Bank[BankNo] << 14) | (addr & 0x3fff) ];
				}
			}
			set {
				BankNo = addr >> 14;
				if (BankNo == 2) {
					Bank[2] = value % (ROM.Length/0x4000);
				}
				else if (RAM != null && BankNo == 1) {
					RAM[addr & 0x3fff] = value;
				}
			}
		}
	}
}
