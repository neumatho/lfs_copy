﻿/******************************************************************************/
/* This source, or parts thereof, may be used in any software as long the     */
/* license of NostalgicPlayer is keep. See the LICENSE file for more          */
/* information.                                                               */
/*                                                                            */
/* Copyright (C) 2021 by Polycode / NostalgicPlayer team.                     */
/* All rights reserved.                                                       */
/******************************************************************************/
namespace Polycode.NostalgicPlayer.Agent.Player.SidPlay.ReSidFp
{
	/// <summary>
	/// 
	/// </summary>
	internal class Integrator8580
	{
		// 8580 integrator
		//
		//                   +---C---+
		//                   |       |
		//     vi -----Rfc---o--[A>--o-- vo
		//                   vx
		//
		//     IRfc + ICr = 0
		//     IRfc + C*(vc - vc0)/dt = 0
		//     dt/C*(IRfc) + vc - vc0 = 0
		//     vc = vc0 - n*(IRfc(vi,vx))
		//     vc = vc0 - n*(IRfc(vi,g(vc)))
		//
		// IRfc = K*W/L*(Vgst^2 - Vgdt^2) = n*((Vddt - vx)^2 - (Vddt - vi)^2)
		//
		// Rfc gate voltage is generated by an OP Amp and depends on chip temperature.

		private readonly ushort[] opamp_rev;

		private int vx;
		private int vc;

		private ushort nVgt;
		private ushort n_dac;

		private readonly double vth;
		private readonly double nKp;
		private readonly double vMin;
		private readonly double n16;

		/********************************************************************/
		/// <summary>
		/// Constructor
		/// </summary>
		/********************************************************************/
		public Integrator8580(ushort[] opamp_rev, double vth, double nKp, double vMin, double n16)
		{
			this.opamp_rev = opamp_rev;
			vx = 0;
			vc = 0;
			this.vth = vth;
			this.nKp = nKp;
			this.vMin = vMin;
			this.n16 = n16;

			SetV(1.5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public void SetFc(double wl)
		{
			// Normalized current factor, 1 cycle at 1Mhz.
			// Fit in 5 bits
			double tmp = (1 << 13) * nKp * wl;
			n_dac = (ushort)(tmp + 0.5);
		}



		/********************************************************************/
		/// <summary>
		/// Set FC gate voltage multiplier
		/// </summary>
		/********************************************************************/
		public void SetV(double v)
		{
			// Gate voltage is controlled by the switched capacitor voltage divider
			// Ua = Ue * v = 4.76v  1<v<2
			double vg = 4.76 * v;
			double vgt = vg - vth;

			// Vg - Vth, normalized so that translated values can be subtracted:
			// Vgt - x = (Vgt - t) - (x - t)
			double tmp = n16 * (vgt - vMin);
			nVgt = (ushort)(tmp + 0.5);
		}



		/********************************************************************/
		/// <summary>
		/// 
		/// </summary>
		/********************************************************************/
		public int Solve(int vi)
		{
			// DAC voltages
			uint vgst = (uint)(nVgt - vx);
			uint vgdt = (uint)((vi < nVgt) ? nVgt - vi : 0);	// triode/saturation mode

			uint vgst_2 = vgst * vgst;
			uint vgdt_2 = vgdt * vgdt;

			// DAC current, scaled by (1/m)*2^13*m*2^16*m*2^16*2^-15 = m*2^30
			int n_I_dac = n_dac * ((int)(vgst_2 - vgdt_2) >> 15);

			// Change in capacitor charge
			vc += n_I_dac;

			// vx = g(vc)
			int tmp = (vc >> 15) + (1 << 15);
			vx = opamp_rev[tmp];

			// Return vo
			return vx - (vc >> 14);
		}
	}
}
