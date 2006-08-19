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
using System.Collections;
using System.IO;
using NUnit.Framework;

namespace Maria.Core {
	[TestFixture]
	public class MD5Test {		

		private byte[] ByteArray(string input) {
			ArrayList result = new ArrayList(input.Length);
			foreach (char c in input)
				result.Add((byte) c);
			return (byte[]) result.ToArray(typeof(byte));
		}

		private void CreateTestFile(string fileName, bool addHeader, string content) {
			StreamWriter writer = null;
			try {
				writer = File.CreateText(fileName);
				if (addHeader)
					writer.Write(new string('*', MD5.A78HeaderSize));
				writer.Write(content);
			}
			finally {
				if (writer != null)
					writer.Close();
			}
		}
		
		[Test]
		public void TestEmptyInput() {
			Assert.AreEqual(
				"d41d8cd98f00b204e9800998ecf8427e",
				MD5.ComputeMD5Digest(ByteArray(""))
			);
		}

		[Test]
		public void TestNonEmptyInput() {
			Assert.AreEqual(
				"0cc175b9c0f1b6a831c399e269772661",
				MD5.ComputeMD5Digest(ByteArray("a"))
			);
			Assert.AreEqual(
				"900150983cd24fb0d6963f7d28e17f72",
				MD5.ComputeMD5Digest(ByteArray("abc"))
			);
			Assert.AreEqual(
				"f96b697d7cb7938d525a2f31aaf161d0",
				MD5.ComputeMD5Digest(ByteArray("message digest"))
			);
			Assert.AreEqual(
				"c3fcd3d76192e4007dfb496cca67e13b",
				MD5.ComputeMD5Digest(ByteArray("abcdefghijklmnopqrstuvwxyz"))
			);
			Assert.AreEqual(
				"d174ab98d277d9f5a5611c2c9f419d9f",
				MD5.ComputeMD5Digest(ByteArray("ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
					"abcdefghijklmnopqrstuvwxyz0123456789"))
			);
			string s = "1234567890";
			Assert.AreEqual(
				"57edf4a22be3c955ac49da2e2107b67a",
				MD5.ComputeMD5Digest(ByteArray(s+s+s+s+s+s+s+s))
			);
		}

		[Test]
		public void TestUnheaderedFile() {
			CheckFile("", false, "d41d8cd98f00b204e9800998ecf8427e");
			CheckFile("abc", false, "900150983cd24fb0d6963f7d28e17f72");
		}

		[Test]
		public void TestHeaderedFile() {
			CheckFile("", true, "d41d8cd98f00b204e9800998ecf8427e");
			CheckFile("abc", true, "900150983cd24fb0d6963f7d28e17f72");
		}

		private void CheckFile(string fileContent, bool addHeader, string expectedMD5) {
			string fileName = addHeader ? "headeredfile.a78" : "unheaderedfile.bin";
			try {
				CreateTestFile(fileName, addHeader, fileContent);
				Assert.AreEqual(
					expectedMD5,
					MD5.ComputeMD5Digest(new FileInfo(fileName))
				);
			}
			finally {
				File.Delete(fileName);
			}
		}
	}
}
