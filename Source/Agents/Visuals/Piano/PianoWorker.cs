﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021-2022 by Polycode / NostalgicPlayer team.                */
/* All rights reserved.                                                       */
/******************************************************************************/
using Polycode.NostalgicPlayer.Agent.Visual.Piano.Display;
using Polycode.NostalgicPlayer.GuiKit.Interfaces;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;

namespace Polycode.NostalgicPlayer.Agent.Visual.Piano
{
	/// <summary>
	/// Main worker class
	/// </summary>
	internal class PianoWorker : IChannelChangeVisualAgent, IAgentGuiDisplay
	{
		private PianoControl userControl;

		#region IAgentDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return some flags telling how to set up the display window
		/// </summary>
		/********************************************************************/
		public DisplayFlag Flags => DisplayFlag.StaticWindow;
		#endregion

		#region IAgentGuiDisplay implementation
		/********************************************************************/
		/// <summary>
		/// Return the user control to show
		/// </summary>
		/********************************************************************/
		public UserControl GetUserControl()
		{
			userControl = new PianoControl();
			return userControl;
		}
		#endregion

		#region IVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Initializes the visual
		/// </summary>
		/********************************************************************/
		public void InitVisual(int channels, int virtualChannels)
		{
			userControl.InitVisual(virtualChannels);
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the visual
		/// </summary>
		/********************************************************************/
		public void CleanupVisual()
		{
			userControl.CleanupVisual();
		}



		/********************************************************************/
		/// <summary>
		/// Set the pause state
		/// </summary>
		/********************************************************************/
		public void SetPauseState(bool paused)
		{
			userControl.SetPauseState(paused);
		}
		#endregion

		#region IChannelChangeVisualAgent implementation
		/********************************************************************/
		/// <summary>
		/// Tell the visual about a channel change
		/// </summary>
		/********************************************************************/
		public void ChannelChange(ChannelChanged channelChanged)
		{
			userControl.ChannelChange(channelChanged);
		}
		#endregion
	}
}