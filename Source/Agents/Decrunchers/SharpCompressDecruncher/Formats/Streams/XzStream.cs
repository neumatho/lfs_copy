﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;

namespace Polycode.NostalgicPlayer.Agent.Decruncher.SharpCompressDecruncher.Formats.Streams
{
	/// <summary>
	/// Wrapper class to the SharpCompress XzStream
	/// </summary>
	internal class XzStream : NoLengthStream
	{
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public XzStream(string agentName, Stream wrapperStream) : base(agentName, wrapperStream)
		{
		}

		#region NoLengthStream overrides
		/********************************************************************/
		/// <summary>
		/// Return the stream holding the packed data
		/// </summary>
		/********************************************************************/
		protected override Stream OpenPackedDataStream()
		{
			return new SharpCompress.Compressors.Xz.XZStream(wrapperStream);
		}
		#endregion
	}
}
