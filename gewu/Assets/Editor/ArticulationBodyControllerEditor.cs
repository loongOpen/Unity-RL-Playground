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
                EditorUtility.SetDirty(controller);
            }

            EditorGUILayout.Space();
            
            // Statistics
            int revoluteCount = 0;
            foreach (var info in controller.articulationBodies)
            {
                if (!info.isFixed)
                    revoluteCount++;
            }
            
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
                for (int i = 0; i < controller.articulationBodies.Count; i++)
                {
                    var info = controller.articulationBodies[i];
                    
                    if (info.articulationBody == null)
                    {
                        EditorGUILayout.HelpBox($"Index {i}: Object has been deleted", MessageType.Warning);
                        continue;
                    }

                    EditorGUILayout.BeginHorizontal("box");
                    
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

                    // Feedforward checkbox (放在文字后面)
                    bool newFeedforward = EditorGUILayout.Toggle(info.feedforward, GUILayout.Width(20));
                    if (newFeedforward != info.feedforward)
                    {
                        info.feedforward = newFeedforward;
                        EditorUtility.SetDirty(controller);
                    }
                    EditorGUILayout.LabelField("FF", GUILayout.Width(30));

                    // Revert checkbox (放在文字后面)
                    bool newRevert = EditorGUILayout.Toggle(info.revert, GUILayout.Width(20));
                    if (newRevert != info.revert)
                    {
                        info.revert = newRevert;
                        EditorUtility.SetDirty(controller);
                    }
                    EditorGUILayout.LabelField("Revert", GUILayout.Width(50));

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

