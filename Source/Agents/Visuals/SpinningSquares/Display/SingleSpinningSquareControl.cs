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
using Polycode.NostalgicPlayer.Kit.Mixer;

namespace Polycode.NostalgicPlayer.Agent.Visual.SpinningSquares.Display
{
	/// <summary>
	/// This control draws a single spinning square
	/// </summary>
	internal partial class SingleSpinningSquareControl : UserControl
	{
		private const double BoxAreaSize = 92;

		private readonly Point[] boxCoords = new Point[4];		// A coordinate for each corner in the box
		private readonly Point[] drawCoords = new Point[4];		// Rotated and scaled coordinates

		private double angle;
		private double speed;
		private ushort oldVolume;
		private uint oldFrequency;

		private bool animate;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SingleSpinningSquareControl()
		{
			InitializeComponent();

			SetStyle(ControlStyles.UserPaint, true);

			boxCoords[0] = new Point(0, 0);
			boxCoords[1] = new Point(0, 0);
			boxCoords[2] = new Point(0, 0);
			boxCoords[3] = new Point(0, 0);

			drawCoords[0] = new Point(0, 0);
			drawCoords[1] = new Point(0, 0);
			drawCoords[2] = new Point(0, 0);
			drawCoords[3] = new Point(0, 0);

			angle = 0.0;
			speed = 0.5;
			oldVolume = 0;
			oldFrequency = 0;

			animate = false;
		}



		/********************************************************************/
		/// <summary> 
		/// Animate internal variables to the next frame
		/// </summary>
		/********************************************************************/
		public void Animate()
		{
			lock (this)
			{
				if (animate)
				{
					// Find factor to use to scale the box
					double factor = Math.Min(Width, Height) / BoxAreaSize;

					// Rotate the box
					//
					// Calculate new coordinates
					double radAngle = angle * 2 * Math.PI / 360;
					double sinAngle = Math.Sin(radAngle);
					double cosAngle = Math.Cos(radAngle);

					for (int i = 0; i < 4; i++)
					{
						drawCoords[i].X = (int)(((boxCoords[i].X * cosAngle) - (boxCoords[i].Y * sinAngle)) * factor + Width / 2);
						drawCoords[i].Y = (int)(((boxCoords[i].X * sinAngle) + (boxCoords[i].Y * cosAngle)) * factor + Height / 2);
					}

					// Calculate new angle
					angle += speed;

					while (angle >= 360)
						angle -= 360;

					while (angle < 0)
						angle += 360;
				}
			}
		}



		/********************************************************************/
		/// <summary>
		/// Will calculate new box coordinates depending on the information
		/// in the channel structure
		/// </summary>
		/********************************************************************/
		public void ChannelChange(Channel.Flags flags, Channel channel)
		{
			lock (this)
			{
				// Begin to rotate the box
				animate = true;

				// Has the volume changed?
				if ((flags & Channel.Flags.Volume) != 0)
				{
					ushort newVol = channel.GetVolume();
					if (newVol != oldVolume)
					{
						oldVolume = newVol;
						newVol /= 8;

						boxCoords[0].Y = newVol;
						boxCoords[1].Y = newVol;
						boxCoords[2].Y = -newVol;
						boxCoords[3].Y = -newVol;
					}
				}

				// Has the frequency changed?
				int cachedFreq = (int)oldFrequency;

				if ((flags & Channel.Flags.Frequency) != 0)
				{
					int newFreq = (int)channel.GetFrequency();
					if (newFreq != oldFrequency)
					{
						oldFrequency = (uint)newFreq;

						if (newFreq < 3547)
							newFreq = 3547;
						else
						{
							if (newFreq > 34104)
								newFreq = 34104;
						}

						newFreq -= (34104 + 3547);
						newFreq = -newFreq / 1066;

						boxCoords[0].X = -newFreq;
						boxCoords[1].X = newFreq;
						boxCoords[2].X = newFreq;
						boxCoords[3].X = -newFreq;
					}
				}

				// Calculate the rotation speed and direction
				double cachedSpeed = speed;

				if (oldVolume != 0)
				{
					double newSpeed = (double)cachedFreq / oldVolume / 8;
					if (newSpeed != 0)
					{
						cachedFreq -= (int)oldFrequency;
						if (cachedFreq < 0)
						{
							cachedFreq = -cachedFreq;
							newSpeed = -newSpeed;
						}

						if (cachedFreq >= 10)
							speed = newSpeed;
					}
				}

				if (cachedSpeed == speed)
				{
					if ((flags & Channel.Flags.TrigIt) != 0)
						speed = -speed;
				}
			}
		}

		#region Event handlers
		/********************************************************************/
		/// <summary> 
		/// Is called every time an update is needed
		/// </summary>
		/********************************************************************/
		private void Control_Paint(object sender, PaintEventArgs e)
		{
			UpdateWindow(e.Graphics);
		}
		#endregion

		#region Private methods
		/********************************************************************/
		/// <summary> 
		/// Update the window
		/// </summary>
		/********************************************************************/
		private void UpdateWindow(Graphics g)
		{
			lock (this)
			{
				// First clear the background
				g.FillRectangle(Brushes.White, 0, 0, Width, Height);

				// Draw the box
				g.FillPolygon(Brushes.DodgerBlue, drawCoords);
			}
		}
		#endregion
	}
}
