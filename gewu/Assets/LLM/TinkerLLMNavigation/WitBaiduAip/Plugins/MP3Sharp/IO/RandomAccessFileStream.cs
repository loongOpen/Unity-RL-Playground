// /***************************************************************************
//  * RandomAccessFileStream.cs
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
using System.IO;

namespace MP3Sharp.IO
{
    internal class RandomAccessFileStream
    {
        public static FileStream CreateRandomAccessFile(string fileName, string mode)
        {
            FileStream newFile = null;

            if (mode.CompareTo("rw") == 0)
                newFile = new FileStream(fileName, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite);
            else if (mode.CompareTo("r") == 0)
                newFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            else
                throw new ArgumentException();

            return newFile;
        }
    }
}
