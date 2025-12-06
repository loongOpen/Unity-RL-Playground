using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKG1 : MonoBehaviour
{
    public Transform body;
    public Transform hand1;
    public Transform hand2;
    public float handspeed;
    Vector3 pos10;
    Vector3 pos20;
    Vector3 pos11;
    Vector3 pos21;
    ArticulationBody[] arts = new ArticulationBody[40];
    ArticulationBody[] acts = new ArticulationBody[40];
    float[] hr=new float[12];
    int ActionNum;
    void Start()
    {
        arts = this.GetComponentsInChildren<ArticulationBody>();
        ActionNum = 0;
        for (int k = 0; k < arts.Length; k++)
        {
            if(arts[k].jointType.ToString() == "RevoluteJoint")
            {
                acts[ActionNum] = arts[k];
                //print(acts[ActionNum]);
                ActionNum++;
            }
        }
        //print(ActionNum);
        pos10=hand1.position-body.position;
        pos20=hand2.position-body.position;
        for (int k = 0; k < 14; k++)SetJointTargetDeg(acts[k],0);

        handspeed = 0.0008f;
    }
    void FixedUpdate()
    {
         var dp=0.5f;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[0]+=dp;
            else pos11.y+=handspeed;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[0]-=dp;
            else pos11.y-=handspeed;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[1]-=dp;
            else pos11.x-=handspeed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[1]+=dp;
            else pos11.x+=handspeed;
        }
        if (Input.GetKey(KeyCode.Return))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[2]+=dp;
            else pos11.z+=handspeed;
        }
        if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[2]-=dp;
            else pos11.z-=handspeed;
        }

        if (Input.GetKey(KeyCode.X))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[3]+=dp;
            else pos21.y+=handspeed;
        }
        if (Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[3]-=dp;
            else pos21.y-=handspeed;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[4]-=dp;
            else pos21.x-=handspeed;
        }
        if (Input.GetKey(KeyCode.Z))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[4]+=dp;
            else pos21.x+=handspeed;
        }
        if (Input.GetKey(KeyCode.Tab))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[5]+=dp;
            else pos21.z+=handspeed;
        }
        if (Input.GetKey(KeyCode.CapsLock))
        {
            if (Input.GetKey(KeyCode.LeftControl))hr[5]-=dp;
            else pos21.z-=handspeed;
        }

        pos11.y=Mathf.Clamp(pos11.y,-0.1f,0.4f);
        pos11.x=Mathf.Clamp(pos11.x,-0.1f,0.2f);
        pos11.z=Mathf.Clamp(pos11.z,-0.2f,0.1f);
        pos21.y=Mathf.Clamp(pos21.y,-0.1f,0.4f);
        pos21.x=Mathf.Clamp(pos21.x,-0.2f,0.1f);
        pos21.z=Mathf.Clamp(pos21.z,-0.2f,0.1f);
        hand1.position = pos11+pos10+body.position;
        hand2.position = pos21+pos20+body.position;
        if (Input.GetKey(KeyCode.Space))
        {
            //pos11=Vector3.zero;
            //pos21=Vector3.zero;
            pos11.x=Mathf.MoveTowards(pos11.x,0,0.001f);
            pos11.y=Mathf.MoveTowards(pos11.y,0,0.001f);
            pos11.z=Mathf.MoveTowards(pos11.z,0,0.001f);
            pos21.x=Mathf.MoveTowards(pos21.x,0,0.001f);
            pos21.y=Mathf.MoveTowards(pos21.y,0,0.001f);
            pos21.z=Mathf.MoveTowards(pos21.z,0,0.001f);
            for (int k = 0; k < 6; k++)hr[k]=Mathf.MoveTowards(hr[k],0,2f);
        }


        for (int k = 4; k < 7; k++)SetJointTargetDeg(acts[k],hr[k-4]);
        for (int k = 11; k < 14; k++)SetJointTargetDeg(acts[k],hr[k-8]);
    }
    public float[] GetAng()
    {
        float[] ang=new float[14];
        for(int i=0;i<14;i++)ang[i]=acts[i].jointPosition[0];
        return ang;
    }
    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        //joint.angularDamping=10;
        var drive = joint.xDrive;
        drive.stiffness = 10f;
        drive.damping = 1f;
        drive.forceLimit = 10f;
        drive.target = x;
        joint.xDrive = drive;
    }
}
