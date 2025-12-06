using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// 手柄选择枚举
/// </summary>
public enum HandSelection
{
    LeftHand,   // 左手柄
    RightHand   // 右手柄
}

/// <summary>
/// VR脚本：获取Pico手柄位姿，控制Cube的移动和旋转
/// </summary>
public class PicoGewu : MonoBehaviour
{
    [Header("Cube对象")]
    public Transform cube;  // 要控制的Cube
    
    [Header("目标位置")]
    public Transform target;  // cube的目标位置
    
    [Header("IK控制")]
    public GewuIK gewuIK;  // GewuIK组件
    
    [Header("手柄选择")]
    [Tooltip("选择使用左手柄还是右手柄控制")]
    public HandSelection handSelection = HandSelection.LeftHand;
    
    [Header("控制参数")]
    [Tooltip("移动速度")]
    public float moveSpeed = 1.0f;
    
    [Tooltip("位置变化缩放系数（放大位置变化）")]
    public float positionScale = 2.0f;
    
    [Tooltip("旋转速度")]
    public float rotationSpeed = 90.0f;

    [Header("平滑参数")]
    [Tooltip("复位平滑速度（秒）")]
    public float resetSmoothTime = 0.5f;
    
    [Header("安全区域限制")]
    [Tooltip("圆心位置，cube不能超过以此为中心的安全半径球面")]
    public Transform centerPoint;
    
    [Tooltip("安全半径（米），cube的位置不能超过此半径")]
    public float safeRadius = 1.0f;

    // XR输入设备
    private InputDevice leftHandDevice;
    private InputDevice rightHandDevice;
    private InputDevice currentHandDevice;  // 当前使用的手柄设备

    // 上一帧的手柄位置和旋转（用于计算相对值）
    private Vector3 lastHandPosition;
    private Quaternion lastHandRotation;
    
    // 是否已经初始化了上一帧的值
    private bool isInitialized = false;
    
    // 记录的初始位姿
    private Vector3 initialPosition;  // cube的初始位置（激活时的位置）
    private Quaternion initialRotation;  // cube的初始旋转（激活时的旋转）
    private bool initialPoseSet = false;  // 标记初始位姿是否已设置
    
    // A键上一帧的状态（用于检测按下瞬间）
    private bool lastAPressed = false;
    
    // 复位协程状态
    private bool isResetting = false;

    void Start()
    {
        // 初始化XR设备
        InitializeXRDevices();
        
        // 更新当前使用的手柄设备
        UpdateCurrentHandDevice();
        
        // 初始时禁用cube
        if (cube != null)
        {
            cube.gameObject.SetActive(false);
        }
        
        // 2秒后将cube移动到target位置并记录初始位置，然后激活cube
        StartCoroutine(MoveToTargetAfterDelay());
    }
    
    /// <summary>
    /// 更新当前使用的手柄设备
    /// </summary>
    void UpdateCurrentHandDevice()
    {
        currentHandDevice = handSelection == HandSelection.LeftHand ? leftHandDevice : rightHandDevice;
    }
    
    /// <summary>
    /// 延迟2秒后启用IK
    /// </summary>
    private System.Collections.IEnumerator MoveToTargetAfterDelay()
    {
        // 等待2秒
        yield return new WaitForSeconds(2f);
        
        // 启用IK
        if (gewuIK != null)
        {
            gewuIK.SetIKEnabled(true);
            Debug.Log("GewuIK已启用");
        }
    }

    void Update()
    {
        // 更新XR设备（防止设备未连接）
        UpdateXRDevices();
        
        // 更新当前使用的手柄设备
        UpdateCurrentHandDevice();

        // 检测A键复位功能
        CheckResetButton();

        // 获取并应用手柄位姿（使用相对值），复位时不更新
        if (!isResetting)
        {
            UpdateCubeFromHand(cube, currentHandDevice);
        }
    }
    
