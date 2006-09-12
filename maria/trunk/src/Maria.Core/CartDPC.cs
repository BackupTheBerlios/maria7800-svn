/*
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
using System.IO;

namespace Maria.Core {
	/*
	 * Pitfall II cartridge.  There are two 4k banks, 2k display bank, and the DPC chip.
	 * For complete details on the DPC chip see David P. Crane's United States Patent
	 * Number 4,644,495.
	 * 
	 * Cart Format                Mapping to ROM Address Space
	 * Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff8,0x1ff9
	 * Bank2: 0x1000:0x1000
	 */
	[Serializable]
	public sealed class CartDPC : Cart {
		private byte[] MusicAmplitudes = new byte[] {0x00, 0x04, 0x05, 0x09, 0x06, 0x0a, 0x0b, 0x0f};
		private const ushort DisplayBaseAddr = 0x2000;
		private ushort BankBaseAddr;
		private byte[] Tops = new byte[8];
		private byte[] Bots = new byte[8];
		private ushort[] Counters = new ushort[8];
		private byte[] Flags = new byte[8];
		private bool[] MusicMode = new bool[3];
		private ulong LastSystemClock;
		private double FractionalClocks;
		private byte _ShiftRegister;

		public CartDPC(BinaryReader br) {
			LoadRom(br, 0x2800);
			Bank = 1;
		}

		public override void Reset() {
			Bank = 1;
			LastSystemClock = 3 * Machine.CPU.Clock;
			FractionalClocks = 0.0;
			ShiftRegister = 1;
		}

		public override byte this[ushort addr] {
			get {
				addr &= 0x0fff;
				if (addr < 0x0040) {
					return ReadPitfall2Reg(addr);
				}
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set {
				addr &= 0x0fff;
				if (addr >= 0x0040 && addr < 0x0080) {
					WritePitfall2Reg(addr, value);
				}
				else {
					UpdateBank(addr);
				}
			}
		}

		private int Bank {
			set { BankBaseAddr = (ushort)(value * 0x1000); }
		}

		// Generate a sequence of pseudo-random numbers 255 numbers long
		// by emulating an 8-bit shift register with feedback taps at
		// bits 4, 3, 2, and 0.
		byte ShiftRegister {
			get {
				byte a, x;
				a = _ShiftRegister;
				a &= (1 << 0);

				x = _ShiftRegister;
				x &= (1 << 2);
				x >>= 2;
				a ^= x;

				x = _ShiftRegister;
				x &= (1 << 3);
				x >>= 3;
				a ^= x;

				x = _ShiftRegister;
				x &= (1 << 4);
				x >>= 4;
				a ^= x;

				a <<= 7;
				_ShiftRegister >>= 1;
				_ShiftRegister |= a;

				return _ShiftRegister;
			}
			set {
				_ShiftRegister = value;
			}
		}

		private void UpdateBank(ushort addr) {
			switch(addr) {
				case 0x0ff8:
					Bank = 0;
					break;
				case 0x0ff9:
					Bank = 1;
					break;
			}
		}

		private byte ReadPitfall2Reg(ushort addr) {
			byte result;

			int i = addr & 0x07;
			int fn = (addr >> 3) & 0x07;

			// Update flag register for selected data fetcher
			if ((Counters[i] & 0x00ff) == Tops[i]) {
				Flags[i] = 0xff;
			}
			else if ((Counters[i] & 0x00ff) == Bots[i]) {
				Flags[i] = 0x00;
			}

			switch (fn) {
				case 0x00:
					if (i < 4) { // This is a random number read
						result = ShiftRegister;
						break;
					}
					// Its a music read
					UpdateMusicModeDataFetchers();

					byte j = 0;
					if (MusicMode[0] == true && Flags[5] != 0) {
						j |= 0x01;
					}
					if (MusicMode[1] == true && Flags[6] != 0) {
						j |= 0x02;
					}
					if (MusicMode[2] == true && Flags[7] != 0) {
						j |= 0x04;
					}
					result = MusicAmplitudes[j];
					break;
					// DFx display data read
				case 0x01:
					result = ROM[DisplayBaseAddr + 0x7ff - Counters[i]];
					break;
					// DFx display data read AND'd w/flag
				case 0x02:
					result = ROM[DisplayBaseAddr + 0x7ff - Counters[i]];
					result &= Flags[i];
					break;
					// DFx flag
				case 0x07:
					result = Flags[i];
					break;
				default:
					result = 0;
					break;
			}

			// Clock the selected data fetcher's counter if needed
			if (i < 5 || i >= 5 && MusicMode[i - 5] == false) {
				Counters[i]--;
				Counters[i] &= 0x07ff;
			}

			return result;
		}

		private void UpdateMusicModeDataFetchers() {
			ulong sysClockDelta = 3 * Machine.CPU.Clock - LastSystemClock;
			LastSystemClock = 3 * Machine.CPU.Clock;

			double OSCclocks = ((15750.0 * sysClockDelta) / 1193182.0)
				+ FractionalClocks;

			int wholeClocks = (int)OSCclocks;

			FractionalClocks = OSCclocks - (double)wholeClocks;

			if (wholeClocks <= 0) {
				return;
			}

			for (int i=0; i < 3; i++) {
				int r = i + 5;
				if (!MusicMode[i]) {
					continue;
				}

				int top = Tops[r] + 1;
				int newLow = Counters[r] & 0x00ff;

				if (Tops[r] != 0) {
					newLow -= (wholeClocks % top);
					if (newLow < 0) {
						newLow += top;
					}
				}
				else {
					newLow = 0;
				}

				if (newLow <= Bots[r]) {
					Flags[r] = 0x00;
				}
				else if (newLow <= Tops[r]) {
					Flags[r] = 0xff;
				}

				Counters[r] = (ushort)((Counters[r] & 0x0700) | (ushort)newLow);
			}
		}

		private void WritePitfall2Reg(ushort addr, byte val) {
			int i = addr & 0x07;
			int fn = (addr >> 3) & 0x07;

			switch (fn) {
					// DFx top count
				case 0x00:
					Tops[i] = val;
					Flags[i] = 0x00;
					break;
					// DFx bottom count
				case 0x01:
					Bots[i] = val;
					break;
					// DFx counter low
				case 0x02:
					Counters[i] &= 0x0700;
					if (i >= 5 && MusicMode[i - 5] == true) {
						// Data fetcher is in music mode so its low counter value
						// should be loaded from the top register not the poked value
						Counters[i] |= Tops[i];
					}
					else {
						// Data fetcher is either not a music mode data fetcher or it
						// isn't in music mode so it's low counter value should be loaded
						// with the poked value
						Counters[i] |= val;
					}
					break;
					// DFx counter high
				case 0x03:
					Counters[i] &= 0x00ff;
					Counters[i] |= (ushort)((val & 0x07) << 8);
					// Execute special code for music mode data fetchers
					if (i >= 5) {
						MusicMode[i - 5] = (val & 0x10) != 0;
						// NOTE: We are not handling the clock source input for
						// the music mode data fetchers.  We're going to assume
						// they always use the OSC input.
					}
					break;
					// Random Number Generator Reset
				case 0x06:
					ShiftRegister = 1;
					break;
				default:
					break;
			}
		}
	}
}
