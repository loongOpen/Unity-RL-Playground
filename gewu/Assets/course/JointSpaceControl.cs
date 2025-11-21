using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class JointAngleRow
{
    public float[] angles;
    
    // 无参构造函数，Unity 序列化需要
    public JointAngleRow()
    {
        angles = new float[0];
    }
    
    public JointAngleRow(int count)
    {
        angles = new float[count];
    }
}

public class JointSpaceControl : MonoBehaviour
{
    [Header("Robot Configuration")]
    public Transform robot;
    
    [Header("Slider Controls")]
    public List<Slider> sliders = new List<Slider>();
    
    [Header("Joint Settings")]
    public float stiffness = 200f;
    public float damping = 10f;
    
    [Header("Trajectory Settings")]
    public float trajectoryDuration = 3.0f; // 三次多项式轨迹持续时间
    
    [Header("Joint Angles")]
    [Tooltip("关节角度数组列表，每行保存所有关节角（度）")]
    public List<JointAngleRow> angles = new List<JointAngleRow>();
    
    private List<ArticulationBody> revoluteJoints = new List<ArticulationBody>();
    private bool continuousSetActive = false;
    private int continuousSetRowIndex = -1;
    private HashSet<int> sliderInteractionHooked = new HashSet<int>();
    private Coroutine trajectoryCoroutine;
    private bool isExecutingTrajectory = false;
    
    void Start()
    {
        if (robot != null)
        {
            FindRevoluteJoints();
            InitializeSliders();
        }
    }
    
    void InitializeSliders()
    {
        // 初始化每个 slider，使其初始位置对应关节的当前角度
        for (int i = 0; i < Mathf.Min(sliders.Count, revoluteJoints.Count); i++)
        {
            if (sliders[i] != null && revoluteJoints[i] != null)
            {
                var drive = revoluteJoints[i].xDrive;
                float lowerLimit = drive.lowerLimit;
                float upperLimit = drive.upperLimit;
                
                // 获取关节的当前角度（弧度转度）
                float currentAngleDeg = revoluteJoints[i].jointPosition[0] * Mathf.Rad2Deg;
                
                // 计算当前角度对应的 slider 值
                // slider = 0 对应 lowerLimit, slider = 1 对应 upperLimit
                // 当前角度对应的 slider 值 = (currentAngle - lowerLimit) / (upperLimit - lowerLimit)
                float sliderValue = 0.5f; // 默认中间值
                if (Mathf.Abs(upperLimit - lowerLimit) > 0.001f)
                {
                    sliderValue = (currentAngleDeg - lowerLimit) / (upperLimit - lowerLimit);
                    sliderValue = Mathf.Clamp01(sliderValue); // 确保在 [0, 1] 范围内
                }
                
                sliders[i].value = sliderValue;
                
                RegisterSliderUserInteraction(sliders[i]);
            }
        }
    }

    void RegisterSliderUserInteraction(Slider slider)
    {
        if (slider == null)
            return;
        
        int id = slider.GetInstanceID();
        if (sliderInteractionHooked.Contains(id))
            return;
        
        sliderInteractionHooked.Add(id);
        
        EventTrigger trigger = slider.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slider.gameObject.AddComponent<EventTrigger>();
        }
        
        if (trigger.triggers == null)
        {
            trigger.triggers = new List<EventTrigger.Entry>();
        }
        
