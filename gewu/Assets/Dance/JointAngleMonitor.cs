using System.Collections.Generic;
using UnityEngine;

public class JointAngleMonitor : MonoBehaviour
{
    [Tooltip("从此物体向下搜集 Revolute 关节(ArticulationBody)。不指定则默认本对象。")]
    public Transform root;

    [Tooltip("仅包含名称包含这些子串的关节（为空表示不过滤）")]
    public List<string> includeNameContains = new List<string>();

    [Tooltip("排除名称包含这些子串的关节（优先级高于包含过滤）")]
    public List<string> excludeNameContains = new List<string>() { "hip", "knee", "ankle", "leg" };

    [Header("手掌控制")]
    [Tooltip("是否启用手掌关节监控")]
    public bool enableHandMonitoring = true;
    
    [Tooltip("是否自动识别手掌关节（推荐启用）")]
    public bool autoDetectHandJoints = true;
    
    [Tooltip("左手手掌关节Transform列表（按dex3顺序：thumb_0, thumb_1, thumb_2, middle_0, middle_1, index_0, index_1）")]
    public List<Transform> leftHandJoints = new List<Transform>();
    
    [Tooltip("右手手掌关节Transform列表（按dex3顺序：thumb_0, thumb_1, thumb_2, middle_0, middle_1, index_0, index_1）")]
    public List<Transform> rightHandJoints = new List<Transform>();

    private readonly List<ArticulationBody> revoluteJoints = new List<ArticulationBody>();
    private readonly List<string> jointNames = new List<string>();
    
    // 手掌关节数据
    private float[] leftHandAngles = new float[7];
    private float[] rightHandAngles = new float[7];
    
    // 调试标志
    private bool printedLeftHandDebug = false;
    private bool printedRightHandDebug = false;

    public bool logOnce = true;

    void Awake()
    {
        if (root == null) root = transform;
        revoluteJoints.Clear();
        jointNames.Clear();

        var all = root.GetComponentsInChildren<ArticulationBody>(true);
        foreach (var ab in all)
        {
            if (ab.jointType != ArticulationJointType.RevoluteJoint) continue;

            string n = ab.name.ToLowerInvariant();
            bool excluded = false;
            foreach (var ex in excludeNameContains)
            {
                if (!string.IsNullOrEmpty(ex) && n.Contains(ex.ToLowerInvariant()))
                {
                    excluded = true;
                    break;
                }
            }
            if (excluded) continue;

            if (includeNameContains != null && includeNameContains.Count > 0)
            {
                bool matched = false;
                foreach (var inc in includeNameContains)
                {
                    if (!string.IsNullOrEmpty(inc) && n.Contains(inc.ToLowerInvariant()))
                    {
                        matched = true;
                        break;
                    }
                }
                if (!matched) continue;
            }

            revoluteJoints.Add(ab);
            jointNames.Add(ab.name);
        }
        if (logOnce)
        {
            Debug.Log($"[JointAngleMonitor] Found {revoluteJoints.Count} revolute joints under '{root.name}'.");
            var names = string.Join(", ", jointNames);
            Debug.Log($"[JointAngleMonitor] Joints: {names}");
        }

        // 自动识别手掌关节
        if (enableHandMonitoring && autoDetectHandJoints)
        {
            AutoDetectHandJoints();
        }
    }

    public int JointCount => revoluteJoints.Count;

    public string[] GetJointNames()
    {
        return jointNames.ToArray();
    }

    public float[] GetAnglesRad()
    {
        var arr = new float[revoluteJoints.Count];
        for (int i = 0; i < revoluteJoints.Count; i++)
        {
            arr[i] = revoluteJoints[i].jointPosition[0];
        }
        return arr;
    }

    public float[] GetAnglesDeg()
    {
        var arr = new float[revoluteJoints.Count];
        for (int i = 0; i < revoluteJoints.Count; i++)
        {
            arr[i] = revoluteJoints[i].jointPosition[0] * Mathf.Rad2Deg;
        }
        return arr;
    }

    /// <summary>
    /// 获取左手手掌关节角度（归一化到0-1范围）
    /// </summary>
    public float[] GetLeftHandAnglesNormalized()
    {
        if (!enableHandMonitoring || leftHandJoints.Count != 7) return new float[7];
        
        for (int i = 0; i < 7; i++)
        {
            if (leftHandJoints[i] == null) continue;
            
            float angle = leftHandJoints[i].localRotation.eulerAngles.y;
            // 将角度转换为-180到180范围
            if (angle > 180f) angle -= 360f;
            
            // 根据关节类型进行归一化
            float normalizedAngle = NormalizeHandAngle(angle, i, true);
            leftHandAngles[i] = Mathf.Clamp01(normalizedAngle);
            
            // 调试输出（仅第一次）
            if (!printedLeftHandDebug)
            {
                Debug.Log($"[JointAngleMonitor] Left[{i}] {leftHandJoints[i].name}: angle={angle:F2}°, normalized={normalizedAngle:F3}");
            }
        }
        if (!printedLeftHandDebug) printedLeftHandDebug = true;
        
        return leftHandAngles;
    }

