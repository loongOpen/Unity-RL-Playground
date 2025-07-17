
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MotorCmd
{
    public byte mode;
    public float q;
    public float dq;
    public float tau;
    public float Kp;
    public float Kd;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] reserve;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct BmsCmd
{
    public byte off;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] reserve;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LowCmd
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] head;
    public byte levelFlag;
    public byte frameReserve;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] SN;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] version;
    public ushort bandWidth;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
    public MotorCmd[] motorCmd;
    public BmsCmd bms;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
    public byte[] wirelessRemote;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] led;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public byte[] fan;
    public ushort gpio;
    public uint reserve;
    public uint crc;
}
