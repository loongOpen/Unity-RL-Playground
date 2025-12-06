# Gewu Playground for Embodied Intelligence

# <img src="gewu/Image/gwlogo3.jpg" height="150" /> 

<div align="center">

| <div align="center"> [gewu2.0](https://www.bilibili.com/video/BV1iJbSz9Eem/) </div> | <div align="center">  [Embodied AI Town](https://www.bilibili.com/video/BV1T7jBzVEZV/) </div> |
| ---  | --- |
| <img src="gewu/Image/Gewu2.0.gif" width="400px"> | <img src="gewu/Image/hetu.gif" width="400px"> |

</div>

<div align="center">
  
| <div align="center"> [Robot Rally](https://www.bilibili.com/video/BV167RbYxEuG/) </div> | <div align="center">  [Rough-Terrain Locomotion](https://www.bilibili.com/video/BV1ZDjGz9EJH/) </div> |  <div align="center"> Autonomous Navigation </div> |  <div align="center"> [Robot Animation](https://www.bilibili.com/video/BV1maEPzQEtD/?vd_source=25bf190003ff1ebd36e7649d3641e141) </div> |
| ---  | --- | --- | --- |
| <img src="gewu/Assets/Menu/image/1.gif" width="180px"> | <img src="gewu/Image/terrain.gif" width="180px"> | <img src="gewu/Assets/Menu/7/7.gif" width="180px"> | <img src="gewu/Image/G1perform.gif" width="180px"> |

</div>

<div align="center">
  
| <div align="center"> [Kung Fu Football](https://www.bilibili.com/video/BV1NuZBYeEq8/) </div> | <div align="center">  [Imitation Learning](https://www.bilibili.com/video/BV1RmKhzVEbS/) </div> |<div align="center"> [Sim2Real](https://b23.tv/y8wXu2N) </div> | <div align="center">  [Mobile Manipulation](https://b23.tv/8htcqZ7) </div> |
| ---  | --- | ---  | --- |
| <img src="gewu/Image/loong-kungfu.gif" width="180px"> | <img src="gewu/Image/g1mimic.gif" width="180px"> |<img src="gewu/Image/s2r.gif" width="180px"> | <img src="gewu/Image/teleop.gif" width="180px"> |

</div>

---

<div id="top" align="left">

[![arxiv](https://img.shields.io/badge/arXiv%202407.10943-red?logo=arxiv)](https://arxiv.org/abs/2503.05146)
[![arxiv](https://img.shields.io/badge/arXiv%202407.10943-red?logo=arxiv)](https://arxiv.org/abs/2309.09167)
[![github](https://img.shields.io/badge/Project-0065D3?logo=rocket&logoColor=white)](https://github.com/loongOpen/Unity-RL-Playground)
[![video-cn](https://img.shields.io/badge/Bilibili-00A1D6?logo=bilibili&logoColor=white)](https://space.bilibili.com/382126997?spm_id_from=333.337.0.0)
<a href="gewu/Image/gewu-wechat.png"><img src="https://img.shields.io/badge/WeChat-07C160?style=for-the-badge&logo=wechat&logoColor=white" height="20" style="display:inline"></a>
</div>
Gewu is an embodied intelligence simulation and training platform jointly developed by the National Local Co-constructed Humanoid Robot Innovation Center, Shanghai University, and Tsinghua University. Leveraging the Unity ML-Agents toolkit, the platform is designed to offer researchers a high-performance and user-friendly environment for reinforcement learning, supporting a wide range of robotic systems.[<a href="gewu/Image/gewu-wechat.png">格物平台微信交流群</a>]

## 🔥 News
- **[2025.8.14]: Gewu 2.0 (Gewu Playground 2.0) Code Release**

  Utilizes Unity 2022 (compatible with Tuanjie Engine), integrates a main interface UI, covers locomotion, navigation, and manipulation tasks, and offers an [install-free version](https://pan.baidu.com/s/1HFQvqdZcPr0HI1gCn6BGFg?pwd=adnq) for qiuck experience.

- **[2025.7.17] Added ROS2 plugin and Sim2Real example (Go2)**
- **[2025.7.01] Added robot animation examples**
- **[2025.6.29] Added general-purpose humanoid mobile manipulation examples**
- **[2025.6.23] Added imitation learning examples (robots can learn to dance)**
- **[2025.5.28] Released Robot Playground and “Along the River During the Qingming Festival” scenario**
- **[2025.5.25] Added challenging terrain scenarios**
- **[2025.4.19] Added action remapping and quadruped examples**
- **[2025.4.04] GeWu 1.0 (Gewu Playground 1.0) Released**

  Upgraded to Unity 2023, with pre-installed dependencies, optimized code, and a new robot soccer example.

- **[2025.3.20] GeWu 0.1 (Unity RL Playground) Released**

  Built with Unity 2021, distributed as a UnityPackage, featuring the "All Robots Together" demo.

## 📖 Related Publications

**For more technical details or to cite this platform in your research, please refer to the following papers:**

[1] Ye, Linqi, Rankun Li, Xiaowen Hu, Jiayi Li, Boyang Xing, Yan Peng, and Bin Liang. "Unity RL Playground: A Versatile Reinforcement Learning Framework for Mobile Robots." arXiv preprint arXiv:2503.05146 (2025). [PDF](https://arxiv.org/abs/2503.05146)

[2] Ye, Linqi, Jiayi Li, Yi Cheng, Xianhao Wang, Bin Liang, and Yan Peng. "From knowing to doing: learning diverse motor skills through instruction learning." arXiv preprint arXiv:2309.09167 (2023).[PDF](https://arxiv.org/abs/2309.09167) 

## ⚙️ System Requirements

Supported operating systems: Windows, Linux, MacOS，[Mac-Silicon Notes](macos-setup-Silicon.md)，[Mac-Intelcore Notes](macos-setup-Intelcore.md)

## 📚 Getting Started

### I.Simulation Environment Setup

1. Install Unity Hub and sign in. In the Install Unity Editor popup, click skip, then Agree and get personal edition license for free activation.

2. In Unity Hub, open the Installs tab → Install Editor → choose Unity Editor 2022 (2022.3) (~7 GB, requires patience).

3. Download Unity RL Playground：https://github.com/loongOpen/Unity-RL-Playground and unzip it locally.

4. In Unity Hub, go to Projects → Open, then select the Unity-RL-Playground/gewu folder. The first load may take a while; if prompted, choose ignore or continue.

5. Once the project is open, find GewuMenu.unity under Assets and double-click to load the main menu. Click the Play button (triangle) in Unity to run, and navigate through the 8 functional modules.

6. To record videos: go to Window → General → Recorder → Recorder Window, click Add Recorder → Movie, then hit the red triangle button to start recording. Files are saved in the path specified under Path.

Each demo scene can also be opened directly without using the main menu.

### II.Training Environment Setup

1. Install Anaconda：https://www.anaconda.com/download

2. Open Anaconda Prompt from the start menu.

3. Create a new environment:
  
     `conda create -n gewu python=3.10.12 -y`

4. Activate the environment:

      `conda activate gewu`

5. Install PyTorch (with CUDA 12.1 support):

      `pip3 install torch~=2.2.1 --index-url https://download.pytorch.org/whl/cu121`

   
（Make sure the network is unobstructed. It will take a long time, so please wait patiently. If the installation fails, try another network.）


6. Install ML-Agents:

      `python -m pip install mlagents==1.1.0`

    

7. Verify installation：
      `mlagents-learn --help`

### III. Training Robots

Example: Robot Assembly Demo(Assets/Playground/Playground.unity)：

1. Select a robot (e.g., Go2) in the Unity Inspector panel and enable Train.

2. Hide all other robots (uncheck the top checkbox in Inspector).

3. In Anaconda, navigate to the project directory, e.g.:
```bash
  D:
  cd D:\Unity-RL-Playground-main\gewu\Assets\Playground
```
4. Start training：
```bash
   mlagents-learn config.yaml --run-id=go2trot --force
```
   
5. When [INFO] Listening on ... appears, return to Unity and click Play.

6. Training progress (reward curves, loss, etc.) can be monitored in the Anaconda window. Typically, training takes ~2,000,000 steps. Stop with Ctrl+C.

7. Trained models are saved in results/go2trot/ as .onnx files.
8. Assign the trained .onnx model to the robot’s Policy field in Unity Inspector.

9. Uncheck Train, run Unity, and observe the trained behaviors.

10. Similarly, you can train for TinkerTrain.unity and LoongTrain.unity, then test in TinkerPlay.unity and LoongPlay.unity.

### IV. Importing and Training New Robots

1. Place robot URDF files under Assets/urdf/.

   - Typically includes xx.urdf and a meshes/ folder. 
    
   - Paths inside .urdf should use package://meshes/xxx.STL.
    
   - Lock joints outside the legs if possible.


2. Open the empty template scene MyRobot.unity.     

3. Import URDF: select .urdf file → Assets → Import Robot from Selected URDF.(In the popup, choose unity as the mesh decomposer.)

5. Adjust the imported robot’s height (Y-axis) so feet touch the ground.

6. Remove unnecessary scripts (Urdf Robot (script) and Controller (script)).

7. Drag the robot into the MyRobot hierarchy.

8. Configure parameters in Inspector:

   - RobotType (e.g., Biped).

   - Target Motion (default Walk for bipeds).

   - Behaviour Parameters (observation/action dimensions).

9. Test feedforward motion: check Fixbody → run Unity → robot should step in place.

   - If mismatched, adjust feedforward mapping in GewuAgent.cs (search for "change here").


10. Once stepping motion is correct, train with ML-Agents as in Section IV.

   - Example: Biped robots often require only ~400k steps (2–5 minutes).

**👉 For more URDF models,check:https://github.com/linqi-ye/robot-universe**

## 🪜 Example Scenarios
### Robot Assembly Demo
Path: Assets/Playground/Playground.unity

Run pre-trained motions of multiple robots.

Select a robot in the Inspector → switch its motion mode via Target Motion dropdown.

### Challenging Terrain

Path: Assets/Playground/Terrain.unity

Features 30cm-long, 15cm-high steps.

Pre-trained robots: Loong, Unitree G1, T1, SA01.

Use curriculum learning by gradually increasing stair height (adjust Stairs.Scale.y).

Only train one robot at a time; hide the others.

### Robot Soccer

Path: Assets/Playground/TinkerPlay.unity (two-player mode)

Player 1: WASD for walking, Left Ctrl to reset robot.

Player 2: Arrow keys for walking, Right Ctrl to reset robot.

Spacebar resets the ball.

Path: Assets/Playground/LoongPlay.unity (auto-battle mode)

Loong “Kungfu Soccer” with automatic play.

Spacebar resets the ball if stuck.

### Action Remapping & Imitation Learning

Path: Assets/Imitation/G1.unity

Includes Unitree H1 and G1 robots.

Pre-trained motions: guitar, golf, violin, waving (H1) + Charleston dance (G1).

Motion data stored in dataset/ (H1 from AMASS, G1 from LEFAN1).

Training: check Train;

Replay: check Replay.

Motion_id controls which motion is played.

### Mobile Manipulation

Path: Assets/Manipulation/G1OP.unity

Keyboard-controlled locomotion and manipulation tasks.

### Robot Animation

Path: Assets/Animation/dance.unity

Pre-trained animations: dancing, playing piano, singing.

More available under Assets/Animation/Animations/.

### Sim2Real 

Path: Assets/Ros2ForUnity/Go2/Go2Deploy.unity

Supported OS: Ubuntu 20.04 (foxy branch) or Ubuntu 22.04 (main branch).

Communication and policy deployment with the Unitree Go2 robot via ROS2.

Requirements:

Install ROS2 ([Ubuntu 20.04 guide] / [Ubuntu 22.04 guide]).

Install Unitree_ROS2.

#### ROS2 Environment Setup

Add the following lines to your ~/.bashrc (modify paths as needed for your setup):
```bash
source ~/ylq/unitree_ros2/setup.sh
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/home/ylq/Unity-RL-Playground/gewu/Assets/Ros2ForUnity/Plugins/Linux/x86_64

```
#### Deployment Instructions

Open Go2Deploy.unity in Unity.

In the left Hierarchy, select Go2Real.

In the right Inspector, check is_ros2_installed.

Connect your PC to the robot with an Ethernet cable.

Power on the Go2, connect to it with the Unitree app, and make the robot lie down.

In the app, go to Device → Service Mode, then click MCF to disable the main control function (wait a moment after clicking).

#### Running the Demo

Before starting: ensure the robot is lying down and MCF is disabled.

When running the scene:

The robot will slightly stand up.

Click Stand Up until it fully stands.

Enable FF_enable (feedforward control) → the robot begins stepping.

Enable NN_enable (neural network control) → now you can control walking and turning via WASD keys.

To stop:

Disable FF_enable and NN_enable.

Click Lie Down to safely stop the robot.

#### Training Scene

Path: Assets/Ros2ForUnity/Go2/Go2Train.unity

Used for training new policies for the Go2 robot.
### Autonomous Navigation

Path: Assets/Navigation/Scene/Go2Navi.unity

Click any target point in the Unity scene.

Uses Unity’s AI Navigation plugin for path planning.

Executes navigation with a pre-trained omnidirectional locomotion policy.



