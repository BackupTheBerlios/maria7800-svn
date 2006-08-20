/*
 * The class representing the memory map or address space of a machine.
 *
 * Copyright (C) 2003 Mike Murphy
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
using System.Diagnostics;

namespace Maria.Core {
	// TODO : implement, remove code below
	[Serializable]
	public sealed class AddressSpace {
		// TODO : check which variables are used in a single method
		// only, and remove these.
		// TODO : check which non-readonly variables could be readonly, and make 'em so
		private Machine machine; 
		private readonly int addrSpaceShift;	// TODO : unused ?
		private readonly int addrSpaceSize;
		private readonly int addrSpaceMask;
		private readonly int pageShift;
		private readonly int pageSize;
		private IDevice[] memoryMap;
		private IDevice snooper;
		private byte dataBusState;

		public Machine Machine {
			get { return machine; }
		}

		public byte DataBusState {
			get { return dataBusState; }
		}

		public override string ToString() {
			return "AdrSpc";
		}
	}
}

// TODO : remove teh shit below.
/*
		public byte this[ushort addr]
		{
			get 
			{
				if (Snooper != null)
				{
					ushort dummyRead = Snooper[addr];
				}
				int pageno = (addr & AddrSpaceMask) >> PageShift;
				IDevice dev = MemoryMap[pageno];
				_DataBusState = dev[addr];
				return _DataBusState;
			}
			set 
			{
				_DataBusState = value;
				if (Snooper != null)
				{
					Snooper[addr] = _DataBusState;
				}
				int pageno = (addr & AddrSpaceMask) >> PageShift;
				IDevice dev = MemoryMap[pageno];
				dev[addr] = _DataBusState;
			}
		}

		public void Map(ushort basea, ushort size, IDevice device)
		{
			if (device == null)
			{
				throw new ArgumentNullException("device");
			}
			for (int addr = basea; addr < basea + size; addr += PageSize)
			{
				int pageno = (addr & AddrSpaceMask) >> PageShift;
				MemoryMap[pageno] = device;
			}
			device.Map(this);

			Debug.WriteLine(String.Format("{0}: Mapped {1} to ${2:x4}:${3:x4}",
				this, device, basea, basea + size));
		}

		public void Map(ushort basea, ushort size, Cart cart)
		{
			if (cart == null)
			{
				throw new ArgumentNullException("cart");
			}
			IDevice device = (IDevice)cart;
			if (cart.RequestSnooping)
			{
				Snooper = device;
			}
			Map(basea, size, device);
		}

		public AddressSpace(Machine m, int addrSpaceShift, int pageShift)
		{
			_M = m;

			AddrSpaceShift = addrSpaceShift;
			AddrSpaceSize  = 1 << AddrSpaceShift;
			AddrSpaceMask = AddrSpaceSize - 1;

			PageShift = pageShift;
			PageSize = 1 << PageShift;

			MemoryMap = new IDevice[1 << addrSpaceShift >> PageShift];
			IDevice nullDev = new NullDevice();
			for (int pageno=0; pageno < MemoryMap.Length; pageno++)
			{
				MemoryMap[pageno] = nullDev;
			}
		}
	}
}
*/
