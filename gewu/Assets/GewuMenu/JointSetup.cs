using System.Collections.Generic;
using UnityEngine;

namespace Gewu
{
    [AddComponentMenu("Gewu/Joint Setup")]
    public class JointSetup : MonoBehaviour
    {
        [System.Serializable]
        public class ArticulationBodyInfo
        {
            public ArticulationBody articulationBody;
            public bool isFixed;
            public bool revert;
            public bool feedforward;
            public string name;

            public ArticulationBodyInfo(ArticulationBody ab)
            {
                articulationBody = ab;
                isFixed = ab.jointType == ArticulationJointType.FixedJoint;
                name = ab.gameObject.name;
            }
        }

        public List<ArticulationBodyInfo> articulationBodies = new List<ArticulationBodyInfo>();
        
        [Header("Joint Initial Angles")]
        [Tooltip("初始关节角列表（度）。按找到的RevoluteJoint顺序对应。如果列表为空或数量不匹配，将使用当前关节角度作为初始值。")]
        public List<float> initialJointAngles = new List<float>();
        
        [Header("Joint Settings")]
        [Tooltip("关节刚度和阻尼（用于设置初始角度）")]
        public float stiffness = 2000f;
        public float damping = 200f;
        
        // 找到的旋转关节列表
        private List<ArticulationBody> revoluteJoints = new List<ArticulationBody>();

        private void OnEnable()
        {
            RefreshArticulationBodies();
        }
        
        void Start()
        {
            InitializeJointAngles();
        }

        public void RefreshArticulationBodies()
        {
            // 保存现有的 revert 和 feedforward 值
            Dictionary<ArticulationBody, (bool revert, bool feedforward)> savedValues = new Dictionary<ArticulationBody, (bool, bool)>();
            foreach (var info in articulationBodies)
            {
                if (info.articulationBody != null)
                {
                    savedValues[info.articulationBody] = (info.revert, info.feedforward);
                }
            }

            articulationBodies.Clear();
            ArticulationBody[] bodies = GetComponentsInChildren<ArticulationBody>();
            foreach (var body in bodies)
            {
                var newInfo = new ArticulationBodyInfo(body);
                // 恢复保存的 revert 和 feedforward 值
                if (savedValues.ContainsKey(body))
                {
                    newInfo.revert = savedValues[body].revert;
                    newInfo.feedforward = savedValues[body].feedforward;
                }
                articulationBodies.Add(newInfo);
            }
            
            // 刷新后自动初始化关节角列表
            InitializeJointAnglesList();
        }

        public void ApplyJointTypes()
        {
            foreach (var info in articulationBodies)
            {
                if (info.articulationBody != null)
                {
                    info.articulationBody.jointType = info.isFixed 
                        ? ArticulationJointType.FixedJoint 
                        : ArticulationJointType.RevoluteJoint;
                }
            }
        }

        public Vector3 GetCenterPosition()
        {
            if (articulationBodies.Count == 0) return transform.position;

            Vector3 center = Vector3.zero;
            int count = 0;
            foreach (var info in articulationBodies)
            {
                if (info.articulationBody != null)
                {
                    center += info.articulationBody.transform.position;
                    count++;
                }
            }
            return count > 0 ? center / count : transform.position;
        }

        public Bounds GetBounds()
        {
            Bounds bounds = new Bounds(GetCenterPosition(), Vector3.zero);
            bool hasBounds = false;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return bounds;
        }

        public int GetRevoluteJointCount()
        {
            int count = 0;
            if (articulationBodies != null)
            {
                foreach (var info in articulationBodies)
                {
                    if (!info.isFixed)
                    {
                        count++;
                    }
                }
            }
            return count;
        }

        public void SwitchToLeftView()
        {
            SwitchCameraView(ViewType.Left);
        }

        public void SwitchToFrontView()
        {
            SwitchCameraView(ViewType.Front);
        }

        public void SwitchToTopView()
        {
            SwitchCameraView(ViewType.Top);
        }

        private enum ViewType
        {
            Left,
            Front,
            Top
        }

        private void SwitchCameraView(ViewType viewType)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // 如果没有主相机，尝试查找场景中的第一个相机
                Camera[] cameras = FindObjectsOfType<Camera>();
                if (cameras.Length > 0)
                {
                    mainCamera = cameras[0];
                }
                else
                {
                    Debug.LogWarning("No camera found in scene. Cannot switch view.");
                    return;
                }
            }

