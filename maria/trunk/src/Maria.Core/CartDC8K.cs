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
	 * Activison's Robot Tank and Decathlon 8KB bankswitching cart.
	 *
	 * Cart Format                Mapping to ROM Address Space
	 * Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by A13=0/1?
	 * Bank2: 0x1000:0x1000
	 *
	 * This does what the Stella code does, which is to follow A13 to determine
	 * the bank.  Since A0-A12 are the only significant bits on the program
	 * counter, I am unsure how the cart/hardware could utilize this.
	 */
	 [Serializable]
	 public sealed class CartDC8K : Cart {

		public CartDC8K(BinaryReader br) {
			LoadRom(br, 0x2000);
		}

		public override byte this[ushort addr] {
			get {
				if ((addr & 0x2000) == 0) {
					return ROM[addr & 0x0fff + 0x1000];
				}
				else {
					return ROM[addr & 0x0fff];
				}
			}
			set {}
		}
	 }
}
