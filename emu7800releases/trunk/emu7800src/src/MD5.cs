/*
 * MD5.cs
 * 
 * Convenient interface to MD5 hash function.
 * 
 * Copyright 2004 (c) Mike Murphy
 * 
 */
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EMU7800
{
	public sealed class MD5
	{
		static MD5CryptoServiceProvider md5CSP = new MD5CryptoServiceProvider();
		static StringBuilder md5SB = new StringBuilder();
		static byte[] buffer = new byte[144 * 1024];
		static int offset, count;

		public static string ComputeMD5Digest(byte[] bytes)
		{
			return StringifyMD5(md5CSP.ComputeHash(bytes));
		}

		public static string ComputeMD5Digest(FileInfo fi)
		{
			offset = (fi.Extension.ToLower() == ".a78") ? 128 : 0;
			count = (int)fi.Length - offset;
			BinaryReader r = null;
			string md5 = "";
			try 
			{
				r = new BinaryReader(File.OpenRead(fi.FullName));
				r.BaseStream.Seek(offset, SeekOrigin.Begin);
				r.Read(buffer, 0, count);
				md5 = StringifyMD5(md5CSP.ComputeHash(buffer, 0, count));
			} 
			catch (Exception ex)
			{
				Trace.Write("Error in ComputeMD5Digest(");
				Trace.Write(fi.FullName);
				Trace.WriteLine("):");
				Trace.WriteLine(ex);
			} 
			finally
			{
				if (r != null)
				{
					r.Close();
				}
			}
			return md5;
		}

		static string StringifyMD5(byte[] bytes)
		{
			md5SB.Length = 0;
			if (bytes != null && bytes.Length >= 16)
			{
				for (int i = 0; i < 16; i++)
				{
					md5SB.AppendFormat("{0:x2}", bytes[i]);
				}
			}
			return md5SB.ToString();
		}
	}
}