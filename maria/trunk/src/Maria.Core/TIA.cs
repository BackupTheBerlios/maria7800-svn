// TODO : just a stub to get things to compile
using System;
using System.Diagnostics;
using System.Runtime.Serialization;
namespace Maria.Core {
	public class TIA {
		[Flags] // TODO : what's the flags attribute good for anyway ?
		public enum CxPairFlags : int
		{
			M0P1 = 1 << 0,
			M0P0 = 1 << 1,
			M1P0 = 1 << 2,
			M1P1 = 1 << 3,
			P0PF = 1 << 4,
			P0BL = 1 << 5,
			P1PF = 1 << 6,
			P1BL = 1 << 7,
			M0PF = 1 << 8,
			M0BL = 1 << 9,
			M1PF = 1 << 10,
			M1BL = 1 << 11,
			BLPF = 1 << 12,
			P0P1 = 1 << 13,
			M0M1 = 1 << 14
		};
		[Flags]
		public enum CxFlags : int
		{
			PF = 1 << 0,
			BL = 1 << 1,
			M0 = 1 << 2,
			M1 = 1 << 3,
			P0 = 1 << 4,
			P1 = 1 << 5
		};
	}
}
