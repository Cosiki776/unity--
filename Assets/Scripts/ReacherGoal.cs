using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReacherGoal : MonoBehaviour
{
    public GameObject agent;
    public GameObject hand;
    public GameObject goalOn;
    public Material successMaterial; // 在 Inspector 里把 Agent Blue 拖到这里
    public Material defaultMaterial; // 把原来的红色材质拖到这里

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.gameObject == hand)
    //     {
    //         // 1. 变色：直接替换材质
    //         goalOn.GetComponent<MeshRenderer>().material = successMaterial;

    //         goalOn.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

    //         // 1. 给一个大的奖励，告诉它“你做到了！”
    //         agent.GetComponent<ReacherRobot>().AddReward(1.0f); 
            
    //         // 2. 也是最重要的一步：结束当前回合，重置环境
    //         agent.GetComponent<ReacherRobot>().EndEpisode();
            
    //     }
    // }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == hand)
        {
            // 开启一个异步过程
            StartCoroutine(SuccessVisualFeedback());
        }
    }

    IEnumerator SuccessVisualFeedback()
    {
        // 1. 立即执行视觉变化
        goalOn.GetComponent<MeshRenderer>().material = successMaterial;
        goalOn.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

        // 2. 立即给分
        agent.GetComponent<ReacherRobot>().AddReward(1.0f);

        // 3. 关键：暂停 0.5 秒，这期间你会看到蓝色小球
        yield return new WaitForSeconds(0.5f);

        // 4. 时间到，再重置回合
        agent.GetComponent<ReacherRobot>().EndEpisode();
    }

    
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == hand)
        {
            goalOn.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);
            goalOn.GetComponent<MeshRenderer>().material = defaultMaterial;
        }
    }

    
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == hand)
        {
            agent.GetComponent<ReacherRobot>().AddReward(0.01f);
        }
    }
}
