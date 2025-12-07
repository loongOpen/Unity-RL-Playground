// /***************************************************************************
//  * BitstreamErrors.cs
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
namespace MP3Sharp.Decoding
{
    /// <summary>
    ///     This struct describes all error codes that can be thrown
    ///     in BistreamExceptions.
    /// </summary>
    internal struct BitstreamErrors
    {
        public static readonly int UNKNOWN_ERROR = BITSTREAM_ERROR + 0;
        public static readonly int UNKNOWN_SAMPLE_RATE = BITSTREAM_ERROR + 1;
        public static readonly int STREAM_ERROR = BITSTREAM_ERROR + 2;
        public static readonly int UNEXPECTED_EOF = BITSTREAM_ERROR + 3;
        public static readonly int STREAM_EOF = BITSTREAM_ERROR + 4;
        public static readonly int BITSTREAM_LAST = 0x1ff;

        public static readonly int BITSTREAM_ERROR = 0x100;
        public static readonly int DECODER_ERROR = 0x200;
    }
}