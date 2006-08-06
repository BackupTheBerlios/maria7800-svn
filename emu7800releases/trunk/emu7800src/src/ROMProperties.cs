/*
 * ROMProperties.cs
 * 
 * Represents the ROMProperties.csv file contents
 * 
 * Copyright 2004-2006 (c) Mike Murphy
 * 
 */
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;

namespace EMU7800
{
	public class ROMProperties
	{
		const string ROM_PROPERTIES_FN = "ROMProperties.csv";
		Hashtable PropertyTable = null;

		public GameSettings GetGameSettings(string md5)
		{
			if (md5 == null)
			{
				throw new ArgumentNullException("md5");
			}
			if (PropertyTable == null)
			{
				Load();
			}
			if (!PropertyTable.ContainsKey(md5))
			{
				PropertyTable[md5] = new GameSettings(md5);
			}
			return (GameSettings)PropertyTable[md5];
		}

		public GameSettings GetGameSettingsFromFile(FileInfo fi)
		{
            GameSettings gs = null;

            if (fi == null)
            {
                throw new ArgumentNullException("fi");
            }

			if (PropertyTable == null)
			{
				Load();
			}

            string ext = Path.GetExtension(fi.FullName).ToLower();

            if (!ext.Equals(".exe") && !ext.Equals(".dll") && !ext.Equals(".csv"))
            {
                string md5 = MD5.ComputeMD5Digest(fi);

                if (PropertyTable.ContainsKey(md5))
                {
                    gs = (GameSettings)PropertyTable[md5];
                    gs.FileInfo = fi;
                }
                else
                {
                    if (md5 == "c8a73288ab97226c52602204ab894286")
                    {
                        Trace.Write("Found 7800 Highscore Cart:");
                    }
                    else if (md5 == "0763f1ffb006ddbe32e52d497ee848ae")
                    {
                        Trace.Write("Found 7800 NTSC BIOS:");
                    }
                    else if (md5 == "b32526ea179dc9ab9b2e5f8a2662b298")
                    {
                        Trace.Write("Found incorrect but widely used 7800 NTSC BIOS:");
                    }
                    else if (md5 == "397bb566584be7b9764e7a68974c4263")
                    {
                        Trace.Write("Found 7800 PAL BIOS:");
                    }
                    else
                    {
                        Trace.Write("Unrecognized file:");
                    }
                    Trace.Write(" ");
                    Trace.Write(fi.FullName);
                    Trace.Write(" ");
                    Trace.Write("MD5=");
                    Trace.WriteLine(md5);
                }
            }
 
			return gs;
		}

		public void Load()
		{
			string fn = Path.Combine(EMU7800App.Instance.Settings.RootDirectory, ROM_PROPERTIES_FN);

			Trace.Write("Loading ");
			Trace.Write(fn);
			Trace.Write("... ");

			PropertyTable = new Hashtable();
			StreamReader r = null;
			try
			{
				r = new StreamReader(fn);
			}
			catch (Exception ex)
			{
				if (r != null)
				{
					r.Close();
				}
				Trace.WriteLine("error");
				Trace.WriteLine(ex.Message);
				return;
			}

			string line = r.ReadLine();

			Hashtable Column = new Hashtable();
			int colno = 0;
			int colMD5 = 0;
			int colTitle = 0;
			int colManufacturer = 0;
			int colYear = 0;
			int colModelNo = 0;
			int colRarity = 0;
			int colCartType = 0;
			int colMachineType = 0;
			int colLController = 0;
			int colRController = 0;
			int colHelpUri = 0;
			foreach (string colnm in line.Split(','))
			{
				switch (colnm)
				{
					case "MD5": colMD5 = colno; break;
					case "Title": colTitle = colno; break;
					case "Manufacturer": colManufacturer = colno; break;
					case "Year": colYear = colno; break;
					case "ModelNo": colModelNo = colno; break;
					case "Rarity": colRarity = colno; break;
					case "CartType": colCartType = colno; break;
					case "MachineType": colMachineType = colno; break;
					case "LController": colLController = colno; break;
					case "RController": colRController = colno; break;
					case "HelpUri": colHelpUri = colno; break;
				}
				colno++;
			}

			if (colno != 11)
			{
				Trace.WriteLine("bad ROMProperties.csv file: not all columns present");
				return;
			}

			string md5;

			while (true)
			{
				line = r.ReadLine();
				if (line == null)
				{
					break;
				}

				string[] row = line.Split(',');
				md5 = row[colMD5];

				GameSettings gs = new GameSettings(md5);
				gs.Title = row[colTitle];
				gs.Manufacturer = row[colManufacturer];
				gs.Year = row[colYear];
				gs.ModelNo = row[colModelNo];
				gs.Rarity = row[colRarity];

				gs.CartType = EnumCartType(row[colCartType], ref md5);
				gs.MachineType = EnumMachineType(row[colMachineType], ref md5);

				Controller dflt;
				switch (gs.MachineType)
				{
					default:
					case MachineType.A2600NTSC:
					case MachineType.A2600PAL:
						dflt = Controller.Joystick;
						break;
					case MachineType.A7800NTSC:
					case MachineType.A7800PAL:
						dflt = Controller.ProLineJoystick;
						break;
				}
				gs.LController = EnumController(row[colLController], dflt, ref md5);
				gs.RController = EnumController(row[colRController], dflt, ref md5);

				if (colHelpUri < row.Length && row[colHelpUri].Trim().Length > 0)
				{
					gs.HelpUri = row[colHelpUri].Trim();
				}

				gs.FileInfo = null;

				PropertyTable[md5] = gs;
			}
			r.Close();
			Trace.Write(PropertyTable.Count);
			Trace.Write(" ");
			Trace.Write(ROM_PROPERTIES_FN);
			Trace.WriteLine(" entries loaded");
		}

