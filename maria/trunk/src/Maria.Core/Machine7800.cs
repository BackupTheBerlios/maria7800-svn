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

	[Serializable]
	public class Machine7800 : Machine {
		protected Maria Maria;
		protected RAM6116 RAM1, RAM2;
		protected Cart Cart;
		protected Bios7800 BIOS;
		protected HSC7800 HSC;

		public Machine7800(Cart c, InputAdapter ia, int slines, int startl, int fHZ, int sRate, int[] p)
			: base(ia, slines, startl, fHZ, sRate, p, 320) {
			mem = new AddressSpace(this, 16, 6);  // 7800: 16bit, 64byte pages

			cpu = new M6502(mem);
			cpu.RunClocksMultiple = 4;

			Maria = new Maria(this);
			mem.Map(0x0000, 0x0040, Maria);
			mem.Map(0x0100, 0x0040, Maria);
			mem.Map(0x0200, 0x0040, Maria);
			mem.Map(0x0300, 0x0040, Maria);

			pia = new PIA(this);
			mem.Map(0x0280, 0x0080, pia);
			mem.Map(0x0480, 0x0080, pia);
			mem.Map(0x0580, 0x0080, pia); // TODO : unsure about this one

			RAM1 = new RAM6116();
			mem.Map(0x1800, 0x0800, RAM1);

			RAM2 = new RAM6116();
			mem.Map(0x2000, 0x0800, RAM2);
			mem.Map(0x0040, 0x00c0, RAM2); // page 0 shadow
			mem.Map(0x0140, 0x00c0, RAM2); // page 1 shadow
			mem.Map(0x2800, 0x0800, RAM2); // shadow1
			mem.Map(0x3000, 0x0800, RAM2); // shadow2
			mem.Map(0x3800, 0x0800, RAM2); // shadow3

			// Insert the 7800 Highscore cartridge if requested
			if (EMU7800App.Instance.Settings.Use7800HSC) {
				HSC = new HSC7800();
				mem.Map(0x1000, 0x800, HSC.SRAM);
				mem.Map(0x3000, 0x1000, HSC);
				Trace.WriteLine("7800 Highscore Cartridge Installed");
			}

			Cart = c;
			mem.Map(0x4000, 0xc000, Cart);
		}

		public override M6502 CPU {
			get { return cpu; }
		}

		public void SwapInBIOS() {
			if (BIOS != null) {
				mem.Map((ushort)(0x10000 - BIOS.Size), BIOS.Size, BIOS);
			}
		}

		protected override void DoReset() {
			SwapInBIOS();
			if (HSC != null) {
				HSC.Reset();
			}
			Cart.Reset();
			Maria.Reset();
			pia.Reset();
			cpu.Reset();
		}

		public void SwapOutBIOS() {
			if (BIOS != null) {
				mem.Map((ushort)(0x10000 - BIOS.Size), BIOS.Size, Cart);
			}
		}

		protected override void DoRun() {
			Maria.StartFrame();
			for (int i = 0; i < Scanlines; i++) {
				CPU.RunClocks = CPU.RunClocksMultiple * 114;
				Maria.DoScanline(i);
				CPU.Execute();
				CPU.Clock += (ulong)(CPU.RunClocks / CPU.RunClocksMultiple);
			}
			Maria.EndFrame();
		}

		protected override void DoDone() {
			if (HSC != null) {
				HSC.SaveSRAM();
			}
		}
	}

	[Serializable]
	public class Machine7800NTSC : Machine7800 {
		public override string ToString() {
			return MachineType.A7800NTSC.ToString();
		}

		public Machine7800NTSC(Cart cart, InputAdapter ia)
			: base(cart, ia, 262, 16, 60, TIASound.NTSC_SAMPLES_PER_SEC, MariaTables.NTSCPalette) {
			if (!EMU7800App.Instance.Settings.Skip7800BIOS) {
				BIOS = new Bios7800(MachineType.A7800NTSC);
			}
			Trace.Write(this);
			Trace.WriteLine(" ready");
		}
	}

	[Serializable]
	public class Machine7800PAL : Machine7800 {
		public override string ToString() {
			return MachineType.A7800PAL.ToString();
		}

		public Machine7800PAL(Cart cart, InputAdapter ia)
			: base(cart, ia, 312, 32, 50, TIASound.PAL_SAMPLES_PER_SEC, MariaTables.PALPalette) {
			if (!EMU7800App.Instance.Settings.Skip7800BIOS) {
				BIOS = new Bios7800(MachineType.A7800PAL);
			}
			Trace.Write(this);
			Trace.WriteLine(" ready");
		}
	}

}
