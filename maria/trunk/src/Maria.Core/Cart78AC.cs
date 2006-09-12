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
	 * Atari 7800 Activision bankswitched cartridge
	 *
	 * Cart Format                Mapping to ROM Address Space
	 * Bank0 : 0x00000:0x2000     0x0000:0x2000
	 * Bank1 : 0x02000:0x2000     0x2000:0x2000
	 * Bank2 : 0x04000:0x2000     0x4000:0x2000  Bank13
	 * Bank3 : 0x06000:0x2000     0x6000:0x2000  Bank12
	 * Bank4 : 0x08000:0x2000     0x8000:0x2000  Bank15
	 * Bank5 : 0x0a000:0x2000     0xa000:0x2000  Bank(n)   n in [0-15], n=0 on startup
	 * Bank6 : 0x0c000:0x2000     0xc000:0x2000  Bank(n+1)
	 * Bank7 : 0x0e000:0x2000     0xe000:0x2000  Bank14
	 * Bank8 : 0x10000:0x2000
	 * Bank9 : 0x12000:0x2000
	 * Bank10: 0x14000:0x2000
	 * Bank11: 0x16000:0x2000
	 * Bank12: 0x18000:0x2000
	 * Bank13: 0x1a000:0x2000
	 * Bank14: 0x1c000:0x2000
	 * Bank15: 0x13000:0x2000
	 */
	[Serializable]
	public sealed class Cart78AC : Cart {
		private int[] Bank = new int[8];

		public Cart78AC(BinaryReader br) {
			if (br == null) {
				throw new ArgumentNullException("br");
			}
			Bank[2] = 13;
			Bank[3] = 12;
			Bank[4] = 15;
			Bank[5] = 0;
			Bank[6] = 1;
			Bank[7] = 14;
			int size = (int)(br.BaseStream.Length - br.BaseStream.Position);
			LoadRom(br, size);
		}

		public override void Reset() {}

		public override byte this[ushort addr] {
			get {
				return ROM[ (Bank[addr >> 13] << 13) | (addr & 0x1fff) ];
			}
			set {
				if ((addr & 0xfff0) == 0xff80) {
					Bank[5] = (addr & 0xf) << 1;
					Bank[6] = Bank[5] + 1;
				}
			}
		}


	}
}
