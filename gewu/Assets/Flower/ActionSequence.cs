using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gewu.Flower
{
    /// <summary>
    /// Holds a sequence of joint-angle or IK actions and exposes quick-add buttons in the inspector.
    /// </summary>
    public class ActionSequence : MonoBehaviour
    {
        [Header("IK Controller")]
        public GewuIK GewuIK;
        
        [Header("Gripper")]
        public Gripper Gripper;
        
        [Header("Animation Settings")]
        [Tooltip("关节角度过渡的持续时间（秒）")]
        private float runDuration = 1f;
        [Tooltip("IK 移动的持续时间（秒）")]
        private float ikDuration = 1f;
        [Tooltip("顺序执行时，每个动作之间的间隔时间（秒）")]
        public float actionInterval = 2f;

        public List<ActionItem> actions = new List<ActionItem>();
        
        private Coroutine currentActionCoroutine;

        [System.Serializable]
        public class ActionItem
        {
            public List<float> jointAngles = new List<float>();
            public string rawText = ""; // 存储原始文本，用于解析 IK 命令（改为可序列化，避免运行时丢失）
        }
        
        /// <summary>
        /// IK 移动命令数据
        /// </summary>
        [System.Serializable]
        public class IKMoveCommand
        {
            public Vector3 moveDelta = Vector3.zero;
            public bool isValid = false;
        }

        /// <summary>
        /// 执行指定的 action，将关节角平滑地发送给机器人
        /// </summary>
        public void RunAction(int index)
        {
            if (index < 0 || index >= actions.Count)
            {
                Debug.LogWarning("ActionSequence: index out of range");
                return;
            }

            // 停止之前的动作
            if (currentActionCoroutine != null)
            {
                StopCoroutine(currentActionCoroutine);
            }
            
            // 启动新的动作协程
            currentActionCoroutine = StartCoroutine(RunActionCoroutine(index));
        }
        
        /// <summary>
        /// 按顺序执行所有 action
        /// </summary>
        public void RunAllActions()
        {
            // 停止之前的动作
            if (currentActionCoroutine != null)
            {
                StopCoroutine(currentActionCoroutine);
            }
            
            // 启动顺序执行协程
            currentActionCoroutine = StartCoroutine(RunAllActionsCoroutine());
        }
        
        /// <summary>
        /// 顺序执行所有 action 的协程
        /// </summary>
        IEnumerator RunAllActionsCoroutine()
        {
            if (actions == null || actions.Count == 0)
            {
                Debug.LogWarning("ActionSequence: No actions to run");
                yield break;
            }
            
            Debug.Log($"ActionSequence: Starting sequential execution of {actions.Count} actions");
            
            for (int i = 0; i < actions.Count; i++)
            {
                Debug.Log($"ActionSequence: Running action {i + 1}/{actions.Count}");
                
                // 执行当前 action
                yield return StartCoroutine(RunActionCoroutine(i));
                
                // 如果不是最后一个 action，等待间隔时间
                if (i < actions.Count - 1)
                {
                    yield return new WaitForSeconds(actionInterval);
                }
            }
            
            Debug.Log("ActionSequence: Sequential execution completed");
            currentActionCoroutine = null;
        }
        
        /// <summary>
        /// 执行动作的协程，平滑过渡关节角度或执行 IK 移动
        /// </summary>
        IEnumerator RunActionCoroutine(int index)
        {
            var item = actions[index];
            
            // 检查是否是 Gripper 命令（不需要 GewuIK）
            string gripperCommand = ParseGripperCommand(item.rawText);
            if (!string.IsNullOrEmpty(gripperCommand))
            {
                // 执行 Gripper 命令
                yield return StartCoroutine(RunGripperCommandCoroutine(gripperCommand));
            }
            // 检查是否是 IK 命令格式
            else
            {
                IKMoveCommand ikCommand = ParseIKCommand(item.rawText);
                if (ikCommand.isValid)
                {
                    // 执行 IK 移动
                    if (GewuIK == null || GewuIK.robot == null)
                    {
                        Debug.LogWarning("ActionSequence: GewuIK or robot is null");
                        yield break;
                    }
                    yield return StartCoroutine(RunIKMoveCoroutine(ikCommand));
                }
                else
                {
                    // 执行关节角移动
                    if (GewuIK == null || GewuIK.robot == null)
                    {
                        Debug.LogWarning("ActionSequence: GewuIK or robot is null");
                        yield break;
                    }
                    yield return StartCoroutine(RunJointAnglesCoroutine(item));
                }
            }
        }
        
        /// <summary>
        /// 执行关节角移动的协程
        /// </summary>
        IEnumerator RunJointAnglesCoroutine(ActionItem item)
        {
            // 获取所有 RevoluteJoint（使用统一的方法确保顺序一致）
            List<ArticulationBody> revoluteJoints = GetRevoluteJoints();
            
            if (revoluteJoints.Count == 0)
            {
                Debug.LogWarning("ActionSequence: No revolute joints found");
                yield break;
            }
            
            int jointCount = Mathf.Min(item.jointAngles.Count, revoluteJoints.Count);
            
            // 获取当前关节角度作为起始值
            List<float> startAngles = new List<float>();
            List<float> targetAngles = new List<float>();
            for (int i = 0; i < jointCount; i++)
            {
                float currentAngle = 0f;
                if (revoluteJoints[i].jointPosition.dofCount > 0)
                {
                    currentAngle = revoluteJoints[i].jointPosition[0] * Mathf.Rad2Deg;
                }
                startAngles.Add(currentAngle);
                targetAngles.Add(item.jointAngles[i]);
            }
            
            // 平滑过渡
            float elapsedTime = 0f;
            while (elapsedTime < runDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / runDuration);
                
                // 使用平滑插值函数（ease in-out）
                float smoothT = t * t * (3f - 2f * t);
                
                for (int i = 0; i < jointCount; i++)
                {
                    float currentAngle = Mathf.Lerp(startAngles[i], targetAngles[i], smoothT);
                    SetJointTargetDeg(revoluteJoints[i], currentAngle);
                    UpdateSliderForJoint(revoluteJoints[i], currentAngle, i);
                }
                
                yield return null;
            }
            
            // 确保最终值准确
            for (int i = 0; i < jointCount; i++)
            {
                SetJointTargetDeg(revoluteJoints[i], targetAngles[i]);
                UpdateSliderForJoint(revoluteJoints[i], targetAngles[i], i);
            }
            
            if (item.jointAngles.Count != revoluteJoints.Count)
            {
                Debug.LogWarning($"ActionSequence: Joint angles count ({item.jointAngles.Count}) doesn't match robot joints count ({revoluteJoints.Count})");
            }
            
            Debug.Log($"ActionSequence: Applied {jointCount} joint angles to robot");
        }
        
        /// <summary>
        /// 执行 IK 移动的协程
        /// </summary>
        IEnumerator RunIKMoveCoroutine(IKMoveCommand ikCommand)
        {
            if (GewuIK.tar == null || GewuIK.tip == null)
            {
                Debug.LogWarning("ActionSequence: GewuIK tar or tip is null");
                yield break;
            }
            
            // 启用 IK
            GewuIK.SetIKEnabled(true);
            
            // 等待一帧确保 IK 已启用
            yield return null;
            
            // 获取起始位置和目标位置
            Vector3 startPosition = GewuIK.tar.position;
            Vector3 targetPosition = startPosition + ikCommand.moveDelta;
            
            // 平滑移动
            float elapsedTime = 0f;
            while (elapsedTime < ikDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / ikDuration);
                
                // 使用平滑插值函数（ease in-out）
                float smoothT = t * t * (3f - 2f * t);
                
                Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, smoothT);
                
                // 移动 tar
                Rigidbody rb = GewuIK.tar.GetComponent<Rigidbody>();
                ArticulationBody ab = GewuIK.tar.GetComponent<ArticulationBody>();
                
                if (rb != null)
                {
                    rb.MovePosition(currentPosition);
                }
                else if (ab != null)
                {
                    GewuIK.tar.position = currentPosition;
                }
                else
                {
                    GewuIK.tar.position = currentPosition;
                }
                
                yield return null;
            }
            
            // 确保最终位置准确
            Rigidbody finalRb = GewuIK.tar.GetComponent<Rigidbody>();
            ArticulationBody finalAb = GewuIK.tar.GetComponent<ArticulationBody>();
            
            if (finalRb != null)
            {
                finalRb.MovePosition(targetPosition);
            }
            else
            {
                GewuIK.tar.position = targetPosition;
            }
            
            Debug.Log($"ActionSequence: Moved IK target by {ikCommand.moveDelta}");
            
            // 执行完 IK 动作后，自动禁用 IK
            GewuIK.SetIKEnabled(false);
        }
        
        /// <summary>
        /// 解析 Gripper 命令（grasp 或 open）
        /// </summary>
        public string ParseGripperCommand(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }
            
            string trimmedText = text.Trim().ToLower();
            if (trimmedText == "grasp" || trimmedText == "open")
            {
                return trimmedText;
            }
            
            return "";
        }
        
        /// <summary>
        /// 执行 Gripper 命令的协程
        /// </summary>
        IEnumerator RunGripperCommandCoroutine(string command)
        {
            if (Gripper == null)
            {
                Debug.LogWarning("ActionSequence: Gripper is null");
                yield break;
            }
            
            if (command == "grasp")
            {
                Gripper.SetGripperPosition(Gripper.graspAngle, Gripper.graspAngle);
                Debug.Log("ActionSequence: Executed gripper grasp");
            }
            else if (command == "open")
            {
                Gripper.SetGripperPosition(Gripper.openAngle, Gripper.openAngle);
                Debug.Log("ActionSequence: Executed gripper open");
            }
            
            // 等待一帧确保命令执行
            yield return null;
        }
        
        /// <summary>
        /// 解析 IK 命令（格式：x+0.15, y-0.1, z+0.2 等）
        /// </summary>
        public IKMoveCommand ParseIKCommand(string text)
        {
            IKMoveCommand command = new IKMoveCommand();
            
            if (string.IsNullOrWhiteSpace(text))
            {
                return command;
            }
            
            // 移除空格并转换为小写
            text = text.Replace(" ", "").ToLower();
            
            // 检查是否包含 IK 命令格式（包含 x/y/z 和 +/-）
            bool hasIKFormat = false;
            Vector3 moveDelta = Vector3.zero;
            
            // 匹配模式：x+0.15, x-0.1, y+0.2, z-0.05 等
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"([xyz])([+-])([\d.]+)");
            var matches = regex.Matches(text);
            
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                hasIKFormat = true;
                string axis = match.Groups[1].Value;
                string sign = match.Groups[2].Value;
                string valueStr = match.Groups[3].Value;
                
                if (float.TryParse(valueStr, out float value))
                {
                    if (sign == "-")
                    {
                        value = -value;
                    }
                    
                    switch (axis)
                    {
                        case "x":
                            moveDelta.x = value;
                            break;
                        case "y":
                            moveDelta.y = value;
                            break;
                        case "z":
                            moveDelta.z = value;
                            break;
                    }
                }
            }
            
            if (hasIKFormat && moveDelta != Vector3.zero)
            {
                command.moveDelta = moveDelta;
                command.isValid = true;
            }
            
            return command;
        }
        
        /// <summary>
        /// 更新对应关节的 slider 值
        /// </summary>
        void UpdateSliderForJoint(ArticulationBody joint, float angleDeg, int jointIndex)
        {
            if (GewuIK == null || GewuIK.sliderParent == null) return;
            
            // 查找对应的 slider（slider 名称格式：Slider_Joint_1, Slider_Joint_2, ...，索引从1开始）
            string sliderName = $"Slider_Joint_{jointIndex + 1}";
            Transform sliderTransform = GewuIK.sliderParent.Find(sliderName);
            if (sliderTransform == null)
            {
                Debug.LogWarning($"ActionSequence: Slider {sliderName} not found for joint {joint.gameObject.name}");
                return;
            }
            
            UnityEngine.UI.Slider slider = sliderTransform.GetComponent<UnityEngine.UI.Slider>();
            if (slider == null) return;
            
            // 获取关节的上下限
            var drive = joint.xDrive;
            float lowerLimit = drive.lowerLimit;
            float upperLimit = drive.upperLimit;
            
            // 计算 slider 值
            float sliderValue = 0.5f;
            if (Mathf.Abs(upperLimit - lowerLimit) > 0.001f)
            {
                sliderValue = (angleDeg - lowerLimit) / (upperLimit - lowerLimit);
                sliderValue = Mathf.Clamp01(sliderValue);
            }
            
            // 更新 slider 值
            slider.value = sliderValue;
            
            // 更新角度值文本显示
            Transform angleValueTransform = sliderTransform.Find("AngleValue");
            if (angleValueTransform != null)
            {
                UnityEngine.UI.Text angleValueText = angleValueTransform.GetComponent<UnityEngine.UI.Text>();
                if (angleValueText != null)
                {
                    angleValueText.text = $"{angleDeg:F1}°";
                }
            }
        }
        
        /// <summary>
        /// 设置关节目标角度（度）
        /// </summary>
        void SetJointTargetDeg(ArticulationBody joint, float angleDeg)
        {
            var drive = joint.xDrive;
            drive.stiffness = GewuIK != null ? GewuIK.stiffness : 2000f;
            drive.damping = GewuIK != null ? GewuIK.damping : 200f;
            drive.target = angleDeg;
            joint.xDrive = drive;
        }

        public void AddAction()
        {
            actions.Add(new ActionItem());
        }

        public void RemoveAction(int index)
        {
            if (index >= 0 && index < actions.Count)
            {
                actions.RemoveAt(index);
            }
        }

        /// <summary>
        /// 获取所有 RevoluteJoint（按层级顺序）
        /// </summary>
        List<ArticulationBody> GetRevoluteJoints()
        {
            List<ArticulationBody> joints = new List<ArticulationBody>();
            
            if (GewuIK == null || GewuIK.robot == null)
            {
                return joints;
            }
            
            // 查找所有 RevoluteJoint，按层级顺序
            ArticulationBody[] allBodies = GewuIK.robot.GetComponentsInChildren<ArticulationBody>(true);
            foreach (ArticulationBody body in allBodies)
            {
                if (body.jointType == ArticulationJointType.RevoluteJoint)
                {
                    joints.Add(body);
                }
            }
            
            return joints;
        }
        
        /// <summary>
        /// 从 GewuIK 机器人获取所有关节角
        /// </summary>
        public List<float> GetJointAnglesFromRobot()
        {
            List<float> angles = new List<float>();
            
            List<ArticulationBody> revoluteJoints = GetRevoluteJoints();
            foreach (ArticulationBody joint in revoluteJoints)
            {
                float angleDeg = 0f;
                if (joint.jointPosition.dofCount > 0)
                {
                    angleDeg = joint.jointPosition[0] * Mathf.Rad2Deg;
                }
                angles.Add(angleDeg);
            }
            
            return angles;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Gewu.Flower.ActionSequence))]
