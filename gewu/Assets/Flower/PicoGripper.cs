using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// PicoGripper脚本：使用Pico手柄扳机键控制夹爪
/// </summary>
public class PicoGripper : MonoBehaviour
{
    [Header("Gripper配置")]
    [Tooltip("Gripper组件，用于控制夹爪")]
    public Gripper gripper;
    
    [Header("手柄选择")]
    [Tooltip("选择使用左手柄还是右手柄控制")]
    public HandSelection handSelection = HandSelection.LeftHand;
    
    // XR输入设备
    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    private InputDevice currentHandDevice;  // 当前使用的手柄设备
    
    // 扳机键上一帧的状态（用于检测按下瞬间）
    private bool lastTriggerPressed = false;
    
    // 当前夹爪状态（true = grasp, false = open）
    private bool isGrasping = false;
    
    void Start()
    {
        // 初始化XR设备
        InitializeXRDevices();
        
        // 更新当前使用的手柄设备
        UpdateCurrentHandDevice();
        
        // 初始化夹爪为打开状态
        if (gripper != null)
        {
            isGrasping = false;
        }
    }
    
    void Update()
    {
        // 更新XR设备（防止设备未连接）
        UpdateXRDevices();
        
        // 更新当前使用的手柄设备
        UpdateCurrentHandDevice();
        
        // 检测扳机键
        CheckTriggerButton();
    }
    
    /// <summary>
    /// 更新当前使用的手柄设备
    /// </summary>
    void UpdateCurrentHandDevice()
    {
        currentHandDevice = handSelection == HandSelection.LeftHand ? leftHandDevice : rightHandDevice;
    }
    
    /// <summary>
    /// 检测扳机键按下，切换夹爪状态
    /// </summary>
    void CheckTriggerButton()
    {
        if (!currentHandDevice.isValid)
            return;
        
        bool isTriggerPressed = false;
        currentHandDevice.TryGetFeatureValue(CommonUsages.triggerButton, out isTriggerPressed);
        
        // 检测按下瞬间（从false变为true）
        if (isTriggerPressed && !lastTriggerPressed)
        {
            // 切换夹爪状态
            isGrasping = !isGrasping;
            
            if (gripper != null)
            {
                if (isGrasping)
                {
                    // 执行grasp
                    gripper.SetGripperPosition(gripper.graspAngle, gripper.graspAngle);
                    Debug.Log("PicoGripper: 执行Grasp");
                }
                else
                {
                    // 执行open
                    gripper.SetGripperPosition(gripper.openAngle, gripper.openAngle);
                    Debug.Log("PicoGripper: 执行Open");
                }
            }
        }
        lastTriggerPressed = isTriggerPressed;
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
}

