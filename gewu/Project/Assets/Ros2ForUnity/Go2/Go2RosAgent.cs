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
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Threading;
using System;
using System.Runtime.InteropServices;
using System.IO;
using Unity.Sentis;

namespace ROS2
{

/// <summary>
/// An example class provided for performance testing of ROS2 communication
/// </summary>
public class Go2RosAgent : Agent
{
    private int interval_ms = 1;
    private ROS2UnityComponent ros2Unity;
    private ROS2Node ros2Node;
    private ROS2Node ros2Node2;
    private IPublisher<unitree_go.msg.LowCmd> go2_pub;
    unitree_go.msg.LowCmd cmd_msg = new unitree_go.msg.LowCmd();
    private ISubscription<unitree_go.msg.LowState> go2_sub;
    unitree_go.msg.IMUState imu = new unitree_go.msg.IMUState();         // Unitree go2 IMU message
    unitree_go.msg.MotorState[] motor = new unitree_go.msg.MotorState[12]; // Unitree go2 motor state message
    float PosStopF = 2.146E+9f;
    float VelStopF = 16000.0f;
    float[] qmid = new float[12]{0f,1.2f,-2f, 0f,1.2f,-2f, 0f,1.2f,-2f, 0f,1.2f,-2f};
    float[] qsit = new float[12]{0f,1.4f,-2.3f, 0f,1.4f,-2.3f, 0f,1.4f,-2.3f, 0f,1.4f,-2.3f};
    float[] qdes = new float[12];
    private bool initialized = false;
    private bool subscriptionInitialized = false;

    public float kh;
    public bool FF_enable;
    public bool NN_enable;
    public float stepheight;
    public float T;//单腿上下
    float[] uff = new float[12];
    float[] utotal = new float[12];
    float[] u = new float[12];
    float tp;

