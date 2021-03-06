/*
 * Cart.cs
 *
 * An abstraction of a game cart.  Attributable to Kevin Horton's Bankswitching
 * document, the Stella source code, and Eckhard Stolberg's 7800 Bankswitching Guide. 
 *
 * Copyright (c) 2003, 2004 Mike Murphy
 *
 */
using System;
using System.IO;

namespace EMU7800 
{
	public enum CartType 
	{
		Default,
		A2K,	// Atari 2kb cart
		TV8K,	// Tigervision 8kb bankswitched cart
		A4K,	// Atari 4kb cart
		PB8K,	// Parker Brothers 8kb bankswitched cart
		MN16K,	// M-Network 16kb bankswitched cart
		A16K,	// Atari 16kb bankswitched cart
		A16KR,	// Atari 16kb bankswitched cart w/128 bytes RAM
		A8K,	// Atari 8KB bankswitched cart
		A8KR,	// Atari 8KB bankswitched cart w/128 bytes RAM
		A32K,	// Atari 32KB bankswitched cart
		A32KR,	// Atari 32KB bankswitched cart w/128 bytes RAM
		CBS12K,	// CBS' RAM Plus bankswitched cart w/256 bytes RAM
		DC8K,	// Special Activision cart (Robot Tank and Decathlon)
		DPC,	// Pitfall II DPC cart
		A7808,	// Atari7800 non-bankswitched 8KB cart
		A7816,	// Atari7800 non-bankswitched 16KB cart
		A7832,	// Atari7800 non-bankswitched 32KB cart
		A7832P,	// Atari7800 non-bankswitched 32KB cart w/Pokey
		A7848,	// Atari7800 non-bankswitched 48KB cart
		A78SG,	// Atari7800 SuperGame cart
		A78SGP,	// Atari7800 SuperGame cart w/Pokey
		A78SGR,
		A78S9,
		A78S4,
		A78S4R,
		A78AB,	// F18 Hornet cart (Absolute)
		A78AC,  // Double dragon cart (Activision)
	};

	[Serializable]
	public abstract class Cart 
	{
		Machine _M;
		protected Machine M
		{
			get
			{
				return _M;
			}
			set
			{
				_M = value;
			}
		}

		byte[] _ROM;
		protected internal byte[] ROM
		{
			get
			{
				return _ROM;
			}
			set
			{
				_ROM = value;
			}
		}

		public virtual void Reset() { }
		public virtual bool RequestSnooping
		{
			get
			{
				return false;
			}
		}
		public void Map(AddressSpace mem)
		{
			if (mem == null)
			{
				throw new ArgumentNullException("mem");
			}
			_M = mem.M;
		}

		public static Cart New(GameSettings gs)
		{
			BinaryReader rom = new BinaryReader(File.OpenRead(gs.FileInfo.FullName));
			rom.BaseStream.Seek(gs.Offset, SeekOrigin.Begin);

			if (gs.CartType == CartType.Default)
			{
				FileInfo fi = new FileInfo(gs.FileInfo.FullName);
				switch (gs.MachineType)
				{
					case MachineType.A2600NTSC:
					case MachineType.A2600PAL:
						switch (fi.Length - gs.Offset)
						{
							case 2048:  gs.CartType = CartType.A2K;  break;
							case 4096:  gs.CartType = CartType.A4K;  break;
							case 8192:  gs.CartType = CartType.A8K;  break;
							case 16384: gs.CartType = CartType.A16K; break;
							default:
								throw new Exception("Unexpected CartType: " + gs.CartType.ToString());
						}
						break;
					case MachineType.A7800NTSC:
					case MachineType.A7800PAL:
						switch (fi.Length - gs.Offset)
						{
							case 8192:  gs.CartType = CartType.A7808; break;
							case 16384: gs.CartType = CartType.A7816; break;
							case 32768: gs.CartType = CartType.A7832; break;
							case 49152: gs.CartType = CartType.A7848; break;
							default:
								throw new Exception("Unexpected CartType: " + gs.CartType.ToString());
						}
						break;
				}
			}

			return Cart.New(rom, gs.CartType);
		}

