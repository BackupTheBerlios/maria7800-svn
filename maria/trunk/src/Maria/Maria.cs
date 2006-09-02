/*
 * Maria main program.
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
using System.Reflection;

namespace Maria {
	public class Maria {

		public static string Title {
			get {
				Assembly myAss = Assembly.GetExecutingAssembly();
				object[] obj = myAss.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				AssemblyTitleAttribute attr = (AssemblyTitleAttribute)obj[0];
				return attr.Title;
			}
		}

		public static string Version {
			get {
				Assembly myAss = Assembly.GetExecutingAssembly();
				return myAss.GetName().Version.ToString();
			}
		}

		public static string Copyright {
			get {
				Assembly myAss = Assembly.GetExecutingAssembly();
				object[] obj = myAss.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				AssemblyCopyrightAttribute attr = (AssemblyCopyrightAttribute)obj[0];
				return attr.Copyright;
			}
		}

		public static Version ClrVersion {
			get { return Environment.Version; }
		}

		public static OperatingSystem OSVersion {
			get { return Environment.OSVersion; }
		}

		public static int Main(string[] args) {
			try {
				// TODO : only the --version switch should output the nonsense below
				Console.WriteLine("{0} {1}", Title, Version);
				Console.WriteLine(Copyright);
				Console.WriteLine("CLR Version : {0}", ClrVersion);
				Console.WriteLine("OS Version : {0}", OSVersion);
				// TODO : parse command line
				// TODO : get global settings from somewhere (system-wide/user-specific)
				// TODO : get rom properties from somewhere (system-wide/user-specific)
				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine("Error: " + e.Message);
				return 1;
			}
		}
	}
}
