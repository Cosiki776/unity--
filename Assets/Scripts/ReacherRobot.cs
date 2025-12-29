using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;


public class ReacherRobot : Agent
{
    public GameObject pendulumA;
    public GameObject pendulumB;
    public GameObject pendulumC;
    public GameObject pendulumD;
    public GameObject pendulumE;
    public GameObject pendulumF;

    Rigidbody m_RbA;
    Rigidbody m_RbB;
    Rigidbody m_RbC;
    Rigidbody m_RbD;
    Rigidbody m_RbE;
    Rigidbody m_RbF;

    public GameObject hand;
    public GameObject goal;

    public float m_GoalHeight = 1.2f;
    float m_GoalRadius;          // Radius of the goal zone
    float m_GoalDegree;          // How much the goal rotates
    float m_GoalSpeed;           // Speed of the goal rotation
    float m_GoalDeviation;       // How much goes up and down
    float m_GoalDeviationFreq;   // Frequency of the goal up and down movement

    // 【新增】: 控制球是否移动的开关
    // 建议：刚开始训练设为 false (静止球)，训练 50-100万步变聪明后，再改为 true (移动球)
    public bool useMovingGoal = false; 
    
    public override void Initialize()
    {
        m_RbA = pendulumA.GetComponent<Rigidbody>();
        m_RbB = pendulumB.GetComponent<Rigidbody>();
        m_RbC = pendulumC.GetComponent<Rigidbody>();
        m_RbD = pendulumD.GetComponent<Rigidbody>();
        m_RbE = pendulumE.GetComponent<Rigidbody>();
        m_RbF = pendulumF.GetComponent<Rigidbody>();

        SetResetParameters();

    }

    // 【重要修改】: OnEpisodeBegin (回合开始)
    // 改动目的：防止 AI "背板" (过拟合)。让它每次从不同姿势、不同目标位置开始。
    public override void OnEpisodeBegin()
    {
        // pendulumA.transform.position = new Vector3(0f, 0.55f, 0f) + transform.position;
        // pendulumA.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        // m_RbA.velocity = Vector3.zero;
        // m_RbA.angularVelocity = Vector3.zero;

        // pendulumB.transform.position = new Vector3(-0.15f, 0.55f, 0f) + transform.position;
        // pendulumB.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        // m_RbB.velocity = Vector3.zero;
        // m_RbB.angularVelocity = Vector3.zero;

        // pendulumC.transform.position = new Vector3(-0.15f, 1.375f, 0f) + transform.position;
        // pendulumC.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        // m_RbC.velocity = Vector3.zero;
        // m_RbC.angularVelocity = Vector3.zero;

        // pendulumD.transform.position = new Vector3(-0.15f, 1.375f, 0f) + transform.position;
        // pendulumD.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        // m_RbD.velocity = Vector3.zero;
        // m_RbD.angularVelocity = Vector3.zero;

        // pendulumE.transform.position = new Vector3(-0.15f, 2f, 0f) + transform.position;
        // pendulumE.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        // m_RbE.velocity = Vector3.zero;
        // m_RbE.angularVelocity = Vector3.zero;

        // pendulumF.transform.position = new Vector3(-0.15f, 2.11f, 0f) + transform.position;
        // pendulumF.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        // m_RbF.velocity = Vector3.zero;
        // m_RbF.angularVelocity = Vector3.zero;

        // --- 1. 随机化机械臂初始姿态 ---
        // 以前是固定的 -90度,现在我们在一定范围内随机,强迫 AI 学会从任何姿势调整。
        ResetPendulum(pendulumA, m_RbA, new Vector3(0f, 0.55f, 0f), -90f, 20f); 
        ResetPendulum(pendulumB, m_RbB, new Vector3(-0.15f, 0.55f, 0f), -90f, 20f);
        ResetPendulum(pendulumC, m_RbC, new Vector3(-0.15f, 1.375f, 0f), -90f, 20f);
        ResetPendulum(pendulumD, m_RbD, new Vector3(-0.15f, 1.375f, 0f), -90f, 20f);
        ResetPendulum(pendulumE, m_RbE, new Vector3(-0.15f, 2f, 0f), -90f, 20f);
        ResetPendulum(pendulumF, m_RbF, new Vector3(-0.15f, 2.11f, 0f), -90f, 20f);

        SetResetParameters();

        // m_GoalDegree += m_GoalSpeed;
        // UpdateGoalPosition();
        // --- 3. 决定球的位置 (静止 vs 移动) ---
        if (useMovingGoal)
        {
            // 如果开启移动，使用原来的逻辑，球会转圈
            m_GoalDegree += m_GoalSpeed;
            UpdateGoalPosition(); 
        }
        else
        {
            // 【新增】如果关闭移动，球静止出现在随机位置
            // 这里的范围 (Random.insideUnitSphere * 4f) 需要根据你机械臂的长度调整
            // 确保球不会生成在够不到的地方
            Vector3 randomPos = UnityEngine.Random.insideUnitSphere * 1.9f; 
            randomPos.y = Mathf.Abs(randomPos.y) + 0.5f; // 保证球在地面以上
            
            // 设置球的位置（相对于基座）
            goal.transform.position = transform.position + randomPos;
            
            m_GoalSpeed = 0f; // 告诉神经网络球的速度是0
        }
    }

