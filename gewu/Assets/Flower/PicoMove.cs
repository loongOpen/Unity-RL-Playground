using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// PicoMove脚本：使用Pico手柄摇杆控制ArticulationBody前后移动和左右转弯
/// </summary>
public class PicoMove : MonoBehaviour
{
    [Header("手柄选择")]
    [Tooltip("选择使用左手柄还是右手柄控制")]
    public HandSelection handSelection = HandSelection.RightHand;
    
    [Header("移动参数")]
    [Tooltip("速度控制时的最大速度（米/秒）")]
    public float maxVelocity = 1.0f;
    
    [Tooltip("速度控制时的最大角速度（度/秒）")]
    public float maxAngularVelocity = 90f;
    
    [Tooltip("速度控制时的力系数（用于达到目标速度）")]
    public float velocityForceCoefficient = 500f;
    
    [Tooltip("角速度控制时的力矩系数（用于达到目标角速度）")]
    public float angularVelocityTorqueCoefficient = 200f;
    
    [Tooltip("力施加位置的Y轴向下偏移（米），用于防止机器人翻倒")]
    public float forcePositionYOffset = 0.2f;
    
    [Tooltip("速度平滑系数（0-1），值越大变化越快，值越小变化越慢）")]
    [Range(0.01f, 1f)]
    public float velocitySmoothing = 0.1f;
    
    // XR输入设备
    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    private InputDevice currentHandDevice;
    
    // 当前平滑后的速度
    private Vector3 currentSmoothedVelocity = Vector3.zero;
    private float currentSmoothedAngularVelocity = 0f;  // 当前使用的手柄设备
    
    // 找到的ArticulationBody
    private ArticulationBody articulationBody;
    
    // 相机切换相关
    [Header("相机切换")]
    [Tooltip("要切换的相机列表（最多2个）")]
    public Camera[] cameras = new Camera[2];
    
    private bool lastBButtonState = false;  // 上一帧B键状态
    private bool lastSpaceKeyState = false;  // 上一帧空格键状态
    
    void Start()
    {
        // 初始化XR设备
        InitializeXRDevices();
        
        // 更新当前使用的手柄设备
        UpdateCurrentHandDevice();
        
        // 查找第一个ArticulationBody
        FindArticulationBody();
        
        // 初始化相机
        InitializeCameras();
    }
    
    /// <summary>
    /// 初始化相机
    /// </summary>
    void InitializeCameras()
    {
        // 如果没有手动指定相机，自动查找场景中的相机
        if (cameras[0] == null || cameras[1] == null)
        {
            Camera[] allCameras = FindObjectsOfType<Camera>();
            if (allCameras.Length >= 2)
            {
                if (cameras[0] == null) cameras[0] = allCameras[0];
                if (cameras[1] == null) cameras[1] = allCameras[1];
                Debug.Log($"PicoMove: 自动找到 {allCameras.Length} 个相机，使用前2个");
            }
            else if (allCameras.Length == 1)
            {
                if (cameras[0] == null) cameras[0] = allCameras[0];
                Debug.LogWarning($"PicoMove: 只找到 {allCameras.Length} 个相机，需要至少2个相机才能切换");
            }
        }
        
        // 设置相机的targetDisplay（不禁用相机）
        // Camera 0 -> Display 0 (Display 1)
        // Camera 1 -> Display 1 (Display 2)
        if (cameras[0] != null)
        {
            cameras[0].targetDisplay = 0;  // Display 1
            Debug.Log($"PicoMove: Camera 0 ({cameras[0].name}) 设置为 Display 1 (索引 0)");
        }
        if (cameras[1] != null)
        {
            cameras[1].targetDisplay = 1;  // Display 2
            Debug.Log($"PicoMove: Camera 1 ({cameras[1].name}) 设置为 Display 2 (索引 1)");
        }
    }
    
    void Update()
    {
        // 更新XR设备（防止设备未连接）
        UpdateXRDevices();
        
        // 更新当前使用的手柄设备
        UpdateCurrentHandDevice();
        
        // 检测右手柄B键切换显示
        CheckDisplaySwitch();
    }
    
