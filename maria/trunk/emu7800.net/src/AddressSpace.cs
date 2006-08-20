/*
 * AddressSpace.cs
 *
 * The class representing the memory map or address space of a machine.
 *
 * Copyright (c) 2003 Mike Murphy
 *
 */
using System;
using System.Diagnostics;

namespace EMU7800
{
	[Serializable]
	public sealed class AddressSpace
	{
		Machine _M;
		public  Machine M
		{
			get
			{
				return _M;
			}
		}

		readonly int AddrSpaceShift;
		readonly int AddrSpaceSize;
		readonly int AddrSpaceMask;

		readonly int PageShift;
		readonly int PageSize;

		IDevice[] MemoryMap;
		IDevice Snooper;

		byte _DataBusState;
		public byte DataBusState
		{
			get 
			{
				return _DataBusState;
			}
		}

		public override string ToString()
		{
			return "AdrSpc";
		}

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