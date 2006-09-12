namespace EMU7800
{
	/**
	  Atari standard 32KB bankswitched carts

	  Cart Format                Mapping to ROM Address Space
	  Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x0ff4-0x0ffc
	  Bank2: 0x1000:0x1000
	  Bank3: 0x2000:0x1000
	  Bank4: 0x3000:0x1000
	  Bank5: 0x4000:0x1000
	  Bank6: 0x5000:0x1000
	  Bank7: 0x6000:0x1000
	  Bank8: 0x7000:0x1000

	 */
	[Serializable]
	public sealed class CartA32K : Cart, IDevice
	{
		ushort BankBaseAddr;

		int Bank
		{
			set
			{
				BankBaseAddr = (ushort)(value * 0x1000);
			}
		}

		public override void Reset()
		{
			Bank = 7;
		}

		public byte this[ushort addr]
		{
			get
			{
				addr &= 0x0fff;
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set
			{
				addr &= 0x0fff;
				UpdateBank(addr);
			}
		}

		public CartA32K(BinaryReader br)
		{
			LoadRom(br, 0x8000);
			Bank = 7;
		}

		void UpdateBank(ushort addr)
		{
			if (addr < 0x0ffc && addr >= 0x0ff4)
			{
				Bank = addr - 0x0ff4;
			}
		}
	}

	/**
	  Pitfall II cartridge.  There are two 4k banks, 2k display bank, and the DPC chip.
	  For complete details on the DPC chip see David P. Crane's United States Patent
	  Number 4,644,495.

	  Cart Format                Mapping to ROM Address Space
	  Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff8,0x1ff9
	  Bank2: 0x1000:0x1000

	 */
	[Serializable]
	public sealed class CartDPC : Cart, IDevice
	{
		const ushort DisplayBaseAddr = 0x2000;
		ushort BankBaseAddr;

		byte[] Tops = new byte[8];
		byte[] Bots = new byte[8];
		ushort[] Counters = new ushort[8];
		byte[] Flags = new byte[8];
		bool[] MusicMode = new bool[3];

		ulong LastSystemClock;
		double FractionalClocks;

		int Bank
		{
			set
			{
				BankBaseAddr = (ushort)(value * 0x1000);
			}
		}

		//
		// Generate a sequence of pseudo-random numbers 255 numbers long
		// by emulating an 8-bit shift register with feedback taps at
		// bits 4, 3, 2, and 0.
		//
		byte _ShiftRegister;
		byte ShiftRegister
		{
			get
			{

				byte a, x;
				a = _ShiftRegister;
				a &= (1 << 0);

				x = _ShiftRegister;
				x &= (1 << 2);
				x >>= 2;
				a ^= x;

				x = _ShiftRegister;
				x &= (1 << 3);
				x >>= 3;
				a ^= x;

				x = _ShiftRegister;
				x &= (1 << 4);
				x >>= 4;
				a ^= x;

				a <<= 7;
				_ShiftRegister >>= 1;
				_ShiftRegister |= a;

				return _ShiftRegister;
			}
			set
			{
				_ShiftRegister = value;
			}
		}

		public override void Reset()
		{
			Bank = 1;
			LastSystemClock = 3*M.CPU.Clock;
			FractionalClocks = 0.0;
			ShiftRegister = 1;
		}

		public byte this[ushort addr]
		{
			get
			{
				addr &= 0x0fff;
				if (addr < 0x0040)
				{
					return ReadPitfall2Reg(addr);
				}
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set
			{
				addr &= 0x0fff;
				if (addr >= 0x0040 && addr < 0x0080)
				{
					WritePitfall2Reg(addr, value);
				}
				else
				{
					UpdateBank(addr);
				}
			}
		}

		public CartDPC(BinaryReader br)
		{
			LoadRom(br, 0x2800);
			Bank = 1;
		}

		void UpdateBank(ushort addr)
		{
			switch(addr)
			{
				case 0x0ff8:
					Bank = 0;
					break;
				case 0x0ff9:
					Bank = 1;
					break;
			}
		}

		byte[] MusicAmplitudes = new byte[] {0x00, 0x04, 0x05, 0x09, 0x06, 0x0a, 0x0b, 0x0f};

		byte ReadPitfall2Reg(ushort addr)
		{
			byte result;

			int i = addr & 0x07;
			int fn = (addr >> 3) & 0x07;

			// Update flag register for selected data fetcher
			if ((Counters[i] & 0x00ff) == Tops[i])
			{
				Flags[i] = 0xff;
			}
			else if ((Counters[i] & 0x00ff) == Bots[i])
			{
				Flags[i] = 0x00;
			}

			switch (fn)
			{
				case 0x00:
					if (i < 4)
					{	// This is a random number read
						result = ShiftRegister;
						break;
					}
					// Its a music read
					UpdateMusicModeDataFetchers();

					byte j = 0;
					if (MusicMode[0] == true && Flags[5] != 0)
					{
						j |= 0x01;
					}
					if (MusicMode[1] == true && Flags[6] != 0)
					{
						j |= 0x02;
					}
					if (MusicMode[2] == true && Flags[7] != 0)
					{
						j |= 0x04;
					}
					result = MusicAmplitudes[j];
					break;
					// DFx display data read
				case 0x01:
					result = ROM[DisplayBaseAddr + 0x7ff - Counters[i]];
					break;
					// DFx display data read AND'd w/flag
				case 0x02:
					result = ROM[DisplayBaseAddr + 0x7ff - Counters[i]];
					result &= Flags[i];
					break;
					// DFx flag
				case 0x07:
					result = Flags[i];
					break;
				default:
					result = 0;
					break;
			}

			// Clock the selected data fetcher's counter if needed
			if (i < 5 || i >= 5 && MusicMode[i - 5] == false)
			{
				Counters[i]--;
				Counters[i] &= 0x07ff;
			}

			return result;
		}

		void UpdateMusicModeDataFetchers()
		{
			ulong sysClockDelta = 3*M.CPU.Clock - LastSystemClock;
			LastSystemClock = 3*M.CPU.Clock;

			double OSCclocks = ((15750.0 * sysClockDelta) / 1193182.0)
				+ FractionalClocks;

			int wholeClocks = (int)OSCclocks;

			FractionalClocks = OSCclocks - (double)wholeClocks;

			if (wholeClocks <= 0)
			{
				return;
			}

			for (int i=0; i < 3; i++)
			{
				int r = i + 5;
				if (!MusicMode[i])
				{
					continue;
				}

				int top = Tops[r] + 1;
				int newLow = Counters[r] & 0x00ff;

				if (Tops[r] != 0)
				{
					newLow -= (wholeClocks % top);
					if (newLow < 0)
					{
						newLow += top;
					}
				}
				else
				{
					newLow = 0;
				}

				if (newLow <= Bots[r])
				{
					Flags[r] = 0x00;
				}
				else if (newLow <= Tops[r])
				{
					Flags[r] = 0xff;
				}

				Counters[r] = (ushort)((Counters[r] & 0x0700) | (ushort)newLow);
			}
		}

		void WritePitfall2Reg(ushort addr, byte val)
		{
			int i = addr & 0x07;
			int fn = (addr >> 3) & 0x07;

			switch (fn)
			{
					// DFx top count
				case 0x00:
					Tops[i] = val;
					Flags[i] = 0x00;
					break;
					// DFx bottom count
				case 0x01:
					Bots[i] = val;
					break;
					// DFx counter low
				case 0x02:
					Counters[i] &= 0x0700;
					if (i >= 5 && MusicMode[i - 5] == true)
					{
						// Data fetcher is in music mode so its low counter value
						// should be loaded from the top register not the poked value
						Counters[i] |= Tops[i];
					}
					else
					{
						// Data fetcher is either not a music mode data fetcher or it
						// isn't in music mode so it's low counter value should be loaded
						// with the poked value
						Counters[i] |= val;
					}
					break;
					// DFx counter high
				case 0x03:
					Counters[i] &= 0x00ff;
					Counters[i] |= (ushort)((val & 0x07) << 8);
					// Execute special code for music mode data fetchers
					if (i >= 5)
					{
						MusicMode[i - 5] = (val & 0x10) != 0;
						// NOTE: We are not handling the clock source input for
						// the music mode data fetchers.  We're going to assume
						// they always use the OSC input.
					}
					break;
					// Random Number Generator Reset
				case 0x06:
					ShiftRegister = 1;
					break;
				default:
					break;
			}
		}
	}


	/**
	  Atari 7800 non-bankswitched 48KB cartridge

	  Cart Format                Mapping to ROM Address Space
	  0x0000:0xc000              0x4000:0xc000

	*/
	[Serializable]
	public sealed class Cart7848 : Cart, IDevice
	{
		public override void Reset() { }

		public byte this[ushort addr]
		{
			get
			{
				return ROM[addr - 0x4000];
			}
			set { }
		}

		public Cart7848(BinaryReader br)
		{
			LoadRom(br, 0xc000);
		}
	}

	/**
	  Atari 7800 SuperGame bankswitched cartridge

	  Cart Format                Mapping to ROM Address Space
	  Bank0: 0x00000:0x4000      0x0000:0x4000
	  Bank1: 0x04000:0x4000      0x4000:0x4000  Bank6
	  Bank2: 0x08000:0x4000      0x8000:0x4000  Bank0-7 (0 on startup)
	  Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank7
	  Bank4: 0x10000:0x4000
	  Bank5: 0x14000:0x4000
	  Bank6: 0x18000:0x4000
	  Bank7: 0x1c000:0x4000

	*/
	[Serializable]
	public sealed class Cart78SG : Cart, IDevice
	{
		int[] Bank = new int[4];
		int BankNo;
		byte[] RAM;

		public override void Reset() { }

		public byte this[ushort addr]
		{
			get
			{
				BankNo = addr >> 14;
				if (RAM != null && BankNo == 1)
				{
					return RAM[addr & 0x3fff];
				}
				else
				{
					return ROM[ (Bank[BankNo] << 14) | (addr & 0x3fff) ];
				}
			}
			set
			{
				BankNo = addr >> 14;
				if (BankNo == 2)
				{
					Bank[2] = value % (ROM.Length/0x4000);
				}
				else if (RAM != null && BankNo == 1)
				{
					RAM[addr & 0x3fff] = value;
				}
			}
		}

		public Cart78SG(BinaryReader br, bool needRAM)
		{
			if (br == null)
			{
				throw new ArgumentNullException();
			}
			if (needRAM)
			{
				RAM = new byte[0x4000];
			}

			Bank[1] = 6;
			Bank[2] = 0;
			Bank[3] = 7;

			int size = (int)(br.BaseStream.Length - br.BaseStream.Position);
			LoadRom(br, size);
		}
	}

	/**
	  Atari 7800 SuperGame S9 bankswitched cartridge

	  Cart Format                Mapping to ROM Address Space
	  Bank0: 0x00000:0x4000      0x0000:0x4000
	  Bank1: 0x04000:0x4000      0x4000:0x4000  Bank0
	  Bank2: 0x08000:0x4000      0x8000:0x4000  Bank0-8 (1 on startup)
	  Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank8
	  Bank4: 0x10000:0x4000
	  Bank5: 0x14000:0x4000
	  Bank6: 0x18000:0x4000
	  Bank7: 0x1c000:0x4000
	  Bank8: 0x20000:0x4000

	*/
	[Serializable]
	public sealed class Cart78S9 : Cart, IDevice
	{
		int[] Bank = new int[4];

		public override void Reset() { }

		public byte this[ushort addr]
		{
			get
			{
				return ROM[ (Bank[addr >> 14] << 14) | (addr & 0x3fff) ];
			}
			set
			{
				if ((addr >> 14) == 2)
				{
					Bank[2] = (value + 1) % (ROM.Length/0x4000);
				}
			}
		}

		public Cart78S9(BinaryReader br)
		{
			if (br == null)
			{
				throw new ArgumentNullException("br");
			}
			Bank[1] = 0;
			Bank[2] = 1;
			Bank[3] = 8;

			int size = (int)(br.BaseStream.Length - br.BaseStream.Position);
			LoadRom(br, size);
		}
	}

	/**
	  Atari 7800 SuperGame S4 bankswitched cartridge

	  Cart Format                Mapping to ROM Address Space
	  Bank0: 0x00000:0x4000      0x0000:0x4000
	  Bank1: 0x04000:0x4000      0x4000:0x4000  Bank2
	  Bank2: 0x08000:0x4000      0x8000:0x4000  Bank0 (0 on startup)
	  Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank3

	  Banks 0-3 are the same as banks 4-7


	*/
	[Serializable]
	public sealed class Cart78S4 : Cart, IDevice
	{
		int[] Bank = new int[4];
		int BankNo;
		byte[] RAM;

		public override void Reset() { }

		public byte this[ushort addr]
		{
			get
			{
				BankNo = addr >> 14;
				if (RAM != null && BankNo == 1)
				{
					return RAM[addr & 0x3fff];
				}
				else
				{
					return ROM[ (Bank[BankNo] << 14) | (addr & 0x3fff) ];
				}
			}
			set
			{
				BankNo = addr >> 14;
				if (BankNo == 2)
				{
					Bank[2] = value % (ROM.Length/0x4000);
				}
				else if (RAM != null && BankNo == 1)
				{
					RAM[addr & 0x3fff] = value;
				}
			}
		}

		public Cart78S4(BinaryReader br, bool needRAM)
		{
			if (br == null)
			{
				throw new ArgumentNullException("br");
			}
			if (needRAM)
			{
				RAM = new byte[0x4000];
			}

			int size = (int)(br.BaseStream.Length - br.BaseStream.Position);
			LoadRom(br, size);

			Bank[1] = 6 % (ROM.Length/0x4000);
			Bank[2] = 0 % (ROM.Length/0x4000);
			Bank[3] = 7 % (ROM.Length/0x4000);
		}
	}

	/**
	  Atari 7800 Absolute bankswitched cartridge

	  Cart Format                Mapping to ROM Address Space
	  Bank0: 0x00000:0x4000      0x0000:0x4000
	  Bank1: 0x04000:0x4000      0x4000:0x4000  Bank0-1 (0 on startup)
	  Bank2: 0x08000:0x4000      0x8000:0x4000  Bank2
	  Bank3: 0x0c000:0x4000      0xc000:0x4000  Bank3

	*/
	[Serializable]
	public sealed class Cart78AB : Cart, IDevice
	{
		int[] Bank = new int[4];
		int BankNo;

		public override void Reset() { }

		public byte this[ushort addr]
		{
			get
			{
				return ROM[ (Bank[addr >> 14] << 14) | (addr & 0x3fff) ];
			}
			set
			{
				BankNo = addr >> 14;
				if (BankNo == 2)
				{
					Bank[1] = (value - 1) & 0x1;
				}
			}
		}

		public Cart78AB(BinaryReader br)
		{
			if (br == null)
			{
				throw new ArgumentNullException("br");
			}
			Bank[1] = 0;
			Bank[2] = 2;
			Bank[3] = 3;
			int size = (int)(br.BaseStream.Length - br.BaseStream.Position);
			LoadRom(br, size);
		}
	}

	/**
	  Atari 7800 Activision bankswitched cartridge

	  Cart Format                Mapping to ROM Address Space
	  Bank0 : 0x00000:0x2000      0x0000:0x2000
	  Bank1 : 0x02000:0x2000      0x2000:0x2000
	  Bank2 : 0x04000:0x2000      0x4000:0x2000  Bank13
	  Bank3 : 0x06000:0x2000      0x6000:0x2000  Bank12
	  Bank4 : 0x08000:0x2000      0x8000:0x2000  Bank15
	  Bank5 : 0x0a000:0x2000      0xa000:0x2000  Bank(n)   n in [0-15], n=0 on startup
	  Bank6 : 0x0c000:0x2000      0xc000:0x2000  Bank(n+1)
	  Bank7 : 0x0e000:0x2000      0xe000:0x2000  Bank14
	  Bank8 : 0x10000:0x2000
	  Bank9 : 0x12000:0x2000
	  Bank10: 0x14000:0x2000
	  Bank11: 0x16000:0x2000
	  Bank12: 0x18000:0x2000
	  Bank13: 0x1a000:0x2000
	  Bank14: 0x1c000:0x2000
	  Bank15: 0x13000:0x2000

	*/
	[Serializable]
	public sealed class Cart78AC : Cart, IDevice
	{
		int[] Bank = new int[8];

		public override void Reset() { }

		public byte this[ushort addr]
		{
			get
			{
				return ROM[ (Bank[addr >> 13] << 13) | (addr & 0x1fff) ];
			}
			set
			{
				if ((addr & 0xfff0) == 0xff80)
				{
					Bank[5] = (addr & 0xf) << 1;
					Bank[6] = Bank[5] + 1;
				}
			}
		}

		public Cart78AC(BinaryReader br)
		{
			if (br == null)
			{
				throw new ArgumentNullException("br");
			}
			Bank[2] = 13;
			Bank[3] = 12;
			Bank[4] = 15;
			Bank[5] = 0;
			Bank[6] = 1;
			Bank[7] = 14;
			int size = (int)(br.BaseStream.Length - br.BaseStream.Position);
			LoadRom(br, size);
		}
	}
}
