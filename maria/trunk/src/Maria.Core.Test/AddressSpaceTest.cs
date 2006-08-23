/*
 * This file is part of Maria.
 * Copyright (C) 2006 Thomas Mathys (tom42@users.berlios.de)
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
	public class AddressSpaceTest {

		private AddressSpace CreateAndCheck(Machine m, int addrSpaceShift,
			int pageShift,
			int expectedAddrSpaceSize, int expectedPageSize,
			int expectedPageCount) {
			AddressSpace result = new AddressSpace(m, addrSpaceShift, pageShift);
			Assert.AreEqual(m, result.Machine);
			Assert.AreEqual(expectedAddrSpaceSize, result.AddrSpaceSize);
			Assert.AreEqual(expectedPageSize, result.PageSize);
			Assert.AreEqual(expectedPageCount, result.PageCount);
			return result;
		}

		[Test]
		public void TestCreation() {
			CreateAndCheck(new Machine(), 16, 12, 0x10000, 0x1000, 16);
			CreateAndCheck(new Machine(), 8, 6, 256, 64, 4);
		}

		[Test]
		public void TestEmpty() {
			AddressSpace s = CreateAndCheck(new Machine(), 16, 13, 0x10000, 0x2000, 8);
			int addr;
			for (addr = 0; addr < s.AddrSpaceSize; ++addr) {
				s[(ushort) addr] = 0x42;
				Assert.AreEqual(0, s[(ushort) addr]);
			}
			Assert.AreEqual(addr, s.AddrSpaceSize); 
		}
	}
}

