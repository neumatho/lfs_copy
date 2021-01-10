﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Containers;
using Polycode.NostalgicPlayer.NostalgicPlayerKit.Interfaces;

namespace Polycode.NostalgicPlayer.NostalgicPlayerKit.Bases
{
	/// <summary>
	/// Base class that can be used for player agents
	/// </summary>
	public abstract class PlayerAgentBase : IPlayerAgent
	{
		/********************************************************************/
		/// <summary>
		/// Returns the file extensions that identify this player
		/// </summary>
		/********************************************************************/
		public abstract string[] FileExtensions
		{
			get;
		}



		/********************************************************************/
		/// <summary>
		/// Test the file to see if it could be identified
		/// </summary>
		/********************************************************************/
		public abstract AgentResult Identify(PlayerFileInfo fileInfo);



		/********************************************************************/
		/// <summary>
		/// Return the name of the module
		/// </summary>
		/********************************************************************/
		public virtual string ModuleName
		{
			get
			{
				return string.Empty;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Return the name of the author
		/// </summary>
		/********************************************************************/
		public virtual string Author
		{
			get
			{
				return string.Empty;
			}
		}



		/********************************************************************/
		/// <summary>
		/// This is the main player method
		/// </summary>
		/********************************************************************/
		public abstract void Play();



		/********************************************************************/
		/// <summary>
		/// Event called when the player reached the end
		/// </summary>
		/********************************************************************/
		public event EventHandler EndReached;

		#region Helper methods
		/********************************************************************/
		/// <summary>
		/// Call this when your player has reached the end of the module
		/// </summary>
		/********************************************************************/
		protected void OnEndReached()
		{
			if (EndReached != null)
				EndReached(this, EventArgs.Empty);
		}
		#endregion
	}
}