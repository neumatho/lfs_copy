﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Polycode.NostalgicPlayer.Ports.Tests.LibXmp.Test.Test_Effect
{
	/// <summary>
	/// 
	/// </summary>
	public partial class Test_Effect
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		[TestMethod]
		public void Test_Effect_Far_Vibrato_Per()
		{
			Compare_Mixer_Data(dataDirectory, "Far_Effect9.far", "Far_Effect9.data");
		}
	}
}