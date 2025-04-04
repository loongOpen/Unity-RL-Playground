# “格物”具身智能仿真平台
- **格物1.0上线啦！**
- **机器人运动会、Tinker足球赛、青龙功夫足球全部上线**
- **采用最新版Unity2023**
- **不再打包，push代码更加方便**
- **依赖包已预置，下载即用**
- **极致优化简洁的代码，开发更方便**

**点击下图观看视频（机器人运动会）**

[![视频封面图片](logo.jpg)](https://www.bilibili.com/video/BV167RbYxEuG/)

**点击下图观看视频（Tinker足球赛）**

[![视频封面图片](soccer.png)](https://www.bilibili.com/video/BV1MBo8YUESp/)

**点击下图观看视频（青龙功夫足球）**

[![视频封面图片](kungfusoccer.png)](https://www.bilibili.com/video/BV1NuZBYeEq8/)

“格物”是由国家地方共建人形机器人创新中心、上海大学、清华大学联合推出的具身智能仿真训练平台。该项目基于Unity ML-Agents工具包构建，旨在为研究人员和开发者提供一个高效且用户友好的强化学习开发环境，适用于各类机器人。

## 相关论文

**更多技术细节，或使用本平台进行研究请参考和引用以下论文：**

[1] Ye, Linqi, Rankun Li, Xiaowen Hu, Jiayi Li, Boyang Xing, Yan Peng, and Bin Liang. "Unity RL Playground: A Versatile Reinforcement Learning Framework for Mobile Robots." arXiv preprint arXiv:2503.05146 (2025). [PDF](https://arxiv.org/abs/2503.05146)

[2] Ye, Linqi, Jiayi Li, Yi Cheng, Xianhao Wang, Bin Liang, and Yan Peng. "From knowing to doing: learning diverse motor skills through instruction learning." arXiv preprint arXiv:2309.09167 (2023).[PDF](https://arxiv.org/abs/2309.09167) 

## 平台特性
- **广泛的机器人支持**：兼容数百种移动机器人，包括人形机器人、四足机器人、轮式机器人等。
  
- **一键导入与训练**：允许用户轻松导入机器人模型并启动训练，无需复杂的配置。
  
- **降低强化学习开发门槛**：简化工作流程并提供工具包，使强化学习技术人人皆可接触和使用。
  
## 开源与社区支持
- **开源项目**：Unity RL Playground是完全开源的，代码和资源在GitHub上公开可用，供开发者自由访问和贡献。
  
- **社区驱动发展**：我们欢迎全球开发者加入我们的社区，共同推动平台的发展，并分享技术专长。

“格物”致力于成为具身智能的开放平台，加速机器人技术的创新。无论您是学术研究人员、开发者还是爱好者，都能在这里找到量身定制的工具和资源，助力您的工作。

**“格物”平台适用于Windows、Linux、macOS等操作系统**

**注：最新上传的功夫足球例程KungfuSoccer.unitypackage需安装unity 2023和mlagents 22使用**

## 一、仿真环境安装

1.搜索安装Unity Hub，注册登录，弹出的Install Unity Editor窗口点击skip跳过，然后点击Agree and get personal edition license免费激活

2.在打开的Unity Hub界面，在Installs菜单点击Install Editor，选择Unity Editor 2023版本安装（7个多G，安装需一定时间，耐心等待）

3.下载Unity RL Playground：https://github.com/loongOpen/Unity-RL-Playground ，解压到本地

4.在Unity Hub的Projects菜单中点击Open，选择Unity-RL-Playground\gewu\Project目录导入，导入后点击该Project，等待项目打开（第一次打开耗时较长，耐心等待）

5.此时在Unity下方的小窗口可看到Assets目录下的Unity-RL-Playground-main，点击进入该目录下，双击Playground.unity打开，点击unity上面的三角形运行即可看到机器人预训练好的运动效果！

6.选中某个机器人，在右边inspector窗口可在对应的target motion下拉框切换运动模式（如果对应的预训练模型非空）。录制视频可添加和使用Unity Recorder插件

7.双击Tinker.unity打开即为Tinker足球赛，双击GedouPlay.unity打开即为青龙功夫足球

8.录制视频在菜单栏Window->General->Recorder->Recorder Window，点击Add Recorder->Movie，点击红色三角形即可录制，在下方Path可找到保存路径

## 二、训练环境安装

1.安装Anaconda：https://www.anaconda.com/download

2.打开anacconda窗口

3.运行conda create -n gewu python=3.10.12

4.运行conda activate gewu

5.运行pip3 install torch~=2.2.1 --index-url https://download.pytorch.org/whl/cu121

（确保网络畅通，耗时较长，耐心等待，若安装失败可换个网络试试）

6.运行python -m pip install mlagents==1.1.0

（耐心等待）

7.运行mlagents-learn --help检查是否安装成功

## 三、训练机器人

1.在unity打开Playground.unity，选中一个要训练的机器人（建议先用Go2测试），在右侧inspector中勾选train

2.选中其他机器人将他们都隐藏（在inspector窗口将最上面一个方框的勾取消即可）

3.回到anaconda界面，进入Unity-RL-Playground主目录（例如，先运行D: 再运行 cd D:\ml-agents-release_20\Project\Assets\Unity-RL-Playground-main （根据自己的实际目录调整））

4.运行mlagents-learn trainer_config.yaml --run-id=go2trot --force开始训练（注：id号名称可自己任取，--force为从零训练，若使用--resume则为断点继续训练）

5.当窗口中出现[INFO] Listening on ...时回到unity界面，点击上面的三角形按钮运行即可开始训练

6.训练时可在anaconda窗口观察训练进度，正常来说奖励会逐渐升高，一般训练2000000个step即可，按ctrl+c终止训练

7.终止训练后在unity界面下方找到刚刚训的神经网络，在results->go2trot（名称与run-id一致）目录中，可看到一个gewu.onnx的文件，即为训练好的神经网络

8.点击选中机器人，在右侧inspector窗口可看到很多policy的方框，将训练好的神经网络拖动到对应方框中（如Q trot policy）

9.在右侧inspector中取消勾选train，运行unity，即可看到机器人的运动效果

## 四、导入和训练新的机器人

1.将新的机器人urdf文件夹（包括meshes）放入Unity-RL-Playground-main\urdf文件夹

2.机器人urdf文件夹一般命名为xx_description，里面包含xx.urdf以及meshes文件夹，xx.urdf里面的路径格式为package://meshes/xxx.STL，机器人腿部以外的关节最好已经锁定。（注：如果腿部以外有关节未锁定，可在导入后打开机器人结构树，选中对应的ArticulationBody将Articulation Joint Type由Revolute改为Fix）

3.在unity下方点击选中机器人xx.urdf，点击菜单栏Assets->Import Robot from Selected URDF，弹出窗口，将mesh decomposer选择unity，点击import URDF

4.看到机器人模型导入后，选中机器人在右侧inspector调整高度(y轴)使其脚着地，可稍高一点点

5.将示例程序中其他机器人隐去

6.右键create empty，将gameobject名称改为自己机器人名字

7.拖动导入的机器人到上一步gameobject的子节点中

8.选中gameobject，在inspector窗口点击add component，搜索添加RobotRLAgent代码，再次点击add component，搜索添加decision requester

9.在Behaviour Parameters设置observation和action维数，可参考其他机器人

10.训练前测试，可选中Fixbody复选框，运行unity查看前馈动作是否正确，如报错不匹配，可在RobotRLAgent代码中的if (name.Contains("机器人名称"))添加适合本机器人的参数即可，具体参考其他机器人

11.配置完毕，即可依照“三”中步骤进行训练

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
Search for and install Unity Hub. Register and log in to activate using Personal Licenses for free.

Open Unity Hub, select the Unity Editor 2021 LTS version in the Installs menu, and install it.

Download Unity ML-Agents from https://github.com/Unity-Technologies/ml-agents. Select Release 20 from the Releases list, download, and unzip it.

Download URDF-Importer from https://github.com/Unity-Technologies/URDF-Importer, unzip it, and place it in the main directory of ml-agents-release_20.

In Unity Hub's Projects menu, click Open and select the ml-agents-release_20\Project directory to open it.

In Unity's Window->Package Manager, click "+", then click Add package from disk. Select the URDF-Importer-main\com.unity.robotics.urdf-importer\package file to complete the URDF importer import.

Download Unity RL Playground from https://github.com/loongOpen/Unity-RL-Playground. Select both "Unity-RL-Playground.part1.rar" and "Unity-RL-Playground.part2.rar" to unzip them simultaneously, resulting in the Unity-RL-Playground.unitypackage file.

In Unity's menu bar, go to Assets->Import Package, select Unity-RL-Playground.unitypackage, and click import in the pop-up window.

You will now see Unity-RL-Playground-main under the Assets directory in the small window at the bottom of Unity. Click to enter this directory, double-click Playground.unity to open it, and click the triangle on top of unity to run and see the pre-trained movement effects of the robots!

Select a robot, and in the inspector window on the right, you can switch motion modes in the corresponding target motion dropdown menu (if the corresponding pre-trained model is not empty).

## II. Training Environment Installation
Install Anaconda from https://www.anaconda.com/download.

Open the Anaconda window.

Run conda create -n ml-agents python=3.7.

Run activate ml-agents.

Run pip3 install torch~=1.7.1 -f https://download.pytorch.org/whl/torch_stable.html.

Run python -m pip install mlagents==0.28.0.

Run pip install importlib-metadata==4.4.

Run pip install six.

Run mlagents-learn --help to check if the installation is successful.

## III. Training Robots
Open Playground.unity in Unity, select a robot to train (e.g., tinker), and check the train box in the inspector on the right.

Hide other robots (uncheck the top box in the inspector window).

Go back to the Anaconda interface and enter the Unity-RL-Playground main directory (e.g., first run D:, then cd D:\ml-agents-release_20\Project\Assets\Unity-RL-Playground-main - adjust according to your actual directory).

Run mlagents-learn trainer_config.yaml --run-id=tinker --force to start training (note: the id name can be customized, --force starts training from scratch, use --resume to continue from a checkpoint).

When [INFO] Listening on ... appears in the window, go back to the Unity interface, click the triangle button to start training.

Observe the training progress in the Anaconda window. Normally, the reward will gradually increase. Training for 2,000,000 steps is typical, and you can terminate training by pressing ctrl+c.

After terminating training, find the trained neural network in the results directory in Unity (e.g., results->tinker, where the name matches the run-id). You will see a gewu.onnx file, which is the trained neural network.

Select the robot, and in the inspector window on the right, you will see multiple policy boxes. Drag the trained neural network into the corresponding box (e.g., B walk policy).

Uncheck train in the inspector on the right, run Unity, and you will see the robot's movement effect.

## IV. Importing and Training New Robots
Place the new robot's urdf folder (including meshes) into Unity-RL-Playground-main\urdf.

The robot's urdf folder is generally named xx_description, containing xx.urdf and a meshes folder. The path format in xx.urdf is package://meshes/xxx.STL. Joints other than the robot's legs should be locked. (Note: If there are unlocked joints other than the legs, you can open the robot's structure tree after importing, select the corresponding ArticulationBody, and change Articulation Joint Type from Revolute to Fix.)

In Unity, click on the selected robot xx.urdf at the bottom, go to Assets->Import Robot from Selected URDF in the menu bar. In the pop-up window, select unity for mesh decomposer and click import URDF.

After seeing the imported robot model, select the robot and adjust its height (y-axis) in the inspector on the right to make its feet touch the ground, slightly higher if necessary.

Hide other robots in the example program.

Right-click to create an empty gameobject and rename it after your robot.

Drag the imported robot into this gameobject as a child node.

Select the gameobject, click add component in the inspector window, search and add the RobotRLAgent script, then click add component again, search and add the decision requester.

Set the observation and action dimensions in Behaviour Parameters, referring to other robots for reference.

Before training, you can test by checking the Fixbody box and running Unity to see if the feedforward actions are correct. If there are mismatch errors, you can add parameters suitable for this robot in the RobotRLAgent code within the if (name.Contains("robot name")) statement, referring to other robots for specifics.

After configuration, proceed with training following the steps in section III.


