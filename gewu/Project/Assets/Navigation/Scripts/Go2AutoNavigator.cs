using UnityEngine;
using UnityEngine.AI;

public class Go2AutoNavigator : MonoBehaviour
{
    public Go2Agent agent; // 拖拽你的Go2Agent脚本
    public NavMeshAgent navAgent; // 拖拽你的NavMeshAgent
    public Transform robotBody; // 允许在Inspector中手动指定实际身体对象

    void Update()
    {
        // 检查鼠标点击，更新目标点和路径
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                navAgent.ResetPath(); // 清空所有目标点
                navAgent.SetDestination(hit.point); // 重新设置目标点
                // 立即停止运动
                agent.v1 = 0f;
                agent.v2 = 0f;
                agent.wr = 0f;
                return; // 本帧不再继续后续导航逻辑，等待NavMeshAgent重新计算路径
            }
        }

        if (navAgent.path == null || navAgent.path.corners.Length == 0) return;

        // 获取robotBody的世界坐标作为robotPos
        if (robotBody == null)
        {
            Debug.LogError("robotBody未设置，请在Inspector中指定实际身体对象！");
            return;
        }
        Vector3 robotPos = robotBody.position;
        int cornerIndex = 0;
        Vector3 targetPoint = navAgent.path.corners[cornerIndex];

        // 检查是否到达当前目标拐点
        float dist = Vector3.Distance(robotPos, targetPoint);
        while (dist < 0.1f && navAgent.path.corners.Length > cornerIndex + 1)
        {
            cornerIndex++;
            targetPoint = navAgent.path.corners[cornerIndex];
            dist = Vector3.Distance(robotPos, targetPoint);
        }

        // 如果只剩一个拐点且距离小于0.1，说明到达最终目标点
        if (navAgent.path.corners.Length == 1 && dist < 0.1f)
        {
            agent.v1 = 0f;
            agent.v2 = 0f;
            agent.wr = 0f;
            return;
        }

        // 输出当前目标拐点（第一个拐点）的坐标
        Debug.Log($"当前目标拐点坐标: {targetPoint}");

        // 输出robot和target的距离
        Debug.Log($"robot与target拐点的距离: {dist}");

        // 世界z轴为0度
        float robotZWorldAngle = robotBody.eulerAngles.y;
        Debug.Log($"robot z轴（世界坐标）角度: {robotZWorldAngle}");

        // robot到target的向量与世界z轴的夹角
        Vector3 toTarget = (targetPoint - robotPos).normalized;
        float toTargetWorldAngle = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
        Debug.Log($"robot到target向量与世界z轴夹角: {toTargetWorldAngle}");

        // robot正方向与toTarget的夹角（有正有负）
        Vector3 robotForward = robotBody.forward;
        float angle = Vector3.Angle(robotForward, toTarget); // 0~180
        float sign = Mathf.Sign(Vector3.Cross(robotForward, toTarget).y);
        float signedAngle = angle * sign;
        Debug.Log($"robot正方向与target向量的有符号夹角: {signedAngle}");

        float kp = 0.9f; // 比例系数
        float angleThreshold = 3f; // 允许的朝向误差范围（度）
        agent.v2 = 0f;
        // 判断水平方向距离
        Vector2 robotXZ = new Vector2(robotPos.x, robotPos.z);
        Vector2 targetXZ = new Vector2(targetPoint.x, targetPoint.z);
        float horizontalDist = Vector2.Distance(robotXZ, targetXZ);
        if (horizontalDist < 0.3f || Mathf.Abs(signedAngle) <= angleThreshold)
        {
            agent.wr = 0f; // 已经面向目标点或距离很近
        }
        else
        {
            float wr_p = kp * signedAngle;
            agent.wr = Mathf.Clamp(wr_p / 45f, -1f, 1f);
        }
        // 解除v1禁用，允许前进
        agent.v1 = Mathf.Clamp(horizontalDist, -10f, 10f);//为什么速度上不去?

        // 输出robot自己的坐标
        Debug.Log($"robot自身坐标: {robotPos}");
    }
}