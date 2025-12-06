using UnityEditor;
using UnityEngine;
using Gewu;

namespace Gewu.Editor
{
    [CustomEditor(typeof(JointSetup))]
    public class JointSetupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            JointSetup controller = (JointSetup)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Joint Setup", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Camera view switching buttons
            EditorGUILayout.LabelField("Camera Views", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Left View", GUILayout.Height(25)))
            {
                controller.SwitchToLeftView();
            }
            if (GUILayout.Button("Front View", GUILayout.Height(25)))
            {
                controller.SwitchToFrontView();
            }
            if (GUILayout.Button("Top View", GUILayout.Height(25)))
            {
                controller.SwitchToTopView();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // Refresh button
            if (GUILayout.Button("Refresh List", GUILayout.Height(30)))
            {
                controller.RefreshArticulationBodies();
                controller.InitializeJointAnglesList();
                EditorUtility.SetDirty(controller);
            }

            EditorGUILayout.Space();
            
            // Statistics - Get revolute joints for both statistics and joint angles section
            var revoluteJoints = controller.GetRevoluteJoints();
            int revoluteCount = revoluteJoints.Count;
            
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"Total: {controller.articulationBodies.Count}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Revolute: {revoluteCount}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();

            // 显示列表
            if (controller.articulationBodies.Count > 0)
            {
                EditorGUILayout.BeginVertical("box");
                
                // Batch operation buttons
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set All Fixed", GUILayout.Height(25)))
                {
                    foreach (var info in controller.articulationBodies)
                    {
                        info.isFixed = true;
                        // Apply immediately
                        if (info.articulationBody != null)
                        {
                            info.articulationBody.jointType = ArticulationJointType.FixedJoint;
                        }
                    }
                    EditorUtility.SetDirty(controller);
                    // 更新 Behavior Parameters
                    UpdateBehaviorParameters(controller);
                }
                if (GUILayout.Button("Set All Revolute", GUILayout.Height(25)))
                {
                    foreach (var info in controller.articulationBodies)
                    {
                        info.isFixed = false;
                        // Apply immediately
                        if (info.articulationBody != null)
                        {
                            info.articulationBody.jointType = ArticulationJointType.RevoluteJoint;
                        }
                    }
                    EditorUtility.SetDirty(controller);
                    // 更新 Behavior Parameters
                    UpdateBehaviorParameters(controller);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                // 列表项
                int revoluteIndex = 0; // 用于未打钩关节的序号计数
                for (int i = 0; i < controller.articulationBodies.Count; i++)
                {
                    var info = controller.articulationBodies[i];
                    
                    if (info.articulationBody == null)
                    {
                        EditorGUILayout.HelpBox($"Index {i}: Object has been deleted", MessageType.Warning);
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal("box");
                    
                    // 如果是未打钩的关节（Revolute），显示序号
                    if (!info.isFixed)
                    {
                        revoluteIndex++;
                        EditorGUILayout.LabelField($"{revoluteIndex}.", GUILayout.Width(30));
                    }
                    else
                    {
                        EditorGUILayout.LabelField("", GUILayout.Width(30)); // 占位，保持对齐
                    }
                    
                    // Fixed/Revolute checkbox
                    bool newValue = EditorGUILayout.Toggle(info.isFixed, GUILayout.Width(20));
                    if (newValue != info.isFixed)
                    {
                        info.isFixed = newValue;
                        // Apply immediately
                        if (info.articulationBody != null)
                        {
                            info.articulationBody.jointType = info.isFixed 
                                ? ArticulationJointType.FixedJoint 
                                : ArticulationJointType.RevoluteJoint;
                        }
                        EditorUtility.SetDirty(controller);
                        // 更新 Behavior Parameters
                        UpdateBehaviorParameters(controller);
                    }

                    // Object name
                    EditorGUILayout.LabelField(info.name, GUILayout.Width(150));

                    // Object reference
                    EditorGUILayout.ObjectField(info.articulationBody, typeof(ArticulationBody), true);

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Checked = FixedJoint, Unchecked = RevoluteJoint. Changes are applied automatically.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("No ArticulationBody components found. Please click Refresh List button.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Joint Initial Angles Section
            EditorGUILayout.LabelField("Joint Initial Angles", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal("box");
            EditorGUILayout.LabelField($"Revolute Joints: {revoluteCount}", EditorStyles.boldLabel);
            if (GUILayout.Button("Refresh Angles", GUILayout.Width(120)))
            {
                controller.InitializeJointAnglesList();
                EditorUtility.SetDirty(controller);
            }
            EditorGUILayout.EndHorizontal();
            
            if (revoluteCount > 0)
            {
                if (controller.initialJointAngles == null || controller.initialJointAngles.Count != revoluteCount)
                {
                    EditorGUILayout.HelpBox($"Joint angles list count ({controller.initialJointAngles?.Count ?? 0}) doesn't match revolute joints count ({revoluteCount}). Click 'Refresh Angles' to update.", MessageType.Warning);
                }
                else
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField("Initial Joint Angles (degrees):", EditorStyles.miniLabel);
                    
                    for (int i = 0; i < Mathf.Min(revoluteCount, controller.initialJointAngles.Count); i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        string jointName = revoluteJoints[i] != null ? revoluteJoints[i].gameObject.name : "Unknown";
                        EditorGUILayout.LabelField($"{i + 1}. {jointName}:", GUILayout.Width(200));
                        controller.initialJointAngles[i] = EditorGUILayout.FloatField(controller.initialJointAngles[i]);
                        EditorGUILayout.EndHorizontal();
                    }
                    
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No revolute joints found. Joint angles will be initialized when joints are available.", MessageType.Info);
            }
        }

        private void UpdateBehaviorParameters(JointSetup controller)
        {
            if (controller == null) return;

            GameObject robot = controller.gameObject;
            if (robot == null) return;

            // 调用 GewuUrdfImportMenu 中的 ConfigureBehaviorParameters 方法
            GewuUrdfImportMenu.ConfigureBehaviorParameters(robot, controller);
        }
    }
}