		static Cart New(BinaryReader rom, CartType cartType)
		{
			Cart c;

			switch (cartType) 
			{
				case CartType.A2K:
					c = new CartA2K(rom);
					break;
				case CartType.A4K:
					c = new CartA4K(rom);
					break;
				case CartType.A8K:
					c = new CartA8K(rom);
					break;
				case CartType.A8KR:
					c = new CartA8KR(rom);
					break;
				case CartType.A16K:
					c = new CartA16K(rom);
					break;
				case CartType.A16KR:
					c = new CartA16KR(rom);
					break;
				case CartType.DC8K:
					c = new CartDC8K(rom);
					break;
				case CartType.PB8K:
					c = new CartPB8K(rom);
					break;
				case CartType.TV8K:
					c = new CartTV8K(rom);
					break;
				case CartType.CBS12K:
					c = new CartCBS12K(rom);
					break;
				case CartType.A32K:
					c = new CartA32K(rom);
					break;
				case CartType.A32KR:
					c = new CartA32KR(rom);
					break;
				case CartType.MN16K:
					c = new CartMN16K(rom);
					break;
				case CartType.DPC:
					c = new CartDPC(rom);
					break;
				case CartType.A7808:
					c = new Cart7808(rom);
					break;
				case CartType.A7816:
					c = new Cart7816(rom);
					break;
				case CartType.A7832P:
				case CartType.A7832:
					c = new Cart7832(rom);
					break;
				case CartType.A7848:
					c = new Cart7848(rom);
					break;
				case CartType.A78SGP:
				case CartType.A78SG:
					c = new Cart78SG(rom, false);
					break;
				case CartType.A78SGR:
					c = new Cart78SG(rom, true);
					break;
				case CartType.A78S9:
					c = new Cart78S9(rom);
					break;
				case CartType.A78S4:
					c = new Cart78S4(rom, false);
					break;
				case CartType.A78S4R:
					c = new Cart78S4(rom, true);
					break;
				case CartType.A78AB:
					c = new Cart78AB(rom);
					break;
				case CartType.A78AC:
					c = new Cart78AC(rom);
					break;
				default:
					throw new Exception("Unexpected CartType: " + cartType.ToString());
			}
			return c;
		}

		protected void LoadRom(BinaryReader br, int minSize)
		{
			if (br == null)
			{
				throw new ArgumentNullException("br");
			}
			int flen = (int)(br.BaseStream.Length - br.BaseStream.Position);
			int size = minSize > flen ? minSize : flen;
			ROM = new byte[size];
			br.Read(ROM, 0, size);
			br.Close();
		}
	}

	/**
	  Atari standard 2KB carts (no bankswitching)
  
	  Cart Format                Mapping to ROM Address Space
	  0x0000:0x0800              0x1000:0x0800
								 0x1800:0x0800  (1st 2k bank repeated)

	 */
	[Serializable]
	public sealed class CartA2K : Cart, IDevice 
	{
		public byte this[ushort addr]
		{
			get 
			{
				return ROM[addr & 0x07ff];
			}
			set { }
		}

		public CartA2K(BinaryReader br)
		{
			LoadRom(br, 0x0800);
		}
	}

	/**
	  Atari standard 4KB carts (no bankswitching)

	 */
	[Serializable]
	public sealed class CartA4K : Cart, IDevice
	{
		public byte this[ushort addr]
		{
			get 
			{
				return ROM[addr & 0x0fff];
			}
			set { }
		}

		public CartA4K(BinaryReader br)
		{
			LoadRom(br, 0x1000);
		}
	}

	/**
	  Tigervision 8KB bankswitched carts

	  Cart Format                Mapping to ROM Address Space
	  Segment1: 0x0000:0x0800    0x1000:0x0800  Selected segment via $003F
	  Segment2: 0x0800:0x0800    0x1800:0x0800  Always last segment
	  Segment3: 0x1000:0x0800
	  Segment4: 0x1800:0x0800

	 */
	[Serializable]
	public sealed class CartTV8K : Cart, IDevice
	{
		ushort BankBaseAddr;
		ushort LastBankBaseAddr;

