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
	public class M6502Test {
		
		private M6502 cpu;
		
		[SetUp]
		public void SetUp() {
			cpu = new M6502();
		}
		
		[Test]
		public void TestReset() {
			cpu.Jammed = true;
			cpu.S = 0x42;
			cpu.PC = 0x1234;			
			cpu.Reset();
			Assert.IsFalse(cpu.Jammed, "Reset didn't unjam CPU.");
			Assert.AreEqual(0xff, cpu.S, "Stack pointer not reset.");
			// TODO : check PC is reset correctly. Need ctor for that, though.
		}
	}
}