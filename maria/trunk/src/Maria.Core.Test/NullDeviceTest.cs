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
	public class NullDeviceTest {
		private IDevice nullDevice;

		[SetUp]
		public void SetUp() {
			nullDevice = new NullDevice();
			nullDevice.Reset();
		}

		[Test]
		public void TestToString() {
			Assert.AreEqual("NullDevice", nullDevice.ToString());
		}

		[Test]
		public void TestReadWrite() {
			for (uint address=0; address<0x10000; address++) {
				Assert.AreEqual(0, nullDevice[(ushort) address]);
				nullDevice[(ushort) address] = 42;
				Assert.AreEqual(0, nullDevice[(ushort) address]);
			}
		}

		[Test]
		public void TestRequestSnooping() {
			Assert.IsFalse(nullDevice.RequestSnooping);
		}
	}
}
