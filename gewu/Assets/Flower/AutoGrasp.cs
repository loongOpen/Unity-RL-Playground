using UnityEngine;
using System.Collections;

namespace Gewu.Flower
{
    /// <summary>
    /// AutoGrasp: 自动抓取目标物体
    /// </summary>
    public class AutoGrasp : MonoBehaviour
    {
        [Header("目标设置")]
        [Tooltip("要抓取的目标物体")]
        public Transform target;
        
        [Header("动作序列")]
        [Tooltip("用于执行抓取动作的 ActionSequence")]
        public ActionSequence actionSequence;
        
        [Header("位置信息")]
        [Tooltip("初始位置（自动记录）")]
        public Vector3 initialPosition;
        
        [Tooltip("X 轴位置差值")]
        public float deltaX;
        
        [Tooltip("Z 轴位置差值")]
        public float deltaZ;
        
        [Header("自动检测设置")]
        [Tooltip("位置差值阈值（超过此值自动更新 action）")]
        public float positionThreshold = 0.01f;
        
        [Tooltip("是否启用自动检测")]
        public bool enableAutoDetection = true;
        
        private bool m_IsGrasping = false;
        private bool m_IsInitialized = false;
        
        void Start()
        {
            // 记录初始位置
            if (target != null)
            {
                initialPosition = target.position;
                m_IsInitialized = true;
            }
        }
        
        void Update()
        {
            if (!enableAutoDetection || !m_IsInitialized)
                return;
            
            if (target == null || actionSequence == null)
                return;
            
            // 持续检测位置变化
            CheckAndUpdatePosition();
        }
        
        /// <summary>
        /// 检查并更新位置（自动检测）
        /// </summary>
        void CheckAndUpdatePosition()
        {
            if (target == null)
                return;
            
            // 如果初始位置为零向量，则记录当前位置为初始位置
            if (initialPosition == Vector3.zero)
            {
                initialPosition = target.position;
                return;
            }
            
            // 计算 X、Z 轴差值
            float newDeltaX = target.position.x - initialPosition.x;
            float newDeltaZ = target.position.z - initialPosition.z;
            
            // 检查差值的绝对值是否超过阈值
            bool xExceeded = Mathf.Abs(newDeltaX) > positionThreshold;
            bool zExceeded = Mathf.Abs(newDeltaZ) > positionThreshold;
            
            // 检查差值是否发生变化（避免重复更新）
            bool xChanged = Mathf.Abs(newDeltaX - deltaX) > 0.001f;
            bool zChanged = Mathf.Abs(newDeltaZ - deltaZ) > 0.001f;
            
            if ((xExceeded || zExceeded) && (xChanged || zChanged))
            {
                // 更新差值
                deltaX = newDeltaX;
                deltaZ = newDeltaZ;
                
                // 自动执行 WriteAction
                WriteAction();
                
                Debug.Log($"AutoGrasp: 位置差值超过阈值，自动更新 action4。X差值: {deltaX:F3}, Z差值: {deltaZ:F3}");
            }
        }
        
        /// <summary>
        /// 刷新位置信息（手动调用）
        /// </summary>
        public void RefreshPosition()
        {
            if (target == null)
            {
                Debug.LogWarning("AutoGrasp: target 未设置，无法刷新位置");
                return;
            }
            
            // 如果初始位置为零向量，则记录当前位置为初始位置
            if (initialPosition == Vector3.zero)
            {
                initialPosition = target.position;
            }
            
            // 计算 X、Z 轴差值
            deltaX = target.position.x - initialPosition.x;
            deltaZ = target.position.z - initialPosition.z;
            
            Debug.Log($"AutoGrasp: 初始位置: {initialPosition}, 当前位置: {target.position}, X差值: {deltaX:F3}, Z差值: {deltaZ:F3}");
        }
        
        /// <summary>
        /// 开始抓取
        /// </summary>
        public void StartGrasp()
        {
            if (m_IsGrasping)
            {
                Debug.LogWarning("AutoGrasp: 正在抓取中，请稍候...");
                return;
            }
            
            if (target == null)
            {
                Debug.LogError("AutoGrasp: target 未设置");
                return;
            }
            
            if (actionSequence == null)
            {
                Debug.LogError("AutoGrasp: actionSequence 未设置");
                return;
            }
            
            StartCoroutine(GraspCoroutine());
        }
        
        /// <summary>
        /// 抓取协程
        /// </summary>
        IEnumerator GraspCoroutine()
        {
            m_IsGrasping = true;
            
            try
            {
                // 这里可以添加具体的抓取逻辑
                // 例如：移动到目标位置、抓取等
                
                Debug.Log($"AutoGrasp: 开始抓取目标: {target.name}");
                
                // 执行 ActionSequence 中的所有动作
                if (actionSequence != null && actionSequence.actions != null && actionSequence.actions.Count > 0)
                {
                    yield return StartCoroutine(ExecuteActionSequence());
                }
                
                Debug.Log("AutoGrasp: 抓取完成");
            }
            finally
            {
                m_IsGrasping = false;
            }
        }
        
        /// <summary>
        /// 执行 ActionSequence 中的所有动作
        /// </summary>
        IEnumerator ExecuteActionSequence()
        {
            for (int i = 0; i < actionSequence.actions.Count; i++)
            {
                Debug.Log($"AutoGrasp: 执行动作 {i + 1}/{actionSequence.actions.Count}");
                
                // 执行动作
                actionSequence.RunAction(i);
                
                // 等待动作完成（根据 actionInterval 设置）
                yield return new WaitForSeconds(actionSequence.actionInterval);
            }
        }
        
        /// <summary>
        /// 停止抓取
        /// </summary>
        public void StopGrasp()
        {
            if (m_IsGrasping)
            {
                StopAllCoroutines();
                m_IsGrasping = false;
                Debug.Log("AutoGrasp: 抓取已停止");
            }
        }
        
        /// <summary>
        /// 检查是否正在抓取
        /// </summary>
        public bool IsGrasping()
        {
            return m_IsGrasping;
        }
        
        /// <summary>
        /// 将位置差值写入 ActionSequence
        /// </summary>
        public void WriteAction()
        {
            if (actionSequence == null)
            {
                Debug.LogError("AutoGrasp: actionSequence 未设置");
                return;
            }
            
            if (actionSequence.actions == null || actionSequence.actions.Count < 4)
            {
                Debug.LogError($"AutoGrasp: actionSequence 需要至少 4 个动作，当前有 {actionSequence.actions?.Count ?? 0} 个");
                return;
            }
            
            // 修改 action4（索引3），同时设置 x 和 z 差值
            if (actionSequence.actions[3] != null)
            {
                string newText = BuildIKCommand(deltaX, deltaZ);
                actionSequence.actions[3].rawText = newText;
                Debug.Log($"AutoGrasp: 更新 action4，X 差值: {deltaX:F3}, Z 差值: {deltaZ:F3}, 新文本: {newText}");
            }
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(actionSequence);
            #endif
        }
        
        /// <summary>
        /// 构建 IK 命令字符串（x+deltaX,z+deltaZ）
        /// </summary>
        string BuildIKCommand(float xDelta, float zDelta)
        {
            string xCmd = $"x{GetSignString(xDelta)}{Mathf.Abs(xDelta):F3}";
            string zCmd = $"z{GetSignString(zDelta)}{Mathf.Abs(zDelta):F3}";
            return $"{xCmd},{zCmd}";
        }
        
        /// <summary>
        /// 获取符号字符串
        /// </summary>
        string GetSignString(float value)
        {
            return value >= 0 ? "+" : "-";
        }
    }
}

