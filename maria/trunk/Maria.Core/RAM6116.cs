/*
 * Implements a 6116 RAM device found in the 7800.
 * Copyright (c) 2004 Mike Murphy
 * Copyright (C) 2006 Thomas Mathys (tom42@sourceforge.net)
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
		private const int SIZE = 0x800;
		private byte[] fRAM = new byte[SIZE];

		public void Reset() {
			// Nothing to do
		}

		public void Map(AddressSpace addressSpace) {
			// Nothing to do
		}

		public byte this[ushort address]
		{
			// TODO : would be neat to see how this gets compiled...
			get { return fRAM[address % SIZE]; }
			set { fRAM[address % SIZE] = value; }
		}

		public int Size {
			get { return SIZE; }
		}
	}
}
