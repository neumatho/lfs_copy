﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Drawing;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.Kit.Containers;

namespace Polycode.NostalgicPlayer.Agent.Visual.SpinningSquares.Display
{
	/// <summary>
	/// The user control holding the squares
	/// </summary>
	internal partial class SpinningSquaresControl : UserControl
	{
		private const int PanelMargin = 4;

		private int channelsInUse = 0;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SpinningSquaresControl()
		{
			InitializeComponent();
		}



		/********************************************************************/
		/// <summary>
		/// Initializes the visual
		/// </summary>
		/********************************************************************/
		public void InitVisual(int channels)
		{
			channelsInUse = Math.Min(channels, 16);

			CreateChannelPanels();

			hashPanel.Visible = false;
			squaresPanel.Visible = true;

			pulseTimer.Start();
		}



		/********************************************************************/
		/// <summary>
		/// Cleanup the visual
		/// </summary>
		/********************************************************************/
		public void CleanupVisual()
		{
			pulseTimer.Stop();

			channelsInUse = 0;

			squaresPanel.Visible = false;
			hashPanel.Visible = true;

			DestroyChannelPanels();
		}



		/********************************************************************/
		/// <summary>
		/// Tell the visual about a channel change
		/// </summary>
		/********************************************************************/
		public void ChannelChange(ChannelChanged channelChanged)
		{
			for (int i = 0; i < channelsInUse; i++)
			{
				if (squaresPanel.Controls[i].Controls[0] is SingleSpinningSquareControl singleSpinningSquare)
					singleSpinningSquare.ChannelChange(channelChanged.Flags[i], channelChanged.VirtualChannels[i]);
			}
		}

		#region Event handlers
		/********************************************************************/
		/// <summary>
		/// Is called every time the squares panel is resize to recalculate
		/// position and sizes
		/// </summary>
		/********************************************************************/
		private void SquaresPanel_Resize(object sender, EventArgs e)
		{
			LayoutPanels();
		}



		/********************************************************************/
		/// <summary>
		/// Is called 50 times per second and do the animation
		/// </summary>
		/********************************************************************/
		private void PulseTimer_Tick(object sender, EventArgs e)
		{
			foreach (Control ctrl in squaresPanel.Controls)
			{
				if (ctrl.Controls[0] is SingleSpinningSquareControl singleSpinningSquare)
				{
					singleSpinningSquare.Animate();
					singleSpinningSquare.Invalidate();
				}
			}
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary>
		/// Create all the panels for the squares
		/// </summary>
		/********************************************************************/
		private void CreateChannelPanels()
		{
			int panelsToCreate = ((channelsInUse + 1) / 2) * 2;

			// First create all the panels
			for (int i = 0; i < panelsToCreate; i++)
			{
				Panel panel = new Panel();
				panel.BorderStyle = BorderStyle.Fixed3D;
				panel.Dock = DockStyle.None;

				Control ctrl;

				if (i < channelsInUse)
					ctrl = new SingleSpinningSquareControl();
				else
				{
					ctrl = new Panel();
					ctrl.BackgroundImage = Resources.IDB_HASH;
				}

				ctrl.Dock = DockStyle.Fill;
				panel.Controls.Add(ctrl);

				squaresPanel.Controls.Add(panel);
			}

			// Now layout the panels
			LayoutPanels();
		}



		/********************************************************************/
		/// <summary>
		/// Destroy channel panels
		/// </summary>
		/********************************************************************/
		private void DestroyChannelPanels()
		{
			foreach (Control control in squaresPanel.Controls)
				control.Dispose();

			squaresPanel.Controls.Clear();
		}



		/********************************************************************/
		/// <summary>
		/// Calculate the position and sizes for all the square panels
		/// </summary>
		/********************************************************************/
		private void LayoutPanels()
		{
			int colNum = (channelsInUse + 1) / 2;

			Size clientArea = ParentForm.ClientSize;

			int panelWidth = (clientArea.Width - PanelMargin * 2) / colNum;
			int panelHeight = (clientArea.Height - PanelMargin * 2) / 2;

			for (int i = 0; i < colNum; i++)
			{
				for (int j = 0; j < 2; j++)
				{
					Panel panel = (Panel)squaresPanel.Controls[i * 2 + j];

					panel.Size = new Size(panelWidth - PanelMargin * 2, panelHeight - PanelMargin * 2);
					panel.Location = new Point(PanelMargin * 2 + i * panelWidth, PanelMargin * 2 + j * panelHeight);
				}
			}
		}
		#endregion
	}
}
