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

using System;
using UnityEngine;
using System.IO;
using System.Text;

namespace ROS2
{

/// <summary>
/// An example class provided for testing of basic ROS2 communication
/// </summary>
public class Go2Rece : MonoBehaviour
{
    private ROS2UnityComponent ros2Unity;
    private ROS2Node ros2Node;
    private ISubscription<unitree_go.msg.LowState> chatter_sub;
    unitree_go.msg.IMUState imu = new unitree_go.msg.IMUState();         // Unitree go2 IMU message
    unitree_go.msg.MotorState[] motor = new unitree_go.msg.MotorState[12]; // Unitree go2 motor state message

    string fileName = "IMU_Gyro_Data.csv";
    private StringBuilder csvData = new StringBuilder();

    void Start()
    {
        ros2Unity = GetComponent<ROS2UnityComponent>();
        // 写入 CSV 头部
        csvData.AppendLine("Time,GyroX,GyroY,GyroZ");
    }

    void FixedUpdate()
    {
        if (ros2Node == null && ros2Unity.Ok())
        {
            ros2Node = ros2Unity.CreateNode("ros2_unity_go2_receive_node");
            chatter_sub = ros2Node.CreateSubscription<unitree_go.msg.LowState>(
                        "lowstate", 
                        msg => 
                        {
                            // 1. 读取 IMU 数据
                            imu = msg.Imu_state;
                            Debug.Log($"IMU - Roll: {imu.Rpy[0]}, Pitch: {imu.Rpy[1]}");
                            /*Debug.Log(string.Format(
                                "GyRo0: {0:F2}, GyRo1: {1:F2}, GyRo2: {2:F2}",
                                imu.Gyroscope[0], imu.Gyroscope[1], imu.Gyroscope[2]
                                    ));*/
                            // 2. 读取 Motor 数据
                            for (int i = 0; i < 12; i++)
                            {
                                motor[i] = msg.Motor_state[i];
                                //Debug.Log($"Motor {i} - Position: {motor[i].Q}, Velocity: {motor[i].Dq}");
                            }
                        }
                    );

            
        }
        // 获取当前时间戳（可选）
        float currentTime = Time.time;

        // 追加数据行
        csvData.AppendLine($"{currentTime},{imu.Gyroscope[0]},{imu.Gyroscope[1]},{imu.Gyroscope[2]}");

        // 写入文件（可选：定期保存或结束时保存）
        File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), csvData.ToString());
        //print(csvData.Length);
        if (Time.time%1 > 0.9) 
        {
            SaveCsvToFile();
            //csvData.Clear(); // 清空缓存（根据需求决定是否保留）
        }
    }
    void OnApplicationQuit()
    {
        // 确保退出时保存数据
        File.WriteAllText(Path.Combine(Application.persistentDataPath, fileName), csvData.ToString());
    }
    public void SaveCsvToFile()
    {
        if (csvData.Length == 0) return;

        // 确保路径正确
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        try
        {
            // 使用 AppendAllText 避免覆盖（或一次性写入）
            File.WriteAllText(filePath, csvData.ToString());
            Debug.Log($"CSV 已保存至: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存 CSV 失败: {e.Message}");
        }
    }
}

}  // namespace ROS2
