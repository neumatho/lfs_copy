﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Flac.Containers
{
	/// <summary>
	/// Return values for the Flac__StreamDecoder read callback
	/// </summary>
	internal enum Flac__StreamDecoderLengthStatus
	{
		/// <summary>
		/// The length call was OK and decoding can continue
		/// </summary>
		Ok,

		/// <summary>
		/// An unrecoverable error occurred. The decoder will return from the process call
		/// </summary>
		Error,

		/// <summary>
		/// Client does not support reporting the length
		/// </summary>
		Unsupported
	}
}