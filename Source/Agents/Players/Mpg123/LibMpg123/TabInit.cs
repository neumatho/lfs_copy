﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/******************************************************************************/
using System;

namespace Polycode.NostalgicPlayer.Agent.Player.Mpg123.LibMpg123
{
	/// <summary>
	/// Initialize tables
	/// </summary>
	internal class TabInit
	{
		public readonly Real[][] Pnts = { CosTabs.Cos64, CosTabs.Cos32, CosTabs.Cos16, CosTabs.Cos8, CosTabs.Cos4 };

		private static readonly c_long[] intWinBase =
		{
			    0,    -1,    -1,    -1,    -1,    -1,    -1,    -2,    -2,    -2,
			   -2,    -3,    -3,    -4,    -4,    -5,    -5,    -6,    -7,    -7,
			   -8,    -9,   -10,   -11,   -13,   -14,   -16,   -17,   -19,   -21,
			  -24,   -26,   -29,   -31,   -35,   -38,   -41,   -45,   -49,   -53,
			  -58,   -63,   -68,   -73,   -79,   -85,   -91,   -97,  -104,  -111,
			 -117,  -125,  -132,  -139,  -147,  -154,  -161,  -169,  -176,  -183,
			 -190,  -196,  -202,  -208,  -213,  -218,  -222,  -225,  -227,  -228,
			 -228,  -227,  -224,  -221,  -215,  -208,  -200,  -189,  -177,  -163,
			 -146,  -127,  -106,   -83,   -57,   -29,     2,    36,    72,   111,
			  153,   197,   244,   294,   347,   401,   459,   519,   581,   645,
			  711,   779,   848,   919,   991,  1064,  1137,  1210,  1283,  1356,
			 1428,  1498,  1567,  1634,  1698,  1759,  1817,  1870,  1919,  1962,
			 2001,  2032,  2057,  2075,  2085,  2087,  2080,  2063,  2037,  2000,
			 1952,  1893,  1822,  1739,  1644,  1535,  1414,  1280,  1131,   970,
			  794,   605,   402,   185,   -45,  -288,  -545,  -814, -1095, -1388,
			-1692, -2006, -2330, -2663, -3004, -3351, -3705, -4063, -4425, -4788,
			-5153, -5517, -5879, -6237, -6589, -6935, -7271, -7597, -7910, -8209,
			-8491, -8755, -8998, -9219, -9416, -9585, -9727, -9838, -9916, -9959,
			-9966, -9935, -9863, -9750, -9592, -9389, -9139, -8840, -8492, -8092,
			-7640, -7134, -6574, -5959, -5288, -4561, -3776, -2935, -2037, -1082,
			  -70,   998,  2122,  3300,  4533,  5818,  7154,  8540,  9975, 11455,
			12980, 14548, 16155, 17799, 19478, 21189, 22929, 24694, 26482, 28289,
			30112, 31947, 33791, 35640, 37489, 39336, 41176, 43006, 44821, 46617,
			48390, 50137, 51853, 53534, 55178, 56778, 58333, 59838, 61289, 62684,
			64019, 65290, 66494, 67629, 68692, 69679, 70590, 71420, 72169, 72835,
			73415, 73908, 74313, 74630, 74856, 74992, 75038
		};

		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void Make_Decode_Tables(Mpg123_Handle fr)
		{
			Span<Real> decWin = fr.DecWin.Span;
			c_int i, j;
			c_int idx = 0;

			// Scale is always based on 1.0
			c_double scaleVal = -0.5 * (fr.LastScale < 0 ? fr.P.OutScale : fr.LastScale);

			for (i = 0, j = 0; i < 256; i++, j++, idx += 32)
			{
				if (idx < 512 + 16)
					decWin[idx + 16] = decWin[idx] = Helpers.Double_To_Real(intWinBase[j] * scaleVal);

				if ((i % 32) == 31)
					idx -= 1023;

				if ((i % 64) == 63)
					scaleVal = -scaleVal;
			}

			for (; i < 512; i++, j--, idx += 32)
			{
				if (idx < 512 + 16)
					decWin[idx + 16] = decWin[idx] = Helpers.Double_To_Real(intWinBase[j] * scaleVal);

				if ((i % 32) == 31)
					idx -= 1023;

				if ((i % 64) == 63)
					scaleVal = -scaleVal;
			}
		}
	}
}
