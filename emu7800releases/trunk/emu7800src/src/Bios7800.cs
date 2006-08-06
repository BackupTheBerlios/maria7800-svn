/*
 * BIOS7800.cs
 * 
 * The BIOS of the Atari 7800.
 * 
 * Copyright (c) 2004 Mike Murphy
 * 
 */
using System;
using System.Diagnostics;
using System.IO;

namespace EMU7800
{
	[Serializable]
	public sealed class Bios7800 : IDevice
	{
		byte[] ROM;
		ushort Mask;

		ushort _Size;
		public ushort Size
		{
			get
			{
				return _Size;
			}
			set
			{
				_Size = value;
			}
		}

		public void Reset() { }
		public void Map(AddressSpace adrspc) { }

		public byte this[ushort addr]
		{
			get
			{
				return ROM[addr & Mask];
			}
			set { }
		}

		public Bios7800(MachineType machineType)
		{
			switch (machineType)
			{
				case MachineType.A7800NTSC:
					if (LoadBios("0763f1ffb006ddbe32e52d497ee848ae"))
					{
					}
					else if (LoadBios("b32526ea179dc9ab9b2e5f8a2662b298"))
					{
						Trace.WriteLine("WARNING: Using incorrect, but widely used, 7800 NTSC BIOS");
					}
					else
					{
						throw new Exception("7800 NTSC BIOS not found in ROMDirectory: " + EMU7800App.Instance.Settings.ROMDirectory);
					}
					break;
				case MachineType.A7800PAL:
					LoadBios("397bb566584be7b9764e7a68974c4263");
					break;
				default:
					throw new Exception("Invalid MachineType");
			}
		}

		bool LoadBios(string md5)
		{
			ROM = null;
			DirectoryInfo di = new DirectoryInfo(EMU7800App.Instance.Settings.ROMDirectory);
			string foundFullName = "";

			foreach (FileInfo fi in di.GetFiles())
			{
				if (fi.Length != 4096 && fi.Length != 16384)
				{
					continue;
				}
				BinaryReader r = null;
				try
				{
					r = new BinaryReader(File.OpenRead(fi.FullName));
					ROM = r.ReadBytes((int)fi.Length);
					if (md5 != MD5.ComputeMD5Digest(ROM))
					{
						ROM = null;
					}
					else
					{
						foundFullName = fi.FullName;
					}
				}
				catch (Exception)
				{
				}
				finally
				{
					if (r != null)
					{
						r.Close();
					}
				}
				if (ROM != null)
				{
					break;
				}
			}

			if (ROM != null)
			{
				Size = Mask = (ushort)ROM.Length;
				Mask--;
				Trace.Write("Found 7800 BIOS: ");
				Trace.WriteLine(foundFullName);
			}

			return ROM != null;
		}
	}
}