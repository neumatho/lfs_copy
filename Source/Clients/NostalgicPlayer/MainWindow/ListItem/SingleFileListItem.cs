﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
using System.IO;
using Polycode.NostalgicPlayer.Kit.Streams;

namespace Polycode.NostalgicPlayer.Client.GuiPlayer.MainWindow.ListItem
{
	/// <summary>
	/// This class holds a list item pointing to a file
	/// </summary>
	public class SingleFileListItem : IModuleListItem
	{
		#region IModuleListItem implementation
		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public SingleFileListItem(string fullFileName)
		{
			FileName = fullFileName;
		}



		/********************************************************************/
		/// <summary>
		/// Return the name which is shown in the list
		/// </summary>
		/********************************************************************/
		public string DisplayName
		{
			get
			{
				return Path.GetFileName(FileName);
			}
		}



		/********************************************************************/
		/// <summary>
		/// Opens a stream containing the file data
		/// </summary>
		/********************************************************************/
		public ModuleStream OpenStream()
		{
			return new ModuleStream(new FileStream(FileName, FileMode.Open, FileAccess.Read));
		}
		#endregion

		/********************************************************************/
		/// <summary>
		/// Return the file name to the file
		/// </summary>
		/********************************************************************/
		public string FileName
		{
			get;
		}
	}
}