		byte Bank 
		{
			set 
			{
				BankBaseAddr = (ushort)(0x0800 * value);
				BankBaseAddr %= (ushort)ROM.Length;
			}
		}
	
		public override void Reset()
		{
			Bank = 0;
		}

		public override bool RequestSnooping 
		{
			get
			{
				return true;
			}
		}
	
		public byte this[ushort addr]
		{
			get
			{
				addr &= 0x0fff;
				if (addr < 0x0800) 
				{
					return ROM[BankBaseAddr + (addr & 0x07ff)];
				} 
				else 
				{
					return ROM[LastBankBaseAddr + (addr & 0x07ff)];
				}
			}
			set
			{
				if (addr <= 0x003f) 
				{
					Bank = value;
				}
			}
		}
	
		public CartTV8K(BinaryReader br)
		{
			LoadRom(br, 0x1000);
			Bank = 0;
			LastBankBaseAddr = (ushort)(ROM.Length - 0x0800);
		}
	}

	/**
	  Atari standard 8KB bankswitched carts

	  Cart Format                Mapping to ROM Address Space
	  Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff8,0x1ff9
	  Bank2: 0x1000:0x1000

	 */
	[Serializable]
	public sealed class CartA8K : Cart, IDevice
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
			Bank = 1;
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
	
		public CartA8K(BinaryReader br)
		{
			LoadRom(br, 0x2000);
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
	}

	/**
	  Atari standard 8KB bankswitched carts with 128 bytes of RAM

	  Cart Format                Mapping to ROM Address Space
	  Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff8,0x1ff9
	  Bank2: 0x1000:0x1000
								 Shadows ROM
								 0x1000:0x0080  RAM write port
					 0x1080:0x0080  RAM read port

	 */
	[Serializable]
	public sealed class CartA8KR : Cart, IDevice
	{
		ushort BankBaseAddr;
		byte[] RAM;
	
		int Bank
		{
			set
			{
				BankBaseAddr = (ushort)(value * 0x1000);
			}
		}
	
		public override void Reset()
		{
			Bank = 1;
		}
	
		public byte this[ushort addr]
		{
			get
			{
				addr &= 0x0fff;
				if (addr < 0x0100 && addr >= 0x0080)
				{
					return RAM[addr & 0x7f];
				}
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set
			{
				addr &= 0x0fff;
				if (addr < 0x0080)
				{
					RAM[addr & 0x7f] = value;
					return;
				}
				UpdateBank(addr);
			}
		}
	
		public CartA8KR(BinaryReader br)
		{
			LoadRom(br, 0x2000);
			Bank = 1;
			RAM = new byte[128];
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
	}

	/**
	  Atari standard 16KB bankswitched carts

	  Cart Format                Mapping to ROM Address Space
	  Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff9-0x1ff9
	  Bank2: 0x1000:0x1000
	  Bank3: 0x2000:0x1000
	  Bank4: 0x3000:0x1000

	 */
	[Serializable]
	public sealed class CartA16K : Cart, IDevice
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
			Bank = 0;
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
	
		public CartA16K(BinaryReader br)
		{
			LoadRom(br, 0x4000);
			Bank = 0;
		}

		void UpdateBank(ushort addr)
		{
			if (addr < 0x0ff6 || addr > 0x0ff9) { }
			else
			{
				Bank = addr - 0x0ff6;
			}
		}
	}

	/**
	  Atari standard 16KB bankswitched carts with 128 bytes of RAM

	  Cart Format                Mapping to ROM Address Space
	  Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x1ff9-0x1ff9
	  Bank2: 0x1000:0x1000
	  Bank3: 0x2000:0x1000
	  Bank4: 0x3000:0x1000
								 Shadows ROM
								 0x1000:0x0080  RAM write port
					 0x1080:0x0080  RAM read port

	 */
	[Serializable]
	public sealed class CartA16KR : Cart, IDevice
	{
		ushort BankBaseAddr;
		byte[] RAM;
	
		int Bank
		{
			set
			{
				BankBaseAddr = (ushort)(value * 0x1000);
			}
		}

