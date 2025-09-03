# Gewu Playground for Embodied Intelligence ([English](README_en.md) | [中文](README.md))

# <img src="gewu/Image/gwlogo3.jpg" height="150" /> 

<div align="center">

| <div align="center"> [Gewu 2.0](https://www.bilibili.com/video/BV1iJbSz9Eem/) </div> | <div align="center">  [Embodied AI Town](https://www.bilibili.com/video/BV1T7jBzVEZV/) </div> |
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

"Gewu" is an embodied AI simulation training platform jointly launched by the National Local Co-constructed Humanoid Robot Innovation Center, Shanghai University, and Tsinghua University. Built on the Unity ML-Agents toolkit, this project aims to provide researchers and the general public with an efficient and user-friendly reinforcement learning development environment suitable for various types of robots. [Gewu Platform WeChat Discussion Group]

August 14, 2025: Gewu 2.0 (Gewu Playground 2.0) Code Release
Utilizes Unity 2022 (compatible with the Unity China Engine), integrates a main interface UI, covers locomotion, navigation, and manipulation tasks, and offers an install-free version for trial.

July 17, 2025: Added ROS2 Plugin and Sim2Real Example (Go2)
July 1, 2025: Added Robot Animation Example
June 29, 2025: Added General Humanoid Robot Mobile Manipulation Example
June 23, 2025: Added Imitation Learning Example, enabling robots to learn dancing
May 28, 2025: Launched Robot Amusement Park and "Along the River During the Qingming Festival" scenario
May 25, 2025: Added Complex Terrain Example
April 19, 2025: Added Motion Remapping Example and Quadrupedal Example
April 4, 2025: Gewu 1.0 (Gewu Playground 1.0) Released, Comprehensive Upgrade
Upgraded to Unity 2023, pre-installed dependencies, optimized code, and added a football match example.

March 20, 2025: Gewu 0.1 (Unity RL Playground) Code Release
Built with Unity 2021, packaged as a UnityPackage, featuring the "Robot Rally" example.

Related Papers
For more technical details or to cite this platform in your research, please refer to the following papers:

[1] Ye, Linqi, Rankun Li, Xiaowen Hu, Jiayi Li, Boyang Xing, Yan Peng, and Bin Liang. "Unity RL Playground: A Versatile Reinforcement Learning Framework for Mobile Robots." arXiv preprint arXiv:2503.05146 (2025). PDF

[2] Ye, Linqi, Jiayi Li, Yi Cheng, Xianhao Wang, Bin Liang, and Yan Peng. "From knowing to doing: learning diverse motor skills through instruction learning." arXiv preprint arXiv:2309.09167 (2023).PDF

Usage Instructions
Compatible with Windows, Linux, macOS, and other operating systems. Mac-Silicon Supplementary Instructions, Mac-Intel Core Supplementary Instructions

I. Simulation Environment Installation
Search for and install Unity Hub, register and log in. When the "Install Unity Editor" window appears, click "skip," then click "Agree and get personal edition license" for free activation.
In the opened Unity Hub interface, click "Install Editor" in the "Installs" menu. Select Unity Editor version 2022 (2022.3) for installation (over 7GB, be patient).
Download Unity RL Playground: https://github.com/loongOpen/Unity-RL-Playground, and unzip it to a local directory.
In the "Projects" menu of Unity Hub, click "Open," select the "Unity-RL-Playground\gewu" directory unzipped in the previous step, click "Open," and wait for the project to open (the first opening may take a long time, be patient). If a pop-up window appears, choose "ignore" or "continue."
After the project opens, you can see the "GewuMenu.unity" file under the "Assets" directory in the small window at the bottom of Unity. Double-click to open it, and the main interface of Gewu will be displayed. Click the triangle above Unity to run it, and you can sequentially enter the eight functional modules.
To record videos, go to the menu bar: Window -> General -> Recorder -> Recorder Window, click "Add Recorder" -> "Movie," and click the red triangle to start recording. The saved path can be found under "Path" below.
You can also open each example without going through the main interface:

Robot Rally Example
In the "Assets/Playground" directory, click to enter it, double-click "Playground.unity" to open it, and click the triangle above Unity to run it. You will see the pre-trained motion effects of the robots.

Select a robot, and in the right "inspector" window, you can switch the motion mode in the corresponding "target motion" drop-down box (if the corresponding pre-trained model is not empty).

Complex Terrain Example
In the "Assets/Playground" directory, "Terrain.unity"

Features a 30cm long and 15cm high step, pre-trained for Qinglong, Unitree G1, Acceleration Evolution T1, and Zhongqing SA01.

Note: Train each robot separately and hide the others during training.

Use curriculum learning: gradually increase the stair height during training (adjust the "y" value of the "Scale" of the "Stairs").

Robot Football Match Example
In the "Assets/Playground" directory, double-click "TinkerPlay.unity" to open it for the Tinker football match, which is set to a two-player mode by default. One player controls the robot's walking direction using the WASD keys on the keyboard and resets the robot with the left Ctrl key. The other player controls the walking direction using the arrow keys on the keyboard and resets the robot with the right Ctrl key. The spacebar resets the football.

Double-click "LoongPlay.unity" to open it for the Qinglong Kung Fu Football match, which is set to an automatic match mode by default. Only press the spacebar to reset the football when it gets stuck in a corner.

Motion Remapping and Imitation Learning Example
In the "Assets/Imitation" directory, "G1.unity," which includes Unitree H1 and G1 robots.

When running, you can see the pre-trained guitar, golf, violin, and waving actions of H1 (sharing a single neural network), as well as the pre-trained Charleston dance action of G1.

The actions are stored in the "dataset" directory (H1 actions are generated from the Humanoid2Humanoid method using the AMASS database, and G1 actions are from the LEFAN1 dataset).

Imitation learning training: Only check "Train" for training (refer to the following steps).

Remapped motion playback: Only check "Replay" to play the actions.

"Motion_id" is the action sequence number and can be modified. During runtime, you can see the action name in "Motion_name."

Mobile Manipulation Example
In the "Assets/Manipulation" directory, "G1OP.unity"

Use the keyboard to control the robot for walking and manipulation.

Robot Animation Example
In the "Assets/Animation" directory, double-click to open "dance.unity," and run it to see the animation effects of the G1 robot dancing, playing the piano, and singing.

More animation effects can be found in the "Assets/Animation/Animations" directory.

Sim2Real Example
In the "Assets/Ros2ForUnity/Go2" folder, "Go2Deploy.unity," which is only available for Ubuntu 20 and 22 (the "main" branch is for Ubuntu 22, and the "foxy" branch is for Ubuntu 20).

Use ROS2 to implement communication with the robot and strategy deployment, currently supporting Unitree Go2. You need to install ROS2 (Ubuntu 20 reference Ubuntu 22 reference), as well as Unitree_ROS2.

Note: Add the following two statements in "~/.bashrc" (modify according to your actual paths of "unitree_ros2" and "gewu"):

source ~/ylq/unitree_ros2/setup.sh
export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:/home/ylq/ylq/Unity-RL-Playground/gewu/Assets/Ros2ForUnity/Plugins/Linux/x86_64
After opening "Go2Deploy.unity," select "Go2Real" in the left window and check "is_ros2_installed" in the right "inspector."

Connect the robot dog to the computer with a network cable, turn on the robot dog, and connect it using the mobile app. Click "lie down" to make it lie on the ground, then enter "Device - Service Status" in the app and click "mcf" to turn off the main motion control service (wait a moment after clicking once).

"Go2Deploy.unity" is an example for real robot deployment. Before running, ensure the robot dog is lying on the ground and "mcf" is turned off. When running, the robot dog will slightly stand up. Click "stand up" until it stands up completely. Then check "FF_enable" (feedforward enable), and the robot dog will start stepping. Next, check "NN_enable" (neural network enable), and you can control the robot dog's walking and turning using the WASD keys on the keyboard. To finish, first uncheck "FF_enable" and "NN_enable," and click "lie down."

"Go2Train.unity" is used for strategy training.

Autonomous Navigation Example
In the "Assets/Navigation/Scene" directory, open "Go2Navi.unity" and click any target point on the screen.

This example uses Unity's AI Navigation plugin to autonomously plan a route and calls the pre-trained omnidirectional walking model of Go2 for control.

II. Training Environment Installation
Install Anaconda: https://www.anaconda.com/download
Search for "anaconda" in the computer's search box and click to open the "anaconda prompt" command-line window.
Run conda create -n gewu python=3.10.12 -y
Run conda activate gewu
Run pip3 install torch~=2.2.1 --index-url https://download.pytorch.org/whl/cu121
(Ensure a stable network connection. It may take a long time, be patient. If the installation fails, try a different network.)

Run python -m pip install mlagents==1.1.0
(Be patient.)

Run mlagents-learn --help to check if the installation is successful (no errors are displayed).
III. Training Robots
Take the "Robot Rally" example as an illustration:

Open "Playground.unity" in the "Assets/Playground" directory, select a robot to train (e.g., Go2) in the left panel, and then check "train" in the right "inspector."
Select other robots and hide them all (uncheck the box at the top in the "inspector" window).
Return to the anaconda interface, enter the main directory of Unity-RL-Playground (e.g., first run D: and then run cd D:\Unity-RL-Playground-main\gewu\Assets\Playground (adjust according to your actual directory)).
Run mlagents-learn config.yaml --run-id=go2trot --force to start training.
(Note: The ID can be any name you choose. --force means training from scratch. If you use --resume, it will continue training from the breakpoint.)

When "[INFO] Listening on..." appears in the window, return to the Unity interface and click the triangle button above to start training.
During training, you can observe the training progress in the anaconda window. Normally, the reward should gradually increase. Generally, training for 2,000,000 steps is sufficient. Press Ctrl+C to terminate the training.
After terminating the training, find the newly trained neural network at the bottom of the Unity interface. In the "results->go2trot" (the name is consistent with the "run-id") directory, you will see a "gewu.onnx" file, which is the trained neural network. To view the training reward curve, etc., run tensorboard --logdir results --port 6006 in the anaconda window, and then enter http://localhost:6006/ in the browser.
Click to select the robot, and in the right "inspector" window, you will see many policy boxes. Drag the trained neural network to the corresponding box (e.g., "Q trot policy").
Uncheck "train" in the right "inspector" and run Unity to see the robot's motion effect.
Similarly, you can train "TinkerTrain.unity" and "LoongTrain.unity," and the trained neural networks can be used in "TinkerPlay.unity" and "LoongPlay.unity."
IV. Importing and Training New Robots
The robot URDF files are located in the "Assets\urdf" directory.
The robot URDF folder generally contains "xx.urdf" and a "meshes" folder. The path format in "xx.urdf" is "package://meshes/xxx.STL." It is best to have the joints outside the robot's legs already locked. (Note: If there are joints outside the legs that are not locked, you can open the robot's structure tree after importing, select the corresponding "ArticulationBody," and change the "Articulation Joint Type" from "Revolute" to "Fix.")
Open the pre-made empty scene "MyRobot.Unity" in Unity.
Take the Zhongqing robot as an example. In the URDF folder, enter "zq_humanoid," single-click to select "zq_sa01.urdf," click "Assets" -> "Import Robot from Selected URDF" in the menu bar, and in the pop-up window, select "unity" for "mesh decomposer" and click "import URDF."
After seeing the robot model imported, select the robot and adjust its height (y-axis) in the right "inspector" to make its feet touch the ground. You can make it slightly higher.
Select the imported robot and delete both the "Urdf Robot (script)" and "Controller (script)" in the right "inspector" window.
Drag the imported robot to the child node of "MyRobot."
Select "MyRobot," select the corresponding "RobotType" in the right "inspector" window (keep the default "Biped" for the Zhongqing robot) and "Target Motion" (keep the default "Walk" under "Biped" for this example). Set the observation and action dimensions in "Behaviour Parameters" (keep the defaults for this example). You can refer to other robots.
Pre-training test: Check the "Fixbody" checkbox and run Unity to check if the feedforward action is correct. In the bipedal "walk" gait, the robot should step up and down.
If the feedforward does not match, search for "change here" in the "GewuAgent" code, find the corresponding code, and modify the parameters suitable for this robot (in this example, add negative signs to all six numbers in line 285). You should see the robot stepping up and down normally.
(Note: "idx" represents the joints to which feedforward should be applied. For bipeds, it is the three pitch joints of the hip, knee, and ankle. Generally, the values can be kept as defaults (a few inconsistent configurations need to be modified). The positive and negative signs are related to the joint rotation direction and should be modified according to the situation.)

After configuration, you can start training using the mlagents-learn... command (refer to the steps in "III"). For this example, only 400,000 steps (2-5 minutes) of training are needed to see the effect.
For more robot URDF models, see the following repository: https://github.com/linqi-ye/robot-universe, which collects URDF models of numerous robots.
