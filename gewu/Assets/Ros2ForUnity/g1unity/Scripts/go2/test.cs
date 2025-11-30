using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.UnitreeGo;

public class SubscribeSportModeState : MonoBehaviour
{
    private ROSConnection ros;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        // 订阅 /sportmodestate
        ros.Subscribe<SportModeStateMsg>("/sportmodestate", OnSportModeStateReceived);
        Debug.Log("已订阅 /sportmodestate");
    }

    void OnSportModeStateReceived(SportModeStateMsg msg)
    {
        // 打印主要字段
        Debug.Log($"[SportModeState] mode: {msg.mode}, gait_type: {msg.gait_type}");
        Debug.Log($"body_height: {msg.body_height}, velocity: ({msg.velocity[0]}, {msg.velocity[1]}, {msg.velocity[2]})");
        Debug.Log($"foot_raise_height: {msg.foot_raise_height}");
        Debug.Log($"foot_force: {string.Join(", ", msg.foot_force)}");
    }
}
