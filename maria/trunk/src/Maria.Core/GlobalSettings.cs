/*
 * Configuration settings.
 *
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

namespace Maria.Core {
	// TODO : initialized with hardcoded (bogus) values, later should
	// get settings from some (system/user) config file
	public class GlobalSettings {
		private int soundVolume;
		private bool use7800HSC;
		private string romDirectory;
		private string outputDirectory;
		private bool skip7800BIOS;
		private int frameRateAdjust;
		public bool deactivateMouseInput;

		public GlobalSettings() {
			SoundVolume = 8;
			Use7800HSC = false;
			ROMDirectory = ".";
			outputDirectory = ".";
			Skip7800BIOS = false;
			FrameRateAdjust = 0;
			DeactivateMouseInput = false;
		}

		public int SoundVolume {
			get { return soundVolume; }
			set { soundVolume = value; }
		}

		public bool Use7800HSC {
			get { return use7800HSC; }
			set { use7800HSC = value; }
		}

		public string ROMDirectory {
			get { return romDirectory; }
			set { romDirectory = value; }
		}

		public string OutputDirectory {
			get { return outputDirectory; }
		}

		public bool Skip7800BIOS {
			get { return skip7800BIOS; }
			set { skip7800BIOS = value; }
		}

		public int FrameRateAdjust {
			get { return frameRateAdjust; }
			set { frameRateAdjust = value; }
		}

		public bool DeactivateMouseInput {
			get { return deactivateMouseInput; }
			set { deactivateMouseInput = value; }
		}
	}
}
