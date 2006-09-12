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
using Mono.GetOptions;
using Maria.Core;

namespace Maria {

 	internal class MariaOptions : Options {
		public MariaOptions() {
			base.ParsingMode = OptionsParsingMode.GNU_DoubleDash;
		}
	}

	public class Maria {
		[STAThread]
		public static int Main(string[] args) {
			try {
				MariaOptions options = new MariaOptions();
				options.ProcessArgs(args);
				if (options.RemainingArguments.Length != 1) {
					throw new ApplicationException("You need to specify exactly one ROM image.");
				}
				// TODO : pull settings from system/user mariarc
				// TODO : pull rom info from system/user db
				// TODO : run the SDL host

				// TODO : what we do here is just a temporary hack:
				EMU7800App.Instance.LaunchFromCL(args);

				return 0;
			}
			catch (Exception e) {
				Console.Error.WriteLine("Error: " + e.Message);
				throw;
				return 1;
			}
		}
	}
}
