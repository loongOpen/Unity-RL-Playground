using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Text;

[InitializeOnLoad]
public static class JointSpaceControlEditorHelper
{
    static JointSpaceControlEditorHelper()
    {
        // 监听 Play 模式状态变化
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // 当退出 Play 模式时保存数据
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // 查找场景中所有的 JointSpaceControl 实例
            JointSpaceControl[] allControls = Object.FindObjectsOfType<JointSpaceControl>();
            
            foreach (JointSpaceControl jointControl in allControls)
            {
                if (jointControl != null && jointControl.angles != null && jointControl.angles.Count > 0)
                {
                    SaveAnglesToEditorPrefs(jointControl);
                }
            }
        }
        // 当进入编辑模式时恢复数据
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // 延迟执行，确保场景已恢复
            EditorApplication.delayCall += () =>
            {
                JointSpaceControl[] allControls = Object.FindObjectsOfType<JointSpaceControl>();
                foreach (JointSpaceControl jointControl in allControls)
                {
                    RestoreAnglesFromEditorPrefs(jointControl);
                }
            };
        }
    }
    
    static string GetPrefsKey(JointSpaceControl jointControl)
    {
        // 使用场景路径和对象在场景中的完整路径作为唯一键
        string scenePath = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
        string objectPath = GetGameObjectPath(jointControl.gameObject);
        return $"JointSpaceControl_Angles_{scenePath}_{objectPath}";
    }
    
    static string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
    
    public static void SaveAnglesToEditorPrefs(JointSpaceControl jointControl)
    {
        if (jointControl.angles == null || jointControl.angles.Count == 0)
            return;
        
        string key = GetPrefsKey(jointControl);
        
        // 将数据序列化为 JSON 格式的字符串
        StringBuilder sb = new StringBuilder();
        sb.Append(jointControl.angles.Count).Append("|"); // 行数
        
        for (int i = 0; i < jointControl.angles.Count; i++)
        {
            if (jointControl.angles[i] != null && jointControl.angles[i].angles != null)
            {
                sb.Append(jointControl.angles[i].angles.Length).Append("|"); // 每行的元素数
                for (int j = 0; j < jointControl.angles[i].angles.Length; j++)
                {
                    sb.Append(jointControl.angles[i].angles[j].ToString("F6"));
                    if (j < jointControl.angles[i].angles.Length - 1)
                        sb.Append(",");
                }
                if (i < jointControl.angles.Count - 1)
                    sb.Append("|");
            }
        }
        
        EditorPrefs.SetString(key, sb.ToString());
        
        // 同时保存到序列化属性
        SaveAnglesToSerializedProperty(jointControl);
    }
    
    public static void RestoreAnglesFromEditorPrefs(JointSpaceControl jointControl)
    {
        string key = GetPrefsKey(jointControl);
        
        if (!EditorPrefs.HasKey(key))
            return;
        
        string data = EditorPrefs.GetString(key);
        if (string.IsNullOrEmpty(data))
            return;
        
        // 解析数据
        string[] parts = data.Split('|');
        if (parts.Length < 2)
            return;
        
        int rowCount = int.Parse(parts[0]);
        if (rowCount == 0)
            return;
        
        // 确保运行时列表已初始化
        if (jointControl.angles == null)
        {
            jointControl.angles = new List<JointAngleRow>();
        }
        
        // 清空现有数据
        jointControl.angles.Clear();
        
        int partIndex = 1;
        for (int i = 0; i < rowCount && partIndex < parts.Length; i++)
        {
            int angleCount = int.Parse(parts[partIndex++]);
            if (partIndex < parts.Length)
            {
                string[] angleStrings = parts[partIndex++].Split(',');
                if (angleStrings.Length == angleCount)
                {
                    JointAngleRow row = new JointAngleRow(angleCount);
                    for (int j = 0; j < angleCount; j++)
                    {
                        if (float.TryParse(angleStrings[j], out float angle))
                        {
                            row.angles[j] = angle;
                        }
                    }
                    jointControl.angles.Add(row);
                }
            }
        }
        
        // 保存到序列化属性
        SaveAnglesToSerializedProperty(jointControl);
        EditorUtility.SetDirty(jointControl);
    }
    
    static void SaveAnglesToSerializedProperty(JointSpaceControl jointControl)
    {
        // 为每个对象创建新的 SerializedObject
        SerializedObject so = new SerializedObject(jointControl);
        SerializedProperty anglesProp = so.FindProperty("angles");
        
        if (anglesProp != null && jointControl.angles != null)
        {
            so.Update();
            
            // 清空现有数据
            anglesProp.ClearArray();
            
            // 复制运行时数据到序列化属性
            for (int i = 0; i < jointControl.angles.Count; i++)
            {
                if (jointControl.angles[i] != null && jointControl.angles[i].angles != null)
                {
                    anglesProp.InsertArrayElementAtIndex(i);
                    SerializedProperty rowProperty = anglesProp.GetArrayElementAtIndex(i);
                    SerializedProperty anglesArrayProperty = rowProperty.FindPropertyRelative("angles");
                    
                    if (anglesArrayProperty != null)
                    {
                        anglesArrayProperty.ClearArray();
                        for (int j = 0; j < jointControl.angles[i].angles.Length; j++)
                        {
                            anglesArrayProperty.InsertArrayElementAtIndex(j);
                            anglesArrayProperty.GetArrayElementAtIndex(j).floatValue = jointControl.angles[i].angles[j];
                        }
                    }
                }
            }
            
            so.ApplyModifiedProperties();
            // 标记对象为已修改
            EditorUtility.SetDirty(jointControl);
        }
    }
}

