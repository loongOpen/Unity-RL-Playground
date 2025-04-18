

import numpy as np
import matplotlib.pyplot as plt
from matplotlib.animation import FuncAnimation
from mpl_toolkits.mplot3d import Axes3D
from scipy.spatial.transform import Rotation as R

# 加载数据
pose_aa = np.load("/home/kr1shu/Unity/package/ml-agents-release_20/Project/feedforward/kick01_h1/pose_aa.npy")  # shape: (N, 22, 3)
root_trans = np.load("/home/kr1shu/Unity/package/ml-agents-release_20/Project/feedforward/kick01_h1/root_trans_offset.npy") 

N, num_joints, _ = pose_aa.shape

# 关节连接关系（假设蛇形连接）
joint_pairs = [(i, i+1) for i in range(num_joints - 1)]

# 初始化图形
fig = plt.figure(figsize=(8, 6))
ax = fig.add_subplot(111, projection='3d')
ax.set_xlim(-1, 1)
ax.set_ylim(-1, 1)
ax.set_zlim(0, 2)
ax.set_title("3D Skeleton Animation")

lines = []
for _ in joint_pairs:
    line, = ax.plot([], [], [], 'bo-', lw=2)
    lines.append(line)

def get_joint_positions_3d(frame_idx):
    pose = pose_aa[frame_idx]  # (22, 3)
    root = root_trans[frame_idx]
    joint_positions = [root]

    pos = root
    for joint_rotvec in pose:
        rot = R.from_rotvec(joint_rotvec)
        offset = rot.apply([0, 0.05, 0])  # 每个关节一个小偏移
        pos = pos + offset
        joint_positions.append(pos)
    return np.array(joint_positions)

# 动画帧更新函数
def update(frame):
    joint_positions = get_joint_positions_3d(frame)
    for idx, (i, j) in enumerate(joint_pairs):
        x = [joint_positions[i][0], joint_positions[j][0]]
        y = [joint_positions[i][1], joint_positions[j][1]]
        z = [joint_positions[i][2], joint_positions[j][2]]
        lines[idx].set_data(x, y)
        lines[idx].set_3d_properties(z)
    ax.set_title(f"Frame {frame}")
    return lines

# 创建动画
ani = FuncAnimation(fig, update, frames=N, interval=1000//30, blit=True)
plt.show()
