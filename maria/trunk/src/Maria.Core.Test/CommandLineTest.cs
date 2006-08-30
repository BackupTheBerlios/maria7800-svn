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
	public class CommandLineTest {

		[Test]
		public void TestEmpty() {
			CommandLine cl1 = new CommandLine("");
			Assert.IsNull(cl1.Verb);
			Assert.AreEqual(0, cl1.Parms.Length);
			CommandLine cl2 = new CommandLine("   ");
			Assert.IsNull(cl2.Verb);
			Assert.AreEqual(0, cl2.Parms.Length);
		}

		[Test]
		public void TestCommandOnly() {
			CommandLine cl1 = new CommandLine("  RUN! ");
			Assert.AreEqual("RUN!", cl1.Verb);
			Assert.AreEqual(0, cl1.Parms.Length);
		}

		[Test]
		public void TestStringParameters() {
			CommandLine cl1 = new CommandLine("Press play on tape.");
			Assert.AreEqual("Press", cl1.Verb);
			Assert.AreEqual(3, cl1.Parms.Length);
			Assert.AreEqual("play", cl1.Parms[0].StrValue);
			Assert.AreEqual(0, cl1.Parms[0].IntValue);
			Assert.IsFalse(cl1.Parms[0].IsInteger);
			Assert.AreEqual("on", cl1.Parms[1].StrValue);
			Assert.AreEqual(0, cl1.Parms[1].IntValue);
			Assert.IsFalse(cl1.Parms[1].IsInteger);
			Assert.AreEqual("tape.", cl1.Parms[2].StrValue);
			Assert.AreEqual(0, cl1.Parms[2].IntValue);
			Assert.IsFalse(cl1.Parms[2].IsInteger);
		}

		[Test]
		public void TestIntegerParameters() {
			CheckIntegerParameter("23", 23);
			CheckIntegerParameter("$10", 16);
			CheckIntegerParameter("0x20", 32);
		}
		private void CheckIntegerParameter(string integerParam, int expectedInt) {
			CommandLine cl = new CommandLine("p " + integerParam);
			Assert.AreEqual("p", cl.Verb);
			Assert.AreEqual(1, cl.Parms.Length);
			Assert.AreEqual(integerParam, cl.Parms[0].StrValue);
			Assert.IsTrue(cl.Parms[0].IsInteger, "Parameter is not an integer.");
			Assert.AreEqual(expectedInt, cl.Parms[0].IntValue);
		}

		[Test]
		public void TestCheckParms() {
			Assert.IsTrue(new CommandLine("").CheckParms(""));
			Assert.IsTrue(new CommandLine("verb a").CheckParms("s"));
			Assert.IsFalse(new CommandLine("verb a").CheckParms("i"));
			Assert.IsTrue(new CommandLine("verb 1").CheckParms("s"));
			Assert.IsTrue(new CommandLine("verb 1").CheckParms("i"));
			Assert.IsTrue(new CommandLine("verb a 2 2 a 3 b").CheckParms("siisis"));
		}
	}
}