        AddTriggerCallback(trigger, EventTriggerType.PointerDown);
        AddTriggerCallback(trigger, EventTriggerType.BeginDrag);
    }

    void AddTriggerCallback(EventTrigger trigger, EventTriggerType type)
    {
        EventTrigger.Entry entry = trigger.triggers.Find(e => e.eventID == type);
        if (entry == null)
        {
            entry = new EventTrigger.Entry { eventID = type, callback = new EventTrigger.TriggerEvent() };
            trigger.triggers.Add(entry);
        }
        entry.callback.AddListener((eventData) => HandleSliderUserInteraction());
    }

    void HandleSliderUserInteraction()
    {
        if (continuousSetActive)
        {
            StopContinuousSet();
            Debug.Log("JointSpaceControl: 检测到用户手动拖动 slider，已停止持续执行 Set。");
        }
        
        if (isExecutingTrajectory)
        {
            StopTrajectory();
            Debug.Log("JointSpaceControl: 检测到用户手动拖动 slider，已停止轨迹执行。");
        }
    }
    
    void FindRevoluteJoints()
    {
        revoluteJoints.Clear();
        if (robot == null) return;
        
        ArticulationBody[] allBodies = robot.GetComponentsInChildren<ArticulationBody>(true);
        foreach (ArticulationBody body in allBodies)
        {
            if (body.jointType == ArticulationJointType.RevoluteJoint)
            {
                revoluteJoints.Add(body);
            }
        }
    }
    
    void FixedUpdate()
    {
        // 如果正在执行轨迹，跳过正常的控制逻辑
        if (isExecutingTrajectory) return;
        
        // 持续执行指定行的角度
        if (continuousSetActive)
        {
            if (!ApplyRowAngles(continuousSetRowIndex, false))
            {
                // 如果失败（例如行被删除），停止持续执行
                StopContinuousSet();
            }
        }
        
        // 获取每个 slider 的值，缩放后赋值给关节
        for (int i = 0; i < Mathf.Min(sliders.Count, revoluteJoints.Count); i++)
        {
            if (sliders[i] != null && revoluteJoints[i] != null)
            {
                // 从关节的 xDrive 获取 lowerLimit 和 upperLimit
                var drive = revoluteJoints[i].xDrive;
                float lowerLimit = drive.lowerLimit;
                float upperLimit = drive.upperLimit;
                
                // 获取 slider 的值（通常在 0-1 范围），缩放到关节限制范围
                float sliderValue = sliders[i].value;
                float angleDeg = Mathf.Lerp(lowerLimit, upperLimit, sliderValue);
                
                // 设置关节目标角度
                SetJointTargetDeg(revoluteJoints[i], angleDeg);
            }
        }
    }
    
    void SetJointTargetDeg(ArticulationBody joint, float angleDeg)
    {
        var drive = joint.xDrive;
        drive.stiffness = stiffness;
        drive.damping = damping;
        drive.target = angleDeg;
        joint.xDrive = drive;
    }
    
    /// <summary>
    /// 设置指定行的关节角到关节，并开启持续执行（使用三次多项式轨迹）
    /// </summary>
    public void SetAnglesFromRow(int rowIndex)
    {
        if (ExecuteCubicTrajectory(rowIndex))
        {
            continuousSetActive = true;
            continuousSetRowIndex = rowIndex;
        }
    }
    
    /// <summary>
    /// 停止持续执行 SetAnglesFromRow
    /// </summary>
    public void StopContinuousSet()
    {
        continuousSetActive = false;
        continuousSetRowIndex = -1;
    }
    
    /// <summary>
    /// 执行三次多项式轨迹
    /// </summary>
    private bool ExecuteCubicTrajectory(int rowIndex)
    {
        if (isExecutingTrajectory)
        {
            StopTrajectory();
        }
        
        if (!ValidateRowIndex(rowIndex)) return false;
        
        trajectoryCoroutine = StartCoroutine(CubicTrajectoryCoroutine(rowIndex));
        return true;
    }
    
    /// <summary>
    /// 停止轨迹执行
    /// </summary>
    public void StopTrajectory()
    {
        if (trajectoryCoroutine != null)
        {
            StopCoroutine(trajectoryCoroutine);
            trajectoryCoroutine = null;
        }
        isExecutingTrajectory = false;
    }
    
    /// <summary>
    /// 三次多项式轨迹协程
    /// </summary>
    private IEnumerator CubicTrajectoryCoroutine(int targetRowIndex)
    {
        isExecutingTrajectory = true;
        
        // 获取起始角度（当前角度）
        float[] startAngles = new float[revoluteJoints.Count];
        for (int i = 0; i < revoluteJoints.Count; i++)
        {
            startAngles[i] = revoluteJoints[i].jointPosition[0] * Mathf.Rad2Deg;
        }
        
        // 获取目标角度
        float[] targetAngles = angles[targetRowIndex].angles;
        int minCount = Mathf.Min(targetAngles.Length, revoluteJoints.Count);
        
        float elapsedTime = 0f;
        
        Debug.Log($"JointSpaceControl: 开始三次多项式轨迹，从当前角度到第 {targetRowIndex + 1} 行角度");
        
        while (elapsedTime < trajectoryDuration)
        {
            elapsedTime += Time.fixedDeltaTime;
            float t = elapsedTime / trajectoryDuration;
            
            // 三次多项式插值函数：h(t) = 3t² - 2t³
            // 这个函数满足：h(0)=0, h(1)=1, h'(0)=0, h'(1)=0
            float cubicT = 3f * t * t - 2f * t * t * t;
            
            for (int i = 0; i < minCount; i++)
            {
                if (revoluteJoints[i] != null)
                {
                    // 使用三次多项式插值计算角度
                    float interpolatedAngle = Mathf.Lerp(startAngles[i], targetAngles[i], cubicT);
                    
                    // 设置关节角度
                    SetJointTargetDeg(revoluteJoints[i], interpolatedAngle);
                    
                    // 更新对应的 slider 值
                    if (i < sliders.Count && sliders[i] != null)
                    {
                        var drive = revoluteJoints[i].xDrive;
                        float lowerLimit = drive.lowerLimit;
                        float upperLimit = drive.upperLimit;
                        
                        float sliderValue = 0.5f;
                        if (Mathf.Abs(upperLimit - lowerLimit) > 0.001f)
                        {
                            sliderValue = (interpolatedAngle - lowerLimit) / (upperLimit - lowerLimit);
                            sliderValue = Mathf.Clamp01(sliderValue);
                        }
                        
                        sliders[i].value = sliderValue;
                    }
                }
            }
            
            yield return new WaitForFixedUpdate();
        }
        
        // 轨迹完成，确保精确到达目标角度
        ApplyRowAnglesImmediate(targetRowIndex, false);
        isExecutingTrajectory = false;
        
        Debug.Log($"JointSpaceControl: 三次多项式轨迹执行完成，到达第 {targetRowIndex + 1} 行角度");
    }
    
    /// <summary>
    /// 内部逻辑：将指定行角度立即应用到关节和 slider（原有的立即设置逻辑）
    /// </summary>
    private bool ApplyRowAngles(int rowIndex, bool logResult)
    {
        // 现在 ApplyRowAngles 也使用三次多项式轨迹
        return ExecuteCubicTrajectory(rowIndex);
    }
    
    /// <summary>
    /// 立即设置角度（不经过轨迹）
    /// </summary>
    private bool ApplyRowAnglesImmediate(int rowIndex, bool logResult)
    {
        if (angles == null || rowIndex < 0 || rowIndex >= angles.Count)
        {
            Debug.LogWarning($"JointSpaceControl: 行索引 {rowIndex} 无效！");
            return false;
        }
        
        if (angles[rowIndex] == null || angles[rowIndex].angles == null)
        {
            Debug.LogWarning($"JointSpaceControl: 第 {rowIndex + 1} 行数据为空！");
            return false;
        }
        
        // 确保关节列表已更新
        if (revoluteJoints.Count == 0)
        {
            FindRevoluteJoints();
        }
        
        if (revoluteJoints.Count == 0)
        {
            Debug.LogWarning("JointSpaceControl: 未找到任何关节！");
            return false;
        }
        
        // 设置每个关节的角度，并更新对应的 slider 值
        int minCount = Mathf.Min(angles[rowIndex].angles.Length, revoluteJoints.Count);
        for (int i = 0; i < minCount; i++)
        {
            if (revoluteJoints[i] != null)
            {
                float targetAngle = angles[rowIndex].angles[i];
                
                // 设置关节角度
                SetJointTargetDeg(revoluteJoints[i], targetAngle);
                
                // 更新对应的 slider 值，这样 FixedUpdate 会持续使用这个角度
                if (i < sliders.Count && sliders[i] != null)
                {
                    var drive = revoluteJoints[i].xDrive;
                    float lowerLimit = drive.lowerLimit;
                    float upperLimit = drive.upperLimit;
                    
                    // 计算目标角度对应的 slider 值
                    float sliderValue = 0.5f;
                    if (Mathf.Abs(upperLimit - lowerLimit) > 0.001f)
                    {
                        sliderValue = (targetAngle - lowerLimit) / (upperLimit - lowerLimit);
                        sliderValue = Mathf.Clamp01(sliderValue);
                    }
                    
                    sliders[i].value = 0.95f*sliders[i].value + 0.05f*sliderValue;
                }
            }
        }
        
        if (logResult)
        {
            Debug.Log($"JointSpaceControl: 已将第 {rowIndex + 1} 行的关节角设置到机械臂（共 {minCount} 个关节）");
        }
        
        return true;
    }
    
    private bool ValidateRowIndex(int rowIndex)
    {
        if (angles == null || rowIndex < 0 || rowIndex >= angles.Count)
        {
            Debug.LogWarning($"JointSpaceControl: 行索引 {rowIndex} 无效！");
            return false;
        }
        
        if (angles[rowIndex] == null || angles[rowIndex].angles == null)
        {
            Debug.LogWarning($"JointSpaceControl: 第 {rowIndex + 1} 行数据为空！");
            return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// 删除指定行的数据
    /// </summary>
    public void DeleteRow(int rowIndex)
    {
        if (angles == null || rowIndex < 0 || rowIndex >= angles.Count)
        {
            Debug.LogWarning($"JointSpaceControl: 行索引 {rowIndex} 无效！");
            return;
        }
        
        angles.RemoveAt(rowIndex);
        Debug.Log($"JointSpaceControl: 已删除第 {rowIndex + 1} 行数据");
        
        // 如果正在持续执行且删除了当前行，停止持续执行
        if (continuousSetActive)
        {
            if (rowIndex == continuousSetRowIndex)
            {
                StopContinuousSet();
            }
            else if (rowIndex < continuousSetRowIndex)
            {
                // 调整索引
                continuousSetRowIndex--;
            }
        }
        
        // 标记对象为已修改
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        // 同时保存到 EditorPrefs
        if (Application.isPlaying)
        {
            System.Type helperType = System.Type.GetType("JointSpaceControlEditorHelper");
            if (helperType != null)
            {
                var method = helperType.GetMethod("SaveAnglesToEditorPrefs", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { this });
                }
            }
        }
        #endif
    }
    
    /// <summary>
    /// 获取当前机械臂关节角并保存到 angles 列表（新增一行）
    /// </summary>
    public void GetCurrentAngles()
    {
        if (robot == null)
        {
            Debug.LogWarning("JointSpaceControl: Robot 未赋值！");
            return;
        }
        
        // 确保关节列表已更新
        if (revoluteJoints.Count == 0)
        {
            FindRevoluteJoints();
        }
        
        if (revoluteJoints.Count == 0)
        {
            Debug.LogWarning("JointSpaceControl: 未找到任何关节！");
            return;
        }
        
        // 创建新的一行数据
        JointAngleRow newRow = new JointAngleRow(revoluteJoints.Count);
        
        // 读取每个关节的当前角度并保存到新行
        for (int i = 0; i < revoluteJoints.Count; i++)
        {
            if (revoluteJoints[i] != null)
            {
                // 获取当前关节角度（弧度转度）
                float currentAngleDeg = revoluteJoints[i].jointPosition[0] * Mathf.Rad2Deg;
                newRow.angles[i] = currentAngleDeg;
            }
        }
        
        // 添加到列表
        if (angles == null)
        {
            angles = new List<JointAngleRow>();
        }
        angles.Add(newRow);
        
        Debug.Log($"JointSpaceControl: 已添加一行，保存了 {revoluteJoints.Count} 个关节的当前角度到 angles 列表（共 {angles.Count} 行）");
        
        // 标记对象为已修改，确保数据被保存
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        // 同时保存到 EditorPrefs
        if (Application.isPlaying)
        {
            System.Type helperType = System.Type.GetType("JointSpaceControlEditorHelper");
            if (helperType != null)
            {
                var method = helperType.GetMethod("SaveAnglesToEditorPrefs", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    method.Invoke(null, new object[] { this });
                }
            }
        }
        #endif
    }
}