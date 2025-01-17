﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Polycode.NostalgicPlayer.Ports.LibXmp.Containers.Xmp;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Fuzzer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Fuzzer
	{
		/********************************************************************/
		/// <summary>
		/// This module caused reads from the sample left redzone due to the
		/// interpolation loop wraparound initialization copying twice as
		/// many samples as necessary
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_It_Interpolation_Loop()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 2, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_It_Interpolation_Loop.it", sequence, 4000, Xmp_Format.Default, Xmp_Interp.Nearest);
		}
	}
}
