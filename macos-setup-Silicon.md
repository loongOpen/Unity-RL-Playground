
---

## 1. 环境信息

- **硬件**：Apple MacBook Pro 14寸，Apple M1 Pro 芯片，16GB 内存
- **操作系统**：macOS Sequoia 15.1
- **Unity 版本**：2023.2.20f1
- **ML-Agents（Unity端）**：com.unity.ml-agents 3.0.0（通过 Unity Package Manager 官方源安装）
- **Python 版本**：3.8.x（x86_64 架构，需通过 Rosetta 终端运行）
- **mlagents（Python端）**：0.28.0
- **PyTorch**：1.8.0
- **Miniforge/Miniconda**：x86_64 版本

---

## 2. 安装与配置步骤

### 2.1 安装 x86_64 版 Miniforge3（必须用 Rosetta 终端）

1. **用 Rosetta 打开终端**  
   Finder > 应用程序 > 实用工具 > 右键"终端" > 显示简介 > 勾选"使用 Rosetta 打开"

2. **下载 x86_64 版 Miniforge3 安装包**  
   `https://github.com/conda-forge/miniforge/releases/download/23.11.0-0/Miniforge3-MacOSX-x86_64.sh`

3. **安装 Miniforge3**
   ```bash
   cd ~/Downloads
   bash Miniforge3-MacOSX-x86_64.sh
   ```

4. **安装完成后，重启 Rosetta 终端**

5. **验证 Python 架构**
   ```bash
   python -c "import platform; print(platform.machine())"
   ```
   输出应为 `x86_64`

---

### 2.2 创建 Python 3.8 虚拟环境

```bash
conda create -n mlagents-028 python=3.8
conda activate mlagents-028
```

---

### 2.3 安装依赖包（严格指定版本）

```bash
conda install pytorch==1.8.0 torchvision==0.9.0 cpuonly -c pytorch
pip install mlagents==0.28.0 mlagents-envs==0.28.0
pip install protobuf==3.20.3
pip install numpy==1.19.5
pip install six
```

---

### 2.4 Unity 端 ML-Agents 安装

1. 打开 Unity 项目
2. 打开 Package Manager（菜单栏 Window > Package Manager）
3. 切换到 Unity Registry，搜索 `ml-agents`
4. 选择并安装 `com.unity.ml-agents` 3.0.0 版本  
   - 如未显示，点击左上角"+" > "Add package by name..."
   - Name: `com.unity.ml-agents`
   - Version: `3.0.0`
5. 确认 ML-Agents 3.0.0 安装成功，且不是 local 版本

---

### 2.5 Unity 场景配置

- Behavior Parameters 组件
  - Behavior Name 与 `trainer_config.yaml` 配置文件中的 key 完全一致（区分大小写）
  - Behavior Type 设为 `Default`
- 场景可正常运行，Agent 能运动，无报错

---

### 2.6 训练配置文件

- 在 Unity 项目目录下准备好 `trainer_config.yaml`，内容示例：

  ```yaml
  gewu:
    trainer_type: ppo
    max_steps: 20000000
    batch_size: 2048
    buffer_size: 20480
    learning_rate: 0.0003
    network_settings:
      hidden_units: 512
      num_layers: 3
  ```

---

### 2.7 启动训练

1. Unity 编辑器中点击 Play，保持场景运行
2. 在终端（已激活 mlagents-028 环境，且在项目目录下）运行：

   ```bash
   mlagents-learn trainer_config.yaml --run-id=run1 --force
   ```

3. 观察终端和 Unity Console 日志，确认训练正常进行

---

### 2.8 训练中断与恢复

- 训练过程中可随时 Ctrl+C 停止，ML-Agents 会自动保存 checkpoint
- 恢复训练时，使用相同的 `--run-id`，ML-Agents 会自动从最近的 checkpoint 继续

---

## 3. 参考命令汇总

```bash
# 用 Rosetta 终端
bash Miniforge3-MacOSX-x86_64.sh

# 创建并激活环境
conda create -n mlagents-028 python=3.8
conda activate mlagents-028

# 安装依赖
conda install pytorch==1.8.0 torchvision==0.9.0 cpuonly -c pytorch
pip install mlagents==0.28.0 mlagents-envs==0.28.0
pip install protobuf==3.20.3
pip install numpy==1.19.5
pip install six

# 启动训练
mlagents-learn trainer_config.yaml --run-id=run1 --force
```

---

## 4. 重要说明

