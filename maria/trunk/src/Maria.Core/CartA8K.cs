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
	 * Atari standard 8KB bankswitched carts
	 *
	 * Cart Format                Mapping to ROM Address Space
	 * Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff8,0x1ff9
	 * Bank2: 0x1000:0x1000
	 */
	[Serializable]
	public sealed class CartA8K : Cart {
		private ushort BankBaseAddr;

		public CartA8K(BinaryReader br) {
			LoadRom(br, 0x2000);
			Bank = 1;
		}
	
		private int Bank {
			set { BankBaseAddr = (ushort)(value * 0x1000); }
		}

		public override void Reset() {
			Bank = 1;
		}
	
		public override byte this[ushort addr] {
			get {
				addr &= 0x0fff;
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set {
				addr &= 0x0fff;
				UpdateBank(addr);
			}
		}
	
		void UpdateBank(ushort addr) {
			switch(addr) {
				case 0x0ff8:
					Bank = 0;
					break;
				case 0x0ff9:
					Bank = 1;
					break;
			}
		}
	}
}
