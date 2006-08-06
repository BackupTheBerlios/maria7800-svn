/*
 * Machine2600.cs
 * 
 * The realization of a 2600 machine.
 * 
 * Copyright (c) 2003, 2004 Mike Murphy
 * 
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace EMU7800
{
	[Serializable]
	public class Machine2600 : Machine
	{
		protected TIA TIA;

		public override M6502 CPU
		{
			get
			{
				return _CPU;
			}
		}

		protected override void DoReset()
		{
			TIA.Reset();
			PIA.Reset();
			CPU.Reset();
		}

		protected override void DoRun()
		{
			TIA.StartFrame();
			CPU.RunClocks = (Scanlines + 3) * 76;
			while (CPU.RunClocks > 0 && !CPU.Jammed)
			{
				if (TIA.WSYNCDelayClocks > 0)
				{
					CPU.Clock += (ulong)TIA.WSYNCDelayClocks / 3;
					CPU.RunClocks -= TIA.WSYNCDelayClocks / 3;
					TIA.WSYNCDelayClocks = 0;
				}
				if (TIA.EndOfFrame)
				{
					break;
				}
				CPU.Execute();
			}
			TIA.EndFrame();
		}

		protected override void DoDone() { }

		public Machine2600(Cart c, InputAdapter ia, int slines, int startl, int fHZ, int sRate, int[] p)
			: base(ia, slines, startl, fHZ, sRate, p, 160)
		{
			Mem = new AddressSpace(this, 13, 6);  // 2600: 13bit, 64byte pages

			_CPU = new M6502(Mem);
			_CPU.RunClocksMultiple = 1;

			TIA = new TIA(this);
			for (ushort i = 0; i < 0x1000; i += 0x100)
			{
				Mem.Map(i, 0x0080, TIA);
			}

			PIA = new PIA(this);
			for (ushort i = 0x0080; i < 0x1000; i += 0x100)
			{
				Mem.Map(i, 0x0080, PIA);
			}

			Mem.Map(0x1000, 0x1000, c);
		}
	}

	[Serializable]
	public class Machine2600NTSC : Machine2600
	{
		public override string ToString()
		{
			return MachineType.A2600NTSC.ToString();
		}

		public Machine2600NTSC(Cart cart, InputAdapter ia)
			: base(cart, ia, 262, 16, 60, TIASound.NTSC_SAMPLES_PER_SEC, TIATables.NTSCPalette)
		{
			Trace.Write(this);
			Trace.WriteLine(" ready");
		}
	}

	[Serializable]
	public class Machine2600PAL : Machine2600
	{
		public override string ToString()
		{
			return MachineType.A2600PAL.ToString();
		}

		public Machine2600PAL(Cart cart, InputAdapter ia)
			: base(cart, ia, 312, 32, 50, TIASound.PAL_SAMPLES_PER_SEC, TIATables.PALPalette)
		{
			Trace.Write(this);
			Trace.WriteLine(" ready");
		}
	}
}
