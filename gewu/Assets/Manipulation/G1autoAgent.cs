using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Unity.Sentis;


public class G1autoAgent : Agent
{
    int tp = 0;
    int tq = 0;
    int tt = 0;
    public IKGauto g1ik;
    public bool fixbody = false;
    public bool train;
    public bool accelerate;
    public float vr = 0;
    public float wr = 0;
    public float cr = 0;
    float uf1 = 0;
    float uf2 = 0;
    float[] u = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float[] utotal = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    int T1 = 50;
    int tp0 = 0;

    List<float> P0 = new List<float>();
    List<float> W0 = new List<float>();
    List<Transform> bodypart = new List<Transform>();
    Vector3 pos0;
    Quaternion rot0;
    ArticulationBody[] arts = new ArticulationBody[40];
    ArticulationBody[] acts = new ArticulationBody[40];
    ArticulationBody[] hand = new ArticulationBody[20];
    GameObject robot;
    float[] hp=new float[12];

    Transform body;
    int ObservationNum;
    int ActionNum;
    int Handnum;

    float[] kb = new float[12] { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };
    float t0;
    public override void Initialize()
    {
        arts = this.GetComponentsInChildren<ArticulationBody>();
        ActionNum = 0;
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
                    print(acts[ActionNum]);
                    ActionNum++;
                }
            }
        }
        body = arts[0].GetComponent<Transform>();
        pos0 = body.position;
        rot0 = body.rotation;
        arts[0].GetJointPositions(P0);
        arts[0].GetJointVelocities(W0);
        accelerate = train;
    }


    void Start()
    {
        Time.fixedDeltaTime = 0.01f;
        t0 = Time.time;
    }


    public override void OnEpisodeBegin()
    {
        tp = 0;
        tp = Random.Range(0,2*T1);
        tq = 0;
        tt = 0;
        for (int i = 0; i< 12; i++) u[i] = 0;

        ObservationNum = 9 + 2 * ActionNum;
        if (fixbody) arts[0].immovable = true;
        if (!fixbody)
        {
            arts[0].TeleportRoot(pos0, rot0);
            arts[0].velocity = Vector3.zero;
            arts[0].angularVelocity = Vector3.zero;
            arts[0].SetJointPositions(P0);
            arts[0].SetJointVelocities(W0);
        }

        vr=0;
        wr=0;
        cr=0.2f;
       
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(body.InverseTransformDirection(Vector3.down));
        sensor.AddObservation(body.InverseTransformDirection(arts[0].angularVelocity));
        sensor.AddObservation(body.InverseTransformDirection(arts[0].velocity));
        for (int i = 0; i < 12; i++)
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
        
        for (int i = 0; i < 12; i++)
        {
            u[i] = u[i] * kk + (1 - kk) * continuousActions[i];
            utotal[i] = 2*kb[i] * u[i];
            if (fixbody) utotal[i] = 0;
        }

        int[] idx = new int[6] { -1, -4, -5, -7, -10, -11 };
        kb = new float[12] { 40, 30, 15, 15, 40, 30,   40, 30, 15, 15, 40, 30};
        T1 = 30;
        float d0 = cr*180f/3.14f;//10;
        float dh = 40;
        if(vr==0 && wr==0)
        {
            if(Mathf.Abs(EulerTrans(body.eulerAngles[0])) < 2f && Mathf.Abs(EulerTrans(body.eulerAngles[2])) < 2f)
                dh=0;///////////////////////////////////////////////////
            else dh=40;
        }
        utotal[Mathf.Abs(idx[0]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[0]);
        utotal[Mathf.Abs(idx[1]) - 1] -= 2 * (dh * uf1 + d0) * Mathf.Sign(idx[1]);
        utotal[Mathf.Abs(idx[2]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[2]);
        utotal[Mathf.Abs(idx[3]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[3]);
        utotal[Mathf.Abs(idx[4]) - 1] -= 2 * (dh * uf2 + d0) * Mathf.Sign(idx[4]);
        utotal[Mathf.Abs(idx[5]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[5]);
            
        utotal[1] = Mathf.Clamp(utotal[1], -200, 0f);
        utotal[7] = Mathf.Clamp(utotal[7], 0f, 200f);
        for (int i = 0; i < 12; i++) SetJDeg(acts[i], utotal[i], 500f, 50f);


        float[] ang = g1ik.GetAng();
        for (int i = 12; i < 15; i++)SetJointTargetDeg(acts[i], 0);
        for (int i = 15; i < ActionNum; i++) SetJointTargetDeg(acts[i], ang[i-15]*180f/3.14f);
        float[] grasp=new float[12] {40,40,40,40,  -40,-40,   -40,-40,-40,-40,  40,40};
        if(Input.GetKey(KeyCode.Alpha1))for (int i = 6; i <12; i++)hp[i]=Mathf.MoveTowards(hp[i], grasp[i], 1f);
        if(Input.GetKey(KeyCode.Alpha2))for (int i = 6; i <12; i++)hp[i]=Mathf.MoveTowards(hp[i], 0, 1f);
        if(Input.GetKey(KeyCode.Alpha9))for (int i = 0; i <6; i++)hp[i]=Mathf.MoveTowards(hp[i], grasp[i], 1f);
        if(Input.GetKey(KeyCode.Alpha0))for (int i = 0; i <6; i++)hp[i]=Mathf.MoveTowards(hp[i], 0, 1f);
        for (int i = 0; i <12; i++)SetJDeg(hand[i], hp[i], 2000f, 200f);

                
        /*float v=0.01f;
        if(Input.GetKey(KeyCode.W))vr=Mathf.MoveTowards(vr, 1.2f, v);
        else if(Input.GetKey(KeyCode.S))vr=Mathf.MoveTowards(vr, -1.2f, v);
        else vr=Mathf.MoveTowards(vr, 0f, v);

        if(Input.GetKey(KeyCode.A))wr=Mathf.MoveTowards(wr, -1f, v);
        else if(Input.GetKey(KeyCode.D))wr=Mathf.MoveTowards(wr, 1f, v);
        else wr=Mathf.MoveTowards(wr, 0f, v);

        if(Input.GetKey(KeyCode.Q))cr=Mathf.MoveTowards(cr, 0.1f, v/3f);
        else if(Input.GetKey(KeyCode.E))cr=Mathf.MoveTowards(cr, 0.7f, v/3f);*/



        vr=Mathf.MoveTowards(vr, 0f, 0.01f);
        wr=Mathf.MoveTowards(wr, 0f, 0.01f);
        if(Time.time>4.2f+t0)for (int i = 6; i <12; i++)hp[i]=Mathf.MoveTowards(hp[i], grasp[i], 1f);
        for (int i = 0; i <12; i++)SetJDeg(hand[i], hp[i], 2000f, 200f);
        if(Time.time>7.5f+t0 && Time.time<10.9f+t0)wr=0.5f;
        if(Time.time>11.2f+t0 && Time.time<11.5f+t0)vr=0.8f;
        if(Time.time>14f+t0 && Time.time<21f+t0)cr=Mathf.MoveTowards(cr,0.6f,0.01f);
        if(Time.time>21f+t0)cr=Mathf.MoveTowards(cr,0.1f,0.01f);
        
    }
    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 2000f;
        drive.damping = 200f;
        //drive.forceLimit = 300f;
        drive.target = x;
        joint.xDrive = drive;
    }
    void SetJDeg(ArticulationBody joint, float x, float kp, float kd)
    {
        var drive = joint.xDrive;
        drive.stiffness = kp;
        drive.damping = kd;
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
        tq++;
        tt++;
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

        

        if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > 30f || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > 30f)
        {
            EndEpisode();
        }

    }

}
