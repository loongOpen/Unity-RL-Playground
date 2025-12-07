// /***************************************************************************
//  * ScaleFactorData.cs
//  * Copyright (c) 2015 the authors.
//  * 
//  * All rights reserved. This program and the accompanying materials
//  * are made available under the terms of the GNU Lesser General Public License
//  * (LGPL) version 3 which accompanies this distribution, and is available at
//  * https://www.gnu.org/licenses/lgpl-3.0.en.html
//  *
//  * This library is distributed in the hope that it will be useful,
//  * but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  * Lesser General Public License for more details.
//  *
//  ***************************************************************************/

namespace MP3Sharp.Decoding.Decoders.LayerIII
{
    internal class ScaleFactorData
    {
        public int[] l; /* [cb] */
        public int[][] s; /* [window][cb] */

        /// <summary>
        ///     Dummy Constructor
        /// </summary>
        public ScaleFactorData()
        {
            l = new int[23];
            s = new int[3][];
            for (int i = 0; i < 3; i++)
            {
                s[i] = new int[13];
            }
        }
    }
}
