// /***************************************************************************
//  * SubbandLayer1Stereo.cs
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
    ///     Class for layer I subbands in stereo mode.
    /// </summary>
    internal class SubbandLayer1Stereo : SubbandLayer1
    {
        protected internal int channel2_allocation;
        protected internal float channel2_factor, channel2_offset;
        protected internal float channel2_sample;
        protected internal int channel2_samplelength;
        protected internal float channel2_scalefactor;

        /// <summary>
        ///     Constructor
        /// </summary>
        public SubbandLayer1Stereo(int subbandnumber)
            : base(subbandnumber)
        {
        }

        /// <summary>
        ///     *
        /// </summary>
        public override void ReadBitAllocation(Bitstream stream, Header header, Crc16 crc)
        {
            allocation = stream.GetBitsFromBuffer(4);
            channel2_allocation = stream.GetBitsFromBuffer(4);
            if (crc != null)
            {
                crc.add_bits(allocation, 4);
                crc.add_bits(channel2_allocation, 4);
            }
            if (allocation != 0)
            {
                samplelength = allocation + 1;
                factor = TableFactor[allocation];
                offset = TableOffset[allocation];
            }
            if (channel2_allocation != 0)
            {
                channel2_samplelength = channel2_allocation + 1;
                channel2_factor = TableFactor[channel2_allocation];
                channel2_offset = TableOffset[channel2_allocation];
            }
        }

        /// <summary>
        ///     *
        /// </summary>
        public override void ReadScaleFactor(Bitstream stream, Header header)
        {
            if (allocation != 0)
                scalefactor = ScaleFactors[stream.GetBitsFromBuffer(6)];
            if (channel2_allocation != 0)
                channel2_scalefactor = ScaleFactors[stream.GetBitsFromBuffer(6)];
        }

        /// <summary>
        ///     *
        /// </summary>
        public override bool ReadSampleData(Bitstream stream)
        {
            bool returnvalue = base.ReadSampleData(stream);
            if (channel2_allocation != 0)
            {
                channel2_sample = stream.GetBitsFromBuffer(channel2_samplelength);
            }
            return (returnvalue);
        }

        /// <summary>
        ///     *
        /// </summary>
        public override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2)
        {
            base.PutNextSample(channels, filter1, filter2);
            if ((channel2_allocation != 0) && (channels != OutputChannels.LEFT_CHANNEL))
            {
                float sample2 = (channel2_sample * channel2_factor + channel2_offset) * channel2_scalefactor;
                if (channels == OutputChannels.BOTH_CHANNELS)
                    filter2.WriteSample(sample2, subbandnumber);
                else
                    filter1.WriteSample(sample2, subbandnumber);
            }
            return true;
        }
    }
}
