/*
 * NullDevice.cs
 *
 * Default memory mappable device.
 *
 * Copyright (c) 2003, 2004 Mike Murphy
 *
 */
using System;
using System.Diagnostics;

namespace EMU7800
{
	[Serializable]
	public sealed class NullDevice : IDevice
	{
		public void Reset()
		{
			Trace.Write(this);
			Trace.WriteLine(" reset");
		}

		public void Map(AddressSpace mem) { }

		public override String ToString()
		{
			return "NullDevice";
		}

		public byte this[ushort addr]
		{
			get { return 0; }
			set { }
		}

		public NullDevice() { }
	}
}