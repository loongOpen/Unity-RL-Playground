import joblib
import numpy as np  # 别忘了这一行！

# 加载 pkl 文件
data = joblib.load("kick01.pkl")

# 查看最顶层的 key
print("Top-level keys:", data.keys())

# 获取第一个 key 的内容
first_key = list(data.keys())[0]
motion = data[first_key]

# 查看 motion 里面的 key
print("Motion keys:", motion.keys())

# 查看每个 key 的数据结构
for key, value in motion.items():
    if isinstance(value, np.ndarray):
        print(f"{key}: shape = {value.shape}, dtype = {value.dtype}")
    else:
        print(f"{key}: type = {type(value)}, value = {value}")
