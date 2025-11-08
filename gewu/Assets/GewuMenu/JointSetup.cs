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

        private void OnEnable()
        {
            RefreshArticulationBodies();
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
    }
}

