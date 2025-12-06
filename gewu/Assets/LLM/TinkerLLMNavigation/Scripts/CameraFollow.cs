using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 可在Inspector设置的跟随对象
    private Vector3 offsetPosition; // 位置偏移
    private bool mouse1Down;

    public float distance;
    public float scrollSpeed = 10;
    public float RotateSpeed = 2;

    // 为了限制垂直旋转的角度，设置一个最大最小的旋转范围
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;
    private float currentVerticalAngle = 0f;

    void Start()
    {
        if (target == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                target = playerObj.transform;
            else
                Debug.LogWarning("CameraFollow: 没有设置target，也未找到Tag为Player的对象！");
        }
        if (target != null)
        {
            transform.LookAt(target.position);
            offsetPosition = transform.position - target.position;
        }
    }

    void Update()
    {
        if (target == null) return;
        transform.position = offsetPosition + target.position;
        Rotate();
        Scroll();
    }

    void Scroll()
    {
        distance = offsetPosition.magnitude;
        distance -= Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        distance = Mathf.Clamp(distance, 2, 20);  // 控制摄像头的距离
        offsetPosition = offsetPosition.normalized * distance;  // 更新偏移位置
    }

    void Rotate()
    {
        if (Input.GetMouseButtonDown(1))
        {
            mouse1Down = true;
        }
        if (Input.GetMouseButtonUp(1))
        {
            mouse1Down = false;
        }

        if (mouse1Down)
        {
            // 水平旋转：绕 Y 轴旋转
            transform.RotateAround(target.position, target.up, RotateSpeed * Input.GetAxis("Mouse X"));

            // 垂直旋转：绕 X 轴旋转
            currentVerticalAngle -= RotateSpeed * Input.GetAxis("Mouse Y");

            // 限制垂直旋转的角度范围
            currentVerticalAngle = Mathf.Clamp(currentVerticalAngle, minVerticalAngle, maxVerticalAngle);

            // 应用旋转
            Quaternion rotation = Quaternion.Euler(currentVerticalAngle, transform.eulerAngles.y, 0);
            transform.rotation = rotation;
        }

        // 更新摄像头的偏移
        offsetPosition = transform.position - target.position;
    }
}
