using UnityEngine;

namespace Unity.MLAgentsExamples
{
    public class HetuCameraFollow : MonoBehaviour
    {
        [Tooltip("The target to follow")] public Transform target;

        [Tooltip("The time it takes to move to the new position")]
        public float smoothingTime; //The time it takes to move to the new position

        [Tooltip("The target to look at (for rotation control)")]
        public Transform lookAt;

        [Tooltip("Rotation speed (degrees per second)")]
        public float rotationSpeed = 90f;

        [Tooltip("Distance from lookAt target")]
        public float distance = 10f;

        [Tooltip("Min vertical angle (degrees)")]
        public float minVerticalAngle = -60f;

        [Tooltip("Max vertical angle (degrees)")]
        public float maxVerticalAngle = 60f;

        private Vector3 m_Offset;
        private Vector3 m_CamVelocity; //Camera's velocity (used by SmoothDamp)
        private float m_HorizontalAngle = 0f; // 水平角度（绕Y轴）
        private float m_VerticalAngle = 0f;   // 垂直角度（上下）

        // Use this for initialization
        void Start()
        {
            m_Offset = gameObject.transform.position - target.position;
            
            // 如果有 lookAt，初始化角度
            if (lookAt != null)
            {
                Vector3 direction = transform.position - lookAt.position;
                distance = direction.magnitude;
                
                // 计算初始水平角度（绕Y轴）
                m_HorizontalAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                
                // 计算初始垂直角度
                float horizontalDistance = Mathf.Sqrt(direction.x * direction.x + direction.z * direction.z);
                m_VerticalAngle = Mathf.Atan2(direction.y, horizontalDistance) * Mathf.Rad2Deg;
            }
        }

        void Update()
        {
            // 如果有 lookAt，处理键盘旋转
            if (lookAt != null)
            {
                float horizontalInput = 0f;
                float verticalInput = 0f;

                // 检测左右键
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    horizontalInput = 1f;
                }
                else if (Input.GetKey(KeyCode.RightArrow))
                {
                    horizontalInput = -1f;
                }

                // 检测上下键
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    verticalInput = 1f;
                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    verticalInput = -1f;
                }

                // 更新角度
                if (horizontalInput != 0f)
                {
                    m_HorizontalAngle += horizontalInput * rotationSpeed * Time.deltaTime;
                }

                if (verticalInput != 0f)
                {
                    m_VerticalAngle += verticalInput * rotationSpeed * Time.deltaTime;
                    m_VerticalAngle = Mathf.Clamp(m_VerticalAngle, minVerticalAngle, maxVerticalAngle);
                }

                // 计算相机位置（球坐标系）
                float verticalRad = m_VerticalAngle * Mathf.Deg2Rad;
                float horizontalRad = m_HorizontalAngle * Mathf.Deg2Rad;

                float x = distance * Mathf.Cos(verticalRad) * Mathf.Sin(horizontalRad);
                float y = distance * Mathf.Sin(verticalRad);
                float z = distance * Mathf.Cos(verticalRad) * Mathf.Cos(horizontalRad);

                Vector3 targetPosition = lookAt.position + new Vector3(x, y, z);
                
                // 平滑移动到新位置
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref m_CamVelocity, smoothingTime, Mathf.Infinity, Time.deltaTime);
                
                // 始终看向 lookAt
                transform.LookAt(lookAt.position);
            }
        }

        void FixedUpdate()
        {
            // 如果没有 lookAt，使用原来的跟随逻辑
            if (lookAt == null && target != null)
            {
                var newPosition = new Vector3(target.position.x + m_Offset.x, transform.position.y,
                    target.position.z + m_Offset.z);

                gameObject.transform.position =
                    Vector3.SmoothDamp(transform.position, newPosition, ref m_CamVelocity, smoothingTime, Mathf.Infinity,
                        Time.fixedDeltaTime);
            }
        }
    }
}
