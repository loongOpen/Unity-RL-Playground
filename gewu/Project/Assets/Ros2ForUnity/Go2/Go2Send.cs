// Copyright 2019-2021 Robotec.ai.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;

namespace ROS2
{

/// <summary>
/// An example class provided for testing of basic ROS2 communication
/// </summary>
public class Go2Send : MonoBehaviour
{
    // Start is called before the first frame update
    private ROS2UnityComponent ros2Unity;
    private ROS2Node ros2Node;
    private IPublisher<unitree_go.msg.LowCmd> chatter_pub;
    unitree_go.msg.LowCmd cmd_msg = new unitree_go.msg.LowCmd();
    unitree_go.msg.MotorState[] motor = new unitree_go.msg.MotorState[12]; // Unitree go2 motor state message

    private int i;

    float PosStopF = 2.146E+9f;
    float VelStopF = 16000.0f;
    float[] qmid = new float[12]{0f,1.2f,-2f, 0f,1.2f,-2f, 0f,1.2f,-2f, 0f,1.2f,-2f};
    float[] qsit = new float[12]{0f,1.4f,-2.1f, 0f,1.4f,-2.1f, 0f,1.4f,-2.1f, 0f,1.4f,-2.1f};
    float[] qdes = new float[12];

    void Start()
    {
        ros2Unity = GetComponent<ROS2UnityComponent>();
        for (int i = 0; i < 12; i++)
        {
            motor[i] = new unitree_go.msg.MotorState();
        }
        cmd_msg.Head[0] = 0xFE;
        cmd_msg.Head[1] = 0xEF;
        cmd_msg.Level_flag = 0xFF;
        cmd_msg.Gpio = 0;
        for (int i = 0; i < 20; i++)
        {
            cmd_msg.Motor_cmd[i] = new unitree_go.msg.MotorCmd();
            cmd_msg.Motor_cmd[i].Mode = 0x01; //Set toque mode, 0x00 is passive mode
            cmd_msg.Motor_cmd[i].Q = PosStopF;
            cmd_msg.Motor_cmd[i].Kp = 0;
            cmd_msg.Motor_cmd[i].Dq = VelStopF;
            cmd_msg.Motor_cmd[i].Kd = 0;
            cmd_msg.Motor_cmd[i].Tau = 0;
        }
    }

    void Update()
    {
        if (ros2Unity.Ok())
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            if (ros2Node == null)
            {
                ros2Node = ros2Unity.CreateNode("ros2_unity_go2_send_node");
                chatter_pub = ros2Node.CreatePublisher<unitree_go.msg.LowCmd>("/lowcmd");
            }

            i++;
            //print(i);
            for(int i=0;i<12;i++)SetTargetRad(i, qsit[i]);
            GetCRC(ref cmd_msg);
            chatter_pub.Publish(cmd_msg);

            // 停止计时并输出耗时
            stopwatch.Stop();
            //UnityEngine.Debug.Log($"Execution Time: {stopwatch.Elapsed.TotalMicroseconds} μs");
            long microseconds = stopwatch.Elapsed.Ticks / 10; // 1 Tick = 100 ns → 10 Ticks = 1 μs
            UnityEngine.Debug.Log($"Execution Time: {microseconds} μs");
        }
    }

    void SetTargetRad(int idx, float pos)//20+0.5,30+1standing
    {
        cmd_msg.Motor_cmd[idx].Q = pos;   // Taregt angular(rad)
        cmd_msg.Motor_cmd[idx].Kp = 50.0f; // Poinstion(rad) control kp gain20+0.5, 50+5.5+1
        cmd_msg.Motor_cmd[idx].Dq = 0.0f;  // Taregt angular velocity(rad/ss)
        cmd_msg.Motor_cmd[idx].Kd = 2.0f;  // Poinstion(rad) control kd gain
        cmd_msg.Motor_cmd[idx].Tau = 0.0f; // Feedforward toque 1N.m
    }
    void GetCRC(ref unitree_go.msg.LowCmd cmd)
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            // 写入头部字段
            //writer.Write(cmd.Head);       // byte[]
            if (cmd.Head != null && cmd.Head.Length >= 2)
            {
                writer.Write(cmd.Head[0]); // 写入第一个 byte
                writer.Write(cmd.Head[1]); // 写入第二个 byte
            }
            else
            {
                // 如果 Head 是 null 或长度不足，写入默认值（0）
                writer.Write((byte)0);
                writer.Write((byte)0);
            }
            writer.Write(cmd.Level_flag);               // byte
            writer.Write(cmd.Frame_reserve);            // byte
            //writer.Write(cmd.Sn);        // byte[]
            // 处理 Sn（uint32[2]）
            if (cmd.Sn != null && cmd.Sn.Length >= 2)
            {
                writer.Write(cmd.Sn[0]); // 写入第一个 uint
                writer.Write(cmd.Sn[1]); // 写入第二个 uint
            }
            else
            {
                // 如果 Sn 是 null 或长度不足，写入默认值（例如 0）
                writer.Write(0u);
                writer.Write(0u);
            }
            //writer.Write(cmd.Version);   // byte[]
            if (cmd.Version != null && cmd.Version.Length >= 2)
            {
                writer.Write(cmd.Version[0]); // 写入第一个 uint
                writer.Write(cmd.Version[1]); // 写入第二个 uint
            }
            else
            {
                // 如果 Sn 是 null 或长度不足，写入默认值（例如 0）
                writer.Write(0u);
                writer.Write(0u);
            }
            writer.Write(cmd.Bandwidth);                // byte
            writer.Write((byte)0);
            writer.Write((byte)0);//****************************************************************************************
            
            // 写入 20 个 MotorCmd
            for (int i = 0; i < 20; i++)
            {
                writer.Write(cmd.Motor_cmd[i].Mode);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);//****************************************************************************************
            
                writer.Write(cmd.Motor_cmd[i].Q);
                writer.Write(cmd.Motor_cmd[i].Dq);
                writer.Write(cmd.Motor_cmd[i].Tau);
                writer.Write(cmd.Motor_cmd[i].Kp);
                writer.Write(cmd.Motor_cmd[i].Kd);
                
                //writer.Write(cmd.Motor_cmd[i].Reserve);
                // 处理 Reserve（uint32[]）
                if (cmd.Motor_cmd[i].Reserve != null)
                {
                    foreach (uint value in cmd.Motor_cmd[i].Reserve)
                    {
                        writer.Write(value); // BinaryWriter 支持 Write(uint)
                    }
                }
                else
                {
                    // 如果 Reserve 是 null，写入默认值（例如 0）
                    writer.Write(0u); // 假设 Reserve 至少有 1 个 uint
                    writer.Write(0u);
                    writer.Write(0u);//**********************************************************
                }
                
            }
            for(int k=0;k<16;k++)writer.Write(0u);
    
            // 计算 CRC
            byte[] arr = ms.ToArray();
            // 每 4 字节（32位）反转一次
            for (int i = 0; i < arr.Length; i += 4)
            {
                if (i + 4 <= arr.Length) // 确保不越界
                {
                    Array.Reverse(arr, i, 4); // 反转从 i 开始的 4 字节
                }
            }
            cmd.Crc = CRC32.Calculate(arr);
            //Debug.Log("CRC: " + cmd.Crc);
            //print(cmd.Crc);
        }
    }
}

}  // namespace ROS2