		public override void Reset()
		{
			Bank = 0;
		}
	
		public byte this[ushort addr]
		{
			get
			{
				addr &= 0x0fff;
				if (addr < 0x0100 && addr >= 0x0080)
				{
					return RAM[addr & 0x7f];
				}
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set
			{
				addr &= 0x0fff;
				if (addr < 0x0080)
				{
					RAM[addr & 0x7f] = value;
					return;
				}
				UpdateBank(addr);
			}
		}
	
		public CartA16KR(BinaryReader br)
		{
			LoadRom(br, 0x4000);
			Bank = 0;
			RAM = new byte[128];
		}

		void UpdateBank(ushort addr) 
		{
			if (addr < 0x0ff6 || addr > 0x0ff9) { }
			else
			{
				Bank = addr - 0x0ff6;
			}
		}
	}

	/**
	  Activison's Robot Tank and Decathlon 8KB bankswitching cart.

	  Cart Format                Mapping to ROM Address Space	
	  Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by A13=0/1?
	  Bank2: 0x1000:0x1000
  
	  This does what the Stella code does, which is to follow A13 to determine
	  the bank.  Since A0-A12 are the only significant bits on the program
	  counter, I am unsure how the cart/hardware could utilize this.

	*/
	[Serializable]
	public sealed class CartDC8K : Cart, IDevice
	{
		public byte this[ushort addr]
		{
			get
			{
				if ((addr & 0x2000) == 0)
				{
					return ROM[addr & 0x0fff + 0x1000];
				} 
				else
				{
					return ROM[addr & 0x0fff];
				}
			}
			set { }
		}
	
		public CartDC8K(BinaryReader br) 
		{
			LoadRom(br, 0x2000);
		}
	}

	/**
	  Parker Brothers 8KB bankswitched carts.

	  Cart Format                Mapping to ROM Address Space	
	  Segment1: 0x0000:0x0400    Bank1:0x1000:0x0400  Select Segment: 1fe0-1fe7
	  Segment2: 0x0400:0x0400    Bank2:0x1400:0x0400  Select Segment: 1fe8-1ff0
	  Segment3: 0x0800:0x0400    Bank3:0x1800:0x0400  Select Segment: 1ff0-1ff8
	  Segment4: 0x0c00:0x0400    Bank4:0x1c00:0x0400  Always Segment8
	  Segment5: 0x1000:0x0400
	  Segment6: 0x1400:0x0400
	  Segment7: 0x1800:0x0400
	  Segment8: 0x1c00:0x0400

	*/
	[Serializable]
	public sealed class CartPB8K : Cart, IDevice
	{
		ushort[] SegmentBase;
	
		public override void Reset()
		{
			SegmentBase[0] = ComputeSegmentBase(4);
			SegmentBase[1] = ComputeSegmentBase(5);
			SegmentBase[2] = ComputeSegmentBase(6);
		}
	
		public byte this[ushort addr]
		{
			get
			{
				addr &= 0x0fff;
				UpdateSegmentBases(addr);
				return ROM[SegmentBase[addr >> 10] + (addr & 0x03ff)];
			}
			set
			{
				addr &= 0x0fff;
				UpdateSegmentBases(addr);
			}
		}
	
		public CartPB8K(BinaryReader br)
		{
			LoadRom(br, 0x2000);
			SegmentBase = new ushort[4];
			SegmentBase[0] = ComputeSegmentBase(4);
			SegmentBase[1] = ComputeSegmentBase(5);
			SegmentBase[2] = ComputeSegmentBase(6);
			SegmentBase[3] = ComputeSegmentBase(7);
		}

		static ushort ComputeSegmentBase(int slice)
		{
			return (ushort)(slice << 10);  // multiply by 1024
		}

		void UpdateSegmentBases(ushort addr)
		{
			if (addr < 0xfe0 || addr >= 0x0ff8) { }
			else if (addr >= 0x0fe0 && addr < 0x0fe8)
			{
				SegmentBase[0] = ComputeSegmentBase(addr & 0x07);
			}
			else if (addr >= 0x0fe8 && addr < 0x0ff0)
			{
				SegmentBase[1] = ComputeSegmentBase(addr & 0x07);
			}
			else if (addr >= 0x0ff0 && addr < 0x0ff8)
			{
				SegmentBase[2] = ComputeSegmentBase(addr & 0x07);
			}
		}
	}

