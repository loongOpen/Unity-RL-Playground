# Unity-RL-Playground
Unity RL Playground (also named **Gewu**) is an embodied intelligence robotics simulation platform jointly launched by the National and Local Co-Built Humanoid Robotics Innovation Center, Shanghai University, and Tsinghua University. Built on top of the Unity ML-Agents Toolkit, this project aims to provide researchers and developers with an efficient and user-friendly reinforcement learning (RL) development environment.

## Platform Features‌

- **Extensive Robot Support‌**: Compatible with hundreds of mobile robots, including humanoid robots, quadruped robots, wheeled robots, and more.
- **One-Click Import & Training‌**: Allowing users to effortlessly import robot models and initiate training without complex configurations.
- **Lowered RL Development Barrier‌**: Simplifies workflows and provides toolkits to make RL technology accessible and approachable for everyone.

## Open-Source & Community Support‌

- **Open-Source Project‌**: Unity RL Playground is fully open-source, with code and resources publicly available on GitHub for developers to freely access and contribute.
- **Community-Driven Growth‌**: We welcome global developers to join our community, collaborate on advancing the platform, and share technical expertise.

Unity RL Playground is committed to becoming an open platform for embodied intelligence, accelerating innovation in robotics technology. Whether you are an academic researcher, developer, or enthusiast, you will find tailored tools and resources here to empower your work.

# Installation of Unity and ML-Agents
## 1 Install Unity

Download and install the latest version of the Unity Editor. It is recommended to choose the LTS (Long-Term Support) version to ensure stability.

## 2 Configure ML-Agents

### 2.1 Create a Virtual Environment

Install Anaconda

If Anaconda is not already installed, visit the Anaconda official website to download and install it.

Create a Virtual Environment

Open the Anaconda Prompt and run the following command to create a virtual environment named "RL-Playground" with Python 3.10: `conda create -n RL-Playground python=3.10`

Activate the virtual environment: `conda activate RL-Playground`

### 2.2 Install ML-Agents

Download ML-Agents

Download the latest version of the ML-Agents toolkit from the ML-Agents GitHub page.

Note: The download path must not contain Chinese characters, as this may cause training failures.
Install ML-Agents Packages

After extracting the downloaded files, navigate to the ml-agents and ml-agents-envs directories, and execute the following commands in each directory to install them: `pip install .`

After installation, run the following command to verify that the installation was successful: `mlagents-learn --help`

### 2.3 Configure Unity

Create a Unity Project

Open the Unity Editor and create a new 3D project.

Import the ML-Agents Package

In the Unity Editor, click on Window > Package Manager. Click the + icon in the top-left corner and select Add package from disk. Select the package.json files from the com.unity.ml-agents and com.unity.ml-agents.extensions directories to complete the package import.

Verify Installation

After installation, you should see ML-Agents and ML-Agents Extensions in the Packages folder.

# Installing and Using the URDF Importer

## 1. Add the URDF Package

Open the Unity Package Manager

Open the Package Manager from the Unity menu: Click Window > Package Manager.

Add the URDF Package

In the top-left corner of the Package Manager window, click the + button and select Add Package from Git URL.

Enter the URDF Importer Git URL

In the text box, enter the following Git URL for the URDF Importer with the latest version tag (currently v0.5.2):

https://github.com/Unity-Technologies/URDF-Importer.git?path=/com.unity.robotics.urdf-importer#v0.5.2

Press Enter to complete the addition.

Import the URDF Package

The Package Manager will automatically download and install the URDF Importer package.

## 2. Create a Robot Using a URDF File

Prepare the URDF File and Related Resources

Copy the URDF file and its related files (such as mesh files) into the Assets folder of your Unity project. Ensure that the paths to the mesh files are correct.

Import the Robot

In the Project window, right-click on your URDF file and select Import Robot from Selected URDF file.

Configure Import Settings

A window will appear with import settings for the robot:

Mesh File Orientation: Set the orientation of the mesh files.

Collision Mesh Decomposition Algorithm: Choose the algorithm for collision mesh decomposition.

Complete the Import

Click the Import URDF button to complete the robot import.

# Importing and Configuring Unity RL Playground

After setting up the environment, the agent needs to be configured to ensure it can interact correctly with the environment and perform learning tasks. Below are the detailed steps:

## 1. Agent Configuration

### 1.1 Attach Scripts

Attach the following scripts to the imported agent object (Agent):

RobotRLAgent: Defines the agent's behavior and training objectives.

Decision Requester: Controls the decision frequency of the agent.

Behavior Name: Modify this to match the name in the training configuration file (config.yaml).

Max Step: Set to 1000, representing the maximum number of steps per training episode.

Decision Frequency: Set to trigger a decision every 1 time step.

### 1.2 Test Feedforward Actions

If Fixed Body is selected, click the Unity Play button to observe whether the feedforward actions applied to the robot are correct.

## 2. Start Training
   
### 2.1 Configure Training Parameters

Modify the following settings:

Edit > Project Settings > Time > Fixed Timestep = 0.01s

In the RobotRLAgent script, configure the following:

Robot Type: Select the type of robot (e.g., Biped, Quadruped, Legwheeled).

Target Motion: Select the target motion to be trained.

Accelerate: Set Time.timeScale to 20 to accelerate the training process.

### 2.2 Initiate Training

Disable Controller Scripts: Uncheck the imported robot controller scripts.

Enable Training Options: Check Train and Accelerate (if accelerated training is desired).

Start Training Command: Open the Anaconda Prompt and navigate to the directory containing the configuration file: `cd /path/to/your/config/file`

Enter the following command to start training: `mlagents-learn configuration.yaml --run-id=g1_jump --force`

If you need to resume the previous training, use the following command: `mlagents-learn configuration.yaml --run-id=g1_jump --resume`

After training begins, the terminal will output prompt messages prefixed with [INFO].

Verify Training Status: Open Unity and start the game. If the terminal prints out the contents of the configuration file, it indicates that the training has started normally.

### 2.3 Monitoring the Training Process

During the training process, reward data will be output based on the number of steps set in the configuration file (for example, reward data may be output every 20,000 steps). If you need to manually interrupt the training, you can press the shortcut Ctrl+C in the Anaconda Prompt.

# 3. Analyzing Training Results

## 3.1 Viewing Training Statistics Using TensorBoard

In the Anaconda Prompt, while in the same directory, enter the following command: `tensorboard --logdir results/g1_jump`

The terminal will output a TensorBoard URL (e.g., http://localhost:6006/). Open this URL to view the training statistics graphs.

# 4. Running the Trained Model

## 4.1 Importing the Model

After training is complete, the terminal will indicate the location of the trained model file (in .onnx format). Drag the model file into the corresponding field in the RobotRLAgent script.

## 4.2 Enabling the Model

Launch the Unity game to observe the training results.
