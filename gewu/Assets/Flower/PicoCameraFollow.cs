using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// PicoCameraFollow脚本：让相机跟随VR头显的位置和旋转
/// </summary>
public class PicoCameraFollow : MonoBehaviour
{
    [Header("相机设置")]
    [Tooltip("要控制的相机（如果为空则使用Main Camera）")]
    public Camera targetCamera;
    
    [Tooltip("是否跟随头显位置")]
    public bool followPosition = true;
    
    [Tooltip("是否跟随头显旋转")]
    public bool followRotation = true;
    
    [Header("位置偏移")]
    [Tooltip("相机位置相对于头显的偏移")]
    public Vector3 positionOffset = Vector3.zero;
    
    [Header("调试")]
    [Tooltip("是否输出调试信息")]
    public bool debugLog = false;
    
    // XR输入设备（头显）
    private InputDevice headDevice;
    
    void Start()
    {
        // 如果没有指定相机，使用Main Camera
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                Debug.LogError("PicoCameraFollow: 未找到Main Camera，请在Inspector中指定targetCamera");
                enabled = false;
                return;
            }
        }
        
        // 初始化头显设备
        InitializeHeadDevice();
    }
    
    void Update()
    {
        // 更新头显设备（防止设备未连接）
        if (!headDevice.isValid)
        {
            InitializeHeadDevice();
        }
        
        // 更新相机位置和旋转
        UpdateCamera();
    }
    
    /// <summary>
    /// 初始化头显设备
    /// </summary>
    void InitializeHeadDevice()
    {
        headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        
        if (headDevice.isValid)
        {
            if (debugLog)
            {
                Debug.Log($"PicoCameraFollow: 找到头显设备 - {headDevice.name}");
            }
        }
        else
        {
            if (debugLog)
            {
                Debug.LogWarning("PicoCameraFollow: 未找到头显设备");
            }
        }
    }
    
    /// <summary>
    /// 更新相机位置和旋转
    /// </summary>
    void UpdateCamera()
    {
        if (targetCamera == null || !headDevice.isValid)
            return;
        
        // 获取头显位置
        if (followPosition)
        {
            Vector3 headPosition;
            if (headDevice.TryGetFeatureValue(CommonUsages.devicePosition, out headPosition))
            {
                // 应用位置偏移
                targetCamera.transform.position = headPosition + positionOffset;
                
                if (debugLog && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"PicoCameraFollow: 头显位置 = {headPosition}");
                }
            }
        }
        
        // 获取头显旋转
        if (followRotation)
        {
            Quaternion headRotation;
            if (headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out headRotation))
            {
                targetCamera.transform.rotation = headRotation;
                
                if (debugLog && Time.frameCount % 60 == 0)
                {
                    Debug.Log($"PicoCameraFollow: 头显旋转 = {headRotation.eulerAngles}");
                }
            }
        }
    }
    
    /// <summary>
    /// 在编辑器中刷新头显设备（用于Inspector）
    /// </summary>
    [ContextMenu("刷新头显设备")]
    void RefreshHeadDevice()
    {
        InitializeHeadDevice();
        if (headDevice.isValid)
        {
            Debug.Log($"PicoCameraFollow: 已刷新，找到头显设备 - {headDevice.name}");
        }
        else
        {
            Debug.LogWarning("PicoCameraFollow: 未找到头显设备");
        }
    }
}