- 所有命令均需在 Rosetta 终端下执行，确保 x86_64 架构
- Unity 端 ML-Agents 必须用官方源安装的 3.0.0 版本，切勿用 local 包
- Python 端所有依赖需严格指定版本，避免兼容性问题

---


## 1. Environment Information

- **Hardware**:  Apple M1 Pro
- **Unity Version**: 2023.2.20f1
- **ML-Agents (Unity side)**: com.unity.ml-agents 3.0.0 (installed via Unity Package Manager Registry)
- **Python Version**: 3.8.x (x86_64 architecture, must run via Rosetta Terminal)
- **mlagents (Python side)**: 0.28.0
- **PyTorch**: 1.8.0
- **Miniforge/Miniconda**: x86_64 version

---

## 2. Installation & Configuration Steps

### 2.1 Install x86_64 Miniforge3 (Must use Rosetta Terminal)

1. **Open Terminal with Rosetta**  
   Finder > Applications > Utilities > Right-click "Terminal" > Get Info > Check "Open using Rosetta"

2. **Download x86_64 Miniforge3 installer**  
   `https://github.com/conda-forge/miniforge/releases/download/23.11.0-0/Miniforge3-MacOSX-x86_64.sh`

3. **Install Miniforge3**
   ```bash
   cd ~/Downloads
   bash Miniforge3-MacOSX-x86_64.sh
   ```

4. **After installation, restart Rosetta Terminal**

5. **Verify Python architecture**
   ```bash
   python -c "import platform; print(platform.machine())"
   ```
   Output should be `x86_64`

---

### 2.2 Create Python 3.8 Virtual Environment

```bash
conda create -n mlagents-028 python=3.8
conda activate mlagents-028
```

---

### 2.3 Install Dependencies (Strict Version)

```bash
conda install pytorch==1.8.0 torchvision==0.9.0 cpuonly -c pytorch
pip install mlagents==0.28.0 mlagents-envs==0.28.0
pip install protobuf==3.20.3
pip install numpy==1.19.5
pip install six
```

---

### 2.4 Install ML-Agents on Unity Side

1. Open your Unity project
2. Open Package Manager (Window > Package Manager)
3. Switch to Unity Registry, search for `ml-agents`
4. Select and install `com.unity.ml-agents` version 3.0.0  
   - If not listed, click "+" > "Add package by name..."
   - Name: `com.unity.ml-agents`
   - Version: `3.0.0`
5. Ensure ML-Agents 3.0.0 is installed from Registry, not as a local package

---

### 2.5 Unity Scene Configuration

- **Behavior Parameters Component**
  - Behavior Name must exactly match the key in `trainer_config.yaml` (case-sensitive)
  - Set Behavior Type to `Default`
- Scene should run without errors, Agent should move as expected

---

### 2.6 Training Config File

- Place `trainer_config.yaml` in your Unity project directory, for example:

  ```yaml
  gewu:
    trainer_type: ppo
    max_steps: 20000000
    batch_size: 2048
    buffer_size: 20480
    learning_rate: 0.0003
    network_settings:
      hidden_units: 512
      num_layers: 3
  ```

---

### 2.7 Start Training

1. Click Play in Unity Editor, keep scene running
2. In terminal (mlagents-028 env activated, in project dir), run:

   ```bash
   mlagents-learn trainer_config.yaml --run-id=run1 --force
   ```

3. Monitor terminal and Unity Console logs to ensure training is running

---

### 2.8 Pause and Resume Training

- You can stop training anytime with Ctrl+C, ML-Agents will auto-save checkpoints.
- To resume, use the same `--run-id`, ML-Agents will continue from the latest checkpoint.

---

## 3. Command Summary

```bash
# Use Rosetta Terminal
bash Miniforge3-MacOSX-x86_64.sh

# Create and activate environment
conda create -n mlagents-028 python=3.8
conda activate mlagents-028

# Install dependencies
conda install pytorch==1.8.0 torchvision==0.9.0 cpuonly -c pytorch
pip install mlagents==0.28.0 mlagents-envs==0.28.0
pip install protobuf==3.20.3
pip install numpy==1.19.5
pip install six

# Start training
mlagents-learn trainer_config.yaml --run-id=run1 --force
```

---

## 4. Important Notes

- All commands must be run in Rosetta Terminal to ensure x86_64 architecture
- ML-Agents in Unity must be installed from the official Registry, not as a local package
- All Python dependencies must be strictly versioned to avoid compatibility issues

---
