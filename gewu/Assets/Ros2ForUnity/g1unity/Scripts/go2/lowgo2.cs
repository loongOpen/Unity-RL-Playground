using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class Go2UnityCmdLow : MonoBehaviour
{
    private ROSConnection ros;
    public string cmdTopic = "/go2_cmd_low";

    // 示例站立和下蹲关节角度
    private float[] standPos = new float[12] { 0f, 1.36f, -2.65f, 0f, 1.36f, -2.65f, -0.2f, 1.36f, -2.65f, 0.2f, 1.36f, -2.65f };
    private float[] sitPos   = new float[12] { 0f, 0.67f, -1.3f, 0f, 0.67f, -1.3f, 0f, 0.67f, -1.3f, 0f, 0.67f, -1.3f };

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<Float32MultiArrayMsg>(cmdTopic);
    }

    void Update()
    {
        // 按键控制站立 / 下蹲
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PublishJointTargets(standPos);
        }
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