    void Start()
    {
        Time.fixedDeltaTime = 0.01f;
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
        kh=1f;
        FF_enable=false;
        NN_enable=false;
        stepheight=0.5f;
        T=0.3f;//单腿上下
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(imu.Rpy[1]);//pitch rad 
        //sensor.AddObservation(imu.Rpy[0]);//roll rad 
        sensor.AddObservation(imu.Gyroscope[1]);//pitch rad 
        sensor.AddObservation(-imu.Gyroscope[2]);//roll rad 
        sensor.AddObservation(-imu.Gyroscope[0]);//roll rad
        for (int i = 0; i < 12; i++)
        {
            sensor.AddObservation(motor[i].Q);
            sensor.AddObservation(motor[i].Dq);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
        var kk = 0.9f;
        float tp1=tp/T;
        float uf1 = stepheight*(-Mathf.Cos(2*3.14f*tp1)+1f)/2f;
        float uf2 = uf1;
        if(tp1%2>1)uf1=0;
        else uf2=0;
        
        uff = new float[12]{0,uf1,-2*uf1,  0,uf2,-2*uf2,  0,uf2,-2*uf2,  0,uf1,-2*uf1,};        
        float kb = 0.6f;
        float kn =0;
        float kf =0;
        if(NN_enable)kn=1;
        if(FF_enable)kf=1;
        for (int i = 0; i < 12; i++)
        {
            u[i] = u[i] * kk + (1 - kk) * continuousActions[i];
            utotal[i] = kn*kb * u[i] + kf*uff[i] + qsit[i]*0.5f;
        }

        
    }

    private void Publish()
    {
        while(true)
        {
            if (ros2Unity.Ok())
            {
                if (ros2Node2 == null)
                {
                    ros2Node2 = ros2Unity.CreateNode("ros2_unity_go2_send_node");
                    go2_pub = ros2Node2.CreateSensorPublisher<unitree_go.msg.LowCmd>("/lowcmd");
                }        
                
                if(tp<=1)
                {
                    for(int i=0;i<12;i++)qdes[i] = motor[i].Q;
                }
                if(tp>1 && tp<=2)
                {
                    float t = tp - 1;
                    for(int i=0;i<12;i++)qdes[i] = motor[i].Q + (qsit[i] - motor[i].Q) * t;
                }
                if(tp>=2)
                {
                    if(FF_enable)
                    {
                        for(int i=0;i<12;i++)qdes[i]=utotal[i];
                    }
                    else 
                    {
                        for(int i=0;i<12;i++)qdes[i]=qsit[i]*kh;
                    }
                }
                for(int i=0;i<12;i++)SetTargetRad(i, qdes[i]);
                if(tp<0.5f)for(int i=0;i<12;i++)SetDampingMode(i);
                GetCRC(ref cmd_msg);

                //var msgWithHeader = cmd_msg as MessageWithHeader;
                //ros2Node.clock.UpdateROSTimestamp(ref msgWithHeader);
                if(tp>0.5f && go2_pub!=null)go2_pub.Publish(cmd_msg);
                if (interval_ms > 0)
                {
                    Thread.Sleep(interval_ms);
                }
                               
            }
        }
    }
    private void Subscribe()
    {
        while(true)
        {
            if (ros2Unity.Ok())
            {
                if (ros2Node == null)
                {
                    ros2Node = ros2Unity.CreateNode("ros2_unity_go2_receive_node");
                }
                
                if (go2_sub == null)
                {
                    go2_sub = ros2Node.CreateSubscription<unitree_go.msg.LowState>(
                        "lowstate", 
                        msg => 
                        {
                            // 1. 读取 IMU 数据
                            imu = msg.Imu_state;
                            //Debug.Log($"IMU - AccelX: {imu.Accelerometer[0]}, GyroY: {imu.Gyroscope[1]}");

                            // 2. 读取 Motor 数据
                            for (int i = 0; i < 12; i++)
                            {
                                motor[i] = msg.Motor_state[i];
                                //Debug.Log($"Motor {i} - Position: {motor[i].Q}, Velocity: {motor[i].Dq}");
                            }
                        }
                    );
                }

                if (interval_ms > 0)
                {
                    Thread.Sleep(interval_ms);
                }
            }
            
        }
    }
    void FixedUpdate()
    {
        if (!initialized)
        {
            Thread publishThread = new Thread(() => Publish());
            publishThread.Start();
            initialized = true;
        }
        if (!subscriptionInitialized)
        {
            Thread subscribeThread = new Thread(() => Subscribe());
            subscribeThread.Start();
            subscriptionInitialized = true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////
        float incrementStep = 0.002f;
        // 按键 DownArrow 增大 kh（最大 1）
        if (Input.GetKey(KeyCode.DownArrow))
        {
            kh += incrementStep;
            kh = Mathf.Min(kh, 1f); // 限制最大值
        }

        // 按键 UpArrow 减小 kh（最小 0.66）
        if (Input.GetKey(KeyCode.UpArrow))
        {
            kh -= incrementStep;
            kh = Mathf.Max(kh, 0.5f); // 限制最小值
        }
        if (Input.GetKeyDown(KeyCode.Space))FF_enable = !FF_enable; // 翻转布尔值

        tp+=0.01f;
    }

    void SetTargetRad(int idx, float pos)//20+0.5,30+1standing
    {
        cmd_msg.Motor_cmd[idx].Q = pos;   // Taregt angular(rad)
        cmd_msg.Motor_cmd[idx].Kp = 50.0f; // Poinstion(rad) control kp gain20+0.5, 50+5.5+1
        cmd_msg.Motor_cmd[idx].Dq = 0.0f;  // Taregt angular velocity(rad/ss)
        cmd_msg.Motor_cmd[idx].Kd = 2.0f;  // Poinstion(rad) control kd gain
        cmd_msg.Motor_cmd[idx].Tau = 0.0f; // Feedforward toque 1N.m
    }
    void SetDampingMode(int idx)//20+0.5,30+1standing
    {
        cmd_msg.Motor_cmd[idx].Q = 0f;   // Taregt angular(rad)
        cmd_msg.Motor_cmd[idx].Kp = 0f; // Poinstion(rad) control kp gain20+0.5, 50+5.5+1
        cmd_msg.Motor_cmd[idx].Dq = 0f;  // Taregt angular velocity(rad/ss)
        cmd_msg.Motor_cmd[idx].Kd = 2.0f;  // Poinstion(rad) control kd gain
        cmd_msg.Motor_cmd[idx].Tau = 0f; // Feedforward toque 1N.m
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
