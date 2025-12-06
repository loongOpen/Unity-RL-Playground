// /***************************************************************************
//  * GranuleInfo.cs
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
    internal class GranuleInfo
    {
        public int BigValues;
        public int BlockType;
        public int Count1TableSelect;
        public int GlobalGain;
        public int MixedBlockFlag;
        public int Part23Length;
        public int Preflag;
        public int Region0Count;
        public int Region1Count;
        public int ScaleFacCompress;
        public int ScaleFacScale;
        public int[] SubblockGain;
        public int[] TableSelect;
        public int WindowSwitchingFlag;

        /// <summary>
        ///     Dummy Constructor
        /// </summary>
        public GranuleInfo()
        {
            TableSelect = new int[3];
            SubblockGain = new int[3];
        }
    }
}
