﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Linq;
using Polycode.NostalgicPlayer.Kit.Containers;
using Polycode.NostalgicPlayer.Kit.Interfaces;
using Polycode.NostalgicPlayer.PlayerLibrary.Containers;
using Polycode.NostalgicPlayer.PlayerLibrary.Resampler;

namespace Polycode.NostalgicPlayer.PlayerLibrary.Players
{
	/// <summary>
	/// Main class to play samples
	/// </summary>
	internal class SamplePlayer : ISamplePlayer
	{
		private ISamplePlayerAgent currentPlayer;

		private IOutputAgent outputAgent;
		private ResamplerStream soundStream;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SamplePlayer()
		{
			// Initialize member variables
			StaticModuleInformation = new ModuleInfoStatic();
			PlayingModuleInformation = new ModuleInfoFloating();

			currentPlayer = null;
		}

		#region IPlayer implementation
		/********************************************************************/
		/// <summary>
		/// Will initialize the player
		/// </summary>
		/********************************************************************/
		public bool InitPlayer(PlayerConfiguration playerConfiguration, out string errorMessage)
		{
			errorMessage = string.Empty;
			bool initOk;

			try
			{
				outputAgent = playerConfiguration.OutputAgent;

				Loader loader = playerConfiguration.Loader;

				// Remember the player
				currentPlayer = (ISamplePlayerAgent)loader.PlayerAgent;

				lock (currentPlayer)
				{
					// Initialize the player
					initOk = currentPlayer.InitPlayer(loader.Stream);

					if (initOk)
					{
						// Subscribe the events
						currentPlayer.PositionChanged += Player_PositionChanged;
						currentPlayer.ModuleInfoChanged += Player_ModuleInfoChanged;

						// Initialize module information
						StaticModuleInformation = new ModuleInfoStatic(loader.PlayerAgentInfo, currentPlayer.ModuleName.Trim(), currentPlayer.Author.Trim(), loader.ModuleFormat, loader.PlayerName, currentPlayer.ChannelCount, loader.ModuleSize, currentPlayer.SupportFlags, currentPlayer.Frequency);

						// Initialize the mixer
						soundStream = new ResamplerStream();
						soundStream.EndReached += Stream_EndReached;

						initOk = soundStream.Initialize(playerConfiguration, out errorMessage);

						if (!initOk)
							CleanupPlayer();
					}
					else
						CleanupPlayer();
				}
			}
			catch (Exception ex)
			{
				CleanupPlayer();

				errorMessage = string.Format(Resources.IDS_ERR_PLAYER_INIT, ex.Message);
				initOk = false;
			}

			return initOk;
		}



