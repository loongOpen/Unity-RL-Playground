// /***************************************************************************
//  * SBI.cs
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
    internal class SBI
    {
        public int[] l;
        public int[] s;

        public SBI()
        {
            l = new int[23];
            s = new int[14];
        }

        public SBI(int[] thel, int[] thes)
        {
            l = thel;
            s = thes;
        }
    }
}
