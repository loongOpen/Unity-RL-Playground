using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;
using Newtonsoft.Json;

public class MotionCaptureReceiver : MonoBehaviour
{
    [Header("基础配置")]
    public int port = 8889;
    public Transform targetObject;

    [Header("轴映射配置（关键：根据实际传感器调整）")]
    [Tooltip("传感器X轴对应Unity的哪个轴（0=X, 1=Y, 2=Z）")]
    public int mapSensorXToUnity = 0; // 默认X→X
    [Tooltip("传感器Y轴对应Unity的哪个轴（0=X, 1=Y, 2=Z）")]
    public int mapSensorYToUnity = 1; // 默认Y→Y
    [Tooltip("传感器Z轴对应Unity的哪个轴（0=X, 1=Y, 2=Z）")]
    public int mapSensorZToUnity = 2; // 默认Z→Z

    [Header("轴方向矫正（传感器可能与Unity方向相反）")]
    public bool invertX = false; // 是否翻转X轴方向
    public bool invertY = false; // 是否翻转Y轴方向
    public bool invertZ = false; // 是否翻转Z轴方向

    [Header("调试选项")]
    public bool showDebugLog = true;
    public float rotationSmooth = 0.1f;

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isRunning = false;
    private Vector3 targetEulerAngles;

    [Serializable]
    public class UnityMotionData
    {
        public string timestamp;
        public int packet_id;
        public EulerAngleData euler_angles;
        public int battery_level;
        public int raw_data_length;
    }

    [Serializable]
    public class EulerAngleData
    {
        public float x;
        public float y;
        public float z;
    }

    void Start()
    {
        if (targetObject == null)
        {
            Debug.LogError("请指定targetObject（要控制的物体）！");
            enabled = false;
            return;
        }

        // 限制轴映射范围（0-2对应X-Y-Z）
        mapSensorXToUnity = Mathf.Clamp(mapSensorXToUnity, 0, 2);
        mapSensorYToUnity = Mathf.Clamp(mapSensorYToUnity, 0, 2);
        mapSensorZToUnity = Mathf.Clamp(mapSensorZToUnity, 0, 2);

        try
        {
            udpClient = new UdpClient(port);
            udpClient.Client.ReceiveTimeout = 1000;
            isRunning = true;
            receiveThread = new Thread(ReceiveUdpData);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            if (showDebugLog)
                Debug.Log($"UDP接收启动，端口: {port}");

            targetEulerAngles = targetObject.eulerAngles;
        }
        catch (Exception e)
        {
            Debug.LogError($"初始化失败: {e.Message}");
            enabled = false;
        }
    }

    void ReceiveUdpData()
    {
        while (isRunning)
        {
            try
            {
                IPEndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
                byte[] receiveBytes = udpClient.Receive(ref remoteIp);
                string jsonData = Encoding.UTF8.GetString(receiveBytes);

                UnityMotionData motionData = JsonConvert.DeserializeObject<UnityMotionData>(jsonData);
                if (motionData != null && motionData.euler_angles != null)
                {
                    // 1. 获取传感器原始角度（可能需要矫正方向）
                    float sensorX = invertX ? -motionData.euler_angles.x : motionData.euler_angles.x;
                    float sensorY = invertY ? -motionData.euler_angles.y : motionData.euler_angles.y;
                    float sensorZ = invertZ ? -motionData.euler_angles.z : motionData.euler_angles.z;

                    // 2. 根据配置映射到Unity的XYZ轴
                    Vector3 unityAngles = new Vector3(
                        GetMappedAngle(sensorX, mapSensorXToUnity),
                        GetMappedAngle(sensorY, mapSensorYToUnity),
                        GetMappedAngle(sensorZ, mapSensorZToUnity)
                    );

                    targetEulerAngles = unityAngles;

                    if (showDebugLog && motionData.packet_id % 50 == 0)
                    {
                        Debug.Log($"收到数据 | 包号: {motionData.packet_id} " +
                                  $"| 传感器角度: X={sensorX:F2}, Y={sensorY:F2}, Z={sensorZ:F2} " +
                                  $"| Unity角度: X={unityAngles.x:F2}, Y={unityAngles.y:F2}, Z={unityAngles.z:F2}");
                    }
                }
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.TimedOut && showDebugLog)
                    Debug.LogError($"UDP错误: {e.Message}");
            }
            catch (Exception e)
            {
                if (showDebugLog)
                    Debug.LogError($"解析错误: {e.Message}");
            }
        }
    }

    // 根据映射配置获取对应轴的角度
    private float GetMappedAngle(float sensorAngle, int targetAxis)
    {
        // targetAxis: 0=X, 1=Y, 2=Z（返回传感器角度到目标轴）
        return sensorAngle;
    }

    void Update()
    {
        //if (rotationSmooth > 0)
          //  targetObject.eulerAngles = Vector3.Lerp(targetObject.eulerAngles, targetEulerAngles, rotationSmooth * Time.deltaTime * 60);
        //else
            targetObject.eulerAngles = targetEulerAngles;
    }

    void OnDestroy()
    {
        isRunning = false;
        if (receiveThread != null && receiveThread.IsAlive)
            receiveThread.Join(1000);
        udpClient?.Close();

        if (showDebugLog)
            Debug.Log("UDP接收已关闭");
    }
}
