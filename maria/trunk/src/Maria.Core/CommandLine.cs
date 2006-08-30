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
		private readonly string verb;
		private readonly Parameter[] parms;

		public CommandLine(string commandLine) {
			ArgumentCheck.NotNull(commandLine, "commandLine");
			ArrayList toks = Tokenize(commandLine);
			if (toks.Count > 0) {
				verb = (string) toks[0];
			}
			ArrayList al = new ArrayList();
			for (int i = 1; i < toks.Count; i++) {
				Parameter p = new Parameter((string) toks[i]);
				al.Add(p);
			}
			parms = (Parameter[]) al.ToArray(typeof(Parameter));
		}

		public string Verb {
			get { return verb; }
		}

		public Parameter[] Parms {
			get { return parms; }
		}

		public bool CheckParms(string chkstr) {
			ArgumentCheck.NotNull(chkstr, "chkstr");
			if (chkstr.Length != Parms.Length)
				return false;
			for (int i = 0; i < chkstr.Length; i++) {
				if (chkstr.Substring(i, 1) == "i" && !Parms[i].IsInteger)
					return false;
			}
			return true;
		}

		private ArrayList Tokenize(string commandLine) {
			string[] tokens = commandLine.Split(new char[] {' '});
			ArrayList result = new ArrayList();
			foreach (string s in tokens) {
				if (s.Length > 0)
					result.Add(s);
			}
			return result;
		}

		public struct Parameter {
			public readonly string StrValue;
			public readonly int IntValue;
			public readonly bool IsInteger;

			public Parameter(string tok) {
				StrValue = tok;
				IntValue = 0;
				IsInteger = false;
				if (tok.Substring(0, 1) == "$") {
					try {
						IntValue = Int32.Parse(tok.Substring(1), NumberStyles.HexNumber);
						IsInteger = true;
					}
					catch (Exception) { }
				}
				else if (tok.Length >= 2 && tok.Substring(0, 2) == "0x") {
					try {
						IntValue = Int32.Parse(tok.Substring(2), NumberStyles.HexNumber);
						IsInteger = true;
					}
					catch (Exception) { }
				}
				else {
					try {
						IntValue = Int32.Parse(tok, NumberStyles.Number);
						IsInteger = true;
					}
					catch (Exception) { }
				}
			}
		}
	}
}
