using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Unity.Sentis;


public class G1opAgent : Agent
{
    int tp = 0;
    int tq = 0;
    int tt = 0;
    public IKG1 g1ik;
    public bool fixbody = false;
    public bool train;
    public bool accelerate;
    public bool keyboard=false;
    public float vx=0;
    public float vz=0;
    public float wr=0;
    float uff = 0;
    float uf1 = 0;
    float uf2 = 0;
    float[] u = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float[] ut = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float[] utt = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float[] utotal = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    int T1 = 50;
    int T2 = 30;
    int tp0 = 0;
    int ran;

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
    float[] kb1 = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float[] kb2 = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float dh = 25;
    float d0 = 15;
    float ko = 2;
    float kh = 0;

    
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


    private bool _isClone = false; 
    void Start()
    {
        Time.fixedDeltaTime = 0.01f;
        Screen.SetResolution(1920, 1080, true);
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");
        SerializedProperty layer = layers.GetArrayElementAtIndex(15);
        int targetLayer = LayerMask.NameToLayer("robot");
        layer.stringValue = "robot";
        tagManager.ApplyModifiedProperties();
        Physics.IgnoreLayerCollision(15, 15, true);
        ChangeLayerRecursively(gameObject, 15);

        if (train && !_isClone) 
        {
            for (int i = 1; i <14; i++)
            {
                GameObject clone = Instantiate(gameObject); 
                clone.transform.position = transform.position + new Vector3(i * 2f, 0, 0);
                clone.name = $"{name}_Clone_{i}"; 
                clone.GetComponent<G1opAgent>()._isClone = true; 
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
        tp = Random.Range(0,2*T1);
        tq = 0;
        tt = 0;
        for (int i = 0; i< 12; i++) u[i] = 0;
        for (int i = 0; i < 12; i++) ut[i] = 0;
        for (int i = 0; i < 12; i++) utt[i] = 0;

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

        //if(train)
        {
            wr=0;
            vx=0;
            vz=0;
            //vz=(Random.Range(0,2)-0.5f)*3;
            //vx=(Random.Range(0,2)-0.5f)*1;
            //wr=(Random.Range(0,2)-0.5f)*2;
            ran = Random.Range(0,2);
            //if(ran==0)wr=(Random.Range(0,2)-0.5f)*2;
            //if(ran==1 || ran==0)vz=(Random.Range(0,2)-0.5f)*2;
            //if(ran==1)vx=(Random.Range(0,2)-0.5f)*1;
        }
        
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
        sensor.AddObservation(vx);
        sensor.AddObservation(vz);
        sensor.AddObservation(wr);
        sensor.AddObservation(Mathf.Sin(3.14f * 2 * tp0 / T1));
        sensor.AddObservation(Mathf.Cos(3.14f * 2 * tp0 / T1));
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
            ut[i] += u[i];
            utt[i] += ut[i];
            utotal[i] = kb[i] * u[i] + kb1[i] * ut[i] + kb2[i] * utt[i];
            if (fixbody) utotal[i] = 0;
        }

        string name = this.name;
        int[] idx = new int[6] { 2, 3, 4, 7, 8, 9 };
        float[] ktemp1 = new float[12] { 5, 5, 30, 60, 30, 5, 5, 30, 60, 30, 0, 0 };
        if (name.Contains("G1"))
        {
            idx = new int[6] { -1, -4, -5, -7, -10, -11 };
            ktemp1 = new float[12] { 40, 30, 15, 20, 40, 30,    40, 30, 15, 20, 40, 30 };
            ko = 01f;
            d0 = 10;
            dh = 20;
        }
        for (int i = 0; i < 12; i++) kb[i] = ktemp1[i];
        T1 = 40;
        if(vx==0 && Mathf.Abs(vz)<=0.2f && Mathf.Abs(wr)<=0.2f)
        {
            if(Mathf.Abs(EulerTrans(body.eulerAngles[0]))<10f && Mathf.Abs(EulerTrans(body.eulerAngles[2]))<10f)dh=0;
        }
        utotal[Mathf.Abs(idx[0]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[0]);
        utotal[Mathf.Abs(idx[1]) - 1] -= 2 * (dh * uf1 + d0) * Mathf.Sign(idx[1]);
        utotal[Mathf.Abs(idx[2]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[2]);
        utotal[Mathf.Abs(idx[3]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[3]);
        utotal[Mathf.Abs(idx[4]) - 1] -= 2 * (dh * uf2 + d0) * Mathf.Sign(idx[4]);
        utotal[Mathf.Abs(idx[5]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[5]);
            
        utotal[1] = Mathf.Clamp(utotal[1], -200, 8f);
        utotal[7] = Mathf.Clamp(utotal[7], -8f, 200f);
        for (int i = 0; i < 12; i++) 
        {
            if(i==4 || i==5 || i==10 || i==11)SetJDeg(acts[i], utotal[i], 20f, 2f);
            else SetJDeg(acts[i], utotal[i], 200f, 20f);
        }
        float[] ang = g1ik.GetAng();
        for (int i = 12; i < 15; i++)SetJointTargetDeg(acts[i], 0);
        for (int i = 15; i < ActionNum; i++) SetJointTargetDeg(acts[i], ang[i-15]*180f/3.14f);
        float[] grasp=new float[12] {40,40,40,40,  -40,-40,   -40,-40,-40,-40,  40,40};
        if(Input.GetKey(KeyCode.Alpha1))for (int i = 6; i <12; i++)hp[i]=Mathf.MoveTowards(hp[i], grasp[i], 1f);
        if(Input.GetKey(KeyCode.Alpha2))for (int i = 6; i <12; i++)hp[i]=Mathf.MoveTowards(hp[i], 0, 1f);
        if(Input.GetKey(KeyCode.Alpha9))for (int i = 0; i <6; i++)hp[i]=Mathf.MoveTowards(hp[i], grasp[i], 1f);
        if(Input.GetKey(KeyCode.Alpha0))for (int i = 0; i <6; i++)hp[i]=Mathf.MoveTowards(hp[i], 0, 1f);
        for (int i = 0; i <12; i++)SetJDeg(hand[i], hp[i], 2000f, 200f);

                
        if(keyboard)
        {
            float v=0.01f;
            if(Input.GetKey(KeyCode.W))vz=Mathf.MoveTowards(vz, 1.5f, v);
            else if(Input.GetKey(KeyCode.S))vz=Mathf.MoveTowards(vz, -1.5f, v);
            else vz=Mathf.MoveTowards(vz, 0f, v);

            if(Input.GetKey(KeyCode.A))wr=Mathf.MoveTowards(wr, -1.2f, v);
            else if(Input.GetKey(KeyCode.D))wr=Mathf.MoveTowards(wr, 1.2f, v);
            else wr=Mathf.MoveTowards(wr, 0f, v);
            
        }
        else
        {
            if(ran==0)vz=1.5f*Mathf.Sin(6.28f*Time.time/5f);
            else wr=1.2f*Mathf.Sin(6.28f*Time.time/5f);
        }
    }
    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 200f;
        drive.damping = 10f;
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
    void SetJointTargetPosition(ArticulationBody joint, float x)
    {
        x = (x + 1f) * 0.5f;
        var x1 = Mathf.Lerp(joint.xDrive.lowerLimit, joint.xDrive.upperLimit, x);
        var drive = joint.xDrive;
        drive.stiffness = 2000f;
        drive.damping = 100f;
        drive.forceLimit = 200f;
        drive.target = x1;
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
        uff = (-Mathf.Cos(3.14f * 2 * tq / T2) + 1f) / 2f;
        if (tq >= T2) tq = 0;

        

        var vel = body.InverseTransformDirection(arts[0].velocity);
        var wel = body.InverseTransformDirection(arts[0].angularVelocity);
        var live_reward = 1f;
        var ori_reward1 = -0.3f * Mathf.Abs(EulerTrans(body.eulerAngles[0]));//-0.5f * Mathf.Min(Mathf.Abs(body.eulerAngles[0]), Mathf.Abs(body.eulerAngles[0] - 360f));
        var abswr=Mathf.Abs(wr);
        if(abswr==0)abswr=1;
        var ori_reward2 = (1-2f * Mathf.Abs(wel[1]-wr));
        var ori_reward3 = -0.3f * Mathf.Abs(EulerTrans(body.eulerAngles[2]));
        var vel_reward1 = vel[2] - Mathf.Abs(vel[0]);
        var absvz=Mathf.Abs(vz);
        var absvx=Mathf.Abs(vx);
        if(absvz==0)absvz=1;
        if(absvx==0)absvx=1;
        var vel_reward2 = (1 - 2*Mathf.Abs(vel[2]-vz)) +( - Mathf.Abs(vel[0]-vx))+ 0*kh * Mathf.Abs(vel[1]);
        //if(dh==0)vel_reward2+=1;
        ko=1;
        var reward = live_reward + (ori_reward1 + ori_reward2 + ori_reward3) * ko + vel_reward2;
        AddReward(reward);
        //(foot1.position-foot2.position).magnitude<0.1f || 
        if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > 30f || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > 30f)
        {
            //if(train)
            EndEpisode();
        }

    }

}
