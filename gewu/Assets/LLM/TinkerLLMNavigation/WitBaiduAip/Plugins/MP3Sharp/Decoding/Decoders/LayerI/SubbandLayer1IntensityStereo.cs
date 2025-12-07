// /***************************************************************************
//  * SubbandLayer1IntensityStereo.cs
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

namespace MP3Sharp.Decoding.Decoders.LayerI
{
    /// <summary>
    ///     Class for layer I subbands in joint stereo mode.
    /// </summary>
    internal class SubbandLayer1IntensityStereo : SubbandLayer1
    {
        protected internal float channel2_scalefactor;

        /// <summary>
        ///     Constructor
        /// </summary>
        public SubbandLayer1IntensityStereo(int subbandnumber)
            : base(subbandnumber)
        {
        }

        /// <summary>
        ///     *
        /// </summary>
        public override void ReadBitAllocation(Bitstream stream, Header header, Crc16 crc)
        {
            base.ReadBitAllocation(stream, header, crc);
        }

        /// <summary>
        ///     *
        /// </summary>
        public override void ReadScaleFactor(Bitstream stream, Header header)
        {
            if (allocation != 0)
            {
                scalefactor = ScaleFactors[stream.GetBitsFromBuffer(6)];
                channel2_scalefactor = ScaleFactors[stream.GetBitsFromBuffer(6)];
            }
        }

        /// <summary>
        ///     *
        /// </summary>
        public override bool ReadSampleData(Bitstream stream)
        {
            return base.ReadSampleData(stream);
        }

        /// <summary>
        ///     *
        /// </summary>
        public override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2)
        {
            if (allocation != 0)
            {
                sample = sample * factor + offset; // requantization
                if (channels == OutputChannels.BOTH_CHANNELS)
                {
                    float sample1 = sample * scalefactor, sample2 = sample * channel2_scalefactor;
                    filter1.WriteSample(sample1, subbandnumber);
                    filter2.WriteSample(sample2, subbandnumber);
                }
                else if (channels == OutputChannels.LEFT_CHANNEL)
                {
                    float sample1 = sample * scalefactor;
                    filter1.WriteSample(sample1, subbandnumber);
                }
                else
                {
                    float sample2 = sample * channel2_scalefactor;
                    filter1.WriteSample(sample2, subbandnumber);
                }
            }
            return true;
        }
    }
}
