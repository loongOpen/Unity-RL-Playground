// /***************************************************************************
//  * MP3SharpException.cs
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

using MP3Sharp.Support;
using System;
using System.IO;

namespace MP3Sharp
{
    /// <summary>
    ///     MP3SharpException is the base class for all API-level
    ///     exceptions thrown by MP3Sharp. To facilitate conversion and
    ///     common handling of exceptions from other domains, the class
    ///     can delegate some functionality to a contained Throwable instance.
    /// </summary>
    [Serializable]
    public class MP3SharpException : Exception
    {
        public MP3SharpException()
        {
        }

        public MP3SharpException(string message) : base(message)
        {
        }

        public MP3SharpException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}