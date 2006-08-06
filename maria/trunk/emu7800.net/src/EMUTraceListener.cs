/*
 * EMUTraceListener.cs
 * 
 * TraceListener for the application
 * 
 * Copyright (c) 2003-2005 Mike Murphy
 * 
 */ 
using System;
using System.Diagnostics;
using System.Text;

namespace EMU7800 
{
	public class EMUTraceListener : TraceListener
	{
		StringBuilder MessageSpool = new StringBuilder();

		public override void Write(string message)
		{
			MessageSpool.AppendFormat(message);
		}

		public override void WriteLine(string message)
		{
			Write(message + "\n");
		}

		public string GetMsgs()
		{
			string msgs = MessageSpool.ToString().Replace("\n", Environment.NewLine);
			MessageSpool.Length = 0;
			return msgs;
		}

		public void Start()
		{
			if (!Trace.Listeners.Contains(this))
			{
				Trace.Listeners.Add(this);
			}
		}

		public static readonly EMUTraceListener Instance = new EMUTraceListener();
		private EMUTraceListener()
		{
			Start();
		}
	}
}
