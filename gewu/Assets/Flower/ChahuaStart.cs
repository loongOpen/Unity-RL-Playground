using UnityEngine;
using UnityEngine.UI;

namespace Gewu.Flower
{
    /// <summary>
    /// ChahuaStart: 启动茶花动作序列
    /// </summary>
    public class ChahuaStart : MonoBehaviour
    {
        [Header("UI 设置")]
        [Tooltip("启动按钮")]
        public Button start;
        
        [Header("动作序列")]
        [Tooltip("用于执行动作序列的 ActionSequence")]
        public ActionSequence actionSequence;
        
        void Start()
        {
            // 绑定按钮点击事件
            if (start != null)
            {
                start.onClick.AddListener(OnStartButtonClick);
            }
            else
            {
                Debug.LogWarning("ChahuaStart: start 按钮未设置");
            }
        }
        
        void OnDestroy()
        {
            // 移除按钮点击事件
            if (start != null)
            {
                start.onClick.RemoveListener(OnStartButtonClick);
            }
        }
        
        /// <summary>
        /// 启动按钮点击事件
        /// </summary>
        void OnStartButtonClick()
        {
            if (actionSequence == null)
            {
                Debug.LogError("ChahuaStart: actionSequence 未设置");
                return;
            }
            
            if (actionSequence.actions == null || actionSequence.actions.Count == 0)
            {
                Debug.LogWarning("ChahuaStart: actionSequence 中没有动作");
                return;
            }
            
            // 执行所有动作
            actionSequence.RunAllActions();
            
            Debug.Log("ChahuaStart: 开始执行所有动作");
        }
    }
}

