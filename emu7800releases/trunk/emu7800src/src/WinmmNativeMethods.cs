/*
 * WinmmNativeMethods.cs
 * 
 * .NET interface to the Windows Multimedia Library
 * 
 * Copyright (c) 2006 Mike Murphy
 * 
 */
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace EMU7800
{
	internal unsafe sealed class WinmmNativeMethods
	{
		static IntPtr Hwo;
		static IntPtr Storage;
		static int StorageSize;
		static int SoundFrameSize;
		static int QueueLen;

		const ushort WAVE_FORMAT_PCM = 1;				// PCM format for wFormatTag of WAVEFORMAT
		const uint WHDR_DONE = 0x00000001;				// done bit for dwFlags of WAVEHDR
		const uint WAVE_MAPPER = unchecked((uint)-1);	// device ID for wave device mapper
		const uint CALLBACK_NULL = 0;					// no callback for waveOutOpen

		[StructLayout(LayoutKind.Sequential)]
		struct WAVEFORMATEX
		{
			internal ushort wFormatTag;		// format type
			internal ushort nChannels;		// number of channels (i.e. mono, stereo...)
			internal uint nSamplesPerSec;	// sample rate
			internal uint nAvgBytesPerSec;	// for buffer estimation
			internal ushort nBlockAlign;	// block size of data
			internal ushort wBitsPerSample;	// number of bits per sample of mono data
			internal ushort cbSize;			// the count in bytes of the size of extra information (after cbSize)
		}

		[StructLayout(LayoutKind.Sequential)]
		struct WAVEHDR
		{
			internal byte* lpData;			// pointer to locked data buffer
			internal uint dwBufferLength;	// length of data buffer
			internal uint dwBytesRecorded;	// used for input only
			internal uint* dwUser;			// for client's use
			internal uint dwFlags;			// assorted flags (see defines)
			internal uint dwLoops;			// loop control counter
			internal WAVEHDR* lpNext;		// reserved for driver
			internal uint* reserved;		// reserved for driver
		}

		internal static int Open(int freq, int soundFrameSize, int queueLen)
		{
			QueueLen = queueLen & 0x3f;
			if (QueueLen < 2)
			{
				QueueLen = 2;
			}
			
			WAVEFORMATEX wfx;
			wfx.wFormatTag = WAVE_FORMAT_PCM;
			wfx.nChannels = 1;
			wfx.wBitsPerSample = 8;
			wfx.nSamplesPerSec = (uint)freq;
			wfx.nAvgBytesPerSec = (uint)freq;
			wfx.nBlockAlign = 1;
			wfx.cbSize = 0;

			SoundFrameSize = soundFrameSize;
			StorageSize = QueueLen * (sizeof(WAVEHDR) + SoundFrameSize);
			Storage = Marshal.AllocHGlobal(StorageSize);

			byte* ptr = (byte*)Storage;
			for (int i=0; i < StorageSize; i++)
			{
				ptr[i] = 0;
			}

			int mmResult;

			fixed (IntPtr* phwo = &Hwo)
			{
				mmResult = (int)waveOutOpen(phwo, WAVE_MAPPER, &wfx, CALLBACK_NULL, 0, 0);
			}

			if (mmResult != 0)
			{
				Marshal.FreeHGlobal(Storage);
			}

			return mmResult;
		}

		internal static uint SetVolume(int left, int right)
		{
			uint uLeft = (uint)left;
			uint uRight = (uint)right;
			uint nVolume = (uLeft & 0xffff) | ((uRight & 0xffff) << 16);
			return waveOutSetVolume(Hwo, nVolume);
		}

		internal static uint GetVolume()
		{
			uint nVolume;
			waveOutGetVolume(Hwo, &nVolume);
			return nVolume;
		}

		internal static int Enqueue(byte[] stream)
		{
			if (stream.Length != SoundFrameSize) {
				return -1;  // bad enqueue request
			}

			byte* ptr = (byte*)Storage;

			for (int i=0; i < QueueLen; i++)
			{
				WAVEHDR* WaveHdr = (WAVEHDR*)ptr;
				if (WaveHdr->dwFlags == 0 || (WaveHdr->dwFlags & WHDR_DONE) != 0)
				{
					waveOutUnprepareHeader(Hwo, WaveHdr, (uint)sizeof(WAVEHDR));
					WaveHdr->dwBufferLength = (uint)SoundFrameSize;
					WaveHdr->dwFlags = 0;
					WaveHdr->lpData = ptr + sizeof(WAVEHDR);
					for (int j=0; j < SoundFrameSize; j++)
					{
						WaveHdr->lpData[j] = stream[j];
					}
					waveOutPrepareHeader(Hwo, WaveHdr, (uint)sizeof(WAVEHDR));
					waveOutWrite(Hwo, WaveHdr, (uint)sizeof(WAVEHDR));
					return 0;  // enqueue request complete
				}
				ptr += sizeof(WAVEHDR);
				ptr += SoundFrameSize;
			}

			return 1;  // full queue
		}

		internal static void Close()
		{
			waveOutClose(Hwo);
			Marshal.FreeHGlobal(Storage);
		}

		private WinmmNativeMethods() { }

		[DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
		static extern uint waveOutOpen(IntPtr* phwo, uint uDeviceID, WAVEFORMATEX* pwfx, uint dwcb, uint dwInstance, uint dwfOpen);

		[DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
		static extern uint waveOutSetVolume(IntPtr hwo, uint dwVolume);

		[DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
		static extern uint waveOutGetVolume(IntPtr hwo, uint* pdwVolume);

		[DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
		static extern uint waveOutPrepareHeader(IntPtr hwo, WAVEHDR* wh, uint cbwh);

		[DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
		static extern uint waveOutUnprepareHeader(IntPtr hwo, WAVEHDR* wh, uint cbwh);

		[DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
		static extern uint waveOutWrite(IntPtr hwo, WAVEHDR* wh, uint cbwh);

		[DllImport("winmm.dll"), SuppressUnmanagedCodeSecurity]
		static extern uint waveOutClose(IntPtr hwo);
	}
}