// 文件路径: Assets/Scripts/PuppetFollower.cs
using UnityEngine;

public class PuppetFollower : MonoBehaviour
{
    [Header("源 (操纵者)")]
    [Tooltip("G1OP 机器人的身体, 通常是 pelvis")]
    public Transform targetActor;

    [Header("目标 (傀儡)")]
    [Tooltip("G1IK 机器人的身体, 通常是 pelvis")]
    public Transform followerBody;

    [Header("傀儡的 IK 目标点")]
    [Tooltip("左手的 IK 目标 (handleft 方块)")]
    public Transform leftHandTarget;
    [Tooltip("右手的 IK 目标 (handright 方块)")]
    public Transform rightHandTarget;

    // 私有变量，用于存储初始的相对位置
    private Vector3 initialLeftHandOffset;
    private Vector3 initialRightHandOffset;
    private bool isInitialized = false;

    // 使用 Start 来进行初始化，确保所有物体都已就位
    void Start()
    {
        InitializeOffsets();
    }

    void InitializeOffsets()
    {
        if (targetActor != null && followerBody != null && leftHandTarget != null && rightHandTarget != null)
        {
            // 计算并存储 handleft/right 相对于 followerBody 的初始局部坐标
            // 这就像是拍下一张快照：“当机器人朝前时，手在身体的这个相对位置”
            initialLeftHandOffset = followerBody.InverseTransformPoint(leftHandTarget.position);
            initialRightHandOffset = followerBody.InverseTransformPoint(rightHandTarget.position);

            isInitialized = true;
            Debug.Log("PuppetFollower 已成功初始化并记录IK目标点偏移量！");
        }
        else
        {
            Debug.LogError("PuppetFollower 的某些引用未设置，无法初始化！请检查 SyncManager 上的所有字段。");
        }
    }

    // 在 LateUpdate 中执行，确保在所有物理和动画更新之后再进行同步
    void LateUpdate()
    {
        if (!isInitialized)
        {
            return;
        }

        // --- 核心同步逻辑 ---

        // 1. 同步身体的旋转
        followerBody.rotation = targetActor.rotation;

        // 2. 根据新的身体旋转，重新计算并强制设置 IK 目标点的“世界坐标位置”
        // 逻辑: "如果我的身体现在转向了这边，那么我的手也应该跟着移动到这个新的世界坐标"
        // TransformPoint: 将一个局部坐标点（我们记录的偏移量）转换为世界坐标点
        leftHandTarget.position = followerBody.TransformPoint(initialLeftHandOffset);
        rightHandTarget.position = followerBody.TransformPoint(initialRightHandOffset);

        // 注意：我们只修改 IK 目标点的 'position'。它们的 'rotation' 应该继续由你的 VR 手柄输入脚本（如IKG1）来控制，所以我们不去动它。
    }
}
