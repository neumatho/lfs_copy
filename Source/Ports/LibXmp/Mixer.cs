﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;
using System.Runtime.InteropServices;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Mixer;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Player;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.LibXmp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Mixer
	{
		private const c_int DownMix_Shift = 12;
		private const c_int Lim16_Hi = 32767;
		private const c_int Lim16_Lo = -32768;

		private const c_int Loop_Prologue = 1;
		private const c_int Loop_Epilogue = 2;

		private const c_int AntiClick_FPShift = 24;

		private delegate void Mix_Fp(Mixer_Voice vi, int32[] buffer, c_int offset, c_int count, c_int vl, c_int vr, c_int step, c_int ramp, c_int delta_L, c_int delta_R);

		#region Loop_Data class
		private class Loop_Data
		{
			public byte[] SPtr;
			public c_int SPtrOffset;
			public c_int Start;
			public c_int End;
			public bool First_Loop;
			public bool _16Bit;
			public bool Active;
			public uint32[] Prologue = new uint[Loop_Prologue];
			public uint32[] Epilogue = new uint[Loop_Epilogue];
		}
		#endregion

		private readonly LibXmp lib;
		private readonly Xmp_Context ctx;

		private readonly Mix_Fp[] nearest_Mixers;
		private readonly Mix_Fp[] linear_Mixers;
		private readonly Mix_Fp[] spline_Mixers;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Mixer(LibXmp libXmp, Xmp_Context ctx)
		{
			lib = libXmp;
			this.ctx = ctx;

			nearest_Mixers = new Mix_Fp[]
			{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};

			linear_Mixers = new Mix_Fp[]
			{
				null,
				null,
				null,
				null,
				null,
				null,
				null,
				null
			};

			spline_Mixers = new Mix_Fp[]
			{
				Mix_All.LibXmp_Mix_Mono_8Bit_Spline,
				Mix_All.LibXmp_Mix_Mono_16Bit_Spline,
				Mix_All.LibXmp_Mix_Stereo_8Bit_Spline,
				Mix_All.LibXmp_Mix_Stereo_16Bit_Spline,
				null,
				null,
				null,
				null
			};
		}



		/********************************************************************/
		/// <summary>
		/// Fill the output buffer calling one of the handlers. The buffer
		/// contains sound for one tick (a PAL frame or 1/50s for standard
		/// vblank-timed mods)
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SoftMixer()
		{
			Player_Data p = ctx.P;
			Mixer_Data s = ctx.S;
			Module_Data m = ctx.M;
			Xmp_Module mod = m.Mod;
			Mixer_Voice vi;
			c_int voc;

/*//XX			if (nostalgicChannels != null)
			{
				// OpenMPT Bidi-Loops.it: "In Impulse Tracker's software
				// mixer, ping-pong loops are shortened by one sample."
				s.BiDir_Adjust = Common.Is_Player_Mode_It(m) ? 1 : 0;

				for (voc = 0; voc < p.Virt.MaxVoc; voc++)
				{
					vi = p.Virt.Voice_Array[voc];

					if (vi.Chn < 0)
						continue;

					if (vi.Period < 1)
					{
						lib.virt.LibXmp_Virt_ResetVoice(voc, true);
						continue;
					}
				}

				return;
			}
*/
			Extra_Sample_Data xtra;
			Xmp_Sample xxs;
			Loop_Data loop_Data = new Loop_Data();
			c_double step, step_Dir;
			c_int samples, size;
			c_int vol, vol_L, vol_R, uSmp;
			c_int prev_L, prev_R = 0;
			c_int buf_Pos;
			Mix_Fp mix_Fn;
			Mix_Fp[] mixerSet;

			switch (s.Interp)
			{
				case Xmp_Interp.Nearest:
				{
					mixerSet = nearest_Mixers;
					break;
				}

				case Xmp_Interp.Linear:
				{
					mixerSet = linear_Mixers;
					break;
				}

				case Xmp_Interp.Spline:
				{
					mixerSet = spline_Mixers;
					break;
				}

				default:
				{
					mixerSet = linear_Mixers;
					break;
				}
			}

			// OpenMPT Bidi-Loops.it: "In Impulse Tracker's software
			// mixer, ping-pong loops are shortened by one sample."
			s.BiDir_Adjust = Common.Is_Player_Mode_It(m) ? 1 : 0;

			LibXmp_Mixer_Prepare();

			for (voc = 0; voc < p.Virt.MaxVoc; voc++)
			{
				c_int c5Spd, rampSize, delta_L, delta_R;

				vi = p.Virt.Voice_Array[voc];

				if ((vi.Flags & Mixer_Flag.AntiClick) != 0)
				{
					if (s.Interp > Xmp_Interp.Nearest)
						Do_AntiClick(voc, null, 0, 0);

					vi.Flags &= ~Mixer_Flag.AntiClick;
				}

				if (vi.Chn < 0)
					continue;

				if (vi.Period < 1)
				{
					lib.virt.LibXmp_Virt_ResetVoice(voc, true);
					continue;
				}

				// Negative positions can be left over from some
				// loop edge cases. These can be safely clamped
				if (vi.Pos < 0.0)
					vi.Pos = 0.0;

				vi.Pos0 = (c_int)vi.Pos;

				buf_Pos = 0;
				vol = vi.Vol;

				// Mix volume (S3M and IT)
				if ((m.MVolBase > 0) && (m.MVol != m.MVolBase))
					vol = vol * m.MVol / m.MVolBase;

				if (vi.Pan == Constants.Pan_Surround)
				{
					vol_R = vol * 0x80;
					vol_L = -vol * 0x80;
				}
				else
				{
					vol_R = vol * (0x80 - vi.Pan);
					vol_L = vol * (0x80 + vi.Pan);
				}

				if (vi.Smp < mod.Smp)
				{
					xxs = mod.Xxs[vi.Smp];
					xtra = m.Xtra[vi.Smp];
					c5Spd = (c_int)m.Xtra[vi.Smp].C5Spd;
				}
				else
					continue;

				step = Constants.C4_Period * c5Spd / s.Freq / vi.Period;

				// Don't allow <=0, otherwise m5v-nwlf.it crashes
				// Extremely high values that can cause undefined float/int
				// conversion are also possible for c5spd modules
				if ((step < 0.001) || (step > c_short.MaxValue))
					continue;

				Adjust_Voice_End(vi, xxs, xtra);
				Init_Sample_Wraparound(s, loop_Data, vi, xxs);

				rampSize = s.TickSize >> Constants.AntiClick_Shift;
				delta_L = (vol_L - vi.Old_VL) / rampSize;
				delta_R = (vol_R - vi.Old_VR) / rampSize;

				for (size = uSmp = s.TickSize; size > 0; )
				{
					bool split_NoLoop = false;

					if (p.Xc_Data[vi.Chn].Split != 0)
						split_NoLoop = true;

					// How many samples we can write before the loop break
					// or sample end...
					if ((~vi.Flags & Mixer_Flag.Voice_Reverse) != 0)
					{
						if (vi.Pos >= vi.End)
						{
							samples = 0;

							if (--uSmp <= 0)
								break;
						}
						else
						{
							c_double c = Math.Ceiling((vi.End - vi.Pos) / step);

							// ...inside the tick boundaries
							if (c > size)
								c = size;

							samples = (c_int)c;
						}

						step_Dir = step;
					}
					else
					{
						// Reverse
						if (vi.Pos <= vi.Start)
						{
							samples = 0;

							if (--uSmp <= 0)
								break;
						}
						else
						{
							c_double c = Math.Ceiling((vi.Pos - vi.Start) / step);

							if (c > size)
								c = size;

							samples = (c_int)c;
						}

						step_Dir = -step;
					}

					if (vi.Vol != 0)
					{
						c_int mix_Size = samples;
						Mixer_Index_Flag mixer_Id = vi.FIdx & Mixer_Index_Flag.FlagMask;

						if ((~s.Format & Xmp_Format.Mono) != 0)
							mix_Size *= 2;

						// For Hipolito's anticlick routine
						if (samples > 0)
						{
							if ((~s.Format & Xmp_Format.Mono) != 0)
								prev_R = s.Buf32[buf_Pos + mix_Size - 2];

							prev_L = s.Buf32[buf_Pos + mix_Size - 1];
						}
						else
							prev_R = prev_L = 0;

						// See OpenMPT env-flt-max.it
						if ((vi.Filter.CutOff >= 0xfe) && (vi.Filter.Resonance == 0))
							mixer_Id &= ~Mixer_Index_Flag.Filter;

						mix_Fn = mixerSet[(c_int)mixer_Id];

						// Call the output handler
						if ((samples > 0) && (vi.SPtr != null))
						{
							c_int rSize = 0;

							if (rampSize > samples)
								rampSize -= samples;
							else
							{
								rSize = samples - rampSize;
								rampSize = 0;
							}

							if ((delta_L == 0) && (delta_R == 0))
							{
								// No need to ramp
								rSize = samples;
							}

							if (mix_Fn != null)
								mix_Fn(vi, s.Buf32, buf_Pos, samples, vol_L >> 8, vol_R >> 8, (c_int)(step_Dir * (1 << Constants.SMix_Shift)), rSize, delta_L, delta_R);

							buf_Pos += mix_Size;
							vi.Old_VL += samples * delta_L;
							vi.Old_VR += samples * delta_R;

							// For Hipolito's anticlick routine
							if ((~s.Format & Xmp_Format.Mono) != 0)
								vi.SRight = s.Buf32[buf_Pos - 2] - prev_R;

							vi.SLeft = s.Buf32[buf_Pos - 1] - prev_L;
						}
					}

					vi.Pos += step_Dir * samples;

					// No more samples in this tick
					size -= samples;

					if (size <= 0)
					{
						if (Has_Active_Loop(vi, xxs))
						{
							// This isn't particularly important for
							// forward loops, but reverse loops need
							// to be corrected here to avoid their
							// negative positions getting clamped
							// in later ticks
							if ((((~vi.Flags & Mixer_Flag.Voice_Reverse) != 0) && (vi.Pos >= vi.End)) ||
								(((vi.Flags & Mixer_Flag.Voice_Reverse) != 0) && (vi.Pos <= vi.Start)))
							{
								if (Loop_Reposition(vi, xxs, xtra))
								{
									Reset_Sample_Wraparound(loop_Data);
									Init_Sample_Wraparound(s, loop_Data, vi, xxs);
								}
							}
						}
						continue;
					}

					// First sample loop run
					if (!Has_Active_Loop(vi, xxs) || split_NoLoop)
					{
						Do_AntiClick(voc, s.Buf32, buf_Pos, size);
						Set_Sample_End(voc, 1);
						size = 0;
						continue;
					}

					if (Loop_Reposition(vi, xxs, xtra))
					{
						Reset_Sample_Wraparound(loop_Data);
						Init_Sample_Wraparound(s, loop_Data, vi, xxs);
					}
				}

				Reset_Sample_Wraparound(loop_Data);
				vi.Old_VL = vol_L;
				vi.Old_VR = vol_R;
			}

			// Render final frame
			size = s.TickSize;
			if ((~s.Format & Xmp_Format.Mono) != 0)
				size *= 2;

			if (size > Constants.Xmp_Max_FrameSize)
				size = Constants.Xmp_Max_FrameSize;

			if ((s.Format & Xmp_Format._8Bit) != 0)
				DownMix_Int_8Bit(s.Buffer, s.Buf32, size, s.Amplify, (s.Format & Xmp_Format.Unsigned) != 0 ? 0x80 : 0);
			else
				DownMix_Int_16Bit(MemoryMarshal.Cast<sbyte, int16>(s.Buffer), s.Buf32, size, s.Amplify, (s.Format & Xmp_Format.Unsigned) != 0 ? 0x8000 : 0);

			s.DtRight = s.DtLeft = 0;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_VoicePos(c_int voc, c_double pos, bool ac)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];
			Xmp_Sample xxs;
			Extra_Sample_Data xtra;

			if (vi.Smp < m.Mod.Smp)
			{
				xxs = m.Mod.Xxs[vi.Smp];
				xtra = m.Xtra[vi.Smp];
			}
			else
				return;

			if ((xxs.Flg & Xmp_Sample_Flag.Synth) != 0)
				return;

			vi.Pos = pos;

			Adjust_Voice_End(vi, xxs, xtra);

			if (vi.Pos >= vi.End)
			{
				vi.Pos = vi.End;

				// Restart forward sample loops
				if (((~vi.Flags & Mixer_Flag.Voice_Reverse) != 0) && Has_Active_Loop(vi, xxs))
					Loop_Reposition(vi, xxs, xtra);
			}
			else if (((vi.Flags & Mixer_Flag.Voice_Reverse) != 0) && (vi.Pos <= 0.1))
			{
				// Hack: 0 maps to the end for reversed samples
				vi.Pos = vi.End;
			}

			if (ac)
				AntiClick(vi);

/*//XX			if (nostalgicChannels != null)
			{
				if (ac)
				{
					nostalgicChannels[voc].PlaySample((short)vi.Smp, vi.SPtr, (uint)(vi.SPtrOffset + vi.Pos), (uint)vi.End, (byte)((vi.FIdx & Mixer_Index_Flag._16_Bits) != 0 ? 16 : 8), (vi.Flags & Mixer_Flag.Voice_Reverse) != 0);

					if (Has_Active_Loop(vi, xxs))
						nostalgicChannels[voc].SetLoop((uint)vi.Start, (uint)(vi.End - vi.Start), (vi.Flags & Mixer_Flag.Voice_BiDir) != 0 ? ChannelLoopType.PingPong : ChannelLoopType.Normal);
				}
			}*/
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_double LibXmp_Mixer_GetVoicePos(c_int voc)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Xmp_Sample xxs = lib.sMix.LibXmp_Get_Sample(vi.Smp);

			if ((xxs.Flg & Xmp_Sample_Flag.Synth) != 0)
				return 0;

			return vi.Pos;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetPatch(c_int voc, c_int smp, bool ac)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Data s = ctx.S;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			Xmp_Sample xxs = lib.sMix.LibXmp_Get_Sample(smp);

			vi.Smp = smp;
			vi.Vol = 0;
			vi.Pan = 0;
			vi.Flags &= ~(Mixer_Flag.Sample_Loop | Mixer_Flag.Voice_Reverse | Mixer_Flag.Voice_BiDir);

			vi.FIdx = 0;

			if ((~s.Format & Xmp_Format.Mono) != 0)
				vi.FIdx |= Mixer_Index_Flag.Stereo;

			Set_Sample_End(voc, 0);

			vi.SPtr = xxs.Data;
			vi.SPtrOffset = xxs.DataOffset;
			vi.FIdx |= Mixer_Index_Flag.Active;

			if (Common.Has_Quirk(m, Quirk_Flag.Filter) && ((s.Dsp & Xmp_Dsp.LowPass) != 0))
				vi.FIdx |= Mixer_Index_Flag.Filter;

			if ((xxs.Flg & Xmp_Sample_Flag._16Bit) != 0)
				vi.FIdx |= Mixer_Index_Flag._16_Bits;

			LibXmp_Mixer_VoicePos(voc, 0, ac);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetNote(c_int voc, c_int note)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			// FIXME: Workaround for crash on notes that are too high
			//        see 6nations.it (+114 transposition on instrument 16)
			if (note > 149)
				note = 149;

			vi.Note = note;
			vi.Period = lib.period.LibXmp_Note_To_Period_Mix(note, 0);

			AntiClick(vi);

//XX			if (nostalgicChannels != null)
//XX				nostalgicChannels[voc].SetAmigaPeriod((uint)vi.Period);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetPeriod(c_int voc, c_double period)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			vi.Period = period;

//XX			if (nostalgicChannels != null)
//XX				nostalgicChannels[voc].SetAmigaPeriod((uint)period);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetVol(c_int voc, c_int vol)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			if (vol == 0)
				AntiClick(vi);

			vi.Vol = vol;

/*//XX			if (nostalgicChannels != null)
			{
				Module_Data m = ctx.M;

				if ((m.MVolBase > 0) && (m.MVol != m.MVolBase))
					vol = vol * m.MVol / m.MVolBase;

				nostalgicChannels[voc].SetVolume((ushort)(vol / 4));
			}*/
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_Release(c_int voc, bool rel)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			if (rel)
			{
				// Cancel voice reverse when releasing an active sustain loop,
				// unless the main loop is bidirectional. This is done both for
				// bidirectional sustain loops and for forward sustain loops
				// that have been reversed with MPT S9F Play Backward
				if ((~vi.Flags & Mixer_Flag.Voice_Release) != 0)
				{
					Xmp_Sample xxs = lib.sMix.LibXmp_Get_Sample(vi.Smp);

					if (Has_Active_Sustain_Loop(vi, xxs) && ((~xxs.Flg & Xmp_Sample_Flag.Loop_BiDir) != 0))
						vi.Flags &= ~Mixer_Flag.Voice_Reverse;
				}

				vi.Flags |= Mixer_Flag.Voice_Release;
			}
			else
				vi.Flags &= ~Mixer_Flag.Voice_Release;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_Reverse(c_int voc, bool rev)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			// Don't reverse samples that have already ended
			if ((~vi.FIdx & Mixer_Index_Flag.Active) != 0)
				return;

			if (rev)
				vi.Flags |= Mixer_Flag.Voice_Reverse;
			else
				vi.Flags &= ~Mixer_Flag.Voice_Reverse;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetEffect(c_int voc, Dsp_Effect type, c_int val)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			switch (type)
			{
				case Dsp_Effect.CutOff:
				{
					vi.Filter.CutOff = val;
					break;
				}

				case Dsp_Effect.Resonance:
				{
					vi.Filter.Resonance = val;
					break;
				}

				case Dsp_Effect.Filter_A0:
				{
					vi.Filter.A0 = val;
					break;
				}

				case Dsp_Effect.Filter_B0:
				{
					vi.Filter.B0 = val;
					break;
				}

				case Dsp_Effect.Filter_B1:
				{
					vi.Filter.B1 = val;
					break;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_SetPan(c_int voc, c_int pan)
		{
			Player_Data p = ctx.P;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			vi.Pan = pan;

/*//XX			if (nostalgicChannels != null)
			{
				if (vi.Pan == Constants.Pan_Surround)
					pan = (int)ChannelPanningType.Surround;
				else
					pan += 128;

				nostalgicChannels[voc].SetPanning((ushort)pan);
			}*/
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Mixer_NumVoices(c_int num)
		{
			Mixer_Data s = ctx.S;

			if ((num > s.NumVoc) || (num < 0))
				return s.NumVoc;
			else
				return num;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public c_int LibXmp_Mixer_On(c_int rate, Xmp_Format format, c_int c4Rate)
		{
			Mixer_Data s = ctx.S;

			s.Buffer = new int8[2 * Constants.Xmp_Max_FrameSize];
			if (s.Buffer == null)
				goto Err;

			s.Buf32 = new int32[Constants.Xmp_Max_FrameSize];
			if (s.Buf32 == null)
				goto Err1;

			s.Freq = rate;
			s.Format = format;
			s.Amplify = Constants.Default_Amplify;
			s.Mix = Constants.Default_Mix;
			s.Interp = Xmp_Interp.Linear;	// Default interpolation type
			s.Dsp = Xmp_Dsp.LowPass;		// Enable filters by default
			s.DtRight = s.DtLeft = 0;
			s.BiDir_Adjust = 0;

			return 0;

			Err1:
			s.Buffer = null;
			Err:
			return -1;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void LibXmp_Mixer_Off()
		{
			Mixer_Data s = ctx.S;

			s.Buf32 = null;
			s.Buffer = null;
		}



		/********************************************************************/
		/// <summary>
		/// Will set the channel objects to use
		/// </summary>
		/********************************************************************/
		public void Xmp_Set_NostalgicPlayer_Channels(IChannel[] channels)
		{
//XX			nostalgicChannels = channels;
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Downmix 32bit samples to 8bit, signed or unsigned, mono or stereo
		/// output
		/// </summary>
		/********************************************************************/
		private void DownMix_Int_8Bit(sbyte[] dest, int32[] src, c_int num, c_int amp, c_int offs)
		{
		}



		/********************************************************************/
		/// <summary>
		/// Downmix 32bit samples to 16bit, signed or unsigned, mono or
		/// stereo output
		/// </summary>
		/********************************************************************/
		private void DownMix_Int_16Bit(Span<int16> dest, int32[] src, c_int num, c_int amp, c_int offs)
		{
			c_int shift = DownMix_Shift - amp;

			for (c_int offset = 0; num-- != 0; offset++)
			{
				c_int smp = src[offset] >> shift;
				if (smp > Lim16_Hi)
					dest[offset] = Lim16_Hi;
				else if (smp < Lim16_Lo)
					dest[offset] = Lim16_Lo;
				else
					dest[offset] = (int16)smp;

				if (offs != 0)
					dest[offset] += (int16)offs;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void AntiClick(Mixer_Voice vi)
		{
			vi.Flags |= Mixer_Flag.AntiClick;
			vi.Old_VL = 0;
			vi.Old_VR = 0;
		}



		/********************************************************************/
		/// <summary>
		/// Ok, it's messy, but it works :-) Hipolito
		/// </summary>
		/********************************************************************/
		private void Do_AntiClick(c_int voc, int32[] buf, c_int offset, c_int count)
		{
			Player_Data p = ctx.P;
			Mixer_Data s = ctx.S;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];
			c_int discharge = s.TickSize >> Constants.AntiClick_Shift;

			c_int smp_R = vi.SRight;
			c_int smp_L = vi.SLeft;
			vi.SRight = vi.SLeft = 0;

			if ((smp_L == 0) && (smp_R == 0))
				return;

			if (buf == null)
			{
				buf = s.Buf32;
				offset = 0;
				count = discharge;
			}
			else if (count > discharge)
			{
				count = discharge;
			}

			if (count <= 0)
				return;

			c_int stepVal = (1 << AntiClick_FPShift) / count;
			c_int stepMul = stepVal * count;

			if ((~s.Format & Xmp_Format.Mono) != 0)
			{
				while ((stepMul -= stepVal) > 0)
				{
					// Truncate to 16-bits of precision so the product is 32-bits
					uint32 stepMul_Sq = (uint32)(stepMul >> (AntiClick_FPShift - 16));
					stepMul_Sq *= stepMul_Sq;

					buf[offset++] += (int32)((stepMul_Sq * smp_R) >> 32);
					buf[offset++] += (int32)((stepMul_Sq * smp_L) >> 32);
				}
			}
			else
			{
				while ((stepMul -= stepVal) > 0)
				{
					// Truncate to 16-bits of precision so the product is 32-bits
					uint32 stepMul_Sq = (uint32)(stepMul >> (AntiClick_FPShift - 16));
					stepMul_Sq *= stepMul_Sq;

					buf[offset++] += (int32)((stepMul_Sq * smp_L) >> 32);
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Set_Sample_End(c_int voc, c_int end)
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Voice vi = p.Virt.Voice_Array[voc];

			if ((uint32)voc >= p.Virt.MaxVoc)
				return;

			Channel_Data xc = p.Xc_Data[vi.Chn];

			if (end != 0)
			{
				xc.Note_Flags |= Note_Flag.Sample_End;
				vi.FIdx &= ~Mixer_Index_Flag.Active;

				if (Common.Has_Quirk(m, Quirk_Flag.RstChn))
					lib.virt.LibXmp_Virt_ResetVoice(voc, false);
			}
			else
				xc.Note_Flags &= ~Note_Flag.Sample_End;
		}



		/********************************************************************/
		/// <summary>
		/// Backup sample data before and after loop and replace it for
		/// interpolation.
		/// TODO: Use an overlap buffer like OpenMPT? This is easier, but a
		/// little dirty
		/// </summary>
		/********************************************************************/
		private void Init_Sample_Wraparound(Mixer_Data s, Loop_Data ld, Mixer_Voice vi, Xmp_Sample xxs)
		{
			if ((vi.SPtr == null) || (s.Interp == Xmp_Interp.Nearest) || ((~xxs.Flg & Xmp_Sample_Flag.Loop) != 0))
			{
				ld.Active = false;
				return;
			}

			ld.SPtr = vi.SPtr;
			ld.SPtrOffset = vi.SPtrOffset;
			ld.Start = vi.Start;
			ld.End = vi.End;
			ld.First_Loop = !((vi.Flags & Mixer_Flag.Sample_Loop) != 0);
			ld._16Bit = (xxs.Flg & Xmp_Sample_Flag._16Bit) != 0;
			ld.Active = true;

			bool biDir = (vi.Flags & Mixer_Flag.Voice_BiDir) != 0;

			if (ld._16Bit)
			{
				Span<uint16> buf = MemoryMarshal.Cast<byte, uint16>(vi.SPtr);
				c_int start = (vi.SPtrOffset / 2) + vi.Start;
				c_int end = (vi.SPtrOffset / 2) + vi.End;

				if (!ld.First_Loop)
				{
					for (c_int i = 0; i < Loop_Prologue; i++)
					{
						c_int j = i - Loop_Prologue;
						ld.Prologue[i] = buf[start + j];
						buf[start + j] = biDir ? buf[start - 1 - j] : buf[end + j];
					}
				}

				for (c_int i = 0; i < Loop_Epilogue; i++)
				{
					ld.Epilogue[i] = buf[end + i];
					buf[end + i] = biDir ? buf[end - 1 - i] : buf[start + i];
				}
			}
			else
			{
				uint8[] buf = vi.SPtr;
				c_int start = vi.SPtrOffset + vi.Start;
				c_int end = vi.SPtrOffset + vi.End;

				if (!ld.First_Loop)
				{
					for (c_int i = 0; i < Loop_Prologue; i++)
					{
						c_int j = i - Loop_Prologue;
						ld.Prologue[i] = buf[start + j];
						buf[start + j] = biDir ? buf[start - 1 - j] : buf[end + j];
					}
				}

				for (c_int i = 0; i < Loop_Epilogue; i++)
				{
					ld.Epilogue[i] = buf[end + i];
					buf[end + i] = biDir ? buf[end - 1 - i] : buf[start + i];
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Restore old sample data from before and after loop
		/// </summary>
		/********************************************************************/
		private void Reset_Sample_Wraparound(Loop_Data ld)
		{
			if (!ld.Active)
				return;

			if (ld._16Bit)
			{
				Span<uint16> buf = MemoryMarshal.Cast<byte, uint16>(ld.SPtr);
				c_int start = (ld.SPtrOffset / 2) + ld.Start;
				c_int end = (ld.SPtrOffset / 2) + ld.End;

				if (!ld.First_Loop)
				{
					for (c_int i = 0; i < Loop_Prologue; i++)
						buf[start + i - Loop_Prologue] = (uint16)ld.Prologue[i];
				}

				for (c_int i = 0; i < Loop_Epilogue; i++)
					buf[end + i] = (uint16)ld.Epilogue[i];
			}
			else
			{
				uint8[] buf = ld.SPtr;
				c_int start = ld.SPtrOffset + ld.Start;
				c_int end = ld.SPtrOffset + ld.End;

				if (!ld.First_Loop)
				{
					for (c_int i = 0; i < Loop_Prologue; i++)
						buf[start + i - Loop_Prologue] = (uint8)ld.Prologue[i];
				}

				for (c_int i = 0; i < Loop_Epilogue; i++)
					buf[end + i] = (uint8)ld.Epilogue[i];
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Has_Active_Sustain_Loop(Mixer_Voice vi, Xmp_Sample xxs)
		{
			Module_Data m = ctx.M;

			return (vi.Smp < m.Mod.Smp) && ((xxs.Flg & Xmp_Sample_Flag.SLoop) != 0) && ((~vi.Flags & Mixer_Flag.Voice_Release) != 0);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Has_Active_Loop(Mixer_Voice vi, Xmp_Sample xxs)
		{
			return ((xxs.Flg & Xmp_Sample_Flag.Loop) != 0) || Has_Active_Sustain_Loop(vi, xxs);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void Adjust_Voice_End(Mixer_Voice vi, Xmp_Sample xxs, Extra_Sample_Data xtra)
		{
			vi.Flags &= ~Mixer_Flag.Voice_BiDir;

			if ((xtra != null) && Has_Active_Sustain_Loop(vi, xxs))
			{
				vi.Start = xtra.Sus;
				vi.End = xtra.Sue;

				if ((xxs.Flg & Xmp_Sample_Flag.SLoop_BiDir) != 0)
					vi.Flags |= Mixer_Flag.Voice_BiDir;
			}
			else if ((xxs.Flg & Xmp_Sample_Flag.Loop) != 0)
			{
				vi.Start = xxs.Lps;

				if (((xxs.Flg & Xmp_Sample_Flag.Loop_Full) != 0) && (((~vi.Flags & Mixer_Flag.Sample_Loop) != 0)))
					vi.End = xxs.Len;
				else
				{
					vi.End = xxs.Lpe;

					if ((xxs.Flg & Xmp_Sample_Flag.Loop_BiDir) != 0)
						vi.Flags |= Mixer_Flag.Voice_BiDir;
				}
			}
			else
			{
				vi.Start = 0;
				vi.End = xxs.Len;
			}
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private bool Loop_Reposition(Mixer_Voice vi, Xmp_Sample xxs, Extra_Sample_Data xtra)
		{
			bool loop_Changed = !((vi.Flags & Mixer_Flag.Sample_Loop) != 0);

			vi.Flags |= Mixer_Flag.Sample_Loop;

			if (loop_Changed)
				Adjust_Voice_End(vi, xxs, xtra);

			if ((~vi.Flags & Mixer_Flag.Voice_BiDir) != 0)
			{
				// Reposition for next loop
				if ((~vi.Flags & Mixer_Flag.Voice_Reverse) != 0)
					vi.Pos -= vi.End - vi.Start;
				else
					vi.Pos += vi.End - vi.Start;
			}
			else
			{
				// Bidirectional loop: switch directions
				vi.Flags ^= Mixer_Flag.Voice_Reverse;

				// Wrap voice position around endpoint
				if ((vi.Flags & Mixer_Flag.Voice_Reverse) != 0)
				{
					// OpenMPT Bidi-Loops.it: "In Impulse Tracker's software
					// mixer, ping-pong loops are shortened by one sample."
					vi.Pos = vi.End * 2 - ctx.S.BiDir_Adjust - vi.Pos;
				}
				else
					vi.Pos = vi.Start * 2 - vi.Pos;
			}

			return loop_Changed;
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private void LibXmp_Mixer_Prepare()
		{
			Player_Data p = ctx.P;
			Module_Data m = ctx.M;
			Mixer_Data s = ctx.S;

			s.TickSize = (c_int)(s.Freq * m.Time_Factor * m.RRate / p.Bpm / 1000);

			if (s.TickSize < (1 << Constants.AntiClick_Shift))
				s.TickSize = 1 << Constants.AntiClick_Shift;

			c_int byteLen = s.TickSize;
			if ((~s.Format & Xmp_Format.Mono) != 0)
				byteLen *= 2;

			Array.Clear(s.Buf32, 0, byteLen);
		}
		#endregion
	}
}