[CustomEditor(typeof(JointSpaceControl))]
[CanEditMultipleObjects]
public class JointSpaceControlEditor : Editor
{
    private SerializedProperty robotProperty;
    private SerializedProperty slidersProperty;
    private SerializedProperty stiffnessProperty;
    private SerializedProperty dampingProperty;
    private SerializedProperty anglesProperty;
    
    void OnEnable()
    {
        robotProperty = serializedObject.FindProperty("robot");
        slidersProperty = serializedObject.FindProperty("sliders");
        stiffnessProperty = serializedObject.FindProperty("stiffness");
        dampingProperty = serializedObject.FindProperty("damping");
        anglesProperty = serializedObject.FindProperty("angles");
        
        // 如果不是在运行模式，尝试从 EditorPrefs 恢复数据
        if (!Application.isPlaying)
        {
            JointSpaceControl jointControl = (JointSpaceControl)target;
            if (jointControl != null)
            {
                JointSpaceControlEditorHelper.RestoreAnglesFromEditorPrefs(jointControl);
            }
        }
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        JointSpaceControl jointControl = (JointSpaceControl)target;
        
        // 绘制 Robot Configuration
        EditorGUILayout.PropertyField(robotProperty);
        
        EditorGUILayout.Space();
        
        // 绘制 Slider Controls
        EditorGUILayout.LabelField("Slider Controls", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(slidersProperty, true);
        
        EditorGUILayout.Space();
        
        // 绘制 Joint Settings
        EditorGUILayout.LabelField("Joint Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(stiffnessProperty);
        EditorGUILayout.PropertyField(dampingProperty);
        
        EditorGUILayout.Space();
        
        // 绘制 Joint Angles（自定义显示）
        EditorGUILayout.LabelField("Joint Angles", EditorStyles.boldLabel);
        
        if (jointControl.angles != null && jointControl.angles.Count > 0)
        {
            for (int i = 0; i < jointControl.angles.Count; i++)
            {
                if (jointControl.angles[i] != null && jointControl.angles[i].angles != null)
                {
                    // 将数组转换为逗号分隔的字符串
                    string angleString = string.Join(", ", jointControl.angles[i].angles.Select(a => a.ToString("F2")));
                    
                    // 显示为文本字段，前面添加 Set 按钮，后面添加删除按钮
                    EditorGUILayout.BeginHorizontal();
                    
                    // Set 按钮
                    if (GUILayout.Button("Set", GUILayout.Width(50)))
                    {
                        jointControl.SetAnglesFromRow(i);
                        EditorUtility.SetDirty(jointControl);
                    }
                    
                    EditorGUILayout.LabelField($"Row {i + 1}:", GUILayout.Width(60));
                    EditorGUILayout.TextField(angleString);
                    
                    // 删除按钮（减号）
                    if (GUILayout.Button("−", GUILayout.Width(30)))
                    {
                        jointControl.DeleteRow(i);
                        EditorUtility.SetDirty(jointControl);
                        serializedObject.Update();
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("暂无数据，点击 'Get Current Angles' 按钮添加", MessageType.Info);
        }
        
        EditorGUILayout.Space();
        
        // 添加按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Get Current Angles", GUILayout.Height(30)))
        {
            jointControl.GetCurrentAngles();
            EditorUtility.SetDirty(jointControl);
            serializedObject.Update();
        }
        
        if (GUILayout.Button("Clear All", GUILayout.Height(30), GUILayout.Width(100)))
        {
            if (jointControl.angles != null)
            {
                jointControl.angles.Clear();
                EditorUtility.SetDirty(jointControl);
                serializedObject.Update();
                Debug.Log("JointSpaceControl: 已清空 angles 列表");
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.HelpBox($"点击 'Get Current Angles' 按钮将在 angles 列表中添加一行新的关节角数据（当前共 {jointControl.angles?.Count ?? 0} 行）", MessageType.Info);
        
        serializedObject.ApplyModifiedProperties();
    }
}