    /// <summary>
    /// 获取右手手掌关节角度（归一化到0-1范围）
    /// </summary>
    public float[] GetRightHandAnglesNormalized()
    {
        if (!enableHandMonitoring || rightHandJoints.Count != 7) return new float[7];
        
        for (int i = 0; i < 7; i++)
        {
            if (rightHandJoints[i] == null) continue;
            
            float angle = rightHandJoints[i].localRotation.eulerAngles.y;
            // 将角度转换为-180到180范围
            if (angle > 180f) angle -= 360f;
            
            // 根据关节类型进行归一化
            float normalizedAngle = NormalizeHandAngle(angle, i, false);
            rightHandAngles[i] = Mathf.Clamp01(normalizedAngle);
            
            // 调试输出（仅第一次）
            if (!printedRightHandDebug)
            {
                Debug.Log($"[JointAngleMonitor] Right[{i}] {rightHandJoints[i].name}: angle={angle:F2}°, normalized={normalizedAngle:F3}");
            }
            
        }
        if (!printedRightHandDebug) printedRightHandDebug = true;
        
        return rightHandAngles;
    }

    /// <summary>
    /// 归一化手掌关节角度
    /// </summary>
    /// <param name="angle">原始角度（度）</param>
    /// <param name="jointIndex">关节索引（0-6）</param>
    /// <param name="isLeftHand">是否为左手</param>
    /// <returns>归一化后的角度（0-1）</returns>
    private float NormalizeHandAngle(float angle, int jointIndex, bool isLeftHand)
    {
        // 关节映射：0=thumb_0, 1=thumb_1, 2=thumb_2, 3=middle_0, 4=middle_1, 5=index_0, 6=index_1
        
        if (isLeftHand)
        {
            // 左手规则（基于精确测试数据）
            // 张开(0,0,0,0,0,0,0) → (0.5,0.3,0,1,1,1,1)
            // 闭合(0,-40,-40,40,40,40,40) → (0.5,0.78,0.45,0.56,0.56,0.56,0.56)
            switch (jointIndex)
            {
                case 0: // thumb_0 - 固定值0.5
                    return 0.5f;
                case 1: // thumb_1 - 从0度到-40度 → 从0.3到0.78
                    return Mathf.Clamp01(0.3f + (-angle / 40f) * (0.78f - 0.3f));
                case 2: // thumb_2 - 从0度到-40度 → 从0到0.45
                    return Mathf.Clamp01((-angle / 40f) * 0.45f);
                case 3: // middle_0 - 从0度到40度 → 从1到0.56
                    return Mathf.Clamp01(1f - (angle / 40f) * (1f - 0.56f));
                case 4: // middle_1 - 从0度到40度 → 从1到0.56
                    return Mathf.Clamp01(1f - (angle / 40f) * (1f - 0.56f));
                case 5: // index_0 - 从0度到40度 → 从1到0.56
                    return Mathf.Clamp01(1f - (angle / 40f) * (1f - 0.56f));
                case 6: // index_1 - 从0度到40度 → 从1到0.56
                    return Mathf.Clamp01(1f - (angle / 40f) * (1f - 0.56f));
                default:
                    return 0.5f;
            }
        }
        else
        {
            // 右手规则（基于精确测试数据）
            // 张开(0,0,0,0,0,0,0) → (0.5,0.67,1,0,0,0,0)
            // 闭合(0,40,40,-40,-40,-40,-40) → (0.5,0.22,0.56,0.45,0.45,0.45,0.45)
            switch (jointIndex)
            {
                case 0: // thumb_0 - 固定值0.5
                    return 0.5f;
                case 1: // thumb_1 - 从0度到40度 → 从0.67到0.22
                    return Mathf.Clamp01(0.67f - (angle / 40f) * (0.67f - 0.22f));
                case 2: // thumb_2 - 从0度到40度 → 从1到0.56
                    return Mathf.Clamp01(1f - (angle / 40f) * (1f - 0.56f));
                case 3: // middle_0 - 从0度到-40度 → 从0到0.45
                    return Mathf.Clamp01((-angle / 40f) * 0.45f);
                case 4: // middle_1 - 从0度到-40度 → 从0到0.45
                    return Mathf.Clamp01((-angle / 40f) * 0.45f);
                case 5: // index_0 - 从0度到-40度 → 从0到0.45
                    return Mathf.Clamp01((-angle / 40f) * 0.45f);
                case 6: // index_1 - 从0度到-40度 → 从0到0.45
                    return Mathf.Clamp01((-angle / 40f) * 0.45f);
                default:
                    return 0.5f;
            }
        }
    }

    /// <summary>
    /// 获取所有手掌数据（左手+右手）
    /// </summary>
    public float[] GetAllHandAnglesNormalized()
    {
        var leftAngles = GetLeftHandAnglesNormalized();
        var rightAngles = GetRightHandAnglesNormalized();
        
        var allAngles = new float[14];
        for (int i = 0; i < 7; i++)
        {
            allAngles[i] = leftAngles[i];
            allAngles[i + 7] = rightAngles[i];
        }
        
        return allAngles;
    }

