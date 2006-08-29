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
	 * Atari 7800 non-bankswitched 32KB cartridge
	 *
	 * Cart Format                Mapping to ROM Address Space
	 * 0x0000:0x8000              0x8000:0x8000
	 */
	[Serializable]
	public sealed class Cart7832 : Cart {
		public const int RomSize = 0x8000;
		
		public override byte this[ushort addr] {
			get { return ROM[addr & (RomSize - 1)]; }
			set {}
		}

		public Cart7832(BinaryReader br) {
			LoadRom(br, RomSize);
		}
	}
}
