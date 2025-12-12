using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Unity.Sentis;
using System.Threading.Tasks;

public class Tinker4Agent : Agent
{
    int tp = 0;
    int tt = 0;

    public bool wasd = false;
    public bool keyboard = false;
    public bool fixbody = false;
    public bool train;
    public bool accelerate;
    float uf1 = 0;
    float uf2 = 0;
    float[] u = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float[] utotal = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    int T1 = 50;
    int tp0 = 0;
    
    Transform body;
    public int ObservationNum;
    public int ActionNum;
    public ModelAsset WalkPolicy;
    public ModelAsset StandupPolicy;
    public int policy=0;
    public float t0=0;
    public float t1=0;

    List<float> P0 = new List<float>();
    List<float> W0 = new List<float>();
    List<Transform> bodypart = new List<Transform>();
    Vector3 pos0;
    Quaternion rot0;
    ArticulationBody[] arts = new ArticulationBody[40];
    ArticulationBody[] acts = new ArticulationBody[20];
    GameObject robot;

    float[] kb = new float[12] { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };
    public float vr = 0;
    public float wr = 0;
    public float cr = 0;
    public bool disturb = false;
    public float UpDotVertical = 0f; // cube的up坐标轴与竖直轴的点积
    public bool lift=false;
    public float fy=70f;
    public override void Initialize()
    {
        arts = this.GetComponentsInChildren<ArticulationBody>();
        ActionNum = 0;
        for (int k = 0; k < arts.Length; k++)
        {
            if(arts[k].jointType.ToString() == "RevoluteJoint")
            {
                acts[ActionNum] = arts[k];
                print(acts[ActionNum]);
                ActionNum++;
            }
        }
        ActionNum = 10;
        body = arts[0].GetComponent<Transform>();
        pos0 = body.position;
        rot0 = body.rotation;
        arts[0].GetJointPositions(P0);
        arts[0].GetJointVelocities(W0);
        accelerate = train;
       
    }

    private bool _isClone = false; 
    void Start()
    {
        Time.fixedDeltaTime = 0.01f;    
        
        int numrob=8;
        if(train)numrob=32;
        if(fixbody || keyboard)numrob=0;
        if (!_isClone) 
        {
            for (int i = 1; i < numrob; i++)
            {
                GameObject clone = Instantiate(gameObject); 
                //clone.transform.position = transform.position + new Vector3(i * 2f, 0, 0);
                clone.name = $"{name}_Clone_{i}"; 
                clone.GetComponent<Tinker4Agent>()._isClone = true; 
            }
        }
    }
    void ChangeLayerRecursively(GameObject obj, int targetLayer)
    {
        obj.layer = targetLayer;
        foreach (Transform child in obj.transform)ChangeLayerRecursively(child.gameObject, targetLayer);
    }