	/**
	  M-Network 16KB bankswitched carts with 2KB RAM.

	  Cart Format                Mapping to ROM Address Space	
	  Segment1: 0x0000:0x0800    Bank1:0x1000:0x0800  Select Seg: 1fe0-1fe6, 1fe7=RAM Seg1
	  Segment2: 0x0800:0x0800    Bank2:0x1800:0x0800  Always Seg8
	  Segment3: 0x1000:0x0800
	  Segment4: 0x1800:0x0800
	  Segment5: 0x2000:0x0800
	  Segment6: 0x2800:0x0800
	  Segment7: 0x3000:0x0800
	  Segment8: 0x3800:0x0800
  
	  RAM                        RAM Segment1 when 1fe7 select is accessed
	  Segment1: 0x0000:0x0400    0x1000-0x13FF write port
	  Segment2: 0x0400:0x0400    0x1400-0x17FF read port

								 RAM Segment2: 1ff8-1ffb selects 256-byte block
					 0x1800-0x18ff write port
					 0x1900-0x19ff read port

	*/
	[Serializable]
	public sealed class CartMN16K : Cart, IDevice 
	{
		ushort BankBaseAddr, BankBaseRAMAddr;
		bool RAMBankOn;
		byte[] RAM;

		int Bank
		{
			set
			{
				BankBaseAddr = (ushort)(value << 11);  // multiply by 2048
				RAMBankOn = (value == 0x07);
			}
		}

		int BankRAM
		{
			set
			{
				BankBaseRAMAddr = (ushort)(value << 8);  // multiply by 256
			}
		}

		public override void Reset()
		{
			Bank = 0;
			BankRAM = 0;
		}
	
		public byte this[ushort addr]
		{
			get
			{
				addr &= 0x0fff;
				UpdateBanks(addr);
				if (RAMBankOn && addr >= 0x0400 && addr < 0x0800)
				{
					return RAM[addr & 0x03ff];
				} 
				else if (addr >= 0x0900 && addr < 0x0a00)
				{
					return RAM[0x400 + BankBaseRAMAddr + (addr & 0xff)];
				} 
				else if (addr < 0x0800)
				{
					return ROM[BankBaseAddr + (addr & 0x07ff)];
				} 
				else
				{
					return ROM[0x3800 + (addr & 0x07ff)];
				}
			}
			set
			{
				addr &= 0x0fff;
				UpdateBanks(addr);
				if (RAMBankOn && addr < 0x0400)
				{
					RAM[addr & 0x03ff] = value;
				} 
				else if (addr >= 0x0800 && addr < 0x0900)
				{
					RAM[0x400 + BankBaseRAMAddr + (addr & 0xff)] = value;
				}
			}
		}
	
		public CartMN16K(BinaryReader br)
		{
			LoadRom(br, 0x4000);
			RAM = new byte[2048];
			Bank = 0;
			BankRAM = 0;
		}

		void UpdateBanks(ushort addr)
		{
			if (addr >= 0x0fe0 && addr < 0x0fe8)
			{
				Bank = addr & 0x07;
			} 
			else if (addr >= 0x0fe8 && addr < 0x0fec)
			{
				BankRAM = addr & 0x03;
			}
		}
	}

	/**
	  CBS RAM Plus 12KB bankswitched carts with 128 bytes of RAM.

	  Cart Format                Mapping to ROM Address Space	
	  Bank1: 0x0000:0x1000       Bank1:0x1000:0x1000  Select Segment: 0ff8-0ffa
	  Bank2: 0x1000:0x1000
	  Bank3: 0x2000:0x1000
								 Shadows ROM
					 0x1000:0x80 RAM write port
					 0x1080:0x80 RAM read port 

	*/
	[Serializable]
	public sealed class CartCBS12K : Cart, IDevice
	{
		ushort BankBaseAddr;
		byte[] RAM;
	
