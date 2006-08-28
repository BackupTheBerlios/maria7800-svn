/*
 * CPU emulator for the MOS Technology 6502 microprocessor.
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
using System.Globalization;
using System.Text;
using System.Runtime.Serialization;

namespace Maria.Core {
	[Serializable]
	public class M6502 : IDeserializationCallback {
		public const ushort NMI_VEC = 0xfffa;
		public const ushort RST_VEC = 0xfffc;
		public const ushort IRQ_VEC = 0xfffe;
		private delegate void OpcodeHandler();

		public ulong Clock;
		public int RunClocks;
		public int RunClocksMultiple;
		public bool EmulatorPreemptRequest;
		public bool Jammed;
		public bool IRQInterruptRequest;
		public bool NMIInterruptRequest;
		public ushort PC;
		public byte A;
		public byte X;
		public byte Y;
		public byte S;
		public byte P;
		private readonly IAddressable mem;
		[NonSerialized]
		private OpcodeHandler[] Opcodes;

		public M6502(IAddressable mem) {
			this.mem = mem;
			InstallOpcodes();
			Clock = 0;
			RunClocks = 0;
			RunClocksMultiple = 1;
			P = 1 << 5;
		}

		public void Reset() {
			Jammed = false;
			S = 0xff;
			PC = WORD(mem[RST_VEC], mem[RST_VEC + 1]);
			Trace.Write(this);
			Trace.WriteLine(
				String.Format(
					CultureInfo.InvariantCulture,
					"(PC:${0:x4}) reset",
					PC
				)
			);
		}

		public void Execute() {
			EmulatorPreemptRequest = false;
			while (RunClocks > 0 && !Jammed) {
				if (EmulatorPreemptRequest) {
					break;
				}
				else if (NMIInterruptRequest) {
					NMIInterruptRequest = false;
					interrupt(NMI_VEC, true);
				}
				else if (!fI && IRQInterruptRequest) {
					IRQInterruptRequest = false;
					interrupt(IRQ_VEC, true);
				}
				else {
					Opcodes[mem[PC++]]();
				}
			}
		}

		public virtual void OnDeserialization(object sender) {
			InstallOpcodes();
		}

		public override String ToString() {
			return "M6502 CPU";
		}

		private static byte MSB(ushort u16) {
			return (byte)(u16 >> 8);
		}

		private static byte LSB(ushort u16) {
			return (byte)u16;
		}

		private static ushort WORD(byte lsb, byte msb) {
			return (ushort)(lsb | msb << 8);
		}

		private void fset(byte flag, bool value) {
			P = (byte)(value ? P | flag : P & ~flag);
		}

		private bool fget(byte flag) {
			return (P & flag) != 0;
		}

		private bool fC {
			get { return fget(1 << 0); }
			set { fset(1 << 0, value); }
		}

		private bool fZ {
			get { return fget(1 << 1); }
			set { fset(1 << 1, value); }
		}

		private bool fI {
			get { return fget(1 << 2); }
			set { fset(1 << 2, value); }
		}

		private bool fD {
			get { return fget(1 << 3); }
			set { fset(1 << 3, value); }
		}

		private bool fB {
			get { return fget(1 << 4); }
			set { fset(1 << 4, value); }
		}

		private bool fV {
			get { return fget(1 << 6); }
			set { fset(1 << 6, value); }
		}

		private bool fN {
			get { return fget(1 << 7); }
			set { fset(1 << 7, value); }
		}

		private void set_fNZ(byte u8) {
			fN = (u8 & 0x80) != 0;
			fZ = (u8 & 0xff) == 0;
		}

		private byte pull() {
			S++;
			return mem[(ushort)(0x0100 + S)];
		}

		private void push(byte data) {
			mem[(ushort)(0x0100 + S)] = data;
			S--;
		}

		private void clk(int ticks) {
			Clock += (ulong)ticks;
			RunClocks -= (ticks*RunClocksMultiple);
		}

		private void interrupt(ushort intr_vector, bool isExternal) {
			if (isExternal) {
				fB = false;
				clk(7); // Charge clks for external interrupts
			}
			else {
				fB = true;
				PC++;
			}
			push(MSB(PC));
			push(LSB(PC));
			push(P);
			fI = true;
			byte lsb = mem[intr_vector];
			intr_vector++;
			byte msb = mem[intr_vector];
			PC = WORD(lsb, msb);
		}

		private void br(bool cond, ushort ea) {
			if (cond) {
				clk( (MSB(PC) == MSB(ea)) ? 1 : 2 );
				PC = ea;
			}
		}

		// Relative: Bxx $aa  (branch instructions only)
		private ushort aREL() {
			sbyte bo = (sbyte) mem[PC];
			PC++;
			return (ushort)(PC + bo);
		}

		// Zero Page: $aa
		private ushort aZPG() {
			return WORD(mem[PC++], 0x00);
		}

		// Zero Page Indexed,X: $aa,X
		private ushort aZPX() {
			return WORD((byte)(mem[PC++] + X), 0x00);
		}

		// Zero Page Indexed,Y: $aa,Y
		private ushort aZPY() {
			return WORD((byte)(mem[PC++] + Y), 0x00);
		}

		// Absolute: $aaaa
		private ushort aABS() {
			byte lsb = mem[PC++];
			byte msb = mem[PC++];
			return WORD(lsb, msb);
		}

		// Absolute Indexed,X: $aaaa,X
		private ushort aABX(int eclk) {
			ushort ea = aABS();
			if (LSB(ea) + X > 0xff) {
				clk(eclk);
			}
			return (ushort)(ea + X);
		}

		// Absolute Indexed,Y: $aaaa,Y
		private ushort aABY(int eclk) {
			ushort ea = aABS();
			if (LSB(ea) + Y > 0xff) {
				clk(eclk);
			}
			return (ushort)(ea + Y);
		}

		// Indexed Indirect: ($aa,X)
		private ushort aIDX() {
			byte zpa = (byte) (mem[PC++] + X);
			byte lsb = mem[zpa++];
			byte msb = mem[zpa];
			return WORD(lsb, msb);
		}

		// Indirect Indexed: ($aa),Y
		private ushort aIDY(int eclk) {
			byte zpa = mem[PC++];
			byte lsb = mem[zpa++];
			byte msb = mem[zpa];
			if (lsb + Y > 0xff) {
				clk(eclk);
			}
			return (ushort)(WORD(lsb, msb) + Y);
		}

		// Indirect Absolute: ($aaaa) (only used by JMP)
		private ushort aIND() {
			ushort ea = aABS();
			byte lsb = mem[ea];
			ea = WORD((byte)(LSB(ea) + 1), MSB(ea)); // emulate bug
			byte msb = mem[ea];
			return WORD(lsb, msb);
		}

		// ADC: Add with carry
		private void iADC(byte mem) {
			int c = fC ? 1 : 0;
			if (fD) {
				int lo = (A & 0x0f) + (mem & 0x0f) + c;
				int hi = (A & 0xf0) + (mem & 0xf0);
				fZ = ((lo + hi) & 0xff) == 0;
				if (lo > 0x09) {
					lo += 0x06;
					hi += 0x10;
				}
				fN = (hi & 0x80) != 0;
				fV = (~(A^mem) & (A^hi) & 0x80) != 0;
				if (hi > 0x90) {
					hi += 0x60;
				}
				fC = (hi & 0xff00) != 0;
				A = (byte)((lo & 0x0f) | (hi & 0xf0));
			}
			else {
				int sum = A + mem + c;
				fV = (~(A^mem) & (A^sum) & 0x80) != 0;
				fC = (sum & 0x100) != 0;
				A = (byte)sum;
				set_fNZ(A);
			}
		}

		// AND: Logical and
		private void iAND(byte mem) {
			A &= mem;
			set_fNZ(A);
		}

		// ASL: Arithmetic shift left: C <- [7][6][5][4][3][2][1][0] <- 0
		private byte iASL(byte mem) {
			fC = (mem & 0x80) != 0;
			mem <<= 1;
			set_fNZ(mem);
			return mem;
		}

		// BIT: Bit test
		private void iBIT(byte mem) {
			fN = (mem & 0x80) != 0;
			fV = (mem & 0x40) != 0;
			fZ = (mem & A) == 0;
		}

		// BRK Force Break  (cause software interrupt)
		private void iBRK() {
			interrupt(IRQ_VEC, false);
		}

		// CLC: Clear carry flag
		private void iCLC() {
			fC = false;
		}

		// CLD: Clear decimal mode
		private void iCLD() {
			fD = false;
		}

		// CLI: Clear interrupt disable
		private void iCLI() {
			fI = false;
		}

		// CLV: Clear overflow flag
		private void iCLV() {
			fV = false;
		}

		// CMP: Compare accumulator
		private void iCMP(byte mem) {
			fC = A >= mem;
			set_fNZ((byte)(A - mem));
		}

		// CPX: Compare index X
		private void iCPX(byte mem) {
			fC = X >= mem;
			set_fNZ((byte)(X - mem));
		}

		// CPY: Compare index Y
		private void iCPY(byte mem) {
			fC = Y >= mem;
			set_fNZ((byte)(Y - mem));
		}

		// DEC: Decrement memory
		private byte iDEC(byte mem) {
			mem--;
			set_fNZ(mem);
			return mem;
		}

		// DEX: Decrement index x
		private void iDEX() {
			X--;
			set_fNZ(X);
		}

		// DEY: Decrement index y
		private void iDEY() {
			Y--;
			set_fNZ(Y);
		}

		// EOR: Logical exclusive or
		private void iEOR(byte mem) {
			A ^= mem;
			set_fNZ(A);
		}

		// INC: Increment memory
		private byte iINC(byte mem) {
			mem++;
			set_fNZ(mem);
			return mem;
		}

		// INX: Increment index x
		private void iINX() {
			X++;
			set_fNZ(X);
		}

		// INY: Increment index y
		private void iINY() {
			Y++;
			set_fNZ(Y);
		}

		// JMP Jump to address
		private void iJMP(ushort ea) {
			PC = ea;
		}

		// JSR Jump to subroutine
		private void iJSR(ushort ea) {
			PC--;           // Yes, the 6502/7 really does this
			push(MSB(PC));
			push(LSB(PC));
			PC = ea;
		}

		// LDA: Load accumulator
		private void iLDA(byte mem) {
			A = mem;
			set_fNZ(A);
		}

		// LDX: Load index X
		private void iLDX(byte mem) {
			X = mem;
			set_fNZ(X);
		}

		// LDY: Load index Y
		private void iLDY(byte mem) {
			Y = mem;
			set_fNZ(Y);
		}

		// LSR: Logic shift right: 0 -> [7][6][5][4][3][2][1][0] -> C
		private byte iLSR(byte mem) {
			fC = (mem & 0x01) != 0;
			mem >>= 1;
			set_fNZ(mem);
			return mem;
		}

		// NOP: No operation
		private void iNOP() {
			// TODO : what do we do about this (I'd say we delete it...)
			/*if (EMU7800App.Instance.Settings.NOPRegisterDumping) {
				Trace.Write("NOP: ");
				Trace.WriteLine(M6502DASM.GetRegisters(this));
			}*/
		}

		// ORA: Logical inclusive or
		private void iORA(byte mem) {
			A |= mem;
			set_fNZ(A);
		}

		// PHA: Push accumulator
		private void iPHA() {
			push(A);
		}

		// PHP: Push processor status (flags)
		private void iPHP() {
			push(P);
		}

		// PLA: Pull accumuator
		private void iPLA() {
			A = pull();
			set_fNZ(A);
		}

		// PLP: Pull processor status (flags)
		private void iPLP() {
			P = pull();
			fB = true;
		}

		// ROL: Rotate left: new C <- [7][6][5][4][3][2][1][0] <- C
		private byte iROL(byte mem) {
			byte d0 = (byte)(fC ? 0x01 : 0x00);
			fC = (mem & 0x80) != 0;
			mem <<= 1;
			mem |= d0;
			set_fNZ(mem);
			return mem;
		}

		// ROR: Rotate right: C -> [7][6][5][4][3][2][1][0] -> new C
		private byte iROR(byte mem) {
			byte d7 = (byte)(fC ? 0x80 : 0x00);
			fC = (mem & 0x01) != 0;
			mem >>= 1;
			mem |= d7;
			set_fNZ(mem);
			return mem;
		}

		// RTI: Return from interrupt
		private void iRTI() {
			P = pull();
			byte lsb = pull();
			byte msb = pull();
			PC = WORD(lsb, msb);
			fB = true;
		}

		// RTS: Return from subroutine
		private void iRTS() {
			byte lsb = pull();
			byte msb = pull();
			PC = WORD(lsb, msb);
			PC++; // Yes, the 6502/7 really does this
		}

		// SBC: Subtract with carry (borrow)
		private void iSBC(byte mem) {
			int c   = fC ? 0 : 1;
			int sum = A - mem - c;
			if (fD) {
				int lo  = (A & 0x0f) - (mem & 0x0f) - c;
				int hi  = (A & 0xf0) - (mem & 0xf0);
				if ((lo & 0x10) != 0) {
					lo -= 0x06;
					hi -= 0x01;
				}
				fV = ((A^mem) & 0x80) != 0 && ((sum^mem) & 0x80) == 0;
				if ((hi & 0x0100) != 0) {
					hi -= 0x60;
				}
				A = (byte)((lo & 0x0f) | (hi & 0xf0));
			}
			else {
				fV = ((A^mem) & 0x80) != 0 && ((sum^mem) & 0x80) == 0;
				A = (byte)sum;
			}
			fC = (sum & 0x100) == 0;
			set_fNZ(A);
		}

		// SEC: Set carry flag
		private void iSEC() {
			fC = true;
		}

		// SED: Set decimal mode
		private void iSED() {
			fD = true;
		}

		// SEI: Set interrupt disable
		private void iSEI() {
			fI = true;
		}

		// STA: Store accumulator
		private byte iSTA() {
			return A;
		}

		// STX: Store index X
		private byte iSTX() {
			return X;
		}

		// STY: Store index Y
		private byte iSTY() {
			return Y;
		}

		// TAX: Transfer accumlator to index X
		private void iTAX() {
			X = A;
			set_fNZ(X);
		}

		// TAY: Transfer accumlator to index Y
		private void iTAY() {
			Y = A;
			set_fNZ(Y);
		}

		// TSX: Transfer stack to index X
		private void iTSX() {
			X = S;
			set_fNZ(X);
		}

		// TXA: Transfer index X to accumlator
		private void iTXA() {
			A = X;
			set_fNZ(A);
		}

		// TXS: Transfer index X to stack
		private void iTXS() {
			S = X;
			// No flags set.
		}

		// TYA: Transfer index Y to accumulator
		private void iTYA() {
			A = Y;
			set_fNZ(A);
		}

		// KIL: Jam the processor
		private void iKIL() {
			Jammed = true;
			Trace.Write(this);
			Trace.WriteLine(": Processor Jammed");
		}

		// LAX: Load accumulator and index x
		private void iLAX(byte mem) {
			A = X = mem;
			set_fNZ(A);
		}

		// ISB: Increment and subtract with carry
		private void iISB(byte mem) {
			mem++;
			iSBC(mem);
		}

		// RLA: Rotate left and logical and accumulator
		// new C <- [7][6][5][4][3][2][1][0] <- C
		private void iRLA(byte mem) {
			byte d0 = (byte)(fC ? 0x01 : 0x00);
			fC = (mem & 0x80) != 0;
			mem <<= 1;
			mem |= d0;
			A &= mem;
			set_fNZ(A);
		}

		// SAX: logical and accumulator with index X and store
		private byte iSAX() {
			return (byte)(A & X);
		}

		private void InstallOpcodes() {
			// TODO : actually do something here...
		}
	}
}

