// /***************************************************************************
//  * ChannelData.cs
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
    internal class ChannelData
    {
        public GranuleInfo[] Granules;
        public int[] ScaleFactorBits;

        /// <summary>
        ///     Dummy Constructor
        /// </summary>
        public ChannelData()
        {
            ScaleFactorBits = new int[4];
            Granules = new GranuleInfo[2];
            Granules[0] = new GranuleInfo();
            Granules[1] = new GranuleInfo();
        }
    }
}