    // 辅助函数：简化重复的重置代码
    void ResetPendulum(GameObject go, Rigidbody rb, Vector3 localOffset, float baseAngle, float randomRange)
    {
        go.transform.position = localOffset + transform.position;
        // 随机角度：在 baseAngle 基础上左右摆动 randomRange 度
        float randomRot = baseAngle + Random.Range(-randomRange, randomRange);
        go.transform.rotation = Quaternion.Euler(randomRot, Random.Range(-10f, 10f), 0f);
        
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void SetResetParameters()
    {
        m_GoalRadius = Random.Range(1f, 1.3f);
        m_GoalDegree = Random.Range(0f, 360f);
        // m_GoalSpeed = Random.Range(-2f, 2f);
        m_GoalSpeed = Random.Range(-2f, 2f);
        m_GoalDeviation = Random.Range(-1f, 1f);
        m_GoalDeviationFreq = Random.Range(0f, 3.14f);
    }

    // 【重要修改】:CollectObservations (眼睛)
    // 改动目的：增加“相对位移”，让 AI 直接感知目标在哪，而不是靠猜。
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pendulumA.transform.localPosition);
        sensor.AddObservation(pendulumA.transform.rotation);
        sensor.AddObservation(m_RbA.angularVelocity);
        sensor.AddObservation(m_RbA.velocity);

        sensor.AddObservation(pendulumB.transform.localPosition);
        sensor.AddObservation(pendulumB.transform.rotation);
        sensor.AddObservation(m_RbB.angularVelocity);
        sensor.AddObservation(m_RbB.velocity);

        sensor.AddObservation(pendulumC.transform.localPosition);
        sensor.AddObservation(pendulumC.transform.rotation);
        sensor.AddObservation(m_RbC.angularVelocity);
        sensor.AddObservation(m_RbC.velocity);

        sensor.AddObservation(pendulumD.transform.localPosition);
        sensor.AddObservation(pendulumD.transform.rotation);
        sensor.AddObservation(m_RbD.angularVelocity);
        sensor.AddObservation(m_RbD.velocity);

        sensor.AddObservation(pendulumE.transform.localPosition);
        sensor.AddObservation(pendulumE.transform.rotation);
        sensor.AddObservation(m_RbE.angularVelocity);
        sensor.AddObservation(m_RbE.velocity);

        sensor.AddObservation(pendulumF.transform.localPosition);
        sensor.AddObservation(pendulumF.transform.rotation);
        sensor.AddObservation(m_RbF.angularVelocity);
        sensor.AddObservation(m_RbF.velocity);

        // --- 下面是原来的 ---
        sensor.AddObservation(goal.transform.localPosition);
        sensor.AddObservation(hand.transform.localPosition);

        // --- 【新增关键观察值】 ---
        // 1. 向量：直接告诉 AI 手指和球的坐标差 (x,y,z)。这比单独给两个坐标有效得多！
        sensor.AddObservation(goal.transform.localPosition - hand.transform.localPosition);

        // 2. 距离：直接告诉 AI 还有多远 (float)
        sensor.AddObservation(Vector3.Distance(goal.transform.localPosition, hand.transform.localPosition));


        sensor.AddObservation(m_GoalSpeed);

    }

    // 【重要修改】:OnActionReceived (行动与奖励)
    // 改动目的：增加“距离惩罚”，迫使 AI 主动去追球，而不是等球撞上来。
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var a = actionBuffers.ContinuousActions;

        var torque = Mathf.Clamp(a[0], -1f, 1f) * 150f;
        m_RbA.AddTorque(new Vector3(0f, torque, 0f));

        torque = Mathf.Clamp(a[1], -1f, 1f) * 150f;
        m_RbB.AddTorque(new Vector3(0f, 0f, torque));

        torque = Mathf.Clamp(a[2], -1f, 1f) * 150f;
        m_RbC.AddTorque(new Vector3(0f, 0f, torque));

        torque = Mathf.Clamp(a[3], -1f, 1f) * 150f;
        m_RbD.AddTorque(new Vector3(0f, torque, 0f));

        torque = Mathf.Clamp(a[4], -1f, 1f) * 150f;
        m_RbE.AddTorque(new Vector3(0f, 0f, torque));

        torque = Mathf.Clamp(a[5], -1f, 1f) * 150f;
        m_RbF.AddTorque(new Vector3(0f, torque, 0f));

        // --- 【新增：奖励函数 Shaping】 ---
        
        // 1. 计算距离
        float distanceToGoal = Vector3.Distance(hand.transform.position, goal.transform.position);

        // 2. 距离惩罚 (Distance Penalty)
        // 离球越远，扣分越多。这会逼迫 AI 尽快靠近球。
        // 系数 0.001f 可以微调。太大了会掩盖掉碰到球的奖励，太小了 AI 感觉不到。
        AddReward(-distanceToGoal * 0.001f);

        // 3. 时间惩罚 (Time Penalty)
        // 鼓励 AI 快速完成任务，不要磨洋工。
        AddReward(-0.0005f);

        // ---------------------------------

        // 只有开启了移动球模式，才更新球的位置
        if (useMovingGoal)
        {
            m_GoalDegree += m_GoalSpeed;
            UpdateGoalPosition();
        }

        // m_GoalDegree += m_GoalSpeed;
        // UpdateGoalPosition();
    }

    void UpdateGoalPosition()
    {
        var m_GoalDegree_rad = m_GoalDegree * Mathf.PI / 180f;

        var goalX = m_GoalRadius * Mathf.Cos(m_GoalDegree_rad);
        var goalZ = m_GoalRadius * Mathf.Sin(m_GoalDegree_rad);
        var goalY = m_GoalHeight + m_GoalDeviation * Mathf.Cos(m_GoalDeviationFreq * m_GoalDegree_rad);

        goal.transform.position = new Vector3(goalX, goalY, goalZ) + transform.position;
    }

    // 把这个方法加到 ReacherRobot 类的最后面
    void OnDrawGizmosSelected()
    {
        // 这里的 2.5f 要和你上面 OnEpisodeBegin 里写的 spawnRadius 一致
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.9f);
    }



}
