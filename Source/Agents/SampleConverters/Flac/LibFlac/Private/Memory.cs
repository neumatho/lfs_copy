﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.Diagnostics;
using Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Share;

namespace Polycode.NostalgicPlayer.Agent.SampleConverter.Flac.LibFlac.Private
{
	/// <summary>
	/// 
	/// </summary>
	internal static class Memory
	{
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public static Flac__bool Flac__Memory_Alloc_Aligned_Int32_Array(size_t elements, ref Flac__int32[] unaligned_Pointer, ref Flac__int32[] aligned_Pointer)
		{
			Debug.Assert(elements > 0);
//			Debug.Assert(unaligned_Pointer != null);
//			Debug.Assert(aligned_Pointer != null);
//			Debug.Assert(unaligned_Pointer != aligned_Pointer);

			Flac__int32[] pu = Flac__Memory_Alloc_Aligned(elements, out Flac__int32[] _);
			if (pu == null)
				return false;
			else
			{
				unaligned_Pointer = pu;
				aligned_Pointer = pu;

				return true;
			}
		}

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		private static T[] Flac__Memory_Alloc_Aligned<T>(size_t elements, out T[] aligned_Address) where T : new()
		{
			T[] x = Alloc.Safe_MAlloc<T>(elements);
			aligned_Address = x;

			return x;
		}
		#endregion
	}
}