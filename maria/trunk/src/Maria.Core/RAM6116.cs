/*
 * Implements a 6116 RAM device found in the 7800.
 * Copyright (c) 2004 Mike Murphy
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

namespace Maria.Core {
	[Serializable]
	public class RAM6116 : IDevice
	{
		public const int Size = 0x800; // Must be a power of 2
		private byte[] fRAM = new byte[Size];

		public void Reset() {
			// Nothing to do
		}

		public void Map(AddressSpace addressSpace) {
			// Nothing to do
		}

		public byte this[ushort address]
		{
			get { return fRAM[address & Size - 1]; }
			set { fRAM[address & Size - 1] = value; }
		}

		public bool RequestSnooping {
			get { return false; }
		}
	}
}
