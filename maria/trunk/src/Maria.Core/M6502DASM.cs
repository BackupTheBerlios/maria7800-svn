/*
 * Provides disassembly services.
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
using System.Globalization;
using System.Text;

namespace Maria.Core {
	// TODO : finish, if we change stuff, by all means test it...
	// TODO : make sure calls to xxxFormat() supply CultureInfo.InvariantCulture
	public class M6502DASM {
		
		// Instruction Mnemonics
		private enum m : uint {
			XXX,
			ADC, AND, ASL,
			BIT, BCC, BCS, BEQ, BMI, BNE, BPL, BRK, BVC, BVS,
			CLC, CLD, CLI, CLV, CMP, CPX, CPY,
			DEC, DEX, DEY,
			EOR,
			INC, INX, INY,
			JMP, JSR,
			LDA, LDX, LDY, LSR,
			NOP,
			ORA,
			PLA, PLP, PHA, PHP,
			ROL, ROR, RTI, RTS,
			SEC, SEI, STA, SBC, SED, STX, STY,
			TAX, TAY, TSX, TXA, TXS, TYA,
			// Illegal/undefined opcodes
			isb,
			kil,
			lax,
			rla,
			sax,
			top
		}
		
		// Addressing Modes
		private enum a : uint {
			REL,	// Relative: $aa (branch instructions only)
			ZPG,	// Zero Page: $aa
			ZPX,	// Zero Page Indexed X: $aa,X
			ZPY,	// Zero Page Indexed Y: $aa,Y
			ABS,	// Absolute: $aaaa
			ABX,	// Absolute Indexed X: $aaaa,X
			ABY,	// Absolute Indexed Y: $aaaa,Y
			IDX,	// Indexed Indirect: ($aa,X)		
			IDY,	// Indirect Indexed: ($aa),Y	
			IND,	// Indirect Absolute: ($aaaa) (JMP only)
			IMM,	// Immediate: #aa
			IMP,	// Implied
			ACC,	// Accumulator
		}
		
		public static string GetRegisters(M6502 cpu) {
			StringBuilder result = new StringBuilder();
			result.AppendFormat(
				CultureInfo.InvariantCulture,
				"PC:{0:x4} A:{1:x2} X:{2:x2} Y:{3:x2} S:{4:x2} P:{5}",
				cpu.PC, cpu.A, cpu.X, cpu.Y, cpu.S, GetFlags(cpu.P)
			);
			return result.ToString();
		}
		
		public static string GetFlags(byte cpuFlags) {
			const string flags = "nv0bdizcNV1BDIZC";
			StringBuilder result = new StringBuilder();			
			byte mask = 0x80;
			for (int i = 0; i < 8; ++i) {
				result.Append((cpuFlags & mask) == 0 ? flags[i] : flags[i+8]);
				mask >>= 1;
			}
			return result.ToString();
		}

		public static string MemDump(IAddressable mem, ushort atAddr, ushort untilAddr) {
			StringBuilder result = new StringBuilder();
			int length = untilAddr - atAddr;
			while (length-- >= 0) {
				result.AppendFormat(CultureInfo.InvariantCulture, "{0:x4}: ", atAddr);
				for (int i = 0; i < 8; ++i) {
					result.AppendFormat(CultureInfo.InvariantCulture, "{0:x2} ", mem[atAddr++]);
					if (i == 3)
						result.Append(" ");
				}
				result.Append(Environment.NewLine);
			}
			if (result.Length > 0)
				result.Length--; // Trim trailing newline
			return result.ToString();
		}
	}
}

// TODO : old stuff below, port
/*
		static m[] MnemonicMatrix = {
			// Get it from original src
};

		static a[] AddressingModeMatrix = {
			get it from original src
};

		public static string Disassemble(AddressSpace mem, ushort atAddr, ushort untilAddr)
		{
			dSB = new StringBuilder();

			dPC = atAddr;
			while (atAddr < untilAddr)
			{
				dSB.Append(String.Format("{0:x4}: ", dPC));

				int len = InstructionLength(mem, dPC);

				for (int i = 0; i < 3; i++)
				{
					if (i < len)
					{
						dSB.Append(String.Format("{0:x2} ", mem[atAddr++]));
					}
					else
					{
						dSB.Append("   ");
					}
				}
				dSB.Append(String.Format("{0,-15:s}\n", OpCodeDecode(mem, dPC)));

				dPC += (ushort)len;
			}
			if (dSB.Length > 0)
			{
				dSB.Length--;  // Trim trailing newline
			}
			return dSB.ToString();
		}

		public static string OpCodeDecode(AddressSpace mem, ushort PC)
		{
			int num_operands = InstructionLength(mem, PC) - 1;
			ushort PC1 = (ushort)(PC + 1);
			string addrmodeStr;

			switch (AddressingModeMatrix[mem[PC]])
			{
				case a.REL:
					addrmodeStr = String.Format("${0:x4}", (ushort)(PC + (sbyte)(mem[PC1]) + 2));
					break;
				case a.ZPG:
				case a.ABS:
					addrmodeStr = EA(mem, PC1, num_operands);
					break;
				case a.ZPX:
				case a.ABX:
					addrmodeStr = EA(mem, PC1, num_operands) + ",X";
					break;
				case a.ZPY:
				case a.ABY:
					addrmodeStr = EA(mem, PC1, num_operands) + ",Y";
					break;
				case a.IDX:
					addrmodeStr = "(" + EA(mem, PC1, num_operands) + ",X)";
					break;
				case a.IDY:
					addrmodeStr = "(" + EA(mem, PC1, num_operands) + "),Y";
					break;
				case a.IND:
					addrmodeStr = "(" + EA(mem, PC1, num_operands) + ")";
					break;
				case a.IMM:
					addrmodeStr = "#" + EA(mem, PC1, num_operands);
					break;
				default:
					// a.IMP, a.ACC
					addrmodeStr = "";
					break;
			}

			return String.Format("{0} {1}", MnemonicMatrix[mem[PC]], addrmodeStr);
		}

		static int InstructionLength(AddressSpace mem, ushort PC)
		{
			switch (AddressingModeMatrix[mem[PC]])
			{
				case a.ACC:
				case a.IMP:
					return 1;
				case a.REL:
				case a.ZPG:
				case a.ZPX:
				case a.ZPY:
				case a.IDX:
				case a.IDY:
				case a.IMM:
					return 2;
				case a.ABS:
				case a.ABX:
				case a.ABY:
				case a.IND:
				default:
					return 3;
			}
		}

		static string EA(AddressSpace mem, ushort PC, int bytes)
		{
			byte lsb = mem[PC];
			byte msb = 0;

			if (bytes == 2)
			{
				msb = mem[(ushort)(PC + 1)];
			}

			ushort ea = (ushort)(lsb | (msb << 8));

			if (bytes == 1)
			{
				return String.Format("${0:x2}", ea);
			}
			else
			{
				return String.Format("${0:x4}", ea);
			}
		}
	}
}
*/
