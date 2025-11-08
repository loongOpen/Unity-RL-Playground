using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Unity.Robotics.UrdfImporter.Editor;
using Unity.Robotics.UrdfImporter;
using Gewu;

namespace Gewu.Editor
{
    public class GewuUrdfImportMenu : EditorWindow
    {
        public string urdfFile;
        public ImportSettings settings = new ImportSettings();
        private bool showLoadBar = false;
        private GameObject importedRobot = null;
        private bool importCompleted = false;

        private void Awake()
        {
            this.titleContent = new GUIContent("URDF Import Settings");
        }

        private void OnGUI()
        {
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 13
            };

            GUILayout.Space(10);
            GUILayout.Label("Select Axis Type", titleStyle);

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            settings.chosenAxis = (ImportSettings.axisType)EditorGUILayout.EnumPopup(
                "Select Axis Type", settings.chosenAxis);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.Label("Select Convex Decomposer", titleStyle);

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            settings.convexMethod = (ImportSettings.convexDecomposer)EditorGUILayout.EnumPopup(
                "Mesh Decomposer", settings.convexMethod);
            EditorGUILayout.EndHorizontal();

            GUILayout.Space(10);
            settings.OverwriteExistingPrefabs = GUILayout.Toggle(settings.OverwriteExistingPrefabs, "Overwrite Existing Prefabs");

            GUILayout.Space(10);
            if (GUILayout.Button("Import URDF"))
            {
                if (urdfFile != "")
                {
                    showLoadBar = true;
                    EditorCoroutineUtility.StartCoroutine(ImportUrdfCoroutine(), this);
                }
            }

            if (showLoadBar)
            {
                float progress = (settings.totalLinks == 0) ? 0 : ((float)settings.linksLoaded / (float)settings.totalLinks);
                EditorGUI.ProgressBar(new Rect(3, 400, position.width - 6, 20), progress, 
                    string.Format("{0}/{1} Links Loaded", settings.linksLoaded, settings.totalLinks));
                if (progress == 1 && importCompleted)
                {
                    Close();
                }
            }
        }

        private IEnumerator ImportUrdfCoroutine()
        {
            IEnumerator<GameObject> createRobot = UrdfRobotExtensions.Create(urdfFile, settings, true);
            while (createRobot.MoveNext())
            {
                importedRobot = createRobot.Current;
                yield return null;
            }
            importedRobot = createRobot.Current;
            
            // 等待一帧确保所有初始化完成
            yield return null;
            
            // 移除组件
            RemoveComponentsFromRobot();
            
            importCompleted = true;
        }

