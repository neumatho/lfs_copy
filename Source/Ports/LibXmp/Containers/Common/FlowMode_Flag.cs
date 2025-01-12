﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Common
{
	[Flags]
	internal enum FlowMode_Flag
	{
		/// <summary>
		/// 
		/// </summary>
		None = 0,

		// Quirks specific to flow effects, especially Pattern Loop

		/// <summary>
		/// Global target for all tracks
		/// </summary>
		Loop_Global_Target = (1 <<  0),

		/// <summary>
		/// Global count for all tracks
		/// </summary>
		Loop_Global_Count = (1 <<  1),

		/// <summary>
		/// Loop end advances target (S3M)
		/// </summary>
		Loop_End_Advances = (1 << 2),

		/// <summary>
		/// Loop end cancels prev jumps on row (LIQ)
		/// </summary>
		Loop_End_Cancels = (1 << 3),

		/// <summary>
		/// Target/count reset on pattern change
		/// </summary>
		Loop_Pattern_Reset = (1 << 4),

		/// <summary>
		/// SBx sets target if it isn't set (ST 3.01)
		/// </summary>
		Loop_Init_SameRow = (1 << 5),

		/// <summary>
		/// Only execute the first E60/E6x in a row
		/// </summary>
		Loop_First_Effect = (1 << 6),

		/// <summary>
		/// Init E6x if no other channel is looping (MPT)
		/// </summary>
		Loop_One_At_A_Time = (1 << 7),

		/// <summary>
		/// Ignore E60 if count is >=1 (LIQ)
		/// </summary>
		Loop_Ignore_Target = (1 << 8),

		Mode_Generic = None,
		Loop_Global = (Loop_Global_Target | Loop_Global_Count),

		// Scream Tracker 3. No S3Ms seem to rely on the earlier behavior mode.
		// 3.01b has a bug where the end advancement sets the target to the same line
		// instead of the next line; there's no way to make use of this without getting
		// stuck, so it's not simulated
		Mode_ST3_301 = (Mode_ST3_321 | Loop_Init_SameRow),
		Mode_ST3_321 = (Loop_Global | Loop_Pattern_Reset | Loop_End_Advances),

		// Impulse Tracker. Not clear if anything relies on either old behavior type
		Mode_IT_100 = (Loop_Global),
		Mode_IT_104 = (Mode_Generic),
		Mode_IT_210 = (Loop_End_Advances),

		// Modplug Tracker/OpenMPT
		Mode_MPT_116 = (Loop_One_At_A_Time),

		// Imago Orpheus. Pattern Jump actually does not reset target/count, but all
		// other forms of pattern change do. Unclear if anything relies on it
		Mode_Orpheus = (Loop_Pattern_Reset),

		// Liquid Tracker uses generic MOD loops with an added behavior where
		// the end of a loop will cancel any other jump in the row that preceded it.
		// M60 is also ignored in channels that have started a loop for some reason.
		// There is also a "Scream Tracker" compatibility mode (only detectable in the
		// newer format) that adds LOOP_MODE_PATTERN_RESET.
		Mode_Liquid = (Loop_End_Cancels | Loop_Ignore_Target),
		Mode_Liquid_Compat = (Mode_Liquid | Loop_Pattern_Reset),

		// Octalyser (Atari). Looping jumps to the original position E60 was used in,
		// which libxmp doesn't simulate for now since it mostly gets the player stuck.
		// Octalyser ignores E60 if a loop is currently active; it's not clear if it's
		// possible for a module to actually rely on this behavior.
		//
		// LOOP_MODE_END_CANCELS is inaccurate but needed to fix "Dammed Illusion",
		// which has multiple E6x on one line that don't trigger because the module
		// expects to play in 4 channel mode. This quirk only works for this module
		// because it uses even loop counts, and doesn't break any other modules
		// because multiple E6x on a row otherwise traps the player
		Mode_Octalyser = (Loop_Global | Loop_Ignore_Target | Loop_End_Cancels),

		// Digital Tracker prior to shareware 1.02 doesn't use LOOP_MODE_FIRST_EFFECT,
		// but any MOD that would rely on it is impossible to fingerprint.
		// Commercial version 1.9(?) added per-track counters.
		// Digital Home Studio added a bizarre tick-0 jump bug
		Mode_DTM_203 = (Loop_Global | Loop_First_Effect),
		Mode_DTM_19 = (Loop_Global_Target)
	}
}
