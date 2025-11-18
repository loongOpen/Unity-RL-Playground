using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
// using UnityEditor;
using System;
using Mujoco;

#if UNITY_EDITOR
using UnityEditor; // ✅ 条件编译
#endif

public class GWZZMAgent : Agent
{
    // UI 相关变量
    private Canvas canvas;
    private TMPro.TextMeshProUGUI infoText; // 使用 TextMeshPro 更美观

    //UDP 设置
    const int NMotMain = 31;
    private UdpClient ctrlUdp;
    // private UdpClient naviUdp;  // 用于navi
    private UdpClient pidUdp;
    private IPEndPoint ctrlEP = new IPEndPoint(IPAddress.Any, 0); // 用于控制命令的发送方
    private IPEndPoint naviEP = new IPEndPoint(IPAddress.Any, 0); // 记录导航命令发送方
    private byte[] txBuffer = new byte[4 + 36 + 12 * NMotMain];  // 412 B
    private CtrlStruct[] ctrlBuf = new CtrlStruct[NMotMain]; // 4*4*31=496 B
    private GCHandle ctrlHandle; // 用于高效 pin ctrl
    private NaviCtrlStruct naviCmd;
    //Struct
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImuStruct  //4*3*3 = 36
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] rpy;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] gyr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] acc;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SensStruct  //4*3*31
    {
        public float j;  // Joint Position
        public float w;  // Joint Velocity
        public float t;  // Torque
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CtrlStruct //4*4*31 = 496
    {
        public short enable;
        public byte pad0, pad1;  // 2 字节手动补齐
        public float j;
        public float w;
        public float t;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NaviSensStruct  // 28字节
    {
        public int reach;           // 到达标志 4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] pos;         // 实际位置 3*4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] rpy;         // 实际朝向 3*4
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NaviCtrlStruct  // 28字节
    {
        public int mode;              // 模式
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] pos;           // 目标位置
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] rpy;           // 目标朝向
    }

    // --- 初始化机器人关节及其他变量 ---
    public bool train = false;
    private bool _isClone = false;
    private Transform body;
    private MjHingeJoint[] jh = new MjHingeJoint[31];
    private MjBaseJoint[] arts = new MjBaseJoint[40];
    private MjBody[] mjbody = new MjBody[40];
    private MjActuator[] actuator = new MjActuator[31];
    private MjSiteQuaternionSensor[] quatsensor = new MjSiteQuaternionSensor[1];
    private MjSiteVectorSensor[] sensor = new MjSiteVectorSensor[3];
    private Quaternion imuquat;
    private Vector3 imuvel;
    private Vector3 imugyr;
    private Vector3 imuacc;
    public bool assistFlag = true;    // 是否启用悬挂
    public float assistH = 0.5f;      // 悬挂目标高度
    private float[] kp = new float[] {
        200,120,80,60,40,20,10,   200,120,80,60,40,20,10,
        100,100,600,600,600,
        2000,1000,2000,2000,1000,1000,   2000,1000,2000,2000,1000,1000
    };//没用上，pd外部设置，因需play前设置才会生效
    private float[] kd = new float[] {
        20,12,8,6,4,2,1,   20,12,8,6,4,2,1,
        30,30,40,40,40,
        250,200,250,250,100,100,   250,200,250,250,100,100
    };
    void Start()
    {
        // 初始化 UDP 客户端
        ctrlUdp = new UdpClient(9966); //9966端口 ctrl通信
        ctrlUdp.Client.ReceiveTimeout = 1; // 接收超时时间，1 ms 非阻塞
        // naviUdp = new UdpClient(8050); //监听8050端口 navi接收 不设超时
        // naviUdp.Client.ReceiveTimeout = 1;
        ctrlHandle = GCHandle.Alloc(ctrlBuf, GCHandleType.Pinned); // 固定控制缓冲区
        pidUdp = new UdpClient(); //python通信
        //设置layer，是否train
        Time.fixedDeltaTime = 0.001f;
        #if UNITY_EDITOR
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");
        SerializedProperty layer = layers.GetArrayElementAtIndex(15);
        int targetLayer = LayerMask.NameToLayer("robot");
        layer.stringValue = "robot";
        tagManager.ApplyModifiedProperties();
        Physics.IgnoreLayerCollision(15, 15, true);
        ChangeLayerRecursively(gameObject, 15);
        #endif
        if (train && !_isClone)
        {
            for (int i = 1; i < 42; i++)
            {
                GameObject clone = Instantiate(gameObject);
                clone.transform.position = transform.position + new Vector3(i * 2f, 0, 0);
                clone.name = $"{name}_Clone_{i}";
                clone.GetComponent<GWZZMAgent>()._isClone = true;
            }
        }
    }
    void ChangeLayerRecursively(GameObject obj, int targetLayer)
    {
        obj.layer = targetLayer;
        foreach (Transform child in obj.transform) ChangeLayerRecursively(child.gameObject, targetLayer);
    }
    public override void Initialize() //获取所有hinge关节，记录初始位姿
    {
        jh = this.GetComponentsInChildren<MjHingeJoint>(true);
        MjBaseJoint[] arts = this.GetComponentsInChildren<MjBaseJoint>(true);
        mjbody = this.GetComponentsInChildren<MjBody>();

        actuator = this.GetComponentsInChildren<MjActuator>(true);//获取执行器
        quatsensor = this.GetComponentsInChildren<MjSiteQuaternionSensor>(true);//获取四元数传感器
        sensor = this.GetComponentsInChildren<MjSiteVectorSensor>(true);//获取向量传感器

        // 打印获取的Hinge关节、执行器、四元数传感器、向量传感器
        for (int i = 0; i < jh.Length; i++)
        {
            print($"Hinge Joint {i}: {jh[i].gameObject.name}");
        }
        for (int i = 0; i < actuator.Length; i++)
        {
            print($"Actuator {i}: {actuator[i].gameObject.name}");
        }
        for (int i = 0; i < quatsensor.Length; i++)
        {
            print($"Quat Sensor {i}: {quatsensor[i].gameObject.name}");
        }
        for (int i = 0; i < sensor.Length; i++)
        {
            print($"Vector Sensor {i}: {sensor[i].gameObject.name}");
        }

        body = mjbody[0].GetComponent<Transform>();
        print(body);//确保body是base_link
        CreateUI();
    }
    public override void OnEpisodeBegin()//回合开始重置
    {
    }
    public override void CollectObservations(VectorSensor sensor)//观测
    {
        sensor.AddObservation(EulerTrans(body.eulerAngles[0]) * 3.14f / 180f);//rad
        sensor.AddObservation(EulerTrans(body.eulerAngles[2]) * 3.14f / 180f);//rad
        // sensor.AddObservation(body.InverseTransformDirection(sensor[1].SensorReading[0]));
        for (int i = 0; i < 31; i++)
        {
            sensor.AddObservation(jh[i].Configuration);
            sensor.AddObservation(jh[i].Velocity);
        }
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;
    }
    void CreateUI()
    {
        // 创建Canvas
        GameObject canvasGO = new GameObject("RobotInfoCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        // 创建文本对象
        GameObject textGO = new GameObject("InfoText");
        textGO.transform.SetParent(canvasGO.transform);
        // 设置TextMeshPro组件
        infoText = textGO.AddComponent<TMPro.TextMeshProUGUI>();
        infoText.alignment = TMPro.TextAlignmentOptions.BottomLeft;
        infoText.fontSize = 16;
        infoText.color = Color.black;
        // infoText.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);

        // 设置在左下角
        RectTransform rectTransform = infoText.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.01f, 0.01f);
        rectTransform.anchorMax = new Vector2(0.01f, 0.01f);
        rectTransform.pivot = new Vector2(0, 0);
        rectTransform.anchoredPosition = new Vector2(10, 10); // 距左下角10像素偏移
        rectTransform.sizeDelta = new Vector2(600, 100);
        // 初始文本
        infoText.text = "Keyboard: {F} for assist, {H} height +, {G} height -\n" +
                          "Assist: Enabled\n" +
                          "Assist H: 0.50m\n" +
                          "Velocity: (0.00, 0.00, 0.00)\n" +
                          "Gyroscope: (0.00, 0.00, 0.00)";
    }
    void FixedUpdate()
    {
        UpdateUI(); // 0 更新UI
        RecvCtrl(); // 1 收控制
        SendSens(); // 2 发传感
        //RecvNavi(); // 3 收导航
        //SendNavi(); // 4 发导航
        SendPos(); // 发实际位置
        // 5 悬挂 升降
        if (Input.GetKeyDown(KeyCode.F))
        {
            assistFlag = !assistFlag;
            //print($"Assist system: {(assistFlag ? "ENABLED" : "DISABLED")}");
            if (!assistFlag) ClearAppliedForces();
        }
        if (Input.GetKey(KeyCode.H)) assistH += 0.001f;
        if (Input.GetKey(KeyCode.G)) assistH -= 0.001f;
        assistH = Mathf.Clamp(assistH, 0.0f, 0.8f);
        if (assistFlag) ApplyAssistPhysics();
    }
    void UpdateUI()
    {
        Vector3 vel = new Vector3(
            sensor[0].SensorReading[0], // X
            sensor[0].SensorReading[1], // Y 
            sensor[0].SensorReading[2]  // Z  
        );
        Vector3 wel = new Vector3(
            sensor[1].SensorReading[0], // X
            sensor[1].SensorReading[1], // Y (注意索引)
            sensor[1].SensorReading[2]  // Z
        );
        // 更新文本 - 只显示请求的信息
        infoText.text = "Robot: {F} for assist, {H} height +, {G} height -\n" +
                        "Player: Click on the ground to move, {Z} turn left, {C} turn right\n" +
                          $"Assist: {(assistFlag ? "Enabled" : "Disabled")}\n" +
                          $"Assist H: {assistH:F2}m\n" +
                          $"Velocity: ({vel.x:F2}, {vel.y:F2}, {vel.z:F2})\n" +
                          $"Gyroscope: ({wel.x:F2}, {wel.y:F2}, {wel.z:F2})";
    }
    private void ApplyAssistPhysics()
    {
        // 位置稳定
        Vector3 vel = new Vector3(
            sensor[0].SensorReading[0], // X
            sensor[0].SensorReading[1], // Y (注意索引)
            sensor[0].SensorReading[2]  // Z  //身体坐标系的相对速度
        );
        Vector3 positionForce = Vector3.zero;
        positionForce.x = Mathf.Clamp(1500 * (-body.position.x) - 400 * vel.x, -600f, 600f);
        positionForce.z = Mathf.Clamp(1500 * (-body.position.z) - 400 * vel.z, -600f, 600f);
        positionForce.y = Mathf.Clamp(1500 * (assistH - 0.04f - body.position.y) - 400 * vel.y, -600f, 600f) + 800f; // 附加恒定升力
        //姿态稳定
        Vector3 wel = new Vector3(
            sensor[1].SensorReading[0], // X
            sensor[1].SensorReading[1], // Y (注意索引)
            sensor[1].SensorReading[2]  // Z
        );
        Vector3 rpy = new Vector3(
        EulerTrans(body.eulerAngles[0]) * Mathf.Deg2Rad,//roll
        EulerTrans(body.eulerAngles[1]) * Mathf.Deg2Rad,//yaw
        EulerTrans(body.eulerAngles[2]) * Mathf.Deg2Rad);//pitch
        //print(rpy);
        Vector3 angularForce = Vector3.zero;
        angularForce.x = Mathf.Clamp(1500 * (0 + rpy[0]) - 400 * wel.x, -600f, 600f);
        angularForce.z = Mathf.Clamp(1500 * (0 + rpy[2]) - 400 * wel.z, -600f, 600f);
        angularForce.y = Mathf.Clamp(1500 * (0 + rpy[1]) - 400 * wel.y, -600f, 600f);
        //旋转轴方向左右手坐标系。rpy前面符号不一样！！！！
        // // 打印传感器0数据
        // print("===== 传感器[0] vel数据 =====");
        // print($"读数[0]: {sensor[0].SensorReading[0]:F4} (Unity-x)");
        // print($"读数[1]: {sensor[0].SensorReading[1]:F4} (Unity-y)");
        // print($"读数[2]: {sensor[0].SensorReading[2]:F4} (Unity-z)");
        // // 打印传感器1数据
        // print("===== 传感器[1] gyr数据 =====");
        // print($"读数[0]: {sensor[1].SensorReading[0]:F4} (unity-roll-X-body坐标系)");
        // print($"读数[1]: {sensor[1].SensorReading[1]:F4} (unity-yaw-Y-body坐标系)");
        // print($"读数[2]: {sensor[1].SensorReading[2]:F4} (unity-pitch-Z-body坐标系)");
        // 应用力
        ApplyForceToBody(positionForce, angularForce);
    }
    private unsafe void ApplyForceToBody(Vector3 positionForce, Vector3 angularForce)
    {
        // 确保当前有MjScene实例并且body已绑定
        if (!MjScene.InstanceExists || mjbody[0].MujocoId < 0) return;
        var data = MjScene.Instance.Data;
        int index = mjbody[0].MujocoId; // 刚体的id
        // 将力从Unity坐标系转换到MuJoCo坐标系
        //unity的x相当于mujoco的y
        Vector3 mujocoForce = new Vector3(positionForce.x, positionForce.z, positionForce.y);
        Vector3 mujocoTorque = new Vector3(angularForce.x, angularForce.z, angularForce.y);

        // 在xfrc_applied数组中设置力和力矩
        data->xfrc_applied[index * 6] = mujocoForce[0];
        data->xfrc_applied[index * 6 + 1] = mujocoForce[1];
        data->xfrc_applied[index * 6 + 2] = mujocoForce[2];
        data->xfrc_applied[index * 6 + 3] = mujocoTorque[0];//r
        data->xfrc_applied[index * 6 + 4] = mujocoTorque[1];//p
        data->xfrc_applied[index * 6 + 5] = mujocoTorque[2];//y
    }
    private unsafe void ClearAppliedForces()
    {
        if (!MjScene.InstanceExists || mjbody[0].MujocoId < 0) return;
        var data = MjScene.Instance.Data;
        int index = mjbody[0].MujocoId;
        for (int i = 0; i < 6; i++) data->xfrc_applied[index * 6 + i] = 0;
    }
    void SendSens()
    {
        //获取身体IMU数据
        ImuStruct imu = new ImuStruct
        {
            rpy = new float[3],
            gyr = new float[3],
            acc = new float[3]
        };
        //imu.rpy[1] = EulerTrans(body.eulerAngles[0]) * Mathf.Deg2Rad; //roll
        imu.rpy[0] = -EulerTrans(quatsensor[0].SensorReading.eulerAngles[0]) * Mathf.Deg2Rad; //roll
        imu.rpy[1] = -EulerTrans(quatsensor[0].SensorReading.eulerAngles[2]) * Mathf.Deg2Rad; // pitch
        imu.rpy[2] = -EulerTrans(quatsensor[0].SensorReading.eulerAngles[1]) * Mathf.Deg2Rad;  // yaw
        imu.gyr[0] = sensor[1].SensorReading[0];
        imu.gyr[1] = sensor[1].SensorReading[2];
        imu.gyr[2] = sensor[1].SensorReading[1];//注意坐标系顺序
        imu.acc[0] = sensor[2].SensorReading[0]; // 前进方向
        imu.acc[1] = sensor[2].SensorReading[2]; // 侧向方向
        imu.acc[2] = sensor[2].SensorReading[1]; // 垂直方向

        SensStruct[] sens = new SensStruct[NMotMain];
        for (int i = 0; i < NMotMain; i++)
        {
            sens[i] = new SensStruct
            {
                j = (float)jh[i].RawConfiguration,//关节角度,单位弧度(float)
                w = jh[i].Velocity,
                t = 0
            };
        }
        // txBuffer
        Buffer.BlockCopy(BitConverter.GetBytes(txBuffer.Length), 0, txBuffer, 0, 4);
        Buffer.BlockCopy(GetBytes(imu), 0, txBuffer, 4, 36);
        for (int i = 0; i < NMotMain; i++)
        {
            byte[] b = GetBytes(sens[i]);
            Buffer.BlockCopy(b, 0, txBuffer, 4 + 36 + i * 12, 12);
        }
        // 发送数据
        if (ctrlEP.Address != IPAddress.Any) // 确保已经记录了一个端点
        {
            ctrlUdp.Send(txBuffer, txBuffer.Length, ctrlEP);
        }
    }
    void RecvCtrl()
    {
        try
        {
            // long ns = _sw.ElapsedTicks * 1_000_000_000 / Stopwatch.Frequency;
            // print($"控制时间: {ns} ns");
            // if (_lastNs > 0)
            // {
            //     long deltaNs = ns - _lastNs;
            //     double actualHz = 1_000_000_000.0 / deltaNs;
            //     print($"Δt: {deltaNs} ns | 频率: {actualHz:F1} Hz");
            // }
            // _lastNs = ns;
            byte[] rx = ctrlUdp.Receive(ref ctrlEP);
            int expectedSize = Marshal.SizeOf(typeof(CtrlStruct)) * NMotMain;
            if (rx.Length != expectedSize)
            {
                print($"UDP len = {rx.Length}, expected data len = {expectedSize}");
            }
            // 将 byte[] 拷贝到已 pin 的 ctrlBuf
            Marshal.Copy(rx, 0, ctrlHandle.AddrOfPinnedObject(), rx.Length);
            // j 写入到关节
            for (int i = 0; i < NMotMain; i++)
            {
                if (ctrlBuf[i].enable == 0) continue;
                SetJointTargetDeg(actuator[i], ctrlBuf[i].j);//接收弧度
            }
        }
        catch (SocketException) { /* 超时忽略 */ }
    }
    void SendPos()
    {
        try
        {
            NaviSensStruct naviSens = new NaviSensStruct();
            naviSens.reach = 1;
            naviSens.pos = new float[3] { -body.position.z, body.position.x, body.position.y };
            naviSens.rpy = new float[3];
            naviSens.rpy[0] = -EulerTrans(quatsensor[0].SensorReading.eulerAngles[0]) * Mathf.Deg2Rad; // roll
            naviSens.rpy[1] = -EulerTrans(quatsensor[0].SensorReading.eulerAngles[2]) * Mathf.Deg2Rad; // pitch
            naviSens.rpy[2] = -EulerTrans(quatsensor[0].SensorReading.eulerAngles[1]) * Mathf.Deg2Rad; // yaw
            byte[] data = GetBytes(naviSens);
            // if (naviEP.Address != IPAddress.Any) // 确保已经记录了一个端点
            // {
            //     naviUdp.Send(data, data.Length, naviEP); //发给导航端
            // }
            // IPEndPoint pyEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8051);
            IPEndPoint pyEndpoint = new IPEndPoint(IPAddress.Parse("192.168.115.121"), 8051);
            pidUdp.Send(data, data.Length, pyEndpoint); //发给py端
        }
        catch (Exception ex) { print($"发送实际位置失败: {ex.Message}"); }
    }
    // 设置关节目标角度、PD参数
    void SetJointTargetDeg(MjActuator actuator, float targetRad)
    {
        // actuator.CustomParams.Kp = kp[i];//position actuator的kv参数
        // actuator.CustomParams.Kvp = kd[i];//同上，但参数是kpv而不是kv
        actuator.Control = targetRad;
    }
    // 欧拉角转换
    float EulerTrans(float eulerAngle)
    {
        if (eulerAngle <= 180)
            return eulerAngle;
        else
            return eulerAngle - 360f;
    }
    // 序列化结构体为 byte[]
    static byte[] GetBytes<T>(T str) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return arr;
    }
    // 反序列化 byte[] 为结构体
    static T FromBytes<T>(byte[] arr) where T : struct
    {
        int size = Marshal.SizeOf<T>();
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.Copy(arr, 0, ptr, size);
        T str = Marshal.PtrToStructure<T>(ptr)!;
        Marshal.FreeHGlobal(ptr);
        return str;
    }
    void OnDestroy()
    {
        ctrlUdp?.Close();
        pidUdp?.Close();
        // naviUdp?.Close();
        if (ctrlHandle.IsAllocated) ctrlHandle.Free();
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {

    }
    // void SendNavi()
    // {
    //     try
    //     {
    //         NaviSensStruct naviSens = new NaviSensStruct();
    //         Vector3 targetPos = Vector3.zero;
    //         if (naviCmdReceived) // 标志表示已接收到导航命令
    //         {
    //             targetPos = new Vector3(naviCmd.pos[0], naviCmd.pos[2], naviCmd.pos[1]);
    //         }
    //         // 计算是否到达目标
    //         float deltaX = Mathf.Abs(body.position.x - targetPos.x);
    //         float deltaY = Mathf.Abs(body.position.z - targetPos.z);
    //         if (deltaX <= 0.01f && deltaY <= 0.01f)
    //         {
    //             naviSens.reach = 1;
    //         }
    //         else
    //         {
    //             naviSens.reach = 0;
    //         }
    //         naviSens.pos = new float[3] { body.position.x, body.position.z, body.position.y };
    //         naviSens.rpy = new float[3];
    //         naviSens.rpy[0] = -EulerTrans(quatsensor[0].SensorReading.eulerAngles[0]) * Mathf.Deg2Rad; // roll
    //         naviSens.rpy[1] = -EulerTrans(quatsensor[0].SensorReading.eulerAngles[2]) * Mathf.Deg2Rad; // pitch
    //         naviSens.rpy[2] = -EulerTrans(quatsensor[0].SensorReading.eulerAngles[1]) * Mathf.Deg2Rad; // yaw
    //         byte[] data = GetBytes(naviSens);
    //         if (naviEP.Address != IPAddress.Any) // 确保已经记录了一个端点
    //         {
    //             naviUdp.Send(data, data.Length, naviEP); //发给导航端
    //         }
    //         //IPEndPoint pyEndpoint = new IPEndPoint(IPAddress.Parse("192.168.115.121"), 8051);
    //         //pidUdp.Send(data, data.Length, pyEndpoint); //发给py端
    //         // 发送的数据
    //         // print($"reach: {naviSens.reach}");
    //         // print($"位置: x={naviSens.pos[0]:F6}, y={naviSens.pos[1]:F6}, z={naviSens.pos[2]:F6}");
    //         // print($"姿态: roll={naviSens.rpy[0]:F6}, pitch={naviSens.rpy[1]:F6}, yaw={naviSens.rpy[2]:F6}");
            
    //     }
    //     catch (Exception ex) { print($"发送导航数据失败: {ex.Message}"); }
    // }
    // void RecvNavi()
    // {
    //     try
    //     {
    //         byte[] rx = naviUdp.Receive(ref naviEP);
    //         if (rx.Length != 28) return;
    //         GCHandle handle = GCHandle.Alloc(rx, GCHandleType.Pinned);
    //         NaviCtrlStruct naviCmd;
    //         try
    //         {
    //             naviCmd = (NaviCtrlStruct)Marshal.PtrToStructure(
    //                 handle.AddrOfPinnedObject(), typeof(NaviCtrlStruct));
    //             naviCmdReceived = true;
    //         }
    //         finally
    //         {
    //             handle.Free(); // 立即释放
    //         }
    //         print($"接受到导航命令 - 模式: {naviCmd.mode}, " +
    //             $"位置: ({naviCmd.pos[0]:F3}, {naviCmd.pos[1]:F3}, {naviCmd.pos[2]:F3}), " +
    //             $"姿态: ({naviCmd.rpy[0]:F3}, {naviCmd.rpy[1]:F3}, {naviCmd.rpy[2]:F3})");
    //     }
    //     catch (SocketException)
    //     {
    //         // 超时忽略，这是正常的
    //     }
    //     catch (Exception ex)
    //     {
    //         print($"接收导航数据异常: {ex.Message}");
    //     }
    // }
}
