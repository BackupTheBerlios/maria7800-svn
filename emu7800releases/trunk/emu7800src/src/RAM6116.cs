/*
 * RAM6116.cs
 *
 * Implements a 6116 RAM device found in the 7800.
 *
 * Copyright (c) 2004 Mike Murphy
 *
 */
using System;

namespace EMU7800
{
	[Serializable]
	public class RAM6116 : IDevice
	{
		byte[] RAM = new byte[0x800];

		public void Reset() {}
		public void Map(AddressSpace adrspc) {}

		public byte this[ushort addr]
		{
			get
			{
				return RAM[addr & 0x07ff];
			}
			set
			{
				RAM[addr & 0x07ff] = value;
			}
		}
	}
}