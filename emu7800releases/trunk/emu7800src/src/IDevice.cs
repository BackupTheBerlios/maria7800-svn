/*
 * IDevice.cs 
 *
 * Defines interface for devices accessable via the AddressSpace class.
 *
 * Copyright (c) 2003 Mike Murphy
 *
 */
using System;

namespace EMU7800
{
	public interface IDevice
	{
		void Reset();
		void Map(AddressSpace adrspc);
		byte this[ushort addr]
		{
			get;
			set;
		}
	}
}
