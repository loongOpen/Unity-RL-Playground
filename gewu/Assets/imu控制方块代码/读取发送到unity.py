import socket
import json
import logging
import struct
from datetime import datetime
import csv
import time
import os

# 设置日志
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(levelname)s - %(message)s')
logger = logging.getLogger(__name__)


# ====================== 新增：UDP 广播工具类 ======================
class UnityUdpBroadcaster:
    def __init__(self, unity_ip="127.0.0.1", unity_port=8889):
        """
        初始化 UDP 广播器（发送到 Unity）
        :param unity_ip: Unity 所在电脑的 IP（本地用 127.0.0.1，局域网填 Unity 电脑的 IPv4）
        :param unity_port: Unity 监听的端口（需与 Unity 脚本一致）
        """
        self.unity_ip = unity_ip
        self.unity_port = unity_port
        self.udp_socket = self._setup_udp_socket()

    def _setup_udp_socket(self):
        """初始化 UDP  socket"""
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
            sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            logger.info(f"UDP 广播器初始化完成，目标 Unity: {self.unity_ip}:{self.unity_port}")
            return sock
        except Exception as e:
            logger.error(f"UDP 初始化失败: {e}")
            return None

    def send_to_unity(self, parsed_data):
        """将解析后的动捕数据发送到 Unity"""
        if not self.udp_socket or not parsed_data:
            return

        try:
            # 构造 Unity 可直接解析的 JSON 格式（只包含核心控制数据，减少冗余）
            unity_data = {
                "timestamp": datetime.now().isoformat(),
                "packet_id": parsed_data.get("packet_count", 0),
                "euler_angles": parsed_data["euler_angles"],  # 欧拉角（控制 Unity 物体旋转）
                "battery_level": parsed_data["battery_level"],  # 电池状态（可选）
                "raw_data_length": len(parsed_data["raw_data_hex"]) // 2  # 数据长度（校验用）
            }

            # 转换为 JSON 字符串并发送
            json_str = json.dumps(unity_data, ensure_ascii=False)
            self.udp_socket.sendto(
                json_str.encode("utf-8"),
                (self.unity_ip, self.unity_port)
            )
        except Exception as e:
            logger.error(f"发送数据到 Unity 失败: {e}")

    def close(self):
        """关闭 UDP 连接"""
        if self.udp_socket:
            self.udp_socket.close()
            logger.info("UDP 广播器已关闭")


