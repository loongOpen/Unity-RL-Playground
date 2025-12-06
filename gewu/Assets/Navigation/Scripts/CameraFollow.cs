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
        distance = Mathf.Clamp(distance, 2, 20);
        offsetPosition = offsetPosition.normalized * distance;
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
            transform.RotateAround(target.position, target.up, RotateSpeed * Input.GetAxis("Mouse X"));

            Vector3 originalPos = transform.position;
            Quaternion originalRotation = transform.rotation;
            transform.RotateAround(target.position, transform.right, -RotateSpeed * Input.GetAxis("Mouse Y"));

            float x = transform.eulerAngles.x;

            if (x < 10 || x > 80)
            {
                transform.position = originalPos;
                transform.rotation = originalRotation;
            }
        }
        offsetPosition = transform.position - target.position;
    }
}
