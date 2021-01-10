﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System;
using System.Windows.Forms;
using Polycode.NostalgicPlayer.NostalgicPlayer.MainWindow;

namespace Polycode.NostalgicPlayer.NostalgicPlayer
{
	/// <summary>
	/// Main entry point
	/// </summary>
	static class Program
	{
		/********************************************************************/
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		/********************************************************************/
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainWindowForm());
			}
			catch (Exception ex)
			{
				MessageBox.Show(string.Format(Properties.Resources.IDS_ERR_EXCEPTION, ex.Message), Properties.Resources.IDS_MAIN_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}