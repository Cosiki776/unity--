Unity ML-Agents: Reacher Robot

🤖 基于 Unity ML-Agents 的机器人的强化学习研究项目
本项目训练6自由度机械臂进行触碰，主要是应付具身智能的课堂作业，挺简单的一个6自由度机械臂机械臂，触碰到小球后小球会变大变蓝，这个操作只是为了明显一点的看见触碰，可以取消。
完全参考了https://www.youtube.com/watch?v=HOUPkBF-yv0的教程，但是在网上没找着代码，所以跟着视频敲了一遍，也做了一些改进：
<img width="1117" height="678" alt="image" src="https://github.com/user-attachments/assets/6324a593-3068-4297-ac11-55cc69a62133" />
<img width="355" height="277" alt="image" src="https://github.com/user-attachments/assets/dc0eea3f-7127-48b9-a776-d7cbf9e96fe9" />

1. 👁️ 感知增强 (Observation Engineering)
为了提升 AI 的空间感知能力，将观察空间从默认的 85维 扩充到了 89维：
修改前 (85维)：仅包含关节角度、速度和目标坐标。AI 难以直观感知“我离目标还有多远”。
修改后 (89维)：
  ➕ 新增 相对位移向量 (Vector3)：直接输入 Goal.Position - Hand.Position，提供明确的方向指引。
  ➕ 新增 实时距离 (Float)：作为标量反馈，辅助 AI 判断逼近程度。
  设置路径：Behavior Parameters -> Vector Observation -> Space Size = 89。

2. 🎯 环境重构：物理约束生成
针对随机生成目标可能导致“物理不可达（生成在底座内或臂展外）”的问题，重写了目标生成算法：
  球壳空间约束：将目标生成范围严格限制在 0.5m (最小半径) 至 1.7m (最大臂展) 的球壳空间内。
  防穿模机制：增加了地面高度检测，防止目标生成在地板以下。
  结果：消除了无效样本，大幅提升了训练数据的有效性。

3. 🍭 交互与视觉反馈优化
视觉反馈：当机械臂成功抓取目标时，目标球会瞬间变色并放大，提供直观的成功提示。
奖励重塑 (Reward Shaping)：
  引入 距离惩罚 (Distance Penalty)：离目标越远扣分越多，迫使机械臂主动出击。
  引入 时间惩罚：鼓励快速完成任务。


🛠️ 技术栈与环境配置 (Tech Stack)
为了确保实验的可复现性，请严格遵循以下版本配置：
Unity Editor: 2020.3.25f1c1 (LTS)
ML-Agents Package: Release 19
Python Library: mlagents == 0.28.0
python: python 3.7


📂 目录结构 (Directory Structure)
├── Assets/
│   ├── Scripts/            # 核心 C# 脚本 (ReacherRobot.cs 等)
│   ├── RobotReacher.onnx             # 🧠 训练好的 .onnx 模型文件 (直接可以用)
│   └── ...
├── Packages/               # Unity 插件依赖
└── ProjectSettings/        # 项目设置
config文件在油管链接下


🏃‍♂️ 如何运行 (How to Run)
打开项目：使用 Unity Hub 添加并打开项目文件夹。
加载模型：
  找到.onnx 文件。
  选中场景中的 ReacherRobot 物体。
  在 Inspector 面板的 Behavior Parameters -> Model 槽位中，拖入该 .onnx 文件。
运行：点击 Unity 编辑器顶部的 ▶️ 按钮即可观看推理效果。

或者配环境跟着教程重新训


