using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
class CRC32
{
    private static readonly uint[] Table = new uint[256];
    
    static CRC32()
    {
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 1) == 1)
                    crc = (crc >> 1) ^ 0xEDB88320;
                else
                    crc >>= 1;
            }
            Table[i] = crc;
        }
    }

    public static uint Calculate(byte[] data) {
        //string hexString = BitConverter.ToString(data); // 默认用 "-" 连接
        //Debug.Log("msg: " + hexString);
        
        uint crc = 0xFFFFFFFF;
        const uint polynomial = 0x04C11DB7; // 非反转多项式
        foreach (byte b in data) {
            crc ^= (uint)b << 24; // 将字节放到高位
            for (int i = 0; i < 8; i++) {
                if ((crc & 0x80000000) != 0)
                    crc = (crc << 1) ^ polynomial;
                else
                    crc <<= 1;
            }
        }
        return crc; // 不取反
    }
}
