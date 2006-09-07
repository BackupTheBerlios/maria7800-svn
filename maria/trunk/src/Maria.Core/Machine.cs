/*
 * Abstraction of an emulated machine.
 *
 * Copyright (C) 2003, 2004 Mike Murphy
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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Vtg.Util;

namespace Maria.Core {
	[Serializable]
	public abstract class Machine {
		[NonSerialized]
		private IHost host;
		private bool machineHalt;
		private InputAdapter inputAdapter;
		private long frameNumber;
		private int scanlines; // Number of scanlines on the display
		private int firstScanline; // Default starting scanline
		private int frameHZ; // Frame rate
		private int soundSampleRate; // samples/sec
		private int[] palette;
		private int visiblePitch;
		protected M6502 cpu;
		protected AddressSpace mem;
		protected PIA pia;

		public Machine(InputAdapter ia, int scanLines, int firstScanline, int fHZ, int soundSampleRate, int[] p, int vPitch) {
			this.inputAdapter = ia;
			this.scanlines = scanLines;
			this.firstScanline = firstScanline;
			this.frameHZ = fHZ;
			this.soundSampleRate = soundSampleRate;
			this.palette = p;
			this.visiblePitch = vPitch;
		}

		public IHost Host {
			get { return host; }
			set { host = value; }
		}

		public bool MachineHalt {
			get { return machineHalt; }
			set {
				if (!machineHalt)
					machineHalt = true;
			}
		}

		public InputAdapter InputAdapter {
			get { return inputAdapter; }
		}

		public virtual M6502 CPU {
			get { return null; }
		}

		// Current frame number
		public long FrameNumber {
			get { return frameNumber; }
		}

		public int Scanlines {
			get { return scanlines; }
		}

		public int FirstScanline {
			get { return firstScanline; }
		}

		public int FrameHZ {
			get {
				if (frameHZ < 1)
					frameHZ = 1;
				return frameHZ;
			}
			set {
				frameHZ = value;
				if (frameHZ < 1)
					frameHZ = 1;
			}
		}

		public int SoundSampleRate {
			get { return soundSampleRate; }
		}

		public int[] Palette {
			get { return palette; }
		}

		public int VisiblePitch {
			get { return visiblePitch; }
		}

		public static Machine New(GameSettings gs, Cart c, InputAdapter ia) {
			ArgumentCheck.NotNull(gs, "gs");
			ArgumentCheck.NotNull(c, "c");
			ArgumentCheck.NotNull(ia, "ia");
			Machine m;
			switch (gs.MachineType) {
				// TODO : enable machine construction...
				/*case MachineType.A2600NTSC:
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
					break;*/
				default:
					throw new Exception("Unexpected machine type: " + gs.MachineType.ToString());
			}
			m.InputAdapter.Controllers[0] = gs.LController;
			m.InputAdapter.Controllers[1] = gs.RController;
			return m;
		}

		public void Serialize(string fn) {
			FileStream s = null;
			try {
				s = new FileStream(fn, FileMode.Create);
				BinaryFormatter f = new BinaryFormatter();
				f.Serialize(s, this);
			}
			catch (Exception ex) {
				throw ex;
			}
			finally {
				if (s != null) {
					s.Close();
				}
			}
		}

		public static Machine Deserialize(string fn) {
			FileStream s = null;
			Machine m = null;
			try {
				s = new FileStream(fn, FileMode.Open);
				BinaryFormatter f = new BinaryFormatter();
				m = (Machine)f.Deserialize(s);
			}
			catch (Exception ex) {
				throw ex;
			}
			finally {
				if (s != null) {
					s.Close();
				}
			}
			return m;
		}

		public void Reset() {
			frameNumber = 0;
			machineHalt = false;
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

		public void Run() {
			if (!MachineHalt) {
				InputAdapter.CheckPoint(frameNumber);
				DoRun();
				frameNumber++;
			}
		}

		// Called when the Machine instance will no longer be utilized.
		public void Done() {
			DoDone();
		}

		protected abstract void DoReset();
		protected abstract void DoRun();
		protected abstract void DoDone();

		public bool ExecuteCommandLine(CommandLine cl) {
			switch (cl.Verb) {
				case "d":
					if (cl.CheckParms("ii")) {
						Trace.WriteLine(M6502DASM.Disassemble(mem,
							(ushort)cl.Parms[0].IntValue,
							(ushort)cl.Parms[1].IntValue));
					}
					else {
						Trace.WriteLine("bad parms");
					}
					break;
				case "m":
					if (cl.CheckParms("ii")) {
						Trace.WriteLine(M6502DASM.MemDump(mem,
							(ushort)cl.Parms[0].IntValue,
							(ushort)cl.Parms[1].IntValue));
					}
					else if (cl.CheckParms("i")) {
						Trace.WriteLine(M6502DASM.MemDump(mem,
							(ushort)cl.Parms[0].IntValue,
							(ushort)cl.Parms[0].IntValue));
					}
					else {
						Trace.WriteLine("bad parms");
					}
					break;
				case "poke":
					if (cl.CheckParms("ii")) {
						mem[(ushort)cl.Parms[0].IntValue] = (byte)cl.Parms[1].IntValue;
						Trace.WriteLine(String.Format("poke #${0:x2} at ${1:x4} complete",
							cl.Parms[0].IntValue,
							cl.Parms[1].IntValue));
						mem[(ushort)cl.Parms[1].IntValue] = (byte)cl.Parms[0].IntValue;
					}
					else {
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
					if (cl.CheckParms("i")) {
						CPU.PC = (ushort)cl.Parms[0].IntValue;
						Trace.WriteLine(String.Format("PC changed to {0:x4}", CPU.PC));
					}
					break;
				case "r":
					Trace.WriteLine(M6502DASM.GetRegisters(CPU));
					break;
				case "step":
					if (cl.CheckParms("i")) {
						Step(cl.Parms[0].IntValue, (ushort)0);
					}
					else if (cl.CheckParms("ii")) {
						Step(cl.Parms[0].IntValue, (ushort)cl.Parms[1].IntValue);
					}
					else {
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

		private void Step(int steps, ushort stopPC) {
			StringBuilder sb = new StringBuilder();
			sb.Append(M6502DASM.Disassemble(mem, CPU.PC, (ushort)(CPU.PC+1)));
			sb.Append(M6502DASM.GetRegisters(CPU));
			sb.Append("\n");
			for (int i=0; i < steps && CPU.PC != stopPC; i++) {
				CPU.RunClocks = 2;
				CPU.Execute();
				sb.Append(M6502DASM.Disassemble(mem, CPU.PC, (ushort)(CPU.PC+1)));
				sb.Append(M6502DASM.GetRegisters(CPU));
				sb.Append("\n");
			}
			Trace.WriteLine(sb);
		}
	}
}
