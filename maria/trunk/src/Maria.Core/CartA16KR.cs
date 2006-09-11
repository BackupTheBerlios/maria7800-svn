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
	 * Atari standard 16KB bankswitched carts with 128 bytes of RAM
	 *
	 * Cart Format                Mapping to ROM Address Space
	 * Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff9-0x1ff9
	 * Bank2: 0x1000:0x1000
	 * Bank3: 0x2000:0x1000
	 * Bank4: 0x3000:0x1000
	 *                            Shadows ROM
	 *                            0x1000:0x0080  RAM write port
	 *                            0x1080:0x0080  RAM read port
	 */
	[Serializable]
	public sealed class CartA16KR : Cart {
		private ushort BankBaseAddr;
		private byte[] RAM;

		public CartA16KR(BinaryReader br) {
			LoadRom(br, 0x4000);
			Bank = 0;
			RAM = new byte[128];
		}

		int Bank {
			set { BankBaseAddr = (ushort)(value * 0x1000); }
		}

		public override void Reset() {
			Bank = 0;
		}
	
		public override byte this[ushort addr] {
			get {
				addr &= 0x0fff;
				if (addr < 0x0100 && addr >= 0x0080) {
					return RAM[addr & 0x7f];
				}
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set {
				addr &= 0x0fff;
				if (addr < 0x0080) {
					RAM[addr & 0x7f] = value;
					return;
				}
				UpdateBank(addr);
			}
		}
	
		void UpdateBank(ushort addr)  {
			if (addr < 0x0ff6 || addr > 0x0ff9) {
			}
			else {
				Bank = addr - 0x0ff6;
			}
		}
	}
}
