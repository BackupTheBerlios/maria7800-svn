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
	 * M-Network 16KB bankswitched carts with 2KB RAM.
	 * 
	 * Cart Format                Mapping to ROM Address Space
	 * Segment1: 0x0000:0x0800    Bank1:0x1000:0x0800  Select Seg: 1fe0-1fe6, 1fe7=RAM Seg1
	 * Segment2: 0x0800:0x0800    Bank2:0x1800:0x0800  Always Seg8
	 * Segment3: 0x1000:0x0800
	 * Segment4: 0x1800:0x0800
	 * Segment5: 0x2000:0x0800
	 * Segment6: 0x2800:0x0800
	 * Segment7: 0x3000:0x0800
	 * Segment8: 0x3800:0x0800
	 * 
	 * RAM                       RAM Segment1 when 1fe7 select is accessed
	 * Segment1: 0x0000:0x0400   0x1000-0x13FF write port
	 * Segment2: 0x0400:0x0400   0x1400-0x17FF read port
	 * 
	 *                           RAM Segment2: 1ff8-1ffb selects 256-byte block
	 *                           0x1800-0x18ff write port
	 *                           0x1900-0x19ff read port
	 */
	[Serializable]
	public sealed class CartMN16K : Cart {
		private ushort BankBaseAddr, BankBaseRAMAddr;
		private bool RAMBankOn;
		private byte[] RAM;

		public CartMN16K(BinaryReader br) {
			LoadRom(br, 0x4000);
			RAM = new byte[2048];
			Bank = 0;
			BankRAM = 0;
		}

		public override void Reset() {
			Bank = 0;
			BankRAM = 0;
		}

		private int Bank {
			set {
				BankBaseAddr = (ushort)(value << 11);
				RAMBankOn = (value == 0x07);
			}
		}

		public override byte this[ushort addr] {
			get {
				addr &= 0x0fff;
				UpdateBanks(addr);
				if (RAMBankOn && addr >= 0x0400 && addr < 0x0800) {
					return RAM[addr & 0x03ff];
				}
				else if (addr >= 0x0900 && addr < 0x0a00) {
					return RAM[0x400 + BankBaseRAMAddr + (addr & 0xff)];
				}
				else if (addr < 0x0800) {
					return ROM[BankBaseAddr + (addr & 0x07ff)];
				}
				else {
					return ROM[0x3800 + (addr & 0x07ff)];
				}
			}
			set {
				addr &= 0x0fff;
				UpdateBanks(addr);
				if (RAMBankOn && addr < 0x0400) {
					RAM[addr & 0x03ff] = value;
				}
				else if (addr >= 0x0800 && addr < 0x0900) {
					RAM[0x400 + BankBaseRAMAddr + (addr & 0xff)] = value;
				}
			}
		}

		void UpdateBanks(ushort addr) {
			if (addr >= 0x0fe0 && addr < 0x0fe8) {
				Bank = addr & 0x07;
			}
			else if (addr >= 0x0fe8 && addr < 0x0fec) {
				BankRAM = addr & 0x03;
			}
		}
		
		private int BankRAM {
			set { BankBaseRAMAddr = (ushort)(value << 8); }
		}
	}
}
