/*
 * HSC7800.cs
 * 
 * The 7800 High Score cartridge--courtesy of Matthias <matthias@atari8bit.de>.
 * 
 */
using System;
using System.Diagnostics;
using System.IO;

namespace EMU7800
{
	[Serializable]
	public sealed class HSC7800 : IDevice
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

		public void Reset()
		{
			LoadRAM();
		}

		public void Map(AddressSpace mem) { }

		public byte this[ushort addr]
		{
			get
			{
				return ROM[addr & Mask];
			}
			set { }
		}

		RAM6116 _SRAM = new RAM6116();
		public RAM6116 SRAM
		{
			get
			{
				return _SRAM;
			}
		}

		string BackingStoreFullName
		{
			get
			{
				return Path.Combine(EMU7800App.Instance.Settings.OutputDirectory, "scores.hsc");
			}
		}

		public void SaveSRAM()
		{
			Trace.Write("Saving Highscore Cartridge RAM: ");
			Trace.Write(BackingStoreFullName);
			Trace.Write("... ");
			BinaryWriter bw = null;
			try
			{
				bw = new BinaryWriter(File.Create(BackingStoreFullName));
				for (ushort addr = 0; addr < 0x0800; addr++)
				{
					bw.Write(SRAM[addr]);
				}
				bw.Flush();
				Trace.WriteLine("ok");
			}
			catch (Exception ex)
			{
				Trace.WriteLine("error");
				Trace.WriteLine(ex.Message);
			}
			finally
			{
				if (bw != null)
				{
					bw.Close();
				}
			}
		}

		public HSC7800()
		{
			LoadROM("c8a73288ab97226c52602204ab894286");
		}

		void LoadROM(string md5)
		{
			ROM = null;
			DirectoryInfo di = new DirectoryInfo(EMU7800App.Instance.Settings.ROMDirectory);
			string foundFullName = "";

			Trace.Write("Loading Highscore Cartridge ROM: ");
			Trace.Write("... ");

			foreach (FileInfo fi in di.GetFiles())
			{
				if (fi.Length == 4096)
				{
					BinaryReader br = null;
					try
					{
						br = new BinaryReader(File.OpenRead(fi.FullName));
						ROM = br.ReadBytes((int)fi.Length);
					}
					catch
					{
						ROM = null;
					}
					finally
					{
						if (br != null)
						{
							br.Close();
						}
					}
					if (md5 != MD5.ComputeMD5Digest(ROM))
					{
						ROM = null;
					}
					else if (ROM != null)
					{
						foundFullName = fi.FullName;
						break;
					}
				}
			}

			if (ROM == null)
			{
				throw new Exception("7800 Highscore BIOS not found in ROM directory: " + di.FullName);
			}
			else
			{
				Trace.WriteLine("ok, found " + di.FullName);
				Size = Mask = (ushort)ROM.Length;
				Mask--;
			}
		}

		void LoadRAM()
		{
			Trace.Write("Loading Highscore Cartridge RAM: ");
			Trace.Write(BackingStoreFullName);
			Trace.Write("... ");
			BinaryReader br = null;
			byte[] data = null;
			try
			{
				br = new BinaryReader(File.OpenRead(BackingStoreFullName));
				data = br.ReadBytes(0x0800);
				Trace.WriteLine("ok");
			}
			catch (Exception ex)
			{
				data = new byte[0x0800];
				Trace.WriteLine("error");
				Trace.WriteLine(ex.Message);
			}
			finally
			{
				if (br != null)
				{
					br.Close();
				}
			}
			for (ushort addr = 0; data != null && addr < data.Length; addr++)
			{
				SRAM[addr] = data[addr];
			}
		}
	}
}