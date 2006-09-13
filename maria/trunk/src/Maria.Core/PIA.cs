/*
 * The Peripheral Interface Adaptor (6532) device.
 * a.k.a. RIOT (RAM I/O Timer)
 *
 * Copyright (c) 2003, 2004 Mike Murphy
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
	public sealed class PIA : IDevice {
		private const int RamSize = 128;
		private Machine machine;
		private byte[] RAM = new byte[RamSize];
		private ulong TimerTarget;
		private int TimerShift;
		private bool IRQEnabled;
		private bool IRQTriggered;
		private byte DDRA;

		public PIA(Machine m) {
			machine = m;
		}

		public void Reset() {
			// Some games will loop/hang on $0284 if these are initialized to zero
			TimerShift = 10;
			TimerTarget = machine.CPU.Clock + (ulong)(0xff << TimerShift);
			IRQEnabled = false;
			IRQTriggered = false;
			DDRA = 0;
			Trace.Write(this);
			Trace.WriteLine(" reset");
		}

		public void Map(AddressSpace mem) {
			// Nothing to do
		}

		public override string ToString() {
			return "PIA/RIOT M6532";
		}

		public byte this[ushort addr] {
			get { return peek(addr); }
			set { poke(addr, value); }
		}

		public bool RequestSnooping { get { return false; } }

		public byte peek(ushort addr) {
			InputAdapter ia = machine.InputAdapter;
			if ((addr & 0x200) == 0) {
				return RAM[addr & 0x7f];
			}
			// A2 Distingusishes I/O registers from the Timer
			if ((addr & 0x04) != 0) {
				if ((addr & 0x01) != 0) {
					return ReadInterruptFlag();
				}
				else {
					return ReadTimerRegister();
				}
			}
			else {
				switch ((byte)(addr & 0x03)) {
					case 0:  // SWCHA: Controllers
						return ia.ReadPortA();
					case 1:	 // SWCHA DDR: 0=input, 1=output
						return DDRA;
					case 2:	 // SWCHB: Console switches
						return ia.ReadPortB();
					default: // SWCHB DDR, hardwired as input
						return 0;
				}
			}
		}

		public void poke(ushort addr, byte data) {
			InputAdapter ia = machine.InputAdapter;
			if ((addr & 0x200) == 0) {
				RAM[addr & 0x7f] = data;
				return;
			}

			// A2 Distingusishes I/O registers from the Timer
			if ((addr & 0x04) != 0) {
				if ((addr & 0x10) != 0) {
					IRQEnabled = (addr & 0x08) != 0;
					SetTimerRegister(data, addr & 0x03);
				}
			}
			else {
				switch ((byte)(addr & 0x03)) {
					case 0: // SWCHA
						ia.WritePortA((byte)(data & DDRA));
						break;
					case 1: // SWCHA DDR
						DDRA = data;
						break;
                    default: // SWCHA hardwired as input
						break;
				}
			}
		}

		// 0: TIM1T:  set    1 clock interval (838 nsec/interval)
		// 1: TIM8T:  set    8 clock interval (6.7 usec/interval)
		// 2: TIM64T: set   64 clock interval (53.6 usec/interval)
		// 3: T1024T: set 1024 clock interval (858.2 usec/interval)
		private static readonly int[] timerShiftTable = new int[] {0, 3, 6, 10};
		void SetTimerRegister(byte data, int interval) {
			IRQTriggered = false;
			TimerShift = timerShiftTable[interval];
			TimerTarget = machine.CPU.Clock + (ulong)(data << TimerShift);
		}

		byte ReadTimerRegister() {
			IRQTriggered = false;
			int delta = (int)(TimerTarget - machine.CPU.Clock);
			if (delta >= 0) {
				return (byte)(delta >> TimerShift);
			}
			else {
				if (delta != -1) {
					IRQTriggered = true;
				}
				return (byte)(delta >= -256 ? delta : 0);
			}
		}

		byte ReadInterruptFlag() {
			int delta = (int)(TimerTarget - machine.CPU.Clock);
			if (delta >= 0 || IRQEnabled && IRQTriggered) {
				return 0x00;
			}
			else {
				return 0x80;
			}
		}
	}
}