		public void Dump()
		{
			if (PropertyTable == null)
			{
				return;
			}

			string fn = Path.Combine(EMU7800App.Instance.Settings.OutputDirectory, ROM_PROPERTIES_FN + ".txt");
			StreamWriter w;

			try
			{
				w = new StreamWriter(fn);
			}
			catch (Exception e)
			{
				Trace.WriteLine(e.Message);
				return;
			}

			w.WriteLine("Title,Year,Manufacturer,ModelNo,CartType,MachineType,Rarity,LController,RController,MD5");
			IDictionaryEnumerator iter = PropertyTable.GetEnumerator();
			int count = 0;
			for (iter.Reset(); iter.MoveNext(); count++)
			{
				GameSettings gs = (GameSettings)iter.Value;
				w.WriteLine("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
					gs.Title,
					gs.Year,
					gs.Manufacturer,
					gs.ModelNo,
					gs.CartType.ToString() == "Default" ? "" : gs.CartType.ToString(),
					gs.MachineType.ToString(),
					gs.Rarity,
					gs.LController.ToString(),
					gs.RController.ToString(),
					gs.MD5);
			}
			w.Flush();
			w.Close();
			Trace.WriteLine(String.Format("Dumped {0} {1}.txt entries", ROM_PROPERTIES_FN, count));
		}

		public ROMProperties() { }

		CartType EnumCartType(string cartType, ref string md5)
		{
			CartType ct;

			if (cartType.Length > 0)
			{
				try
				{
					ct = (CartType)Enum.Parse(typeof(CartType), cartType.ToUpper());
				}
				catch (ArgumentException)
				{
					ct = CartType.Default;
					Trace.WriteLine("");
					Trace.Write("bad CartType (");
					Trace.Write(cartType);
					Trace.Write(") for md5=");
					Trace.Write(md5);
					Trace.Write(" in ");
					Trace.WriteLine(ROM_PROPERTIES_FN);
				}
			}
			else
			{
				ct = CartType.Default;
			}
			return ct;
		}

		MachineType EnumMachineType(string machineType, ref string md5)
		{
			MachineType mt;

			if (machineType.Length > 0)
			{
				try
				{
					mt = (MachineType)Enum.Parse(typeof(MachineType), machineType.ToUpper());
				}
				catch (ArgumentException)
				{
					mt = MachineType.A2600NTSC;
					Trace.WriteLine("");
					Trace.Write("bad MachineType (");
					Trace.Write(machineType);
					Trace.Write(") for md5=");
					Trace.Write(md5);
					Trace.Write(" in ");
					Trace.WriteLine(ROM_PROPERTIES_FN);
				}
			}
			else
			{
				mt = MachineType.A2600NTSC;
			}
			return mt;
		}

		Controller EnumController(string controller, Controller dflt, ref string md5)
		{
			Controller c;

			if (controller.Length > 0)
			{
				try
				{
					c = (Controller)Enum.Parse(typeof(Controller), controller);
				}
				catch (ArgumentException)
				{
					c = dflt;
					Trace.WriteLine("");
					Trace.Write("bad Controller (");
					Trace.Write(controller);
					Trace.Write(") for md5=");
					Trace.Write(md5);
					Trace.Write(" in ");
					Trace.WriteLine(ROM_PROPERTIES_FN);
				}
			}
			else
			{
				c = dflt;
			}
			return c;
		}
	}
}