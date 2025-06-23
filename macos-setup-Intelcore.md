# Unity-RL-Playground 仿真环境安装与配置说明（macOS）

## 一、环境与前提条件

- **操作系统**：macOS Monterey 12.6.8
- **硬件**：MacBook Pro (Intel Core i9)
- **Unity 版本**：2023.2.20f1
- **Python 版本**：推荐 3.8.x
- **ML-Agents 版本**：Unity端 3.0.0（Registry），Python端 0.28.0
- **终端环境**：/bin/zsh
- **包管理工具**：conda（推荐使用 Miniconda 或 Anaconda）

---

## 二、安装与配置步骤

### 1. 安装 Homebrew（如未安装）

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

### 2. 安装 Miniconda

```bash
brew install --cask miniconda
```
安装后，重启终端，并执行：
```bash
conda init zsh
source ~/.zshrc
```

### 3. 创建 Python 虚拟环境

```bash
conda create -n mlagents38 python=3.8 -y
conda activate mlagents38
```

### 4. 安装 Python 依赖

```bash
pip install mlagents==0.28.0
pip install protobuf==3.20.3 numpy six
```

### 5. 安装 Unity Hub 与 Unity 编辑器

- 下载并安装 [Unity Hub](https://unity3d.com/get-unity/download)
- 通过 Unity Hub 安装 Unity 2023.2.20f1 版本
- 在 Unity Package Manager 中安装 ML-Agents（Registry 版本 3.0.0）

---

# Unity-RL-Playground Simulation Environment Setup Guide (macOS)

## 1. Environment and Prerequisites

- **Operating System**: macOS Monterey 12.6.8
- **Hardware**: MacBook Pro (Intel Core i9)
- **Unity Version**: 2023.2.20f1
- **Python Version**: Recommended 3.8.x
- **ML-Agents Version**: Unity side 3.0.0 (Registry), Python side 0.28.0
- **Shell**: /bin/zsh
- **Package Manager**: conda (Miniconda or Anaconda recommended)

---

## 2. Installation and Configuration Steps

### 1. Install Homebrew (if not already installed)

```bash
/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
```

### 2. Install Miniconda

```bash
brew install --cask miniconda
```
After installation, restart your terminal and run:
```bash
conda init zsh
source ~/.zshrc
```

### 3. Create a Python Virtual Environment

```bash
conda create -n mlagents38 python=3.8 -y
conda activate mlagents38
```

### 4. Install Python Dependencies

```bash
pip install mlagents==0.28.0
pip install protobuf==3.20.3 numpy six
```

### 5. Install Unity Hub and Unity Editor

- Download and install [Unity Hub](https://unity3d.com/get-unity/download)
- Use Unity Hub to install Unity 2023.2.20f1
- Install ML-Agents (Registry version 3.0.0) via Unity Package Manager

--- 