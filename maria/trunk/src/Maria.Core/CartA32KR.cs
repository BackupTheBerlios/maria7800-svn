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
	 * Atari standard 32KB bankswitched carts with 128 bytes of RAM
	 * 
	 * Cart Format                Mapping to ROM Address Space
	 * Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x0ff4-0x0ffc
	 * Bank2: 0x1000:0x1000
	 * Bank3: 0x2000:0x1000
	 * Bank4: 0x3000:0x1000
	 * Bank5: 0x4000:0x1000
	 * Bank6: 0x5000:0x1000
	 * Bank7: 0x6000:0x1000
	 * Bank8: 0x7000:0x1000
	 *                            Shadows ROM
	 *        0x1000:0x80         RAM write port
	 *        0x1080:0x80         RAM read port
	 */
	[Serializable]
	public sealed class CartA32KR : Cart {
		private ushort BankBaseAddr;
		private byte[] RAM;

		public CartA32KR(BinaryReader br) {
			LoadRom(br, 0x8000);
			RAM = new byte[128];
			Bank = 7;
		}

		public override void Reset() {
			Bank = 7;
		}

		public override byte this[ushort addr] {
			get {
				addr &= 0x0fff;
				if (addr >= 0x0080 && addr < 0x0100) {
					return RAM[addr & 0x007f];
				}
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set {
				addr &= 0x0fff;
				if (addr < 0x0080) {
					RAM[addr & 0x007f] = value;
					return;
				}
				UpdateBank(addr);
			}
		}

		private void UpdateBank(ushort addr) {
			if (addr < 0x0ffc && addr >= 0x0ff4 ) {
				Bank = addr - 0x0ff4;
			}
		}

		private int Bank {
			set { BankBaseAddr = (ushort)(value * 0x1000); }
		}
	}
}
