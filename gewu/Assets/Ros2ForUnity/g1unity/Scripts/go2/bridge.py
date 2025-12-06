#!/usr/bin/env python3
import rclpy
from rclpy.node import Node
from std_msgs.msg import String  # Unity 发送命令的消息类型
from unitree_sdk2py.core.channel import ChannelFactoryInitialize
from unitree_sdk2py.go2.sport.sport_client import SportClient
import time

class UnityBridge(Node):
    def __init__(self, network_interface="enp4s0"):
        super().__init__('go2_bridge')

        # 初始化 DDS 通道
        print(f"[INFO] 初始化 DDS 通道，使用网卡: {network_interface}")
        ChannelFactoryInitialize(0, network_interface)

        # 初始化 SportClient
        print("[INFO] 初始化 Go2 SportClient...")
        self.sport_client = SportClient()
        self.sport_client.SetTimeout(10.0)
        self.sport_client.Init()
        print("[INFO] SportClient 初始化完成")

        # 订阅 Unity 指令
        self.subscription = self.create_subscription(
            String,
            '/go2_cmd',
            self.unity_command_callback,
            10
        )
        self.subscription  # prevent unused variable warning

    def unity_command_callback(self, msg):
        cmd = msg.data.lower()
        print(f"[UnityCommand] 收到命令: {cmd}")

        # 简单命令映射
        if cmd == "stand_up":
            self.sport_client.StandUp()
        elif cmd == "stand_down":
            self.sport_client.StandDown()
        elif cmd == "damp":
            self.sport_client.Damp()
        elif cmd == "move_forward":
            self.sport_client.Hello()
        elif cmd == "move_backward":
            self.sport_client.Stretch()
        elif cmd == "move_left":
            self.sport_client.Move(0, 0.3, 0)
        elif cmd == "move_right":
            self.sport_client.Move(0, -0.3, 0)
        elif cmd == "rotate_left":
            self.sport_client.Move(0, 0, 3)
        elif cmd == "rotate_right":
            self.sport_client.Move(0, 0, -3)
        elif cmd == "stop":
            self.sport_client.StopMove()
        else:
            print(f"[WARN] 未知命令: {cmd}")

def main(args=None):
    rclpy.init(args=args)
    bridge = UnityBridge()

    try:
        print("[INFO] Go2 Bridge 已启动，等待 Unity 指令...")
        rclpy.spin(bridge)
    except KeyboardInterrupt:
        pass
    finally:
        bridge.sport_client.StopMove()
        bridge.destroy_node()
        rclpy.shutdown()
        print("[INFO] Bridge 已关闭")

if __name__ == '__main__':
    main()