    public override void OnEpisodeBegin()
    {
        tp = 0;
        tt = 0;
        for (int i = 0; i< 12; i++) u[i] = 0;
        
        Quaternion randRot = rot0 * Quaternion.Euler(Random.Range(-80f,80f), Random.Range(-180f,180f), -90);//Random.Range(-80f,80f));
        float px;
        float pz;
        if(Random.Range(0,2)==0)
        {
            px = 1*(Random.Range(0,2)*2-1);
            pz = Random.Range(-1f,1f);
        }
        else
        {
            pz = 1*(Random.Range(0,2)*2-1);
            px = Random.Range(-1f,1f);
        }
        
        Vector3 randPos = new Vector3(pos0[0]+px, pos0[1], pos0[2]+pz);
        ObservationNum = 9 + 2 * ActionNum;
        if (fixbody) arts[0].immovable = true;
        if (keyboard)
        {
            randPos=pos0;
            randRot=rot0;
        }
        if (!fixbody)
        {
            arts[0].TeleportRoot(randPos, randRot);
            arts[0].velocity = Vector3.zero;
            arts[0].angularVelocity = Vector3.zero;
            arts[0].SetJointPositions(P0);
            arts[0].SetJointVelocities(W0);
        }
        //if(train)
        {
            vr=0;
            wr=0;

            if(Random.Range(0,3)==0)vr = Random.Range(0.3f,0.6f)*(Random.Range(0,2)*2-1);
            else if(Random.Range(0,2)==0) wr = Random.Range(0.3f,0.6f)*(Random.Range(0,2)*2-1);
            cr = 0.5f;//Random.Range(0.1f,0.7f);
            
        }
        /*else
        {
            vr=0;
            wr=0;
            cr=0.5f;
        }*/
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(body.InverseTransformDirection(Vector3.down));
        sensor.AddObservation(EulerTrans(body.eulerAngles[0])*3.14f/180f);
        sensor.AddObservation(EulerTrans(body.eulerAngles[2])*3.14f/180f);
        sensor.AddObservation(body.InverseTransformDirection(arts[0].angularVelocity));
        sensor.AddObservation(body.InverseTransformDirection(arts[0].velocity));
        for (int i = 0; i < ActionNum; i++)
        {
            sensor.AddObservation(acts[i].jointPosition[0]);
            sensor.AddObservation(acts[i].jointVelocity[0]);
        }
        sensor.AddObservation(vr);
        sensor.AddObservation(wr);
        sensor.AddObservation(cr);
        sensor.AddObservation(Mathf.Sin(3.14f * 1 * tp / T1));
        sensor.AddObservation(Mathf.Cos(3.14f * 1 * tp / T1));
    }
    float EulerTrans(float eulerAngle)
    {
        if (eulerAngle <= 180)
            return eulerAngle;
        else
            return eulerAngle - 360f;
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        for (int i = 0; i < 12; i++) utotal[i] = 0;
        var continuousActions = actionBuffers.ContinuousActions;
        var kk = 0.9f;
        
        kb = new float[10] { 15, 30, 40, 15, 40,   15, 30, 40, 15, 40};
        for (int i = 0; i < ActionNum; i++)
        {
            u[i] = u[i] * kk + (1 - kk) * continuousActions[i];
            if(policy==1)
            {
                if(t1%4>0 && t1%4<0.4) u[i] = 0;
            }
            if(policy==1)utotal[i] = 2.5f * kb[i] * u[i];
            if(policy==0)utotal[i] = 2f * kb[i] * u[i];
            if (fixbody) utotal[i] = 0;
        }
        

        int[] idx = new int[6] { -3, 4, 5, 8, -9, -10 };
        
        T1 = 30;
        float d0 = cr*180f/3.14f;//10;
        float dh = 40;
        if(vr==0 && wr==0 && !fixbody)
        {
            /*if(Mathf.Abs(EulerTrans(body.eulerAngles[0])) < 10f && Mathf.Abs(EulerTrans(body.eulerAngles[2])) < 10f)
                dh=0;///////////////////////////////////////////////////
            else dh=40;*/

            if(tt>450 && tt<600)dh=0;
            if(tt>840)dh=0;
            if(keyboard)dh=0;
        }
        if(policy==1)dh = 0;
        //if(Mathf.Abs(EulerTrans(body.eulerAngles[0])) < 30 && Mathf.Abs(EulerTrans(body.eulerAngles[2])) < 30)
        {
            utotal[Mathf.Abs(idx[0]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[0]);
            utotal[Mathf.Abs(idx[1]) - 1] += 2 * (dh * uf1 + d0) * Mathf.Sign(idx[1]);
            utotal[Mathf.Abs(idx[2]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[2]);
            utotal[Mathf.Abs(idx[3]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[3]);
            utotal[Mathf.Abs(idx[4]) - 1] += 2 * (dh * uf2 + d0) * Mathf.Sign(idx[4]);
            utotal[Mathf.Abs(idx[5]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[5]);
        }
        //utotal[1] = Mathf.Clamp(utotal[1], -200f, 0f);
        //utotal[7] = Mathf.Clamp(utotal[7], 0f, 200f);
        for (int i = 0; i < ActionNum; i++) SetJointTargetDeg(acts[i], utotal[i]);
    }
    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 50f;
        drive.damping = 2f;
        //drive.forceLimit = 300f;
        drive.target = x;
        joint.xDrive = drive;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }

    void FixedUpdate()
    {
        
        if (accelerate) Time.timeScale = 20;
        if (!accelerate) Time.timeScale = 1;
 
        tp++;
        if (tp > 0 && tp <= T1)
        {
            tp0 = tp;
            uf1 = (-Mathf.Cos(3.14f * 2 * tp0 / T1) + 1f) / 2f;
            uf2 = 0;
        }
        if (tp > T1 && tp <= 2 * T1)
        {
            tp0 = tp - T1;
            uf1 = 0;
            uf2 = (-Mathf.Cos(3.14f * 2 * tp0 / T1) + 1f) / 2f;
        }
        if (tp >= 2 * T1) tp = 0;

        tt++;
        if(keyboard && wasd)
        {
            float v=0.01f;
            if(Input.GetKey(KeyCode.W))vr=Mathf.MoveTowards(vr, 0.6f, v);
            else if(Input.GetKey(KeyCode.S))vr=Mathf.MoveTowards(vr, -0.6f, v);
            else vr=Mathf.MoveTowards(vr, 0f, v);

            if(Input.GetKey(KeyCode.A))wr=Mathf.MoveTowards(wr, -0.6f, v);
            else if(Input.GetKey(KeyCode.D))wr=Mathf.MoveTowards(wr, 0.6f, v);
            else wr=Mathf.MoveTowards(wr, 0f, v);

            //if(Input.GetKey(KeyCode.Q))cr=Mathf.MoveTowards(cr, 0.1f, v/3f);
            //else if(Input.GetKey(KeyCode.E))cr=Mathf.MoveTowards(cr, 0.7f, v/3f);
        }
        if(keyboard && !wasd)
        {
            float v=0.01f;
            if(Input.GetKey(KeyCode.UpArrow))vr=Mathf.MoveTowards(vr, 0.6f, v);
            else if(Input.GetKey(KeyCode.DownArrow))vr=Mathf.MoveTowards(vr, -0.6f, v);
            else vr=Mathf.MoveTowards(vr, 0f, v);

            if(Input.GetKey(KeyCode.LeftArrow))wr=Mathf.MoveTowards(wr, -0.6f, v);
            else if(Input.GetKey(KeyCode.RightArrow))wr=Mathf.MoveTowards(wr, 0.6f, v);
            else wr=Mathf.MoveTowards(wr, 0f, v);

            //if(Input.GetKey(KeyCode.Q))cr=Mathf.MoveTowards(cr, 0.1f, v/3f);
            //else if(Input.GetKey(KeyCode.E))cr=Mathf.MoveTowards(cr, 0.7f, v/3f);
        }

        //Vector3 randomForce=new Vector3(Random.Range(-1f, 1f),0,Random.Range(-1f, 1f));
        //if(Random.Range(0, 100)==1 && disturb)arts[0].AddForce(2*randomForce, ForceMode.Impulse);
        Vector3 ffy=new Vector3(0,fy,0);
        if(lift)arts[0].AddForce(ffy , ForceMode.Force);

        // 计算cube的up坐标轴与竖直轴的点积
        UpDotVertical = Vector3.Dot(body.up, Vector3.up);

        var vel = body.InverseTransformDirection(arts[0].velocity);
        var wel = body.InverseTransformDirection(arts[0].angularVelocity);
        var live_reward = 1f;
        var ori_reward1 = -0.1f * Mathf.Abs(EulerTrans(body.eulerAngles[0]));
        var ori_reward2 = -0.1f * Mathf.Min(Mathf.Abs(body.eulerAngles[2]), Mathf.Abs(body.eulerAngles[2] - 360f));
        var wel_reward = 1 - 4*Mathf.Abs(wel[1] - wr);
        var vel_reward = 1 - 4*Mathf.Abs(vel[2] - vr) + 0*Mathf.Clamp(vel[2],-5f,1.5f) - Mathf.Abs(vel[0]);
        
        // 计算u[i]的绝对值之和
        float uSumAbs = 0f;
        for (int i = 0; i < u.Length; i++)
        {
            uSumAbs += Mathf.Abs(u[i]);
        }
        var u_penalty = -0.3f * uSumAbs;
        
        var reward = live_reward + (ori_reward1 + ori_reward2) * 1 +  wel_reward * 1 + vel_reward + u_penalty;
        if(UpDotVertical<0.7f)u_penalty = -2f;
        reward = UpDotVertical+u_penalty;

        AddReward(reward);
        float fallang=10f;
        //if(train)fallang=20f;
        if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > fallang || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > fallang)
        {
            //if(train)EndEpisode();
            //if(policy==0)for (int i = 0; i< 12; i++) u[i] = 0;
            policy=1; 
            t0=0;
        }
        else t0+=Time.fixedDeltaTime;
        if(t0>0.4f)policy=0;
        if (policy==0) 
        {
            SetModel("gewu", WalkPolicy);
            t1=0;
        }
        if (policy==1) 
        {
            SetModel("gewu", StandupPolicy);
            t1+=Time.fixedDeltaTime;
            
        }
        //if(!train && tt>500)EndEpisode();
    }

}
