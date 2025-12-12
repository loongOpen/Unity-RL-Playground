using UnityEngine;
using UnityEngine.UI;

namespace Gewu.Flower
{
    /// <summary>
    /// ChahuaReset: 重置茶花位置
    /// </summary>
    public class ChahuaReset : MonoBehaviour
    {
        [Header("UI 设置")]
        [Tooltip("重置按钮")]
        public Button reset;
        
        [Header("动作序列")]
        [Tooltip("用于执行重置动作的 ActionSequence")]
        public ActionSequence actionSequence;
        
        void Start()
        {
            // 绑定按钮点击事件
            if (reset != null)
            {
                reset.onClick.AddListener(OnResetButtonClick);
            }
            else
            {
                Debug.LogWarning("ChahuaReset: reset 按钮未设置");
            }
        }
        
        void OnDestroy()
        {
            // 移除按钮点击事件
            if (reset != null)
            {
                reset.onClick.RemoveListener(OnResetButtonClick);
            }
        }
        
        /// <summary>
        /// 重置按钮点击事件
        /// </summary>
        void OnResetButtonClick()
        {
            if (actionSequence == null)
            {
                Debug.LogError("ChahuaReset: actionSequence 未设置");
                return;
            }
            
            if (actionSequence.actions == null || actionSequence.actions.Count == 0)
            {
                Debug.LogWarning("ChahuaReset: actionSequence 中没有动作");
                return;
            }
            
            // 执行最后一个 action
            int lastIndex = actionSequence.actions.Count - 1;
            actionSequence.RunAction(lastIndex);
            
            Debug.Log($"ChahuaReset: 执行最后一个动作 (索引 {lastIndex})");
        }
    }
}

