/*
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
using NUnit.Framework;

namespace Maria.Core {
	[TestFixture]
	public class M6502DASMTest {
		private M6502 cpu;
		private IDevice ram;

		[SetUp]
		public void SetUp() {
			cpu = new M6502(ram);
			cpu.PC = 0x1234;
			cpu.A = 1;
			cpu.X = 2;
			cpu.Y = 3;
			cpu.S = 4;
			cpu.P = 0;
			ram = new RAM6116();
			for (ushort addr = 0; addr < 256; ++addr)
				ram[addr] = (byte) addr;
		}

		[Test]
		public void TestGetRegisters() {
			Assert.AreEqual(
				"PC:1234 A:01 X:02 Y:03 S:04 P:nv0bdizc",
				M6502DASM.GetRegisters(cpu)
			);
		}

		[Test]
		public void TestGetFlags() {
			Assert.AreEqual("nv0bdizc", M6502DASM.GetFlags(0));
			Assert.AreEqual("Nv0bdizc", M6502DASM.GetFlags(0x80));
			Assert.AreEqual("nv0bdizC", M6502DASM.GetFlags(1));
			Assert.AreEqual("nV0bDIzc", M6502DASM.GetFlags(0x4C));
		}

		[Test]
		public void TestMemDump() {
			Assert.AreEqual(
				"0000: 00 01 02 03  04 05 06 07 " + Environment.NewLine +
				"0008: 08 09 0a 0b  0c 0d 0e 0f ",
				M6502DASM.MemDump(ram, 0, 1)
			);
		}

		[Test]
		public void TestDisassembleData() {
			Assert.AreEqual(
				"0000: 00       BRK            " + Environment.NewLine +
				"0001: 01 02    ORA ($02,X)    " + Environment.NewLine +
				"0003: 03 04    XXX $0009      " + Environment.NewLine +
				"0005: 05 06    ORA $06        " + Environment.NewLine +
				"0007: 07 08    XXX $0011      " + Environment.NewLine +
				"0009: 09 0a    ORA #$0a       " + Environment.NewLine +
				"000b: 0b 0c    XXX $0019      " + Environment.NewLine +
				"000d: 0d 0e 0f ORA $0f0e      ",
				M6502DASM.Disassemble(ram, 0, 16)
			);
		}
	}
}