// TODO : port the shit below. Take care, I'm pretty sure it's butt ugly
		/*
		void InstallOpcodes()
		{
			Opcodes = new OpcodeHandler[0x100];
			ushort EA;

			Opcodes[0x65] = delegate() { EA = aZPG();  clk(3); iADC(Mem[EA]); };
			Opcodes[0x75] = delegate() { EA = aZPX();  clk(4); iADC(Mem[EA]); };
			Opcodes[0x61] = delegate() { EA = aIDX();  clk(6); iADC(Mem[EA]); };
			Opcodes[0x71] = delegate() { EA = aIDY(1); clk(5); iADC(Mem[EA]); };
			Opcodes[0x79] = delegate() { EA = aABY(1); clk(4); iADC(Mem[EA]); };
			Opcodes[0x6d] = delegate() { EA = aABS();  clk(4); iADC(Mem[EA]); };
			Opcodes[0x7d] = delegate() { EA = aABX(1); clk(4); iADC(Mem[EA]); };
			Opcodes[0x69] = delegate() { clk(2); iADC(Mem[PC++]); }; // aIMM

			Opcodes[0x25] = delegate() { EA = aZPG();  clk(3); iAND(Mem[EA]); };
			Opcodes[0x35] = delegate() { EA = aZPX();  clk(4); iAND(Mem[EA]); };
			Opcodes[0x21] = delegate() { EA = aIDX();  clk(6); iAND(Mem[EA]); };
			Opcodes[0x31] = delegate() { EA = aIDY(1); clk(5); iAND(Mem[EA]); };
			Opcodes[0x2d] = delegate() { EA = aABS();  clk(4); iAND(Mem[EA]); };
			Opcodes[0x39] = delegate() { EA = aABY(1); clk(4); iAND(Mem[EA]); };
			Opcodes[0x3d] = delegate() { EA = aABX(1); clk(4); iAND(Mem[EA]); };
			Opcodes[0x29] = delegate() { clk(2); iAND(Mem[PC++]); }; // aIMM

			Opcodes[0x06] = delegate() { EA = aZPG();  clk(5); Mem[EA] = iASL(Mem[EA]); };
			Opcodes[0x16] = delegate() { EA = aZPX();  clk(6); Mem[EA] = iASL(Mem[EA]); };
			Opcodes[0x0e] = delegate() { EA = aABS();  clk(6); Mem[EA] = iASL(Mem[EA]); };
			Opcodes[0x1e] = delegate() { EA = aABX(0); clk(7); Mem[EA] = iASL(Mem[EA]); };
			Opcodes[0x0a] = delegate() { clk(2); A = iASL(A); }; // aACC

			Opcodes[0x24] = delegate() { EA = aZPG();  clk(3); iBIT(Mem[EA]); };
			Opcodes[0x2c] = delegate() { EA = aABS();  clk(4); iBIT(Mem[EA]); };

			Opcodes[0x10] = delegate() { EA = aREL();  clk(2); br(!fN, EA); }; // BPL
			Opcodes[0x30] = delegate() { EA = aREL();  clk(2); br( fN, EA); }; // BMI
			Opcodes[0x50] = delegate() { EA = aREL();  clk(2); br(!fV, EA); }; // BVC
			Opcodes[0x70] = delegate() { EA = aREL();  clk(2); br( fV, EA); }; // BVS
			Opcodes[0x90] = delegate() { EA = aREL();  clk(2); br(!fC, EA); }; // BCC
			Opcodes[0xb0] = delegate() { EA = aREL();  clk(2); br( fC, EA); }; // BCS
			Opcodes[0xd0] = delegate() { EA = aREL();  clk(2); br(!fZ, EA); }; // BNE
			Opcodes[0xf0] = delegate() { EA = aREL();  clk(2); br( fZ, EA); }; // BEQ

			Opcodes[0x00] = delegate() { clk(7); iBRK(); }; // aIMP

			Opcodes[0x18] = delegate() { clk(2); iCLC(); }; // aIMP

			Opcodes[0xd8] = delegate() { clk(2); iCLD(); }; // aIMP

			Opcodes[0x58] = delegate() { clk(2); iCLI(); }; // aIMP

			Opcodes[0xb8] = delegate() { clk(2); iCLV(); }; // aIMP

			Opcodes[0xc5] = delegate() { EA = aZPG();  clk(3); iCMP(Mem[EA]); };
			Opcodes[0xd5] = delegate() { EA = aZPX();  clk(4); iCMP(Mem[EA]); };
			Opcodes[0xc1] = delegate() { EA = aIDX();  clk(6); iCMP(Mem[EA]); };
			Opcodes[0xd1] = delegate() { EA = aIDY(1); clk(5); iCMP(Mem[EA]); };
			Opcodes[0xcd] = delegate() { EA = aABS();  clk(4); iCMP(Mem[EA]); };
			Opcodes[0xdd] = delegate() { EA = aABX(1); clk(4); iCMP(Mem[EA]); };
			Opcodes[0xd9] = delegate() { EA = aABY(1); clk(4); iCMP(Mem[EA]); };
			Opcodes[0xc9] = delegate() { clk(2); iCMP(Mem[PC++]); }; // aIMM

			Opcodes[0xe4] = delegate() { EA = aZPG();  clk(3); iCPX(Mem[EA]); };
			Opcodes[0xec] = delegate() { EA = aABS();  clk(4); iCPX(Mem[EA]); };
			Opcodes[0xe0] = delegate() { clk(2); iCPX(Mem[PC++]); }; // aIMM

			Opcodes[0xc4] = delegate() { EA = aZPG();  clk(3); iCPY(Mem[EA]); };
			Opcodes[0xcc] = delegate() { EA = aABS();  clk(4); iCPY(Mem[EA]); };
			Opcodes[0xc0] = delegate() { clk(2); iCPY(Mem[PC++]); }; // aIMM

			Opcodes[0xc6] = delegate() { EA = aZPG();  clk(5); Mem[EA] = iDEC(Mem[EA]); };
			Opcodes[0xd6] = delegate() { EA = aZPX();  clk(6); Mem[EA] = iDEC(Mem[EA]); };
			Opcodes[0xce] = delegate() { EA = aABS();  clk(6); Mem[EA] = iDEC(Mem[EA]); };
			Opcodes[0xde] = delegate() { EA = aABX(0); clk(7); Mem[EA] = iDEC(Mem[EA]); };

			Opcodes[0xca] = delegate() { clk(2); iDEX(); }; // aIMP

			Opcodes[0x88] = delegate() { clk(2); iDEY(); }; // aIMP

			Opcodes[0x45] = delegate() { EA = aZPG();  clk(3); iEOR(Mem[EA]); };
			Opcodes[0x55] = delegate() { EA = aZPX();  clk(4); iEOR(Mem[EA]); };
			Opcodes[0x41] = delegate() { EA = aIDX();  clk(6); iEOR(Mem[EA]); };
			Opcodes[0x51] = delegate() { EA = aIDY(1); clk(5); iEOR(Mem[EA]); };
			Opcodes[0x4d] = delegate() { EA = aABS();  clk(4); iEOR(Mem[EA]); };
			Opcodes[0x5d] = delegate() { EA = aABX(1); clk(4); iEOR(Mem[EA]); };
			Opcodes[0x59] = delegate() { EA = aABY(1); clk(4); iEOR(Mem[EA]); };
			Opcodes[0x49] = delegate() { clk(2); iEOR(Mem[PC++]); }; // aIMM

			Opcodes[0xe6] = delegate() { EA = aZPG();  clk(5); Mem[EA] = iINC(Mem[EA]); };
			Opcodes[0xf6] = delegate() { EA = aZPX();  clk(6); Mem[EA] = iINC(Mem[EA]); };
			Opcodes[0xee] = delegate() { EA = aABS();  clk(6); Mem[EA] = iINC(Mem[EA]); };
			Opcodes[0xfe] = delegate() { EA = aABX(0); clk(7); Mem[EA] = iINC(Mem[EA]); };

			Opcodes[0xe8] = delegate() { clk(2); iINX(); }; // aIMP

			Opcodes[0xc8] = delegate() { clk(2); iINY(); }; // aIMP

			Opcodes[0xa5] = delegate() { EA = aZPG();  clk(3); iLDA(Mem[EA]); };
			Opcodes[0xb5] = delegate() { EA = aZPX();  clk(4); iLDA(Mem[EA]); };
			Opcodes[0xa1] = delegate() { EA = aIDX();  clk(6); iLDA(Mem[EA]); };
			Opcodes[0xb1] = delegate() { EA = aIDY(1); clk(5); iLDA(Mem[EA]); };
			Opcodes[0xad] = delegate() { EA = aABS();  clk(4); iLDA(Mem[EA]); };
			Opcodes[0xbd] = delegate() { EA = aABX(1); clk(4); iLDA(Mem[EA]); };
			Opcodes[0xb9] = delegate() { EA = aABY(1); clk(4); iLDA(Mem[EA]); };
			Opcodes[0xa9] = delegate() { clk(2); iLDA(Mem[PC++]); }; // aIMM

			Opcodes[0xa6] = delegate() { EA = aZPG();  clk(3); iLDX(Mem[EA]); };
			Opcodes[0xb6] = delegate() { EA = aZPY();  clk(4); iLDX(Mem[EA]); };
			Opcodes[0xae] = delegate() { EA = aABS();  clk(4); iLDX(Mem[EA]); };
			Opcodes[0xbe] = delegate() { EA = aABY(1); clk(4); iLDX(Mem[EA]); };
			Opcodes[0xa2] = delegate() { clk(2); iLDX(Mem[PC++]); }; // aIMM

			Opcodes[0xa4] = delegate() { EA = aZPG();  clk(3); iLDY(Mem[EA]); };
			Opcodes[0xb4] = delegate() { EA = aZPX();  clk(4); iLDY(Mem[EA]); };
			Opcodes[0xac] = delegate() { EA = aABS();  clk(4); iLDY(Mem[EA]); };
			Opcodes[0xbc] = delegate() { EA = aABX(1); clk(4); iLDY(Mem[EA]); };
			Opcodes[0xa0] = delegate() { clk(2); iLDY(Mem[PC++]); }; // aIMM

			Opcodes[0x46] = delegate() { EA = aZPG();  clk(5); Mem[EA] = iLSR(Mem[EA]); };
			Opcodes[0x56] = delegate() { EA = aZPX();  clk(6); Mem[EA] = iLSR(Mem[EA]); };
			Opcodes[0x4e] = delegate() { EA = aABS();  clk(6); Mem[EA] = iLSR(Mem[EA]); };
			Opcodes[0x5e] = delegate() { EA = aABX(0); clk(7); Mem[EA] = iLSR(Mem[EA]); };
			Opcodes[0x4a] = delegate() { clk(2); A = iLSR(A); }; // aACC

			Opcodes[0x4c] = delegate() { EA = aABS();  clk(3); iJMP(EA); };
			Opcodes[0x6c] = delegate() { EA = aIND();  clk(5); iJMP(EA); };

			Opcodes[0x20] = delegate() { EA = aABS();  clk(6); iJSR(EA); };

			Opcodes[0xea] = delegate() { clk(2); iNOP(); }; // aIMP

			Opcodes[0x05] = delegate() { EA = aZPG();  clk(3); iORA(Mem[EA]); };
			Opcodes[0x15] = delegate() { EA = aZPX();  clk(4); iORA(Mem[EA]); };
			Opcodes[0x01] = delegate() { EA = aIDX();  clk(6); iORA(Mem[EA]); };
			Opcodes[0x11] = delegate() { EA = aIDY(1); clk(5); iORA(Mem[EA]); };
			Opcodes[0x0d] = delegate() { EA = aABS();  clk(4); iORA(Mem[EA]); };
			Opcodes[0x1d] = delegate() { EA = aABX(1); clk(4); iORA(Mem[EA]); };
			Opcodes[0x19] = delegate() { EA = aABY(1); clk(4); iORA(Mem[EA]); };
			Opcodes[0x09] = delegate() { clk(2); iORA(Mem[PC++]); }; // aIMM

			Opcodes[0x48] = delegate() { clk(3); iPHA(); }; // aIMP

			Opcodes[0x68] = delegate() { clk(4); iPLA(); }; // aIMP

			Opcodes[0x08] = delegate() { clk(3); iPHP(); }; // aIMP

			Opcodes[0x28] = delegate() { clk(4); iPLP(); }; // aIMP

			Opcodes[0x26] = delegate() { EA = aZPG();  clk(5); Mem[EA] = iROL(Mem[EA]); };
			Opcodes[0x36] = delegate() { EA = aZPX();  clk(6); Mem[EA] = iROL(Mem[EA]); };
			Opcodes[0x2e] = delegate() { EA = aABS();  clk(6); Mem[EA] = iROL(Mem[EA]); };
			Opcodes[0x3e] = delegate() { EA = aABX(0); clk(7); Mem[EA] = iROL(Mem[EA]); };
			Opcodes[0x2a] = delegate() { clk(2); A = iROL(A); }; // aACC

			Opcodes[0x66] = delegate() { EA = aZPG();  clk(5); Mem[EA] = iROR(Mem[EA]); };
			Opcodes[0x76] = delegate() { EA = aZPX();  clk(6); Mem[EA] = iROR(Mem[EA]); };
			Opcodes[0x6e] = delegate() { EA = aABS();  clk(6); Mem[EA] = iROR(Mem[EA]); };
			Opcodes[0x7e] = delegate() { EA = aABX(0); clk(7); Mem[EA] = iROR(Mem[EA]); };
			Opcodes[0x6a] = delegate() { clk(2); A = iROR(A); }; // aACC

			Opcodes[0x40] = delegate() { clk(6); iRTI(); }; // aIMP

			Opcodes[0x60] = delegate() { clk(6); iRTS(); }; // aIMP

			Opcodes[0xe5] = delegate() { EA = aZPG();  clk(3); iSBC(Mem[EA]); };
			Opcodes[0xf5] = delegate() { EA = aZPX();  clk(4); iSBC(Mem[EA]); };
			Opcodes[0xe1] = delegate() { EA = aIDX();  clk(6); iSBC(Mem[EA]); };
			Opcodes[0xf1] = delegate() { EA = aIDY(1); clk(5); iSBC(Mem[EA]); };
			Opcodes[0xed] = delegate() { EA = aABS();  clk(4); iSBC(Mem[EA]); };
			Opcodes[0xfd] = delegate() { EA = aABX(1); clk(4); iSBC(Mem[EA]); };
			Opcodes[0xf9] = delegate() { EA = aABY(1); clk(4); iSBC(Mem[EA]); };
			Opcodes[0xe9] = delegate() { clk(2); iSBC(Mem[PC++]); }; // aIMM

			Opcodes[0x38] = delegate() { clk(2); iSEC(); }; // aIMP

			Opcodes[0xf8] = delegate() { clk(2); iSED(); }; // aIMP

			Opcodes[0x78] = delegate() { clk(2); iSEI(); }; // aIMP

			Opcodes[0x85] = delegate() { EA = aZPG();  clk(3); Mem[EA] = iSTA(); };
			Opcodes[0x95] = delegate() { EA = aZPX();  clk(4); Mem[EA] = iSTA(); };
			Opcodes[0x81] = delegate() { EA = aIDX();  clk(6); Mem[EA] = iSTA(); };
			Opcodes[0x91] = delegate() { EA = aIDY(0); clk(6); Mem[EA] = iSTA(); };
			Opcodes[0x8d] = delegate() { EA = aABS();  clk(4); Mem[EA] = iSTA(); };
			Opcodes[0x99] = delegate() { EA = aABY(0); clk(5); Mem[EA] = iSTA(); };
			Opcodes[0x9d] = delegate() { EA = aABX(0); clk(5); Mem[EA] = iSTA(); };

			Opcodes[0x86] = delegate() { EA = aZPG();  clk(3); Mem[EA] = iSTX(); };
			Opcodes[0x96] = delegate() { EA = aZPY();  clk(4); Mem[EA] = iSTX(); };
			Opcodes[0x8e] = delegate() { EA = aABS();  clk(4); Mem[EA] = iSTX(); };

			Opcodes[0x84] = delegate() { EA = aZPG();  clk(3); Mem[EA] = iSTY(); };
			Opcodes[0x94] = delegate() { EA = aZPX();  clk(4); Mem[EA] = iSTY(); };
			Opcodes[0x8c] = delegate() { EA = aABS();  clk(4); Mem[EA] = iSTY(); };

			Opcodes[0xaa] = delegate() { clk(2); iTAX(); }; // aIMP

			Opcodes[0xa8] = delegate() { clk(2); iTAY(); }; // aIMP

			Opcodes[0xba] = delegate() { clk(2); iTSX(); }; // aIMP

			Opcodes[0x8a] = delegate() { clk(2); iTXA(); }; // aIMP

			Opcodes[0x9a] = delegate() { clk(2); iTXS(); }; // aIMP

			Opcodes[0x98] = delegate() { clk(2); iTYA(); }; // aIMP

			// Illegal opcodes
			foreach (int opCode in new ushort[] {0x02, 0x12, 0x22, 0x32, 0x42, 0x52, 0x62, 0x72, 0x92, 0xb2, 0xd2, 0xf2})
			{
				Opcodes[opCode] = delegate() { clk(2); iKIL(); };
			}
			Opcodes[0x3f] = delegate() { EA = aABX(0); clk(4); iRLA(Mem[EA]); };
			Opcodes[0xa7] = delegate() { EA = aZPX();  clk(3); iLAX(Mem[EA]); };
			Opcodes[0xb3] = delegate() { EA = aIDY(0); clk(6); iLAX(Mem[EA]); };
			Opcodes[0xef] = delegate() { EA = aABS();  clk(6); iISB(Mem[EA]); };
			Opcodes[0x0c] = delegate() { EA = aABS();  clk(2); iNOP(); };
			foreach (int opCode in new ushort[] {0x1c, 0x3c, 0x5c, 0x7c, 0x9c, 0xdc, 0xfc})
			{
				Opcodes[opCode] = delegate() { EA = aABX(0); clk(2); iNOP(); };
			}
			Opcodes[0x83] = delegate() { EA = aIDX();  clk(6); Mem[EA] = iSAX(); };
			Opcodes[0x87] = delegate() { EA = aZPG();  clk(3); Mem[EA] = iSAX(); };
			Opcodes[0x8f] = delegate() { EA = aABS();  clk(4); Mem[EA] = iSAX(); };
			Opcodes[0x97] = delegate() { EA = aZPY();  clk(4); Mem[EA] = iSAX(); };
			Opcodes[0xa3] = delegate() { EA = aIDX();  clk(6); iLAX(Mem[EA]); };
			Opcodes[0xb7] = delegate() { EA = aZPY();  clk(4); iLAX(Mem[EA]); };
			Opcodes[0xaf] = delegate() { EA = aABS();  clk(5); iLAX(Mem[EA]); };
			Opcodes[0xbf] = delegate() { EA = aABY(0); clk(6); iLAX(Mem[EA]); };
			Opcodes[0xff] = delegate() { EA = aABX(0); clk(7); iISB(Mem[EA]); };

			OpcodeHandler opNULL = delegate()
			{
				Trace.WriteLine(String.Format("{0}:**UNKNOWN OPCODE: ${1:x2} at ${2:x4}\n", this, Mem[(ushort)(PC-1)], PC-1));
			};

			for (int i=0; i < Opcodes.Length; i++)
			{
				if (Opcodes[i] == null)
				{
					Opcodes[i] = opNULL;
				}
			}
		}
	}
}
*/
