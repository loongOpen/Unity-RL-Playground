using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class trackjoint : MonoBehaviour
{
    public JointAngleMonitor monitor;
    ArticulationBody[] arts = new ArticulationBody[40];
    ArticulationBody[] acts = new ArticulationBody[40];
    ArticulationBody[] hand = new ArticulationBody[20];
    int ActionNum;
    int Handnum;
    
    // Start is called before the first frame update
    void Start()
    {
        arts = this.GetComponentsInChildren<ArticulationBody>();
        for (int k = 0; k < arts.Length; k++)
        {
            if(arts[k].jointType.ToString() == "RevoluteJoint")
            {
                if(arts[k].ToString().Contains("hand"))
                {
                    hand[Handnum] = arts[k];
                    Handnum++;
                }
                else
                {
                    acts[ActionNum] = arts[k];
                    //print(acts[ActionNum]);
                    ActionNum++;
                }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var rad = monitor.GetAnglesRad();
        //print(rad.Length);
        for(int i=0;i<15;i++)SetJointTargetRad(acts[i],0,2000f,200f);
        for(int i=15;i<22;i++)SetJointTargetRad(acts[i],rad[i-12],60f,1.5f);
        for(int i=22;i<29;i++)SetJointTargetRad(acts[i],rad[i-6],60f,1.5f);
        float[] grasp=new float[12] {-40,-40,-40,-40,  40,40,      40,40,40,40,  -40,-40   };
        for (int i = 0; i <12; i++)SetJointTargetRad(hand[i], 2*grasp[i]*3.14f/180f, 60f, 1.5f);
    }
    void SetJointTargetRad(ArticulationBody joint, float x,float kp,float kd)
    {
        var drive = joint.xDrive;
        drive.stiffness = kp;//60f;//2000f;
        drive.damping = kd;//1.5f;//200f;
        //drive.forceLimit = 300f;
        drive.target = x*180f/3.14f;
        joint.xDrive = drive;
    }
}