    /// <summary>
    /// 验证角度归一化计算（调试用）
    /// </summary>
    [ContextMenu("验证角度归一化")]
    private void VerifyAngleNormalization()
    {
        Debug.Log("=== 验证角度归一化计算 ===");
        
        // 测试左手
        Debug.Log("左手测试：");
        Debug.Log($"张开状态(0,0,0,0,0,0,0):");
        for (int i = 0; i < 7; i++)
        {
            float result = NormalizeHandAngle(0f, i, true);
            Debug.Log($"  [{i}] = {result:F3}");
        }
        
        Debug.Log($"闭合状态(0,-40,-40,40,40,40,40):");
        float[] leftClosedAngles = {0f, -40f, -40f, 40f, 40f, 40f, 40f};
        for (int i = 0; i < 7; i++)
        {
            float result = NormalizeHandAngle(leftClosedAngles[i], i, true);
            Debug.Log($"  [{i}] = {result:F3}");
        }
        
        // 测试右手
        Debug.Log("右手测试：");
        Debug.Log($"张开状态(0,0,0,0,0,0,0):");
        for (int i = 0; i < 7; i++)
        {
            float result = NormalizeHandAngle(0f, i, false);
            Debug.Log($"  [{i}] = {result:F3}");
        }
        
        Debug.Log($"闭合状态(0,-40,-40,40,40,40,40):");
        float[] rightClosedAngles = {0f, -40f, -40f, 40f, 40f, 40f, 40f};
        for (int i = 0; i < 7; i++)
        {
            float result = NormalizeHandAngle(rightClosedAngles[i], i, false);
            Debug.Log($"  [{i}] = {result:F3}");
        }
    }

    /// <summary>
    /// 自动识别手掌关节
    /// </summary>
    private void AutoDetectHandJoints()
    {
        // 定义手掌关节名称（按dex3顺序）
        string[] leftHandJointNames = {
            "left_hand_thumb_0_link",
            "left_hand_thumb_1_link", 
            "left_hand_thumb_2_link",
            "left_hand_middle_0_link",
            "left_hand_middle_1_link",
            "left_hand_index_0_link",
            "left_hand_index_1_link"
        };

        string[] rightHandJointNames = {
            "right_hand_thumb_0_link",
            "right_hand_thumb_1_link",
            "right_hand_thumb_2_link", 
            "right_hand_middle_0_link",
            "right_hand_middle_1_link",
            "right_hand_index_0_link",
            "right_hand_index_1_link"
        };

        // 清空现有列表
        leftHandJoints.Clear();
        rightHandJoints.Clear();

        // 获取所有子对象
        var allTransforms = root.GetComponentsInChildren<Transform>(true);

        // 查找左手关节
        foreach (string jointName in leftHandJointNames)
        {
            Transform foundJoint = null;
            foreach (var t in allTransforms)
            {
                if (t.name == jointName)
                {
                    foundJoint = t;
                    break;
                }
            }
            leftHandJoints.Add(foundJoint);
        }

        // 查找右手关节
        foreach (string jointName in rightHandJointNames)
        {
            Transform foundJoint = null;
            foreach (var t in allTransforms)
            {
                if (t.name == jointName)
                {
                    foundJoint = t;
                    break;
                }
            }
            rightHandJoints.Add(foundJoint);
        }

        // 输出识别结果
        if (logOnce)
        {
            Debug.Log($"[JointAngleMonitor] 自动识别手掌关节完成:");
            
            int leftFound = 0, rightFound = 0;
            for (int i = 0; i < 7; i++)
            {
                if (leftHandJoints[i] != null) leftFound++;
                if (rightHandJoints[i] != null) rightFound++;
            }
            
            Debug.Log($"[JointAngleMonitor] 左手关节: {leftFound}/7 个找到");
            Debug.Log($"[JointAngleMonitor] 右手关节: {rightFound}/7 个找到");

            // 详细输出找到的关节
            for (int i = 0; i < 7; i++)
            {
                string leftStatus = leftHandJoints[i] != null ? "✓" : "✗";
                string rightStatus = rightHandJoints[i] != null ? "✓" : "✗";
                //Debug.Log($"[JointAngleMonitor] {leftHandJointNames[i]}: {leftStatus} | {rightHandJointNames[i]}: {rightStatus}");
                
                // 输出实际找到的关节名称
                if (leftHandJoints[i] != null)
                {
                    Debug.Log($"[JointAngleMonitor] Left[{i}] = {leftHandJoints[i].name}");
                }
                if (rightHandJoints[i] != null)
                {
                    Debug.Log($"[JointAngleMonitor] Right[{i}] = {rightHandJoints[i].name}");
                }
            }

            if (leftFound == 0 && rightFound == 0)
            {
                Debug.LogWarning($"[JointAngleMonitor] 未找到任何手掌关节！请检查关节命名是否正确。");
            }
        }
    }

    /// <summary>
    /// 手动重新识别手掌关节（可在Inspector中调用）
    /// </summary>
    [ContextMenu("重新识别手掌关节")]
    public void ReDetectHandJoints()
    {
        AutoDetectHandJoints();
    }
}


