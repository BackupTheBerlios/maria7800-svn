/*
 * Maria main program.
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
using System.Threading;
using Tao.Sdl;

namespace Maria {
	public class Maria {

		public void Run() {
			bool quitFlag = false;
			int flags = Sdl.SDL_HWSURFACE | Sdl.SDL_DOUBLEBUF | Sdl.SDL_ANYFORMAT;
			int bpp = 16;
			int width = 640;
			int height = 480;
			Console.WriteLine("Just some SDL test...");
			// We don't do any error checking here, which isn't particularly good...
			try {
				Sdl.SDL_Init(Sdl.SDL_INIT_VIDEO);
				Sdl.SDL_WM_SetCaption("Just some SDL test...", "");
				IntPtr surfacePtr = Sdl.SDL_SetVideoMode(width, height, bpp, flags);
				Sdl.SDL_Rect rect2 =
					new Sdl.SDL_Rect(0,0, (short) width, (short) height);
				Sdl.SDL_SetClipRect(surfacePtr, ref rect2);
				Random rand = new Random();
				Sdl.SDL_Event evt;
				while (quitFlag == false) {
					while (0 != Sdl.SDL_PollEvent(out evt)) {
						if (evt.type == Sdl.SDL_QUIT) {
							quitFlag = true;
						}
						else if (evt.type == Sdl.SDL_KEYDOWN) {
							if ((evt.key.keysym.sym == (int)Sdl.SDLK_ESCAPE) ||
								(evt.key.keysym.sym == (int)Sdl.SDLK_q)) {
								quitFlag = true;
							}
						}
					}
					SdlGfx.filledCircleRGBA(
						surfacePtr,
						(short)rand.Next(10,width - 100),
						(short)rand.Next(10, height - 100),
						(short)rand.Next(10,100),
						(byte)rand.Next(255),
						(byte)rand.Next(255),
						(byte)rand.Next(255),
						(byte)rand.Next(255)
					);
					Sdl.SDL_Flip(surfacePtr);
					Thread.Sleep(100);
				}
			}
			finally {
				Sdl.SDL_Quit();
			}
		}

		public static void Main(string[] args) {
			new Maria().Run();
		}
	}
}