public class ActionSequenceEditor : Editor
{
    // 用于存储运行时修改的值，key: objectPath_actionIndex, value: 关节角字符串
    private static Dictionary<string, string> runtimeValues = new Dictionary<string, string>();
    
    static ActionSequenceEditor()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // 在退出 Play Mode 之前，保存所有运行时修改的值到 EditorPrefs
            SaveRuntimeValuesToEditorPrefs();
            // 保存运行时添加的 actions
            SaveRuntimeActions();
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // 进入 Edit Mode 后，从 EditorPrefs 恢复保存的值
            RestoreRuntimeValuesFromEditorPrefs();
            // 恢复运行时添加的 actions
            RestoreRuntimeActions();
        }
        else if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // 进入 Play Mode 时，保存当前的 rawText 到 runtimeValues
            SaveRawTextToRuntimeValues();
            // 保存当前的 actions 数量
            SaveActionsCount();
        }
    }
    
    static void SaveActionsCount()
    {
        var sequences = FindObjectsOfType<Gewu.Flower.ActionSequence>();
        foreach (var seq in sequences)
        {
            if (seq == null) continue;
            
            string objectPath = GetObjectPath(seq.gameObject);
            EditorPrefs.SetInt($"ActionSequence_Count_{objectPath}", seq.actions.Count);
        }
    }
    
    static void SaveRuntimeActions()
    {
        var sequences = FindObjectsOfType<Gewu.Flower.ActionSequence>();
        foreach (var seq in sequences)
        {
            if (seq == null) continue;
            
            string objectPath = GetObjectPath(seq.gameObject);
            int actionCount = seq.actions.Count;
            
            // 保存 action 数量
            EditorPrefs.SetInt($"ActionSequence_Runtime_Count_{objectPath}", actionCount);
            
            // 保存每个 action 的内容
            for (int i = 0; i < actionCount; i++)
            {
                string key = $"{objectPath}_action_{i}";
                
                // 保存 jointAngles
                string anglesText = string.Join(",", seq.actions[i].jointAngles.ConvertAll(a => a.ToString("F2")));
                EditorPrefs.SetString($"ActionSequence_Runtime_{key}_angles", anglesText);
                
                // 保存 rawText
                EditorPrefs.SetString($"ActionSequence_Runtime_{key}_rawText", seq.actions[i].rawText ?? "");
            }
        }
    }
    
    static void RestoreRuntimeActions()
    {
        var sequences = FindObjectsOfType<Gewu.Flower.ActionSequence>();
        foreach (var seq in sequences)
        {
            if (seq == null) continue;
            
            string objectPath = GetObjectPath(seq.gameObject);
            string countKey = $"ActionSequence_Runtime_Count_{objectPath}";
            
            if (EditorPrefs.HasKey(countKey))
            {
                int savedCount = EditorPrefs.GetInt(countKey);
                int currentCount = seq.actions.Count;
                
                // 如果保存的数量与当前数量不同，说明运行时添加或删除了 action
                // 恢复保存的 actions（包括删除后的状态）
                if (savedCount != currentCount)
                {
                    // 恢复所有保存的 actions
                    seq.actions.Clear();
                    for (int i = 0; i < savedCount; i++)
                    {
                        string key = $"{objectPath}_action_{i}";
                        
                        var actionItem = new Gewu.Flower.ActionSequence.ActionItem();
                        
                        // 恢复 jointAngles
                        string anglesKey = $"ActionSequence_Runtime_{key}_angles";
                        if (EditorPrefs.HasKey(anglesKey))
                        {
                            string anglesText = EditorPrefs.GetString(anglesKey);
                            if (!string.IsNullOrEmpty(anglesText))
                            {
                                string[] parts = anglesText.Split(',');
                                foreach (var part in parts)
                                {
                                    if (float.TryParse(part.Trim(), out float value))
                                    {
                                        actionItem.jointAngles.Add(value);
                                    }
                                }
                            }
                            EditorPrefs.DeleteKey(anglesKey);
                        }
                        
                        // 恢复 rawText
                        string rawTextKey = $"ActionSequence_Runtime_{key}_rawText";
                        if (EditorPrefs.HasKey(rawTextKey))
                        {
                            actionItem.rawText = EditorPrefs.GetString(rawTextKey);
                            EditorPrefs.DeleteKey(rawTextKey);
                        }
                        
                        seq.actions.Add(actionItem);
                    }
                    
                    EditorUtility.SetDirty(seq);
                    var prefabInstance = PrefabUtility.GetPrefabInstanceHandle(seq);
                    if (prefabInstance != null)
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(seq);
                    }
                }
                
                EditorPrefs.DeleteKey(countKey);
            }
        }
    }
    
    static void SaveRawTextToRuntimeValues()
    {
        var sequences = FindObjectsOfType<Gewu.Flower.ActionSequence>();
        foreach (var seq in sequences)
        {
            if (seq == null) continue;
            
            string objectPath = GetObjectPath(seq.gameObject);
            for (int i = 0; i < seq.actions.Count; i++)
            {
                if (!string.IsNullOrEmpty(seq.actions[i].rawText))
                {
                    string key = $"{objectPath}_{i}";
                    runtimeValues[key] = seq.actions[i].rawText;
                }
            }
        }
    }
    
    static void SaveRuntimeValuesToEditorPrefs()
    {
        if (runtimeValues.Count == 0) return;
        
        int index = 0;
        foreach (var kvp in runtimeValues)
        {
            EditorPrefs.SetString($"ActionSequence_Runtime_{kvp.Key}", kvp.Value);
            index++;
        }
        EditorPrefs.SetInt("ActionSequence_Runtime_Count", runtimeValues.Count);
        runtimeValues.Clear();
    }
    
    static void RestoreRuntimeValuesFromEditorPrefs()
    {
        int count = EditorPrefs.GetInt("ActionSequence_Runtime_Count", 0);
        if (count == 0) return;
        
        var sequences = FindObjectsOfType<Gewu.Flower.ActionSequence>();
        foreach (var seq in sequences)
        {
            if (seq == null) continue;
            
            string objectPath = GetObjectPath(seq.gameObject);
            for (int i = 0; i < seq.actions.Count; i++)
            {
                string key = $"{objectPath}_{i}";
                string prefKey = $"ActionSequence_Runtime_{key}";
                if (EditorPrefs.HasKey(prefKey))
                {
                    string savedText = EditorPrefs.GetString(prefKey);
                    
                    // 恢复 rawText
                    seq.actions[i].rawText = savedText;
                    
                    // 检查是否是 Gripper、IK 或关节角格式
                    string gripperCmd = seq.ParseGripperCommand(savedText);
                    var ikCommand = seq.ParseIKCommand(savedText);
                    if (string.IsNullOrEmpty(gripperCmd) && !ikCommand.isValid)
                    {
                        // 不是 Gripper 或 IK 格式，解析为关节角
                        List<float> angles = ParseAnglesFromText(savedText);
                        seq.actions[i].jointAngles = angles;
                    }
                    // 如果是 Gripper 或 IK 格式，jointAngles 保持为空列表即可
                    
                    EditorPrefs.DeleteKey(prefKey);
                }
            }
            
            EditorUtility.SetDirty(seq);
            var prefabInstance = PrefabUtility.GetPrefabInstanceHandle(seq);
            if (prefabInstance != null)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(seq);
            }
        }
        
        EditorPrefs.DeleteKey("ActionSequence_Runtime_Count");
    }
    
    static string GetObjectPath(GameObject obj)
    {
        if (obj == null) return "";
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
    
    static List<float> ParseAnglesFromText(string text)
    {
        List<float> angles = new List<float>();
        if (string.IsNullOrWhiteSpace(text)) return angles;
        
        string[] parts = text.Split(',');
        foreach (var part in parts)
        {
            if (float.TryParse(part.Trim(), out float value))
            {
                angles.Add(value);
            }
        }
        return angles;
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("GewuIK"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("Gripper"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("actionInterval"));

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Action", GUILayout.Height(24)))
        {
            foreach (var targetObj in targets)
            {
                var seq = targetObj as Gewu.Flower.ActionSequence;
                Undo.RecordObject(seq, "Add Action");
                seq.AddAction();
                EditorUtility.SetDirty(seq);
                
                // 如果是 Prefab 实例，记录属性修改
                var prefabInstance = PrefabUtility.GetPrefabInstanceHandle(seq);
                if (prefabInstance != null)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(seq);
                }
            }
        }
        if (GUILayout.Button("Run All", GUILayout.Height(24)))
        {
            foreach (var targetObj in targets)
            {
                var seq = targetObj as Gewu.Flower.ActionSequence;
                if (seq == null) continue;
                if (Application.isPlaying)
                {
                    seq.RunAllActions();
                }
                else
                {
                    Debug.LogWarning("Run All requires Play Mode.");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        DrawActionsWithRunButtons();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawActionsWithRunButtons()
    {
        var actionsProp = serializedObject.FindProperty("actions");
        if (actionsProp == null)
        {
            EditorGUILayout.HelpBox("actions property not found.", MessageType.Warning);
            return;
        }

        for (int i = 0; i < actionsProp.arraySize; i++)
        {
            var element = actionsProp.GetArrayElementAtIndex(i);
            var jointAnglesProp = element.FindPropertyRelative("jointAngles");
            
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            
            // Get 按钮（放在最左边）
            if (GUILayout.Button("Get", GUILayout.Width(50)))
            {
                foreach (var targetObj in targets)
                {
                    var actionSeq = targetObj as Gewu.Flower.ActionSequence;
                    if (actionSeq == null || actionSeq.GewuIK == null) continue;
                    
                    Undo.RecordObject(actionSeq, "Get Joint Angles");
                    List<float> angles = actionSeq.GetJointAnglesFromRobot();
                    
                    // 直接修改对象的序列化字段
                    if (i >= 0 && i < actionSeq.actions.Count)
                    {
                        actionSeq.actions[i].jointAngles = new List<float>(angles);
                        
                        // 更新 rawText
                        string anglesTextForStorage = string.Join(", ", angles.ConvertAll(a => a.ToString("F2")));
                        actionSeq.actions[i].rawText = anglesTextForStorage;
                        
                        // 如果在运行时，保存值到临时存储（使用对象路径作为 key）
                        if (Application.isPlaying)
                        {
                            string objectPath = GetObjectPath(actionSeq.gameObject);
                            string key = $"{objectPath}_{i}";
                            runtimeValues[key] = anglesTextForStorage;
                        }
                    }
                    
                    // 标记对象为已修改
                    EditorUtility.SetDirty(actionSeq);
                    
                    // 如果是 Prefab 实例，记录属性修改
                    var prefabInstance = PrefabUtility.GetPrefabInstanceHandle(actionSeq);
                    if (prefabInstance != null)
                    {
                        PrefabUtility.RecordPrefabInstancePropertyModifications(actionSeq);
                    }
                }
                // 刷新 serializedObject 以反映直接修改的值
                serializedObject.Update();
            }
            
            // 向上移动按钮
            EditorGUI.BeginDisabledGroup(i == 0);
            if (GUILayout.Button("↑", GUILayout.Width(25)))
            {
                Undo.RecordObject(serializedObject.targetObject, "Move Action Up");
                actionsProp.MoveArrayElement(i, i - 1);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            EditorGUI.EndDisabledGroup();
            
            // 向下移动按钮
            EditorGUI.BeginDisabledGroup(i == actionsProp.arraySize - 1);
            if (GUILayout.Button("↓", GUILayout.Width(25)))
            {
                Undo.RecordObject(serializedObject.targetObject, "Move Action Down");
                actionsProp.MoveArrayElement(i, i + 1);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            EditorGUI.EndDisabledGroup();
            
            // 删除按钮
            if (GUILayout.Button("-", GUILayout.Width(30)))
            {
                Undo.RecordObject(serializedObject.targetObject, "Remove Action");
                actionsProp.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
                
                // 在运行时，确保 actions 的变化被保存
                if (Application.isPlaying)
                {
                    var actionSeqForDelete = serializedObject.targetObject as Gewu.Flower.ActionSequence;
                    if (actionSeqForDelete != null)
                    {
                        EditorUtility.SetDirty(actionSeqForDelete);
                        // 更新保存的 action 数量
                        string objectPath = GetObjectPath(actionSeqForDelete.gameObject);
                        EditorPrefs.SetInt($"ActionSequence_Runtime_Count_{objectPath}", actionSeqForDelete.actions.Count);
                    }
                }
                
                return;
            }
            
            // Run 按钮
            if (GUILayout.Button("Run", GUILayout.Width(60)))
            {
                foreach (var targetObj in targets)
                {
                    var actionSeq = targetObj as Gewu.Flower.ActionSequence;
                    if (actionSeq == null) continue;
                    if (Application.isPlaying)
                    {
                        actionSeq.RunAction(i);
                    }
                    else
                    {
                        Debug.LogWarning("Run action requires Play Mode.");
                    }
                }
            }
            
            EditorGUILayout.LabelField($"Action {i + 1}", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            string anglesText = GetJointAnglesAsText(jointAnglesProp);
            
            // 获取或设置原始文本（用于 IK 命令解析）
            var seq = serializedObject.targetObject as Gewu.Flower.ActionSequence;
            string displayText = anglesText;
            
            if (seq != null && i < seq.actions.Count)
            {
                // 如果 rawText 为空，只有在 anglesText 不为空时才设置（避免用空字符串覆盖）
                if (string.IsNullOrEmpty(seq.actions[i].rawText))
                {
                    if (!string.IsNullOrEmpty(anglesText))
                    {
                        seq.actions[i].rawText = anglesText;
                    }
                }
                // 如果 rawText 不为空
                else
                {
                    // 检查是否是 Gripper 或 IK 格式
                    string gripperCmd = seq.ParseGripperCommand(seq.actions[i].rawText);
                    var testCommand = seq.ParseIKCommand(seq.actions[i].rawText);
                    // 如果是 Gripper 或 IK 格式，保持 rawText 不变，永远不覆盖
                    if (!string.IsNullOrEmpty(gripperCmd) || testCommand.isValid)
                    {
                        // Gripper 或 IK 格式，保持 rawText 不变
                    }
                    // 如果不是特殊格式，且 anglesText 不为空，才更新 rawText
                    else if (!string.IsNullOrEmpty(anglesText) && anglesText != seq.actions[i].rawText)
                    {
                        seq.actions[i].rawText = anglesText;
                    }
                }
                
                // 优先显示 rawText，如果 rawText 为空，则显示 anglesText
                displayText = !string.IsNullOrEmpty(seq.actions[i].rawText) ? seq.actions[i].rawText : anglesText;
            }
            string newAnglesText = EditorGUILayout.TextField(displayText);
            if (newAnglesText != displayText)
            {
                // 检查是否是 Gripper、IK 或关节角格式
                if (seq != null && i < seq.actions.Count)
                {
                    string gripperCmd = seq.ParseGripperCommand(newAnglesText);
                    var testCommand = seq.ParseIKCommand(newAnglesText);
                    
                    if (!string.IsNullOrEmpty(gripperCmd) || testCommand.isValid)
                    {
                        // Gripper 或 IK 格式，只更新 rawText，不解析为关节角
                        seq.actions[i].rawText = newAnglesText;
                    }
                    else
                    {
                        // 关节角格式，解析并更新
                        SetJointAnglesFromText(jointAnglesProp, newAnglesText);
                        seq.actions[i].rawText = newAnglesText;
                    }
                    
                    // 在运行时，始终保存到 runtimeValues（因为 rawText 是 NonSerialized，会被重置）
                    if (Application.isPlaying)
                    {
                        string objectPath = GetObjectPath(seq.gameObject);
                        string key = $"{objectPath}_{i}";
                        runtimeValues[key] = newAnglesText;
                    }
                    
                    EditorUtility.SetDirty(seq);
                }
                else
                {
                    SetJointAnglesFromText(jointAnglesProp, newAnglesText);
                }
            }

            EditorGUILayout.EndVertical();
        }
    }
    
    private string GetJointAnglesAsText(SerializedProperty jointAnglesProp)
    {
        if (jointAnglesProp == null || !jointAnglesProp.isArray) return "";
        var parts = new System.Collections.Generic.List<string>();
        for (int i = 0; i < jointAnglesProp.arraySize; i++)
        {
            parts.Add(jointAnglesProp.GetArrayElementAtIndex(i).floatValue.ToString("F2"));
        }
        return string.Join(", ", parts);
    }
    
    private void SetJointAnglesFromText(SerializedProperty jointAnglesProp, string text)
    {
        if (jointAnglesProp == null || !jointAnglesProp.isArray) return;
        
        jointAnglesProp.ClearArray();
        if (string.IsNullOrWhiteSpace(text)) return;
        
        string[] parts = text.Split(',');
        foreach (var part in parts)
        {
            if (float.TryParse(part.Trim(), out float value))
            {
                jointAnglesProp.arraySize++;
                jointAnglesProp.GetArrayElementAtIndex(jointAnglesProp.arraySize - 1).floatValue = value;
            }
        }
    }
}
#endif

