/*
 * Class containing the input state of the console and its controllers,
 * mapping emulator input devices to external input.
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
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;

namespace Maria.Core {

	public enum ConsoleSwitch {
		GameReset,
		GameSelect,
		GameBW,
		LDifficultyA,
		RDifficultyA
	}

	public enum ControllerAction {
		Up,
		Down,
		Left,
		Right,
		Trigger,
		Trigger2,
		Keypad1,
		Keypad2,
		Keypad3,
		Keypad4,
		Keypad5,
		Keypad6,
		Keypad7,
		Keypad8,
		Keypad9,
		KeypadA,
		Keypad0,
		KeypadP
	}

	[Serializable]
	public class InputAdapter : IDeserializationCallback {
		public const int PADDLEOHM_MIN = 100000;
		public const int PADDLEOHM_MAX = 800000;
		
		// Indexed by device number
		public Controller[] Controllers = new Controller[2];

		protected int[] ControllerActions = new int[4];
		protected int[] Ohms = new int[4];
		protected int[] LightgunPos = new int[4];
		protected byte ConsoleSwitches;
		protected byte KeypadState;
		protected byte[] DrivingCounters = new byte[2];
		protected bool InputChanged;
		protected bool IgnoreInput;

		public InputAdapter() {
			IgnoreInput = false;
		}

		public bool this[ConsoleSwitch conSwitch] {
			get {
				return (ConsoleSwitches & (1 << (int)conSwitch)) != 0;
			}
			set {
				if (!IgnoreInput) {
					if (value) {
						ConsoleSwitches |= (byte)(1 << (byte)conSwitch);
					}
					else {
						ConsoleSwitches &= (byte)~(1 << (byte)conSwitch);
					}
					InputChanged = true;
				}
			}
		}

		public bool this[int playerno, ControllerAction action] {
			get {
				return (ControllerActions[playerno] & (1 << (int)action)) != 0;
			}
			set {
				if (!IgnoreInput) {
					if (value) {
						ControllerActions[playerno] |= (1 << (int)action);
					}
					else {
						ControllerActions[playerno] &= ~(1 << (int)action);
					}
					InputChanged = true;
				}
			}
		}

		public void SetOhms(int playerno, int ohms) {
			if (!IgnoreInput) {
				if (playerno < 4) {
					Ohms[playerno] = ohms;
					InputChanged = true;
				}
			}
		}

		public void SetLightgunPos(int playerno, int scanline, int hpos) {
			if (!IgnoreInput) {
				if (playerno < 2) {
					LightgunPos[playerno << 1] = scanline;
					LightgunPos[(playerno << 1) + 1] = hpos;
					InputChanged = true;
				}
			}
		}

		//	PortA/TIA: Controller Jacks
		//
		//            Left Jack                Right Jack
		//          -------------             -------------
		//          \ 1 2 3 4 5 /             \ 1 2 3 4 5 /
		//           \ 6 7 8 9 /               \ 6 7 8 9 /
		//            ---------                 ---------
		//  
		//	pin 1   D4 PIA SWCHA           D0 PIA SWCHA
		//	pin 2   D5 PIA SWCHA           D1 PIA SWCHA
		//	pin 3   D6 PIA SWCHA           D2 PIA SWCHA
		//	pin 4   D7 PIA SWCHA           D3 PIA SWCHA
		//	pin 5   D7 TIA INPT1 (Dumped)  D7 TIA INPT3 (Dumped)
		//	pin 6   D7 TIA INPT4 (Latched) D7 TIA INPT5 (Latched)
		//	pin 7   +5                     +5
		//	pin 8   GND                    GND
		//	pin 9   D7 TIA INPT0 (Dumped)  D7 TIA INPT2 (Dumped)
		//
		public byte ReadPortA() {
			int porta = 0;

			switch (Controllers[0]) {
				case Controller.Joystick:
				case Controller.ProLineJoystick:
				case Controller.BoosterGrip:
					porta |= this[0, ControllerAction.Up] ? 0 : (1 << 4);
					porta |= this[0, ControllerAction.Down] ? 0 : (1 << 5);
					porta |= this[0, ControllerAction.Left] ? 0 : (1 << 6);
					porta |= this[0, ControllerAction.Right] ? 0 : (1 << 7);
					break;
				case Controller.Driving:
					porta |= GetDrivingState(0) << 4;
					break;
				case Controller.Paddles:
					porta |= this[0, ControllerAction.Trigger] ? 0 : (1 << 7);
					porta |= this[1, ControllerAction.Trigger] ? 0 : (1 << 6);
					break;
				case Controller.Lightgun:
					porta |= this[0, ControllerAction.Trigger] ? 0 : (1 << 4);
					break;
			}

			switch (Controllers[1]) {
				case Controller.Joystick:
				case Controller.ProLineJoystick:
				case Controller.BoosterGrip:
					porta |= this[1, ControllerAction.Up] ? 0 : (1 << 0);
					porta |= this[1, ControllerAction.Down] ? 0 : (1 << 1);
					porta |= this[1, ControllerAction.Left] ? 0 : (1 << 2);
					porta |= this[1, ControllerAction.Right] ? 0 : (1 << 3);
					break;
				case Controller.Driving:
					porta |= GetDrivingState(1);
					break;
				case Controller.Paddles:
					porta |= this[2, ControllerAction.Trigger] ? 0 : (1 << 3);
					porta |= this[3, ControllerAction.Trigger] ? 0 : (1 << 2);
					break;
				case Controller.Lightgun:
					porta |= this[1, ControllerAction.Trigger] ? 0 : (1 << 0);
					break;
			}
			return (byte) porta;
		}

		public void WritePortA(byte porta) {
			KeypadState = porta;
		}

		//	PortB: Console Switches
		//   
		//	D0 PIA SWCHB  Game Reset 0=on
		//	D1 PIA SWCHB  Game Select 0=on
		//	D2 (unused)
		//	D3 PIA SWCHB  Console Color 1=Color, 0=B/W
		//	D4 (unused)
		//	D5 (unused)
		//	D6 PIA SWCHB  Left Difficulty A 1=A (pro), 0=B (novice)
		//	D7 PIA SWCHB  Right Difficulty A 1=A (pro), 0=B (novice)  
		//
		public byte ReadPortB() {
			int portb = 0;
			portb |= this[ConsoleSwitch.GameReset] ? 0 : (1 << 0);
			portb |= this[ConsoleSwitch.GameSelect] ? 0 : (1 << 1);
			portb |= this[ConsoleSwitch.GameBW] ? 0 : (1 << 3);
			portb |= this[ConsoleSwitch.LDifficultyA] ? (1 << 6) : 0;
			portb |= this[ConsoleSwitch.RDifficultyA] ? (1 << 7) : 0;
			return (byte) portb;
		}

		public int ReadINPT0() { return INPTDumped(0); }
		public int ReadINPT1() { return INPTDumped(1); }
		public int ReadINPT2() { return INPTDumped(2); }
		public int ReadINPT3() { return INPTDumped(3); }
		public bool ReadINPT4(int scanline, int hpos) { return INPTLatched(4, scanline, hpos); }
		public bool ReadINPT5(int scanline, int hpos) { return INPTLatched(5, scanline, hpos); }

		public override string ToString() {
			return "InputAdapter";
		}

		public void ClearAllInput() {
			ConsoleSwitches = 0;
			KeypadState = 0;
			for (int i = 0; i < 4; i++) {
				ControllerActions[i] = 0;
				Ohms[i] = 0;
				LightgunPos[i] = 0;
				DrivingCounters[i / 2] = 0;
			}
			InputChanged = true;
		}

		public virtual void OnDeserialization(object sender) {
			ClearAllInput();
		}

		public void Reset() {
			ClearAllInput();
		}

		public void CheckPoint(long frameNumber) {
			DoCheckPoint(frameNumber);
			InputChanged = false;
		}

		protected virtual void DoCheckPoint(long frameNumber) {}

		int INPTDumped(int inpt) {
			int val = Int32.MaxValue;

			// controller = inpt/2: left=0, right=1
			switch (Controllers[inpt >> 1]) {
				case Controller.Paddles:
					// playerno = inpt
					val = Ohms[inpt];
					break;
				case Controller.ProLineJoystick:
					// playerno = inpt/2
					switch (inpt) {
						case 0:
							val = this[0, ControllerAction.Trigger] ? 0 : Int32.MaxValue;
							break;
						case 1:
							val = this[0, ControllerAction.Trigger2] ? 0 : Int32.MaxValue;
							break;
						case 2:
							val = this[1, ControllerAction.Trigger] ? 0 : Int32.MaxValue;
							break;
						case 3:
							val = this[1, ControllerAction.Trigger2] ? 0 : Int32.MaxValue;
							break;
					}
					break;
				case Controller.BoosterGrip:
					// playerno = inpt
					val = this[inpt, ControllerAction.Trigger2] ? 0 : Int32.MaxValue;
					break;
				case Controller.Keypad:
					val = GetKeypadStateDumped(inpt);
					break;
			}
			return val;
		}

		bool INPTLatched(int inpt, int scanline, int hpos) {
			int playerno = inpt - 4;
			bool val = false;

			switch (Controllers[playerno]) {
				case Controller.Joystick:
				case Controller.ProLineJoystick:
				case Controller.Driving:
				case Controller.BoosterGrip:
					val = this[playerno, ControllerAction.Trigger];
					break;
				case Controller.Keypad:
					val = GetKeypadStateLatched(playerno);
					break;
				case Controller.Lightgun:
					if (scanline >= LightgunPos[playerno << 1]
						&& hpos >= LightgunPos[(playerno << 1) + 1])
						val = true;
					break;
			}
			return val;
		}

		byte[] rotGrayCodes = new byte[] { 0x0f, 0x0d, 0x0c, 0x0e };

		byte GetDrivingState(int playerno) {
			if (this[playerno, ControllerAction.Left]) {
				DrivingCounters[playerno]--;
			}
			if (this[playerno, ControllerAction.Right]) {
				DrivingCounters[playerno]++;
			}
			return rotGrayCodes[(DrivingCounters[playerno] / 20) & 0x03];
		}

		bool GetKeypadStateLatched(int playerno) {
			ControllerAction action;

			if ((KeypadState & 0x01) == 0) {
				action = ControllerAction.Keypad3;
			}
			else if ((KeypadState & 0x02) == 0) {
				action = ControllerAction.Keypad6;
			}
			else if ((KeypadState & 0x04) == 0) {
				action = ControllerAction.Keypad9;
			}
			else if ((KeypadState & 0x08) == 0) {
				action = ControllerAction.KeypadP;
			}
			else {
				return false;
			}
			return this[playerno, action];
		}
		
		int GetKeypadStateDumped(int inpt) {
			ControllerAction action;

			switch (inpt) {
				case 0:
				case 2:
					if ((KeypadState & 0x01) == 0) {
						action = ControllerAction.Keypad1;
					}
					else if ((KeypadState & 0x02) == 0) {
						action = ControllerAction.Keypad4;
					}
					else if ((KeypadState & 0x04) == 0) {
						action = ControllerAction.Keypad7;
					}
					else if ((KeypadState & 0x08) == 0) {
						action = ControllerAction.KeypadA;
					}
					else {
						return Int32.MaxValue;
					}
					break;
				case 1:
				case 3:
					if ((KeypadState & 0x01) == 0) {
						action = ControllerAction.Keypad2;
					}
					else if ((KeypadState & 0x02) == 0) {
						action = ControllerAction.Keypad5;
					}
					else if ((KeypadState & 0x04) == 0) {
						action = ControllerAction.Keypad8;
					}
					else if ((KeypadState & 0x08) == 0) {
						action = ControllerAction.Keypad0;
					}
					else {
						return Int32.MaxValue;
					}
					break;
				default:
					return Int32.MaxValue;
			}
			// playerno = inpt/2
			return this[inpt >> 1, action] ? Int32.MaxValue : 0;
		}		
	}
}

/*namespace EMU7800
{

	/*
	 * PlaybackInputAdapter
	 * 
	 * An InputAdapter extension providing input playback support 
	 * 
	 * FIXME: Lightgun needs to be included
	 * 
	 */