        private void RemoveComponentsFromRobot()
        {
            // 如果 importedRobot 为空，尝试从场景中查找最近创建的机器人对象
            if (importedRobot == null)
            {
                // 查找场景中所有带有 UrdfRobot 组件的对象
                UrdfRobot[] robots = Object.FindObjectsOfType<UrdfRobot>();
                if (robots.Length > 0)
                {
                    importedRobot = robots[robots.Length - 1].gameObject; // 获取最后一个（最可能是刚导入的）
                }
            }

            if (importedRobot != null)
            {
                // 移除 UrdfRobot 组件
                var urdfRobot = importedRobot.GetComponent<UrdfRobot>();
                if (urdfRobot != null)
                {
                    Object.DestroyImmediate(urdfRobot);
                }

                // 移除 Controller 组件
                var urdfController = importedRobot.GetComponent<Unity.Robotics.UrdfImporter.Control.Controller>();
                if (urdfController != null)
                {
                    Object.DestroyImmediate(urdfController);
                }

                // 移除所有 UrdfLink 组件（递归查找所有子对象）
                UrdfLink[] urdfLinks = importedRobot.GetComponentsInChildren<UrdfLink>();
                foreach (var urdfLink in urdfLinks)
                {
                    if (urdfLink != null)
                    {
                        Object.DestroyImmediate(urdfLink);
                    }
                }

                // 添加 JointSetup 脚本
                var articulationController = importedRobot.GetComponent<JointSetup>();
                if (articulationController == null)
                {
                    articulationController = importedRobot.AddComponent<JointSetup>();
                    articulationController.RefreshArticulationBodies();
                }

                // 自动调整高度，使模型刚好接触地面
                AdjustRobotHeightToGround(importedRobot);

                // 添加 GewunewAgent 脚本
                var existingGewuAgent = importedRobot.GetComponent("GewunewAgent");
                Component gewuAgent = null;
                
                if (existingGewuAgent == null)
                {
                    // 尝试从所有程序集中查找 GewunewAgent 类型
                    System.Type gewuAgentType = null;
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        gewuAgentType = assembly.GetType("GewunewAgent");
                        if (gewuAgentType != null) break;
                    }
                    
                    if (gewuAgentType != null)
                    {
                        gewuAgent = importedRobot.AddComponent(gewuAgentType);
                    }
                    else
                    {
                        Debug.LogWarning("GewunewAgent script not found. Please ensure GewunewAgent.cs exists in the project.");
                    }
                }
                else
                {
                    gewuAgent = existingGewuAgent as Component;
                }

                // 设置 GewunewAgent 的 Max Step 为 1000
                if (gewuAgent != null)
                {
                    // 使用 SerializedObject 设置 Max Step
                    SerializedObject agentSerialized = new SerializedObject(gewuAgent);
                    var maxStepProp = agentSerialized.FindProperty("MaxStep");
                    if (maxStepProp == null)
                    {
                        // 尝试其他可能的字段名
                        maxStepProp = agentSerialized.FindProperty("m_MaxStep");
                    }
                    if (maxStepProp != null)
                    {
                        if (maxStepProp.intValue != 1000)
                        {
                            maxStepProp.intValue = 1000;
                            agentSerialized.ApplyModifiedProperties();
                            EditorUtility.SetDirty(gewuAgent);
                        }
                    }
                    else
                    {
                        // 如果找不到属性，尝试直接设置字段
                        var agentType = gewuAgent.GetType();
                        var maxStepField = agentType.GetField("MaxStep");
                        if (maxStepField != null)
                        {
                            maxStepField.SetValue(gewuAgent, 1000);
                            EditorUtility.SetDirty(gewuAgent);
                        }
                    }
                }

                // 添加 Decision Requester 组件
                var existingDecisionRequester = importedRobot.GetComponent("DecisionRequester");
                Component decisionRequester = null;
                
                if (existingDecisionRequester == null)
                {
                    // 尝试从所有程序集中查找 DecisionRequester 类型
                    System.Type decisionRequesterType = null;
                    foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                    {
                        decisionRequesterType = assembly.GetType("Unity.MLAgents.DecisionRequester") ?? 
                                               assembly.GetType("DecisionRequester");
                        if (decisionRequesterType != null) break;
                    }
                    
                    if (decisionRequesterType != null)
                    {
                        decisionRequester = importedRobot.AddComponent(decisionRequesterType);
                    }
                    else
                    {
                        Debug.LogWarning("DecisionRequester component not found. Please ensure ML-Agents package is installed.");
                    }
                }
                else
                {
                    decisionRequester = existingDecisionRequester as Component;
                }

                // 设置 Decision Period 为 1
                if (decisionRequester != null)
                {
                    var decisionPeriodField = decisionRequester.GetType().GetField("DecisionPeriod");
                    if (decisionPeriodField != null)
                    {
                        decisionPeriodField.SetValue(decisionRequester, 1);
                    }
                    else
                    {
                        // 尝试使用属性
                        var decisionPeriodProperty = decisionRequester.GetType().GetProperty("DecisionPeriod");
                        if (decisionPeriodProperty != null && decisionPeriodProperty.CanWrite)
                        {
                            decisionPeriodProperty.SetValue(decisionRequester, 1);
                        }
                    }
                }

                // 配置 Behavior Parameters
                ConfigureBehaviorParameters(importedRobot, articulationController);

                // 设置物体及所有子节点的layer为6 (robot1)
                SetLayerRecursively(importedRobot, 6);

                // 切换到左视图
                EditorApplication.delayCall += () =>
                {
                    if (articulationController != null)
                    {
                        articulationController.SwitchToLeftView();
                    }
                };
            }
        }

        /// <summary>
        /// 递归设置物体及其所有子节点的layer
        /// </summary>
        private void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;
            
            obj.layer = layer;
            
            // 递归设置所有子节点
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        public static void ConfigureBehaviorParameters(GameObject robot, JointSetup controller)
        {
            if (robot == null) return;

            // 直接从 JointSetup 获取 Revolute Joint 的数量
            int revoluteJointCount = 0;
            if (controller != null)
            {
                revoluteJointCount = controller.GetRevoluteJointCount();
            }

            // 查找或添加 Behavior Parameters 组件
            var existingBehaviorParams = robot.GetComponent("BehaviorParameters");
            Component behaviorParams = null;

            if (existingBehaviorParams == null)
            {
                // 尝试从所有程序集中查找 BehaviorParameters 类型
                System.Type behaviorParamsType = null;
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    behaviorParamsType = assembly.GetType("Unity.MLAgents.BehaviorParameters") ?? 
                                        assembly.GetType("BehaviorParameters");
                    if (behaviorParamsType != null) break;
                }

                if (behaviorParamsType != null)
                {
                    behaviorParams = robot.AddComponent(behaviorParamsType);
                }
                else
                {
                    Debug.LogWarning("BehaviorParameters component not found. Please ensure ML-Agents package is installed.");
                    return;
                }
            }
            else
            {
                behaviorParams = existingBehaviorParams as Component;
            }

            if (behaviorParams != null)
            {
                // 使用 SerializedObject 来正确设置序列化字段
                SerializedObject serializedObject = new SerializedObject(behaviorParams);
                bool hasChanges = false;

                // 设置 Name 为 "gewu"
                var behaviorNameProp = serializedObject.FindProperty("m_BehaviorName");
                if (behaviorNameProp != null)
                {
                    behaviorNameProp.stringValue = "gewu";
                    hasChanges = true;
                }

                // 获取 BrainParameters
                var brainParamsProp = serializedObject.FindProperty("m_BrainParameters");
                if (brainParamsProp != null)
                {
                    // 设置 VectorObservationSize (Space Size)
                    int spaceSize = revoluteJointCount * 2 + 9;
                    var vectorObsSizeProp = brainParamsProp.FindPropertyRelative("VectorObservationSize");
                    if (vectorObsSizeProp != null)
                    {
                        if (vectorObsSizeProp.intValue != spaceSize)
                        {
                            vectorObsSizeProp.intValue = spaceSize;
                            hasChanges = true;
                        }
                    }

                    // 设置 ActionSpec
                    var actionSpecProp = brainParamsProp.FindPropertyRelative("m_ActionSpec");
                    if (actionSpecProp != null)
                    {
                        // 设置 NumContinuousActions (Continuous Action Size)
                        var numContinuousProp = actionSpecProp.FindPropertyRelative("m_NumContinuousActions");
                        if (numContinuousProp != null)
                        {
                            if (numContinuousProp.intValue != revoluteJointCount)
                            {
                                numContinuousProp.intValue = revoluteJointCount;
                                hasChanges = true;
                            }
                        }

                        // 设置 BranchSizes (Discrete Branches) 为空数组
                        var branchSizesProp = actionSpecProp.FindPropertyRelative("BranchSizes");
                        if (branchSizesProp != null)
                        {
                            if (branchSizesProp.arraySize != 0)
                            {
                                branchSizesProp.arraySize = 0;
                                hasChanges = true;
                            }
                        }
                    }
                }

                // 应用更改并刷新 Inspector
                if (hasChanges)
                {
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(behaviorParams);
                    EditorUtility.SetDirty(robot);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                }
            }
        }

        private void AdjustRobotHeightToGround(GameObject robot)
        {
            if (robot == null) return;

            // 计算所有 Renderer 的包围盒
            Renderer[] renderers = robot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;

            // 找到最低点
            float minY = float.MaxValue;
            foreach (Renderer renderer in renderers)
            {
                if (renderer.bounds.min.y < minY)
                {
                    minY = renderer.bounds.min.y;
                }
            }

            // 如果最低点不在 y=0，调整位置
            if (minY != 0f)
            {
                Vector3 currentPos = robot.transform.position;
                robot.transform.position = new Vector3(currentPos.x, currentPos.y - minY, currentPos.z);
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }

    public static class GewuMenu
    {

        [MenuItem("Gewu/Import URDF", false, 10)]
        public static void ImportURDF()
        {
            // 检查是否有选中的对象
            if (Selection.activeObject == null)
            {
                EditorUtility.DisplayDialog("URDF Import",
                    "Please select a URDF file in the Project window first.", "Ok");
                return;
            }

            // 获取选中对象的路径
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

            // 检查是否是 URDF 文件
            if (Path.GetExtension(assetPath)?.ToLower() == ".urdf")
            {
                // 打开自定义导入窗口
                GewuUrdfImportMenu window = (GewuUrdfImportMenu)EditorWindow.GetWindow(typeof(GewuUrdfImportMenu));
                window.urdfFile = UrdfAssetPathHandler.GetFullAssetPath(assetPath);
                // 设置默认的 Mesh Decomposer 为 Unity
                window.settings.convexMethod = ImportSettings.convexDecomposer.unity;
                window.minSize = new Vector2(500, 200);
                window.Show();
            }
            else
            {
                EditorUtility.DisplayDialog("URDF Import",
                    "The file you selected was not a URDF file. Please select a valid URDF file.", "Ok");
            }
        }

        [MenuItem("Gewu/Import URDF", true)]
        public static bool ImportURDF_IsValid()
        {
            // 检查是否有选中的对象
            if (Selection.activeObject == null)
                return false;

            // 检查选中的文件是否是 URDF 文件
            string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            return (Path.GetExtension(assetPath)?.ToLower() == ".urdf");
        }

        [MenuItem("Gewu/New Scene", false, 0)]
        public static void NewScene()
        {
            // 检查当前场景是否有未保存的更改
            if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().isDirty)
            {
                int result = EditorUtility.DisplayDialogComplex("New Scene", 
                    "Current scene has unsaved changes. Do you want to save before creating a new scene?", 
                    "Save", "Don't Save", "Cancel");
                
                if (result == 0) // Save
                {
                    UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                }
                else if (result == 2) // Cancel
                {
                    return; // 用户取消，退出
                }
                // result == 1 (Don't Save) 继续执行，不保存
            }

            // 创建新场景
            var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            // 设置相机位置和旋转
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(-2f, 0.8f, 0f);
                mainCamera.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }

            // 设置光照旋转
            Light[] lights = Object.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    light.transform.rotation = Quaternion.Euler(50f, 135f, 0f);
                    break; // 只设置第一个方向光
                }
            }

            // 从 Assets 加载 Ground Prefab
            GameObject groundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Gewumenu/Ground.prefab");
            GameObject ground = null;
            
            if (groundPrefab != null)
            {
                // 实例化 Prefab
                ground = (GameObject)PrefabUtility.InstantiatePrefab(groundPrefab);
                ground.name = "Ground";
            }
            else
            {
                // 如果找不到 Prefab，创建默认地面
                Debug.LogWarning("Ground.prefab not found at Assets/Ground.prefab. Creating default ground.");
                ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
                ground.name = "Ground";
                ground.transform.position = Vector3.zero;
                ground.transform.localScale = Vector3.one * 100f;
            }

            // 保存场景
            string scenePath = EditorUtility.SaveFilePanelInProject(
                "Save New Scene",
                "NewScene",
                "unity",
                "Please enter a file name to save the scene");

            if (!string.IsNullOrEmpty(scenePath))
            {
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
                AssetDatabase.Refresh();
            }

            // 选中地面对象
            Selection.activeGameObject = ground;
        }
    }
}

