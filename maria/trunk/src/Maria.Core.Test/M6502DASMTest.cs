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
		
		[SetUp]
		public void SetUp() {
			cpu = new M6502();
			cpu.PC = 0x1234;
			cpu.A = 1;
			cpu.X = 2;
			cpu.Y = 3;
			cpu.S = 4;
			cpu.P = 0;
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
		public void TestMemoryDump() {
			// TODO : test something
		}
	}
}