    void FixedUpdate()
    {
        // 在FixedUpdate中处理物理移动
        HandleMovement();
    }
    
    /// <summary>
    /// 查找挂载物体及子节点第一个ArticulationBody
    /// </summary>
    void FindArticulationBody()
    {
        // 先检查挂载物体本身
        articulationBody = GetComponent<ArticulationBody>();
        
        // 如果挂载物体没有，则在子节点中查找
        if (articulationBody == null)
        {
            articulationBody = GetComponentInChildren<ArticulationBody>();
        }
        
        if (articulationBody != null)
        {
            // 确保ArticulationBody不是immovable
            if (articulationBody.immovable)
            {
                Debug.LogWarning($"PicoMove: ArticulationBody {articulationBody.gameObject.name} 是immovable，设置为false");
                articulationBody.immovable = false;
            }
            Debug.Log($"PicoMove: 找到ArticulationBody - {articulationBody.gameObject.name}, immovable={articulationBody.immovable}, mass={articulationBody.mass}");
        }
        else
        {
            Debug.LogWarning("PicoMove: 未找到ArticulationBody组件");
        }
    }
    
    /// <summary>
    /// 更新当前使用的手柄设备
    /// </summary>
    void UpdateCurrentHandDevice()
    {
        currentHandDevice = handSelection == HandSelection.LeftHand ? leftHandDevice : rightHandDevice;
    }
    
    /// <summary>
    /// 处理移动
    /// </summary>
    void HandleMovement()
    {
        if (articulationBody == null)
            return;
        
        // 获取摇杆输入值（2D向量，范围通常是-1到1）
        Vector2 joystickValue = Vector2.zero;
        bool gotJoystick = false;
        
        if (currentHandDevice.isValid)
        {
            gotJoystick = currentHandDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out joystickValue);
        }
        
        // 获取键盘输入（WASD）
        float keyboardForward = 0f;
        float keyboardTurn = 0f;
        
        if (Input.GetKey(KeyCode.W))
            keyboardForward += 1f;
        if (Input.GetKey(KeyCode.S))
            keyboardForward -= 1f;
        if (Input.GetKey(KeyCode.A))
            keyboardTurn -= 1f;
        if (Input.GetKey(KeyCode.D))
            keyboardTurn += 1f;
        
        // 优先使用摇杆输入，如果没有则使用键盘输入
        float forwardInput = 0f;
        float turnInput = 0f;
        
        if (gotJoystick && (Mathf.Abs(joystickValue.x) > 0.1f || Mathf.Abs(joystickValue.y) > 0.1f))
        {
            // 使用摇杆输入
            forwardInput = joystickValue.y;  // 前后（Y轴）
            turnInput = joystickValue.x;     // 左右（X轴）
        }
        else if (Mathf.Abs(keyboardForward) > 0.01f || Mathf.Abs(keyboardTurn) > 0.01f)
        {
            // 使用键盘输入
            forwardInput = keyboardForward;
            turnInput = keyboardTurn;
        }
        else
        {
            // 没有输入，返回
            return;
        }
        
        // 确保ArticulationBody不是immovable
        if (articulationBody.immovable)
        {
            articulationBody.immovable = false;
        }
        
        // 获取当前旋转
        Quaternion currentRotation = articulationBody.transform.rotation;
        
        // 获取力施加位置（使用质心位置，并向下偏移以防止翻倒）
        Vector3 forcePosition = articulationBody.worldCenterOfMass;
        // 向下偏移，使力施加在更低的位置，防止机器人翻倒
        forcePosition.y -= forcePositionYOffset;
        
        // 使用速度控制
        Vector3 forwardDirection = currentRotation * Vector3.forward;
        Vector3 targetVelocity = forwardDirection * forwardInput * maxVelocity;
        
        // 平滑过渡到目标速度
        currentSmoothedVelocity = Vector3.Lerp(currentSmoothedVelocity, targetVelocity, velocitySmoothing);
        
        // 计算速度差，施加力来达到平滑后的目标速度
        Vector3 velocityError = currentSmoothedVelocity - articulationBody.velocity;
        Vector3 force = velocityError * velocityForceCoefficient;
        
