/*
 * Convenient interface to MD5 hash function.
 * Copyright (C) 2004 Mike Murphy
 * Copyright (C) 2006 Thomas Mathys (tom42@sourceforge.net)
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
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Maria.Core {

	public sealed class MD5 {
		private MD5() {}

		public const int A78HeaderSize = 128;

		public static string ComputeMD5Digest(byte[] bytes) {
			return StringifyMD5(new MD5CryptoServiceProvider().ComputeHash(bytes));
		}

		public static string ComputeMD5Digest(FileInfo fileInfo) {
			FileStream stream = null;
			try {
				stream = File.OpenRead(fileInfo.FullName);
				if (fileInfo.Extension.ToLower() == ".a78")
					stream.Seek(A78HeaderSize, SeekOrigin.Begin);
				return StringifyMD5(new MD5CryptoServiceProvider().ComputeHash(stream));
			}
			finally {
				if (stream != null)
					stream.Close();
			}
		}

		private static string StringifyMD5(byte[] bytes) {
			if (bytes.Length != 16) {
				throw new InternalErrorException(
					String.Format(
						CultureInfo.InvariantCulture,
						"Wrong MD5 checksum size : {0} bytes",
						bytes.Length
					)
				);
			}
			StringBuilder result = new StringBuilder();
			foreach (byte b in bytes)
				result.AppendFormat("{0:x2}", b);
			return result.ToString();
		}
	}
}
