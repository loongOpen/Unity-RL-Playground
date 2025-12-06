using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class Go2UnityKeyboardControl : MonoBehaviour
{
    private ROSConnection ros;
    public string cmdTopic = "/go2_cmd";

    void Start()
    {
        // 初始化 ROS 连接
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<StringMsg>(cmdTopic);
    }

    void Update()
    {
        // 高层控制键盘映射
        if (Input.GetKeyDown(KeyCode.W))
        {
            PublishCommand("move_forward");
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            PublishCommand("move_backward");
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            PublishCommand("move_left");
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            PublishCommand("move_right");
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            PublishCommand("rotate_left");
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            PublishCommand("rotate_right");
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            PublishCommand("stand_up");
        }
        else if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            PublishCommand("stand_down");
        }
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            PublishCommand("damp");
        }
    }

    // 发布 ROS 指令
    private void PublishCommand(string cmd)
    {
        StringMsg msg = new StringMsg(cmd);
        ros.Publish(cmdTopic, msg);
        Debug.Log("Unity发送指令: " + cmd);
    }
}
