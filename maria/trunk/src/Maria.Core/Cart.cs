/*
 * An abstraction of a game cart.  Attributable to Kevin Horton's Bankswitching
 * document, the Stella source code, and Eckhard Stolberg's 7800 Bankswitching Guide.
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
using System.IO;
using Vtg.Util;

namespace Maria.Core {

	public class UnknownCartTypeException : MariaCoreException {
		public UnknownCartTypeException(string msg) : base(msg) {}
	}

	[Serializable]
	public abstract class Cart : IDevice {
		private Machine machine;
		private byte[] rom;

		public virtual void Reset() {}

		public virtual bool RequestSnooping {
			get {
				return false;
			}
		}

		public void Map(AddressSpace mem) {
			ArgumentCheck.NotNull(mem, "mem");
			machine = mem.Machine;
		}

		public abstract byte this[ushort addr] {
			get;
			set;
		}

		protected Machine Machine {
			get { return machine; }
			set { machine = value; }
		}

		protected internal byte[] ROM {
			get { return rom; }
			set { rom = value; }
		}

		private static Cart New(BinaryReader rom, CartType cartType) {
			Cart c;
			switch (cartType) {
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
				case CartType.MN16K:
					c = new CartMN16K(rom);
					break;
				case CartType.A32KR:
					c = new CartA32KR(rom);
					break;
				case CartType.DPC:
					c = new CartDPC(rom);
					break;
				// TODO : remaining 2600 cart types

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
				// TODO : remaining 7800 cart types

				default:
					throw new UnknownCartTypeException("Unknown cart type: " +
						cartType.ToString());
			}
			return c;
			// TODO : move everything up into real switch...
			/*switch (cartType) {
				case CartType.A32K:
					c = new CartA32K(rom);
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
			}*/
		}

		public static Cart New(GameSettings gs) {
			BinaryReader rom = new BinaryReader(File.OpenRead(gs.FileInfo.FullName));
			rom.BaseStream.Seek(gs.Offset, SeekOrigin.Begin);
			Console.WriteLine("CartType from database : " + gs.CartType);
			if (gs.CartType == CartType.Default)
				FixCartType(gs);
			Console.WriteLine("I'm gonna use CartType " + gs.CartType);
			return Cart.New(rom, gs.CartType);
		}

		// TODO : should probably be in the GameSettings class
		private static void FixCartType(GameSettings gs) {
			FileInfo fi = new FileInfo(gs.FileInfo.FullName);
			switch (gs.MachineType) {
				case MachineType.A2600NTSC:
				case MachineType.A2600PAL:
					switch (fi.Length - gs.Offset) {
						case 2048: gs.CartType = CartType.A2K; break;
						case 4096: gs.CartType = CartType.A4K; break;
						case 8192: gs.CartType = CartType.A8K; break;
						case 16384: gs.CartType = CartType.A16K; break;
						default:
							throw new UnknownCartTypeException("Couldn't guess type of cart image " +
								fi.FullName);
					}
					break;
				case MachineType.A7800NTSC:
				case MachineType.A7800PAL:
					switch (fi.Length - gs.Offset) {
						case 8192: gs.CartType = CartType.A7808; break;
						case 16384: gs.CartType = CartType.A7816; break;
						case 32768: gs.CartType = CartType.A7832; break;
						case 49152: gs.CartType = CartType.A7848; break;
						default:
							throw new UnknownCartTypeException("Couldn't guess type of cart image " +
								fi.FullName);
					}
					break;
			}
		}

		protected void LoadRom(BinaryReader br, int minSize) {
			ArgumentCheck.NotNull(br, "br");
			int flen = (int)(br.BaseStream.Length - br.BaseStream.Position);
			int size = minSize > flen ? minSize : flen;
			ROM = new byte[size];
			br.Read(ROM, 0, size);
			br.Close();
		}
	}
}