# ====================== 原有：动捕数据解析类 ======================
class MotionDataParser:
    def __init__(self):
        self.packet_count = 0
        self.debug = False  # 关闭调试信息

    def parse_packet(self, data_hex):
        """解析动捕蓝魔方数据包（原有逻辑不变，新增 packet_count 字段）"""
        try:
            data_bytes = bytes.fromhex(data_hex)

            # 检查帧头和长度（原有逻辑）
            if len(data_bytes) < 4 or data_bytes[0] != 0xBA or data_bytes[1] != 0xC0:
                return None
            if len(data_bytes) != 42:
                return None

            # 解析核心数据（原有逻辑不变）
            self.packet_count += 1
            parsed_data = {
                "packet_count": self.packet_count,  # 新增：数据包序号
                "header": f"{data_bytes[0]:02X}{data_bytes[1]:02X}",
                "packet_size": data_bytes[2],
                "battery_level": data_bytes[3],
                "raw_data_hex": data_hex,
                "euler_raw_hex": data_bytes[12:18].hex().upper(),
                "euler_raw_values": self._parse_euler_raw(data_bytes[12:18]),
                "euler_angles": self._parse_euler_angles(data_bytes[12:18]),  # 核心旋转数据
                "quaternion": self._parse_quaternion(data_bytes[4:12]),
                "acceleration": self._parse_acceleration(data_bytes[18:24]),
                "gyroscope": self._parse_gyroscope(data_bytes[24:30]),
                "magnetometer": self._parse_magnetometer(data_bytes[30:36]),
                "displacement": self._parse_displacement(data_bytes[36:42])
            }
            return parsed_data
        except Exception as e:
            return None

    # 以下为原有解析方法（_bytes_to_int16、_parse_quaternion 等），保持不变
    def _bytes_to_int16(self, low_byte, high_byte, debug_name=""):
        value = (high_byte << 8) | low_byte
        if value >= 0x8000:
            value -= 0x10000
        return value

    def _parse_euler_raw(self, data):
        if len(data) < 6:
            return {'x_raw': 0, 'y_raw': 0, 'z_raw': 0}
        return {
            'x_raw': self._bytes_to_int16(data[0], data[1]),
            'y_raw': self._bytes_to_int16(data[2], data[3]),
            'z_raw': self._bytes_to_int16(data[4], data[5])
        }

    def _parse_euler_angles(self, data):
        if len(data) < 6:
            return {'x': 0.0, 'y': 0.0, 'z': 0.0}
        x = self._bytes_to_int16(data[0], data[1]) / 100.0
        y = self._bytes_to_int16(data[2], data[3]) / 100.0
        z = self._bytes_to_int16(data[4], data[5]) / 100.0
        return {
            'x': self._normalize_angle(x),
            'y': self._normalize_angle(y),
            'z': self._normalize_angle(z)
        }

    def _normalize_angle(self, angle):
        while angle > 180:
            angle -= 360
        while angle < -180:
            angle += 360
        return angle

    def _parse_quaternion(self, data):
        if len(data) < 8:
            return {'w': 0.0, 'x': 0.0, 'y': 0.0, 'z': 0.0}
        return {
            'w': self._bytes_to_int16(data[0], data[1]) / 10000.0,
            'x': self._bytes_to_int16(data[2], data[3]) / 10000.0,
            'y': self._bytes_to_int16(data[4], data[5]) / 10000.0,
            'z': self._bytes_to_int16(data[6], data[7]) / 10000.0
        }

    def _parse_acceleration(self, data):
        if len(data) < 6:
            return {'x': 0.0, 'y': 0.0, 'z': 0.0}
        return {
            'x': self._bytes_to_int16(data[0], data[1]),
            'y': self._bytes_to_int16(data[2], data[3]),
            'z': self._bytes_to_int16(data[4], data[5])
        }

    def _parse_gyroscope(self, data):
        if len(data) < 6:
            return {'x': 0.0, 'y': 0.0, 'z': 0.0}
        return {
            'x': self._bytes_to_int16(data[0], data[1]) / 10.0,
            'y': self._bytes_to_int16(data[2], data[3]) / 10.0,
            'z': self._bytes_to_int16(data[4], data[5]) / 10.0
        }

    def _parse_magnetometer(self, data):
        if len(data) < 6:
            return {'x': 0.0, 'y': 0.0, 'z': 0.0}
        return {
            'x': self._bytes_to_int16(data[0], data[1]) / 100.0,
            'y': self._bytes_to_int16(data[2], data[3]) / 100.0,
            'z': self._bytes_to_int16(data[4], data[5]) / 100.0
        }

    def _parse_displacement(self, data):
        if len(data) < 6:
            return {'x': 0.0, 'y': 0.0, 'z': 0.0}
        return {
            'x': self._bytes_to_int16(data[0], data[1]),
            'y': self._bytes_to_int16(data[2], data[3]),
            'z': self._bytes_to_int16(data[4], data[5])
        }


