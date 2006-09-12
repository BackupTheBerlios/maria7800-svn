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
