/*
 * FontRenderer
 *
 * A simple font renderer for displaying text during emulation.  Font data and
 * rendering algorithm courtesy of Bradford W. Mott's Stella source.
 *
 * Copyright (c) 2004 Mike Murphy
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
	public class FontRenderer {
		private static uint[] AlphaFontData = {
			0x699f999, // A
			0xe99e99e, // B
			0x6988896, // C
			0xe99999e, // D
			0xf88e88f, // E
			0xf88e888, // F
			0x698b996, // G
			0x999f999, // H
			0x7222227, // I
			0x72222a4, // J
			0x9accaa9, // K
			0x888888f, // L
			0x9ff9999, // M
			0x9ddbb99, // N
			0x6999996, // O
			0xe99e888, // P
			0x69999b7, // Q
			0xe99ea99, // R
			0x6986196, // S
			0x7222222, // T
			0x9999996, // U
			0x9999966, // V
			0x9999ff9, // W
			0x99fff99, // X
			0x9996244, // Y
			0xf12488f  // Z
		};

		private static uint[] DigitFontData = {
			0x69bd996, // 0
			0x2622227, // 1
			0x691248f, // 2
			0x6916196, // 3
			0xaaaf222, // 4
			0xf88e11e, // 5
			0x698e996, // 6
			0xf112244, // 7
			0x6996996, // 8
			0x6997196  // 9
		};

		private uint[] FrameBuffer;
		private int Pitch;
		private int[] Palette;
		private byte Alpha;

		public void DrawText(string text, int xoffset, int yoffset, int fore, int back) {
			char[] textchars = text.ToUpper().ToCharArray();
			for (int i = 0; i < text.Length + 1; i++) {
				for (int j = 0; j < 9; j++) {
					int pos = (j + yoffset) * Pitch + i * 5;
					for (int k = 0; k < 5; k++) {
						while (pos >= FrameBuffer.Length) {
							pos -= FrameBuffer.Length;
						}
						while (pos < 0) {
							pos += FrameBuffer.Length;
						}
						FrameBuffer[pos] = (uint)(Alpha << 24 | Palette[back]);
						pos++;
					}
				}
			}

			for (int i = 0; i < text.Length; i++) {
				char c = textchars[i];
				uint fdata;
				switch (c) {
					case '/':
					case '\\':
						fdata = 0x0122448;
						break;
					case '(':
						fdata = 0x2488842;
						break;
					case ')':
						fdata = 0x4211124;
						break;
					case '.':
						fdata = 0x0000066;
						break;
					case ':':
						fdata = 0x0660660;
						break;
					case '-':
						fdata = 0x0007000;
						break;
					default:
						if (c >= 'A' && c <= 'Z') {
							fdata = AlphaFontData[c - 'A'];
						}
						else if (c >= '0' && c <= '9') {
							fdata = DigitFontData[c - '0'];
						}
						else {
							fdata = 0;
						}
						break;
				}

				int xpos, ypos = 8;
				for (int j = 0; j < 32; j++) {
					xpos = j % 4;
					if (xpos == 0) {
						ypos--;
					}

					int pos = (ypos + yoffset) * Pitch + (4 - xpos) + xoffset;
					while (pos >= FrameBuffer.Length) {
						pos -= FrameBuffer.Length;
					}
					while (pos < 0) {
						pos += FrameBuffer.Length;
					}
					if (((fdata >> j) & 1) != 0) {
						FrameBuffer[pos] = (uint)(Alpha << 24 | Palette[fore]);
					}
				}
				xoffset += 5;
			}
		}

		public FontRenderer(uint[] frameBuffer, int pitch, int[] palette, byte alpha) {
			FrameBuffer = frameBuffer;
			Pitch = pitch;
			Palette = palette;
			Alpha = alpha;
		}
	}
}
