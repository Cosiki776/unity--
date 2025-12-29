# Unity ML-Agents: Reacher Robot

> 🤖 **基于 Unity ML-Agents 的机器人强化学习研究项目**  
> 本项目训练了一个 6 自由度机械臂进行触碰任务，主要是一个具身智能课程的作业。虽然是一个简单的 6 自由度机械臂，但为了让触碰效果更明显，触碰到小球后小球会变大变蓝（这个视觉反馈可以在代码中取消）。

本项目基础逻辑完全参考了 [YouTube 教程](https://www.youtube.com/watch?v=HOUPkBF-yv0) 。由于网上没有找到现成代码，我跟着视频手动敲了一遍，并在此基础上针对训练效果做了一些**核心改进**。

<img width="1117" height="678" alt="image" src="https://github.com/user-attachments/assets/6324a593-3068-4297-ac11-55cc69a62133" />
<img width="355" height="277" alt="image" src="https://github.com/user-attachments/assets/dc0eea3f-7127-48b9-a776-d7cbf9e96fe9" />

> *注：蓝色的菱形相当于感应块，在unity上方的 Gizmos 上拉 3D Icons 缩小。*

---

## 🚀 修改部分 (Key Improvements)

虽然参考了教程，但我针对原始代码中存在的**训练收敛慢**、**样本无效**等问题进行了以下优化：

### 1. 👁️ 感知增强 (Observation Engineering)
为了提升 AI 的空间感知能力，将观察空间从默认的 **85维** 扩充到了 **89维**：

*   **修改前 (85维)**：仅包含关节角度、速度和目标坐标。AI 难以直观感知“我离目标还有多远”。
*   **修改后 (89维)**：
    *   ➕ **新增 相对位移向量 (Vector3)**：直接输入 `Goal.Position - Hand.Position`，提供明确的方向指引。
    *   ➕ **新增 实时距离 (Float)**：作为标量反馈，辅助 AI 判断逼近程度。
*   **配置路径**：`Behavior Parameters` -> `Vector Observation` -> `Space Size` = **89**。

### 2. 🎯 环境重构：物理约束生成
针对随机生成目标可能导致“物理不可达（生成在底座内或臂展外）”的问题，重写了目标生成算法：

*   **球壳空间约束**：将目标生成范围严格限制在 **0.5m (最小半径)** 至 **1.7m (最大臂展)** 的球壳空间内。
*   **防穿模机制**：增加了地面高度检测，防止目标生成在地板以下。
*   **结果**：消除了无效样本，大幅提升了训练数据的有效性。

### 3. 🍭 交互与视觉反馈优化
*   **视觉反馈**：当机械臂成功抓取目标时，目标球会瞬间变色并放大，提供直观的成功提示。
*   **奖励重塑 (Reward Shaping)**：
    *   引入 **距离惩罚 (Distance Penalty)**：离目标越远扣分越多，迫使机械臂主动出击。
    *   引入 **时间惩罚**：鼓励快速完成任务。

---

## 🛠️ 技术栈与环境配置 (Tech Stack)

为了确保实验的可复现性，请严格遵循以下版本配置：

*   **Unity Editor**: 2020.3.25f1c1 (LTS)
*   **ML-Agents Package**: Release 19
*   **Python Library**: `mlagents == 0.28.0`
*   **Python**: 3.7

---

## 📂 目录结构 (Directory Structure)

```text
├── Assets/
│   ├── Scripts/            # 核心 C# 脚本 (ReacherRobot.cs 等)
│   ├── RobotReacher.onnx   # 🧠 训练好的 .onnx 模型文件 (直接可用)
│   └── ...
├── Packages/               # Unity 插件依赖
└── ProjectSettings/        # 项目设置
