using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(JointAngleRecorder))]
public class JointAngleRecorderEditor : Editor
{
    private JointAngleRecorder recorder;
    
    void OnEnable()
    {
        recorder = (JointAngleRecorder)target;
    }
    
    public override void OnInspectorGUI()
    {
        // 绘制基本属性（排除纵轴设置，后面自定义显示）
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lineChart"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxDataCount"));
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("纵轴设置", EditorStyles.boldLabel);
        
        // 纵轴自动调节复选框
        EditorGUILayout.PropertyField(serializedObject.FindProperty("yAxisAutoRange"), 
            new GUIContent("自动调节范围"));
        
        // 如果不自动调节，显示最小最大值输入框
        if (!recorder.yAxisAutoRange)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("yAxisMin"), 
                new GUIContent("最小值 (度)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("yAxisMax"), 
                new GUIContent("最大值 (度)"));
            
            // 验证最小值小于最大值
            if (recorder.yAxisMin >= recorder.yAxisMax)
            {
                EditorGUILayout.HelpBox("最小值必须小于最大值", MessageType.Warning);
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("背景设置", EditorStyles.boldLabel);
        
        // 背景透明度滑块
        EditorGUILayout.PropertyField(serializedObject.FindProperty("backgroundAlpha"), 
            new GUIContent("背景透明度"));
        
        // 显示当前透明度百分比
        EditorGUILayout.LabelField($"当前透明度: {recorder.backgroundAlpha * 100f:F0}%", EditorStyles.helpBox);
        
        serializedObject.ApplyModifiedProperties();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("关节选择", EditorStyles.boldLabel);
        
        // 如果关节列表为空，显示初始化按钮
        if (recorder.jointInfos == null || recorder.jointInfos.Count == 0)
        {
            EditorGUILayout.HelpBox("关节列表为空，点击下方按钮初始化关节列表", MessageType.Info);
            if (GUILayout.Button("初始化关节列表"))
            {
                InitializeJoints();
            }
        }
        else
        {
            // 显示全选/全不选按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("全选"))
            {
                SetAllJointsSelected(true);
            }
            if (GUILayout.Button("全不选"))
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
            EditorGUILayout.LabelField($"已选中: {selectedCount} / {recorder.jointInfos.Count}", EditorStyles.helpBox);
        }
        
        // 标记为已修改
        if (GUI.changed)
        {
            EditorUtility.SetDirty(recorder);
        }
    }
    
    void InitializeJoints()
    {
        recorder.InitializeJointInfos();
        EditorUtility.SetDirty(recorder);
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

