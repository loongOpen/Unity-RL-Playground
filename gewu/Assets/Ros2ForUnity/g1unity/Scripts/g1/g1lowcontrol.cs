using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class Go2UnityCmdLowFull : MonoBehaviour
{
    private ROSConnection ros;
    public string cmdTopic = "/unity_joint_targets";


    private float[] standPos = new float[29] {
        0f, 0f, 0f, 0f, 0f, 0f,   // 左腿
        0f, 0f, 0f, 0f, 0f, 0f,   // 右腿
        0f, 0f, 0f,                       // 腰
        0f, 0f, 0f, 0f, 0f, 0f, 0f,       // 左臂
        0f, 0f, 0f, 0f, 0f, 0f, 0f        // 右臂
    };

 
    private float[] sitPos = new float[29] {
        -0.5f, 0f, 0f, 1.5f, -0.8f, 0f,
        -0.5f, 0f, 0f, 1.5f, -0.8f, 0f,
        0f, 0f, 0f,
        0f, 0f, 0f, 0f, 0f, 0f, 0f,
        0f, 0f, 0f, 0f, 0f, 0f, 0f
    };

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float32MultiArrayMsg>(cmdTopic);
    }

    void Update()
    {
        // 空格站立
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PublishJointTargets(standPos);
        }
        // 左Ctrl下蹲
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            PublishJointTargets(sitPos);
        }
    }

    private void PublishJointTargets(float[] jointTargets)
    {
        Float32MultiArrayMsg msg = new Float32MultiArrayMsg();
        msg.data = jointTargets;
        ros.Publish(cmdTopic, msg);
        Debug.Log("[Unity] 发送关节目标: " + string.Join(",", jointTargets));
    }
}
