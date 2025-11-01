using UnityEngine;

/// <summary>
/// 手掌控制测试脚本
/// 用于测试手掌关节角度读取和归一化功能
/// </summary>
public class HandControlTest : MonoBehaviour
{
    [Header("测试配置")]
    public JointAngleMonitor monitor;
    public bool enableDebugLog = true;
    public float logInterval = 1.0f; // 日志输出间隔（秒）
    
    private float lastLogTime = 0f;

    void Start()
    {
        if (monitor == null)
        {
            monitor = GetComponent<JointAngleMonitor>();
        }
        
        if (monitor == null)
        {
            Debug.LogError("[HandControlTest] 未找到JointAngleMonitor组件！");
            enabled = false;
            return;
        }
        
        Debug.Log("[HandControlTest] 手掌控制测试已启动");
        LogHandJointInfo();
    }

    void Update()
    {
        if (!enableDebugLog || Time.time - lastLogTime < logInterval) return;
        
        lastLogTime = Time.time;
        LogHandAngles();
    }

    void LogHandJointInfo()
    {
        if (monitor == null) return;
        
        Debug.Log($"[HandControlTest] 启用手掌监控: {monitor.enableHandMonitoring}");
        Debug.Log($"[HandControlTest] 左手关节数量: {monitor.leftHandJoints.Count}");
        Debug.Log($"[HandControlTest] 右手关节数量: {monitor.rightHandJoints.Count}");
        
        // 检查左手关节
        for (int i = 0; i < monitor.leftHandJoints.Count; i++)
        {
            var joint = monitor.leftHandJoints[i];
            if (joint != null)
            {
                Debug.Log($"[HandControlTest] 左手关节{i}: {joint.name}");
            }
            else
            {
                Debug.LogWarning($"[HandControlTest] 左手关节{i}: 未设置");
            }
        }
        
        // 检查右手关节
        for (int i = 0; i < monitor.rightHandJoints.Count; i++)
        {
            var joint = monitor.rightHandJoints[i];
            if (joint != null)
            {
                Debug.Log($"[HandControlTest] 右手关节{i}: {joint.name}");
            }
            else
            {
                Debug.LogWarning($"[HandControlTest] 右手关节{i}: 未设置");
            }
        }
    }

    void LogHandAngles()
    {
        if (monitor == null || !monitor.enableHandMonitoring) return;
        
        var leftAngles = monitor.GetLeftHandAnglesNormalized();
        var rightAngles = monitor.GetRightHandAnglesNormalized();
        
        string leftStr = string.Join(", ", System.Array.ConvertAll(leftAngles, x => x.ToString("F3")));
        string rightStr = string.Join(", ", System.Array.ConvertAll(rightAngles, x => x.ToString("F3")));
        
        Debug.Log($"[HandControlTest] 左手角度: [{leftStr}]");
        Debug.Log($"[HandControlTest] 右手角度: [{rightStr}]");
    }

    [ContextMenu("测试手掌角度")]
    public void TestHandAngles()
    {
        if (monitor == null) return;
        
        LogHandAngles();
        
        // 测试特定角度值
        var leftAngles = monitor.GetLeftHandAnglesNormalized();
        var rightAngles = monitor.GetRightHandAnglesNormalized();
        
        Debug.Log($"[HandControlTest] 左手角度检查:");
        Debug.Log($"  thumb_0 (固定): {leftAngles[0]:F3} (期望: 0.500)");
        Debug.Log($"  thumb_1: {leftAngles[1]:F3} (30°→-60° = 0→1)");
        Debug.Log($"  thumb_2: {leftAngles[2]:F3} (0°→-90° = 0→1)");
        Debug.Log($"  middle_0: {leftAngles[3]:F3} (0°→90° = 1→0)");
        Debug.Log($"  middle_1: {leftAngles[4]:F3} (0°→90° = 1→0)");
        Debug.Log($"  index_0: {leftAngles[5]:F3} (0°→90° = 1→0)");
        Debug.Log($"  index_1: {leftAngles[6]:F3} (0°→90° = 1→0)");
        
        Debug.Log($"[HandControlTest] 右手角度检查:");
        Debug.Log($"  thumb_0 (固定): {rightAngles[0]:F3} (期望: 0.500)");
        Debug.Log($"  thumb_1: {rightAngles[1]:F3} (-30°→60° = 1→0)");
        Debug.Log($"  thumb_2: {rightAngles[2]:F3} (0°→90° = 1→0)");
        Debug.Log($"  middle_0: {rightAngles[3]:F3} (0°→-90° = 0→1)");
        Debug.Log($"  middle_1: {rightAngles[4]:F3} (0°→-90° = 0→1)");
        Debug.Log($"  index_0: {rightAngles[5]:F3} (0°→-90° = 0→1)");
        Debug.Log($"  index_1: {rightAngles[6]:F3} (0°→-90° = 0→1)");
    }
}
