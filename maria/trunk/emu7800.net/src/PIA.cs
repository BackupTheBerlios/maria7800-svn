/*
 * PIA.cs
 *
 * The Peripheral Interface Adaptor (6532) device.
 * a.k.a. RIOT (RAM I/O Timer?)
 *
 * Copyright (c) 2003, 2004 Mike Murphy
 *
 */
using System;
using System.Diagnostics;

namespace EMU7800
{
	[Serializable]
	public sealed class PIA : IDevice
	{
		Machine M;

		byte[] RAM = new byte[128];

		ulong TimerTarget;
		int TimerShift;
		bool IRQEnabled, IRQTriggered;

		byte DDRA;

		public void Reset()
		{
			// Some games will loop/hang on $0284 if these are initialized to zero
			TimerShift = 10;
			TimerTarget = M.CPU.Clock + (ulong)(0xff << TimerShift);

			IRQEnabled = false;
			IRQTriggered = false;

			DDRA = 0;

            Trace.Write(this);
			Trace.WriteLine(" reset");
		}

		public void Map(AddressSpace mem) { }

		public override string ToString()
		{
			return "PIA/RIOT M6532";
		}

		public byte this[ushort addr]
		{
			get
			{
				return peek(addr);
			}
			set
			{
				poke(addr, value);
			}
		}

		public byte peek(ushort addr)
		{
			InputAdapter ia = M.InputAdapter;

			if ((addr & 0x200) == 0)
			{
				return RAM[addr & 0x7f];
			}

			// A2 Distingusishes I/O registers from the Timer
			if ((addr & 0x04) != 0)
			{
				if ((addr & 0x01) != 0)
				{
					return ReadInterruptFlag();
				}
				else
				{
					return ReadTimerRegister();
				}
			}
			else
			{
				switch ((byte)(addr & 0x03))
				{
					case 0:  // SWCHA: Controllers
						return ia.ReadPortA();
					case 1:	 // SWCHA DDR: 0=input, 1=output
						return DDRA;
					case 2:	 // SWCHB: Console switches
						return ia.ReadPortB();
					default: // SWCHB DDR, hardwired as input
						return 0;
				}
			}
		}

		public void poke(ushort addr, byte data)
		{
			InputAdapter ia = M.InputAdapter;

			if ((addr & 0x200) == 0)
			{
				RAM[addr & 0x7f] = data;
				return;
			}

			// A2 Distingusishes I/O registers from the Timer
			if ((addr & 0x04) != 0)
			{
				if ((addr & 0x10) != 0)
				{
					IRQEnabled = (addr & 0x08) != 0;
					SetTimerRegister(data, addr & 0x03);
				}
			}
			else
			{
				switch ((byte)(addr & 0x03))
				{
					case 0: // SWCHA
						ia.WritePortA((byte)(data & DDRA));
						break;
					case 1: // SWCHA DDR
						DDRA = data;
						break;
                    default: // SWCHA hardwired as input
						break;
				}
			}
		}

		public PIA(Machine m)
		{
			M = m;
		}

		// 0: TIM1T:  set    1 clock interval (838 nsec/interval)
		// 1: TIM8T:  set    8 clock interval (6.7 usec/interval)
		// 2: TIM64T: set   64 clock interval (53.6 usec/interval)
		// 3: T1024T: set 1024 clock interval (858.2 usec/interval)
		void SetTimerRegister(byte data, int interval)
		{
			IRQTriggered = false;
			TimerShift = new int[] { 0, 3, 6, 10 }[interval];
			TimerTarget = M.CPU.Clock + (ulong)(data << TimerShift);
		}

		byte ReadTimerRegister()
		{
			IRQTriggered = false;
			int delta = (int)(TimerTarget - M.CPU.Clock);
			if (delta >= 0)
			{
				return (byte)(delta >> TimerShift);
			}
			else
			{
				if (delta != -1)
				{
					IRQTriggered = true;
				}
				return (byte)(delta >= -256 ? delta : 0);
			}
		}

		byte ReadInterruptFlag()
		{
			int delta = (int)(TimerTarget - M.CPU.Clock);
			if (delta >= 0 || IRQEnabled && IRQTriggered)
			{
				return 0x00;
			}
			else
			{
				return 0x80;
			}
		}
	}
}