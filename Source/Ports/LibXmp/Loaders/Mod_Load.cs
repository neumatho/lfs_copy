﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.IO;
using System.Text;
using Polycode.NostalgicPlayer.Kit;
using Polycode.NostalgicPlayer.Kit.Utility;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Format;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Loader;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Loaders
{
	/// <summary>
	/// 
	/// </summary>
	internal class Mod_Load : IFormatLoader
	{
		#region Internal structures

		#region Mod_Header
		private class Mod_Header
		{
			public uint8[] Name = new uint8[20];
			public Mod_Instrument[] Ins = ArrayHelper.InitializeArray<Mod_Instrument>(31);
			public uint8 Len;
			public uint8 Restart;						// Number of patterns in Soundtracker, restart in Noisetracker/Startrekker, 0x7f in Protracker
			public uint8[] Order = new uint8[128];
			public uint8[] Magic = new uint8[4];
		}
		#endregion

		#region Mod_Instrument
		private class Mod_Instrument
		{
			public uint8[] Name = new uint8[22];		// Instrument name
			public uint16 Size;							// Sample length in 16-bit words
			public int8 FineTune;						// Finetune (signed nibble)
			public int8 Volume;							// Linear playback volume
			public uint16 Loop_Start;					// Loop start in 16-bit words
			public uint16 Loop_Size;					// Loop length in 16-bit words
		}
		#endregion

		#endregion

		private enum ExternalFormat
		{
			Unknown,
			FastTracker,
			TakeTracker,

			TestOnly
		}

		private enum InternalFormat
		{
			NotRecognized,
			ProTracker,
			NoiseTracker,
			SoundTracker,
			FastTracker,
			FastTracker2,
			Octalyser,
			TakeTracker,
			DigitalTracker,
			Flextrax,
			ModsGrave,
			ScreamTracker3,
			OpenMpt,
			Unknown_Conv,
			ConvertedSt,
			Converted,
			Clone,
			Unknown,

			Probably_NoiseTracker
		}

		private class Mod_Magic
		{
			/********************************************************************/
			/// <summary>
			/// Constructor
			/// </summary>
			/********************************************************************/
			public Mod_Magic(uint32 magic, bool flag, InternalFormat id, c_int ch)
			{
				Magic = magic;
				Flag = flag;
				Id = id;
				Ch = ch;
			}

			public uint32 Magic;
			public bool Flag;
			public InternalFormat Id;
			public c_int Ch;
		}

		private static readonly Mod_Magic[] mod_Magic =
		{
			new Mod_Magic(Common.Magic4("M.K."), false, InternalFormat.ProTracker, 4),
			new Mod_Magic(Common.Magic4("6CHN"), false, InternalFormat.FastTracker, 6),
			new Mod_Magic(Common.Magic4("8CHN"), false, InternalFormat.FastTracker, 8),
			new Mod_Magic(Common.Magic4("CD61"), true, InternalFormat.Octalyser, 6),		// Atari STe/Falcon
			new Mod_Magic(Common.Magic4("CD81"), true, InternalFormat.Octalyser, 8),		// Atari STe/Falcon
			new Mod_Magic(Common.Magic4("TDZ1"), true, InternalFormat.TakeTracker, 1),		// TakeTracker 1ch
			new Mod_Magic(Common.Magic4("TDZ2"), true, InternalFormat.TakeTracker, 2),		// TakeTracker 2ch
			new Mod_Magic(Common.Magic4("TDZ3"), true, InternalFormat.TakeTracker, 3),		// TakeTracker 3ch
			new Mod_Magic(Common.Magic4("TDZ4"), true, InternalFormat.TakeTracker, 4),		// TakeTracker 4ch
			new Mod_Magic(Common.Magic4("FA04"), true, InternalFormat.DigitalTracker, 4),	// Atari Falcon
			new Mod_Magic(Common.Magic4("FA06"), true, InternalFormat.DigitalTracker, 6),	// Atari Falcon
			new Mod_Magic(Common.Magic4("FA08"), true, InternalFormat.DigitalTracker, 8)	// Atari Falcon
		};

		private readonly ExternalFormat format;
		private readonly LibXmp lib;

		private InternalFormat tracker_Id;
		private c_int channels;
		private uint8 restartPosition;
		private bool needs_Timing_Detection;
		private bool out_Of_Range;
		private bool mkMark;

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Fast = new Format_Loader
		{
			Id = Guid.Parse("622FC871-244D-4E46-9423-35609EDFCF48"),
			Name = "FastTracker",
			Description = "This tracker is from the PC, but uses the same file format as the other mod trackers. It supports up to 8 channels, but MOD files saved with FastTracker 2 can have up to 32 channels. The tracker was written by Fredrik Muss.",
			Create = Create_FastTracker
		};

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_Take = new Format_Loader
		{
			Id = Guid.Parse("73128541-1DF2-46DE-93FF-614C2D1ECB18"),
			Name = "TakeTracker",
			Description = "This tracker is from the PC, but uses the same file format as FastTracker MOD files. It has extended the number of channels up to 16 and it also supports an odd number of channels. The tracker was written by Anders B. Ervik.",
			Create = Create_TakeTracker
		};

		/// <summary></summary>
		public static readonly Format_Loader LibXmp_Loader_TestOnly = new Format_Loader
		{
			Id = Guid.Parse("0D3538F7-BF9F-484E-967F-9E84C92DE010"),
			Name = "Test only",
			OnlyAvailableInTest = true,
			Create = Create_TestOnly
		};

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		private Mod_Load(LibXmp libXmp, ExternalFormat format)
		{
			this.format = format;
			lib = libXmp;
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create_FastTracker(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Mod_Load(libXmp, ExternalFormat.FastTracker);
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create_TakeTracker(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Mod_Load(libXmp, ExternalFormat.TakeTracker);
		}



		/********************************************************************/
		/// <summary>
		/// Create a new instance of the loader
		/// </summary>
		/********************************************************************/
		private static IFormatLoader Create_TestOnly(LibXmp libXmp, Xmp_Context ctx)
		{
			return new Mod_Load(libXmp, ExternalFormat.TestOnly);
		}



		/********************************************************************/
		/// <summary>
		/// The test method has been rewritten based on the original code in
		/// LibXmp. It combines what is in the original test and loader
		/// functions, so it is capable to detect exactly which format the
		/// module is
		/// </summary>
		/********************************************************************/
		public c_int Test(Hio f, out string t, c_int start)
		{
			c_int i;

			t = null;

			byte[] buf = new byte[4];

			f.Hio_Seek(start + 1080, SeekOrigin.Begin);
			if (f.Hio_Read(buf, 1, 4) < 4)
				return -1;

			if ((buf[2] == 'C') && (buf[3] == 'H') && char.IsDigit((char)buf[0]) && char.IsDigit((char)buf[1]))
			{
				i = (buf[0] - '0') * 10 + buf[1] - '0';
				if ((i > 0) && (i <= 32))
					goto Found;
			}

			if ((buf[1] == 'C') && (buf[2] == 'H') && (buf[3] == 'N') && char.IsDigit((char)buf[0]))
			{
				if ((buf[0] - '0') != 0)
					goto Found;
			}

			uint32 magic = Common.Magic4(buf[0], buf[1], buf[2], buf[3]);

			for (i = 0; i < mod_Magic.Length; i++)
			{
				if (mod_Magic[i].Magic == magic)
					break;
			}

			if (i >= mod_Magic.Length)
				return -1;

			bool detected = mod_Magic[i].Flag;

			// Sanity check to prevent loading NoiseRunner and other module
			// formats with valid magic at offset 1080 (e.g. His Master's Noise)
			f.Hio_Seek(start + 20, SeekOrigin.Begin);

			for (i = 0; i < 31; i++)
			{
				f.Hio_Seek(22, SeekOrigin.Current);		// Instrument name

				// OpenMPT can create mods with large samples
				f.Hio_Read16B();	// Sample size

				// Chris Spiegel tells me that sandman.mod has 0x20 in finetune
				uint8 x = f.Hio_Read8();
				if (((x & 0xf0) != 0) && (x != 0x20))	// Test finetune
					return -1;

				if (f.Hio_Read8() > 0x40)		// Test volume
					return -1;

				f.Hio_Read16B();		// Loop start
				f.Hio_Read16B();		// Loop size
			}

			// The following checks are only relevant for filtering out atypical
			// M.K. variants. If the magic is from a recognizable source, skip them
			if (detected)
				goto Found;

			// Test for UNIC tracker modules
			//
			// From Gryzor's Pro-Wizard PW_FORMATS-Engl.guide:
			// ``The UNIC format is very similar to Protracker... At least in the
			// heading... same length : 1084 bytes. Even the "M.K." is present,
			// sometimes !! Maybe to disturb the rippers.. hehe but Pro-Wizard
			// doesn't test this only!''

			// Get file size
			c_long size = f.Hio_Size();
			c_int smp_Size = 0;

			f.Hio_Seek(start + 20, SeekOrigin.Begin);

			// Get sample sizes
			for (i = 0; i < 31; i++)
			{
				f.Hio_Seek(22, SeekOrigin.Current);
				smp_Size += 2 * f.Hio_Read16B();	// Length in 16-bit words
				f.Hio_Seek(6, SeekOrigin.Current);
			}

			// Get number of patterns
			c_int num_Pat = 0;

			f.Hio_Seek(start + 952, SeekOrigin.Begin);

			for (i = 0; i < 128; i++)
			{
				uint8 x = f.Hio_Read8();
				if (x > 0x7f)
					break;

				if (x > num_Pat)
					num_Pat = x;
			}

			num_Pat++;

			// See if module size matches UNIC
			if ((start + 1084 + num_Pat * 0x300 + smp_Size) == size)
				return -1;

			// Validate pattern data in an attempt to catch UNICs with MOD size
			uint8[] pat_Buf = new uint8[1024];
			c_int count;

			for (count = i = 0; i < num_Pat; i++)
			{
				f.Hio_Seek(start + 1084 + 1024 * i, SeekOrigin.Begin);

				if (f.Hio_Read(pat_Buf, 1024, 1) == 0)
					return -1;

				if (Validate_Pattern(pat_Buf) < 0)
				{
					// Allow a few errors, "lexstacy" has 0x52
					count++;
				}
			}

			if (count > 2)
				return -1;

			Found:
			f.Hio_Seek(start + 0, SeekOrigin.Begin);
			lib.common.LibXmp_Read_Title(f, out t, 20, EncoderCollection.Dos);

			return FindFormat(f, start) == format ? 0 : -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int Loader(Module_Data m, Hio f, c_int start)
		{
			Xmp_Module mod = m.Mod;
			Mod_Header mh = new Mod_Header();
			uint8[] magic = new uint8[8];
			uint8[] pat_High_Fxx = new uint8[256];
			out_Of_Range = false;
			bool sameRow_Fxx = false;	// Speed + BPM set on same row
			bool high_Fxx = false;		// High Fxx is used anywhere
			bool ptkLoop = false;		// Protracker loop

			Encoding encoder = (tracker_Id == InternalFormat.Octalyser) || (tracker_Id == InternalFormat.DigitalTracker) ? EncoderCollection.Atari :
								(tracker_Id <= InternalFormat.SoundTracker) ? EncoderCollection.Amiga : EncoderCollection.Dos;

			mod.Ins = 31;
			mod.Smp = mod.Ins;
			mod.Chn = channels;
			m.Period_Type = Containers.Common.Period.ModRng;

			f.Hio_Read(mh.Name, 20, 1);

			for (c_int i = 0; i < 31; i++)
			{
				f.Hio_Read(mh.Ins[i].Name, 22, 1);		// Instrument name
				mh.Ins[i].Size = f.Hio_Read16B();					// Length in 16-bit words
				mh.Ins[i].FineTune = f.Hio_Read8S();                // Finetune (signed nibble)
				mh.Ins[i].Volume = f.Hio_Read8S();                  // Linear playback volume
				mh.Ins[i].Loop_Start = f.Hio_Read16B();             // Loop start in 16-bit words
				mh.Ins[i].Loop_Size = f.Hio_Read16B();				// Loop size in 16-bit words
			}

			mh.Len = f.Hio_Read8();
			mh.Restart = f.Hio_Read8();

			f.Hio_Read(mh.Order, 128, 1);
			f.Hio_Read(magic, 1, 4);

			if (f.Hio_Error() != 0)
				return -1;

			// Digital Tracker MODs have an extra four bytes after the magic.
			// These are always 00h 40h 00h 00h and can probably be ignored
			if (tracker_Id == InternalFormat.DigitalTracker)
				f.Hio_Read32B();

			lib.common.LibXmp_Copy_Adjust(out mod.Name, mh.Name, 20, encoder);

			mod.Len = mh.Len;
			mod.Rst = restartPosition;

			Array.Copy(mh.Order, mod.Xxo, 128);

			for (c_int i = 0; i < 128; i++)
			{
				// This fixes dragnet.mod (garbage in the order list)
				if (mod.Xxo[i] > 0x7f)
					break;

				if (mod.Xxo[i] > mod.Pat)
					mod.Pat = mod.Xxo[i];
			}

			mod.Pat++;

			if (lib.common.LibXmp_Init_Instrument(m) < 0)
				return -1;

			for (c_int i = 0; i < mod.Ins; i++)
			{
				if (lib.common.LibXmp_Alloc_SubInstrument(mod, i, 1) < 0)
					return -1;

				Xmp_Instrument xxi = mod.Xxi[i];
				Xmp_SubInstrument sub = xxi.Sub[0];
				Xmp_Sample xxs = mod.Xxs[i];

				xxs.Len = 2 * mh.Ins[i].Size;
				xxs.Lps = 2 * mh.Ins[i].Loop_Start;
				xxs.Lpe = xxs.Lps + 2 * mh.Ins[i].Loop_Size;

				if (xxs.Lpe > xxs.Len)
					xxs.Lpe = xxs.Len;

				xxs.Flg = ((mh.Ins[i].Loop_Size > 1) && (xxs.Lpe >= 4)) ? Xmp_Sample_Flag.Loop : Xmp_Sample_Flag.None;
				sub.Fin = (int8)(mh.Ins[i].FineTune << 4);
				sub.Vol = mh.Ins[i].Volume;
				sub.Pan = 0x80;
				sub.Sid = i;

				lib.common.LibXmp_Instrument_Name(mod, i, mh.Ins[i].Name, 22, encoder);

				if (xxs.Len > 0)
					xxi.Nsm = 1;
			}

			if (mod.Chn >= Constants.Xmp_Max_Channels)
				return -1;

			mod.Trk = mod.Chn * mod.Pat;

			if (lib.common.LibXmp_Init_Pattern(mod) < 0)
				return -1;

			// Load and convert patterns
			uint8[] patBuf = new uint8[64 * 4 * mod.Chn];
			if (patBuf == null)
				return -1;

			for (c_int i = 0; i < mod.Pat; i++)
			{
				if (lib.common.LibXmp_Alloc_Pattern_Tracks(mod, i, 64) < 0)
					return -1;

				if (f.Hio_Read(patBuf, (size_t)(64 * 4 * mod.Chn), 1) < 1)
					return -1;

				c_int mod_Event = 0;

				for (c_int j = 0; j < 64; j++)
				{
					bool speed_Row = false;
					bool bpm_Row = false;

					for (c_int k = 0; k < mod.Chn; k++)
					{
						c_int period = (Ports.LibXmp.Common.Lsn(patBuf[mod_Event]) << 8) | patBuf[mod_Event + 1];
						if ((period != 0) && ((period < 108) || (period > 907)))
							out_Of_Range = true;

						// Needs CIA/VBlank detection?
						if (Ports.LibXmp.Common.Lsn(patBuf[mod_Event + 2]) == 0x0f)
						{
							if (patBuf[mod_Event + 3] >= 0x20)
							{
								pat_High_Fxx[i] = patBuf[mod_Event + 3];
								m.Compare_VBlank = true;
								high_Fxx = true;
								bpm_Row = true;
							}
							else
								speed_Row = true;
						}

						mod_Event += 4;
					}

					if (bpm_Row && speed_Row)
						sameRow_Fxx = true;
				}

				mod_Event = 0;

				for (c_int j = 0; j < 64; j++)
				{
					for (c_int k = 0; k < mod.Chn; k++)
					{
						Xmp_Event @event = Ports.LibXmp.Common.Event(m, i, k, j);

						switch (tracker_Id)
						{
							case InternalFormat.Probably_NoiseTracker:
							case InternalFormat.NoiseTracker:
							{
								lib.common.LibXmp_Decode_NoiseTracker_Event(@event, patBuf, mod_Event);
								break;
							}

							default:
							{
								lib.common.LibXmp_Decode_ProTracker_Event(@event, patBuf, mod_Event);
								break;
							}
						}

						mod_Event += 4;
					}
				}
			}

			// VBlank detection routine.
			// Despite VBlank being dependent on the tracker used, VBlank detection
			// is complex and uses heuristics mostly independent from tracker ID
			if (!needs_Timing_Detection)
			{
				// Noisetracker and some other trackers do not support CIA timing. The
				// only known MOD in the wild that relies on this is muppenkorva.mod
				// by Glue Master (loaded by the His Master's Noise loader)
				if (Tracker_Is_VBlank(tracker_Id))
					m.Quirk |= Quirk_Flag.NoBpm;

				m.Compare_VBlank = false;
			}
			else if (sameRow_Fxx)
			{
				// If low Fxx and high Fxx are on the same row, there's a high chance
				// this is from a CIA-based tracker. There are some exceptions
				//
				// Checks are moved to the test function
				m.Compare_VBlank = false;
			}
			else if (high_Fxx && (mod.Len >= 8))
			{
				// Test for high Fxx at the end only--this is typically VBlank,
				// and is used to add silence to the end of modules.
				//
				// Exception: if the final high Fxx is F7D, this module is either CIA
				// or is VBlank that was modified to play as CIA, so do nothing.
				//
				// TODO: MPT resets modules on the end loop, so some of the very long
				// silent sections in modules affected by this probably expect CIA. It
				// should eventually be possible to detect those
				c_int threshold = mod.Len - 2;
				c_int i;

				for (i = 0; i < threshold; i++)
				{
					if (pat_High_Fxx[mod.Xxo[i]] != 0)
						break;
				}

				if (i == threshold)
				{
					for (i = mod.Len - 1; i >= threshold; i--)
					{
						uint8 fxx = pat_High_Fxx[mod.Xxo[i]];
						if (fxx == 0x00)
							continue;

						if (fxx == 0x7d)
							break;

						m.Compare_VBlank = false;
						m.Quirk |= Quirk_Flag.NoBpm;
						break;
					}
				}
			}

			string tracker;

			switch (tracker_Id)
			{
				case InternalFormat.ProTracker:
				{
					tracker = "Protracker";
					ptkLoop = true;
					break;
				}

				case InternalFormat.Probably_NoiseTracker:
				case InternalFormat.NoiseTracker:
				{
					tracker = "Noisetracker";
					break;
				}

				case InternalFormat.SoundTracker:
				{
					tracker = "Soundtracker";
					break;
				}

				case InternalFormat.FastTracker:
				case InternalFormat.FastTracker2:
				{
					tracker = "Fast Tracker";
					m.Period_Type = Containers.Common.Period.Amiga;
					break;
				}

				case InternalFormat.TakeTracker:
				{
					tracker = "Take Tracker";
					m.Period_Type = Containers.Common.Period.Amiga;
					break;
				}

				case InternalFormat.Octalyser:
				{
					tracker = "Octalyser";
					break;
				}

				case InternalFormat.DigitalTracker:
				{
					tracker = "Digital Tracker";
					break;
				}

				case InternalFormat.Flextrax:
				{
					tracker = "Flextrax";
					break;
				}

				case InternalFormat.ModsGrave:
				{
					tracker = "Mod's Grave";
					break;
				}

				case InternalFormat.ScreamTracker3:
				{
					tracker = "Scream Tracker";
					m.Period_Type = Containers.Common.Period.Amiga;
					break;
				}

				case InternalFormat.ConvertedSt:
				case InternalFormat.Converted:
				{
					tracker = "Converted";
					break;
				}

				case InternalFormat.Clone:
				{
					tracker = "Protracker clone";
					m.Period_Type = Containers.Common.Period.Amiga;
					break;
				}

				case InternalFormat.OpenMpt:
				{
					tracker = "OpenMPT";
					ptkLoop = true;
					break;
				}

				default:
				case InternalFormat.Unknown_Conv:
				case InternalFormat.Unknown:
				{
					tracker = "Unknown tracker";
					m.Period_Type = Containers.Common.Period.Amiga;
					break;
				}
			}

			if (out_Of_Range)
				m.Period_Type = Containers.Common.Period.Amiga;

			if (tracker_Id == InternalFormat.ModsGrave)
				lib.common.LibXmp_Set_Type(m, tracker);
			else
				lib.common.LibXmp_Set_Type(m, string.Format("{0} {1}", tracker, Encoding.ASCII.GetString(magic, 0, 4)));

			// Load samples
			for (c_int i = 0; i < mod.Smp; i++)
			{
				if (mod.Xxs[i].Len == 0)
					continue;

				Sample_Flag flags = ptkLoop && (mod.Xxs[i].Lps == 0) ? Sample_Flag.FullRep : Sample_Flag.None;

				c_long pos = f.Hio_Tell();
				if (pos < 0)
					return -1;

				uint8[] buf = new uint8[5];
				c_int num = (c_int)f.Hio_Read(buf, 1, 5);

				if ((num == 5) && (buf[0] == 'A')  && (buf[1] == 'D') && (buf[2] == 'P') && (buf[3] == 'C') && (buf[4] == 'M'))
					flags |= Sample_Flag.Adpcm;
				else
					f.Hio_Seek(pos, SeekOrigin.Begin);

				if (Sample.LibXmp_Load_Sample(m, f, flags, mod.Xxs[i], null, i) < 0)
					return -1;
			}

			if ((tracker_Id == InternalFormat.ProTracker) || (tracker_Id == InternalFormat.OpenMpt))
				m.Quirk |= Quirk_Flag.ProTrack;
			else if (tracker_Id == InternalFormat.ScreamTracker3)
			{
				m.C4Rate = Constants.C4_Ntsc_Rate;
				m.Quirk |= Quirk_Flag.St3;
				m.Read_Event_Type = Read_Event.St3;
			}
			else if ((tracker_Id == InternalFormat.FastTracker) || (tracker_Id == InternalFormat.FastTracker2) || (tracker_Id == InternalFormat.TakeTracker) || (tracker_Id == InternalFormat.ModsGrave) || (mod.Chn > 4))
			{
				m.C4Rate = Constants.C4_Ntsc_Rate;
				m.Quirk |= Quirk_Flag.Ft2 | Quirk_Flag.FtMod;
				m.Read_Event_Type = Read_Event.Ft2;
				m.Period_Type = Containers.Common.Period.Amiga;
			}

			return 0;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Returns non-zero if the given tracker ONLY supports VBlank
		/// timing. This should be used only when the tracker is known for
		/// sure, e.g. magic match
		/// </summary>
		/********************************************************************/
		private bool Tracker_Is_VBlank(InternalFormat trackerId)
		{
			switch (trackerId)
			{
				case InternalFormat.NoiseTracker:
				case InternalFormat.SoundTracker:
					return true;

				default:
					return false;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private c_int Validate_Pattern(uint8[] buf)
		{
			for (c_int i = 0; i < 64; i++)
			{
				for (c_int j = 0; j < 4; j++)
				{
					c_int d = (i * 4 + j) * 4;

					if ((buf[d] >> 4) > 1)
						return -1;
				}
			}

			return 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Is_St_Ins(string s)
		{
			if (s.Length < 6)
				return false;

			if ((s[0] != 's') && (s[0] != 'S'))
				return false;

			if ((s[1] != 't') && (s[1] != 'T'))
				return false;

			if ((s[2] != '-') || (s[5] != ':'))
				return false;

			if (!char.IsDigit(s[3]) || !char.IsDigit(s[4]))
				return false;

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private InternalFormat Get_Tracker_Id(Hio f, c_int start, InternalFormat id, uint8 restart, c_int pat)
		{
			bool has_Loop_0 = false;
			bool has_Vol_In_Empty_Ins = false;

			uint8[] buffer = new uint8[22];

			Encoding encoding = EncoderCollection.Amiga;
			string[] sample_Names = new string[31];
			uint16[] sample_Sizes = new uint16[31];
			uint16[] sample_Loop_Sizes = new uint16[31];
			uint8[] sample_Volumes = new uint8[31];

			f.Hio_Seek(start + 20, SeekOrigin.Begin);

			for (c_int i = 0; i < 31; i++)
			{
				f.Hio_Read(buffer, 22, 1);
				sample_Names[i] = encoding.GetString(buffer);

				uint16 size = f.Hio_Read16B();
				uint8 volume = (uint8)(f.Hio_Read16B() & 0x00ff);
				f.Hio_Read16B();
				uint16 loop_Size = f.Hio_Read16B();

				// Check if has instruments with loop size 0
				if (loop_Size == 0)
					has_Loop_0 = true;

				// Check if has instruments with size 0 and volume > 0
				if ((size == 0) && volume > 0)
					has_Vol_In_Empty_Ins = true;

				sample_Sizes[i] = size;
				sample_Loop_Sizes[i] = loop_Size;
				sample_Volumes[i] = volume;
			}

			// Test Protracker-like files
			if (restart == pat)
			{
				if (channels == 4)
					id = InternalFormat.SoundTracker;
				else
					id = InternalFormat.Unknown;
			}
			else if (restart == 0x78)
			{
				if (channels == 4)
				{
					// Can't trust this for Noisetracker, MOD.Data City Remix
					// has Protracker effects and Noisetracker restart byte
					id = InternalFormat.Probably_NoiseTracker;
				}
				else
					id = InternalFormat.Unknown;

				return id;
			}
			else if (restart < 0x7f)
			{
				if ((channels == 4) && !has_Vol_In_Empty_Ins)
					id = InternalFormat.NoiseTracker;
				else
					id = InternalFormat.Unknown;	// ?

				restartPosition = restart;
			}
			else if (restart == 0x7f)
			{
				if (channels == 4)
				{
					if (has_Loop_0)
						id = InternalFormat.Clone;
				}
				else
					id = InternalFormat.ScreamTracker3;

				return id;
			}
			else if (restart > 0x7f)
			{
				id = InternalFormat.Unknown;	// ?
				return id;
			}

			if (!has_Loop_0)
			{
				c_int i;

				for (i = 0; i < 31; i++)
				{
					if ((sample_Sizes[i] == 1) && (sample_Volumes[i] == 0))
						return InternalFormat.Converted;
				}

				for (i = 0; i < 31; i++)
				{
					if (Is_St_Ins(sample_Names[i]))
						break;
				}

				if (i == 31)	// No st- instruments
				{
					for (i = 0; i < 31; i++)
					{
						if ((sample_Sizes[i] != 0) || (sample_Loop_Sizes[i] != 1))
							continue;

						switch (channels)
						{
							case 4:
							{
								if (has_Vol_In_Empty_Ins)
									id = InternalFormat.OpenMpt;
								else
								{
									id = InternalFormat.NoiseTracker;
									// or Octalyser
								}
								break;
							}

							case 6:
							case 8:
							{
								id = InternalFormat.Octalyser;
								break;
							}

							default:
							{
								id = InternalFormat.Unknown;
								break;
							}
						}

						return id;
					}

					if (channels == 4)
						id = InternalFormat.ProTracker;
					else if ((channels == 6) || (channels == 8))
					{
						// FastTracker 1.01?
						id = InternalFormat.FastTracker;
					}
					else
						id = InternalFormat.Unknown;
				}
			}
			else	// Has loops with size 0
			{
				c_int i;

				for (i = 15; i < 31; i++)
				{
					// Is the name or size set?
					if (!string.IsNullOrEmpty(sample_Names[i]) || (sample_Sizes[i] > 0))
						break;
				}

				if ((i == 31) && Is_St_Ins(sample_Names[14]))
					return InternalFormat.ConvertedSt;

				// Assume that Fast Tracker modules won't have ST- instruments
				for (i = 0; i < 31; i++)
				{
					if (Is_St_Ins(sample_Names[i]))
						break;
				}

				if (i < 31)
					return InternalFormat.Unknown_Conv;

				if ((channels == 4) || (channels == 6) || (channels == 8))
					return InternalFormat.FastTracker;

				id = InternalFormat.Unknown;		// ?!
			}

			return id;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private ExternalFormat FindFormat(Hio f, c_int start)
		{
			tracker_Id = FindInternalFormat(f, start);

			if (!out_Of_Range && mkMark && !LibXmp.UnitTestMode)
				return ExternalFormat.Unknown;

			switch (tracker_Id)
			{
				case InternalFormat.FastTracker:
				case InternalFormat.FastTracker2:
					return ExternalFormat.FastTracker;

				case InternalFormat.TakeTracker:
					return ExternalFormat.TakeTracker;

				// Converted and unknown are treated as FastTracker
				case InternalFormat.Converted:
				case InternalFormat.ConvertedSt:
				case InternalFormat.Clone:
				case InternalFormat.Unknown:
				case InternalFormat.Unknown_Conv:
					return ExternalFormat.FastTracker;

				case InternalFormat.SoundTracker:
				case InternalFormat.NoiseTracker:
				case InternalFormat.Probably_NoiseTracker:
				case InternalFormat.ProTracker:
				case InternalFormat.ScreamTracker3:
				case InternalFormat.OpenMpt:
					return LibXmp.UnitTestMode ? ExternalFormat.TestOnly : ExternalFormat.Unknown;

				case InternalFormat.Octalyser:
				case InternalFormat.DigitalTracker:
				case InternalFormat.Flextrax:
				case InternalFormat.ModsGrave:
					return ExternalFormat.Unknown;
			}

			return ExternalFormat.Unknown;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private InternalFormat FindInternalFormat(Hio f, c_int start)
		{
			bool detected = false;
			InternalFormat trackerId = InternalFormat.ProTracker;
			bool maybe_Wow = true;
			needs_Timing_Detection = false;
			out_Of_Range = false;
			c_int smp_Size = 0;
			bool has_Big_Samples = false;
			c_long fileSize = f.Hio_Size();

			channels = 0;
			restartPosition = 0;

			f.Hio_Seek(start + 20, SeekOrigin.Begin);

			for (c_int i = 0; i < 31; i++)
			{
				f.Hio_Seek(22, SeekOrigin.Current);

				// Mod's Grave WOW files are converted from 669s and have default
				// finetune and volume
				uint16 size = f.Hio_Read16B();
				uint8 finetune = f.Hio_Read8();
				uint8 volume = f.Hio_Read8();

				if ((size != 0) && ((finetune != 0) || (volume != 64)))
					maybe_Wow = false;

				if (size >= 0x8000)
					has_Big_Samples = true;

				smp_Size += 2 * size;

				f.Hio_Seek(4, SeekOrigin.Current);
			}

			f.Hio_Seek(start + 951, SeekOrigin.Begin);
			uint8 restart = f.Hio_Read8();

			c_int pat = 0;

			for (c_int i = 0; i < 128; i++)
			{
				uint8 x = f.Hio_Read8();

				if (x > 0x7f)
					break;

				if (x > pat)
					pat = x;
			}

			pat++;

			// Mod's Grave WOW files always have a 0 restart byte; 6692WOW implements
			// 669 repeating by inserting a pattern jump and ignores this byte
			if (restart != 0)
				maybe_Wow = false;

			f.Hio_Seek(start + 1080, SeekOrigin.Begin);
			uint32 magic = f.Hio_Read32B();
			mkMark = magic == Common.Magic4("M.K.");

			for (c_int i = 0; i < mod_Magic.Length; i++)
			{
				if (mod_Magic[i].Magic == magic)
				{
					channels = mod_Magic[i].Ch;
					trackerId = mod_Magic[i].Id;
					detected = mod_Magic[i].Flag;
					break;
				}
			}

			if (trackerId == InternalFormat.ProTracker)
				needs_Timing_Detection = true;

			if (channels == 0)
			{
				char digit1 = (char)((magic & 0xff000000) >> 24);
				char digit2 = (char)((magic & 0x00ff0000) >> 16);

				if (((magic & 0x0000ffff) == Common.Magic4('\0', '\0', 'C', 'H')) && char.IsDigit(digit1) && char.IsDigit(digit2))
					channels = (digit1 - '0') * 10 + digit2 - '0';
				else if (((magic & 0x00ffffff) == Common.Magic4('\0', 'C', 'H', 'N')) && char.IsDigit(digit1))
					channels = digit1 - '0';
				else
					return InternalFormat.NotRecognized;

				trackerId = (channels & 1) != 0 ? InternalFormat.TakeTracker : InternalFormat.FastTracker2;
				detected = true;
			}

			if (has_Big_Samples)
			{
				trackerId = InternalFormat.OpenMpt;
				needs_Timing_Detection = false;
				detected = true;
			}

			// Experimental tracker-detection routine
			if (detected)
				goto Skip_Test;

			// Test for Flextrax modules
			//
			// FlexTrax is a soundtracker for Atari Falcon030 compatible computers.
			// FlexTrax supports the standard MOD file format (up to eight channels)
			// for compatibility reasons but also features a new enhanced module
			// format FLX. The FLX format is an extended version of the standard
			// MOD file format with support for real-time sound effects like reverb
			// and delay
			if ((0x43c + pat * 4 * channels * 0x40 + smp_Size) < fileSize)
			{
				uint8[] idBuffer = new uint8[4];

				c_int pos = f.Hio_Tell();
				if (pos < 0)
					return InternalFormat.NotRecognized;

				f.Hio_Seek(start + 0x43c + pat * 4 * channels * 0x40 + smp_Size, SeekOrigin.Begin);
				c_int num_Read = (c_int)f.Hio_Read(idBuffer, 1, 4);
				f.Hio_Seek(pos, SeekOrigin.Begin);

				if ((num_Read == 4) && (Common.Magic4(idBuffer[0], idBuffer[1], idBuffer[2], idBuffer[3]) == Common.Magic4("FLEX")))
				{
					trackerId = InternalFormat.Flextrax;
					needs_Timing_Detection = false;
					goto Skip_Test;
				}
			}

			// Test for Mod's Grave WOW modules
			//
			// Stefan Danes <sdanes@marvels.hacktic.nl> said:
			// This weird format is identical to '8CHN' but still uses the 'M.K.' ID.
			// You can only test for WOW by calculating the size of the module for 8
			// channels and comparing this to the actual module length. If it's equal,
			// the module is an 8 channel WOW.
			//
			// Addendum: very rarely, WOWs will have an odd length due to an extra byte,
			// so round the filesize down in this check. False positive WOWs can be ruled
			// out by checking the restart byte and sample volume (see above).
			//
			// Worst case if there are still issues with this, OpenMPT validates later
			// patterns in potential WOW files (where sample data would be located in a
			// regular M.K. MOD) to rule out false positives
			if ((magic == Common.Magic4("M.K.")) && maybe_Wow && ((0x43c + pat * 32 * 0x40 + smp_Size) == (fileSize & ~1)))
			{
				channels = 8;
				trackerId = InternalFormat.ModsGrave;
				needs_Timing_Detection = false;
			}
			else
				trackerId = Get_Tracker_Id(f, start, trackerId, restart, pat);

			Skip_Test:
			if ((trackerId is InternalFormat.ProTracker or InternalFormat.NoiseTracker or InternalFormat.Probably_NoiseTracker or InternalFormat.SoundTracker or InternalFormat.Unknown) || mkMark)
			{
				uint8[] patBuf = new uint8[64 * 4 * channels];
				out_Of_Range = false;
				bool sameRow_Fxx = false;

				f.Hio_Seek(start + 1084, SeekOrigin.Begin);

				for (c_int i = 0; i < pat; i++)
				{
					if (f.Hio_Read(patBuf, (size_t)(64 * 4 * channels), 1) < 1)
						break;

					c_int mod_Event = 0;

					for (c_int j = 0; j < 64; j++)
					{
						bool speed_Row = false;
						bool bpm_Row = false;

						for (c_int k = 0; k < channels; k++)
						{
							c_int period = (Ports.LibXmp.Common.Lsn(patBuf[mod_Event]) << 8) | patBuf[mod_Event + 1];
							if ((period != 0) && ((period < 113/*108*/) || (period > 856/*907*/)))
								out_Of_Range = true;

							// Filter Noisetracker events
							if (trackerId == InternalFormat.Probably_NoiseTracker)
							{
								uint8 fxT = Ports.LibXmp.Common.Lsn(patBuf[mod_Event + 2]);
								uint8 fxP = Ports.LibXmp.Common.Lsn(patBuf[mod_Event + 3]);

								if (((fxT > 0x06) && (fxT < 0x0a)) || ((fxT == 0x0e) && (fxP > 1)))
									trackerId = InternalFormat.Unknown;
							}

							if (Ports.LibXmp.Common.Lsn(patBuf[mod_Event + 2]) == 0x0f)
							{
								if (patBuf[mod_Event + 3] >= 0x20)
									bpm_Row = true;
								else
									speed_Row = true;
							}

							mod_Event += 4;
						}

						if (bpm_Row && speed_Row)
							sameRow_Fxx = true;
					}

					if (out_Of_Range)
					{
						if ((trackerId == InternalFormat.Unknown) && (restart == 0x7f))
						{
							trackerId = InternalFormat.ScreamTracker3;
							goto Done;
						}

						// Check out-of-range notes in Amiga trackers
						if (trackerId is InternalFormat.ProTracker or InternalFormat.NoiseTracker or InternalFormat.Probably_NoiseTracker or InternalFormat.SoundTracker)
							trackerId = InternalFormat.Unknown;
					}
				}

				Done:
				if (needs_Timing_Detection && sameRow_Fxx)
				{
					if (trackerId is InternalFormat.NoiseTracker or InternalFormat.Probably_NoiseTracker or InternalFormat.SoundTracker)
						trackerId = InternalFormat.Unknown;
				}
			}

			return trackerId;
		}
		#endregion
	}
}
