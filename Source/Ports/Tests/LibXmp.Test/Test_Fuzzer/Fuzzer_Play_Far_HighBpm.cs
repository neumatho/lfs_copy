﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Fuzzer
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Fuzzer
	{
		/********************************************************************/
		/// <summary>
		/// This FAR attempts to cause libxmp to play a high BPM value using
		/// old tempo mode. At very low sampling rates this previously
		/// crashed the ramp length calculation
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Fuzzer_Play_Far_HighBpm()
		{
			Playback_Sequence[] sequence = new Playback_Sequence[]
			{
				new Playback_Sequence(Playback_Action.Play_Frames, 2, 0),
				new Playback_Sequence(Playback_Action.Play_End, 0, 0)
			};

			Util.Compare_Playback(Path.Combine(dataDirectory, "F"), "Play_Far_HighBpm.far", sequence, 4000, 0, 0);
		}
	}
}
