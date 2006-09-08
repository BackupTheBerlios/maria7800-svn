// TODO : empty stub to get things to compile
using System;

namespace Maria.Core {
	public class Bios7800 : IDevice{
		// THESE ARE ALL DUMMIES TO MAKE THE SHIT COMPILE !
		public void Reset() {}
		public void Map(AddressSpace m) {}
		public bool RequestSnooping { get { return false; } }
		public byte this[ushort adx] {
			get { return 0; }
			set {}			
		}
		public int Size;
	}
}
