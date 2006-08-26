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

		private static readonly m[] mnemonicMatrix = {
//   		0      1      2      3      4      5      6      7      8      9      a      b      c      d      e      f
/*0*/		m.BRK, m.ORA, m.kil, m.XXX, m.XXX, m.ORA, m.ASL, m.XXX, m.PHP, m.ORA, m.ASL, m.XXX, m.top, m.ORA, m.ASL, m.XXX,
/*1*/		m.BPL, m.ORA, m.kil, m.XXX, m.XXX, m.ORA, m.ASL, m.XXX, m.CLC, m.ORA, m.XXX, m.XXX, m.top, m.ORA, m.ASL, m.XXX,
/*2*/		m.JSR, m.AND, m.kil, m.XXX, m.BIT, m.AND, m.ROL, m.XXX, m.PLP, m.AND, m.ROL, m.XXX, m.BIT, m.AND, m.ROL, m.XXX,
/*3*/		m.BMI, m.AND, m.kil, m.XXX, m.XXX, m.AND, m.ROL, m.XXX, m.SEC, m.AND, m.XXX, m.XXX, m.top, m.AND, m.ROL, m.rla,
/*4*/		m.RTI, m.EOR, m.kil, m.XXX, m.XXX, m.EOR, m.LSR, m.XXX, m.PHA, m.EOR, m.LSR, m.XXX, m.JMP, m.EOR, m.LSR, m.XXX,
/*5*/		m.BVC, m.EOR, m.kil, m.XXX, m.XXX, m.EOR, m.LSR, m.XXX, m.CLI, m.EOR, m.XXX, m.XXX, m.top, m.EOR, m.LSR, m.XXX,
/*6*/		m.RTS, m.ADC, m.kil, m.XXX, m.XXX, m.ADC, m.ROR, m.XXX, m.PLA, m.ADC, m.ROR, m.XXX, m.JMP, m.ADC, m.ROR, m.XXX,
/*7*/		m.BVS, m.ADC, m.kil, m.XXX, m.XXX, m.ADC, m.ROR, m.XXX, m.SEI, m.ADC, m.XXX, m.XXX, m.top, m.ADC, m.ROR, m.XXX,
/*8*/		m.XXX, m.STA, m.XXX, m.sax, m.STY, m.STA, m.STX, m.sax, m.DEY, m.XXX, m.TXA, m.XXX, m.STY, m.STA, m.STX, m.sax,
/*9*/		m.BCC, m.STA, m.kil, m.XXX, m.STY, m.STA, m.STX, m.sax, m.TYA, m.STA, m.TXS, m.XXX, m.top, m.STA, m.XXX, m.XXX,
/*a*/		m.LDY, m.LDA, m.LDX, m.lax, m.LDY, m.LDA, m.LDX, m.lax, m.TAY, m.LDA, m.TAX, m.XXX, m.LDY, m.LDA, m.LDX, m.lax,
/*b*/		m.BCS, m.LDA, m.kil, m.lax, m.LDY, m.LDA, m.LDX, m.lax, m.CLV, m.LDA, m.TSX, m.XXX, m.LDY, m.LDA, m.LDX, m.lax,
/*c*/		m.CPY, m.CMP, m.XXX, m.XXX, m.CPY, m.CMP, m.DEC, m.XXX, m.INY, m.CMP, m.DEX, m.XXX, m.CPY, m.CMP, m.DEC, m.XXX,
/*d*/		m.BNE, m.CMP, m.kil, m.XXX, m.XXX, m.CMP, m.DEC, m.XXX, m.CLD, m.CMP, m.XXX, m.XXX, m.top, m.CMP, m.DEC, m.XXX,
/*e*/		m.CPX, m.SBC, m.XXX, m.XXX, m.CPX, m.SBC, m.INC, m.XXX, m.INX, m.SBC, m.NOP, m.XXX, m.CPX, m.SBC, m.INC, m.isb,
/*f*/		m.BEQ, m.SBC, m.kil, m.XXX, m.XXX, m.SBC, m.INC, m.XXX, m.SED, m.SBC, m.XXX, m.XXX, m.top, m.SBC, m.INC, m.isb
		};

		private static readonly a[] addressingModeMatrix = {
//   		0      1      2      3      4      5      6      7      8      9      A      B      C      D      E      F
/*0*/		a.IMP, a.IDX, a.IMP, a.REL, a.REL, a.ZPG, a.ZPG, a.REL, a.IMP, a.IMM, a.ACC, a.REL, a.ABS, a.ABS, a.ABS, a.REL,
/*1*/		a.REL, a.IDY, a.IMP, a.REL, a.REL, a.ZPG, a.ZPG, a.REL, a.IMP, a.ABY, a.REL, a.REL, a.ABS, a.ABX, a.ABX, a.REL,
/*2*/		a.ABS, a.IDX, a.IMP, a.REL, a.ZPG, a.ZPG, a.ZPG, a.REL, a.IMP, a.IMM, a.ACC, a.REL, a.ABS, a.ABS, a.ABS, a.REL,
/*3*/		a.REL, a.IDY, a.IMP, a.REL, a.REL, a.ZPG, a.ZPG, a.REL, a.IMP, a.ABY, a.REL, a.REL, a.ABS, a.ABX, a.ABX, a.ABX,
/*4*/		a.IMP, a.IDY, a.IMP, a.REL, a.REL, a.ZPG, a.ZPG, a.REL, a.IMP, a.IMM, a.ACC, a.REL, a.ABS, a.ABS, a.ABS, a.REL,
/*5*/		a.REL, a.IDY, a.IMP, a.REL, a.REL, a.ZPG, a.ZPG, a.REL, a.IMP, a.ABY, a.REL, a.REL, a.ABS, a.ABX, a.ABX, a.REL,
/*6*/		a.IMP, a.IDX, a.IMP, a.REL, a.REL, a.ZPG, a.ZPG, a.REL, a.IMP, a.IMM, a.ACC, a.REL, a.IND, a.ABS, a.ABS, a.REL,
/*7*/		a.REL, a.IDY, a.IMP, a.REL, a.REL, a.ZPX, a.ZPX, a.REL, a.IMP, a.ABY, a.REL, a.REL, a.ABS, a.ABX, a.ABX, a.REL,
/*8*/		a.REL, a.IDY, a.REL, a.IDX, a.ZPG, a.ZPG, a.ZPG, a.ZPG, a.IMP, a.REL, a.IMP, a.REL, a.ABS, a.ABS, a.ABS, a.ABS,
/*9*/		a.REL, a.IDY, a.IMP, a.REL, a.ZPX, a.ZPX, a.ZPY, a.ZPY, a.IMP, a.ABY, a.IMP, a.REL, a.ABS, a.ABX, a.REL, a.REL,
/*A*/		a.IMM, a.IND, a.IMM, a.IDX, a.ZPG, a.ZPG, a.ZPG, a.ZPX, a.IMP, a.IMM, a.IMP, a.REL, a.ABS, a.ABS, a.ABS, a.ABS,
/*B*/		a.REL, a.IDY, a.IMP, a.IDY, a.ZPX, a.ZPX, a.ZPY, a.ZPY, a.IMP, a.ABY, a.IMP, a.REL, a.ABX, a.ABX, a.ABY, a.ABY,
/*C*/		a.IMM, a.IDX, a.REL, a.REL, a.ZPG, a.ZPG, a.ZPG, a.REL, a.IMP, a.IMM, a.IMP, a.REL, a.ABS, a.ABS, a.ABS, a.REL,
/*D*/		a.REL, a.IDY, a.IMP, a.REL, a.REL, a.ZPX, a.ZPX, a.REL, a.IMP, a.ABY, a.REL, a.REL, a.ABS, a.ABX, a.ABX, a.REL,
/*E*/		a.IMM, a.IDX, a.REL, a.REL, a.ZPG, a.ZPG, a.ZPG, a.REL, a.IMP, a.IMM, a.IMP, a.REL, a.ABS, a.ABS, a.ABS, a.ABS,
/*F*/		a.REL, a.IDY, a.IMP, a.REL, a.REL, a.ZPX, a.ZPX, a.REL, a.IMP, a.ABY, a.REL, a.REL, a.ABS, a.ABX, a.ABX, a.ABX
		};

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

		public static string Disassemble(IAddressable mem, ushort atAddr, ushort untilAddr) {
			StringBuilder result = new StringBuilder();
			ushort dPC = atAddr;
			while (atAddr < untilAddr) {
				result.AppendFormat(CultureInfo.InvariantCulture, "{0:x4}: ", dPC);
				int len = InstructionLength(mem, dPC);
				for (int i = 0; i < 3; i++) {
					if (i < len)
						result.AppendFormat(CultureInfo.InvariantCulture, "{0:x2} ", mem[atAddr++]);
					else
						result.Append("   ");
				}
				result.AppendFormat(CultureInfo.InvariantCulture, "{0,-15:s}{1}",
					OpCodeDecode(mem, dPC), Environment.NewLine);
				dPC += (ushort)len;
			}
			if (result.Length > 0)
				result.Length--;  // Trim trailing newline
			return result.ToString();
		}

		public static string OpCodeDecode(IAddressable mem, ushort PC) {
			int num_operands = InstructionLength(mem, PC) - 1;
			ushort PC1 = (ushort)(PC + 1);
			string addrmodeStr;
			switch (addressingModeMatrix[mem[PC]]) {
				case a.REL:
					addrmodeStr = String.Format(CultureInfo.InvariantCulture,
						"${0:x4}", (ushort)(PC + (sbyte)(mem[PC1]) + 2));
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
			return String.Format(CultureInfo.InvariantCulture,
				"{0} {1}", mnemonicMatrix[mem[PC]], addrmodeStr);
		}

		private static string EA(IAddressable mem, ushort PC, int bytes) {
			byte lsb = mem[PC];
			byte msb = 0;
			if (bytes == 2)
				msb = mem[(ushort)(PC + 1)];
			ushort ea = (ushort)(lsb | (msb << 8));
			if (bytes == 1)
				return String.Format(CultureInfo.InvariantCulture, "${0:x2}", ea);
			else
				return String.Format(CultureInfo.InvariantCulture, "${0:x4}", ea);
		}

		private static int InstructionLength(IAddressable mem, ushort PC) {
			switch (addressingModeMatrix[mem[PC]]) {
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
	}
}
