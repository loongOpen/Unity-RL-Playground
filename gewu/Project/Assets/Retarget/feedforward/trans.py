import joblib
import numpy as np
import os
import pandas as pd

# 输入文件夹和输出目录
pkl_dir = "/home/dell/ylq/Unity-RL-Playground/gewu/Project/Assets/H1/feedforward"  # 替换为你的文件夹路径
output_base_dir = "/home/dell/ylq/Unity-RL-Playground/gewu/Project/Assets/H1/feedforward"  # 保存目录

# 确保输出目录存在
os.makedirs(output_base_dir, exist_ok=True)

# 遍历目录下的所有.pkl文件
for pkl_file in os.listdir(pkl_dir):
    if pkl_file.endswith('.pkl'):
        pkl_path = os.path.join(pkl_dir, pkl_file)
        output_dir = os.path.join(output_base_dir, pkl_file.split('.')[0])
        
        # 创建每个 pkl 文件对应的输出目录
        os.makedirs(output_dir, exist_ok=True)
        
        # 加载 pkl 文件
        data = joblib.load(pkl_path)

        # 通常只有一个 key
        key = list(data.keys())[0]
        motion = data[key]

        # 遍历 motion 中的字段，分别保存为 .npy 或 .txt
        for k, v in motion.items():
            if isinstance(v, np.ndarray):
                np.save(os.path.join(output_dir, f"{k}.npy"), v)
                print(f"保存 {k}.npy 成功！ shape = {v.shape}")

                # 将 dof、root_rot 和 root_trans_offset 转换为 CSV 文件
                if k == "dof" or k == "root_rot" or k == "root_trans_offset":
                    # 将 np.ndarray 转换为 DataFrame
                    df = pd.DataFrame(v)
                    # 保存为 CSV，直接放在 output_dir 下
                    csv_path = os.path.join(output_dir, f"{k}.csv")
                    df.to_csv(csv_path, index=False)
                    print(f"保存 {k}.csv 成功！")
            else:
                # 如果是 int 或其他标量（如 fps）
                with open(os.path.join(output_dir, f"{k}.txt"), 'w') as f:
                    f.write(str(v))
                print(f"保存 {k}.txt 成功！ value = {v}")
