// 文件路径: Assets/Manipulation/VRInputBinder.cs
using UnityEngine;
using UnityEngine.InputSystem;

public class VRInputBinder : MonoBehaviour
{
    [Header("抓取动作绑定")]
    [Tooltip("请在此处绑定左手柄的抓取/选择输入动作")]
    [SerializeField] private InputActionProperty leftGraspAction;

    [Tooltip("请在此处绑定右手柄的抓取/选择输入动作")]
    [SerializeField] private InputActionProperty rightGraspAction;

    private void Awake()
    {
        if (leftGraspAction != null && leftGraspAction.action != null)
        {
            VRInputProvider.LeftGraspAction = leftGraspAction.action;
            VRInputProvider.LeftGraspAction.Enable();
        }
        else { Debug.LogError("VRInputBinder: 左手抓取动作未在Inspector中绑定!", this); }

        if (rightGraspAction != null && rightGraspAction.action != null)
        {
            VRInputProvider.RightGraspAction = rightGraspAction.action;
            VRInputProvider.RightGraspAction.Enable();
        }
        else { Debug.LogError("VRInputBinder: 右手抓取动作未在Inspector中绑定!", this); }
    }
}
