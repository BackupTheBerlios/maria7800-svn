/*
 * CommandLine.cs
 *
 * Supporting class for executable command lines.
 *
 * Copyright (c) 2003 Mike Murphy
 *
 */
using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace EMU7800
{
	public class CommandLine
	{
		internal struct Parameter
		{
			public string StrValue;
			public int IntValue;
			public bool IsInteger;
		}

		string _Verb;
		internal string Verb
		{
			get
			{
				return _Verb;
			}
		}

		Parameter[] _Parms;
		internal Parameter[] Parms
		{
			get
			{
				return _Parms;
			}
		}

		internal bool CheckParms(string chkstr)
		{
			if (chkstr == null)
			{
				throw new ArgumentNullException("chkstr");
			}

			bool retval = true;

			if (chkstr.Length != Parms.Length)
			{
				retval = false;
			}
			else
			{
				for (int i = 0; i < chkstr.Length; i++)
				{
					if (chkstr.Substring(i, 1) == "i" && !Parms[i].IsInteger)
					{
						retval = false;
						break;
					}
				}
			}
			return retval;
		}

		public CommandLine(string commandLine)
		{
			if (commandLine == null)
			{
				throw new ArgumentNullException("commandLine");
			}

			string[] toks = commandLine.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			if (toks.Length > 0)
			{
				_Verb = toks[0];
			}

			ArrayList al = new ArrayList();

			for (int i = 1; i < toks.Length; i++)
			{
				string tok = toks[i];

				Parameter p = new Parameter();

				p.StrValue = tok;
				p.IsInteger = false;

				if (tok.Substring(0, 1) == "$")
				{
					try
					{
						p.IntValue = Int32.Parse(tok.Substring(1), NumberStyles.HexNumber);
						p.IsInteger = true;
					}
					catch (Exception) { }
				}
				else if (tok.Length >= 2 && tok.Substring(0, 2) == "0x")
				{
					try
					{
						p.IntValue = Int32.Parse(tok.Substring(2), NumberStyles.HexNumber);
						p.IsInteger = true;
					}
					catch (Exception) { }
				}
				else
				{
					try
					{
						p.IntValue = Int32.Parse(tok, NumberStyles.Number);
						p.IsInteger = true;
					}
					catch (Exception) { }
				}

				al.Add(p);
			}

			_Parms = new Parameter[al.Count];
			object[] objs = al.ToArray();
			for (int i = 0; i < al.Count; i++)
			{
				_Parms[i] = (Parameter)objs[i];
			}
		}
	}
}
