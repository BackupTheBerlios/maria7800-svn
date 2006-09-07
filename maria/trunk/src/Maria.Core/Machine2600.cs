/*
 * The realization of a 2600 machine.
 *
 * Copyright (C) 2003, 2004 Mike Murphy
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
	public abstract class Machine2600 : Machine {
		protected TIA tia;

		public Machine2600(Cart c, InputAdapter ia, int slines, int startl, int fHZ, int sRate, int[] p)
			: base(ia, slines, startl, fHZ, sRate, p, 160) {
			mem = new AddressSpace(this, 13, 6);  // 2600: 13bit, 64byte pages

			cpu = new M6502(mem);
			cpu.RunClocksMultiple = 1;

			tia = new TIA(this);
			for (ushort i = 0; i < 0x1000; i += 0x100) {
				mem.Map(i, 0x0080, tia);
			}

			pia = new PIA(this);
			for (ushort i = 0x0080; i < 0x1000; i += 0x100) {
				mem.Map(i, 0x0080, pia);
			}

			mem.Map(0x1000, 0x1000, c);
		}

		public override M6502 CPU {
			get { return cpu; }
		}

		protected override void DoReset() {
			tia.Reset();
			pia.Reset();
			cpu.Reset();
		}

		protected override void DoRun() {
			tia.StartFrame();
			cpu.RunClocks = (Scanlines + 3) * 76;
			while (cpu.RunClocks > 0 && !CPU.Jammed) {
				if (tia.WSYNCDelayClocks > 0) {
					cpu.Clock += (ulong) tia.WSYNCDelayClocks / 3;
					cpu.RunClocks -= tia.WSYNCDelayClocks / 3;
					tia.WSYNCDelayClocks = 0;
				}
				if (tia.EndOfFrame) {
					break;
				}
				cpu.Execute();
			}
			tia.EndFrame();
		}

		protected override void DoDone() {
			// Nothing to do.
		}
	}

	[Serializable]
	public class Machine2600NTSC : Machine2600 {

		public Machine2600NTSC(Cart cart, InputAdapter ia)
			: base(cart, ia, 262, 16, 60, TIASound.NTSC_SAMPLES_PER_SEC, TIATables.NTSCPalette) {
			Trace.Write(this);
			Trace.WriteLine(" ready");
		}

		public override string ToString() {
			return MachineType.A2600NTSC.ToString();
		}
	}

	[Serializable]
	public class Machine2600PAL : Machine2600 {

		public Machine2600PAL(Cart cart, InputAdapter ia)
			: base(cart, ia, 312, 32, 50, TIASound.PAL_SAMPLES_PER_SEC, TIATables.PALPalette) {
			Trace.Write(this);
			Trace.WriteLine(" ready");
		}

		public override string ToString() {
			return MachineType.A2600PAL.ToString();
		}
	}
}
