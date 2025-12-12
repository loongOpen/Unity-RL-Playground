using UnityEngine;

namespace Unity.MLAgentsExamples
{
    /// <summary>
    /// CameraStabilize: 抑制相机抖动，通过平滑位置和旋转来减少高频振动
    /// </summary>
    public class CameraStabilize : MonoBehaviour
    {
        [Header("位置平滑设置")]
        [Tooltip("位置平滑时间（秒），值越大越平滑但响应越慢")]
        public float positionSmoothingTime = 0.1f;
        
        [Tooltip("是否启用位置平滑")]
        public bool smoothPosition = true;
        
        [Header("旋转平滑设置")]
        [Tooltip("旋转平滑时间（秒），值越大越平滑但响应越慢")]
        public float rotationSmoothingTime = 0.1f;
        
        [Tooltip("是否启用旋转平滑")]
        public bool smoothRotation = true;
        
        [Header("高级设置")]
        [Tooltip("更新模式：FixedUpdate 更稳定但可能不够流畅，LateUpdate 更流畅但可能不够稳定")]
        public UpdateMode updateMode = UpdateMode.LateUpdate;
        
        [Tooltip("最大位置变化速度（米/秒），用于限制突然的大幅移动")]
        public float maxPositionDelta = 10f;
        
        [Tooltip("最大旋转变化速度（度/秒），用于限制突然的大幅旋转")]
        public float maxRotationDelta = 180f;
        
        // 内部状态
        private Vector3 m_PositionVelocity;
        private Vector3 m_SmoothedPosition;
        private Quaternion m_SmoothedRotation;
        
        // 相对于父对象的初始偏移（保持不变）
        private Vector3 m_LocalPositionOffset;
        private Quaternion m_LocalRotationOffset;
        
        // 上一帧父对象的位置和旋转（用于检测变化）
        private Vector3 m_LastParentPosition;
        private Quaternion m_LastParentRotation;
        
        private bool m_IsInitialized = false;
        
        public enum UpdateMode
        {
            Update,
            FixedUpdate,
            LateUpdate
        }
        
        void Start()
        {
            // 记录相对于父对象的初始偏移
            if (transform.parent != null)
            {
                m_LocalPositionOffset = transform.localPosition;
                m_LocalRotationOffset = transform.localRotation;
                m_LastParentPosition = transform.parent.position;
                m_LastParentRotation = transform.parent.rotation;
            }
            else
            {
                // 如果没有父对象，记录世界坐标
                m_LocalPositionOffset = transform.position;
                m_LocalRotationOffset = transform.rotation;
                m_LastParentPosition = Vector3.zero;
                m_LastParentRotation = Quaternion.identity;
            }
            
            m_SmoothedPosition = transform.position;
            m_SmoothedRotation = transform.rotation;
            m_IsInitialized = true;
        }
        
        void Update()
        {
            if (updateMode == UpdateMode.Update)
            {
                StabilizeCamera();
            }
        }
        
        void FixedUpdate()
        {
            if (updateMode == UpdateMode.FixedUpdate)
            {
                StabilizeCamera();
            }
        }
        
        void LateUpdate()
        {
            if (updateMode == UpdateMode.LateUpdate)
            {
                StabilizeCamera();
            }
        }
        
        void StabilizeCamera()
        {
            if (!m_IsInitialized)
                return;
            
            // 计算目标位置和旋转（保持相对于父对象的偏移不变）
            Vector3 targetPosition;
            Quaternion targetRotation;
            
            if (transform.parent != null)
            {
                // 计算目标位置：父对象位置 + 平滑后的偏移
                Vector3 targetLocalOffset = m_LocalPositionOffset;
                Quaternion targetLocalRotation = m_LocalRotationOffset;
                
                // 检测父对象的变化
                Vector3 parentPositionDelta = transform.parent.position - m_LastParentPosition;
                Quaternion parentRotationDelta = transform.parent.rotation * Quaternion.Inverse(m_LastParentRotation);
                
                // 限制父对象位置变化速度（防止突然的大幅移动）
                if (maxPositionDelta > 0 && parentPositionDelta.magnitude > maxPositionDelta * Time.deltaTime)
                {
                    parentPositionDelta = parentPositionDelta.normalized * maxPositionDelta * Time.deltaTime;
                }
                
                // 限制父对象旋转变化速度（防止突然的大幅旋转）
                if (maxRotationDelta > 0)
                {
                    float parentRotationAngle = Quaternion.Angle(Quaternion.identity, parentRotationDelta);
                    float maxAngle = maxRotationDelta * Time.deltaTime;
                    if (parentRotationAngle > maxAngle)
                    {
                        parentRotationDelta = Quaternion.RotateTowards(Quaternion.identity, parentRotationDelta, maxAngle);
                    }
                }
                
                // 平滑父对象的变化
                Vector3 smoothedParentPosition;
                if (smoothPosition)
                {
                    float deltaTime = updateMode == UpdateMode.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
                    smoothedParentPosition = Vector3.SmoothDamp(
                        m_LastParentPosition,
                        transform.parent.position,
                        ref m_PositionVelocity,
                        positionSmoothingTime,
                        Mathf.Infinity,
                        deltaTime
                    );
                }
                else
                {
                    smoothedParentPosition = m_LastParentPosition + parentPositionDelta;
                }
                
                // 平滑父对象的旋转变化
                Quaternion smoothedParentRotation;
                if (smoothRotation)
                {
                    float deltaTime = updateMode == UpdateMode.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
                    float t = Mathf.Clamp01(deltaTime / rotationSmoothingTime);
                    smoothedParentRotation = Quaternion.Slerp(m_LastParentRotation, transform.parent.rotation, t);
                }
                else
                {
                    smoothedParentRotation = m_LastParentRotation * parentRotationDelta;
                }
                
                // 计算目标位置和旋转（保持相对位姿不变）
                targetPosition = smoothedParentPosition + smoothedParentRotation * m_LocalPositionOffset;
                targetRotation = smoothedParentRotation * m_LocalRotationOffset;
                
                // 更新上一帧的父对象状态
                m_LastParentPosition = smoothedParentPosition;
                m_LastParentRotation = smoothedParentRotation;
            }
            else
            {
                // 没有父对象，保持世界坐标不变（平滑当前值）
                targetPosition = transform.position;
                targetRotation = transform.rotation;
            }
            
            // 应用平滑后的位置和旋转
            transform.position = targetPosition;
            transform.rotation = targetRotation;
            m_SmoothedPosition = targetPosition;
            m_SmoothedRotation = targetRotation;
        }
        
        /// <summary>
        /// 重置平滑状态
        /// </summary>
        public void ResetSmoothing()
        {
            m_PositionVelocity = Vector3.zero;
            if (transform.parent != null)
            {
                m_LastParentPosition = transform.parent.position;
                m_LastParentRotation = transform.parent.rotation;
            }
            m_SmoothedPosition = transform.position;
            m_SmoothedRotation = transform.rotation;
        }
        
        /// <summary>
        /// 更新相对偏移（保持新的相对位姿）
        /// </summary>
        public void UpdateRelativeOffset()
        {
            if (transform.parent != null)
            {
                m_LocalPositionOffset = transform.localPosition;
                m_LocalRotationOffset = transform.localRotation;
                m_LastParentPosition = transform.parent.position;
                m_LastParentRotation = transform.parent.rotation;
                ResetSmoothing();
            }
        }
    }
}

