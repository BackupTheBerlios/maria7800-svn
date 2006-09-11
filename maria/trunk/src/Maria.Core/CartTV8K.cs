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
	 * Tigervision 8KB bankswitched carts
	 *
	 * Cart Format                Mapping to ROM Address Space
	 * Segment1: 0x0000:0x0800    0x1000:0x0800  Selected segment via $003F
	 * Segment2: 0x0800:0x0800    0x1800:0x0800  Always last segment
	 * Segment3: 0x1000:0x0800
	 * Segment4: 0x1800:0x0800
	 */
	[Serializable]
	public sealed class CartTV8K : Cart {
		private ushort BankBaseAddr;
		private ushort LastBankBaseAddr;

		public CartTV8K(BinaryReader br) {
			LoadRom(br, 0x1000);
			Bank = 0;
			LastBankBaseAddr = (ushort)(ROM.Length - 0x0800);
		}

		public override void Reset() {
			Bank = 0;
		}

		public override bool RequestSnooping {
			get { return true; }
		}

		public override byte this[ushort addr] {
			get {
				addr &= 0x0fff;
				if (addr < 0x0800) {
					return ROM[BankBaseAddr + (addr & 0x07ff)];
				}
				else {
					return ROM[LastBankBaseAddr + (addr & 0x07ff)];
				}
			}
			set {
				if (addr <= 0x003f) {
					Bank = value;
				}
			}
		}

		private byte Bank {
			set {
				BankBaseAddr = (ushort) (0x0800 * value);
				BankBaseAddr %= (ushort) ROM.Length;
			}
		}

	}
}