# ====================== 原有：数据接收与存储类（新增 UDP 广播调用） ======================
class MotionDataReceiver:
    def __init__(self, port=8888, duration_minutes=10, unity_ip="127.0.0.1", unity_port=8889):
        self.port = port
        self.duration_minutes = duration_minutes
        self.parser = MotionDataParser()
        self.packet_count = 0
        self.csv_file = None
        self.csv_writer = None
        self.csv_filename = None
        self.report_filename = None
        # 新增：初始化 Unity UDP 广播器
        self.unity_broadcaster = UnityUdpBroadcaster(unity_ip, unity_port)

    def setup_csv(self):
        # 原有 CSV 初始化逻辑不变
        timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        self.csv_filename = f"motion_data_{timestamp}.csv"
        self.report_filename = f"statistics_report_{timestamp}.txt"
        self.csv_file = open(self.csv_filename, 'w', newline='', encoding='utf-8')
        self.csv_writer = csv.writer(self.csv_file)
        self.csv_writer.writerow([
            'timestamp', 'packet_count', 'raw_data_hex', 'euler_raw_hex',
            'euler_x_raw', 'euler_y_raw', 'euler_z_raw',
            'euler_x_degrees', 'euler_y_degrees', 'euler_z_degrees',
            'battery_level', 'quaternion_w', 'quaternion_x', 'quaternion_y', 'quaternion_z'
        ])
        print(f"数据将保存到: {self.csv_filename}")
        print(f"统计报告将保存到: {self.report_filename}")
        return self.csv_filename

    def close_csv(self):
        # 原有 CSV 关闭逻辑不变
        if self.csv_file:
            self.csv_file.close()
            self.csv_file = None

    def write_to_csv(self, parsed_data):
        # 原有 CSV 写入逻辑不变
        if self.csv_writer and parsed_data:
            timestamp = datetime.now().strftime("%Y-%m-%d %H:%M:%S.%f")[:-3]
            euler = parsed_data['euler_angles']
            euler_raw = parsed_data['euler_raw_values']
            quaternion = parsed_data['quaternion']
            self.csv_writer.writerow([
                timestamp, self.packet_count, parsed_data['raw_data_hex'], parsed_data['euler_raw_hex'],
                euler_raw['x_raw'], euler_raw['y_raw'], euler_raw['z_raw'],
                f"{euler['x']:.6f}", f"{euler['y']:.6f}", f"{euler['z']:.6f}",
                parsed_data['battery_level'],
                f"{quaternion['w']:.6f}", f"{quaternion['x']:.6f}", f"{quaternion['y']:.6f}", f"{quaternion['z']:.6f}"
            ])

    def generate_report(self, stats):
        # 原有报告生成逻辑不变
        try:
            with open(self.report_filename, 'w', encoding='utf-8') as f:
                f.write("=== 动捕数据采集统计报告 ===\n\n")
                f.write(f"采集时间: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
                f.write(f"数据文件: {self.csv_filename}\n")
                f.write(f"采集时长: {self.duration_minutes} 分钟\n")
                f.write(f"总数据包数: {stats['total_packets']}\n")
                f.write(f"实际时长: {stats['duration_seconds']:.2f} 秒\n")
                f.write(f"数据频率: {stats['total_packets'] / stats['duration_seconds']:.2f} Hz\n\n")

                # 后续报告内容不变...
            print(f"统计报告已生成: {self.report_filename}")
        except Exception as e:
            print(f"生成报告时出错: {e}")

    def calculate_statistics(self):
        # 原有统计计算逻辑不变
        try:
            import pandas as pd
            df = pd.read_csv(self.csv_filename)
            stats = {
                'total_packets': len(df),
                'duration_seconds': (pd.to_datetime(df['timestamp'].iloc[-1]) - pd.to_datetime(
                    df['timestamp'].iloc[0])).total_seconds(),
                'euler_x_raw': {'mean': df['euler_x_raw'].mean(), 'std': df['euler_x_raw'].std(),
                                'min': df['euler_x_raw'].min(), 'max': df['euler_x_raw'].max(),
                                'drift': df['euler_x_raw'].max() - df['euler_x_raw'].min()},
                'euler_y_raw': {'mean': df['euler_y_raw'].mean(), 'std': df['euler_y_raw'].std(),
                                'min': df['euler_y_raw'].min(), 'max': df['euler_y_raw'].max(),
                                'drift': df['euler_y_raw'].max() - df['euler_y_raw'].min()},
                'euler_z_raw': {'mean': df['euler_z_raw'].mean(), 'std': df['euler_z_raw'].std(),
                                'min': df['euler_z_raw'].min(), 'max': df['euler_z_raw'].max(),
                                'drift': df['euler_z_raw'].max() - df['euler_z_raw'].min()},
                'euler_x_degrees': {'mean': df['euler_x_degrees'].mean(), 'std': df['euler_x_degrees'].std(),
                                    'min': df['euler_x_degrees'].min(), 'max': df['euler_x_degrees'].max(),
                                    'drift': df['euler_x_degrees'].max() - df['euler_x_degrees'].min()},
                'euler_y_degrees': {'mean': df['euler_y_degrees'].mean(), 'std': df['euler_y_degrees'].std(),
                                    'min': df['euler_y_degrees'].min(), 'max': df['euler_y_degrees'].max(),
                                    'drift': df['euler_y_degrees'].max() - df['euler_y_degrees'].min()},
                'euler_z_degrees': {'mean': df['euler_z_degrees'].mean(), 'std': df['euler_z_degrees'].std(),
                                    'min': df['euler_z_degrees'].min(), 'max': df['euler_z_degrees'].max(),
                                    'drift': df['euler_z_degrees'].max() - df['euler_z_degrees'].min()}
            }
            return stats
        except Exception as e:
            print(f"计算统计信息时出错: {e}")
            return None

    def start(self):
        # 原有数据接收逻辑不变，新增「发送到 Unity」调用
        self.setup_csv()
        sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        sock.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        sock.bind(('', self.port))
        print(f"开始监听端口 {self.port}，持续 {self.duration_minutes} 分钟")
        print("实时显示欧拉角数据并保存到CSV文件...")
        print("按 Ctrl+C 可提前停止")
        sock.settimeout(1.0)
        start_time = time.time()
        end_time = start_time + self.duration_minutes * 60

        try:
            while time.time() < end_time:
                try:
                    data, addr = sock.recvfrom(1024)
                    try:
                        data_dict = json.loads(data.decode('utf-8'))
                        raw_data = data_dict.get('raw_data', '')

                        if raw_data:
                            parsed = self.parser.parse_packet(raw_data)
                            if parsed:
                                euler = parsed['euler_angles']
                                euler_raw = parsed['euler_raw_values']

                                # 1. 写入 CSV（原有逻辑）
                                self.write_to_csv(parsed)

                                # 2. 实时显示（原有逻辑）
                                remaining_time = end_time - time.time()
                                print(f"剩余时间: {int(remaining_time // 60):02d}:{int(remaining_time % 60):02d} | "
                                      f"原始值: X={euler_raw['x_raw']:6d}, Y={euler_raw['y_raw']:6d}, Z={euler_raw['z_raw']:6d} | "
                                      f"欧拉角: X={euler['x']:7.2f}°, Y={euler['y']:7.2f}°, Z={euler['z']:7.2f}° | "
                                      f"包计数: {self.packet_count}")

                                # 3. 新增：发送解析后的数据到 Unity
                                self.unity_broadcaster.send_to_unity(parsed)

                            self.packet_count += 1
                    except json.JSONDecodeError:
                        pass
                    except Exception as e:
                        pass
                except socket.timeout:
                    continue
        except KeyboardInterrupt:
            print("\n用户中断数据采集")
        finally:
            sock.close()
            self.close_csv()
            # 新增：关闭 UDP 广播器
            self.unity_broadcaster.close()

            # 原有统计逻辑不变
            print(f"\n数据采集完成，共接收 {self.packet_count} 个数据包")
            print(f"数据已保存到: {self.csv_filename}")
            stats = self.calculate_statistics()
            if stats:
                self.generate_report(stats)
                self._print_statistics(stats)

    def _print_statistics(self, stats):
        # 原有统计打印逻辑不变
        print("\n=== 数据统计信息 ===")
        print(f"总数据包数: {stats['total_packets']}")
        print(f"采集时长: {stats['duration_seconds']:.2f} 秒")
        print(f"数据频率: {stats['total_packets'] / stats['duration_seconds']:.2f} Hz")
        # 后续打印内容不变...


# ====================== 主函数（新增 Unity 地址配置） ======================
if __name__ == "__main__":
    # 配置：如果 Python 和 Unity 在同一台电脑，用 127.0.0.1；如果在局域网，填 Unity 电脑的 IPv4（如 192.168.1.100）
    UNITY_IP = "127.0.0.1"
    UNITY_PORT = 8889  # 需与 Unity 脚本的 port 一致

    # 运行 6 小时的数据采集，并发送到 Unity
    receiver = MotionDataReceiver(
        port=8888,  # 动捕数据接收端口
        duration_minutes=60 * 6,  # 采集时长（6小时）
        unity_ip=UNITY_IP,
        unity_port=UNITY_PORT
    )
    receiver.start()