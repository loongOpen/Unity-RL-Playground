# Unity RL Playground - MuJoCo Sim
本项目用于开发基于 **Unity + MuJoCo 物理引擎** 的机器人强化学习 **sim2sim 仿真环境**，支持人形机器人在复杂工业场景中的双足行走强化学习策略验证以及上肢操作与下肢移动协同控制。
> 🔗 本项目需配合 [`loongOpen/loong_sim_sdk_release`](https://github.com/loongOpen/loong_sim_sdk_release) 使用。
---
## 📚 目录
- [1. 当前场景](#1-当前场景-rxjqr_gjdssxtzpjg_v2)
- [2. 环境依赖](#2-环境依赖)
- [3. 操作流程详解](#3-操作流程详解)
---
## 1. 当前场景：RXJQR_GJDSSXTZPJG_V2
**名称**：人形机器人工业训练场  
**类型**：模块化工业厂区仿真环境  
**核心资产**：
- `untitled.usd`：主场景几何模型（USD 格式，适用于 Unity 2021+）
---
## 2. 环境依赖
请确保以下包已正确安装并配置：
| Package | 版本要求 | 说明 |
|--------|----------|------|
| MuJoCo | `3.3.3` | 启用高保真物理仿真 |
| USD Core & Importer | `1.0.0` | 支持 `.usd` 场景导入 |

---
## 3. 操作流程详解
操作前检查：打开 Unity 项目，确认 Console 无报错（特别是 USD 导入或 MuJoCo 组件初始化异常）。依次执行以下步骤：

1️⃣ 启动后台控制脚本（在 `loong_sim_sdk_release/tools/` 目录下运行）<br>
**./run_driver.sh**          # 启动驱动，打印关节信息（等待格物端数据接通）<br>
**./run_interface.sh**       # 启动命令通信服务 <br>
**./run_locomotion.sh**      # 加载腿部运动控制器 <br>
**./run_manipulation.sh**    # 加载手臂操作控制器 <br>
**python3 py_ui.py**         # 打开图形化 UI 控制界面

2️⃣ 启动 Unity 仿真端<br>
打开 Unity 项目并加载 MujocoSim 场景，点击 Play 按钮，机器人进入 **等待指令状态**

3️⃣ 初始化机器人状态<br>
在 UI 控制界面 点击 en（上使能），后点击 rc（复位），此时机器人应进入 **复位状态**

4️⃣ 放置机器人至地面，并实现腿足行走和手臂操纵<br>
- Display1 为第一人称可交互窗口，左下角显示机器人操作说明及实时状态
- 按G将机器人放至脚底接触地面，再按F或取消Assist Flag勾选，此时机器人不再有外力施加，若稳定站立则可进行下一步，否则按H抬升后进行dis（下使能）再重新en（上使能）并rc（复位）放下，直到机器人可平稳站立。
- 随后UI控制界面点击rl（强化学习行走策略），此时为策略驱动的平衡姿态，按下q可踏步，e停止，wasdjl模拟遥杆增量，空格清零，UI另可进行手臂操作，手臂与双腿互不影响，可同时接受指令；点击格物界面的场景地面可实现旁观者移动，ZC为左右转向。
---
