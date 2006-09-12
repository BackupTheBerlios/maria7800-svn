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
	 * Atari 7800 Absolute bankswitched cartridge
	 *
	 * Cart Format                Mapping to ROM Address Space
	 * Bank0: 0x00000:0x4000      0x0000:0x4000
	 * Bank1: 0x04000:0x4000      0x4000:0x4000  Bank0-1 (0 on startup)
	 * Bank2: 0x08000:0x4000      0x8000:0x4000  Bank2
	 * Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank3
	 */
	[Serializable]
	public sealed class Cart78AB : Cart {
		private int[] Bank = new int[4];
		private int BankNo;

		public Cart78AB(BinaryReader br) {
			if (br == null) {
				throw new ArgumentNullException("br");
			}
			Bank[1] = 0;
			Bank[2] = 2;
			Bank[3] = 3;
			int size = (int)(br.BaseStream.Length - br.BaseStream.Position);
			LoadRom(br, size);
		}

		public override void Reset() {}

		public override byte this[ushort addr] {
			get {
				return ROM[ (Bank[addr >> 14] << 14) | (addr & 0x3fff) ]; }
			set {
				BankNo = addr >> 14;
				if (BankNo == 2) {
					Bank[1] = (value - 1) & 0x1;
				}
			}
		}
	}
}
