from mcp.server.fastmcp import FastMCP
import logging
import subprocess
import webbrowser
import math
import random
#import ctypes
import sys
import os
#import requests
#from bs4 import BeautifulSoup

import paho.mqtt.client as mqtt
import threading

请求私钥 = "ac9a9f2b686bb4257867806c1dcfaf67"

# -------------------------------------------------------------------------------------------------
# 配置编码和日志
# 确保输出和日志使用正确的编码，避免中文字符显示问题
if sys.stderr.reconfigure(encoding='utf-8'):
    sys.stderr.reconfigure(encoding='utf-8')
    sys.stdout.reconfigure(encoding='utf-8')

# 配置日志记录
logging.basicConfig(level=logging.INFO, format='%(asctime)s - %(name)s - %(levelname)s - %(message)s')
logger = logging.getLogger('Windows_MCP_Tools')
# -------------------------------------------------------------------------------------------------

# 创建MCP服务器实例
mcp = FastMCP("WindowsToos")

# -------------------------------------------------------------------------------------------------
# 获取当前脚本所在的目录
# 这样可以确保我们能够正确找到预设文件，而无需手动指定绝对路径
current_dir = os.path.dirname(os.path.abspath(__file__))

# 构建预设文件的完整路径
# 假设预设文件位于当前目录下的"预设"子文件夹中
programs_file_path = os.path.join(current_dir, "预设", "程序预设.txt")
commands_file_path = os.path.join(current_dir, "预设", "命令预设.txt")

# 构建用户巴法私钥文件的完整路径
用户巴法私钥_file_path = os.path.join(current_dir, "数据", "接入API", "用户巴法私钥.txt")
# -------------------------------------------------------------------------------------------------

# 读取预设文件
def load_presets(file_path: str) -> dict:
    """
    从指定的文本文档中加载预设信息。
    参数： file_path: 文本文档的路径
    """
    presets = {}
    try:
        if not os.path.exists(file_path):  # 检查文件是否存在
            logger.info(f"\n未找到预设文件！已创建并写入默认内容")
            # 确保文件所在目录存在
            os.makedirs(os.path.dirname(file_path), exist_ok=True)
            # 创建文件并写入默认内容
            default_content = get_default_content(file_path)
            with open(file_path, 'w', encoding='utf-8') as file:
                file.write(default_content)
        with open(file_path, 'r', encoding='utf-8') as file:
            for line in file:
                line = line.strip()
                if line:
                    # 分割键值对，格式为 "键=值"
                    key, value = line.split('=', 1)
                    presets[key] = value
        return presets
    except Exception as e:
        logger.error(f"读取预设文件 {file_path} 时出错: {str(e)}")
        return {}

def get_default_content(file_path: str) -> str:
    """
    根据文件路径返回对应的默认内容。
    参数： file_path: 文本文档的路径
    """
    if "程序预设.txt" in file_path:
        return """记事本=C:\\Windows\\System32\\notepad.exe
计算器=C:\\Windows\\System32\\calc.exe
"""
    elif "命令预设.txt" in file_path:
        return """IP配置=ipconfig
系统信息=systeminfo
网络状态=netstat -ano
锁定电脑=rundll32.exe user32.dll,LockWorkStation
"""
    else:
        return ""

# 加载预设
preset_programs = load_presets(programs_file_path)
preset_commands = load_presets(commands_file_path)

