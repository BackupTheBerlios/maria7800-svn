/*
 * The Maria display device.
 * 
 * Derived from much of Dan Boris' work with 7800 emulation
 * within the MESS emulator.
 *
 * Thanks to Matthias Luedtke <matthias@atari8bit.de> for correcting
 * the BuildLineRAM320B() method to correspond to what the real hardware does.
 * (Matthias credited an insightful response by Eckhard Stolberg on a forum on
 * Atari Age circa June 2005.)
 * 
 * Copyright (C) 2004-2005 Mike Murphy
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
	public sealed class Maria : IDevice {
		private const int 
			INPTCTRL= 0x01,	// Write: input port control (VBLANK in TIA)
			INPT0	= 0x08,	// Read pot port: D7
			INPT1	= 0x09,	// Read pot port: D7
			INPT2	= 0x0a,	// Read pot port: D7
			INPT3	= 0x0b,	// Read pot port: D7
			INPT4	= 0x0c,	// Read P1 joystick trigger: D7
			INPT5	= 0x0d,	// Read P2 joystick trigger: D7
			AUDC0	= 0x15,	// Write: audio control 0 (D3-0)
			AUDC1	= 0x16,	// Write: audio control 1 (D4-0)
			AUDF0	= 0x17,	// Write: audio frequency 0 (D4-0)
			AUDF1	= 0x18,	// Write: audio frequency 1 (D3-0)
			AUDV0	= 0x19,	// Write: audio volume 0 (D3-0)
			AUDV1	= 0x1a,	// Write: audio volume 1 (D3-0)

			BACKGRND= 0x20,	// Background color
			P0C1	= 0x21, // Palette 0 - color 1
			P0C2	= 0x22, // Palette 0 - color 2
			P0C3	= 0x23, // Palette 0 - color 3
			WSYNC	= 0x24, // Wait for sync
			P1C1	= 0x25, // Palette 1 - color 1
			P1C2	= 0x26, // Palette 1 - color 2
			P1C3	= 0x27,	// Palette 1 - color 3
			MSTAT	= 0x28,	// Maria status
			P2C1	= 0x29,	// Palette 2 - color 1
			P2C2	= 0x2a,	// Palette 2 - color 2
			P2C3	= 0x2b,	// Palette 2 - color 3
			DPPH	= 0x2c,	// Display list list point high
			P3C1	= 0x2d,	// Palette 3 - color 1
			P3C2	= 0x2e,	// Palette 3 - color 2
			P3C3	= 0x2f,	// Palette 3 - color 3
			DPPL	= 0x30,	// Display list list point low
			P4C1	= 0x31, // Palette 4 - color 1
			P4C2	= 0x32,	// Palette 4 - color 2
			P4C3	= 0x33,	// Palette 4 - color 3
			CHARBASE= 0x34,	// Character base address
			P5C1	= 0x35,	// Palette 5 - color 1
			P5C2	= 0x36,	// Palette 5 - color 2
			P5C3	= 0x37,	// Palette 5 - color 3
			OFFSET	= 0x38,	// Future expansion (store zero here)
			P6C1	= 0x39,	// Palette 6 - color 1
			P6C2	= 0x3a,	// Palette 6 - color 2
			P6C3	= 0x3b,	// Palette 6 - color 3
			CTRL	= 0x3c,	// Maria control register
			P7C1	= 0x3d,	// Palette 7 - color 1
			P7C2	= 0x3e,	// Palette 7 - color 2
			P7C3	= 0x3f;	// Palette 7 - color 3

		private AddressSpace mem;
		private Machine7800 machine;
		private TIASound tiaSound;
		private int Scanline;
		private byte[] ScanlineBuffer = new byte[320];
		private byte[] LineRAM = new byte[512];
		private byte[,] MariaPalette = new byte[8,4];
		private byte[] Registers = new byte[0x40];
		private byte WM;
		private ushort DLL;
		private ushort DL;
		private int Offset;
		private int Holey;
		private int Width;
		private byte HPOS;
		private byte PaletteNo;
		private bool INDMode;
		private bool DLI;
		private bool CtrlLock;
		private byte VBlank;
		// MARIA CNTL
		private bool DMAEnabled, DMAOn;
		private bool ColorKill;
		private bool CWidth;
		private bool BCntl;
		private bool Kangaroo;
		private byte RM;

		public Maria(Machine7800 machine) {
			this.machine = machine;
			tiaSound = new TIASound(machine);
		}	
		
		public void Reset()  {
			CtrlLock = false;
			for (int i=0; i < 8; i++)  {
				for (int j=0; j < 4; j++)  {
					MariaPalette[i,j] = 0;
				}
			}
			VBlank = 0x80;
			DMAEnabled = false;
			DMAOn = false;
			ColorKill = false;
			CWidth = false;
			BCntl = false;
			Kangaroo = false;
			RM = 0;
			tiaSound.Reset();
            Trace.Write(this);
            Trace.WriteLine(" reset");
		}		

		public byte this[ushort addr]  {
			get { return peek(addr); }
			set { poke(addr, value); }
		}	

		public bool RequestSnooping {
			get { return false; }
		}

		public void Map(AddressSpace mem) {
			this.mem = mem;
		}

		public override string ToString() {
			return "Maria";
		}

		public void StartFrame() {
			tiaSound.StartFrame();
		}

		public void DoScanline(int scanline) {
			Scanline = scanline;
			for (int i=0; i < LineRAM.Length; i++) {
				LineRAM[i] = Registers[BACKGRND];
			}
			if (Scanline == 15) {
				VBlank = 0x00;  // End of VBLANK
				machine.CPU.RunClocks -= 15;// 10-13 + 5-9
				if (DMAEnabled || DMAOn) {
					DMAOn = true;
					DLL = WORD(Registers[DPPL], Registers[DPPH]);

					DL = WORD(mem[(ushort)(DLL + 2)], mem[(ushort)(DLL + 1)]);
					byte dll0 = mem[DLL];				
					DLI = (dll0 & 0x80) != 0;
					Holey = (dll0 & 0x60) >> 5;
					Offset = dll0 & 0x0f;
				}
			} 
			else if (Scanline == (machine.Scanlines - 5))  {
				VBlank = 0x80;  // Start of VBLANK
				DMAOn = false;
			}
			if (DMAOn) {
				// DMA Startup
				machine.CPU.RunClocks -= 5;// 5-9
				BuildLineRAM();
				if (--Offset < 0) {
					DLL += 3;
					DL = WORD(mem[(ushort)(DLL + 2)], mem[(ushort)(DLL + 1)]);
					byte dll0 = mem[DLL];				
					DLI = (dll0 & 0x80) != 0;
					Holey = (dll0 & 0x60) >> 5;
					Offset = dll0 & 0x0f;
					// DMA Shutdown: Last line of zone
					machine.CPU.RunClocks -= 10;// 10-13
				} //else {
				// DMA Shutdown: Other line of zone
				machine.CPU.RunClocks -= 4;// 4-7
			}
			if (DLI) {
				machine.CPU.NMIInterruptRequest = true;
				DLI = false;
				machine.CPU.RunClocks -= 1;
			}
			if (machine.Host != null) {
				for (int i=0; i < 320; i++) {
					ScanlineBuffer[i] = LineRAM[i];
				}
				machine.Host.UpdateDisplay(ScanlineBuffer, Scanline + 1, 0, 320);
			}
		}

		public void EndFrame() {
			tiaSound.EndFrame();
		}

		void BuildLineRAM() {
			ushort dl = DL;
			ushort graphaddr;
			// Iterate until end of display list (DL)
			while (mem[(ushort)(dl + 1)] != 0) {
				if ((mem[(ushort)(dl + 1)] & 0x5f) == 0x40) {
					// Extended DL header
					graphaddr = WORD(mem[dl], mem[(ushort)(dl + 2)]);
					Width = ((mem[(ushort)(dl + 3)] ^ 0xff) & 0x1f) + 1;
					HPOS = mem[(ushort)(dl + 4)];
					PaletteNo = (byte)(mem[(ushort)(dl + 3)] >> 5);
					WM = (byte)((mem[(ushort)(dl + 1)] & 0x80) >> 5);
					INDMode = (mem[(ushort)(dl + 1)] & 0x20) != 0;
					dl += 5;
					machine.CPU.RunClocks -= 12;
				} 
				else {
					// Normal DL header
					graphaddr = WORD(mem[dl], mem[(ushort)(dl + 2)]);
					Width = ((mem[(ushort)(dl + 1)] ^ 0xff) & 0x1f) + 1;
					HPOS = mem[(ushort)(dl + 3)];
					PaletteNo = (byte)(mem[(ushort)(dl + 1)] >> 5);
					INDMode = false;
					dl += 4;
					machine.CPU.RunClocks -= 8;
				}

				switch (RM | WM) {
					case 0x00:
						//case 0x01:
						BuildLineRAM160A(graphaddr);
						break;
					case 0x02:
						BuildLineRAM320D(graphaddr);
						break;
					case 0x03:
						BuildLineRAM320A(graphaddr);
						break;
					case 0x04:
						//case 0x05:
						BuildLineRAM160B(graphaddr);
						break;
					case 0x06:
						BuildLineRAM320B(graphaddr);
						break;
					case 0x07:
						BuildLineRAM320C(graphaddr);
						break;
				}
			}
		}

		void BuildLineRAM160A(ushort graphaddr) {
			ushort dataaddr;
			int c, d, indbytes;
			byte hpos = HPOS;
			for (int x=0; x < Width; x++) {
				dataaddr = (ushort)(graphaddr + x);
				if (INDMode) {
					dataaddr = WORD(mem[dataaddr], Registers[CHARBASE]);
					machine.CPU.RunClocks -= 3;
				}
				dataaddr += (ushort)(Offset << 8);
				if (Holey == 0x02 && ((dataaddr & 0x9000) == 0x9000))
					continue;
				if (Holey == 0x01 && ((dataaddr & 0x8800) == 0x8800))
					continue;
				indbytes = (INDMode && CWidth) ? 2 : 1;
				while (indbytes-- > 0) {
					d = mem[dataaddr++];
					machine.CPU.RunClocks -= 3;
					c = (d & 0xc0) >> 6;
					if (c != 0) {
						LineRAM[hpos << 1]
							= LineRAM[(hpos << 1) + 1]
							= MariaPalette[PaletteNo,c];
					}
					hpos++;
					c = (d & 0x30) >> 4;
					if (c != 0) {
						LineRAM[hpos << 1]
							= LineRAM[(hpos << 1) + 1]
							= MariaPalette[PaletteNo,c];
					}
					hpos++;
					c = (d & 0x0c) >> 2;
					if (c != 0) {
						LineRAM[hpos << 1]
							= LineRAM[(hpos << 1) + 1]
							= MariaPalette[PaletteNo,c];
					}
					hpos++;
					c = d & 0x03;
					if (c != 0) {
						LineRAM[hpos << 1]
							= LineRAM[(hpos << 1) + 1]
							= MariaPalette[PaletteNo,c];
					}
					hpos++;
				}
			}
		}

		void BuildLineRAM160B(ushort graphaddr) {
			ushort dataaddr;
			int c, d, indbytes, p;
			byte hpos = HPOS;
			for (int x=0; x < Width; x++) {
				dataaddr = (ushort)(graphaddr + x);
				if (INDMode) {
					dataaddr = WORD(mem[dataaddr], Registers[CHARBASE]);
					machine.CPU.RunClocks -= 3;
				}
				dataaddr += (ushort)(Offset << 8);
				if (Holey == 0x02 && ((dataaddr & 0x9000) == 0x9000))
					continue;
				if (Holey == 0x01 && ((dataaddr & 0x8800) == 0x8800))
					continue;
				indbytes = (INDMode && CWidth) ? 2 : 1;
				while (indbytes-- > 0) {
					d = mem[dataaddr++];
					machine.CPU.RunClocks -= 3;
					c = (d & 0xc0) >> 6;
					if (c != 0) {
						p = (PaletteNo & 0x04) | ((d & 0x0c) >> 2);
						LineRAM[hpos << 1]
							= LineRAM[(hpos << 1) + 1]
							= MariaPalette[p,c];
					}
					hpos++;
					c = (d & 0x30) >> 4;
					if (c != 0) {
						p = (PaletteNo & 0x04) | (d & 0x03);
						LineRAM[hpos << 1]
							= LineRAM[(hpos << 1) + 1]
							= MariaPalette[p,c];
					}
					hpos++;
				}
			}
		}

		void BuildLineRAM320A(ushort graphaddr) {
			ushort dataaddr;
			int d;
			int hpos = HPOS << 1;
			for (int x=0; x < Width; x++) {
				dataaddr = (ushort)(graphaddr + x);
				if (INDMode) {
					dataaddr = WORD(mem[dataaddr], Registers[CHARBASE]);
					machine.CPU.RunClocks -= 3;
				}
				dataaddr += (ushort)(Offset << 8);
				if (Holey == 0x02 && ((dataaddr & 0x9000) == 0x9000))
					continue;
				if (Holey == 0x01 && ((dataaddr & 0x8800) == 0x8800))
					continue;
				d = mem[dataaddr];
				machine.CPU.RunClocks -= 3;
				if ((d & 0x80) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				if ((d & 0x40) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				if ((d & 0x20) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				if ((d & 0x10) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				if ((d & 0x08) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				if ((d & 0x04) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				if ((d & 0x02) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				if ((d & 0x01) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
			}
		}

		void BuildLineRAM320B(ushort graphaddr) {
			ushort dataaddr;
			int c, d, indbytes;
			int hpos = HPOS << 1;
			for (int x = 0; x < Width; x++) {
				dataaddr = (ushort)(graphaddr + x);
				if (INDMode) {
					dataaddr = WORD(mem[dataaddr], Registers[CHARBASE]);
					machine.CPU.RunClocks -= 3;
				}
				dataaddr += (ushort)(Offset << 8);
				if (Holey == 0x02 && ((dataaddr & 0x9000) == 0x9000))
					continue;
				if (Holey == 0x01 && ((dataaddr & 0x8800) == 0x8800))
					continue;
				indbytes = (INDMode && CWidth) ? 2 : 1;
				while (indbytes-- > 0) {
					d = mem[dataaddr++];
					machine.CPU.RunClocks -= 3;
					c = ((d & 0x80) >> 6) | ((d & 0x08) >> 3);
					if (c != 0) {
						if ((d & 0xc0) != 0 || Kangaroo)
							LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo, c];
					}
					else if (Kangaroo)
						LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
					else if ((d & 0xcc) != 0)
						LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
					hpos++;
					c = ((d & 0x40) >> 5) | ((d & 0x04) >> 2);
					if (c != 0) {
						if ((d & 0xc0) != 0 || Kangaroo)
							LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo, c];
					}
					else if (Kangaroo)
						LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
					else if ((d & 0xcc) != 0)
						LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
					hpos++;

					c = ((d & 0x20) >> 4) | ((d & 0x02) >> 1);
					if (c != 0) {
						if ((d & 0x30) != 0 || Kangaroo)
							LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo, c];
					}
					else if (Kangaroo)
						LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
					else if ((d & 0x33) != 0)
						LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
					hpos++;

					c = ((d & 0x10) >> 3) | (d & 0x01);
					if (c != 0)
					{
						if ((d & 0x30) != 0 || Kangaroo)
							LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo, c];
					}
					else if (Kangaroo)
						LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
					else if ((d & 0x33) != 0)
						LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
					hpos++;
				}
			}
		}
		
		void BuildLineRAM320C(ushort graphaddr) {
			ushort dataaddr;
			int d, p;
			int hpos = HPOS << 1;

			for (int x=0; x < Width; x++) {
				dataaddr = (ushort)(graphaddr + x);
				if (INDMode) {
					dataaddr = WORD(mem[dataaddr], Registers[CHARBASE]);
					machine.CPU.RunClocks -= 3;
				}
				dataaddr += (ushort)(Offset << 8);
				if (Holey == 0x02 && ((dataaddr & 0x9000) == 0x9000))
					continue;
				if (Holey == 0x01 && ((dataaddr & 0x8800) == 0x8800))
					continue;
				d = mem[dataaddr];
				machine.CPU.RunClocks -= 3;
				p = ((d & 0x0c) >> 2) | (PaletteNo & 0x04);
				if ((d & 0x80) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[p,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				if ((d & 0x40) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[p,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				p = (d & 0x03) | (PaletteNo & 0x04);
				if ((d & 0x20) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[p,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				if ((d & 0x10) != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[p,2];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
			}
		}

		void BuildLineRAM320D(ushort graphaddr) {
			ushort dataaddr;
			int c, d;
			int hpos = HPOS << 1;
			for (int x=0; x < Width; x++) {
				dataaddr = (ushort)(graphaddr + x);
				if (INDMode) {
					dataaddr = WORD(mem[dataaddr], Registers[CHARBASE]);
					machine.CPU.RunClocks -= 3;
				}
				dataaddr += (ushort)(Offset << 8);
				if (Holey == 0x02 && ((dataaddr & 0x9000) == 0x9000))
					continue;
				if (Holey == 0x01 && ((dataaddr & 0x8800) == 0x8800))
					continue;
				d = mem[dataaddr];
				machine.CPU.RunClocks -= 3;
				c = ((d & 0x80) >> 6) | ((PaletteNo & 2) >> 1);
				if (c != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo & 0x04,c];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				c = ((d & 0x40) >> 5) | (PaletteNo & 1);
				if (c != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo & 0x04,c];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				c = ((d & 0x20) >> 4) | ((PaletteNo & 2) >> 1);
				if (c != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo & 0x04,c];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				c = ((d & 0x10) >> 3) | (PaletteNo & 1);
				if (c != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo & 0x04,c];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				c = ((d & 0x08) >> 2) | ((PaletteNo & 2) >> 1);
				if (c != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo & 0x04,c];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				c = ((d & 0x04) >> 1 ) | (PaletteNo & 1);
				if (c != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo & 0x04,c];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				c = (d & 0x02) | ((PaletteNo & 2) >> 1);
				if (c != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo & 0x04,c];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
				c = ((d & 0x01) << 1) | (PaletteNo & 1);
				if (c != 0)
					LineRAM[hpos & 0x1ff] = MariaPalette[PaletteNo & 0x04,c];
				else if (Kangaroo)
					LineRAM[hpos & 0x1ff] = Registers[BACKGRND];
				hpos++;
			}
		}

		byte peek(ushort addr) {
			byte retval = 0;
			int hpos, scanline = Scanline;
			addr &= 0x3f;
			if (addr < 0x20) {
				machine.CPU.RunClocks -= 2;
			}
			switch(addr) {
				case MSTAT:
					retval = VBlank;
					break;
				case INPT0:
					retval = DumpedInputPort(machine.InputAdapter.ReadINPT0());
					break;
				case INPT1:
					retval = DumpedInputPort(machine.InputAdapter.ReadINPT1());
					break;
				case INPT2:
					retval = DumpedInputPort(machine.InputAdapter.ReadINPT2());
					break;
				case INPT3:
					retval = DumpedInputPort(machine.InputAdapter.ReadINPT3());
					break;
				case INPT4:
					hpos = 114 * machine.CPU.RunClocksMultiple - machine.CPU.RunClocks;
					retval = machine.InputAdapter.ReadINPT4(scanline, hpos) ? (byte)0x00 : (byte)0x80;
					break;
				case INPT5:
					hpos = 114 * machine.CPU.RunClocksMultiple - machine.CPU.RunClocks;
					retval = machine.InputAdapter.ReadINPT5(Scanline, hpos) ? (byte)0x00 : (byte)0x80;
					break;
				default:
#if DEBUG
					Trace.WriteLine(String.Format("Unhandled Maria/TIA peek:{0:x4}", addr));
#endif
					retval = Registers[addr];
					break;
			}

			return retval;
		}

		void poke(ushort addr, byte data) {
			addr &= 0x3f;
			if (addr < 0x20) {
				machine.CPU.RunClocks -= 2;
			}
			switch (addr)  {
					//		Not sure about these:
					//		0000 0000 = ($00) disable 7800 RAM (2600 mode?)
					//		0000 0010 = ($02) enable 7800 RAM/switch in bios
					//		0001 0110 = ($16) switch in cart
					//		0001 1101 - ($1d) self-test failure, enable tia/cart lockout
				case INPTCTRL:
					if (data == 0x1d) {
						Trace.WriteLine(String.Format("7800 BIOS Error (#{0}):\n", machine.CPU.Y));
						switch (machine.CPU.Y) {
                            case 00: Trace.WriteLine("BADCPU: CPU ERROR"); break;
                            case 01: Trace.WriteLine("BAD6116A: ERROR IN RAM $2000-$27FF"); break;
                            case 02: Trace.WriteLine("BAD6116B: ERROR IN RAM $1800-$1FFF"); break;
                            case 03: Trace.WriteLine("BADRAM: CAN'T GET TO ANY OF THE RAM"); break;
                            case 04: Trace.WriteLine("BADMARIA: MARIA SHADOWING NOT WORING"); break;
                            case 05: Trace.WriteLine("BADVALID: BAD VALIDATION OR DECRYPTION"); break;
                            default: Trace.WriteLine("Unknown BIOS error code"); break;
						}
						Trace.WriteLine("Machine Halted");
						machine.MachineHalt = true;
					}

					if (!CtrlLock) {
						CtrlLock = (data & 0x01) != 0;
						if ((data & 0x04) != 0) {
							machine.SwapOutBIOS();
						} 
						else {
							machine.SwapInBIOS();
						}
					}
					break;
				case WSYNC:
					// Request a CPU preemption to service the delay request
					machine.CPU.EmulatorPreemptRequest = true;
					break;
				case CTRL:
					ColorKill = (data & 0x80) != 0;
					DMAEnabled = (data & 0x60) == 0x40;
					if (!DMAEnabled) 
					{
						DMAOn = false;
					}
					CWidth = (data & 0x10) != 0;
					BCntl = (data & 0x08) != 0;
					Kangaroo = (data & 0x04) != 0;
					RM = (byte)(data & 0x03);
					break;
				case CHARBASE:
					Registers[CHARBASE] = data;
					break;
				case BACKGRND:
					Registers[BACKGRND] = data;
					break;
				case DPPH:
					Registers[DPPH] = data;
					break;
				case DPPL:
					Registers[DPPL] = data;
					break;
				case P0C1:
					MariaPalette[0,1] = data;
					break;
				case P0C2:
					MariaPalette[0,2] = data;
					break;
				case P0C3:
					MariaPalette[0,3] = data;
					break;
				case P1C1:
					MariaPalette[1,1] = data;
					break;
				case P1C2:
					MariaPalette[1,2] = data;
					break;
				case P1C3:
					MariaPalette[1,3] = data;
					break;
				case P2C1:
					MariaPalette[2,1] = data;
					break;
				case P2C2:
					MariaPalette[2,2] = data;
					break;
				case P2C3:
					MariaPalette[2,3] = data;
					break;
				case P3C1:
					MariaPalette[3,1] = data;
					break;
				case P3C2:
					MariaPalette[3,2] = data;
					break;
				case P3C3:
					MariaPalette[3,3] = data;
					break;
				case P4C1:
					MariaPalette[4,1] = data;
					break;
				case P4C2:
					MariaPalette[4,2] = data;
					break;
				case P4C3:
					MariaPalette[4,3] = data;
					break;
				case P5C1:
					MariaPalette[5,1] = data;
					break;
				case P5C2:
					MariaPalette[5,2] = data;
					break;
				case P5C3:
					MariaPalette[5,3] = data;
					break;
				case P6C1:
					MariaPalette[6,1] = data;
					break;
				case P6C2:
					MariaPalette[6,2] = data;
					break;
				case P6C3:
					MariaPalette[6,3] = data;
					break;
				case P7C1:
					MariaPalette[7,1] = data;
					break;
				case P7C2:
					MariaPalette[7,2] = data;
					break;
				case P7C3:
					MariaPalette[7,3] = data;
					break;
				case AUDC0:
				case AUDC1:
				case AUDF0:
				case AUDF1:
				case AUDV0:
				case AUDV1:
					tiaSound.Update(addr, data);
					break;
				case OFFSET:
					Trace.WriteLine(String.Format("ROM wrote 0x{0:x2} to 0x{1:x4} (reserved for future expansion)", data, addr));
					break;
#if DEBUG
				default:

					Trace.WriteLine(String.Format("Unhandled Maria/TIA poke:{0:x4} w/{1:x2}", addr, data));
					break;
#endif
			}
			Registers[addr] = data;
		}

		byte DumpedInputPort(int resistance) {
			byte retval = 0;
			if (resistance == 0) {
				retval = 0x80;
			}
			return retval;
		}

		ushort WORD(byte lsb, byte msb) {
			return (ushort)(lsb | msb << 8);
		}
	}
}
