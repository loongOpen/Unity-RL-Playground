using UnityEngine;

public class GaitAnimator : MonoBehaviour
{
    int tp = 0;
    int tp0 = 0;
    int T1 = 20;
    float uf1 = 0;
    float uf2 = 0;
    float dh = 30;
    float d0 = 30;
    float[] utotal = new float[12];
    ArticulationBody[] acts;
    int ActionNum = 12;

    void Start()
    {
        // 自动收集12个RevoluteJoint类型的关节
        var allJoints = GetComponentsInChildren<ArticulationBody>();
        acts = new ArticulationBody[12];
        int idx = 0;
        foreach (var joint in allJoints)
        {
            if (joint.jointType == ArticulationJointType.RevoluteJoint && idx < 12)
            {
                acts[idx++] = joint;
            }
        }
        // 如果你的机器人关节数不是12，请根据实际情况调整
    }

    void FixedUpdate()
    {
        tp++;
        if (tp > 0 && tp <= T1)
        {
            tp0 = tp;
            uf1 = (-Mathf.Cos(Mathf.PI * 2 * tp0 / T1) + 1f) / 2f;
            uf2 = 0;
        }
        else if (tp > T1 && tp <= 2 * T1)
        {
            tp0 = tp - T1;
            uf1 = 0;
            uf2 = (-Mathf.Cos(Mathf.PI * 2 * tp0 / T1) + 1f) / 2f;
        }
        if (tp >= 2 * T1) tp = 0;

        for (int i = 0; i < ActionNum; i++) utotal[i] = 0;
        utotal[1] += dh * uf1 + d0;
        utotal[2] += (dh * uf1 + d0) * -2;
        utotal[4] += dh * uf2 + d0;
        utotal[5] += (dh * uf2 + d0) * -2;
        utotal[7] += dh * uf2 + d0;
        utotal[8] += (dh * uf2 + d0) * -2;
        utotal[10] += dh * uf1 + d0;
        utotal[11] += (dh * uf1 + d0) * -2;

        for (int i = 0; i < ActionNum; i++)
        {
            if (acts[i] != null)
                SetJointTargetDeg(acts[i], utotal[i]);
        }
    }

    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 180f;
        drive.damping = 8f;
        drive.target = x;
        joint.xDrive = drive;
    }
}