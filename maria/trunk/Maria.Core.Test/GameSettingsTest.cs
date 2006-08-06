/*
 * This file is part of Maria.
 * Copyright (C) 2006 Thomas Mathys (tom42@sourceforge.net)
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
using System.IO;
using NUnit.Framework;

namespace Maria.Core {
	[TestFixture]
	public class GameSettingsTest {
		private GameSettings gs;

		[SetUp]
		public void SetUp() {
			gs = new GameSettings();
		}

		[Test]
		public void TestMD5() {
			Assert.IsNull(gs.MD5);
			gs.MD5 = "foo";
			Assert.AreEqual("foo", gs.MD5);
		}

		[Test]
		public void TestTitle() {
			Assert.IsNull(gs.Title);
			gs.Title = "Ninja Golf";
			Assert.AreEqual("Ninja Golf", gs.Title);
		}

		[Test]
		public void TestManufacturer() {
			Assert.IsNull(gs.Manufacturer);
			gs.Manufacturer = "ACME";
			Assert.AreEqual("ACME", gs.Manufacturer);
		}

		[Test]
		public void TestYear() {
			Assert.IsNull(gs.Year);
			gs.Year = "2006";
			Assert.AreEqual("2006", gs.Year);
		}

		[Test]
		public void TestModelNo() {
			Assert.IsNull(gs.ModelNo);
			gs.ModelNo = "2342";
			Assert.AreEqual("2342", gs.ModelNo);
		}

		[Test]
		public void TestRarity() {
			Assert.IsNull(gs.Rarity);
			gs.Rarity = "Incredibly rare yet fairly common.";
			Assert.AreEqual("Incredibly rare yet fairly common.", gs.Rarity);
		}

		[Test]
		public void TestCartType() {
			Assert.AreEqual(CartType.Default, gs.CartType);
			gs.CartType = CartType.DPC;
			Assert.AreEqual(CartType.DPC, gs.CartType); 
		}

		[Test]
		public void TestMachineType() {
			Assert.AreEqual(MachineType.A2600NTSC, gs.MachineType);
			gs.MachineType = MachineType.A7800PAL;
			Assert.AreEqual(MachineType.A7800PAL, gs.MachineType);
		}

		[Test]
		public void TestLController() {
			Assert.AreEqual(Controller.None, gs.LController);
			gs.LController = Controller.Joystick;
			Assert.AreEqual(Controller.Joystick, gs.LController);
		}

		[Test]
		public void TestRController() {
			Assert.AreEqual(Controller.None, gs.RController);
			gs.RController = Controller.Driving;
			Assert.AreEqual(Controller.Driving, gs.RController);
		}

		[Test]
		public void TestHelpUri() {
			Assert.IsNull(gs.HelpUri);
			gs.HelpUri = "/dev/null";
			Assert.AreEqual("/dev/null", gs.HelpUri);
		}

		public void TestFileInfo() {
			FileInfo fi = new FileInfo("/foo/bar");
			Assert.IsNull(gs.FileInfo);
			gs.FileInfo = fi;
			Assert.AreEqual(fi, gs.FileInfo);
		}
	}
}
