/*
 * The realization of a 7800 machine.
 *
 * Copyright (C) 2003-2005 Mike Murphy
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

	// TODO : not yet possible (Maria still missing)
	/*[Serializable]
	public class Machine7800 : Machine {
		protected Maria Maria;
		protected RAM6116 RAM1, RAM2;
		protected Cart Cart;
		protected Bios7800 BIOS;
		protected HSC7800 HSC;

		public override M6502 CPU {
			get { return cpu; }
		}

	}
	*/
}

// TODO : enable stuff below
/*
namespace EMU7800
{
		public void SwapInBIOS()
		{
			if (BIOS != null)
			{
				Mem.Map((ushort)(0x10000 - BIOS.Size), BIOS.Size, BIOS);
			}
		}

		public void SwapOutBIOS()
		{
			if (BIOS != null)
			{
				Mem.Map((ushort)(0x10000 - BIOS.Size), BIOS.Size, Cart);
			}
		}

		protected override void DoReset()
		{
			SwapInBIOS();
			if (HSC != null)
			{
				HSC.Reset();
			}
			Cart.Reset();
			Maria.Reset();
			PIA.Reset();
			CPU.Reset();
		}

		protected override void DoRun()
		{
			Maria.StartFrame();
			for (int i = 0; i < Scanlines; i++)
			{
				CPU.RunClocks = CPU.RunClocksMultiple * 114;
				Maria.DoScanline(i);
				CPU.Execute();
				CPU.Clock += (ulong)(CPU.RunClocks / CPU.RunClocksMultiple);
			}
			Maria.EndFrame();
		}

		protected override void DoDone()
		{
			if (HSC != null)
			{
				HSC.SaveSRAM();
			}
		}

		public Machine7800(Cart c, InputAdapter ia, int slines, int startl, int fHZ, int sRate, int[] p)
			: base(ia, slines, startl, fHZ, sRate, p, 320)
		{
			Mem = new AddressSpace(this, 16, 6);  // 7800: 16bit, 64byte pages

			_CPU = new M6502(Mem);
			_CPU.RunClocksMultiple = 4;

			Maria = new Maria(this);
			Mem.Map(0x0000, 0x0040, Maria);
			Mem.Map(0x0100, 0x0040, Maria);
			Mem.Map(0x0200, 0x0040, Maria);
			Mem.Map(0x0300, 0x0040, Maria);

			PIA = new PIA(this);
			Mem.Map(0x0280, 0x0080, PIA);
			Mem.Map(0x0480, 0x0080, PIA);
			Mem.Map(0x0580, 0x0080, PIA);  // unsure about this one

			RAM1 = new RAM6116();
			Mem.Map(0x1800, 0x0800, RAM1);

			RAM2 = new RAM6116();
			Mem.Map(0x2000, 0x0800, RAM2);
			Mem.Map(0x0040, 0x00c0, RAM2); // page 0 shadow
			Mem.Map(0x0140, 0x00c0, RAM2); // page 1 shadow
			Mem.Map(0x2800, 0x0800, RAM2); // shadow1
			Mem.Map(0x3000, 0x0800, RAM2); // shadow2
			Mem.Map(0x3800, 0x0800, RAM2); // shadow3

			// Insert the 7800 Highscore cartridge if requested
			if (EMU7800App.Instance.Settings.Use7800HSC)
			{
				HSC = new HSC7800();
				Mem.Map(0x1000, 0x800, HSC.SRAM);
				Mem.Map(0x3000, 0x1000, HSC);
				Trace.WriteLine("7800 Highscore Cartridge Installed");
			}

			Cart = c;
			Mem.Map(0x4000, 0xc000, Cart);
		}
	}

	[Serializable]
	public class Machine7800NTSC : Machine7800
	{
		public override string ToString()
		{
			return MachineType.A7800NTSC.ToString();
		}

		public Machine7800NTSC(Cart cart, InputAdapter ia)
			: base(cart, ia, 262, 16, 60, TIASound.NTSC_SAMPLES_PER_SEC, MariaTables.NTSCPalette)
		{
			if (!EMU7800App.Instance.Settings.Skip7800BIOS)
			{
				BIOS = new Bios7800(MachineType.A7800NTSC);
			}
			Trace.Write(this);
			Trace.WriteLine(" ready");
		}
	}

	[Serializable]
	public class Machine7800PAL : Machine7800
	{
		public override string ToString()
		{
			return MachineType.A7800PAL.ToString();
		}

		public Machine7800PAL(Cart cart, InputAdapter ia)
			: base(cart, ia, 312, 32, 50, TIASound.PAL_SAMPLES_PER_SEC, MariaTables.PALPalette)
		{
			if (!EMU7800App.Instance.Settings.Skip7800BIOS)
			{
				BIOS = new Bios7800(MachineType.A7800PAL);
			}
			Trace.Write(this);
			Trace.WriteLine(" ready");
		}
	}
}
*/
