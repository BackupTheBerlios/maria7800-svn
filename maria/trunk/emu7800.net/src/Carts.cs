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
}
