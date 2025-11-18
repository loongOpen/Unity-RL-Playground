using UnityEngine;
using UnityEngine.AI;

public class NavCapsule : MonoBehaviour
{
    private NavMeshAgent agent;
    public Camera targetCamera;
    public float rotationSpeed = 2f;  // 固定旋转速度（弧度/秒）
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // 鼠标点击移动
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // 确保目标点在导航网格上
                NavMeshHit navHit;
                if (NavMesh.SamplePosition(hit.point, out navHit, 1.0f, NavMesh.AllAreas))
                {
                    agent.SetDestination(navHit.position);
                }
            }
        }
        
        // // 检查是否在导航过程中
        bool isNavigating = agent.hasPath && agent.remainingDistance > agent.stoppingDistance;
        
        // 如果不在导航过程中，启用键盘控制
        if (!isNavigating)
        {
            // Q/E 键控制旋转
            float rotationAmount = 0f;
            if (Input.GetKey(KeyCode.Z))
            {
                rotationAmount = -8f * rotationSpeed; // 左转
            }
            else if (Input.GetKey(KeyCode.C))
            {
                rotationAmount = 8f * rotationSpeed; // 右转
            }
            
            // 应用旋转
            if (rotationAmount != 0f)
            {
                float rotationDegrees = rotationAmount * Mathf.Rad2Deg;
                transform.Rotate(Vector3.up, rotationDegrees * Time.deltaTime);
            }
        }
    }
}