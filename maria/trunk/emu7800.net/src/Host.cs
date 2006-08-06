/*
 * Host.cs
 * 
 * Abstraction of an emulated machine host.
 * 
 * Copyright (c) 2004 Mike Murphy
 * 
 */
using System;

namespace EMU7800
{
	public abstract class Host
	{
		public static Host New(HostType hostType)
		{
			Host host = null;
			switch (hostType)
			{
				case HostType.GDI:
					host = HostGDI.Instance;
					break;
				case HostType.SDL:
					host = HostSdl.Instance;
					break;
				default:
					throw new Exception("Unexpected HostType: " + hostType.ToString());
			}
			return host;
		}
		public abstract void Run(Machine m);
		public abstract void UpdateDisplay(byte[] buf, int scanline, int start, int len);
		public abstract void UpdateSound(byte[] buf);
	}
}
