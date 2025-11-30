using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnitreeHg;

public class G1LowStateViewer : MonoBehaviour
{
    // ROS Connector
    private ROSConnection ros;

    // 订阅的 Topic 名
    public string lowStateTopic = "/lf/lowstate";

    void Start()
    {
        // 获取 ROSConnection 实例
        ros = ROSConnection.GetOrCreateInstance();

        // 注册订阅 LowStateMsg
        ros.Subscribe<LowStateMsg>(lowStateTopic, LowStateCallback);
    }

    // 回调函数
    void LowStateCallback(LowStateMsg msg)
    {
        Debug.Log("=== LowStateMsg ===");
        Debug.Log($"Mode PR: {msg.mode_pr}, Mode Machine: {msg.mode_machine}, Tick: {msg.tick}");

        // 打印 IMU 数据
        if (msg.imu_state != null)
        {
            Debug.Log($"IMU RPY: {msg.imu_state.rpy[0]:F2}, {msg.imu_state.rpy[1]:F2}, {msg.imu_state.rpy[2]:F2}");
            Debug.Log($"IMU Accel: {msg.imu_state.accelerometer[0]:F2}, {msg.imu_state.accelerometer[1]:F2}, {msg.imu_state.accelerometer[2]:F2}");
            Debug.Log($"IMU Gyro: {msg.imu_state.gyroscope[0]:F2}, {msg.imu_state.gyroscope[1]:F2}, {msg.imu_state.gyroscope[2]:F2}");
        }

        // 打印电机状态
        if (msg.motor_state != null)
        {
            for (int i = 0; i < msg.motor_state.Length; i++)
            {
                var motor = msg.motor_state[i];
                Debug.Log($"Motor {i}: q={motor.q:F2}, dq={motor.dq:F2}, tau_est={motor.tau_est:F2}");
            }
        }

        // 打印 CRC
        Debug.Log($"CRC: {msg.crc}");
    }
}