		int Bank
		{
			set
			{
				BankBaseAddr = (ushort)(value * 0x1000);
			}
		}

		public override void Reset()
		{
			Bank = 2;
		}
	
		public byte this[ushort addr]
		{
			get
			{
				addr &= 0x0fff;
				if (addr < 0x0200 && addr >= 0x0100)
				{
					return RAM[addr & 0xff];
				}
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set 
			{
				addr &= 0x0fff;
				if (addr < 0x0100)
				{
					RAM[addr & 0xff] = value;
					return;
				}
				UpdateBank(addr);
			}
		}
	
		public CartCBS12K(BinaryReader br) 
		{
			LoadRom(br, 0x3000);
			Bank = 2;
			RAM = new byte[256];
		}

		void UpdateBank(ushort addr)
		{
			if (addr < 0x0ff8 || addr > 0x0ffa) { }
			else
			{
				Bank = addr - 0x0ff8;
			}
		}
	}

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
	  Atari standard 32KB bankswitched carts with 128 bytes of RAM

	  Cart Format                Mapping to ROM Address Space
	  Bank1: 0x0000:0x1000       0x1000:0x1000  Bank selected by accessing 0x0ff4-0x0ffc
	  Bank2: 0x1000:0x1000
	  Bank3: 0x2000:0x1000
	  Bank4: 0x3000:0x1000
	  Bank5: 0x4000:0x1000
	  Bank6: 0x5000:0x1000
	  Bank7: 0x6000:0x1000
	  Bank8: 0x7000:0x1000
								 Shadows ROM
					 0x1000:0x80 RAM write port
					 0x1080:0x80 RAM read port
	 */
	[Serializable]
	public sealed class CartA32KR : Cart, IDevice
	{
		ushort BankBaseAddr;
		byte[] RAM;
	
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
				if (addr >= 0x0080 && addr < 0x0100)
				{
					return RAM[addr & 0x007f];
				}
				UpdateBank(addr);
				return ROM[BankBaseAddr + addr];
			}
			set
			{
				addr &= 0x0fff;
				if (addr < 0x0080)
				{
					RAM[addr & 0x007f] = value;
					return;
				}
				UpdateBank(addr);
			}
		}
	
		public CartA32KR(BinaryReader br)
		{
			LoadRom(br, 0x8000);
			RAM = new byte[128];
			Bank = 7;
		}

		void UpdateBank(ushort addr)
		{
			if (addr < 0x0ffc && addr >= 0x0ff4 )
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
	  Atari 7800 non-bankswitched 8KB cartridge
  
	  Cart Format                Mapping to ROM Address Space
	  0x0000:0x2000              0xE000:0x2000
  
	*/
	[Serializable]
	public sealed class Cart7808 : Cart, IDevice
	{
		public override void Reset() { }
	
		public byte this[ushort addr]
		{
			get
			{
				return ROM[addr & 0x1fff];
			}
			set { }
		}

		public Cart7808(BinaryReader br)
		{
			LoadRom(br, 0x2000);
		}
	}

	/**
	  Atari 7800 non-bankswitched 16KB cartridge
  
	  Cart Format                Mapping to ROM Address Space
	  0x0000:0x4000              0xC000:0x4000
  
	*/
	[Serializable]
	public sealed class Cart7816 : Cart, IDevice
	{
		public override void Reset() { }

		public byte this[ushort addr]
		{
			get
			{
				return ROM[addr & 0x3fff];
			}
			set { }
		}

		public Cart7816(BinaryReader br)
		{
			LoadRom(br, 0x4000);
		}
	}

	/**
	  Atari 7800 non-bankswitched 32KB cartridge
  
	  Cart Format                Mapping to ROM Address Space
	  0x0000:0x8000              0x8000:0x8000
  
	*/
	[Serializable]
	public sealed class Cart7832 : Cart, IDevice
	{
		public override void Reset() { }

		public byte this[ushort addr]
		{
			get
			{
				return ROM[addr & 0x7fff];
			}
			set { }
		}

		public Cart7832(BinaryReader br)
		{
			LoadRom(br, 0x8000);
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