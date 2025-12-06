// /***************************************************************************
//  * BitstreamException.cs
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

using System;

namespace MP3Sharp.Decoding
{
    /// <summary>
    ///     Instances of BitstreamException are thrown
    ///     when operations on a Bitstream fail.
    ///     <p>
    ///     The exception provides details of the exception condition
    ///     in two ways:
    ///     <ol>
    ///         <li>
    ///             as an error-code describing the nature of the error
    ///         </li>
    ///         <br></br>
    ///         <li>
    ///             as the Throwable instance, if any, that was thrown
    ///             indicating that an exceptional condition has occurred.
    ///         </li>
    ///     </ol>
    ///     </p>
    /// </summary>
    [Serializable]
    public class BitstreamException : MP3SharpException
    {
        private int m_Errorcode;

        public BitstreamException(string message, Exception inner) : base(message, inner)
        {
            InitBlock();
        }

        public BitstreamException(int errorcode, Exception inner) : this(GetErrorString(errorcode), inner)
        {
            InitBlock();
            m_Errorcode = errorcode;
        }

        public virtual int ErrorCode
        {
            get { return m_Errorcode; }
        }

        private void InitBlock()
        {
            m_Errorcode = BitstreamErrors.UNKNOWN_ERROR;
        }

        public static string GetErrorString(int errorcode)
        {
            return "Bitstream errorcode " + Convert.ToString(errorcode, 16);
        }
    }
}