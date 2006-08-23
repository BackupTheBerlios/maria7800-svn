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
using Vtg.Util;

namespace Maria.Core {
	// TODO : implement, remove code below
	[Serializable]
	public sealed class AddressSpace {
		private readonly Machine machine;
		private readonly int addrSpaceShift;	// TODO : unused ?
		private readonly int addrSpaceSize;
		private readonly int addrSpaceMask;
		private readonly int pageShift;
		private readonly int pageSize;
		private readonly IDevice[] memoryMap;
		private IDevice snooper;
		private byte dataBusState;

		public AddressSpace(Machine m, int addrSpaceShift, int pageShift) {
			ArgumentCheck.NotNull(m, "m");
			this.machine = m;
			this.addrSpaceShift = addrSpaceShift;
			this.addrSpaceSize = 1 << addrSpaceShift;
			this.addrSpaceMask = addrSpaceSize - 1;
			this.pageShift = pageShift;
			this.pageSize = 1 << pageShift;
			this.memoryMap = new IDevice[addrSpaceSize / pageSize];
			IDevice nullDev = new NullDevice();
			for (int pageno=0; pageno < PageCount; ++pageno)
				memoryMap[pageno] = nullDev;
		}

		public Machine Machine {
			get { return machine; }
		}

		public byte DataBusState {
			get { return dataBusState; }
		}

		public int AddrSpaceSize {
			get { return addrSpaceSize; }
		}

		public int PageSize {
			get { return pageSize; }
		}

		public int PageCount {
			get { return memoryMap.Length; }
		}

		public override string ToString() {
			return "AdrSpc";
		}

		public byte this[ushort addr] {
			get {
				// TODO : hardcoded for empty space...
				return 0;
			}
			set {
				// TODO : hardcoded for empty space (which is read-only)
			}
		}

		public void Map(ushort baseAddress, ushort sizeInBytes, IDevice device) {
			ArgumentCheck.NotNull(device, "device");
			// TODO : let's do the snooping business first...
			// TODO : then let's do the remaining stuff...
			if (device.RequestSnooping) {
				// TODO : check only one is allowed...
				if (snooper != null)
					throw new InternalErrorException("Only one snooper is allowed.");
				snooper = device;
			}
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
		}
	}
}
*/
