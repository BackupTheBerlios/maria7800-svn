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
	 * CBS RAM Plus 12KB bankswitched carts with 128 bytes of RAM.
	 * 
	 * Cart Format                Mapping to ROM Address Space
	 * Bank1: 0x0000:0x1000       Bank1:0x1000:0x1000  Select Segment: 0ff8-0ffa
	 * Bank2: 0x1000:0x1000
	 * Bank3: 0x2000:0x1000
	 *                            Shadows ROM
	 *        0x1000:0x80         RAM write port
	 *        0x1080:0x80         RAM read port
	 */
	[Serializable]
	public sealed class CartCBS12K : Cart {
		private ushort BankBaseAddr;
		private byte[] RAM;

		public CartCBS12K(BinaryReader br) {
			LoadRom(br, 0x3000);
			Bank = 2;
			RAM = new byte[256];
		}

		public override void Reset() {
			Bank = 2;
		}

		public override byte this[ushort addr] {
			get {
				addr &= 0x0fff;
				if (addr < 0x0200 && addr >= 0x0100) {
					return RAM[addr & 0xff];
				}
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set {
				addr &= 0x0fff;
				if (addr < 0x0100) {
					RAM[addr & 0xff] = value;
					return;
				}
				UpdateBank(addr);
			}
		}

		void UpdateBank(ushort addr) {
			if (addr < 0x0ff8 || addr > 0x0ffa) {
				// Nothing to do
			}
			else {
				Bank = addr - 0x0ff8;
			}
		}

		private int Bank {
			set { BankBaseAddr = (ushort)(value * 0x1000); }
		}
	}
}