		/********************************************************************/
		/// <summary>
		/// Will cleanup the player
		/// </summary>
		/********************************************************************/
		public void CleanupPlayer()
		{
			try
			{
				if (currentPlayer != null)
				{
					// End the mixer
					if (soundStream != null)
					{
						soundStream.EndReached -= Stream_EndReached;

						soundStream.Cleanup();
						soundStream.Dispose();
						soundStream = null;
					}

					// Shutdown the player
					currentPlayer.CleanupSound();
					currentPlayer.CleanupPlayer();

					// Unsubscribe the events
					currentPlayer.PositionChanged -= Player_PositionChanged;
					currentPlayer.ModuleInfoChanged -= Player_ModuleInfoChanged;

					currentPlayer = null;

					// Clear player information
					StaticModuleInformation = new ModuleInfoStatic();
					PlayingModuleInformation = new ModuleInfoFloating();
				}
			}
			catch (Exception)
			{
				// Just ignore it
				;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will start playing the music
		/// </summary>
		/********************************************************************/
		public bool StartPlaying(Loader loader, MixerConfiguration newMixerConfiguration)
		{
			if (newMixerConfiguration != null)
				soundStream.ChangeConfiguration(newMixerConfiguration);

			lock (currentPlayer)
			{
				currentPlayer.InitSound();

				// Find the length of the song
				int songLength = currentPlayer.SongLength;

				// Get the position times for the current song
				TimeSpan totalTime = currentPlayer.GetPositionTimeTable(out TimeSpan[] positionTimes);

				// Initialize the module information
				PlayingModuleInformation = new ModuleInfoFloating(totalTime, currentPlayer.SongPosition, songLength, positionTimes, PlayerHelper.GetModuleInformation(currentPlayer).ToArray());
			}

			soundStream.Start();

			if (outputAgent.SwitchStream(soundStream, loader.FileName, StaticModuleInformation.ModuleName, StaticModuleInformation.Author) == AgentResult.Error)
				return false;

			outputAgent.Play();

			return true;
		}



		/********************************************************************/
		/// <summary>
		/// Will stop the playing
		/// </summary>
		/********************************************************************/
		public void StopPlaying(bool stopOutputAgent)
		{
			if (currentPlayer != null)
			{
				// Stop the mixer
				if (stopOutputAgent)
					outputAgent.Stop();

				soundStream.Stop();

				// Cleanup the player
				lock (currentPlayer)
				{
					currentPlayer.CleanupSound();
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will pause the playing
		/// </summary>
		/********************************************************************/
		public void PausePlaying()
		{
			if (currentPlayer != null)
			{
				outputAgent.Pause();
				soundStream.Pause();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will resume the playing
		/// </summary>
		/********************************************************************/
		public void ResumePlaying()
		{
			if (currentPlayer != null)
			{
				soundStream.Resume();
				outputAgent.Play();
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will set the master volume
		/// </summary>
		/********************************************************************/
		public void SetMasterVolume(int volume)
		{
			soundStream?.SetMasterVolume(volume);
		}



		/********************************************************************/
		/// <summary>
		/// Will change the mixer settings that can be change real-time
		/// </summary>
		/********************************************************************/
		public void ChangeMixerSettings(MixerConfiguration mixerConfiguration)
		{
			soundStream?.ChangeConfiguration(mixerConfiguration);
		}



		/********************************************************************/
		/// <summary>
		/// Return all the static information about the module
		/// </summary>
		/********************************************************************/
		public ModuleInfoStatic StaticModuleInformation
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Return all the information about the module which changes while
		/// playing
		/// </summary>
		/********************************************************************/
		public ModuleInfoFloating PlayingModuleInformation
		{
			get; private set;
		}



		/********************************************************************/
		/// <summary>
		/// Event called when the player reached the end
		/// </summary>
		/********************************************************************/
		public event EventHandler EndReached;



		/********************************************************************/
		/// <summary>
		/// Event called when the player change some of the module information
		/// </summary>
		/********************************************************************/
		public event ModuleInfoChangedEventHandler ModuleInfoChanged;
		#endregion

		#region ISamplePlayer implementation
		/********************************************************************/
		/// <summary>
		/// Will set a new song position
		/// </summary>
		/********************************************************************/
		public void SetSongPosition(int position)
		{
			if (currentPlayer != null)
			{
				lock (currentPlayer)
				{
					currentPlayer.SongPosition = position;
				}

				PlayingModuleInformation.SongPosition = position;
			}
		}



		/********************************************************************/
		/// <summary>
		/// Event called when the player change position
		/// </summary>
		/********************************************************************/
		public event EventHandler PositionChanged;
		#endregion

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called every time the player changed position
		/// </summary>
		/********************************************************************/
		private void Player_PositionChanged(object sender, EventArgs e)
		{
			if (currentPlayer != null)
			{
				lock (currentPlayer)
				{
					// Update the position
					PlayingModuleInformation.SongPosition = currentPlayer.SongPosition;
				}

				// Call the next event handler
				if (PositionChanged != null)
					PositionChanged(sender, e);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player change some of the module information
		/// </summary>
		/********************************************************************/
		private void Player_ModuleInfoChanged(object sender, ModuleInfoChangedEventArgs e)
		{
			if (currentPlayer != null)
			{
				// Just call the next event handler
				if (ModuleInfoChanged != null)
					ModuleInfoChanged(sender, e);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Is called when the player has reached the end
		/// </summary>
		/********************************************************************/
		private void Stream_EndReached(object sender, EventArgs e)
		{
			if (currentPlayer != null)
			{
				// Just call the next event handler
				if (EndReached != null)
					EndReached(sender, e);
			}
		}
		#endregion
	}
}