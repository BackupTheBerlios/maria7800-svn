/*
 * Represents attribute data associated with ROMs
 * Copyright 2003, 2004 (c) Mike Murphy
 * Copyright (C) 2006 Thomas Mathys (tom42@users.berlios.de)
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
using System.IO;
using System.Text;

namespace Maria.Core {
	public class GameSettings {
		private string fMD5;
		private string fTitle;
		private string fManufacturer;
		private string fYear;
		private string fModelNo;
		private string fRarity;
		private CartType fCartType;
		private MachineType fMachineType;
		private Controller fLController;
		private Controller fRController;
		private string fHelpUri;
		private FileInfo fFileInfo;

		public string MD5 {
			get { return fMD5; }
			set { fMD5 = value; }
		}

		public string Title {
			get { return fTitle; }
			set { fTitle = value; }
		}

		public string Manufacturer {
			get { return fManufacturer; }
			set { fManufacturer = value; }
		}

		public string Year {
			get { return fYear; }
			set { fYear = value; }
		}

		public string ModelNo {
			get { return fModelNo; }
			set { fModelNo = value; }
		}

		public string Rarity {
			get { return fRarity; }
			set { fRarity = value; }
		}

		public CartType CartType {
			get { return fCartType; }
			set { fCartType = value; }
		}

		public MachineType MachineType {
			get { return fMachineType; }
			set { fMachineType = value; }
		}

		public Controller LController {
			get { return fLController; }
			set { fLController = value; }
		}

		public Controller RController {
			get { return fRController; }
			set { fRController = value; }
		}

		public string HelpUri {
			get { return fHelpUri; }
			set { fHelpUri = value; }
		}

		public FileInfo FileInfo {
			get { return fFileInfo; }
			set { fFileInfo = value; }
		}

		// TODO : Maria.Core.MD5 contains same code !
		// actually there should probably be some kind of ROMImage class
		// that knows how to load rom images and perhaps create Cart objects
		// from them.
		// TODO : uncomment. It's needed in some other piece of messy code.
//		public int Offset {
	//		get { return (FileInfo.Extension.ToLower() == ".a78") ? 128 : 0; }
		//}
	}
}

/*
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

*/