# 读取用户巴法私钥
def load_用户巴法私钥(file_path: str) -> str:
    """
    从指定的文本文档中加载用户巴法私钥。
    参数： file_path: 文本文档的路径
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as file:
            用户巴法私钥 = file.read().strip()
            return 用户巴法私钥
    except FileNotFoundError:
        logger.error(f"用户巴法私钥文件 {file_path} 未找到")
        return ""
    except Exception as e:
        logger.error(f"读取用户巴法私钥文件 {file_path} 时出错: {str(e)}")
        return ""

# 加载用户巴法私钥
用户巴法私钥 = load_用户巴法私钥(用户巴法私钥_file_path)


# -------------------------------------------------------------------------------------------------


# 添加一个计算器工具
@mcp.tool()
def 计算器(python_expression: str) -> dict:
    """
    用于数学计算时，请始终使用此工具来计算 Python 表达式的结果。
    可以使用 `math` 和 `random` 模块。
    """
    result = eval(python_expression)
    logger.info(f"计算公式：{python_expression}，结果：{result}")
    return {"是否成功": True, "结果": result}

# -------------------------------------------------------------------------------------------------

# 定义工具函数：运行电脑端程序
@mcp.tool()
def 运行电脑端软件文件或程序(program_name: str) -> dict:
    """
    运行预设程序或指定路径的程序
    参数： program_name: 程序名称或路径，例如 "记事本" 或 "C:\\Windows\\System32\\notepad.exe"
    """
    try:
        # 如果是预设程序名称，则获取对应的路径
        program_path = preset_programs.get(program_name, program_name)
        if program_path.endswith('.lnk'):
            # 如果是.lnk文件，使用os.startfile打开
            os.startfile(program_path)
        else:
            # 否则直接运行程序
            subprocess.Popen(program_path)
        logger.info(f"\n\n运行程序：{program_path}\n")
        return {"是否成功": True, "结果": f"程序已启动：{program_path}"}
    except Exception as e:
        logger.error(f"\n\n错误！程序: {program_name} 运行失败！: {str(e)}\n")
        return {"是否成功": False, "错误请检查路径": str(e)}

# -------------------------------------------------------------------------------------------------
# 定义工具函数：在电脑上打开URL网址
@mcp.tool()
def 在电脑上打开URL网址(url: str) -> dict:
    """
    打开指定URL的网页。在电脑的浏览器上
    参数： url: 网页URL，例如 "https://www.baidu.com"
    """
    try:
        webbrowser.open(url)
        logger.info(f"\n\n执行打开URL网页: {url}\n")
        return {"是否成功": True, "结果": f"网页已打开：{url}"}
    except Exception as e:
        logger.error(f"错误！网页 {url} 打开失败！: {str(e)}\n")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------


# 定义工具函数：在电脑上运行CMD指令
@mcp.tool()
def 在电脑上运行CMD命令(command_name: str) -> dict:
    """
    运行预设CMD指令或指定的CMD指令。控制查看操作电脑信息/状态/锁定电脑等
    参数： command_name: CMD指令名称或命令，例如 "锁定电脑" 或 "ipconfig"
    """
    try:
        # 如果是预设指令名称，则获取对应的命令
        command = preset_commands.get(command_name, command_name)
        output = subprocess.check_output(command, shell=True, stderr=subprocess.STDOUT, text=True)
        logger.info(f"\n\n执行 CMD 命令: {command}\n执行结果: {output}\n")

        return {"是否成功": True, "结果": f"命令执行成功：\n{output}"}
    except Exception as e:
        logger.error(f"错误！运行 CMD 命令：{command_name} 失败！: {str(e)}\n")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------
   
# 定义工具函数：创建文件写入内容
@mcp.tool()
def 在电脑上创建文件与写入内容(file_path: str, content: str) -> dict:
    """
    在指定路径创建文件并写入内容
    参数： file_path: 文件路径，例如 "C:\\小智创建的文件.txt"
    参数： content: 要写入的内容
    """
    try:
        # 确保文件路径的目录存在
        os.makedirs(os.path.dirname(file_path), exist_ok=True)
        # 打开文件并写入内容
        with open(file_path, 'w', encoding='utf-8') as file:
            file.write(content)
        logger.info(f"\n\n文件创建并写入成功：{file_path}\n内容：{content}\n")
        return {"是否成功": True, "结果": f"文件已创建并写入成功：{file_path}"}
    except Exception as e:
        logger.error(f"创建文件并写入内容失败：{str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------


# 定义工具函数：读取复制内容
@mcp.tool()
def 读取复制内容() -> dict:
    """
    读取计算机中复制的内容，比如复制题目，复制文字文本等
    调用方法：读取复制内容({})
    """
    try:
        # 导入tkinter模块来处理剪贴板
        import tkinter as tk
        
        # 创建一个Tkinter根窗口
        root = tk.Tk()
        root.withdraw()  # 隐藏窗口
        
        # 从剪贴板读取内容
        clipboard_content = root.clipboard_get()
        
        # 销毁窗口
        root.destroy()
        
        logger.info(f"\n\n从剪贴板读取到内容: {clipboard_content}\n")
        
        return {"是否成功": True, "结果": clipboard_content}
    except Exception as e:
        logger.error(f"读取剪贴板内容失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------

        
import pyautogui
import pyperclip
import time

# 定义工具函数：自动粘贴内容到当前光标位置
@mcp.tool()
def 填入写入一段内容(content: str) -> dict:
    """
    填入写入将指定内容复制到剪贴板，然后模拟 Ctrl+V 操作粘贴到当前光标所在位置。不进行回车发送
    参数：
    content: 要粘贴的内容，例如 "这是有小智填入的一段内容！"
    """
    try:
        # 复制内容到剪贴板
        pyperclip.copy(content)

        # 等待1秒，确保目标窗口已准备好
        time.sleep(1)

        # 模拟 Ctrl+V 操作粘贴内容
        pyautogui.hotkey('Ctrl', 'v')

        logger.info(f"\n\n已将内容复制到剪贴板并粘贴到当前光标位置: {content}\n")
        return {"是否成功": True, "结果": f"已成功复制并粘贴内容: {content}"}
    except Exception as e:
        logger.error(f"自动粘贴内容失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}



# 定义工具函数：回车发送一段内容
@mcp.tool()
def 回车发送() -> dict:
    """
    模拟按下回车发送当前输入框中的内容
    """
    try:

        # 模拟 Enter 操作发送内容

        pyautogui.hotkey('Enter')
        
        logger.info(f"\n\n已模拟按下回车发送\n")
        return {"是否成功": True, "结果": f"已模拟按下回车发送！"}
    except Exception as e:
        logger.error(f"发送消息失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}


# -------------------------------------------------------------------------------------------------



# 定义工具函数：回车发送一段内容
@mcp.tool()
def 撤销操作() -> dict:
    """
    模拟按下Ctrl+Z 撤销刚刚的操作！
    比如刚刚填充的内容
    """
    try:

        # 模拟 Ctrl+Z 操作粘贴内容
        pyautogui.hotkey('Ctrl', 'z')
        
        logger.info(f"\n\n已模拟按下撤销快捷键！\n")
        return {"是否成功": True, "结果": f"已模拟按下撤销快捷键！"}
    except Exception as e:
        logger.error(f"撤销操作失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}


# -------------------------------------------------------------------------------------------------




import subprocess

# 定义工具函数：锁定电脑
@mcp.tool()
def 锁定电脑() -> dict:
    """
    锁定当前 Windows 计算机
    调用方法：锁定电脑({})
    """
    try:
        # 使用 subprocess 调用命令锁定电脑
        subprocess.run("rundll32.exe user32.dll,LockWorkStation", shell=True, check=True)
        logger.info("\n\n电脑已锁定\n")
        return {"是否成功": True, "结果": "电脑已锁定"}
    except Exception as e:
        logger.error(f"锁定电脑失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}


# -------------------------------------------------------------------------------------------------


# 定义工具函数：Shutdown系统操作（关机、重启、注销等）
@mcp.tool()
def 电脑关机计划(操作类型: str, 延迟时间: int = 0) -> dict:
    """
    执行Shutdown电脑关机计划操作，如关机、重启、等，或 取消已计划的操作。
    支持延时执行。
    参数：
    操作类型: 操作类型，可以是 "关机"、"重启" 或 "取消"
    延迟时间: 操作前的延迟时间（秒），默认为0（立即执行）。仅在操作类型不是"取消"时有效
    调用时需要向用户确认，以免关机导致数据丢失
    """
    try:
        # 映射操作类型到 shutdown 参数
        operation_map = {
            "关机": "/s",
            "重启": "/r",
            "取消": "/a"
        }

        # 获取对应的 shutdown 参数
        operation_param = operation_map.get(操作类型)
        if not operation_param:
            return {"是否成功": False, "错误": f"不支持的操作类型: {操作类型}"}

        if 操作类型 == "取消":
            # 取消操作
            subprocess.run(f"shutdown {operation_param}", shell=True, check=True)
            logger.info("\n\n已取消计划的系统操作\n")
            return {"是否成功": True, "结果": "已取消计划的系统操作"}
        else:
            # 构建命令
            command = f"shutdown {operation_param} /t {延迟时间}"

            # 执行命令
            subprocess.run(command, shell=True, check=True)

            logger.info(f"\n\n已计划系统操作：{操作类型}，延迟：{延迟时间}秒\n")
            return {"是否成功": True, "结果": f"已计划系统操作：{操作类型}，延迟：{延迟时间}秒"}
    except Exception as e:
        logger.error(f"执行系统操作失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------



from pycaw.pycaw import AudioUtilities, IAudioEndpointVolume
import comtypes
import ctypes

@mcp.tool()
def 设置主人电脑系统的音量(params: dict) -> dict:
    """
    设置 Windows 系统的音量
    调用方法：设置电脑音量({"音量": 50})  # 音量范围为 0-100
    """
    try:
        # 从参数中获取音量值，如果不存在则默认为50
        volume = params.get("音量", 50)
        
        # 确保音量值在0-100范围内
        if not 0 <= volume <= 100:
            return {"是否成功": False, "错误": "音量值必须在0-100之间"}
        
        # 获取音频设备
        devices = AudioUtilities.GetSpeakers()
        # 激活音量控制接口
        interface = devices.Activate(
            IAudioEndpointVolume._iid_, comtypes.CLSCTX_ALL, None
        )
        volume_control = ctypes.cast(interface, ctypes.POINTER(IAudioEndpointVolume))
        
        # 设置音量
        volume_control.SetMasterVolumeLevelScalar(volume / 100.0, None)
        
        logger.info(f"\n\n电脑音量已设置为: {volume}%\n")
        return {"是否成功": True, "结果": f"电脑音量已设置为: {volume}%"}
    except Exception as e:
        logger.error(f"设置电脑音量失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}
# -------------------------------------------------------------------------------------------------

@mcp.tool()
def 调用系统截图工具(screenshot_type: str) -> dict:
    """
    调用系统的截图工具
    参数：
    screenshot_type: 截图类型，"全屏" 或 "区域"
    """
    try:
        import pyautogui
        # 模拟按键：Print Screen（区域截图）或 Alt+Print Screen（全屏截图）
        if screenshot_type == "全屏":
            pyautogui.hotkey('alt', 'printscreen')
            action_text = "全屏截屏"
        elif screenshot_type == "区域":
            pyautogui.hotkey('printscreen')
            action_text = "区域截屏"
        else:
            return {"是否成功": False, "错误": "无效的截图类型，仅支持 '全屏' 或 '区域'"}

        logger.info(f"\n\n已调用系统 {action_text} 工具，截图将保存到剪贴板！\n")
        return {"是否成功": True, "结果": f"已调用系统 {action_text} 工具，截图将保存到剪贴板！"}
    except Exception as e:
        logger.error(f"调用系统截图工具失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------

@mcp.tool()
def 显示电脑桌面() -> dict:
    """
    模拟按下 Win+D 组合键，显示桌面
    """
    try:
        import pyautogui
        pyautogui.hotkey('winleft', 'd')  # 模拟按下 Win+D 组合键
        logger.info("\n\n已调用 Win+D 返回桌面\n")
        return {"是否成功": True, "结果": "已按下 Win+D 返回桌面"}
    except Exception as e:
        logger.error(f"返回桌面失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}
    
# -------------------------------------------------------------------------------------------------

import psutil
import time

@mcp.tool()
def 查看系统资源使用情况() -> dict:
    """
    查看系统资源使用情况，包括CPU、内存、磁盘、和网络使用情况
    """
    try:
        # 获取 CPU 使用率
        cpu_usage = psutil.cpu_percent(interval=1)
        
        # 获取内存使用情况
        memory = psutil.virtual_memory()
        memory_usage = memory.percent
        
        # 获取所有磁盘分区的使用情况
        disk_partitions = psutil.disk_partitions()
        disk_usages = []
        for partition in disk_partitions:
            try:
                usage = psutil.disk_usage(partition.mountpoint)
                disk_usages.append(f"{partition.device}: {usage.percent}% 已使用")
            except:
                disk_usages.append(f"{partition.device}: 无法获取使用情况")
        
        # 获取网络吞吐量
        net_io_before = psutil.net_io_counters()
        time.sleep(1)
        net_io_after = psutil.net_io_counters()
        upload_speed = (net_io_after.bytes_sent - net_io_before.bytes_sent) / 1024  # KB/s
        download_speed = (net_io_after.bytes_recv - net_io_before.bytes_recv) / 1024  # KB/s
        
        # 构建结果字符串
        result = (
            f"CPU使用率: {cpu_usage}%\n"
            f"内存使用率: {memory_usage}%\n"
            f"磁盘使用情况: {', '.join(disk_usages)}\n"
            f"网络上传速度: {upload_speed:.2f} KB/s\n"
            f"网络下载速度: {download_speed:.2f} KB/s"
        )
        
        logger.info(f"\n\n系统资源使用情况：\n{result}\n")
        return {"是否成功": True, "结果": result}
    except Exception as e:
        logger.error(f"查看系统资源使用情况失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------


import subprocess


@mcp.tool()
def 查看电脑配置信息() -> dict:
    """
    查看电脑的配置信息，使用 CMD 命令获取详细信息
    """
    try:
        # 获取系统信息
        system_info = subprocess.check_output("systeminfo", shell=True, text=True, errors="replace")
        
        # 获取 CPU 信息
        cpu_info = subprocess.check_output("wmic cpu get name", shell=True, text=True, errors="replace").strip()
        
        # 获取内存信息
        ram_info = subprocess.check_output("wmic memorychip get capacity", shell=True, text=True, errors="replace").strip()
        
        # 获取主板信息
        motherboard_info = subprocess.check_output("wmic baseboard get manufacturer,model", shell=True, text=True, errors="replace").strip()
        
        # 获取磁盘信息
        disk_info = subprocess.check_output("wmic diskdrive get model,size", shell=True, text=True, errors="replace").strip()
        
        # 获取 GPU 信息
        gpu_info = subprocess.check_output("wmic path win32_videocontroller get name", shell=True, text=True, errors="replace").strip()
        
        # 构建结果字符串
        result = (
            f"系统信息：\n{system_info}\n\n"
            f"CPU 信息：\n{cpu_info}\n\n"
            f"内存信息：\n{ram_info}\n\n"
            f"主板信息：\n{motherboard_info}\n\n"
            f"磁盘信息：\n{disk_info}\n\n"
            f"GPU 信息：\n{gpu_info}"
        )
        
        logger.info(f"\n\n电脑配置信息：\n{result}\n")
        return {"是否成功": True, "结果": f"电脑配置信息：\n{result}"}
    except Exception as e:
        logger.error(f"查看电脑配置信息失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------


import subprocess

@mcp.tool()
def 获取桌面完整路径() -> dict:
    """
    使用 CMD 命令获取当前用户的桌面完整路径
    并返回给你，如果用户让你在桌面创建文档，你不知道桌面完整路径，就可以使用此工具查看后再帮用户在桌面生成
    只要在桌面之类的需要知道桌面完整路径的都可以调用此工具查看！
    """
    try:
        # 使用 cmd 命令获取用户主目录，然后拼接 Desktop 路径
        user_profile = subprocess.check_output("echo %USERPROFILE%", shell=True, text=True, errors="replace").strip()
        desktop_path = f"{user_profile}\\Desktop"
        
        result = f"当前用户的桌面完整路径：{desktop_path}"
        
        logger.info(result)
        return {"是否成功": True, "结果": result}
    except Exception as e:
        logger.error(f"获取桌面路径失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------


# 检查是否允许使用微信工具  使用即同意协议
允许使用微信发消息工具 = False
允许使用微信发消息工具判断文件路径 = r"C:\xiaozhi\MCP\MCP_Windows\数据\允许使用微信发消息工具.DLL"
if os.path.exists(允许使用微信发消息工具判断文件路径):
    允许使用微信发消息工具 = True

# 根据权限文件动态注册微信工具
if 允许使用微信发消息工具:
    @mcp.tool()
    def 向微信指定联系人发送内容(微信联系人: str, 要发送的内容: str) -> dict:
        """
        以打开微信的方式显示微信窗口，搜索联系人显示联系人对话框，输入内容后直接回车发送
        因为是完全自动化指令，没有任何空隙，内容发错可能会有影响
        一定要向用户确认要发送的联系人和内容！
        参数：
        微信联系人: 要搜索的微信联系人  比如 "文件传输助手"
        要发送的内容: 要发送的内容  比如 "晚上好"
        模拟操作，所以发送速度较慢请耐心等待返回
        """
        try:

            logger.info(f"\n\n开始执行向{微信联系人}\n发送了内容！\n")
            #使用快捷键 Ctrl+Alt+W 呼出微信界面
            pyautogui.hotkey('Ctrl', 'alt', 'w')
            # 如果是预设程序名称，则获取对应的路径
            program_path = preset_programs.get("微信", "微信")
            if program_path.endswith('.lnk'):
                # 如果是.lnk文件，使用os.startfile打开
            # 运行微信以显示窗口
                os.startfile(program_path)
                # 等待
                time.sleep(0.1)
                os.startfile(program_path)
                # 等待
                time.sleep(0.1)
            else:
            # 运行微信以显示窗口 运行多次保证显示
                subprocess.Popen(program_path)
                # 等待
                time.sleep(0.1)           
                subprocess.Popen(program_path)
                # 等待
                time.sleep(0.1)
            # 等待
            time.sleep(0.2)
            #模拟 Ctrl+F 跳转到搜索
            pyautogui.hotkey('Ctrl', 'f')
            # 等待
            time.sleep(0.1)
            # 复制要搜索的联系人到剪贴板
            pyperclip.copy(微信联系人)
            # 等待0.1秒
            time.sleep(0.1)
            # 模拟 Ctrl+V 操作 粘贴要搜索的联系人
            pyautogui.hotkey('Ctrl', 'v')
            # 等待
            time.sleep(1.6)
            # 模拟 按下 Enter 操作 选中进入联系人对话框
            pyautogui.hotkey('Enter')
            #准备发送消息
            # 等待
            time.sleep(0.6)
            #使用快捷键 Ctrl+Alt+W 隐藏再呼出微信界面，确保光标在输入框
            pyautogui.hotkey('Ctrl', 'alt', 'w')
            time.sleep(0.5)
            pyautogui.hotkey('Ctrl', 'alt', 'w')
            # 复制要发送的内容到剪贴板
            pyperclip.copy(要发送的内容)
            # 等待
            time.sleep(0.5)
            # 模拟 Ctrl+V 操作 粘贴要搜索的联系人
            pyautogui.hotkey('Ctrl', 'v')
            # 等待
            time.sleep(0.2)
            # 模拟 按下 Enter 操作   发送内容
            pyautogui.hotkey('Enter')

            logger.info(f"\n\n已尝试向联系人：{微信联系人}\n发送了内容：{要发送的内容}\n")
            return {"是否成功": True, "结果": f"已尝试向联系人：{微信联系人}]\n发送了内容：{要发送的内容}"}
        except Exception as e:
            logger.error(f"\n\n错误！运行失败！: {str(e)}  可能未添加微信路径预设！\n")
            return {"是否成功": False, "错误！运行失败！ 可能未添加微信路径预设！": str(e)}

# -------------------------------------------------------------------------------------------------



@mcp.tool()
def 设置主人电脑系统深浅色主题(params: dict) -> dict:
    """
    设置 Windows 系统的浅色/深色主题
    调用方法：设置主人电脑系统深浅色主题({"深色": false})  # true = 深色，false = 浅色
    """
    import winreg
    try:
        dark = bool(params.get("深色", True))
        key_path = r"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize"
        value = 0 if dark else 1

        with winreg.OpenKey(winreg.HKEY_CURRENT_USER, key_path, 0, winreg.KEY_SET_VALUE) as key:
            # 两个关键值都改！
            winreg.SetValueEx(key, "AppsUseLightTheme",    0, winreg.REG_DWORD, value)
            winreg.SetValueEx(key, "SystemUsesLightTheme", 0, winreg.REG_DWORD, value)
            winreg.SetValueEx(key, "ColorPrevalence",      0, winreg.REG_DWORD, 1)

        logger.info(f"\n\n已切换为 {'深色' if dark else '浅色'} 主题\n")
        return {"是否成功": True, "结果": f"已切换为 {'深色' if dark else '浅色'} 主题"}
    except Exception as e:
        logger.error(f"切换主题失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}


# -------------------------------------------------------------------------------------------------



#以下为Open_API工具函数



# -------------------------------------------------------------------------------------------------


import requests

# 定义工具函数：获取心灵毒鸡汤
@mcp.tool()
def 获取心灵毒鸡汤() -> dict:
    """
    从网页公共开放的API获取心灵毒鸡汤
    """
    try:
        # 发送请求获取毒鸡汤
        response = requests.get("https://api.52vmy.cn/api/wl/yan/du")
        response.raise_for_status()  # 确保请求成功
        毒鸡汤 = response.text.strip()  # 获取返回的毒鸡汤

        logger.info(f"\n\n获取到的心灵毒鸡汤: {毒鸡汤}\n")
        return {"是否成功": True, "结果": 毒鸡汤}
    except Exception as e:
        logger.error(f"获取心灵毒鸡汤失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------

# 定义工具函数：获取抖音热点
@mcp.tool()
def 查询抖音热榜(limit: int = 20) -> dict:
    """
    获取抖音实时热榜（精简版）

    参数:
        limit: 返回前 N 条，默认 20，最大 50
    """
    url = "https://v2.xxapi.cn/api/douyinhot"

    try:
        resp = requests.get(url, timeout=10)
        resp.raise_for_status()
        data = resp.json()

        if data.get("code") != 200:
            msg = data.get("msg", "未知错误")
            logger.error(f"抖音热榜接口异常：{msg}")
            return {"是否成功": False, "错误": msg}

        hot_list = data.get("data", [])
        if not hot_list:
            return {"是否成功": False, "错误": "热榜数据为空"}

        # 限制条数
        limit = max(1, min(limit, 50))
        hot_list = hot_list[:limit]

        lines = []
        for idx, item in enumerate(hot_list, 1):
            title = item.get("word", "无标题")
            hot_val = item.get("hot_value", 0)
            lines.append(f"{idx:>2}. {title}  ({hot_val:,})")

        formatted = "\n【抖音实时热榜】\n" + "\n".join(lines)
        logger.info(f"\n{formatted}\n")
        return {"是否成功": True, "result": formatted}

    except requests.RequestException as e:
        logger.error(f"网络请求失败：{e}")
        return {"是否成功": False, "错误": f"网络请求失败：{e}"}
    except Exception as e:
        logger.error(f"解析失败：{e}")
        return {"是否成功": False, "错误": f"解析失败：{e}"}
    
# -------------------------------------------------------------------------------------------------



# 定义工具函数：获取随机一言
@mcp.tool()
def 获取随机一言() -> dict:
    """
    从网页公共开放的API获取随机一言（Hitokoto）。
    """
    try:
        # 发送 GET 请求
        response = requests.get("https://api.52vmy.cn/api/wl/yan/yiyan")
        response.raise_for_status()  # 确保请求成功
        # 解析 JSON 响应
        data = response.json()
        # 提取一言内容
        hitokoto = data.get("data", {}).get("hitokoto", "").strip()

        logger.info(f"\n\n获取到的随机一言: {hitokoto}\n")
        return {"是否成功": True, "结果": hitokoto}
    except Exception as e:
        logger.error(f"获取随机一言失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}


# -------------------------------------------------------------------------------------------------

# 定义工具函数：获取舔狗日记
@mcp.tool()
def 获取舔狗日记() -> dict:
    """
    从网页公共开放的API获取最新一条“舔狗日记”。
    """
    try:
        response = requests.get("https://api.52vmy.cn/api/wl/yan/tiangou")
        response.raise_for_status()
        data = response.json()
        # API 返回的日记内容在 content 字段
        diary = data.get("content", "").strip()

        logger.info(f"\n\n获取到的舔狗日记: {diary}\n")
        return {"是否成功": True, "结果": diary}
    except Exception as e:
        logger.error(f"获取舔狗日记失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------

# 定义工具函数：获取星座运势


@mcp.tool()
def 查询星座运势(星座名: str = "天秤座") -> dict:
    """
    获取指定星座的今日运势

    参数:
        星座名: 星座名称，例如“天秤座，天蝎座，巨蟹座”。若留空则默认为“天秤座”。
    """
    try:
        url = f"https://api.52vmy.cn/api/wl/s/xingzuo?msg={星座名}"
        # 发送请求获取
        response = requests.get(url)
        response.raise_for_status()  # 确保请求成功
        运势内容 = response.text.strip()  # 获取返回的内容

        logger.info(f"\n\n获取到的 【{星座名}】 运势内容: \n\n{运势内容}\n")
        return {"是否成功": True, "结果": 运势内容}
    except Exception as e:
        logger.error(f"获取今日电影票房榜失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------

# 定义工具函数：运势抽签
@mcp.tool()
def 运势抽签() -> dict:
    """
    调用公共 API 抽取一支今日运势签。
    """
    try:
        response = requests.get("https://api.52vmy.cn/api/wl/s/draw", timeout=10)
        response.raise_for_status()
        data = response.json()
        if data.get("code") != 200:
            logger.warning(f"运势抽签接口返回异常: {data}")
            return {"是否成功": False, "错误": data.get("msg", "未知错误")}
        sign_data = data.get("data", {})
        sign_text = sign_data.get("text", "").strip()
        logger.info(f"\n\n今日运势签: {sign_text}\n")
        return {"是否成功": True, "结果": sign_text}
    except Exception as e:
        logger.error(f"运势抽签失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------

# 定义工具函数：获取实时热榜
@mcp.tool()
def 查询三大平台热点(platform: str = "baidu", top: int = 10) -> dict:
    """
    获取百度 / 知乎 / 微博实时热榜
    """
    platform = platform.strip().lower()
    if platform not in {"baidu", "zhihu", "weibo"}:
        return {"是否成功": False, "错误": "platform 只能是 baidu / zhihu / weibo"}

    url = f"https://api.52vmy.cn/api/wl/hot?type={platform}"

    try:
        resp = requests.get(url, timeout=10)
        resp.raise_for_status()
        data = resp.json()

        if data.get("code") != 200:
            logger.warning(f"查询 {platform} 热榜接口异常: {data}")
            return {"是否成功": False, "错误": data.get("msg", "未知错误")}

        items = data.get("data", [])[:top]
        lines = []
        for idx, item in enumerate(items, 1):
            title = item.get("title", "")
            # 统一去掉 “万”“W”“个内容” 等后缀
            hot_raw = str(item.get("hot", "0"))
            hot_num = ''.join(filter(str.isdigit, hot_raw)) or "0"
            lines.append(f"{idx:>2}. {title}  ({hot_num})")

        formatted = (
            f"【{data.get('title', platform.upper())} 实时热榜 TOP{len(items)}】\n"
            + "\n".join(lines)
        )

        logger.info(f"\n\n{formatted}\n")
        return {"是否成功": True, "结果": formatted}

    except Exception as e:
        logger.error(f"查询 {platform} 热榜失败: {e}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------
 

# 定义工具函数：获取名人名言
@mcp.tool()
def 获取名人名言() -> dict:
    """
    从开放API随机获取一条名人名言，包含名言内容与作者信息。
    """
    try:
        response = requests.get("https://api.52vmy.cn/api/wl/yan/ming", timeout=10)
        response.raise_for_status()

        data = response.json()
        if data.get("code") != 200:
            logger.warning(f"名人名言接口返回异常: {data}")
            return {"是否成功": False, "错误": data.get("msg", "未知错误")}

        quote = data.get("data", {}).get("msg", "").strip()
        author = data.get("data", {}).get("source", "").strip()

        result = f"{quote} ——{author}" if author else quote
        logger.info(f"\n\n获取到的名人名言: {result}\n")
        return {"是否成功": True, "结果": result}
    except Exception as e:
        logger.error(f"获取名人名言失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------
 

# 定义工具函数：获取每日一句
@mcp.tool()
def 获取每日一句() -> dict:
    """
    获取今日的中英双语励志短句（每日一句）。
    返回：
        dict: {"是否成功": bool, "结果": str}
              成功时返回格式化后的中英双语句子；
              失败时返回错误信息。
    """
    try:
        resp = requests.get("https://api.52vmy.cn/api/wl/yan/day", timeout=10)
        resp.raise_for_status()
        data = resp.json()

        if data.get("code") != 200:
            logger.warning(f"每日一句接口异常: {data}")
            return {"是否成功": False, "错误": data.get("msg", "未知错误")}

        info = data.get("data", {})
        zh = info.get("zh", "").strip()
        en = info.get("en", "").strip()

        result = f"{zh}\n{en}"
        logger.info(f"\n\n获取到每日一句：\n{result}\n")
        return {"是否成功": True, "结果": result}

    except Exception as e:
        logger.error(f"获取每日一句失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}
    
# -------------------------------------------------------------------------------------------------
 
# 定义工具函数：获取绕口令
@mcp.tool()
def 获取绕口令() -> dict:
    """
    随机获取一条中文绕口令。
    返回:
        dict: {
            "是否成功": bool,
            "结果": str   # 完整的绕口令文本
        }
    """
    try:
        resp = requests.get("https://api.52vmy.cn/api/wl/yan/rao", timeout=10)
        resp.raise_for_status()
        data = resp.json()

        if data.get("code") != 200:
            logger.warning(f"绕口令接口异常: {data}")
            return {"是否成功": False, "错误": data.get("msg", "未知错误")}

        tongue_twister = data.get("data", {}).get("msg", "").strip()
        logger.info(f"\n\n获取到绕口令: {tongue_twister}\n")
        return {"是否成功": True, "结果": tongue_twister}

    except Exception as e:
        logger.error(f"获取绕口令失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}


# -------------------------------------------------------------------------------------------------
 

@mcp.tool()
def 查询油价() -> dict:
    """
    获取全国各省最新油价
    """
    url = "https://api.52vmy.cn/api/query/oil"

    try:
        resp = requests.get(url, timeout=10)
        resp.raise_for_status()
        data = resp.json()

        if data.get("code") != 200:
            msg = data.get("msg", "未知错误")
            logger.error(f"查询油价失败：{msg}")
            return {"是否成功": False, "错误": msg}

        oil_list = data.get("data", [])
        if not oil_list:
            logger.warning("油价接口返回数据为空")
            return {"是否成功": False, "错误": "返回数据为空"}

        formatted = ""
        for item in oil_list:
            city = item.get("city", "未知地区")
            formatted += f"地区：{city}\n"
            formatted += f"    0号柴油：{item.get('0', '无数据')} 元/升\n"
            formatted += f"    92号汽油：{item.get('92', '无数据')} 元/升\n"
            formatted += f"    95号汽油：{item.get('95', '无数据')} 元/升\n"
            formatted += f"    98号汽油：{item.get('98', '无数据')} 元/升\n"
            formatted += "-" * 50 + "\n"

        logger.info(f"\n\n获取油价成功：\n{formatted}\n")
        return {"是否成功": True, "result": formatted}

    except requests.RequestException as e:
        logger.error(f"请求油价接口时发生网络错误：{e}")
        return {"是否成功": False, "错误": f"网络请求失败：{e}"}
    except Exception as e:
        logger.error(f"解析油价数据时发生错误：{e}")
        return {"是否成功": False, "错误": f"解析数据失败：{e}"}

# -------------------------------------------------------------------------------------------------
 

# 定义工具函数：获取新年祝福语
@mcp.tool()
def 获取新年祝福语(category: str = "通用") -> dict:
    """
    随机获取一条 2025 蛇年祝福语，可按场景分类。

    参数:
        category (str): 可选分类关键词，默认“通用”。
                        支持：通用、生活、事业、家庭、友情、健康、学习、财富、情感、
                             领导、老师、同事、下属、朋友、家人、爱人、自己、孩子
    返回:
        dict: {
            "是否成功": bool,
            "结果": str   # 祝福语文本
        }
    """
    url = "https://api.52vmy.cn/api/zhufu/2025"
    params = {"msg": category.strip()}

    try:
        resp = requests.get(url, params=params, timeout=10)
        resp.raise_for_status()
        data = resp.json()

        if data.get("code") != 200:
            logger.warning(f"祝福语接口异常: {data}")
            return {"是否成功": False, "错误": data.get("msg", "未知错误")}

        blessing = data.get("data", {}).get("text", "").strip()
        if not blessing:
            return {"是否成功": False, "错误": "未返回祝福语内容"}

        logger.info(f"\n\n获取到【{category}】祝福语: {blessing}\n")
        return {"是否成功": True, "结果": blessing}

    except Exception as e:
        logger.error(f"获取新年祝福语失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}


# -------------------------------------------------------------------------------------------------
 
# 定义工具函数：获取今日电影票房 Top10
@mcp.tool()
def 获取今日电影票房() -> dict:
    """
    从网页公共开放的API获取今日电影票房榜前10名。
    """
    try:
        # 发送请求获取电影票房榜
        response = requests.get("https://api.52vmy.cn/api/wl/top/movie?type=text")
        response.raise_for_status()  # 确保请求成功
        票房内容 = response.text.strip()  # 获取返回的票房榜内容

        logger.info(f"\n\n获取到的今日电影票房榜: \n{票房内容}\n")
        return {"是否成功": True, "结果": 票房内容}
    except Exception as e:
        logger.error(f"获取今日电影票房榜失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------
      

# 定义工具函数：获取一条脑筋急转弯
@mcp.tool()
def 获取脑筋急转弯() -> dict:
    """
    从网页公共开放的API随机获取一条脑筋急转弯（含问题与答案）。
    你可以先向用户提出问题，让用户猜答案，再在回答完的时候提示用户正确答案。
    不要直接把问题和答案同时直接告诉用户！
    """
    try:
        response = requests.get("https://api.52vmy.cn/api/wl/s/jzw")
        response.raise_for_status()
        data = response.json()

        logger.info(f"\n\n获取到的脑筋急转弯: {data}\n")
        return {
            "是否成功": True,
            "问题": data.get("data", {}).get("question", ""),
            "答案": data.get("data", {}).get("answer", "")
        }
    except Exception as e:
        logger.error(f"获取脑筋急转弯失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------
  

# 定义工具函数：每日早报
@mcp.tool()
def 每日早报() -> dict:
    """
    从网页公共开放的API获取「每日60秒早报」。
    """
    try:
        response = requests.get("https://api.52vmy.cn/api/wl/60s/new")
        response.raise_for_status()
        data = response.json()

        if data.get("code") != 200:
            raise ValueError(f"接口返回非 200：{data}")

        news_list = data.get("data", [])
        logger.info(f"\n\n今日早报：\n" + "\n".join(news_list) + "\n")
        return {
            "是否成功": True,
            "早报列表": news_list
        }
    except Exception as e:
        logger.error(f"获取「每日早报」失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------

# 定义工具函数：今天吃什么
@mcp.tool()
def 今天吃什么() -> dict:
    """
    从网页公共开放的API随机获取一条「今天吃什么」推荐。
    """
    try:
        response = requests.get("https://api.52vmy.cn/api/wl/s/eat")
        response.raise_for_status()
        data = response.json()

        if data.get("code") != 200:
            raise ValueError(f"接口返回非 200：{data}")

        food = data.get("data", "")
        logger.info(f"\n\n今天吃：{food}\n")
        return {
            "是否成功": True,
            "推荐菜品": food
        }
    except Exception as e:
        logger.error(f"获取「今天吃什么」失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------
  

# 定义工具函数：搜索百度百科
@mcp.tool()
def 搜索百度百科(query: str) -> dict:
    """
    根据关键词搜索百度百科，返回简介及百度百科网页链接。请将简介信息告诉用户！
    可以询问用户是否需要跳转打开这个查询到的网页，如果用户让你打开这个网页，那么必须调用工具："在电脑上打开URL网址"，传递百度百科网址打开
    参数:
        query (str): 搜索词，例如 "QQ"、"Python" 之类。
    """
    try:
        # 构造请求 URL（对搜索词做 URL 编码）
        url = f"https://api.52vmy.cn/api/query/baike?msg={requests.utils.quote(query)}"
        response = requests.get(url, timeout=10)
        response.raise_for_status()
        data = response.json()

        if data.get("code") != 200:
            raise ValueError(f"接口返回非 200：{data}")

        info = data.get("data", {})
        logger.info(f"\n\n百度百科「{query}」搜索结果: \n{info.get('text')}\n")
        return {
            "是否成功": True,
            "搜索到的内容：": info.get("text", ""),
            "百度百科网址：": info.get("url", "")
        }
    except Exception as e:
        logger.error(f"搜索百度百科失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------
 

# 定义工具函数：获取历史上的今天
@mcp.tool()
def 获取历史上的今天() -> dict:
    """
    从网页获取历史上的今天的信息。
    """
    try:
        # 发送请求获取历史上的今天的信息
        response = requests.get("https://api.52vmy.cn/api/wl/today")
        response.raise_for_status()  # 确保请求成功
        历史数据 = response.json()  # 获取返回的JSON数据

        # 提取历史事件的年份和标题并格式化
        历史事件 = []
        for 事件 in 历史数据["data"]:
            年份 = 事件["year"]
            标题 = 事件["title"]
            # 格式化输出，年份和标题在同一行，每个事件之间换行
            历史事件.append(f"{年份} {标题}")

        # 将历史事件列表转换为一个整齐的字符串
        格式化历史事件 = "\n".join(历史事件)

        logger.info(f"\n\n获取到的历史上的今天的信息: \n\n{格式化历史事件}\n")
        return {"是否成功": True, "结果": 格式化历史事件}
    except Exception as e:
        logger.error(f"获取历史上的今天的信息失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}
# -------------------------------------------------------------------------------------------------


# 定义工具函数：获取万年历
@mcp.tool()
def 获取万年历() -> dict:
    """
    从网页获取当日的万年历信息。
    """
    try:
        # 发送请求获取万年历信息
        response = requests.get("https://api.52vmy.cn/api/wl/wnl")
        response.raise_for_status()  # 确保请求成功
        万年历数据 = response.json()  # 获取返回的JSON数据

        # 提取并格式化所需字段
        农历日期 = f"{万年历数据['lunarYear']}年{万年历数据['lMonth']}{万年历数据['lDate']}"
        干支 = f"{万年历数据['gzYear']}年 {万年历数据['gzMonth']}月 {万年历数据['gzDate']}日"
        生肖 = 万年历数据['animal']
        宜 = 万年历数据['suit']
        忌 = 万年历数据['avoid']

        格式化万年历 = (
            f"【公历】{万年历数据['year']}年{万年历数据['month']}月{万年历数据['day']}日\n"
            f"【农历】{农历日期}\n"
            f"【干支】{干支}\n"
            f"【生肖】{生肖}\n"
            f"【宜】{宜}\n"
            f"【忌】{忌}"
        )

        logger.info(f"\n\n获取到的万年历信息: \n\n{格式化万年历}\n")
        return {"是否成功": True, "结果": 格式化万年历}
    except Exception as e:
        logger.error(f"获取万年历信息失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}
    
# -------------------------------------------------------------------------------------------------
 



@mcp.tool()        
def 获取深证成指() -> dict:
    """
    获取深证成指实时行情数据（腾讯财经API）
    """
    try:
        url = "http://qt.gtimg.cn/q=s_sz399001"
        response = requests.get(url, timeout=5)
        response.encoding = 'gbk'
        data = response.text.split('~')
        result = {
            "指数名称": "深证成指", 
            "当前点数": data[3],
            "涨跌值": data[4],
            "涨跌幅": data[5] + "%",
            "成交量(手)": data[6],
            "成交额(万元)": data[7]
        }
        logger.info(f"\n\n获取到的深证成指: {result}\n")
        return {"是否成功": True, "结果": result}
    except Exception as e:
        logger.error(f"\n\n获取深证成指失败: {str(e)}\n")
        return {"是否成功": False, "错误": str(e)}


# -------------------------------------------------------------------------------------------------


@mcp.tool()
def 查询个股行情(股票代码: str) -> dict:
    """
    查询个股实时行情数据（腾讯财经API）
    参数：股票代码（如：sh601318 或 sz000001）
    """
    try:
        url = f"http://qt.gtimg.cn/q={股票代码}"
        response = requests.get(url, timeout=5)
        response.encoding = 'gbk'
        data = response.text.split('~')
        result = {
            "股票名称": data[1],
            "股票代码": data[2],
            "当前价": data[3],
            "昨收": data[4],
            "今开": data[5],
            "成交量(手)": data[6],
            "外盘": data[7],
            "内盘": data[8],
            "买一价": data[9],
            "买一量": data[10],
            "卖一价": data[19],
            "卖一量": data[20],
            "涨跌值": data[31],
            "涨跌幅": data[32] + "%",
            "最高": data[33],
            "最低": data[34],
            "成交额(万元)": data[37]
        }
        logger.info(f"\n\n获取到的：{股票代码}\n股票行情: {result}\n")
        return {"是否成功": True, "结果": result}
    except Exception as e:
        logger.error(f"\n\n获取：{股票代码} \n股票行情失败: {str(e)}\n")
        return {"是否成功": False, "错误": str(e)}


# -------------------------------------------------------------------------------------------------


@mcp.tool()
def 查询公司基本面(股票代码: str) -> dict:
    """
    查询上市公司基本面数据（腾讯财经API）
    参数：股票代码（如：sh601318 或 sz000001）
    """
    try:
        # 使用腾讯财经API获取简要基本面数据
        url = f"http://qt.gtimg.cn/q={股票代码}"
        response = requests.get(url, timeout=5)
        response.encoding = 'gbk'
        data = response.text.split('~')
        
        result = {
            "股票名称": data[1],
            "股票代码": data[2],
            "市盈率": data[39],
            "市净率": data[46],
            "总市值(亿元)": data[44],
            "流通市值(亿元)": data[45],
            "每股收益": data[32],
            "每股净资产": data[33]
        }
        logger.info(f"\n\n获取到的：{股票代码}\n股票行情: {result}\n")
        return {"是否成功": True, "结果": result}
    except Exception as e:
        logger.error(f"\n\n查询公司基本面：{股票代码} \n股票数据失败: {str(e)}\n")
        return {"是否成功": False, "错误": str(e)}

# -------------------------------------------------------------------------------------------------

import json
# 定义工具函数：12306查询车票
@mcp.tool()
def 查询高铁票(出发站: str, 到达站: str, 出发日期: str) -> dict:
    """
    查询火车/高铁票信息（12306）
    参数：出发站、到达站、出发日期（2025-07-12）
    目前仅支持查询车票无法预定！暂时无法查询中转！
    如果报错地名，说明此地名作者未声明，可以提醒用户联系作者添加！
    """
    try:
        城市代码列表文件路径= r"C:\xiaozhi\MCP\MCP_Windows\组件\MCP工具服务组件\12306查询车次\城市代码列表.json"
        k = open(城市代码列表文件路径,encoding='utf-8').read()
        城市代码=json.loads(k)
        #导入伪装信息
        User_agent=r"C:\xiaozhi\MCP\MCP_Windows\组件\MCP工具服务组件\12306查询车次\本地伪装信息\User-agent.txt"
        Cookie=r"C:\xiaozhi\MCP\MCP_Windows\组件\MCP工具服务组件\12306查询车次\本地伪装信息\Cookie.txt"
        Referer=r"C:\xiaozhi\MCP\MCP_Windows\组件\MCP工具服务组件\12306查询车次\本地伪装信息\Referer.txt"

        url =f"https://kyfw.12306.cn/otn/leftTicket/queryU?leftTicketDTO.train_date={出发日期}&leftTicketDTO.from_station={城市代码[出发站]}&leftTicketDTO.to_station={城市代码[到达站]}&purpose_codes=ADULT"

        headers =  {'user-agent': open(User_agent).read(),
                    'cookie': open(Cookie).read(),
                    'referer': open(Referer).read()}
        #发送请求
        res= requests.get(url,headers=headers)
        JSON = res.json()       # 将 12306 返回的 result 列表拿出来
        trains = JSON['data']['result']         # 车站代码→中文映射
        station_map = JSON['data']['map']

        count = 0
        车辆信息 = []          # ① 先建空列表
        for raw in trains:
            c = raw.split('|')
            # 不再去重
            count += 1
            from_zh = station_map.get(c[6], c[6])
            to_zh   = station_map.get(c[7], c[7])

            # ② 追加，而不是覆盖
            车辆信息.append(
                f"{c[3]:<5}  "
                f"{from_zh:<6}→  {to_zh:<6}"
                f"{c[8]}开  {c[9]}到"
                f" 时长：{c[10]:<5}"
                f" 商务 {c[32] or '-':<3}"
                f"一等 {c[31] or '-':<3}"
                f"二等 {c[30] or '-':<3}"
                f"无座 {c[29] or '-'}"
            )

        # ③ 把总趟数放在最末尾一起返回
        车辆信息.append(f"\n共查询到 {count} 趟列车!\n")
        logger.info("\n获取到的火车票信息:\n\n" + "\n".join(车辆信息))
        return {"是否成功": True, "结果": 车辆信息}
    
    except Exception as e:
        logger.error(f"获取火车票信息失败: {str(e)}")
        return {"是否成功": False, "错误": str(e)}
# -------------------------------------------------------------------------------------------------




# 工具：更换桌面壁纸
@mcp.tool()
def 更换桌面壁纸(content: str) -> dict:
    """
    根据关键词从在线壁纸 API 获取图片并设置为 Windows 桌面静态壁纸。最低1080P，最高4k
    参数:
        content (str): 壁纸类型关键词，例如 "风景"、"动漫"、"美女"、"宝马"、"斑马" 等。
                       留空，则返回随机类型壁纸。

    """
    api_root = "https://wp.upx8.com/api.php"
    save_dir = r"C:\xiaozhi\MCP\MCP_Windows\组件\MCP工具服务组件\下载\壁纸图片"
    os.makedirs(save_dir, exist_ok=True)

    # 统一主题命名：用户输入为空 → “随机”
    theme = content.strip() if content.strip() else "随机"

    try:
        # 1. 取 302 真实直链
        params = {"content": content.strip()} if content.strip() else {}
        resp = requests.get(api_root, params=params, timeout=15, allow_redirects=False)
        resp.raise_for_status()
        image_url = resp.headers.get("Location") or resp.headers.get("location")
        if not image_url:
            raise ValueError("未能获取壁纸，可尝试更换关键词！")

        # 2. 下载
        img_resp = requests.get(image_url, timeout=15, stream=True)
        img_resp.raise_for_status()

        # 3. 生成文件名：20250817-204728=风景.jpg
        file_name = f"{time.strftime('%Y%m%d-%H%M%S')}={theme}.jpg"
        local_path = os.path.join(save_dir, file_name)

        with open(local_path, "wb") as f:
            for chunk in img_resp.iter_content(chunk_size=8192):
                if chunk:
                    f.write(chunk)

        # 4. 设为桌面壁纸
        ctypes.windll.user32.SystemParametersInfoW(0x0014, 0, local_path, 0x01 | 0x02)

        logger.info(f"桌面壁纸已更换，类型：{theme}")
        return {
            "是否成功": True,
            "壁纸类型": theme
        }

    except Exception as e:
        logger.error(f"更换桌面壁纸失败: {e}")
        return {"是否成功": False, "错误，可尝试更换关键词！": str(e)}
    
    
# -------------------------------------------------------------------------------------------------



# 定义工具函数：作者环境下可用的专属工具

# 检查是否是作者工作环境
是作者工作环境 = False
是作者工作环境判断文件路径 = r"C:\粽子同学的PC.exe"
if os.path.exists(是作者工作环境判断文件路径):
    是作者工作环境 = True

    # 根据权限文件动态注册微信工具
    if 是作者工作环境:

        @mcp.tool()
        def 获取房间温湿度() -> dict:
            """
            获取房间环境温湿度
            获取到的内容前者为温度后者为湿度
            """

            HOST      = "bemfa.com"
            PORT      = 9501
            USERNAME  = "UserName"
            PASSWORD  = "Passwd"
            TOPIC     = "WSD004"
            MESSAGE   = "当前温湿度"
            TIMEOUT   = 3

            result = {"是否成功": False, "结果": ""}
            received = False
            timer = None

            def on_connect(client, userdata, flags, reason_code, properties):
                client.subscribe(TOPIC)
                client.publish(TOPIC, MESSAGE)

            def on_message(client, userdata, msg):
                nonlocal received, result
                payload = msg.payload.decode("utf-8", errors="ignore")
                if payload == MESSAGE:
                    return
                received = True
                result["是否成功"] = True
                result["结果"] = payload
                # 按你的格式记录日志
                logger.info("\n\n获取到温湿度:\n\n" + payload)
                client.disconnect()

            def on_timeout():
                nonlocal result
                if not received:
                    result["是否成功"] = False
                    result["结果"] = "请求超时"
                    # 按你的格式记录日志
                    logger.error("\n\n获取温湿度失败:\n\n" + str(e))

            client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=请求私钥)
            client.username_pw_set(USERNAME, PASSWORD)
            client.on_connect = on_connect
            client.on_message = on_message

            try:
                client.connect(HOST, PORT, 60)
                timer = threading.Timer(TIMEOUT, on_timeout)
                timer.start()
                client.loop_forever()
            except Exception as e:
                result["是否成功"] = False
                result["结果"] = str(e)
            finally:
                timer.cancel()

            return result


# -------------------------------------------------------------------------------------------------



#以下为洛雪音乐软件控制工具


# -------------------------------------------------------------------------------------------------


# 需要设置软件内全局快捷键

# 显示/隐藏程序  Alt+L

# 播放/暂停控制  Ctrl+Shift+空格

# 上一曲控制     Ctrl+Shift+A

# 下一曲控制     Ctrl+Shift+DD


# 需设置软件内快捷方式

#  聚焦搜索框    F1




# 定义工具函数：作者环境下可用的专属工具

# 检查是否是作者工作环境
使用控制洛雪音乐工具 = False
使用控制洛雪音乐工具判断文件路径 = r"C:\xiaozhi\MCP\MCP_Windows\数据\使用控制洛雪音乐工具.DLL"
if os.path.exists(使用控制洛雪音乐工具判断文件路径):
    使用控制洛雪音乐工具 = True

    # 根据权限文件动态注册微信工具
    if 使用控制洛雪音乐工具:


        @mcp.tool()
        def 洛雪音乐_搜索并播放音乐(要搜索的歌曲: str) -> dict:
            """
            以打开洛雪音乐的方式显示音乐窗口
            因为是完全自动化指令，没有任何空隙，内容发错可能会有影响
            可以向用户确认要搜索的歌曲！
            参数：
            要搜索的歌曲: 要搜索的歌曲  比如 "茫"
            模拟操作，所以操作速度较慢请耐心等待返回
            """
            try:

                logger.info(f"\n\n开始执行搜索并播放音乐：{要搜索的歌曲}！\n")
                #使用快捷键 'Alt', 'l' 呼出洛雪音乐界面
                pyautogui.hotkey('Alt', 'l')
                # 如果是预设程序名称，则获取对应的路径
                program_path = preset_programs.get("洛雪音乐", "洛雪音乐")
                if program_path.endswith('.lnk'):
                    # 如果是.lnk文件，使用os.startfile打开
                # 运行微信以显示窗口
                    os.startfile(program_path)
                    # 等待
                    time.sleep(0.1)
                    os.startfile(program_path)
                    # 等待
                    time.sleep(0.1)
                else:
                # 运行微信以显示窗口 运行多次保证显示
                    subprocess.Popen(program_path)
                    # 等待
                    time.sleep(0.1)           
                    subprocess.Popen(program_path)
                    # 等待
                    time.sleep(0.1)
                #准备搜索歌曲
                # 等待
                time.sleep(1)
                #模拟按下F1聚焦搜索框
                pyautogui.hotkey('F1')
                # 复制要搜索的歌曲名到剪贴板
                pyperclip.copy(要搜索的歌曲)
                # 模拟 Ctrl+A 操作 选中搜索框的内容覆盖
                pyautogui.hotkey('Ctrl', 'a')
                # 模拟 Ctrl+V 操作 粘贴要搜索的歌曲名
                pyautogui.hotkey('Ctrl', 'v')
                # 等待
                time.sleep(0)
                # 模拟 按下 Enter 操作 选中进入联系人对话框
                pyautogui.hotkey('Enter')
                # 等待
                time.sleep(1.6)
                #连按5次Tab 将光标移动到搜索到的第1首
                # 模拟 Tab 6次 间隔0.1秒
                for _ in range(6):
                    pyautogui.hotkey('Tab')
                    # 等待0.1秒
                    time.sleep(0.1)
                # 等待
                time.sleep(0.5)

                # 模拟 按下 Enter 操作 播放第1首搜索到的歌
                pyautogui.hotkey('Enter')

                logger.info(f"\n\n已尝试洛雪音乐搜索并播放歌曲：{要搜索的歌曲}\n")
                return {"是否成功": True, "结果": f"n已尝试洛雪音乐搜索并播放歌曲：{要搜索的歌曲}"}
            except Exception as e:
                logger.error(f"\n\n错误！洛雪音乐运行失败！: {str(e)}\n")
                return {"是否成功": False, "错误！洛雪音乐运行失败！": str(e)}

        # -------------------------------------------------------------------------------------------------


        @mcp.tool()
        def 洛雪音乐_暂停或继续播放音乐() -> dict:
            """
            模拟按下Ctrl+Shift+空格 暂停或继续播放音乐
            """
            try:

                # 模拟 Ctrl+Shift+空格 操作 控制全局暂停或播放音乐
                pyautogui.hotkey('Ctrl', 'Shift', 'Space')

                logger.info(f"\n\n已尝试按下洛雪音乐暂停或播放快捷键\n")
                return {"是否成功": True, "结果": f"已尝试按下洛雪音乐暂停或播放快捷键"}
            except Exception as e:
                logger.error(f"\n\n错误！洛雪音乐运行失败！: {str(e)}\n")
                return {"是否成功": False, "错误！洛雪音乐运行失败！": str(e)}

        # -------------------------------------------------------------------------------------------------

        

        @mcp.tool()
        def 洛雪音乐_上一首音乐() -> dict:
            """
            模拟按下全局快捷键Ctrl+Shift+A 播放上一首音乐
            """
            try:

                # 模拟 按下 Ctrl+Shift+A 操作 跳转到上一首歌
                
                pyautogui.hotkey('Ctrl', 'Shift', 'A')

                logger.info(f"\n\n已尝试播放洛雪音乐上一曲\n")
                return {"是否成功": True, "结果": f"已尝试播放洛雪音乐上一曲"}
            except Exception as e:
                logger.error(f"\n\n错误！洛雪音乐运行失败！: {str(e)}\n")
                return {"是否成功": False, "错误！洛雪音乐运行失败！": str(e)}

        # -------------------------------------------------------------------------------------------------


        @mcp.tool()
        def 洛雪音乐_下一首音乐() -> dict:
            """
            模拟按下全局快捷键Ctrl+Shift+D 播放下一首音乐
            """
            try:
                # 模拟 按下 Ctrl+Shift+D 操作 跳转到下一首歌
                pyautogui.hotkey('Ctrl', 'Shift', 'D')
                
                logger.info(f"\n\n已尝试播放洛雪音乐下一曲\n")
                return {"是否成功": True, "结果": f"已尝试播放洛雪音乐下一曲"}
            except Exception as e:
                    logger.error(f"\n\n错误！洛雪音乐运行失败！: {str(e)}\n")
                    return {"是否成功": False, "错误！洛雪音乐运行失败！": str(e)}


    # 根据权限文件动态注册工具
    if 是作者工作环境:
        
        @mcp.tool()
        def 洛雪音乐_播放收藏列表() -> dict:
            """
            以打开洛雪音乐的方式显示音乐窗口
            跳转到收藏列表，自动开始播放收藏列表的歌
            模拟操作，所以操作速度较慢请耐心等待返回
            """
            try:

                logger.info(f"\n\n开始播放收藏列表！\n")
                #使用快捷键 'Alt', 'l' 呼出洛雪音乐界面
                pyautogui.hotkey('Alt', 'l')
                # 如果是预设程序名称，则获取对应的路径
                program_path = preset_programs.get("洛雪音乐", "洛雪音乐")
                if program_path.endswith('.lnk'):
                    # 如果是.lnk文件，使用os.startfile打开
                # 运行微信以显示窗口
                    os.startfile(program_path)
                    # 等待
                    time.sleep(0.1)
                    os.startfile(program_path)
                    # 等待
                    time.sleep(0.1)
                else:
                # 运行微信以显示窗口 运行多次保证显示
                    subprocess.Popen(program_path)
                    # 等待
                    time.sleep(0.1)           
                    subprocess.Popen(program_path)
                    # 等待
                    time.sleep(0.1)
                #准备搜索歌曲
                # 等待
                time.sleep(1)
                #模拟按下F1聚焦搜索框
                pyautogui.hotkey('F1')
                time.sleep(0.6)

                #连按5次 Shift + Tab 将光标移动到收藏列表

                # 模拟 Tab 3次 间隔0.1秒 
                for _ in range(3):
                    pyautogui.hotkey('Shift', 'Tab')
                    # 等待0.1秒
                    time.sleep(0.1)

                # 模拟 按下Enter进入收藏列表
                pyautogui.hotkey('Enter')
                time.sleep(0.1)

                #连按5次 Tab 将光标移动到收藏列表第1首
                # 模拟 Tab 12次 间隔0.1秒
                for _ in range(12):
                    pyautogui.hotkey('Tab')
                    # 等待0.1秒
                    time.sleep(0.1)

                # 模拟 按下 Enter 操作 播放收藏列表第1首搜歌
                pyautogui.hotkey('Enter')

                logger.info(f"\n\n已尝试播放洛雪音乐收藏列表的歌!\n")
                return {"是否成功": True, "结果": f"n已尝试播放洛雪音乐收藏列表的歌"}
            except Exception as e:
                logger.error(f"\n\n错误！洛雪音乐运行失败！: {str(e)}\n")
                return {"是否成功": False, "错误！洛雪音乐运行失败！": str(e)}

        # -------------------------------------------------------------------------------------------------


# 回声洞获取
@mcp.tool()
def 获取回声洞() -> dict:
    """
    获取回声洞内容
    """

    HOST      = "bemfa.com"
    PORT      = 9501
    USERNAME  = "UserName"
    PASSWORD  = "Passwd"
    TOPIC     = "HSD004"
    MESSAGE   = "请求回声洞"
    TIMEOUT   = 3

    result = {"是否成功": False, "结果": ""}
    received = False
    timer = None

    def on_connect(client, userdata, flags, reason_code, properties):
        client.subscribe(TOPIC)
        client.publish(TOPIC, MESSAGE)

    def on_message(client, userdata, msg):
        nonlocal received, result
        payload = msg.payload.decode("utf-8", errors="ignore")
        if payload == MESSAGE:
            return
        received = True
        result["是否成功"] = True
        result["获取到的内容："] = payload
        # 按你的格式记录日志
        logger.info("\n\n获取到回声洞内容:\n\n" + payload)
        client.disconnect()

    def on_timeout():
        nonlocal result
        if not received:
            result["是否成功"] = False
            result["结果"] = "请求超时"
            # 按你的格式记录日志
            logger.error("\n\n获取回声洞内容失败:\n\n" + str(e))

    client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=请求私钥)
    client.username_pw_set(USERNAME, PASSWORD)
    client.on_connect = on_connect
    client.on_message = on_message

    try:
        client.connect(HOST, PORT, 60)
        timer = threading.Timer(TIMEOUT, on_timeout)
        timer.start()
        client.loop_forever()
    except Exception as e:
        result["是否成功"] = False
        result["结果"] = str(e)
    finally:
        timer.cancel()

    return result


# -------------------------------------------------------------------------------------------------


@mcp.tool()
def 推送巴法消息(要推送的主题: str, 要推送的消息: str) -> dict:
    """
    推送巴法消息
    参数：
    要推送的主题: 要推送的主题名称
    要推送的消息：要推送主题的内容
    要推送的内容和主题由用户提供！

    """
    HOST      = "bemfa.com"
    PORT      = 9501
    USERNAME  = "UserName"
    PASSWORD  = "Passwd"

    # 创建MQTT客户端
    client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=用户巴法私钥)
    client.username_pw_set(USERNAME, PASSWORD)

    try:
        # 连接并发送消息
        client.connect(HOST, PORT, 60)
        client.publish(要推送的主题, 要推送的消息)
        # 立即返回成功状态

        logger.info(f"\n\n已向主题：{要推送的主题}\n推送了内容：{要推送的消息}\n")
        return {"是否成功": True, "结果": f"已尝试已向主题：{要推送的主题}]\n推送了内容：{要推送的消息}"}
    except Exception as e:
        # 连接或发送失败时返回错误信息
        
        logger.error(f"\n\n错误！推送失败！: {str(e)}\n")
        return {"是否成功": False, "结果": f"推送失败: {str(e)}"}
    finally:
        # 确保断开连接
        try:
            client.disconnect()
        except:
            pass


# -------------------------------------------------------------------------------------------------

#添加自己更多的API工具 或者MCP工具

import threading
import time
import pyautogui

# W
stop_w = False

def 持续按下w():
    global stop_w
    pyautogui.keyDown('w')  # 模拟真实长按 W
    print("已按下 W 键并保持不松开（前进中）")
    while not stop_w:
        time.sleep(0.05)  # 控制检测频率
    pyautogui.keyUp('w')    # 松开 W
    print("已松开 W 键（停止前进）")

@mcp.tool()
def 开始前进() -> dict:
    global stop_w
    stop_w = False
    threading.Thread(target=持续按下w, daemon=True).start()
    print("\n\n已尝试按下 W 键并保持不松开（开始前进）\n")
    return {"是否成功": True, "结果": "开始前进"}

@mcp.tool()
def 停止前进() -> dict:
    global stop_w
    stop_w = True
    print("\n\n已尝试松开 W 键（停止前进）\n")
    return {"是否成功": True, "结果": "停止前进"}

# A
stop_a = False

def 持续按下a():
    global stop_a
    pyautogui.keyDown('a') 
    print("已按下 A 键并保持不松开（左转中）")
    while not stop_a:
        time.sleep(0.05)
    pyautogui.keyUp('a')  
    print("已松开 A 键（停止左转）")

@mcp.tool()
def 开始左转() -> dict:
    global stop_a
    stop_a = False
    threading.Thread(target=持续按下a, daemon=True).start()
    print("\n\n已尝试按下 A 键并保持不松开（开始左转）\n")
    return {"是否成功": True, "结果": "开始左转"}

@mcp.tool()
def 停止左转() -> dict:
    global stop_a
    stop_a = True
    print("\n\n已尝试松开 A 键（停止左转）\n")
    return {"是否成功": True, "结果": "停止左转"}


# S
stop_s = False

def 持续按下s():
    global stop_s
    pyautogui.keyDown('s') 
    print("已按下 S 键并保持不松开（后退中）")
    while not stop_s:
        time.sleep(0.05)
    pyautogui.keyUp('s')  
    print("已松开 S 键（停止后退）")

@mcp.tool()
def 开始后退() -> dict:
    global stop_s
    stop_s = False
    threading.Thread(target=持续按下s, daemon=True).start()
    print("\n\n已尝试按下 S 键并保持不松开（开始后退）\n")
    return {"是否成功": True, "结果": "开始后退"}

@mcp.tool()
def 停止后退() -> dict:
    global stop_s
    stop_s = True
    print("\n\n已尝试松开 S 键（停止后退）\n")
    return {"是否成功": True, "结果": "停止后退"}


# D
stop_d = False

def 持续按下d():
    global stop_d
    pyautogui.keyDown('d') 
    print("已按下 D 键并保持不松开（右转中）")
    while not stop_d:
        time.sleep(0.05)
    pyautogui.keyUp('d')  
    print("已松开 D 键（停止右转）")

@mcp.tool()
def 开始右转() -> dict:
    global stop_d
    stop_d = False
    threading.Thread(target=持续按下d, daemon=True).start()
    print("\n\n已尝试按下 D 键并保持不松开（开始右转）\n")
    return {"是否成功": True, "结果": "开始右转"}

@mcp.tool()
def 停止右转() -> dict:
    global stop_d
    stop_d = True
    print("\n\n已尝试松开 D 键（停止右转）\n")
    return {"是否成功": True, "结果": "停止右转"}


# 1
stop_1 = False

def 持续按下1():
    global stop_1
    pyautogui.keyDown('1') 
    print("已按下 1 键并保持不松开（闭合左夹爪中）")
    while not stop_1:
        time.sleep(0.05)
    pyautogui.keyUp('1')  
    print("已松开 1 键（停止闭合左夹爪）")

@mcp.tool()
def 开始闭合左夹爪() -> dict:
    global stop_1
    stop_1 = False
    threading.Thread(target=持续按下1, daemon=True).start()
    print("\n\n已尝试按下 1 键并保持不松开（开始闭合左夹爪）\n")
    return {"是否成功": True, "结果": "开始闭合左夹爪"}

@mcp.tool()
def 停止闭合左夹爪() -> dict:
    global stop_1
    stop_1 = True
    print("\n\n已尝试松开 1 键（停止闭合左夹爪）\n")
    return {"是否成功": True, "结果": "停止闭合左夹爪"}

# 2
stop_2 = False

def 持续按下2():
    global stop_2
    pyautogui.keyDown('2') 
    print("已按下 2 键并保持不松开（打开左夹爪中）")
    while not stop_2:
        time.sleep(0.05)
    pyautogui.keyUp('2')  
    print("已松开 2 键（停止打开左夹爪）")

@mcp.tool()
def 开始打开左夹爪() -> dict:
    global stop_2
    stop_2 = False
    threading.Thread(target=持续按下2, daemon=True).start()
    print("\n\n已尝试按下 2 键并保持不松开（开始打开左夹爪）\n")
    return {"是否成功": True, "结果": "开始打开左夹爪"}

@mcp.tool()
def 停止打开左夹爪() -> dict:
    global stop_2
    stop_2 = True
    print("\n\n已尝试松开 2 键（停止打开左夹爪）\n")
    return {"是否成功": True, "结果": "停止打开左夹爪"}


# 9
stop_9 = False

def 持续按下9():
    global stop_9
    pyautogui.keyDown('9') 
    print("已按下 9 键并保持不松开（闭合右夹爪中）")
    while not stop_9:
        time.sleep(0.05)
    pyautogui.keyUp('9')  
    print("已松开 9 键（停止闭合右夹爪）")

@mcp.tool()
def 开始闭合右夹爪() -> dict:
    global stop_9
    stop_9 = False
    threading.Thread(target=持续按下9, daemon=True).start()
    print("\n\n已尝试按下 9 键并保持不松开（开始闭合右夹爪）\n")
    return {"是否成功": True, "结果": "开始闭合右夹爪"}

@mcp.tool()
def 停止闭合右夹爪() -> dict:
    global stop_9
    stop_9 = True
    print("\n\n已尝试松开 9 键（停止闭合右夹爪）\n")
    return {"是否成功": True, "结果": "停止闭合右夹爪"}

# 0
stop_0 = False

def 持续按下0():
    global stop_0
    pyautogui.keyDown('0') 
    print("已按下 0 键并保持不松开（打开右夹爪中）")
    while not stop_0:
        time.sleep(0.05)
    pyautogui.keyUp('0')  
    print("已松开 0 键（停止打开右夹爪）")

@mcp.tool()
def 开始打开右夹爪() -> dict:
    global stop_0
    stop_0 = False
    threading.Thread(target=持续按下0, daemon=True).start()
    print("\n\n已尝试按下 0 键并保持不松开（开始打开右夹爪）\n")
    return {"是否成功": True, "结果": "开始打开右夹爪"}

@mcp.tool()
def 停止打开右夹爪() -> dict:
    global stop_0
    stop_0 = True
    print("\n\n已尝试松开 0 键（停止打开右夹爪）\n")
    return {"是否成功": True, "结果": "停止打开右夹爪"}


#********************左臂***********************

# x
stop_x = False

def 持续按下x():
    global stop_x
    pyautogui.keyDown('x') 
    print("已按下 X 键并保持不松开（抬起左臂）")
    while not stop_x:
        time.sleep(0.05)
    pyautogui.keyUp('x')  
    print("已松开 X 键（停止抬起左臂）")

@mcp.tool()
def 开始抬起左臂() -> dict:
    global stop_x
    stop_x = False
    threading.Thread(target=持续按下x, daemon=True).start()
    print("\n\n已尝试按下 X 键并保持不松开（开始抬起左臂）\n")
    return {"是否成功": True, "结果": "开始抬起左臂"}

@mcp.tool()
def 停止抬起左臂() -> dict:
    global stop_x
    stop_x = True
    print("\n\n已尝试松开 X 键（停止抬起左臂）\n")
    return {"是否成功": True, "结果": "停止抬起左臂"}

# 左Alt键
stop_alt = False

def 持续按下左alt():
    global stop_alt
    pyautogui.keyDown('altleft')
    print("已按下 左Alt 键并保持不松开（向下旋转左小臂中）")
    while not stop_alt:
        time.sleep(0.05)
    pyautogui.keyUp('altleft')
    print("已松开 左Alt 键（停止向下旋转左小臂）")

@mcp.tool()
def 开始向下旋转左小臂() -> dict:
    global stop_alt
    stop_alt = False
    threading.Thread(target=持续按下左alt, daemon=True).start()
    print("\n\n已尝试按下 左Alt 键并保持不松开（开始向下旋转左小臂）\n")
    return {"是否成功": True, "结果": "开始向下旋转左小臂"}

@mcp.tool()
def 停止向下旋转左小臂() -> dict:
    global stop_alt
    stop_alt = True
    print("\n\n已尝试松开 左Alt 键（停止向下旋转左小臂）\n")
    return {"是否成功": True, "结果": "停止向下旋转左小臂"}

# 左Shift键
stop_shift = False

def 持续按下左shift():
    global stop_shift
    pyautogui.keyDown('shiftleft')
    print("已按下 左Shift 键并保持不松开（向左旋转左大臂中）")
    while not stop_shift:
        time.sleep(0.05)
    pyautogui.keyUp('shiftleft')
    print("已松开 左Shift 键（停止向左旋转左大臂）")

@mcp.tool()
def 开始向左旋转左大臂() -> dict:
    global stop_shift
    stop_shift = False
    threading.Thread(target=持续按下左shift, daemon=True).start()
    print("\n\n已尝试按下 左Shift 键并保持不松开（开始向左旋转左大臂）\n")
    return {"是否成功": True, "结果": "开始向左旋转左大臂"}

@mcp.tool()
def 停止向左旋转左大臂() -> dict:
    global stop_shift
    stop_shift = True
    print("\n\n已尝试松开 左Shift 键（停止向左旋转左大臂）\n")
    return {"是否成功": True, "结果": "停止向左旋转左大臂"}

# Z 键
stop_z = False

def 持续按下z():
    global stop_z
    pyautogui.keyDown('z')
    print("已按下 Z 键并保持不松开（向右旋转左大臂中）")
    while not stop_z:
        time.sleep(0.05)
    pyautogui.keyUp('z')
    print("已松开 Z 键（停止向右旋转左大臂）")

@mcp.tool()
def 开始向右旋转左大臂() -> dict:
    global stop_z
    stop_z = False
    threading.Thread(target=持续按下z, daemon=True).start()
    print("\n\n已尝试按下 Z 键并保持不松开（开始向右旋转左大臂）\n")
    return {"是否成功": True, "结果": "开始向右旋转左大臂"}

@mcp.tool()
def 停止向右旋转左大臂() -> dict:
    global stop_z
    stop_z = True
    print("\n\n已尝试松开 Z 键（停止向右旋转左大臂）\n")
    return {"是否成功": True, "结果": "停止向右旋转左大臂"}


# Tab 键
stop_tab = False

def 持续按下tab():
    global stop_tab
    pyautogui.keyDown('tab')
    print("已按下 Tab 键并保持不松开（向前旋转左大臂中）")
    while not stop_tab:
        time.sleep(0.05)
    pyautogui.keyUp('tab')
    print("已松开 Tab 键（停止向前旋转左大臂）")

@mcp.tool()
def 开始向前旋转左大臂() -> dict:
    global stop_tab
    stop_tab = False
    threading.Thread(target=持续按下tab, daemon=True).start()
    print("\n\n已尝试按下 Tab 键并保持不松开（开始向前旋转左大臂）\n")
    return {"是否成功": True, "结果": "开始向前旋转左大臂"}

@mcp.tool()
def 停止向前旋转左大臂() -> dict:
    global stop_tab
    stop_tab = True
    print("\n\n已尝试松开 Tab 键（停止向前旋转左大臂）\n")
    return {"是否成功": True, "结果": "停止向前旋转左大臂"}

# CapsLock 键
stop_caps = False

def 持续按下capslock():
    global stop_caps
    pyautogui.keyDown('capslock')
    print("已按下 CapsLock 键并保持不松开（向后旋转左大臂中）")
    while not stop_caps:
        time.sleep(0.05)
    pyautogui.keyUp('capslock')
    print("已松开 CapsLock 键（停止向后旋转左大臂）")

@mcp.tool()
def 开始向后旋转左大臂() -> dict:
    global stop_caps
    stop_caps = False
    threading.Thread(target=持续按下capslock, daemon=True).start()
    print("\n\n已尝试按下 CapsLock 键并保持不松开（开始向后旋转左大臂）\n")
    return {"是否成功": True, "结果": "开始向后旋转左大臂"}

@mcp.tool()
def 停止向后旋转左大臂() -> dict:
    global stop_caps
    stop_caps = True
    print("\n\n已尝试松开 CapsLock 键（停止向后旋转左大臂）\n")
    return {"是否成功": True, "结果": "停止向后旋转左大臂"}




# ********************右臂**********************

# I 键
stop_i = False

def 持续按下i():
    global stop_i
    pyautogui.keyDown('i')
    print("已按下 I 键并保持不松开（抬起右臂中）")
    while not stop_i:
        time.sleep(0.05)
    pyautogui.keyUp('i')
    print("已松开 I 键（停止抬起右臂）")

@mcp.tool()
def 开始抬起右臂() -> dict:
    global stop_i
    stop_i = False
    threading.Thread(target=持续按下i, daemon=True).start()
    print("\n\n已尝试按下 I 键并保持不松开（开始抬起右臂）\n")
    return {"是否成功": True, "结果": "开始抬起右臂"}

@mcp.tool()
def 停止抬起右臂() -> dict:
    global stop_i
    stop_i = True
    print("\n\n已尝试松开 I 键（停止抬起右臂）\n")
    return {"是否成功": True, "结果": "停止抬起右臂"}

# K 键
stop_k = False

def 持续按下k():
    global stop_k
    pyautogui.keyDown('k')
    print("已按下 K 键并保持不松开（放下右臂中）")
    while not stop_k:
        time.sleep(0.05)
    pyautogui.keyUp('k')
    print("已松开 K 键（停止放下右臂）")

@mcp.tool()
def 开始放下右臂() -> dict:
    global stop_k
    stop_k = False
    threading.Thread(target=持续按下k, daemon=True).start()
    print("\n\n已尝试按下 K 键并保持不松开（开始放下右臂）\n")
    return {"是否成功": True, "结果": "开始放下右臂"}

@mcp.tool()
def 停止放下右臂() -> dict:
    global stop_k
    stop_k = True
    print("\n\n已尝试松开 K 键（停止放下右臂）\n")
    return {"是否成功": True, "结果": "停止放下右臂"}

# J 键
stop_j = False

def 持续按下j():
    global stop_j
    pyautogui.keyDown('j')
    print("已按下 J 键并保持不松开（向左转右臂中）")
    while not stop_j:
        time.sleep(0.05)
    pyautogui.keyUp('j')
    print("已松开 J 键（停止向左转右臂）")

@mcp.tool()
def 开始向左转右臂() -> dict:
    global stop_j
    stop_j = False
    threading.Thread(target=持续按下j, daemon=True).start()
    print("\n\n已尝试按下 J 键并保持不松开（开始向左转右臂）\n")
    return {"是否成功": True, "结果": "开始向左转右臂"}

@mcp.tool()
def 停止向左转右臂() -> dict:
    global stop_j
    stop_j = True
    print("\n\n已尝试松开 J 键（停止向左转右臂）\n")
    return {"是否成功": True, "结果": "停止向左转右臂"}

# L 键
stop_l = False

def 持续按下l():
    global stop_l
    pyautogui.keyDown('l')
    print("已按下 L 键并保持不松开（向右转右臂中）")
    while not stop_l:
        time.sleep(0.05)
    pyautogui.keyUp('l')
    print("已松开 L 键（停止向右转右臂）")

@mcp.tool()
def 开始向右转右臂() -> dict:
    global stop_l
    stop_l = False
    threading.Thread(target=持续按下l, daemon=True).start()
    print("\n\n已尝试按下 L 键并保持不松开（开始向右转右臂）\n")
    return {"是否成功": True, "结果": "开始向右转右臂"}

@mcp.tool()
def 停止向右转右臂() -> dict:
    global stop_l
    stop_l = True
    print("\n\n已尝试松开 L 键（停止向右转右臂）\n")
    return {"是否成功": True, "结果": "停止向右转右臂"}



# 回车键
stop_enter = False

def 持续按下回车():
    global stop_enter
    pyautogui.keyDown('enter')
    print("已按下 回车键并保持不松开（向前伸右臂中）")
    while not stop_enter:
        time.sleep(0.05)
    pyautogui.keyUp('enter')
    print("已松开 回车键（停止向前伸右臂）")

@mcp.tool()
def 开始向前伸右臂() -> dict:
    global stop_enter
    stop_enter = False
    threading.Thread(target=持续按下回车, daemon=True).start()
    print("\n\n已尝试按下 回车键并保持不松开（开始向前伸右臂）\n")
    return {"是否成功": True, "结果": "开始向前伸右臂"}

@mcp.tool()
def 停止向前伸右臂() -> dict:
    global stop_enter
    stop_enter = True
    print("\n\n已尝试松开 回车键（停止向前伸右臂）\n")
    return {"是否成功": True, "结果": "停止向前伸右臂"}

# 右Shift键
stop_rshift = False

def 持续按下右shift():
    global stop_rshift
    pyautogui.keyDown('shiftright')
    print("已按下 右Shift 键并保持不松开（向后转右大臂中）")
    while not stop_rshift:
        time.sleep(0.05)
    pyautogui.keyUp('shiftright')
    print("已松开 右Shift 键（停止向后转右大臂）")

@mcp.tool()
def 开始向后转右大臂() -> dict:
    global stop_rshift
    stop_rshift = False
    threading.Thread(target=持续按下右shift, daemon=True).start()
    print("\n\n已尝试按下 右Shift 键并保持不松开（开始向后转右大臂）\n")
    return {"是否成功": True, "结果": "开始向后转右大臂"}

@mcp.tool()
def 停止向后转右大臂() -> dict:
    global stop_rshift
    stop_rshift = True
    print("\n\n已尝试松开 右Shift 键（停止向后转右大臂）\n")
    return {"是否成功": True, "结果": "停止向后转右大臂"}



# ****************空格键*************
stop_space = False

def 持续按下space():
    global stop_space
    pyautogui.keyDown('space')
    print("已按下空格键并保持不松开（重置双臂中）")
    while not stop_space:
        time.sleep(0.05)
    pyautogui.keyUp('space')
    print("已松开空格键（停止重置双臂）")

@mcp.tool()
def 开始重置双臂() -> dict:
    global stop_space
    stop_space = False
    threading.Thread(target=持续按下space, daemon=True).start()
    print("\n\n已尝试按下空格键并保持不松开（开始重置双臂）\n")
    return {"是否成功": True, "结果": "开始重置双臂"}

@mcp.tool()
def 停止重置双臂() -> dict:
    global stop_space
    stop_space = True
    print("\n\n已尝试松开空格键（停止重置双臂）\n")
    return {"是否成功": True, "结果": "停止重置双臂"}




















# -------------------------------------------------------------------------------------------------


# 控制电脑相关功能列表（第一部分）
控制电脑功能 = [
    "1.运行电脑端程序 预设软件 或 具体路径",
    "2.在电脑上打开URL网址 网页名 或 具体URL网址",
    "3.在电脑上运行CMD指令 预设指令 或 具体指令",
    "4.官方的计算器示例",
    "5.创建文件写入内容",
    "6.读取复制内容",
    "7.填入一段内容",
    "8.回车发送",
    "9.撤销操作",
    "10.锁定电脑",
    "11.电脑关机计划",
    "12.设置电脑音量",
    "13.调用系统截图工具",
    "14.显示桌面",
    "15.查看系统资源使用情况",
    "16.查看电脑配置信息 & 获取桌面完整路径",
    "17.设置Windows系统深浅色主题",
    "18.更换桌面壁纸"

]

# AI 平台相关的 API 能力列表（第二部分）
API功能 = [
    "\n支持的 开放_API-能力：\n",
    "1.获取心灵毒鸡汤",
    "2.获取抖音热点",
    "3.获取随机一言",
    "4.获取舔狗日记",
    "5.获取星座运势",
    "6.运势抽签",
    "7.获取百度知乎微博实时热榜",
    "8.获取名人名言",
    "9.获取每日一句",
    "10.获取绕口令",
    "11.查询油价",
    "12.获取新年祝福语",
    "13.获取今日电影票房",
    "14.获取脑筋急转弯",
    "15.每日早报",
    "16.今天吃什么",
    "17.搜索百度百科",
    "18.获取历史上的今天",
    "19.获取万年历",
    "20.获取深证成指",
    "21.查询个股行情",
    "22.查询公司基本面",
    "23.查询高铁票",
    "24.获取回声洞",
    "25.推送巴法消息"
]

# 动态插入工具功能到第一部分
if 允许使用微信发消息工具:
    控制电脑功能.append("19.向微信指定联系人发送内容")


# 动态插入工具功能
if 是作者工作环境:

    API功能.append("26.获取房间环境温湿度！")

    # 控制洛雪音乐工具列表（第3部分）

# 动态插入工具功能
if 使用控制洛雪音乐工具:

    洛雪音乐控制 = [
        "\n支持的 洛雪音乐控制工具：\n",
        "1.洛雪音乐_搜索并播放音",
        "2.洛雪音乐_暂停或继续播放音乐",  
        "3.洛雪音乐_下一首音乐",
        "4.洛雪音乐_上一首音乐",
    ]

    # 动态插入工具功能
    if 是作者工作环境:

        洛雪音乐控制.append("5.洛雪音乐_播放收藏列表")


# 动态插入工具功能
if 使用控制洛雪音乐工具:

    # 将两部分功能列表合并
    功能内容 = 控制电脑功能 + API功能 + 洛雪音乐控制

else:
    功能内容 = 控制电脑功能 + API功能
    

# 主程序入口
if __name__ == "__main__":
    logger.info("\n\n\tMCP_Windows服务已启动！等待调用！\n\n当前支持的控制电脑工具：\n" + "\n".join(功能内容) + "\n\n快尝试小智的能力吧！\n")
    logger.info("\n\n\t\b版本：v48.56.31 (2025-08-18 更新)\n\t\tBy[粽子同学]\n\n")
    mcp.run(transport="stdio")
# -------------------------------------------------------------------------------------------------



