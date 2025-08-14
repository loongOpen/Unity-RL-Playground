using UnityEngine;

public class SyncTransform : MonoBehaviour
{
    // 在Unity编辑器中，将你那个名为 "root" 的目标对象拖到这里
    [Tooltip("要同步其位置和姿态的目标对象")]
    public Transform targetRoot;

    // 决定是在Update还是LateUpdate中执行
    public enum SyncTime
    {
        Update,
        LateUpdate
    }

    [Tooltip("在Update还是LateUpdate中同步？LateUpdate通常更适合跟随相机和动画后的移动，可以避免抖动。")]
    public SyncTime syncTime = SyncTime.LateUpdate;

    void Update()
    {
        if (syncTime == SyncTime.Update)
        {
            FollowTarget();
        }
    }

    void LateUpdate()
    {
        if (syncTime == SyncTime.LateUpdate)
        {
            FollowTarget();
        }
    }

    private void FollowTarget()
    {
        if (targetRoot != null)
        {
            // 将当前对象（机器人）的位置设置为目标对象的位置
            transform.position = targetRoot.position;

            // 将当前对象（机器人）的旋转设置为目标对象的旋转
            transform.rotation = targetRoot.rotation;
        }
        else
        {
            // 如果目标丢失，可以在控制台打印一个警告，方便调试
            Debug.LogWarning("同步目标(targetRoot)未设置！", this.gameObject);
        }
    }
}