        // 在质心下方位置施加力（更稳定，不会导致翻转）
        articulationBody.AddForceAtPosition(force, forcePosition);
        
        // 角速度控制
        float targetAngularVelocity = turnInput * maxAngularVelocity;
        
        // 平滑过渡到目标角速度
        currentSmoothedAngularVelocity = Mathf.Lerp(currentSmoothedAngularVelocity, targetAngularVelocity, velocitySmoothing);
        
        float angularVelocityError = currentSmoothedAngularVelocity - articulationBody.angularVelocity.y * Mathf.Rad2Deg;
        Vector3 torque = Vector3.up * angularVelocityError * angularVelocityTorqueCoefficient * 0.01f; // 转换为弧度
        articulationBody.AddTorque(torque);
        
        // 调试输出（每秒输出一次）
        if (Time.frameCount % 50 == 0)
        {
            Debug.Log($"PicoMove: 摇杆值=({forwardInput:F2}, {turnInput:F2}), 速度={articulationBody.velocity.magnitude:F2}m/s, 角速度={articulationBody.angularVelocity.y * Mathf.Rad2Deg:F1}°/s, immovable={articulationBody.immovable}");
        }
    }
    
    /// <summary>
    /// 初始化XR设备
    /// </summary>
    void InitializeXRDevices()
    {
        leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }
    
    /// <summary>
    /// 更新XR设备（处理设备重连）
    /// </summary>
    void UpdateXRDevices()
    {
        if (!leftHandDevice.isValid)
        {
            leftHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }
        
        if (!rightHandDevice.isValid)
        {
            rightHandDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }
    }
    
    /// <summary>
    /// 检测并处理显示切换（右手柄B键或空格键）
    /// </summary>
    void CheckDisplaySwitch()
    {
        bool shouldSwitch = false;
        
        // 检测右手柄B键
        if (rightHandDevice.isValid)
        {
            bool currentBButtonState = false;
            if (rightHandDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out currentBButtonState))
            {
                // 检测B键从按下到释放（按下瞬间）
                if (currentBButtonState && !lastBButtonState)
                {
                    shouldSwitch = true;
                }
                
                lastBButtonState = currentBButtonState;
            }
        }
        
        // 检测空格键
        bool currentSpaceKeyState = Input.GetKey(KeyCode.Space);
        if (currentSpaceKeyState && !lastSpaceKeyState)
        {
            shouldSwitch = true;
        }
        lastSpaceKeyState = currentSpaceKeyState;
        
        // 执行切换
        if (shouldSwitch)
        {
            SwitchDisplay();
        }
    }
    
    /// <summary>
    /// 在camera 1和2之间切换（通过交换targetDisplay）
    /// </summary>
    void SwitchDisplay()
    {
        // 检查是否有足够的相机
        if (cameras[0] == null || cameras[1] == null)
        {
            Debug.LogWarning($"PicoMove: 相机未设置完整，Camera 0: {cameras[0]?.name ?? "null"}, Camera 1: {cameras[1]?.name ?? "null"}");
            return;
        }
        
        // 交换两个相机的targetDisplay
        int tempDisplay = cameras[0].targetDisplay;
        cameras[0].targetDisplay = cameras[1].targetDisplay;
        cameras[1].targetDisplay = tempDisplay;
        
        Debug.Log($"PicoMove: 已交换相机显示 - Camera 0 ({cameras[0].name}) -> Display {cameras[0].targetDisplay + 1} (索引 {cameras[0].targetDisplay}), Camera 1 ({cameras[1].name}) -> Display {cameras[1].targetDisplay + 1} (索引 {cameras[1].targetDisplay})");
    }
    
    /// <summary>
    /// 在编辑器中刷新ArticulationBody（用于Inspector）
    /// </summary>
    [ContextMenu("刷新ArticulationBody")]
    void RefreshArticulationBody()
    {
        FindArticulationBody();
        if (articulationBody != null)
        {
            Debug.Log($"PicoMove: 已刷新，找到ArticulationBody - {articulationBody.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("PicoMove: 未找到ArticulationBody");
        }
    }
}

