﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Text;
using Polycode.NostalgicPlayer.Kit.Encoders;

namespace Polycode.NostalgicPlayer.Kit
{
	/// <summary>
	/// This class in the entry point to retrieve different character encoders
	/// </summary>
	public static class EncoderCollection
	{
		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode Amiga characters
		/// </summary>
		/********************************************************************/
		public static Encoding Amiga
		{
			get;
		} = new Amiga();



		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode PC DOS characters
		/// </summary>
		/********************************************************************/
		public static Encoding Dos
		{
			get;
		} = new Ibm865();



		/********************************************************************/
		/// <summary>
		/// Return the encoder to decode Macintosh characters
		/// </summary>
		/********************************************************************/
		public static Encoding Macintosh
		{
			get;
		} = new MacintoshRoman();
	}
}
