/*
 * CommandLine.cs
 *
 * Supporting class for executable command lines.
 *
 * Copyright (C) 2003 Mike Murphy
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
using System.Collections;
using System.Globalization;
using System.Text;
using Vtg.Util;

namespace Maria.Core {
	public class CommandLine {
		private string verb;
		private Parameter[] parms;

		public CommandLine(string commandLine) {
			ArgumentCheck.NotNull(commandLine, "commandLine");
			string[] toks0 = commandLine.Split(new char[] {' '});
			ArrayList toks1 = new ArrayList();
			foreach (string s in toks0) {
				if (s.Length > 0)
					toks1.Add(s);
			}
			string[] toks = (string[]) toks1.ToArray(typeof(string));
			if (toks.Length > 0) {
				verb = toks[0];
			}
			ArrayList al = new ArrayList();
			for (int i = 1; i < toks.Length; i++) {
				string tok = toks[i];
				Parameter p = new Parameter();
				p.StrValue = tok;
				p.IsInteger = false;
				if (tok.Substring(0, 1) == "$") {
					try {
						p.IntValue = Int32.Parse(tok.Substring(1), NumberStyles.HexNumber);
						p.IsInteger = true;
					}
					catch (Exception) { }
				}
				else if (tok.Length >= 2 && tok.Substring(0, 2) == "0x") {
					try {
						p.IntValue = Int32.Parse(tok.Substring(2), NumberStyles.HexNumber);
						p.IsInteger = true;
					}
					catch (Exception) { }
				}
				else {
					try {
						p.IntValue = Int32.Parse(tok, NumberStyles.Number);
						p.IsInteger = true;
					}
					catch (Exception) { }
				}
				al.Add(p);
			}
			parms = new Parameter[al.Count];
			object[] objs = al.ToArray();
			for (int i = 0; i < al.Count; i++) {
				parms[i] = (Parameter) objs[i];
			}
		}

		public string Verb {
			get { return verb; }
		}

		public Parameter[] Parms {
			get { return parms; }
		}

		public bool CheckParms(string chkstr) {
			ArgumentCheck.NotNull(chkstr, "chkstr");
			bool retval = true;
			if (chkstr.Length != Parms.Length) {
				retval = false;
			}
			else {
				for (int i = 0; i < chkstr.Length; i++) {
					if (chkstr.Substring(i, 1) == "i" && !Parms[i].IsInteger) {
						retval = false;
						break;
					}
				}
			}
			return retval;
		}

		// TODO : make fields readonly, initialization goes via ctor. methinks.
		public struct Parameter {
			public string StrValue;
			public int IntValue;
			public bool IsInteger;
		}
	}
}
