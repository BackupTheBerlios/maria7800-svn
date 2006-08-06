/*
 * GameSettings.cs
 * 
 * Represents attribute data associated with ROMs
 * 
 * Copyright 2003, 2004 (c) Mike Murphy
 * 
 */
using System;
using System.IO;
using System.Text;

namespace EMU7800
{
	public class GameSettings
	{
		public string MD5;
		public string Title, Manufacturer, Year, ModelNo, Rarity;
		public CartType CartType;
		public MachineType MachineType;
		public Controller LController, RController;
		public string HelpUri;

		public FileInfo FileInfo;

		public int Offset
		{
			get
			{
				return (FileInfo.Extension.ToLower() == ".a78") ? 128 : 0;
			}
		}

		public override string ToString()
		{
			StringBuilder s = new StringBuilder("GameSettings:\n");

			s.AppendFormat(" MD5: {0}\n", MD5);
			s.AppendFormat(" Title: {0}\n", Title);
			s.AppendFormat(" Manufacturer: {0}\n", Manufacturer);
			s.AppendFormat(" Year: {0}\n", Year);
			s.AppendFormat(" ModelNo: {0}\n", ModelNo);
			s.AppendFormat(" Rarity: {0}\n", Rarity);
			s.AppendFormat(" CartType: {0}\n", CartType);
			s.AppendFormat(" MachineType: {0}\n", MachineType);
			s.AppendFormat(" LController: {0}\n", LController);
			s.AppendFormat(" RController: {0}\n", RController);

			s.AppendFormat(" FileName: {0}", FileInfo != null ? FileInfo.FullName : "<null>");
			if (FileInfo != null)
			{
				s.AppendFormat("\n Size: {0}", FileInfo.Length);
			}
			s.AppendFormat("\n HelpUri: {0}", HelpUri);

			return s.ToString();
		}

		public GameSettings(string md5)
		{
			MD5 = md5;
		}
	}
}