using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeTuCameraRotation : MonoBehaviour
{
    public float sensitivityHor = 6f;
    public float sensitivityVert = 6f;
    // 大小范围
    public float minmumVert = -45f;
    public float maxmumVert = 45f;

    private float _rotationX = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))           //鼠标右键
        {
            // 垂直方向
            _rotationX -= Input.GetAxis("Mouse Y") * sensitivityVert;
            _rotationX = Mathf.Clamp(_rotationX, minmumVert, maxmumVert);
            // 水平方向
            float delta = Input.GetAxis("Mouse X") * sensitivityHor;
            float rotationY = transform.localEulerAngles.y + delta;
            transform.localEulerAngles = new Vector3(_rotationX, rotationY, 0);
        }
    }
}
