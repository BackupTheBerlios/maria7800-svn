/*
 * Machine.cs
 * 
 * Abstraction of an emulated machine.
 * 
 * Copyright (c) 2003, 2004 Mike Murphy
 * 
 */
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace EMU7800
{
	[Serializable]
	public abstract class Machine
	{
		protected M6502 _CPU;
		protected AddressSpace Mem;
		protected PIA PIA;

		[NonSerialized]
		Host _H;
		public Host H
		{
			get
			{
				return _H;
			}
			set
			{
				_H = value;
			}
		}

		bool _MachineHalt;
		public bool MachineHalt
		{
			get
			{
				return _MachineHalt;
			}
			set
			{
				if (!_MachineHalt)
				{
					_MachineHalt = true;
				}
			}
		}

		InputAdapter _InputAdapter;
		public InputAdapter InputAdapter
		{
			get
			{
				return _InputAdapter;
			}
		}

		public virtual M6502 CPU
		{
			get
			{
				return null;
			}
		}

		// Current frame number
		long _FrameNumber;
		public long FrameNumber
		{
			get 
			{
				return _FrameNumber;
			}
		}

		// Number of scanlines on the display
		int _Scanlines;
		public int Scanlines
		{
			get 
			{
				return _Scanlines;
			}
		}

		// Default starting scanline
		int _FirstScanline;
		public int FirstScanline
		{
			get 
			{
				return _FirstScanline;
			}
		}

		// Frame rate
		int _FrameHZ;
		public int FrameHZ
		{
			get 
			{
				if (_FrameHZ < 1)
					_FrameHZ = 1;
				return _FrameHZ;
			}
			set 
			{
				_FrameHZ = value;
				if (_FrameHZ < 1)
					_FrameHZ = 1;
			}
		}

		// ...in units of samples/sec
		int _SoundSampleRate;
		public int SoundSampleRate
		{
			get 
			{
				return _SoundSampleRate;
			}
		}

		int[] _Palette;
		public int[] Palette
		{
			get 
			{
				return _Palette;
			}
		}

		int _VisiblePitch;
		public int VisiblePitch
		{
			get 
			{
				return _VisiblePitch;
			}
		}

		public static Machine New(GameSettings gs, Cart c, InputAdapter ia)
		{
			if (gs == null)
			{
				throw new ArgumentNullException("gs");
			}
			if (c == null)
			{
				throw new ArgumentNullException("c");
			}
			if (ia == null)
			{
				throw new ArgumentNullException("ia");
			}
		
			Machine m;

			switch (gs.MachineType)
			{
				case MachineType.A2600NTSC:
					m = new Machine2600NTSC(c, ia);
					break;
				case MachineType.A2600PAL:
					m = new Machine2600PAL(c, ia);
					break;
				case MachineType.A7800NTSC:
					m = new Machine7800NTSC(c, ia);
					break;
				case MachineType.A7800PAL:
					m = new Machine7800PAL(c, ia);
					break;
				default:
					throw new Exception("Unexpected machine type: " + gs.MachineType.ToString());
			}

			m.InputAdapter.Controllers[0] = gs.LController;
			m.InputAdapter.Controllers[1] = gs.RController;

			return m;
		}

		public void Serialize(string fn)
		{
			FileStream s = null;
			try
			{
				s = new FileStream(fn, FileMode.Create);
				BinaryFormatter f = new BinaryFormatter();
				f.Serialize(s, this);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (s != null)
				{
					s.Close();
				}
			}
		}

		public static Machine Deserialize(string fn)
		{
			FileStream s = null;
			Machine m = null;
			try
			{
				s = new FileStream(fn, FileMode.Open);
				BinaryFormatter f = new BinaryFormatter();
				m = (Machine)f.Deserialize(s);
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				if (s != null)
				{
					s.Close();
				}
			}
			return m;
		}

		public void Reset()
		{
			_FrameNumber = 0;
			_MachineHalt = false;
			InputAdapter.Reset();
			DoReset();
			Trace.Write("Machine ");
			Trace.Write(this);
			Trace.Write(" reset complete (");
			Trace.Write(FrameHZ);
			Trace.Write(" HZ  ");
			Trace.Write(Scanlines);
			Trace.WriteLine(" scanlines)");
		}

		public void Run()
		{
			if (!MachineHalt)
			{
				InputAdapter.CheckPoint(_FrameNumber);
				DoRun();
				_FrameNumber++;
			}
		}

		// Called when the Machine instance will no longer be utilized.
		public void Done()
		{
			DoDone();
		}

		protected abstract void DoReset();
		protected abstract void DoRun();
		protected abstract void DoDone();

		public bool ExecuteCommandLine(CommandLine cl) 
		{
			switch (cl.Verb) 
			{
				case "d":
					if (cl.CheckParms("ii")) 
					{
						Trace.WriteLine(M6502DASM.Disassemble(Mem,
							(ushort)cl.Parms[0].IntValue,
							(ushort)cl.Parms[1].IntValue));
					} 
					else 
					{
						Trace.WriteLine("bad parms");
					}
					break;
				case "m":
					if (cl.CheckParms("ii")) 
					{
						Trace.WriteLine(M6502DASM.MemDump(Mem,
							(ushort)cl.Parms[0].IntValue,
							(ushort)cl.Parms[1].IntValue));
					} 
					else if (cl.CheckParms("i")) 
					{
						Trace.WriteLine(M6502DASM.MemDump(Mem,
							(ushort)cl.Parms[0].IntValue,
							(ushort)cl.Parms[0].IntValue));
					} 
					else 
					{
						Trace.WriteLine("bad parms");
					}
					break;
				case "poke":
					if (cl.CheckParms("ii")) 
					{
						Mem[(ushort)cl.Parms[0].IntValue] = (byte)cl.Parms[1].IntValue;
						Trace.WriteLine(String.Format("poke #${0:x2} at ${1:x4} complete",
							cl.Parms[0].IntValue,
							cl.Parms[1].IntValue));
						Mem[(ushort)cl.Parms[1].IntValue] = (byte)cl.Parms[0].IntValue;
					} 
					else 
					{
						Trace.WriteLine("bad parms");
					}
					break;
				case "reset":
					Reset();
					break;
				case "halt":
					MachineHalt = true;
					break;
				case "pc":
					if (cl.CheckParms("i")) 
					{
						CPU.PC = (ushort)cl.Parms[0].IntValue;
						Trace.WriteLine(String.Format("PC changed to {0:x4}", CPU.PC));
					}
					break;
				case "r":
					Trace.WriteLine(M6502DASM.GetRegisters(CPU));
					break;
				case "step":
					if (cl.CheckParms("i")) 
					{
						Step(cl.Parms[0].IntValue, (ushort)0);
					} 
					else if (cl.CheckParms("ii")) 
					{
						Step(cl.Parms[0].IntValue, (ushort)cl.Parms[1].IntValue);
					} 
					else 
					{
						Trace.WriteLine("malformed step command");
					}
					break;
				case "help":
				case "h":
				case "?":
					Trace.Write("** Machine Specific Commands **\n"
						+ " d [ataddr] [toaddr]: disassemble\n"
						+ " halt: halt machine\n"
						+ " m [ataddr] [toaddr]: memory dump\n"
						+ " pc [addr]: change CPU program counter\n"
						+ " poke [ataddr] [dataval]: poke dataval to ataddr\n"
						+ " r: display CPU registers\n"
						+ " reset: reset machine\n"
						+ " resume: resume machine from halted state\n"
						+ " step [#cpu cycles] [stop PC]: step CPU execution\n"
						);
					break;
				default:
					return false;
			}
			return true;
		}

		public Machine(InputAdapter ia, int scanLines, int firstScanline, int fHZ, int soundSampleRate, int[] p, int vPitch) 
		{
			_InputAdapter = ia;
			_Scanlines = scanLines;
			_FirstScanline = firstScanline;
			_FrameHZ = fHZ;
			_SoundSampleRate = soundSampleRate;
			_Palette = p;
			_VisiblePitch = vPitch;
		}

		void Step(int steps, ushort stopPC) 
		{
			StringBuilder sb = new StringBuilder();

			sb.Append(M6502DASM.Disassemble(Mem, CPU.PC, (ushort)(CPU.PC+1)));
			sb.Append(M6502DASM.GetRegisters(CPU));
			sb.Append("\n");
			for (int i=0; i < steps && CPU.PC != stopPC; i++) 
			{
				CPU.RunClocks = 2;
				CPU.Execute();
				sb.Append(M6502DASM.Disassemble(Mem, CPU.PC, (ushort)(CPU.PC+1)));
				sb.Append(M6502DASM.GetRegisters(CPU));
				sb.Append("\n");
			}
			Trace.WriteLine(sb);
		}
	}
}
