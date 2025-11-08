using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(RobotPlot))]
public class RobotPlotEditor : Editor
{
    private RobotPlot recorder;
    private Transform previousRobot; // 记录上一次的 robot 值
    private bool hasInitialized = false; // 标记是否已尝试初始化
    
    void OnEnable()
    {
        recorder = (RobotPlot)target;
        previousRobot = recorder.robot;
        hasInitialized = false;
    }
    
    public override void OnInspectorGUI()
    {
        // 绘制基本属性
        serializedObject.Update();
        
        // 第一行显示 Robot 字段
        var robotProperty = serializedObject.FindProperty("robot");
        EditorGUILayout.PropertyField(robotProperty, new GUIContent("Robot"));
        
        // 显示 Root ArticulationBody 字段（只读，自动填入）
        var rootArticulationBodyProperty = serializedObject.FindProperty("rootArticulationBody");
        EditorGUI.BeginDisabledGroup(true); // 设置为只读
        EditorGUILayout.PropertyField(rootArticulationBodyProperty, new GUIContent("Root ArticulationBody"));
        EditorGUI.EndDisabledGroup();
        
        // 应用修改以获取最新值
        serializedObject.ApplyModifiedProperties();
        
        // 检查 robot 是否发生变化
        if (recorder.robot != previousRobot)
        {
            previousRobot = recorder.robot;
            hasInitialized = false; // 重置初始化标志
            
            // 如果 robot 不为空，自动检测其子节点中的 ArticulationBody
            if (recorder.robot != null)
            {
                InitializeJoints();
                hasInitialized = true;
                
                // 更新 Root ArticulationBody 显示
                serializedObject.Update();
            }
            else
            {
                // 如果 robot 被清空，清空关节列表和 rootArticulationBody
                if (recorder.jointInfos != null && recorder.jointInfos.Count > 0)
                {
                    Undo.RecordObject(recorder, "Clear Joint Infos");
                    recorder.jointInfos.Clear();
                    EditorUtility.SetDirty(recorder);
                }
                if (recorder.rootArticulationBody != null)
                {
                    Undo.RecordObject(recorder, "Clear Root ArticulationBody");
                    recorder.rootArticulationBody = null;
                    EditorUtility.SetDirty(recorder);
                }
            }
        }
        // 如果 robot 已设置但关节列表为空，且尚未初始化，自动初始化
        else if (recorder.robot != null && 
                 (recorder.jointInfos == null || recorder.jointInfos.Count == 0) && 
                 !hasInitialized)
        {
            InitializeJoints();
            hasInitialized = true;
            
            // 更新 Root ArticulationBody 显示
            serializedObject.Update();
        }
        
        // 如果 robot 已设置但 rootArticulationBody 为空，尝试自动查找
        if (recorder.robot != null && recorder.rootArticulationBody == null)
        {
            // 重新更新序列化对象以获取最新值
            serializedObject.Update();
        }
        
        // 重新更新以继续绘制其他字段
        serializedObject.Update();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Charts", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("jointAngleChart"), new GUIContent("Joint Angle Chart"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("eulerAngleChart"), new GUIContent("Euler Angle Chart"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("linearVelocityChart"), new GUIContent("Linear Velocity Chart"));
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDataCount"));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Background Settings", EditorStyles.boldLabel);
        
        // 背景透明度滑块
        EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundAlpha"), 
            new GUIContent("Background Alpha"));
        
        // 显示当前透明度百分比
        EditorGUILayout.LabelField($"Current Alpha: {recorder.backgroundAlpha * 100f:F0}%", EditorStyles.helpBox);
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Joint Selection", EditorStyles.boldLabel);
        
        // 如果 Robot 未设置，显示提示
        if (recorder.robot == null)
        {
            EditorGUILayout.HelpBox("Please set Robot field to auto-detect joints", MessageType.Info);
        }
        // 如果关节列表为空，显示提示
        else if (recorder.jointInfos == null || recorder.jointInfos.Count == 0)
        {
            EditorGUILayout.HelpBox("No joints detected. Please check:\n1. Robot object and its children contain ArticulationBody components\n2. Joint type is RevoluteJoint", MessageType.Warning);
        }
        else
        {
            // 显示全选/全不选按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All"))
            {
                SetAllJointsSelected(true);
            }
            if (GUILayout.Button("Deselect All"))
            {
                SetAllJointsSelected(false);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 显示每个关节的复选框
            EditorGUI.indentLevel++;
            for (int i = 0; i < recorder.jointInfos.Count; i++)
            {
                var info = recorder.jointInfos[i];
                if (info.joint == null)
                {
                    // 如果关节引用丢失，显示名称
                    info.isSelected = EditorGUILayout.Toggle(info.jointName, info.isSelected);
                }
                else
                {
                    info.isSelected = EditorGUILayout.Toggle(info.joint.gameObject.name, info.isSelected);
                }
            }
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space();
            
            // 显示选中数量
            int selectedCount = 0;
            foreach (var info in recorder.jointInfos)
            {
                if (info.isSelected) selectedCount++;
            }
            EditorGUILayout.LabelField($"Selected: {selectedCount} / {recorder.jointInfos.Count}", EditorStyles.helpBox);
        }
        
        // 标记为已修改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(recorder);
        }
    }
    
    void InitializeJoints()
    {
        if (recorder.robot == null)
        {
            return;
        }
        
        // 记录 Undo
        Undo.RecordObject(recorder, "Initialize Joint Infos");
        
        // 调用初始化方法
        recorder.InitializeJointInfos();
        
        // 标记为已修改
        EditorUtility.SetDirty(recorder);
        
        // 强制刷新序列化对象
        serializedObject.Update();
        serializedObject.ApplyModifiedProperties();
        
        // 强制刷新 Inspector
        Repaint();
    }
    
    void SetAllJointsSelected(bool selected)
    {
        foreach (var info in recorder.jointInfos)
        {
            info.isSelected = selected;
        }
        EditorUtility.SetDirty(recorder);
    }
}