            Bounds bounds = GetBounds();
            Vector3 center = bounds.center;
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            float distance = maxSize * 1.2f; // 相机距离（减小倍数使物体显示更大）

            Vector3 cameraPosition = Vector3.zero;
            Quaternion cameraRotation = Quaternion.identity;

            switch (viewType)
            {
                case ViewType.Left:
                    // 左视图：从左侧看（x 轴负方向），看向右侧（x 轴正方向）
                    cameraPosition = center + new Vector3(-distance, 0, 0);
                    cameraRotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
                    break;
                case ViewType.Front:
                    // 前视图：从前方看（z 轴正方向），看向后方（z 轴负方向）
                    cameraPosition = center + new Vector3(0, 0, distance);
                    cameraRotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
                    break;
                case ViewType.Top:
                    // 俯视图：从上方看（y 轴正方向），看向下方（y 轴负方向）
                    cameraPosition = center + new Vector3(0, distance, 0);
                    cameraRotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                    break;
            }

            mainCamera.transform.position = cameraPosition;
            mainCamera.transform.rotation = cameraRotation;
        }
        
        /// <summary>
        /// 获取所有RevoluteJoint
        /// </summary>
        void FindRevoluteJoints()
        {
            revoluteJoints.Clear();
            if (articulationBodies == null) return;
            
            foreach (var info in articulationBodies)
            {
                if (info.articulationBody != null && !info.isFixed)
                {
                    if (info.articulationBody.jointType == ArticulationJointType.RevoluteJoint)
                    {
                        revoluteJoints.Add(info.articulationBody);
                    }
                }
            }
        }
        
        /// <summary>
        /// 初始化关节角度列表（如果为空或数量不匹配）
        /// </summary>
        public void InitializeJointAnglesList()
        {
            FindRevoluteJoints();
            
            if (revoluteJoints.Count == 0)
            {
                initialJointAngles.Clear();
                return;
            }
            
            // 如果列表为空或数量不匹配，自动初始化
            if (initialJointAngles == null || initialJointAngles.Count != revoluteJoints.Count)
            {
                initialJointAngles = new List<float>();
                for (int i = 0; i < revoluteJoints.Count; i++)
                {
                    if (revoluteJoints[i] != null)
                    {
                        // 获取当前关节角度作为初始值
                        float currentAngle = 0f;
                        if (revoluteJoints[i].jointPosition.dofCount > 0)
                        {
                            currentAngle = revoluteJoints[i].jointPosition[0] * Mathf.Rad2Deg;
                        }
                        initialJointAngles.Add(currentAngle);
                    }
                }
                Debug.Log($"JointSetup: 已自动初始化 {initialJointAngles.Count} 个关节的初始角度");
            }
        }
        
        /// <summary>
        /// 初始化关节角度
        /// </summary>
        void InitializeJointAngles()
        {
            FindRevoluteJoints();
            
            if (revoluteJoints.Count == 0)
            {
                Debug.LogWarning("JointSetup: 未找到任何旋转关节，无法初始化关节角度");
                return;
            }
            
            // 如果初始角度列表为空或数量不匹配，自动初始化
            if (initialJointAngles == null || initialJointAngles.Count != revoluteJoints.Count)
            {
                InitializeJointAnglesList();
            }
            
            // 应用初始关节角度
            ApplyInitialJointAngles();
        }
        
        /// <summary>
        /// 应用初始关节角度
        /// </summary>
        void ApplyInitialJointAngles()
        {
            if (initialJointAngles == null || initialJointAngles.Count == 0)
            {
                Debug.LogWarning("JointSetup: 初始关节角度列表为空，无法设置初始角度");
                return;
            }
            
            for (int i = 0; i < Mathf.Min(initialJointAngles.Count, revoluteJoints.Count); i++)
            {
                if (revoluteJoints[i] != null)
                {
                    float initialAngle = initialJointAngles[i];
                    SetJointTargetDeg(revoluteJoints[i], initialAngle);
                    Debug.Log($"JointSetup: 设置关节 {revoluteJoints[i].gameObject.name} 初始角度为 {initialAngle} 度");
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
        /// 获取RevoluteJoint列表（用于Editor）
        /// </summary>
        public List<ArticulationBody> GetRevoluteJoints()
        {
            FindRevoluteJoints();
            return new List<ArticulationBody>(revoluteJoints);
        }
    }
}

