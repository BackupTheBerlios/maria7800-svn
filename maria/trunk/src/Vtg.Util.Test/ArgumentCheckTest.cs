/*
 * Copyright (C) 2006 Thomas Mathys (tom42@users.berlios.de)
 *
 * This file is part of Vtg.Util.
 *
 * Vtg.Util is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * Vtg.Util is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Vtg.Util; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */
using System;
using NUnit.Framework;

namespace Vtg.Util {
	[TestFixture]
	public class ArgumentCheckTest {
		[Test]
		public void TestNotNull() {
			ArgumentCheck.NotNull(true, "someString");
			try {
				ArgumentCheck.NotNull(null, "paramName");
				Assert.Fail("ArgumentCheck.NotNull() " +
					"should have thrown ArgumentNullException");
			}
			catch (ArgumentNullException expected) {
				Assert.IsTrue(expected.Message.IndexOf("paramName") > 0);
			}
		}
	}
}
