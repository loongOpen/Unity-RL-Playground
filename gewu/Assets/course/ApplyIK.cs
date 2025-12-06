using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ApplyIK脚本：从ikrobot获取所有revolute joint的关节角，并应用到挂载此脚本的机器人上同名的关节
/// </summary>
public class ApplyIK : MonoBehaviour
{
    [Header("IK Robot Configuration")]
    [Tooltip("IK机器人Transform，从此对象获取关节角度")]
    public Transform ikRobot;
    
    [Header("Joint Settings")]
    [Tooltip("关节刚度和阻尼（用于设置目标角度）")]
    public float stiffness = 2000f;
    public float damping = 200f;
    
    // IK机器人的旋转关节列表
    private List<ArticulationBody> ikRevoluteJoints = new List<ArticulationBody>();
    private Dictionary<string, ArticulationBody> ikJointDict = new Dictionary<string, ArticulationBody>();
    
    // 当前机器人的旋转关节字典（按名称索引）
    private Dictionary<string, ArticulationBody> targetJointDict = new Dictionary<string, ArticulationBody>();
    
    void Start()
    {
        InitializeJoints();
    }
    
    /// <summary>
    /// 初始化关节
    /// </summary>
    void InitializeJoints()
    {
        // 清空字典
        ikJointDict.Clear();
        targetJointDict.Clear();
        ikRevoluteJoints.Clear();
        
        // 获取IK机器人的所有旋转关节
        if (ikRobot != null)
        {
            ArticulationBody[] ikBodies = ikRobot.GetComponentsInChildren<ArticulationBody>(true);
            foreach (ArticulationBody body in ikBodies)
            {
                if (body.jointType == ArticulationJointType.RevoluteJoint)
                {
                    ikRevoluteJoints.Add(body);
                    ikJointDict[body.gameObject.name] = body;
                }
            }
            Debug.Log($"ApplyIK: 从IK机器人找到 {ikRevoluteJoints.Count} 个旋转关节");
        }
        else
        {
            Debug.LogWarning("ApplyIK: ikRobot未设置，无法获取关节角度");
        }
        
        // 获取当前机器人的所有旋转关节
        ArticulationBody[] targetBodies = GetComponentsInChildren<ArticulationBody>(true);
        foreach (ArticulationBody body in targetBodies)
        {
            if (body.jointType == ArticulationJointType.RevoluteJoint)
            {
                targetJointDict[body.gameObject.name] = body;
            }
        }
        Debug.Log($"ApplyIK: 从当前机器人找到 {targetJointDict.Count} 个旋转关节");
    }
    
    void FixedUpdate()
    {
        ApplyJointAngles();
    }
    
    /// <summary>
    /// 应用关节角度：从IK机器人复制到当前机器人
    /// </summary>
    void ApplyJointAngles()
    {
        if (ikRobot == null)
            return;
        
        // 遍历IK机器人的所有旋转关节
        foreach (var kvp in ikJointDict)
        {
            string jointName = kvp.Key;
            ArticulationBody ikJoint = kvp.Value;
            
            // 在当前机器人中查找同名关节
            if (targetJointDict.ContainsKey(jointName))
            {
                ArticulationBody targetJoint = targetJointDict[jointName];
                
                // 获取IK关节的角度
                if (ikJoint.jointPosition.dofCount > 0)
                {
                    float ikAngleRad = ikJoint.jointPosition[0];
                    float ikAngleDeg = ikAngleRad * Mathf.Rad2Deg;
                    
                    // 应用到目标关节
                    SetJointTargetDeg(targetJoint, ikAngleDeg);
                }
            }
        }
    }
    
    /// <summary>
    /// 设置关节目标角度（度）
    /// </summary>
    void SetJointTargetDeg(ArticulationBody joint, float angleDeg)
    {
        var drive = joint.xDrive;
        drive.stiffness = stiffness;
        drive.damping = damping;
        drive.target = angleDeg;
        joint.xDrive = drive;
    }
    
    /// <summary>
    /// 在编辑器中刷新关节列表（用于Inspector）
    /// </summary>
    [ContextMenu("刷新关节列表")]
    void RefreshJointList()
    {
        InitializeJoints();
        Debug.Log($"ApplyIK: 已刷新关节列表 - IK机器人: {ikJointDict.Count} 个关节, 当前机器人: {targetJointDict.Count} 个关节");
    }
}

