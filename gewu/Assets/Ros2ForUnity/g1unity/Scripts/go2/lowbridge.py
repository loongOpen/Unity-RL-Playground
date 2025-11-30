import sys
import time
import rclpy
from rclpy.node import Node
from std_msgs.msg import Float32MultiArray

from unitree_sdk2py.core.channel import ChannelPublisher, ChannelFactoryInitialize, ChannelSubscriber
from unitree_sdk2py.idl.unitree_go.msg.dds_ import LowCmd_, LowState_, MotorCmd_, BmsCmd_
from unitree_sdk2py.utils.crc import CRC
from unitree_sdk2py.utils.thread import RecurrentThread


class Go2LowBridge:
    def __init__(self):
        # PID 增益
        self.Kp = 60.0
        self.Kd = 5.0

        # 初始化底层命令
        self.low_cmd = LowCmd_(
            head=[0xFE, 0xEF],
            level_flag=0xFF,
            frame_reserve=0,
            sn=[0, 0],
            version=[0, 0],
            bandwidth=0,
            motor_cmd=[MotorCmd_(mode=1, q=0.0, dq=0.0, tau=0.0,
                                  kp=self.Kp, kd=self.Kd, reserve=[0,0,0]) for _ in range(20)],
            bms_cmd=BmsCmd_(off=0, reserve=[0,0,0]),
            wireless_remote=[0]*40,
            led=[0]*12,
            fan=[0]*2,
            gpio=0,
            reserve=0,
            crc=0
        )

        self.low_state = None
        self.crc = CRC()
        self.firstRun = True
        self.startPos = [0.0]*12

        # Unity 发送的目标角度
        self.currentTarget = None
        self.movePercent = 0.0
        self.moveDuration = 1000  # 运动周期，可调慢

        # DDS Publisher/Subscriber
        self.lowcmd_publisher = ChannelPublisher("rt/lowcmd", LowCmd_)
        self.lowcmd_publisher.Init()
        self.lowstate_subscriber = ChannelSubscriber("rt/lowstate", LowState_)
        self.lowstate_subscriber.Init(self.LowStateHandler, 10)

        # 定时发送线程
        self.lowCmdWriteThreadPtr = RecurrentThread(interval=0.002,
                                                    target=self.LowCmdWrite,
                                                    name="LowCmdWriter")

    def LowStateHandler(self, msg: LowState_):
        self.low_state = msg

    def LowCmdWrite(self):
        if self.low_state is None or self.currentTarget is None:
            return

        # 第一次运行，记录当前机器人实际关节角度作为起点
        if self.firstRun:
            for i in range(12):
                self.startPos[i] = self.low_state.motor_state[i].q
            self.firstRun = False

        # 线性插值比例，按 moveDuration 控制动作速度
        self.movePercent += 1.0 / self.moveDuration
        self.movePercent = min(self.movePercent, 1.0)

        # 三次平滑插值: smoothstep(t) = 3t^2 - 2t^3
        t = self.movePercent
        smooth_t = 3*t*t - 2*t*t*t

        for i in range(12):
            target_q = self.currentTarget[i]
            start_q = self.startPos[i]
            interpolated_q = start_q + (target_q - start_q) * smooth_t

            self.low_cmd.motor_cmd[i].q = interpolated_q
            self.low_cmd.motor_cmd[i].dq = 0.0
            self.low_cmd.motor_cmd[i].kp = self.Kp
            self.low_cmd.motor_cmd[i].kd = self.Kd
            self.low_cmd.motor_cmd[i].tau = 0.0

        # 更新 CRC 并发送
        self.low_cmd.crc = self.crc.Crc(self.low_cmd)
        self.lowcmd_publisher.Write(self.low_cmd)

        # 动作完成后重置
        if self.movePercent >= 1.0:
            self.startPos = self.currentTarget.copy()

            

    def Start(self):
        self.lowCmdWriteThreadPtr.Start()

    def SetTarget(self, joint_angles, duration=1000):
        if len(joint_angles) != 12:
            raise ValueError("必须提供12个关节角度")
        self.currentTarget = joint_angles
        self.moveDuration = duration
        self.movePercent = 0


class UnityBridgeNode(Node):
    def __init__(self, lowbridge: Go2LowBridge):
        super().__init__('unity_bridge_low')
        self.lowbridge = lowbridge

        # 订阅 Unity 发来的 Float32MultiArray
        self.subscription = self.create_subscription(
            Float32MultiArray,
            '/go2_cmd_low',
            self.joint_callback,
            10
        )
        self.subscription
        self.get_logger().info("[INFO] Unity -> LowBridge 已订阅 /go2_joint_targets")

    def joint_callback(self, msg: Float32MultiArray):
        if len(msg.data) != 12:
            self.get_logger().warn(f"接收到数组长度不为12: {len(msg.data)}")
            return
        self.get_logger().info(f"收到 Unity 指令: {msg.data}")
        self.lowbridge.SetTarget(list(msg.data))
      

def main(args=None):
    print("WARNING: 确保机器人周围无障碍物！")
    
    if len(sys.argv) > 1:
        ChannelFactoryInitialize(0, sys.argv[1])
    else:
        ChannelFactoryInitialize(0)

    # 初始化底层控制
    bridge = Go2LowBridge()
    bridge.Start()
   

    # 初始化 ROS 2 节点
    rclpy.init(args=args)
    unity_node = UnityBridgeNode(bridge)

    try:
        rclpy.spin(unity_node)
    except KeyboardInterrupt:
        pass
    finally:
        unity_node.destroy_node()
        rclpy.shutdown()
        print("[INFO] Bridge 已关闭")


if __name__ == "__main__":
    main()