/*	public delegate void PlaybackEofListener();

	public class PlaybackInputAdapter : InputAdapter
	{
		public event PlaybackEofListener EofListeners;

		string _CartMD5;
		public string CartMD5
		{
			get
			{
				return _CartMD5;
			}
		}

		long NextPlaybackFrameNumber;
		BinaryReader BR;

		public override string ToString()
		{
			return "PlaybackInputAdapter";
		}

		void LogPlaybackStatusMsg(string msg)
		{
			Trace.Write(this);
			Trace.Write(": ");
			Trace.WriteLine(msg);
		}

		protected override void DoCheckPoint(long frameNumber)
		{
			if (BR != null)
			{
				try
				{
					while (NextPlaybackFrameNumber <= frameNumber)
					{
						ConsoleSwitches = BR.ReadByte();
						KeypadState = BR.ReadByte();
						for (int i = 0; i < 4; i++)
						{
							ControllerActions[i] = BR.ReadInt32();
						}
						for (int i = 0; i < 4; i++)
						{
							Ohms[i] = BR.ReadInt32();
						}
						for (int i = 0; i < 2; i++)
						{
							DrivingCounters[i] = BR.ReadByte();
						}
						NextPlaybackFrameNumber = BR.ReadInt64();
					}
				}
				catch (EndOfStreamException)
				{
					StopPlayback();
					if (EofListeners != null)
					{
						EofListeners();
					}
				}
				catch (Exception ex)
				{
					LogPlaybackStatusMsg(ex.Message);
					StopPlayback();
				}
			}
		}

		public void StopPlayback()
		{
			if (BR != null)
			{
				BR.Close();
				BR = null;
				IgnoreInput = false;
				ClearAllInput();
				LogPlaybackStatusMsg("Stopped");
			}
		}

		public PlaybackInputAdapter(string fn)
		{
			IgnoreInput = true;
			try
			{
				BR = new BinaryReader(File.OpenRead(fn));

				string fver = BR.ReadString();
				if (fver != "1")
				{
					throw new Exception("bad file format");
				}

				_CartMD5 = BR.ReadString();

				if (BR.ReadString() != "EMU7800")
				{
					throw new Exception("bad file format");
				}

				NextPlaybackFrameNumber = BR.ReadInt64();
				LogPlaybackStatusMsg("Started");
			}
			catch (Exception ex)
			{
				LogPlaybackStatusMsg(ex.Message);
				StopPlayback();
			}
		}
	}

	/*
	 * RecordInputAdapter
	 * 
	 * An InputAdapter extension providing input recording support 
	 * 
	 * FIXME: Lightgun needs to be included
	 * 
	 */
