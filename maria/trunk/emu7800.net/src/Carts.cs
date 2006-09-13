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
}
