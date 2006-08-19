/*
 * Cart type enumeration
 * Copyright (c) 2003, 2004 Mike Murphy
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

namespace Maria.Core {
	public enum CartType {
		Default,
		A2K,    // Atari 2kb cart
		TV8K,   // Tigervision 8kb bankswitched cart
		A4K,    // Atari 4kb cart
		PB8K,   // Parker Brothers 8kb bankswitched cart
		MN16K,  // M-Network 16kb bankswitched cart
		A16K,   // Atari 16kb bankswitched cart
		A16KR,  // Atari 16kb bankswitched cart w/128 bytes RAM
		A8K,    // Atari 8KB bankswitched cart
		A8KR,   // Atari 8KB bankswitched cart w/128 bytes RAM
		A32K,   // Atari 32KB bankswitched cart
		A32KR,  // Atari 32KB bankswitched cart w/128 bytes RAM
		CBS12K, // CBS' RAM Plus bankswitched cart w/256 bytes RAM
		DC8K,   // Special Activision cart (Robot Tank and Decathlon)
		DPC,    // Pitfall II DPC cart
		A7808,  // Atari7800 non-bankswitched 8KB cart
		A7816,  // Atari7800 non-bankswitched 16KB cart
		A7832,  // Atari7800 non-bankswitched 32KB cart
		A7832P, // Atari7800 non-bankswitched 32KB cart w/Pokey
		A7848,  // Atari7800 non-bankswitched 48KB cart
		A78SG,  // Atari7800 SuperGame cart
		A78SGP, // Atari7800 SuperGame cart w/Pokey
		A78SGR,
		A78S9,
		A78S4,
		A78S4R,
		A78AB,  // F18 Hornet cart (Absolute)
		A78AC,  // Double dragon cart (Activision)
	}
}