    /// <summary>
    /// 检测A键按下，复位cube到初始位置并归零旋转（使用2秒时记录的初始位置）
    /// </summary>
    void CheckResetButton()
    {
        if (!currentHandDevice.isValid)
            return;
        
        bool isAPressed = false;
        currentHandDevice.TryGetFeatureValue(CommonUsages.primaryButton, out isAPressed);
        
        // 检测按下瞬间（从false变为true）
        if (isAPressed && !lastAPressed)
        {
            // 使用记录的初始位姿进行复位
            if (initialPoseSet)
            {
                ResetCube(cube, initialPosition, initialRotation);
                Debug.Log($"按A键复位到初始位姿 - 位置: {initialPosition}, 旋转: {initialRotation.eulerAngles}");
            }
            else
            {
                Debug.LogWarning("初始位姿未记录，无法复位");
            }
        }
        lastAPressed = isAPressed;
    }
    
    /// <summary>
    /// 复位cube到指定位置和旋转（平滑归位）
    /// </summary>
    void ResetCube(Transform cube, Vector3 resetPosition, Quaternion resetRotation)
    {
        if (cube == null)
            return;
        
        // 检查是否正在复位，避免重复启动协程
        if (isResetting)
            return;
        
        // 启动平滑复位协程
        StartCoroutine(SmoothResetCube(cube, resetPosition, resetRotation));
    }
    
    /// <summary>
    /// 平滑复位cube到指定位置和旋转
    /// </summary>
    private System.Collections.IEnumerator SmoothResetCube(Transform cube, Vector3 resetPosition, Quaternion resetRotation)
    {
        // 标记正在复位
        isResetting = true;
        
        // 记录起始位置和旋转
        Vector3 startPosition = cube.position;
        Quaternion startRotation = cube.rotation;
        
        float elapsedTime = 0f;
        
        // 平滑插值到目标位置和旋转
        while (elapsedTime < resetSmoothTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / resetSmoothTime;
            
            // 使用平滑曲线（easeInOut）
            t = t * t * (3f - 2f * t);
            
            // 插值位置
            cube.position = Vector3.Lerp(startPosition, resetPosition, t);
            
            // 插值旋转
            cube.rotation = Quaternion.Slerp(startRotation, resetRotation, t);
            
            yield return null;
        }
        
        // 确保最终位置和旋转准确
        cube.position = resetPosition;
        cube.rotation = resetRotation;
        
        // 重置上一帧的手柄位置和旋转，避免下次按下时突然跳跃
        if (currentHandDevice.isValid)
        {
            currentHandDevice.TryGetFeatureValue(CommonUsages.devicePosition, out lastHandPosition);
            currentHandDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out lastHandRotation);
        }
        
        // 标记复位完成
        isResetting = false;
        
