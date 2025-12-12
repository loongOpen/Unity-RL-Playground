using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Gewu.Flower
{
    /// <summary>
    /// ChahuaAuto: 自动循环执行随机位置和动作序列
    /// </summary>
    public class ChahuaAuto : MonoBehaviour
    {
        [Header("UI 设置")]
        [Tooltip("自动循环按钮")]
        public Button auto;
        
        [Tooltip("显示计数信息的 Text 组件")]
        public Text count;
        
        [Header("动作序列")]
        [Tooltip("用于执行动作序列的 ActionSequence")]
        public ActionSequence actionSequence;
        
        [Header("随机位置")]
        [Tooltip("用于随机设置位置的 RandomPose")]
        public RandomPose randomPose;
        
        [Header("花朵检测")]
        [Tooltip("要检测的花朵 Transform")]
        public Transform flower;
        
        [Tooltip("花朵 Y 位置偏差阈值（小于此值则跳转到 Action 12）")]
        public float flowerYThreshold = 0.02f;
        
        [Header("循环设置")]
        [Tooltip("是否在 Start 时自动开始循环")]
        public bool autoStartOnStart = false;
        
        private bool m_IsRunning = false;
        private Coroutine m_AutoLoopCoroutine;
        private float m_FlowerInitialY = 0f;
        private bool m_FlowerInitialized = false;
        private int m_TotalCount = 0;  // 总循环次数
        private int m_FailCount = 0;   // 失败次数（偏差小于0.03时跳转）
        
        void Start()
        {
            // 绑定按钮点击事件
            if (auto != null)
            {
                auto.onClick.AddListener(OnAutoButtonClick);
            }
            else
            {
                Debug.LogWarning("ChahuaAuto: auto 按钮未设置");
            }
            
            // 记录花朵初始 Y 位置
            if (flower != null)
            {
                m_FlowerInitialY = flower.position.y;
                m_FlowerInitialized = true;
            }
            
            // 初始化计数显示
            UpdateCountText();
            
            // 如果设置了自动开始，则开始循环
            if (autoStartOnStart)
            {
                StartAutoLoop();
            }
        }
        
        void OnDestroy()
        {
            // 移除按钮点击事件
            if (auto != null)
            {
                auto.onClick.RemoveListener(OnAutoButtonClick);
            }
            
            // 停止循环
            StopAutoLoop();
        }
        
        /// <summary>
        /// 自动按钮点击事件
        /// </summary>
        void OnAutoButtonClick()
        {
            if (m_IsRunning)
            {
                StopAutoLoop();
            }
            else
            {
                StartAutoLoop();
            }
        }
        
        /// <summary>
        /// 开始自动循环
        /// </summary>
        public void StartAutoLoop()
        {
            if (m_IsRunning)
            {
                Debug.LogWarning("ChahuaAuto: 已经在运行中");
                return;
            }
            
            if (actionSequence == null)
            {
                Debug.LogError("ChahuaAuto: actionSequence 未设置");
                return;
            }
            
            if (randomPose == null)
            {
                Debug.LogError("ChahuaAuto: randomPose 未设置");
                return;
            }
            
            m_IsRunning = true;
            m_AutoLoopCoroutine = StartCoroutine(AutoLoopCoroutine());
            
            Debug.Log("ChahuaAuto: 开始自动循环");
        }
        
        /// <summary>
        /// 停止自动循环
        /// </summary>
        public void StopAutoLoop()
        {
            if (!m_IsRunning)
                return;
            
            if (m_AutoLoopCoroutine != null)
            {
                StopCoroutine(m_AutoLoopCoroutine);
                m_AutoLoopCoroutine = null;
            }
            
            m_IsRunning = false;
            Debug.Log("ChahuaAuto: 停止自动循环");
        }
        
        /// <summary>
        /// 自动循环协程
        /// </summary>
        IEnumerator AutoLoopCoroutine()
        {
            while (m_IsRunning)
            {
                // 每循环一次，总计数加一
                m_TotalCount++;
                UpdateCountText();
                
                // 执行 RandomPose 的 RandomizePose
                if (randomPose != null)
                {
                    randomPose.RandomizePose();
                    Debug.Log("ChahuaAuto: 执行随机位置");
                    
                    // 等待一小段时间，让位置设置完成
                    yield return new WaitForSeconds(0.5f);
                }
                
                // 执行 ActionSequence 的动作（自定义执行，以便在 Action7 后检查）
                if (actionSequence != null && actionSequence.actions != null && actionSequence.actions.Count > 0)
                {
                    // 执行动作序列（带检测）
                    yield return StartCoroutine(ExecuteActionsWithCheck());
                    
                    Debug.Log("ChahuaAuto: 动作执行完成，准备下一轮循环");
                }
                else
                {
                    Debug.LogWarning("ChahuaAuto: actionSequence 中没有动作");
                    yield return new WaitForSeconds(1f);
                }
                
                // 所有动作完成后，等待一小段时间再开始下一轮
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        /// <summary>
        /// 执行动作序列并检查花朵位置
        /// </summary>
        IEnumerator ExecuteActionsWithCheck()
        {
            if (actionSequence == null || actionSequence.actions == null)
                yield break;
            
            int actionCount = actionSequence.actions.Count;
            if (actionCount == 0)
                yield break;
            
            // 平均动作持续时间
            float averageActionDuration = 1.1f;
            
            // 跳转目标索引（Action 12，索引为11）
            int jumpToIndex = 11;
            
            for (int i = 0; i < actionCount; i++)
            {
                // 执行当前 action
                actionSequence.RunAction(i);
                
                // 等待动作完成
                yield return new WaitForSeconds(averageActionDuration);
                
                // 如果是 Action7（索引6），检查花朵位置
                if (i == 6 && flower != null && m_FlowerInitialized)
                {
                    float currentY = flower.position.y;
                    float yDelta = Mathf.Abs(currentY - m_FlowerInitialY);
                    
                    Debug.Log($"ChahuaAuto: Action7 执行完成，花朵 Y 位置偏差: {yDelta:F4}");
                    
                    // 使用 0.03 作为阈值判断失败（用户明确要求）
                    if (yDelta < 0.03f)
                    {
                        // 失败计数加一
                        m_FailCount++;
                        UpdateCountText();
                        
                        Debug.Log($"ChahuaAuto: 花朵 Y 位置偏差 ({yDelta:F4}) 小于阈值 (0.03), 跳转到 Action 12 (索引 {jumpToIndex})");
                        
                        // 如果跳转目标索引有效，跳转到 Action 12
                        if (jumpToIndex < actionCount)
                        {
                            // 跳过 Action 8-11，直接跳转到 Action 12
                            i = jumpToIndex - 1; // 因为循环会执行 i++，所以设置为 jumpToIndex - 1
                        }
                        else
                        {
                            Debug.LogWarning($"ChahuaAuto: 跳转目标索引 {jumpToIndex} 超出范围 (总动作数: {actionCount})");
                        }
                    }
                }
                
                // 如果不是最后一个 action，等待间隔时间
                if (i < actionCount - 1)
                {
                    yield return new WaitForSeconds(actionSequence.actionInterval);
                }
            }
        }
        
        /// <summary>
        /// 更新计数文本显示
        /// </summary>
        void UpdateCountText()
        {
            if (count != null)
            {
                count.text = $"已采集数据【{m_TotalCount}】条，失败【{m_FailCount}】条";
            }
        }
        
        /// <summary>
        /// 检查是否正在运行
        /// </summary>
        public bool IsRunning()
        {
            return m_IsRunning;
        }
    }
}

