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
	public class RAM6116Test {
		private IDevice fRAM;

		[SetUp]
		public void SetUp() {
			fRAM = new RAM6116();
			for (ushort adx=0; adx<fRAM.Size; ++adx)
				fRAM[adx] = 0;
		}

		[Test]
		public void TestSize() {
			Assert.AreEqual(0x800, fRAM.Size);
		}

		[Test]
		public void TestNormalReadWrite() {
			for (ushort adx=0; adx<fRAM.Size; ++adx) {
				Assert.AreEqual(0, fRAM[adx]);
				fRAM[adx] = 42;
				Assert.AreEqual(42, fRAM[adx]);
			}
		}

		[Test]
		public void TestReadWraparound() {
			fRAM[0] = 42;
			fRAM[1] = 23;
			Assert.AreEqual(42, fRAM[(ushort) fRAM.Size]);
			Assert.AreEqual(23, fRAM[(ushort) (fRAM.Size * 2 + 1)]);
		}

		[Test]
		public void TestWriteWraparound() {
			fRAM[(ushort) fRAM.Size] = 42;
			fRAM[(ushort) (fRAM.Size * 2 + 1)] = 23;
			Assert.AreEqual(42, fRAM[0]);
			Assert.AreEqual(23, fRAM[1]);
		}

		[Test]
		public void TestRequestSnooping() {
			Assert.IsFalse(fRAM.RequestSnooping);
		}
	}
}