        Debug.Log($"Cube平滑复位到初始位置: {resetPosition}, 旋转已归零");
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
    /// 根据手柄位姿更新Cube的位置和旋转（使用相对值）
    /// </summary>
    void UpdateCubeFromHand(Transform cube, InputDevice handDevice)
    {
        if (cube == null || !handDevice.isValid)
            return;
        
        // 如果cube刚激活且初始位姿未记录，记录初始位姿
        if (cube.gameObject.activeSelf && !initialPoseSet)
        {
            initialPosition = cube.position;
            initialRotation = cube.rotation;
            initialPoseSet = true;
            Debug.Log($"记录cube初始位姿 - 位置: {initialPosition}, 旋转: {initialRotation.eulerAngles}");
        }

        // 检测抓握键（用于控制位置移动和旋转）
        bool isGripPressed = false;
        handDevice.TryGetFeatureValue(CommonUsages.gripButton, out isGripPressed);

        // 获取当前手柄位置和旋转
        bool gotPosition = handDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 currentHandPosition);
        bool gotRotation = handDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion currentHandRotation);

        // 处理位置移动（使用相对位移）
        if (isGripPressed && gotPosition)
        {
            if (isInitialized)
            {
                // 计算相对位移
                Vector3 deltaPosition = currentHandPosition - lastHandPosition;
                
                // 交换前后和左右：X轴（左右）映射到Z轴（前后），Z轴（前后）映射到X轴（左右）
                Vector3 swappedDeltaPosition = new Vector3(
                    deltaPosition.z,  // 原本的Z轴（前后）映射到X轴（左右）
                    deltaPosition.y,  // Y轴（上下）保持不变
                    -deltaPosition.x   // 原本的X轴（左右）映射到Z轴（前后）
                );
                
                // 应用相对位移到Cube（乘以移动速度和位置缩放系数，放大位置变化）
                Vector3 newPosition = cube.position + swappedDeltaPosition * moveSpeed * positionScale;
                
                // 限制cube位置在安全半径球面内
                newPosition = ClampToSphere(newPosition);
                
                cube.position = newPosition;
            }
            
            // 更新上一帧位置
            lastHandPosition = currentHandPosition;
        }
        else if (!isGripPressed && gotPosition)
        {
            // 未按下抓握键时，更新上一帧位置但不移动Cube（避免下次按下时突然跳跃）
            lastHandPosition = currentHandPosition;
        }

        // 处理旋转（使用相对旋转）- 按下抓握键时同步手柄的相对旋转
        if (isGripPressed && gotRotation)
        {
            if (isInitialized)
            {
                // 计算相对旋转
                Quaternion deltaRotation = currentHandRotation * Quaternion.Inverse(lastHandRotation);
                
                // 将相对旋转转换为欧拉角，以便交换旋转轴
                Vector3 deltaEuler = deltaRotation.eulerAngles;
                
                // 交换旋转轴：X轴（俯仰）↔ Z轴（翻滚），Y轴（偏航）保持不变
                // 类似于位置移动的轴交换
                Vector3 swappedDeltaEuler = new Vector3(
                    deltaEuler.z,  // 原本的Z轴（翻滚）映射到X轴（俯仰）
                    deltaEuler.y,  // Y轴（偏航）保持不变
                    -deltaEuler.x   // 原本的X轴（俯仰）映射到Z轴（翻滚）
                );
                
                // 将交换后的欧拉角转换回四元数
                Quaternion swappedDeltaRotation = Quaternion.Euler(swappedDeltaEuler);
                
                // 应用相对旋转到Cube（世界坐标系）
                Quaternion newRotation = swappedDeltaRotation * cube.rotation;
                
                cube.rotation = newRotation;
            }
            
            // 更新上一帧旋转
            lastHandRotation = currentHandRotation;
        }
        else if (!isGripPressed && gotRotation)
        {
            // 未按下抓握键时，更新上一帧旋转但不旋转Cube（避免下次按下时突然跳跃）
            lastHandRotation = currentHandRotation;
        }

        // 标记已初始化
        if (gotPosition && gotRotation)
        {
            isInitialized = true;
        }
    }

    /// <summary>
    /// 将位置限制在安全半径球面内
    /// </summary>
    Vector3 ClampToSphere(Vector3 position)
    {
        if (centerPoint == null)
            return position;
        
        Vector3 center = centerPoint.position;
        Vector3 direction = position - center;
        float distance = direction.magnitude;
        
        // 如果距离超过安全半径，则限制在球面上
        if (distance > safeRadius)
        {
            direction = direction.normalized;
            return center + direction * safeRadius;
        }
        
        return position;
    }
    
    /// <summary>
    /// 在编辑器中绘制调试信息
    /// </summary>
    void OnDrawGizmos()
    {
        if (cube != null)
        {
            Gizmos.color = handSelection == HandSelection.LeftHand ? Color.blue : Color.red;
            Gizmos.DrawWireSphere(cube.position, 0.1f);
        }
        
        // 绘制安全半径球面
        if (centerPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPoint.position, safeRadius);
        }
    }
}

