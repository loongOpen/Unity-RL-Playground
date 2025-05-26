# “格物”具身智能仿真平台
**格物免安装体验版（解压后运行Gewu.exe）：[飞书](https://gvtdwawnc78.feishu.cn/wiki/Zy1EwQSg0ibU9hkNdTTc5uwtnye?from=from_copylink)[百度网盘](https://pan.baidu.com/s/1IQ7_aVZ44f1xgd4Qnthi7w?pwd=bngw)**

**[格物平台图文版安装使用教程](https://gvtdwawnc78.feishu.cn/wiki/BdQywjXQ0iHPrGkICsUct4Nenqg?from=from_copylink)**

- **2025.5.25，添加复杂地形例程**
- **2025.4.19，添加动作重映射例程、添加四轮足例程**
- **2025.4.4，格物1.0上线，全面升级**
  机器人运动会、Tinker足球赛、青龙功夫足球全上线
  升级至Unity2023，依赖包预置，下载即用，代码优化，开发更方便
- **2025.3.20，格物0.1代码发布**
  采用Unity2021，打包为UnityPackage

<div align="center">

| <div align="center"> [机器人总动员](https://www.bilibili.com/video/BV167RbYxEuG/) </div> | <div align="center">  [全地形行走](https://www.bilibili.com/video/BV1ZDjGz9EJH/) </div> |  <div align="center"> [动作重映射](https://www.bilibili.com/video/BV1vpdYYaEXe/) </div> |  <div align="center"> [机器人动画](https://www.bilibili.com/video/BV1maEPzQEtD/?vd_source=25bf190003ff1ebd36e7649d3641e141) </div> |
| ---  | --- | --- | --- |
| <img src="gewu/sport-meeting.gif" width="180px"> | <img src="gewu/terrain.gif" width="180px"> | <img src="gewu/retarget.gif" width="180px"> | <img src="gewu/G1perform.gif" width="180px"> |

</div>

---

“格物”是由国家地方共建人形机器人创新中心、上海大学、清华大学联合推出的具身智能仿真训练平台。该项目基于Unity ML-Agents工具包构建，旨在为研究人员和开发者提供一个高效且友好的强化学习开发环境，适用于各类机器人。

## 相关论文

**更多技术细节，或使用本平台进行研究请参考和引用以下论文：**

[1] Ye, Linqi, Rankun Li, Xiaowen Hu, Jiayi Li, Boyang Xing, Yan Peng, and Bin Liang. "Unity RL Playground: A Versatile Reinforcement Learning Framework for Mobile Robots." arXiv preprint arXiv:2503.05146 (2025). [PDF](https://arxiv.org/abs/2503.05146)

[2] Ye, Linqi, Jiayi Li, Yi Cheng, Xianhao Wang, Bin Liang, and Yan Peng. "From knowing to doing: learning diverse motor skills through instruction learning." arXiv preprint arXiv:2309.09167 (2023).[PDF](https://arxiv.org/abs/2309.09167) 

## 使用说明（适用于Windows、Linux、macOS等操作系统）

## 一、仿真环境安装

1. 搜索安装Unity Hub，注册登录，弹出的Install Unity Editor窗口点击skip跳过，然后点击Agree and get personal edition license免费激活

2. 在打开的Unity Hub界面，在Installs菜单点击Install Editor，选择Unity Editor 2023版本（2023.2.20f1c1）安装（7个多G，耐心等待，若之前安装了2021版本可将其卸载以腾出空间）

3. 下载Unity RL Playground：https://github.com/loongOpen/Unity-RL-Playground ，解压到本地

4. 在Unity Hub的Projects菜单中点击Open，选择上一步解压的Unity-RL-Playground\gewu\Project目录，点击Open，等待项目打开（第一次打开耗时较长，耐心等待）

5. 项目打开后，在Unity下方的小窗口可看到Assets目录下的RL-Playground，点击进入该目录下，双击Playground.unity打开，点击unity上面的三角形运行即可看到机器人预训练好的运动效果！

6. 选中某个机器人，在右边inspector窗口可在对应的target motion下拉框切换运动模式（如果对应的预训练模型非空）。

7. 双击TinkerPlay.unity打开即为Tinker足球赛，预设为双人对战模式，一人通过键盘上的WASD键控制行走方向、左ctrl键复位机器人，另一人通过键盘上的上下左右键控制行走方向、右ctrl键复位机器人，空格键复位足球

8. 双击LoongPlay.unity打开即为青龙功夫足球，预设为自动对战模式，只当足球卡在角落时可按空格键复位

9. 双击Go2.unity打开为四足机器人全向行走例程，通过WASD和左右箭头按键控制行走方向，空格键复位

10. 录制视频在菜单栏Window->General->Recorder->Recorder Window，点击Add Recorder->Movie，点击红色三角形即可录制，在下方Path可找到保存路径

## 动作重映射例程，用于复杂动作的模仿学习

在Assets/Retarget目录下，H1.unity运行后会依次播放feedforward文件夹下存入的预设动作

可参考H2O github代码生成新的重映射动作

## 复杂地形例程Terrain.unity

长30cm高15cm台阶，预训练青龙、宇树G1、加速进化T1、众擎SA01

注意训练时要单独每个机器人，其他机器人隐藏

采用课程学习，训练时逐渐增大楼梯高度（调整Stairs的Scale的y值）

## 二、训练环境安装

1. 安装Anaconda：https://www.anaconda.com/download

2. 在电脑搜索框搜索anaconda，点击打开anacconda prompt命令行窗口

3. 运行`conda create -n gewu python=3.10.12 -y`

    （注：若安装了之前的老版本，可通过conda remove -n ml-agents命令将其删除）

4. 运行`conda activate gewu`

5. 运行`pip3 install torch~=2.2.1 --index-url https://download.pytorch.org/whl/cu121`

    （确保网络畅通，耗时较长，耐心等待，若安装失败可换个网络试试）

6. 运行`python -m pip install mlagents==1.1.0`

    （耐心等待）

7. 运行`mlagents-learn --help`检查是否安装成功（无报错即可）

## 三、训练机器人

1. 在unity打开Playground.unity，选中一个要训练的机器人（建议先用Go2测试），在右侧inspector中勾选train

2. 选中其他机器人将他们都隐藏（在inspector窗口将最上面一个方框的勾取消即可）

3. 回到anaconda界面，进入Unity-RL-Playground主目录（例如，先运行`D:` 再运行 `cd D:\Unity-RL-Playground-main\gewu\Project\Assets\RL-Playground` （根据自己的实际目录调整））

4. 运行`mlagents-learn trainer_config.yaml --run-id=go2trot --force`开始训练

    （注：id号名称可自己任取，--force为从零训练，若使用--resume则为断点继续训练）
   
6. 当窗口中出现[INFO] Listening on ...时回到unity界面，点击上面的三角形按钮运行即可开始训练

7. 训练时可在anaconda窗口观察训练进度，正常来说奖励会逐渐升高，一般训练2000000个step即可，按ctrl+c终止训练

8. 终止训练后在unity界面下方找到刚刚训的神经网络，在results->go2trot（名称与run-id一致）目录中，可看到一个gewu.onnx的文件，即为训练好的神经网络

9. 点击选中机器人，在右侧inspector窗口可看到很多policy的方框，将训练好的神经网络拖动到对应方框中（如Q trot policy）

10. 在右侧inspector中取消勾选train，运行unity，即可看到机器人的运动效果

11. 类似地，可对TinkerTrain.unity和LoongTrain.unity进行训练，训练所得的神经网络可用于TinkerPlay.unity和LoongPlay.unity

## 四、导入和训练新的机器人

**以下仓库集齐了众多机器人URDF模型：https://github.com/linqi-ye/robot-universe**

1. 将新的机器人urdf文件夹（包括meshes）放入Unity-RL-Playground-main\urdf文件夹

2. 机器人urdf文件夹一般命名为xx_description，里面包含xx.urdf以及meshes文件夹，xx.urdf里面的路径格式为package://meshes/xxx.STL，机器人腿部以外的关节最好已经锁定。        （注：如果腿部以外有关节未锁定，可在导入后打开机器人结构树，选中对应的ArticulationBody将Articulation Joint Type由Revolute改为Fix）

3. 在unity中打开预制的空场景MyRobot.Unity

4. 以众擎机器人为例，在urdf文件夹中进入zq_humanoid，单击选中zq_sa01.urdf，点击菜单栏Assets->Import Robot from Selected URDF，弹出窗口，将mesh decomposer选择unity，点击import URDF

5. 看到机器人模型导入后，选中机器人在右侧inspector调整高度(y轴)使其脚着地，可稍高一点点

6. 选中导入的机器人，在inspector窗口将Urdf Robot (script)和Controller (script) 都删除

7. 拖动导入的机器人到MyRobot的子节点中

8. 选中MyRobot，在inspector窗口选择对应的RobotType（众擎机器人保持默认Biped即可）和Target Motion（此例在Biped下面保持默认的Walk即可），在Behaviour Parameters设置observation和action维数（此例保持默认即可），可参考其他机器人

9. 训练前测试，选中Fixbody复选框，运行unity查看前馈动作是否正确，双足walk步态下机器人应上下踏步

10. 如前馈不匹配，可在GewuAgent代码中搜索“change here”,找到对应代码修改适合本机器人的参数（本例中在285行的idx六个数全加上负号即可），看到机器人正常上下踏步即可

    （注：idx代表要给前馈的关节，对于双足是髋、膝、踝的三个pitch关节，一般来说数值用默认即可（少数构型不一致的需修改），正负号和关节转向有关，根据情况修改）

11. 配置完毕，即可通过`mlagents-learn …… `语句进行训练（参考“三”中步骤），本例只需训练40万step（2～5分钟）即可看到效果

# Unity RL Playground

Unity RL Playground (also named **Gewu**) is an embodied intelligence robotics simulation platform jointly launched by the National and Local Co-Built Humanoid Robotics Innovation Center, Shanghai University, and Tsinghua University. Built on top of the Unity ML-Agents Toolkit, this project aims to provide researchers and developers with an efficient and user-friendly reinforcement learning (RL) development environment for various robots.

## Related Publication
F‌or more details, please read and cite the following papers when conducting research using this platform‌:

Ye, Linqi, Rankun Li, Xiaowen Hu, Jiayi Li, Boyang Xing, Yan Peng, and Bin Liang. "Unity RL Playground: A Versatile Reinforcement Learning Framework for Mobile Robots." arXiv preprint arXiv:2503.05146 (2025). [PDF](https://arxiv.org/abs/2503.05146)

Ye, Linqi, Jiayi Li, Yi Cheng, Xianhao Wang, Bin Liang, and Yan Peng. "From knowing to doing: learning diverse motor skills through instruction learning." arXiv preprint arXiv:2309.09167 (2023). [PDF](https://arxiv.org/abs/2309.09167) 

## Platform Features‌

- **Extensive Robot Support‌**: Compatible with hundreds of mobile robots, including humanoid robots, quadruped robots, wheeled robots, and more.
- **One-Click Import & Training‌**: Allowing users to effortlessly import robot models and initiate training without complex configurations.
- **Lowered RL Development Barrier‌**: Simplifies workflows and provides toolkits to make RL technology accessible and approachable for everyone.

## Open-Source & Community Support‌

- **Open-Source Project‌**: Unity RL Playground is fully open-source, with code and resources publicly available on GitHub for developers to freely access and contribute.
- **Community-Driven Growth‌**: We welcome global developers to join our community, collaborate on advancing the platform, and share technical expertise.

Unity RL Playground is committed to becoming an open platform for embodied intelligence, accelerating innovation in robotics technology. Whether you are an academic researcher, developer, or enthusiast, you will find tailored tools and resources here to empower your work.

## I. Simulation Environment Installation

1. Search for and install Unity Hub, register and log in. When the "Install Unity Editor" window pops up, click "skip", then click "Agree and get personal edition license" to activate it for free.

2. In the opened Unity Hub interface, click "Install Editor" under the "Installs" menu, and select the Unity Editor 2023 version (2023.2.20f1c1) for installation (over 7GB, please be patient. If the 2021 version was previously installed, it can be uninstalled to free up space).

3. Download Unity RL Playground: [https://github.com/loongOpen/Unity-RL-Playground](https://github.com/loongOpen/Unity-RL-Playground), and unzip it to a local directory.

4. In the "Projects" menu of Unity Hub, click "Open", select the Unity-RL-Playground\gewu\Project directory from the previous step, click "Open", and wait for the project to open (the first time may take longer, please be patient).

5. After the project opens, in the small window at the bottom of Unity, you can see the RL-Playground directory under the Assets directory. Click to enter this directory, double-click Playground.unity to open it, and click the triangle at the top of Unity to run it to see the pre-trained movement effects of the robot!

6. Select a robot, and in the inspector window on the right, you can switch the movement mode in the corresponding target motion dropdown box (if the corresponding pre-trained model is not empty).

7. Double-click TinkerPlay.unity to open the Tinker soccer game, which is preset to a two-player battle mode. One player controls the walking direction with the WASD keys on the keyboard and resets the robot with the left Ctrl key, while the other player controls the walking direction with the arrow keys and resets the robot with the right Ctrl key. Press the spacebar to reset the soccer ball.

8. Double-click LoongPlay.unity to open the Loong Kung Fu Soccer, which is preset to an automatic battle mode. Press the spacebar to reset the soccer ball only when it gets stuck in a corner.

9. Double-click Go2.unity to open the quadrupedal robot omnidirectional walking routine. Control the walking direction with the WASD and arrow keys, and press the spacebar to reset.

10. To record a video, go to the menu bar Window->General->Recorder->Recorder Window, click Add Recorder->Movie, click the red triangle to start recording, and find the save path under Path.

## II. Training Environment Installation

1. Install Anaconda: [https://www.anaconda.com/download](https://www.anaconda.com/download)

2. Search for Anaconda in the computer search box, and click to open the Anaconda Prompt command line window.

3. Run `conda create -n gewu python=3.10.12 -y`

    (Note: If an older version was previously installed, it can be removed with the command `conda remove -n ml-agents`)

4. Run `conda activate gewu`

5. Run `pip3 install torch~=2.2.1 --index-url https://download.pytorch.org/whl/cu121`

    (Ensure a stable network connection, as this may take a while. If the installation fails, try a different network.)

6. Run `python -m pip install mlagents==1.1.0`

    (Be patient.)

7. Run `mlagents-learn --help` to check if the installation was successful (no errors means success).

## III. Training the Robot

1. Open Playground.unity in Unity, select a robot to train (it is recommended to start with Go2 for testing), and check the "train" box in the inspector on the right.

2. Hide the other robots (uncheck the top box in the inspector window).

3. Return to the Anaconda interface and navigate to the main directory of Unity-RL-Playground (for example, first run `D:` and then `cd D:\Unity-RL-Playground-main\gewu\Project\Assets\Unity-RL-Playground-main` (adjust according to your actual directory)).

4. Run `mlagents-learn trainer_config.yaml --run-id=go2trot --force` to start training (Note: the run-id can be named as desired, `--force` starts training from scratch, while `--resume` continues training from a checkpoint).

5. When "[INFO] Listening on ..." appears in the window, return to the Unity interface, click the triangle button at the top to start training.

6. During training, you can observe the training progress in the Anaconda window. Normally, the reward will gradually increase. Generally, train for 2,000,000 steps, and press Ctrl+C to terminate the training.

7. After terminating the training, find the newly trained neural network in the results->go2trot (the name matches the run-id) directory, where you can see a gewu.onnx file, which is the trained neural network.

8. Click to select the robot, and in the inspector window on the right, you can see many policy boxes. Drag the trained neural network into the corresponding box (e.g., Q trot policy).

9. Uncheck the "train" box in the inspector on the right, and run Unity to see the robot's movement effects.

10. Similarly, you can train TinkerTrain.unity and LoongTrain.unity, and the trained neural networks can be used in TinkerPlay.unity and LoongPlay.unity.

## IV. Importing and Training a New Robot

**The following repository collects numerous robot URDF models: [https://github.com/linqi-ye/robot-universe](https://github.com/linqi-ye/robot-universe)**

1. Place the new robot's URDF folder (including meshes) into the Unity-RL-Playground-main\urdf folder.

2. The robot's URDF folder is generally named xx_description, which contains xx.urdf and a meshes folder. The path format in xx.urdf is package://meshes/xxx.STL. It is best if the joints other than the robot's legs are already locked. (Note: If there are unlocked joints other than the legs, you can open the robot's structure tree after importing, select the corresponding ArticulationBody, and change the Articulation Joint Type from Revolute to Fix.)

3. Open the prefabricated empty scene MyRobot.Unity in Unity.

4. Taking the Zhongqing robot as an example, navigate to zq_humanoid in the urdf folder, click to select zq_sa01.urdf, click Assets->Import Robot from Selected URDF in the menu bar, in the pop-up window, select unity for mesh decomposer, and click import URDF.

5. After seeing the imported robot model, select the robot and adjust its height (y-axis) in the inspector on the right to make its feet touch the ground (it can be slightly higher).

6. Select the imported robot, and in the inspector window, delete both the Urdf Robot (script) and Controller (script).

7. Drag the imported robot into the child node of MyRobot.

8. Select MyRobot, and in the inspector window, choose the corresponding RobotType (keep the default Biped for the Zhongqing robot) and Target Motion (in this case, keep the default Walk under Biped), and set the observation and action dimensions in Behaviour Parameters (keep the default in this case, you can refer to other robots).

9. Before training, test by checking the Fixbody checkbox, run Unity to see if the feedforward action is correct. The robot should take up-and-down steps in the bipedal walk gait.

10. If the feedforward does not match, you can search for "change here" in the GewuAgent code, find the corresponding code to modify the parameters suitable for this robot (in this case, add a negative sign to all six numbers on line 285), and ensure the robot takes normal up-and-down steps.

    (Note: idx represents the joints to be fed forward. For bipeds, these are the three pitch joints of the hip, knee, and ankle. Generally, the default values can be used (modify only if the configuration is inconsistent), and the positive/negative signs are related to the joint direction, so adjust according to the situation.)

11. After configuration, you can train using the `mlagents-learn ...` statement (refer to the steps in "III"). In this case, only 400,000 steps (2-5 minutes) are needed to see the effect.