/*	public class RecordInputAdapter : InputAdapter
	{
		BinaryWriter BW;

		public override string ToString()
		{
			return "RecordInputAdapter";
		}

		void LogRecordingStatusMsg(string msg)
		{
			Trace.Write(this);
			Trace.Write(": ");
			Trace.WriteLine(msg);
		}

		protected override void DoCheckPoint(long frameNumber)
		{
			if (BW != null && InputChanged)
			{
				try
				{
					BW.Write(frameNumber);
					BW.Write(ConsoleSwitches);
					BW.Write(KeypadState);
					for (int i = 0; i < 4; i++)
					{
						BW.Write(ControllerActions[i]);
					}
					for (int i = 0; i < 4; i++)
					{
						BW.Write(Ohms[i]);
					}
					for (int i = 0; i < 2; i++)
					{
						BW.Write(DrivingCounters[i]);
					}
				}
				catch (Exception ex)
				{
					LogRecordingStatusMsg(ex.Message);
					StopRecording();
				}
			}
		}

		public void StopRecording()
		{
			if (BW != null)
			{
				BW.Flush();
				BW.Close();
				BW = null;
				LogRecordingStatusMsg("Stopped");
			}
		}

		public RecordInputAdapter(string fn, string cartMD5)
		{
			try
			{
				BW = new BinaryWriter(File.Create(fn));
				BW.Write("1");
				BW.Write(cartMD5);
				BW.Write("EMU7800");
				BW.Flush();
				LogRecordingStatusMsg("Started");
			}
			catch (Exception ex)
			{
				LogRecordingStatusMsg(ex.Message);
				StopRecording();
			}
		}
	}
}
*/
