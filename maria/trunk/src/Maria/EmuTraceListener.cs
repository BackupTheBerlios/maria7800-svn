/*
 * TraceListener for the application
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
using System.Diagnostics;
using System.IO;

namespace Maria {
	public class EmuTraceListener : TraceListener {
		private readonly TextWriter output;

		public EmuTraceListener(TextWriter output) {
			this.output = output;
		}

		public override void Write(string message) {
			output.Write(message);
		}

		public override void WriteLine(string message) {
			Write(message);
			Write(Environment.NewLine);
		}
	}
}
