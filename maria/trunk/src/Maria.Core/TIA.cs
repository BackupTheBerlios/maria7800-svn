/*
 * The Television Interface Adaptor device.
 *
 * Copyright (c) 2003, 2004 Mike Murphy
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
using System.Runtime.Serialization;

namespace Maria.Core {

	[Serializable]
	struct HMOVERegister {
		private byte _Data;
		public bool MoreMotionReq;

		public byte Data {
			get { return _Data; }
			set {
				_Data = value;
				_Data ^= 0x80;
				_Data >>= 4;
				_Data &= 0x0f;
			}
		}

		public void DoCounterCompare(int hmCounter) {
			if (((hmCounter & 0x0f) ^ _Data) == 0x0f)
				MoreMotionReq = false;
		}
	}

	[Serializable]
	public class TIA : IDevice, IDeserializationCallback {
		const int
			VSYNC = 0x00,   // Write: vertical sync set-clear (D1)
			VBLANK = 0x01,  // Write: vertical blank set-clear (D7-6,D1)
			WSYNC = 0x02,   // Write: wait for leading edge of hrz. blank (strobe)
			RSYNC = 0x03,   // Write: reset hrz. sync counter (strobe)
			NUSIZ0 = 0x04,  // Write: number-size player-missle 0 (D5-0)
			NUSIZ1 = 0x05,  // Write: number-size player-missle 1 (D5-0)
			COLUP0 = 0x06,  // Write: color-lum player 0 (D7-1)
			COLUP1 = 0x07,  // Write: color-lum player 1 (D7-1)
			COLUPF = 0x08,  // Write: color-lum playfield (D7-1)
			COLUBK = 0x09,  // Write: color-lum background (D7-1)
			CTRLPF = 0x0a,  // Write: cntrl playfield ballsize & coll. (D5-4,D2-0)
			REFP0 = 0x0b,   // Write: reflect player 0 (D3)
			REFP1 = 0x0c,   // Write: reflect player 1 (D3)
			PF0 = 0x0d,     // Write: playfield register byte 0 (D7-4)
			PF1 = 0x0e,     // Write: playfield register byte 1 (D7-0)
			PF2 = 0x0f,     // Write: playfield register byte 2 (D7-0)
			RESP0 = 0x10,   // Write: reset player 0 (strobe)
			RESP1 = 0x11,   // Write: reset player 1 (strobe)
			RESM0 = 0x12,   // Write: reset missle 0 (strobe)
			RESM1 = 0x13,   // Write: reset missle 1 (strobe)
			RESBL = 0x14,   // Write: reset ball (strobe)
			AUDC0 = 0x15,   // Write: audio control 0 (D3-0)
			AUDC1 = 0x16,   // Write: audio control 1 (D4-0)
			AUDF0 = 0x17,   // Write: audio frequency 0 (D4-0)
			AUDF1 = 0x18,   // Write: audio frequency 1 (D3-0)
			AUDV0 = 0x19,   // Write: audio volume 0 (D3-0)
			AUDV1 = 0x1a,   // Write: audio volume 1 (D3-0)
			GRP0 = 0x1b,    // Write: graphics player 0 (D7-0)
			GRP1 = 0x1c,    // Write: graphics player 1 (D7-0)
			ENAM0 = 0x1d,   // Write: graphics (enable) missle 0 (D1)
			ENAM1 = 0x1e,   // Write: graphics (enable) missle 1 (D1)
			ENABL = 0x1f,   // Write: graphics (enable) ball (D1)
			HMP0 = 0x20,    // Write: horizontal motion player 0 (D7-4)
			HMP1 = 0x21,    // Write: horizontal motion player 1 (D7-4)
			HMM0 = 0x22,    // Write: horizontal motion missle 0 (D7-4)
			HMM1 = 0x23,    // Write: horizontal motion missle 1 (D7-4)
			HMBL = 0x24,    // Write: horizontal motion ball (D7-4)
			VDELP0 = 0x25,  // Write: vertical delay player 0 (D0)
			VDELP1 = 0x26,  // Write: vertical delay player 1 (D0)
			VDELBL = 0x27,  // Write: vertical delay ball (D0)
			RESMP0 = 0x28,  // Write: reset missle 0 to player 0 (D1)
			RESMP1 = 0x29,  // Write: reset missle 1 to player 1 (D1)
			HMOVE = 0x2a,   // Write: apply horizontal motion (strobe)
			HMCLR = 0x2b,   // Write: clear horizontal motion registers (strobe)
			CXCLR = 0x2c,   // Write: clear collision latches (strobe)
			CXM0P = 0x0,    // Read collision: D7=(M0,P1); D6=(M0,P0)
			CXM1P = 0x1,    // Read collision: D7=(M1,P0); D6=(M1,P1)
			CXP0FB = 0x2,   // Read collision: D7=(P0,PF); D6=(P0,BL)
			CXP1FB = 0x3,   // Read collision: D7=(P1,PF); D6=(P1,BL)
			CXM0FB = 0x4,   // Read collision: D7=(M0,PF); D6=(M0,BL)
			CXM1FB = 0x5,   // Read collision: D7=(M1,PF); D6=(M1,BL)
			CXBLPF = 0x6,   // Read collision: D7=(BL,PF); D6=(unused)
			CXPPMM = 0x7,   // Read collision: D7=(P0,P1); D6=(M0,M1)
			INPT0 = 0x8,    // Read pot port: D7
			INPT1 = 0x9,    // Read pot port: D7
			INPT2 = 0xa,    // Read pot port: D7
			INPT3 = 0xb,    // Read pot port: D7
			INPT4 = 0xc,    // Read P1 joystick trigger: D7
			INPT5 = 0xd;    // Read P2 joystick trigger: D7

		[Flags]
		public enum CxFlags : int {
			PF = 1 << 0,
			BL = 1 << 1,
			M0 = 1 << 2,
			M1 = 1 << 3,
			P0 = 1 << 4,
			P1 = 1 << 5
		};

		[Flags]
		public enum CxPairFlags : int {
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

		public int WSYNCDelayClocks;
		public bool EndOfFrame;
		private TIASound TIASound;
		private byte[] ScanlineBuffer = new byte[160];
		private int FBindex;
		private Machine machine;
		private AddressSpace mem;
		private byte[] RegW = new byte[0x40];
		private ulong FrameStartClock;
		private ulong FrameStopClock;
		private ulong FrameLastClock;
		private ulong LastHMOVEClock;
		private int RemainingScanlineClocks;
		private uint PF210;
		private int PFReflectionState;
		private bool OldENABL;
		private int BLpos, BLsize;
		private byte EffGRP0, OldGRP0;
		private byte EffGRP1, OldGRP1;
		private int P0pos, P0type, P0suppress;
		private int P1pos, P1type, P1suppress;
		private int M0pos, M0size, M0type;
		private int M1pos, M1size, M1type;
		private bool LRHBOn, LateHMOVEBlankOn;
		private int HMOVECounter;
		private HMOVERegister HMRP0, HMRP1, HMRM0, HMRM1, HMRBL;
		private bool vblankon, scoreon, pfpriority;
		private bool p0on, p1on, m0on, m1on, blon;
		private byte colubk, colupf, colup0, colup1;
		private bool DumpEnabled;
		private ulong DumpDisabledCycle;
		private CxPairFlags Collisions;
		[NonSerialized]
		private int[] PokeDelay;
		[NonSerialized]
		private PokeOpTyp[] PokeOp;

		delegate void PokeOpTyp(ushort addr, byte data);

		public TIA(Machine m) {
			this.machine = m;
			BuildPokeOpTable();
			PokeDelay = BuildPokeDelayTable();
			TIASound = new TIASound(machine);
		}

		public void Reset() {
			for (int i = 0; i < RegW.Length; i++) {
				RegW[i] = 0;
			}
			vblankon = false;
			scoreon = false;
			pfpriority = false;
			HMRP0.MoreMotionReq = false;
			HMRP1.MoreMotionReq = false;
			HMRM0.MoreMotionReq = false;
			HMRM1.MoreMotionReq = false;
			HMRBL.MoreMotionReq = false;
			p0on = false;
			p1on = false;
			m0on = false;
			m1on = false;
			blon = false;
			colubk = colupf = colup0 = colup1 = 0;
			PFReflectionState = 0;
			TIASound.Reset();
			Trace.Write(this);
			Trace.WriteLine(" reset");
		}

		public ulong Clock {
			get { return 3 * machine.CPU.Clock; }
		}

		public bool RequestSnooping {
			get { return false; }
		}

		public byte this[ushort addr] {
			get { return peek(addr); }
			set { poke(addr, value); }
		}

		public void Map(AddressSpace mem) {
			this.mem = mem;
		}

		public override String ToString() {
			return "TIA 1A";
		}

		public void StartFrame() {
			WSYNCDelayClocks = 0;
			EndOfFrame = false;
			FrameStartClock = Clock - ((Clock - FrameStartClock) % 228);
			FrameStopClock = FrameStartClock + (ulong)(228 * machine.Scanlines);
			FrameLastClock = FrameStartClock;
			LastHMOVEClock = 0;
			LRHBOn = false;
			LateHMOVEBlankOn = false;
			RemainingScanlineClocks = 228;
			FBindex = 0;
			TIASound.StartFrame();
		}

		public void EndFrame() {
			TIASound.EndFrame();
		}

		byte DumpedInputPort(int resistance) {
			byte retval = 0;
			if (resistance == 0) {
				retval = 0x80;
			}
			else if (DumpEnabled || resistance == Int32.MaxValue) {
				retval = 0x00;
			}
			else {
				double charge_time = 1.6 * resistance * 0.01e-6;
				ulong needed = (ulong)(charge_time * machine.Scanlines * 228 * machine.FrameHZ / 3);
				if (machine.CPU.Clock > DumpDisabledCycle + needed) {
					retval = 0x80;
				}
			}
			return retval;
		}

		byte peek(ushort addr) {
			InputAdapter ia = machine.InputAdapter;
			int retval = 0;
			int scanline, hpos;
			addr &= 0xf;
			UpdateFrame(Clock);
			switch (addr) {
				case CXM0P:
					retval |= ((Collisions & CxPairFlags.M0P1) != 0 ? 0x80 : 0);
					retval |= ((Collisions & CxPairFlags.M0P0) != 0 ? 0x40 : 0);
					break;
				case CXM1P:
					retval |= ((Collisions & CxPairFlags.M1P0) != 0 ? 0x80 : 0);
					retval |= ((Collisions & CxPairFlags.M1P1) != 0 ? 0x40 : 0);
					break;
				case CXP0FB:
					retval |= ((Collisions & CxPairFlags.P0PF) != 0 ? 0x80 : 0);
					retval |= ((Collisions & CxPairFlags.P0BL) != 0 ? 0x40 : 0);
					break;
				case CXP1FB:
					retval |= ((Collisions & CxPairFlags.P1PF) != 0 ? 0x80 : 0);
					retval |= ((Collisions & CxPairFlags.P1BL) != 0 ? 0x40 : 0);
					break;
				case CXM0FB:
					retval |= ((Collisions & CxPairFlags.M0PF) != 0 ? 0x80 : 0);
					retval |= ((Collisions & CxPairFlags.M0BL) != 0 ? 0x40 : 0);
					break;
				case CXM1FB:
					retval |= ((Collisions & CxPairFlags.M1PF) != 0 ? 0x80 : 0);
					retval |= ((Collisions & CxPairFlags.M1BL) != 0 ? 0x40 : 0);
					break;
				case CXBLPF:
					retval |= ((Collisions & CxPairFlags.BLPF) != 0 ? 0x80 : 0);
					break;
				case CXPPMM:
					retval |= ((Collisions & CxPairFlags.P0P1) != 0 ? 0x80 : 0);
					retval |= ((Collisions & CxPairFlags.M0M1) != 0 ? 0x40 : 0);
					break;
				case INPT0:
					retval = DumpedInputPort(ia.ReadINPT0());
					break;
				case INPT1:
					retval = DumpedInputPort(ia.ReadINPT1());
					break;
				case INPT2:
					retval = DumpedInputPort(ia.ReadINPT2());
					break;
				case INPT3:
					retval = DumpedInputPort(ia.ReadINPT3());
					break;
				case INPT4:
					scanline = (int)(Clock - FrameStartClock) / 228;
					hpos = (int)(Clock - FrameStartClock) % 228 - 68;
					if (hpos < 0) {
						hpos += 228;
						scanline--;
					}
					if (ia.ReadINPT4(scanline, hpos)) {
						retval &= 0x7f;
					}
					else {
						retval |= 0x80;
					}
					break;
				case INPT5:
					scanline = (int)(Clock - FrameStartClock) / 228;
					hpos = (int)(Clock - FrameStartClock) % 228 - 68;
					if (hpos < 0) {
						hpos += 228;
						scanline--;
					}
					if (ia.ReadINPT5(scanline, hpos)) {
						retval &= 0x7f;
					}
					else {
						retval |= 0x80;
					}
					break;
			}
			return (byte)(retval | (mem.DataBusState & 0x3f));
		}

		void poke(ushort addr, byte data) {
			addr &= 0x3f;
			// TODO: Unsure if this has any meaningful effect
			int delay = PokeDelay[addr];
			if (delay == -1) {
				int hclk = (int)(Clock - FrameStartClock) % 228;
				delay = new int[] { 4, 3, 2, 5 }[hclk & 3];
			}
			if (delay >= 0) {
				UpdateFrame(Clock + (ulong)delay);
			}
			PokeOp[addr](addr, data);
		}

		public virtual void OnDeserialization(object sender) {
			BuildPokeOpTable();
			PokeDelay = BuildPokeDelayTable();
		}

		void DoVisibleScanline(int updateClocks, int hposStart) {
			int hpos = hposStart;
			// emulate polynomial counters for each of the objects
			// 160 is added to prevent negative values
			// each counter is mod'd w/160 to provide wraparound
			int p0c = hpos - P0pos + 160;
			int p1c = hpos - P1pos + 160;
			int m0c = hpos - M0pos + 160;
			int m1c = hpos - M1pos + 160;
			int blc = hpos - BLpos + 160;

			CxFlags cxflags;

			int SLindex = FBindex % 160;
			int fidx_end = SLindex + updateClocks;

			byte fbyte;
			byte fbyte_colupf = colupf;
			bool colupfon;

			// crt gun is off
			if (vblankon) {
				for (int fidx = SLindex; fidx < fidx_end; fidx++) {
					ScanlineBuffer[fidx] = 0;
				}
				goto done;
			}

			for (int fidx = SLindex; fidx < fidx_end; fidx++, hpos++, p0c++, p1c++, m0c++, m1c++, blc++) {
				if (LateHMOVEBlankOn) {
					if (hpos < 8) {
						ScanlineBuffer[fidx] = 0;
						continue;
					}
					else {
						LateHMOVEBlankOn = false;
					}
				}

				if (hpos == 76) {
					PFReflectionState = RegW[CTRLPF] & 0x01;
				}

				fbyte = colubk;
				cxflags = 0;

				// provide wraparound for the counters
				p0c %= 160;
				p1c %= 160;
				m0c %= 160;
				m1c %= 160;
				blc %= 160;

				colupfon = false;
				if ((PF210 & TIATables.PFMask[PFReflectionState, hpos]) != 0) {
					if (scoreon) {
						fbyte_colupf = hpos < 80 ? colup0 : colup1;
					}
					colupfon = true;
					cxflags |= CxFlags.PF;
				}
				if (blon && TIATables.BLMask[BLsize, blc]) {
					colupfon = true;
					cxflags |= CxFlags.BL;
				}

				if (!pfpriority && colupfon) {
					fbyte = fbyte_colupf;
				}
				if (m1on && TIATables.MxMask[M1size, M1type, m1c]) {
					fbyte = colup1;
					cxflags |= CxFlags.M1;
				}
				if (p1on && (TIATables.PxMask[P1suppress, P1type, p1c] & EffGRP1) != 0) {
					fbyte = colup1;
					cxflags |= CxFlags.P1;
				}
				if (m0on && TIATables.MxMask[M0size, M0type, m0c]) {
					fbyte = colup0;
					cxflags |= CxFlags.M0;
				}
				if (p0on && (TIATables.PxMask[P0suppress, P0type, p0c] & EffGRP0) != 0) {
					fbyte = colup0;
					cxflags |= CxFlags.P0;
				}

				if (pfpriority && colupfon) {
					fbyte = fbyte_colupf;
				}

				Collisions |= TIATables.CollisionMask[(int)cxflags];

				ScanlineBuffer[fidx] = fbyte;
			}
		done:
			if (machine.Host != null) {
				machine.Host.UpdateDisplay(ScanlineBuffer, FBindex / 160, hposStart, updateClocks);
			}
		}

		void UpdateFrame(ulong clock) {
			if (clock < FrameStartClock || clock < FrameLastClock) {
				return;
			}
			if (FrameLastClock >= FrameStopClock) {
				return;
			}
			if (clock >= FrameStopClock) {
				clock = FrameStopClock;
			}
			int ProcessedScanlineClocks;
			int HBLANKclocks, UPDATEclocks, hposStart;
			do {
				ProcessedScanlineClocks = 228 - RemainingScanlineClocks;
				if (clock > (FrameLastClock + (ulong)RemainingScanlineClocks)) {
					// If more than one scanline to update,
					// then just finish the first complete scanline
					UPDATEclocks = RemainingScanlineClocks;
					RemainingScanlineClocks = 228;
				}
				else {
					// Do as much of current scanline as possible
					UPDATEclocks = (int)(clock - FrameLastClock);
					RemainingScanlineClocks -= UPDATEclocks;
				}

				ServiceHMOVE(FrameLastClock, ProcessedScanlineClocks, UPDATEclocks);

				FrameLastClock += (ulong)UPDATEclocks;

				HBLANKclocks = 0;
				if (ProcessedScanlineClocks < 68) {
					HBLANKclocks = 68 - ProcessedScanlineClocks;
					if (HBLANKclocks >= UPDATEclocks) {
						HBLANKclocks = UPDATEclocks;
					}
					ProcessedScanlineClocks += HBLANKclocks;
					UPDATEclocks -= HBLANKclocks;
				}

				if (UPDATEclocks > 0) {
					hposStart = ProcessedScanlineClocks - 68;
					DoVisibleScanline(UPDATEclocks, hposStart);
					FBindex += UPDATEclocks;
				}

				// Check whether we are at the end of a scanline
				if (RemainingScanlineClocks == 228) {
					P0suppress = 0;
					P1suppress = 0;
				}
			} while (FrameLastClock < clock);
		}

		void ServiceHMOVE(ulong clk, int hclk, int UPDATEclocks) {
			int end_hclk = hclk + UPDATEclocks;

			for (; hclk < end_hclk; clk++, hclk++) {
				if (clk < LastHMOVEClock) {
					continue;
				}
				else if (clk == LastHMOVEClock) {
					HMOVECounter = 0x0f;
					HMRP0.MoreMotionReq = true;
					HMRP1.MoreMotionReq = true;
					HMRM0.MoreMotionReq = true;
					HMRM1.MoreMotionReq = true;
					HMRBL.MoreMotionReq = true;

					LRHBOn = (hclk < 64) ? true : false;
				}
				if (LRHBOn && hclk >= 68 && hclk < 76) {
					HMOVEit(ref P0pos, 1);
					HMOVEit(ref P1pos, 1);
					HMOVEit(ref M0pos, 1);
					HMOVEit(ref M1pos, 1);
					HMOVEit(ref BLpos, 1);
				}
				if (hclk > 76) {
					LRHBOn = false;
				}
				if ((hclk & 0x03) == 0) {
					HMRP0.DoCounterCompare(HMOVECounter);
					HMRP1.DoCounterCompare(HMOVECounter);
					HMRM0.DoCounterCompare(HMOVECounter);
					HMRM1.DoCounterCompare(HMOVECounter);
					HMRBL.DoCounterCompare(HMOVECounter);
					if (HMOVECounter >= 0) {
						HMOVECounter--;
					}
				}
				if (HMOVECounter >= 0x0f) {
					continue;
				}
				if ((hclk & 0x03) == 2) {
					if (LRHBOn && hclk < 74 || hclk < 64 || hclk > 225) {
						if (HMRP0.MoreMotionReq) HMOVEit(ref P0pos, -1);
						if (HMRP1.MoreMotionReq) HMOVEit(ref P1pos, -1);
						if (HMRM0.MoreMotionReq) HMOVEit(ref M0pos, -1);
						if (HMRM1.MoreMotionReq) HMOVEit(ref M1pos, -1);
						if (HMRBL.MoreMotionReq) HMOVEit(ref BLpos, -1);
					}
				}
			}
		}

		byte opNULL(ushort addr) {
			return 0x00;
		}

		void opNULL(ushort addr, byte data)
		{
		}

		void opVSYNC(ushort addr, byte data) {
			//
			// Many games don't appear to supply 3 scanlines of
			// VSYNC in accordance with the Atari documentation.
			// Enduro turns on VSYNC, then turns it off twice.
			// One of the Atari Bowling ROMs turns it off, but never turns it on.
			// So, we always end the frame if VSYNC is turned on and then off.
			// We also end the frame if VSYNC is simply turned off during the bottom
			// half of the frame.
			//
			if ((data & 0x02) == 0) {
				bool atBottomHalf = (int)((Clock - FrameStartClock) / 228) > (machine.Scanlines >> 1);
				if ((RegW[VSYNC] & 0x02) != 0 || atBottomHalf) {
					// End of frame, halt CPU
					EndOfFrame = true;
					machine.CPU.EmulatorPreemptRequest = true;
				}
			}
			RegW[VSYNC] = data;
		}

		void opVBLANK(ushort addr, byte data) {
			if ((RegW[VBLANK] & 0x80) == 0) {
				// dump to ground is clear and will be set
				// thus discharging all INPTx capacitors
				if ((data & 0x80) != 0) {
					DumpEnabled = true;
				}
			}
			else {
				// dump to ground is set and will be cleared
				// thus starting all INPTx capacitors charging
				if ((data & 0x80) == 0) {
					DumpEnabled = false;
					DumpDisabledCycle = machine.CPU.Clock;
				}
			}
			RegW[VBLANK] = data;
			vblankon = (data & 0x02) != 0;
		}

		void opWSYNC(ushort addr, byte data) {
			int wsyncDelayClocks = (int)(228 - ((Clock - FrameStartClock) % 228));
			if (wsyncDelayClocks < 228) {
				// Report the number of system clocks to delay the CPU
				WSYNCDelayClocks = wsyncDelayClocks;
				// Request a CPU preemption to service the delay request
				machine.CPU.EmulatorPreemptRequest = true;
			}
		}

		void opNUSIZ0(ushort addr, byte data)
		{
			RegW[NUSIZ0] = (byte)(data & 0x37);

			M0size = (RegW[NUSIZ0] & 0x30) >> 4;
			M0type = RegW[NUSIZ0] & 0x07;
			P0type = M0type;
			P0suppress = 0;
		}

		void opNUSIZ1(ushort addr, byte data)
		{
			RegW[NUSIZ1] = (byte)(data & 0x37);

			M1size = (RegW[NUSIZ1] & 0x30) >> 4;
			M1type = RegW[NUSIZ1] & 0x07;
			P1type = M1type;
			P1suppress = 0;
		}

		void opCOLUBK(ushort addr, byte data)
		{
			colubk = data;
		}

		void opCOLUPF(ushort addr, byte data)
		{
			colupf = data;
		}

		void opCOLUP0(ushort addr, byte data)
		{
			colup0 = data;
		}

		void opCOLUP1(ushort addr, byte data)
		{
			colup1 = data;
		}

		void opCTRLPF(ushort addr, byte data)
		{
			RegW[CTRLPF] = data;

			BLsize = (data & 0x30) >> 4;
			scoreon = (data & 0x02) != 0;
			pfpriority = (data & 0x04) != 0;
		}

		void SetEffGRP0()
		{
			byte grp0 = RegW[VDELP0] != 0 ? OldGRP0 : RegW[GRP0];
			EffGRP0 = RegW[REFP0] != 0 ? TIATables.GRPReflect[grp0] : grp0;

			p0on = EffGRP0 != 0;
		}

		void SetEffGRP1()
		{
			byte grp1 = RegW[VDELP1] != 0 ? OldGRP1 : RegW[GRP1];
			EffGRP1 = RegW[REFP1] != 0 ? TIATables.GRPReflect[grp1] : grp1;

			p1on = EffGRP1 != 0;
		}

		void Setblon()
		{
			blon = RegW[VDELBL] != 0 ? OldENABL : RegW[ENABL] != 0;
		}

		void opREFP0(ushort addr, byte data)
		{
			RegW[REFP0] = (byte)(data & 0x08);
			SetEffGRP0();
		}

		void opREFP1(ushort addr, byte data)
		{
			RegW[REFP1] = (byte)(data & 0x08);
			SetEffGRP1();
		}

		void opPF(ushort addr, byte data)
		{
			RegW[addr] = data;

			PF210 = (uint)((RegW[PF2] << 12)
						 | (RegW[PF1] << 4)
						 |((RegW[PF0] >> 4) & 0x0f));
		}

		void opRESP0(ushort addr, byte data)
		{
			int hpos = (int)(Clock - FrameStartClock) % 228;
			int newP0pos = (hpos < 68) ? 0 : ((hpos - 68 + 4) % 160);

			P0suppress = 1;
			P0pos = newP0pos;

			// TODO: Positioning cheat for Activision's Grand Prix
			// This is a player reset during the extended late HMOVE blank period,
			// and I'm not sure exactly what happens here.
			if (LateHMOVEBlankOn && hpos == 72 && (Clock - FrameStartClock) / 228 == 200)
			{
				P0pos -= 2;
				Debug.WriteLine(String.Format("P0pos GrandPrix CHEAT: hpos:{0} LateHMOVEBlankOn:{1}", hpos, LateHMOVEBlankOn));
			}
		}

		void opRESP1(ushort addr, byte data)
		{
			int hpos = (int)(Clock - FrameStartClock) % 228;
			int newP1pos = (hpos < 68) ? 0 : ((hpos - 68 + 4) % 160);

			P1suppress = 1;
			P1pos = newP1pos;
		}

		void opRESM0(ushort addr, byte data)
		{
			int hpos = (int)(Clock - FrameStartClock) % 228;
			M0pos = (hpos < 68) ? 0 : ((hpos - 68 + 4) % 160);
		}

		void opRESM1(ushort addr, byte data)
		{
			int hpos = (int)(Clock - FrameStartClock) % 228;
			M1pos = (hpos < 68) ? 0 : ((hpos - 68 + 4) % 160);
		}

		void opRESBL(ushort addr, byte data)
		{
			int hpos = (int)(Clock - FrameStartClock) % 228;
			BLpos = (hpos < 68) ? 0 : ((hpos - 68 + 4) % 160);
		}

		void opAUD(ushort addr, byte data)
		{
			RegW[addr] = data;
			TIASound.Update(addr, data);
		}

		void opGRP0(ushort addr, byte data)
		{
			RegW[GRP0] = data;
			OldGRP1 = RegW[GRP1];

			SetEffGRP0();
			SetEffGRP1();
		}

		void opGRP1(ushort addr, byte data)
		{
			RegW[GRP1] = data;
			OldGRP0 = RegW[GRP0];

			OldENABL = RegW[ENABL] != 0;

			SetEffGRP0();
			SetEffGRP1();
			Setblon();
		}

		void opENAM0(ushort addr, byte data)
		{
			RegW[ENAM0] = (byte)(data & 0x02);
			m0on = RegW[ENAM0] != 0 && RegW[RESMP0] == 0;
		}

		void opENAM1(ushort addr, byte data)
		{
			RegW[ENAM1] = (byte)(data & 0x02);
			m1on = RegW[ENAM1] != 0 && RegW[RESMP1] == 0;
		}

		void opENABL(ushort addr, byte data)
		{
			RegW[ENABL] = (byte)(data & 0x02);
			Setblon();
		}

		void opHM(ushort addr, byte data)
		{
			switch (addr)
			{
				case HMP0: HMRP0.Data = data; break;
				case HMP1: HMRP1.Data = data; break;
				case HMM0: HMRM0.Data = data; break;
				case HMM1: HMRM1.Data = data; break;
				case HMBL: HMRBL.Data = data; break;
				default: break;
			}
		}

		void opVDELP0(ushort addr, byte data)
		{
			RegW[VDELP0] = (byte)(data & 0x01);
			SetEffGRP0();
		}

		void opVDELP1(ushort addr, byte data)
		{
			RegW[VDELP1] = (byte)(data & 0x01);
			SetEffGRP1();
		}

		void opVDELBL(ushort addr, byte data)
		{
			RegW[VDELBL] = (byte)(data & 0x01);
			Setblon();
		}

		void opRESMP0(ushort addr, byte data)
		{
			if (RegW[RESMP0] != 0 && (data & 0x02) == 0)
			{
				int middle = 4;
				switch (RegW[NUSIZ0] & 0x07)
				{
					case 0x05: middle <<= 1; break;  // double size
					case 0x07: middle <<= 2; break;  // quad size
				}
				M0pos = (P0pos + middle) % 160;
			}
			RegW[RESMP0] = (byte)(data & 0x02);
			m0on = RegW[ENAM0] != 0 && RegW[RESMP0] == 0;
		}

		void opRESMP1(ushort addr, byte data)
		{
			if (RegW[RESMP1] != 0 && (data & 0x02) == 0)
			{
				int middle = 4;
				switch (RegW[NUSIZ1] & 0x07)
				{
					case 0x05: middle <<= 1; break;  // double size
					case 0x07: middle <<= 2; break;  // quad size
				}
				M1pos = (P1pos + middle) % 160;
			}
			RegW[RESMP1] = (byte)(data & 0x02);
			m1on = RegW[ENAM1] != 0 && RegW[RESMP1] == 0;
		}

		void HMOVEit(ref int pos, int hdelta) {
			pos += hdelta;
			if (pos > 159) {
				pos -= 160;
			}
			else if (pos < 0) {
				pos += 160;
			}
		}

		void opHMOVE(ushort addr, byte data) {
			P0suppress = 0;
			P1suppress = 0;

			LastHMOVEClock = Clock + 3;

			if ((LastHMOVEClock % 228) < 64) {
				LateHMOVEBlankOn = true;
			}
		}

		void opHMCLR(ushort addr, byte data) {
			RegW[HMP0] = 0;
			RegW[HMP1] = 0;
			RegW[HMM0] = 0;
			RegW[HMM1] = 0;
			RegW[HMBL] = 0;

			HMRP0.Data = 0;
			HMRP1.Data = 0;
			HMRM0.Data = 0;
			HMRM1.Data = 0;
			HMRBL.Data = 0;
		}

		void opCXCLR(ushort addr, byte data) {
			Collisions = 0;
		}

		void BuildPokeOpTable() {
			PokeOp = new PokeOpTyp[64];
			for (int i = 0; i < PokeOp.Length; i++) {
				PokeOp[i] = new PokeOpTyp(opNULL);
			}
			PokeOp[VSYNC]  = new PokeOpTyp(opVSYNC);
			PokeOp[VBLANK] = new PokeOpTyp(opVBLANK);
			PokeOp[WSYNC]  = new PokeOpTyp(opWSYNC);
			PokeOp[NUSIZ0] = new PokeOpTyp(opNUSIZ0);
			PokeOp[NUSIZ1] = new PokeOpTyp(opNUSIZ1);
			PokeOp[COLUP0] = new PokeOpTyp(opCOLUP0);
			PokeOp[COLUP1] = new PokeOpTyp(opCOLUP1);
			PokeOp[COLUPF] = new PokeOpTyp(opCOLUPF);
			PokeOp[COLUBK] = new PokeOpTyp(opCOLUBK);
			PokeOp[CTRLPF] = new PokeOpTyp(opCTRLPF);
			PokeOp[REFP0]  = new PokeOpTyp(opREFP0);
			PokeOp[REFP1]  = new PokeOpTyp(opREFP1);
			PokeOp[PF0]    = new PokeOpTyp(opPF);
			PokeOp[PF1]    = new PokeOpTyp(opPF);
			PokeOp[PF2]    = new PokeOpTyp(opPF);
			PokeOp[RESP0]  = new PokeOpTyp(opRESP0);
			PokeOp[RESP1]  = new PokeOpTyp(opRESP1);
			PokeOp[RESM0]  = new PokeOpTyp(opRESM0);
			PokeOp[RESM1]  = new PokeOpTyp(opRESM1);
			PokeOp[RESBL]  = new PokeOpTyp(opRESBL);
			PokeOp[AUDC0]  = new PokeOpTyp(opAUD);
			PokeOp[AUDC1]  = new PokeOpTyp(opAUD);
			PokeOp[AUDF0]  = new PokeOpTyp(opAUD);
			PokeOp[AUDF1]  = new PokeOpTyp(opAUD);
			PokeOp[AUDV0]  = new PokeOpTyp(opAUD);
			PokeOp[AUDV1]  = new PokeOpTyp(opAUD);
			PokeOp[GRP0]   = new PokeOpTyp(opGRP0);
			PokeOp[GRP1]   = new PokeOpTyp(opGRP1);
			PokeOp[ENAM0]  = new PokeOpTyp(opENAM0);
			PokeOp[ENAM1]  = new PokeOpTyp(opENAM1);
			PokeOp[ENABL]  = new PokeOpTyp(opENABL);
			PokeOp[HMP0]   = new PokeOpTyp(opHM);
			PokeOp[HMP1]   = new PokeOpTyp(opHM);
			PokeOp[HMM0]   = new PokeOpTyp(opHM);
			PokeOp[HMM1]   = new PokeOpTyp(opHM);
			PokeOp[HMBL]   = new PokeOpTyp(opHM);
			PokeOp[VDELP0] = new PokeOpTyp(opVDELP0);
			PokeOp[VDELP1] = new PokeOpTyp(opVDELP1);
			PokeOp[VDELBL] = new PokeOpTyp(opVDELBL);
			PokeOp[RESMP0] = new PokeOpTyp(opRESMP0);
			PokeOp[RESMP1] = new PokeOpTyp(opRESMP1);
			PokeOp[HMOVE]  = new PokeOpTyp(opHMOVE);
			PokeOp[HMCLR]  = new PokeOpTyp(opHMCLR);
			PokeOp[CXCLR]  = new PokeOpTyp(opCXCLR);
		}

		static int[] BuildPokeDelayTable() {
			int[] t = new int[0x40];

			for (int i = 0; i < t.Length; i++) {
				t[i] = 0;
			}

			// TODO: Unsure if any of these are really necessary
			//t[VBLANK] = 1;
			//t[ENABL] = 1;
			//t[ENAM0] = t[ENAM1] = 1;

			t[NUSIZ0] = t[NUSIZ1] = 12;
			t[REFP0] = t[REFP1] = 1;

			t[RESM0] = 8;
			t[RESM1] = 8;
			t[RESBL] = 8;

			t[GRP0] = t[GRP1] = 1;
			t[PF0] = t[PF1] = t[PF2] = -1;
			t[AUDC0] = t[AUDF0] = t[AUDV0] = t[AUDC1] = t[AUDF1] = t[AUDV1] = -2;

			return t;
		}
	}
}
