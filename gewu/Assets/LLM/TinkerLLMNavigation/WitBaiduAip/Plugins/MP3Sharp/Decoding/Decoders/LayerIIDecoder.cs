// /***************************************************************************
//  * LayerIIDecoder.cs
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

using MP3Sharp.Decoding.Decoders.LayerII;

namespace MP3Sharp.Decoding.Decoders
{
    /// <summary>
    ///     Implements decoding of MPEG Audio Layer II frames.
    /// </summary>
    internal class LayerIIDecoder : LayerIDecoder, IFrameDecoder
    {
        protected internal override void CreateSubbands()
        {
            int i;
            if (mode == Header.SINGLE_CHANNEL)
                for (i = 0; i < num_subbands; ++i)
                    subbands[i] = new SubbandLayer2(i);
            else if (mode == Header.JOINT_STEREO)
            {
                for (i = 0; i < header.intensity_stereo_bound(); ++i)
                    subbands[i] = new SubbandLayer2Stereo(i);
                for (; i < num_subbands; ++i)
                    subbands[i] = new SubbandLayer2IntensityStereo(i);
            }
            else
            {
                for (i = 0; i < num_subbands; ++i)
                    subbands[i] = new SubbandLayer2Stereo(i);
            }
        }

        protected internal override void ReadScaleFactorSelection()
        {
            for (int i = 0; i < num_subbands; ++i)
                ((SubbandLayer2) subbands[i]).read_scalefactor_selection(stream, crc);
        }
    }
}