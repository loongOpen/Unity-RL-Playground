import asyncio
from bleak import BleakClient, BleakScanner
import logging
import socket
import json
from datetime import datetime

# 设置日志
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


class MotionCaptureCubeUDPBroadcaster:
    def __init__(self, broadcast_port=8888):
        # UUID配置
        self.write_uuid = "91680002-1111-6666-8888-0123456789ab"
        self.read_uuid = "91680003-1111-6666-8888-0123456789ab"

        # 启动指令：AB01FFFFFF (获取所有数据)
        self.start_command = bytes.fromhex("AB01FFFFFF")

        # UDP广播配置
        self.broadcast_port = broadcast_port
        self.broadcast_address = '10.98.217.103'  # 广播地址
        self.udp_socket = None

        self.client = None
        self.is_connected = False
        self.packet_count = 0

        # 初始化UDP socket
        self._setup_udp_socket()

    def _setup_udp_socket(self):
        """设置UDP广播socket"""
        try:
            self.udp_socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            self.udp_socket.setsockopt(socket.SOL_SOCKET, socket.SO_BROADCAST, 1)
            self.udp_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            logger.info(f"UDP广播socket初始化完成，端口: {self.broadcast_port}")
        except Exception as e:
            logger.error(f"UDP socket初始化失败: {e}")

    def broadcast_data(self, data_hex, packet_count):
        """通过UDP广播数据"""
        try:
            # 构建广播数据包
            broadcast_data = {
                "timestamp": datetime.now().isoformat(),
                "packet_id": packet_count,
                "device_type": "motion_capture_cube",
                "data_length": len(data_hex) // 2,  # 十六进制字符串长度转换为字节数
                "raw_data": data_hex,
                "data_bytes": list(bytes.fromhex(data_hex))
            }

            # 转换为JSON字符串
            json_data = json.dumps(broadcast_data, ensure_ascii=False)

            # 发送广播
            self.udp_socket.sendto(
                json_data.encode('utf-8'),
                (self.broadcast_address, self.broadcast_port)
            )

            if packet_count % 50 == 0:  # 每50个包打印一次日志，避免太频繁
                logger.info(f"已广播 {packet_count} 个数据包")

        except Exception as e:
            logger.error(f"广播数据失败: {e}")

    def notification_handler(self, sender, data):
        """BLE通知处理函数 - 处理并广播数据"""
        try:
            self.packet_count += 1

            # 获取原始数据
            data_hex = data.hex().upper()

            # 打印数据帧信息（可选）
            if self.packet_count % 100 == 0:  # 每100个包打印一次，避免太频繁
                print(f"\n数据帧 #{self.packet_count}")
                print("=" * 50)
                print(f"数据长度: {len(data)} 字节")
                print(f"十六进制: {data_hex}")
                print("=" * 50)

            # 广播数据
            self.broadcast_data(data_hex, self.packet_count)

        except Exception as e:
            logger.error(f"处理通知数据时出错: {e}")

    async def connect_to_device(self, device_address, max_retries=5, retry_delay=3):
        """连接到动捕蓝魔方设备并开始广播数据"""

        for attempt in range(max_retries):
            try:
                logger.info(f"尝试连接设备 {device_address} (尝试 {attempt + 1}/{max_retries})")

                # 扫描设备
                logger.info("正在扫描设备...")
                device = await BleakScanner.find_device_by_address(
                    device_address,
                    timeout=10.0
                )

                if device is None:
                    logger.warning("未找到指定设备")
                    if attempt < max_retries - 1:
                        logger.info(f"{retry_delay}秒后重试...")
                        await asyncio.sleep(retry_delay)
                    continue

                logger.info(f"找到设备: {device.name or 'Unknown'} - {device.address}")

                # 连接设备
                async with BleakClient(device, timeout=15.0) as client:
                    self.client = client
                    self.is_connected = True

                    logger.info(f"连接成功: {client.is_connected}")

                    # 等待服务解析完成
                    await asyncio.sleep(2.0)

                    # 检查服务特征
                    services = client.services
                    logger.info("发现的服务特征:")

                    write_char_found = False
                    read_char_found = False

                    for service in services:
                        for char in service.characteristics:
                            if char.uuid == self.write_uuid:
                                write_char_found = True
                                logger.info(f"找到写入特征: {self.write_uuid}")
                            if char.uuid == self.read_uuid:
                                read_char_found = True
                                logger.info(f"找到读取特征: {self.read_uuid}")

                    if not write_char_found or not read_char_found:
                        logger.error("未找到所有必需的特征")
                        continue

                    # 开启数据通知
                    await client.start_notify(self.read_uuid, self.notification_handler)
                    logger.info("数据通知已开启")

                    # 发送启动指令进入实时模式
                    logger.info(f"发送启动指令: {self.start_command.hex().upper()}")
                    await client.write_gatt_char(self.write_uuid, self.start_command, response=False)
                    logger.info("启动指令发送成功，开始读取并广播数据...")

                    # 保持连接状态
                    keep_alive_count = 0
                    try:
                        while client.is_connected:
                            await asyncio.sleep(1)
                            keep_alive_count += 1

                            # 每30秒发送一次保持连接指令
                            if keep_alive_count % 30 == 0:
                                try:
                                    await client.write_gatt_char(self.write_uuid, self.start_command, response=False)
                                    logger.debug("发送保持连接指令")
                                except Exception as e:
                                    logger.error(f"发送保持连接指令失败: {e}")
                                    break

                    except KeyboardInterrupt:
                        logger.info("用户中断连接")
                    finally:
                        # 清理资源
                        await client.stop_notify(self.read_uuid)
                        logger.info("数据通知已停止")
                        self.is_connected = False
                        if self.udp_socket:
                            self.udp_socket.close()
                            logger.info("UDP socket已关闭")

                    return

            except asyncio.TimeoutError:
                logger.error("连接超时")
            except Exception as e:
                logger.error(f"连接失败: {e}")

            if attempt < max_retries - 1:
                logger.info(f"{retry_delay}秒后重试...")
                await asyncio.sleep(retry_delay)

        logger.error("达到最大重试次数，连接失败")


async def main():
    """主函数"""
    # 设备MAC地址
    DEVICE_ADDRESS = "03:23:06:03:75:39"  # 根据实际设备修改
    BROADCAST_PORT = 8888  # 广播端口
    print("=" * 50)
    print("按 Ctrl+C 停止程序")
    print()

    try:
        # 连接设备并开始读取广播数据
        await motion_cube_broadcaster.connect_to_device(DEVICE_ADDRESS)
    except KeyboardInterrupt:
        logger.info("程序被用户中断")
    except Exception as e:
        logger.error(f"程序运行出错: {e}")


BROADCAST_PORT = 8888  # 可根据需要修改端口号
motion_cube_broadcaster = MotionCaptureCubeUDPBroadcaster(BROADCAST_PORT)
if __name__ == "__main__":
    asyncio.run(main())
    # 创建动捕蓝魔方UDP广播实例
    motion_cube_broadcaster = MotionCaptureCubeUDPBroadcaster(BROADCAST_PORT)

    print("动捕蓝魔方数据读取与UDP广播程序")
    print("=" * 50)
    print(f"目标设备: {DEVICE_ADDRESS}")
    print(f"广播端口: {BROADCAST_PORT}")
    print("广播地址: 10.98.217.103")

