using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Unity.Sentis;
using System.Threading.Tasks;


public class TerrainAgent : Agent
{
    int tp = 0;
    int tt = 0;

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

    List<float> P0 = new List<float>();
    List<float> W0 = new List<float>();
    List<Transform> bodypart = new List<Transform>();
    Vector3 pos0;
    Quaternion rot0;
    ArticulationBody[] arts = new ArticulationBody[40];
    ArticulationBody[] acts = new ArticulationBody[20];
    GameObject robot;

    float[] kb = new float[12] { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };
    float dh = 25;
    float d0 = 15;
    float ko = 0.4f;
    float kw = 1f;
    float khaa = 5f;
    public float vr = 0;
    public float wr = 0;
    public bool wasd = false;

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
        ActionNum = 12;
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
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");
        if(wasd)
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(15);
            int targetLayer = LayerMask.NameToLayer("robot1");
            layer.stringValue = "robot1";
            tagManager.ApplyModifiedProperties();
            Physics.IgnoreLayerCollision(15, 15, true);
            ChangeLayerRecursively(gameObject, 15);
        }
        else
        {
            SerializedProperty layer = layers.GetArrayElementAtIndex(16);
            int targetLayer = LayerMask.NameToLayer("robot2");
            layer.stringValue = "robot2";
            tagManager.ApplyModifiedProperties();
            Physics.IgnoreLayerCollision(16, 16, true);
            ChangeLayerRecursively(gameObject, 16);
        }
        

        //if (train && !_isClone) 
        int numrob=15;
        if(train)numrob=80;
        if (!_isClone) 
        {
            for (int i = 1; i < numrob; i++)
            {
                GameObject clone = Instantiate(gameObject); 
                //clone.transform.position = transform.position + new Vector3(i * 2f, 0, 0);
                clone.name = $"{name}_Clone_{i}"; 
                clone.GetComponent<TerrainAgent>()._isClone = true; 
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
        
        Quaternion randRot = rot0 * Quaternion.Euler(0, Random.Range(-180f,180f), 0);
        float px;
        float pz;
        if(Random.Range(0,2)==0)
        {
            px = 4*(Random.Range(0,2)*2-1);
            pz = Random.Range(-4f,4f);
        }
        else
        {
            pz = 4*(Random.Range(0,2)*2-1);
            px = Random.Range(-4f,4f);
        }
        
        Vector3 randPos = new Vector3(pos0[0]+px, pos0[1], pos0[2]+pz);
        ObservationNum = 9 + 2 * ActionNum;
        if (fixbody) arts[0].immovable = true;
        if (!fixbody)
        {
            arts[0].TeleportRoot(randPos, randRot);
            arts[0].velocity = Vector3.zero;
            arts[0].angularVelocity = Vector3.zero;
            arts[0].SetJointPositions(P0);
            arts[0].SetJointVelocities(W0);
        }
        if(train)wr = Random.Range(-1,2);
        wr=0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(body.InverseTransformDirection(Vector3.down));
        sensor.AddObservation(body.InverseTransformDirection(arts[0].angularVelocity));
        sensor.AddObservation(body.InverseTransformDirection(arts[0].velocity));
        for (int i = 0; i < ActionNum; i++)
        {
            sensor.AddObservation(acts[i].jointPosition[0]);
            sensor.AddObservation(acts[i].jointVelocity[0]);
        }
        sensor.AddObservation(wr);
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
        
        for (int i = 0; i < ActionNum; i++)
        {
            u[i] = u[i] * kk + (1 - kk) * continuousActions[i];
            utotal[i] = 2 * kb[i] * u[i];
            if (fixbody) utotal[i] = 0;
        }
        

        int[] idx = new int[6] { 3, 4, 5, 9, 10, 11 };
        kb = new float[12]{ 20, 20, 30, 30, 30, 20,   20, 20, 30, 30, 30, 20 };
        d0 = 5;
        T1 = 30;
        dh = 40;
        if (this.name.Contains("G1"))
        {
            idx = new int[6] { -1, -4, -5, -7, -10, -11 };
            kb = new float[12] { 30, 20, 10, 30, 30, 20,   30, 20, 10, 30, 30, 20};
            T1 = 40;
            d0 = 10;
            dh = 30;
        }
        if (this.name.Contains("T1"))
        {
            idx = new int[6] { -1, -4, -5, -7, -10, -11 };
            kb = new float[12] { 30, 20, 20, 30, 30, 20,   30, 20, 20, 30, 30, 20};
            T1 = 30;
            d0 = 10;
            dh = 40;
        }
        if (this.name.Contains("zqsa"))
        {
            idx = new int[6] { -3, -4, -5, -9, -10, -11 };
            kb = new float[12] { 20, 20, 30, 30, 30, 20,   20, 20, 30, 30, 30, 20 };
            T1 = 40;
            d0 = 10;
            dh = 30;
        }
        
        utotal[Mathf.Abs(idx[0]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[0]);
        utotal[Mathf.Abs(idx[1]) - 1] -= 2 * (dh * uf1 + d0) * Mathf.Sign(idx[1]);
        utotal[Mathf.Abs(idx[2]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[2]);
        utotal[Mathf.Abs(idx[3]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[3]);
        utotal[Mathf.Abs(idx[4]) - 1] -= 2 * (dh * uf2 + d0) * Mathf.Sign(idx[4]);
        utotal[Mathf.Abs(idx[5]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[5]);

        if (this.name.Contains("Loong"))
        {
            utotal[0] = Mathf.Clamp(utotal[0], -1f*khaa, 200f);
            utotal[6] = Mathf.Clamp(utotal[6], -1f*khaa, 200f);
        }
        if (this.name.Contains("G1"))
        {
            utotal[1] = Mathf.Clamp(utotal[1], -200f, 0f);
            utotal[7] = Mathf.Clamp(utotal[7], 0f, 200f);
        }
        if (this.name.Contains("T1"))
        {
            utotal[1] = Mathf.Clamp(utotal[1], -200f, 2f);
            utotal[7] = Mathf.Clamp(utotal[7], -2f, 200f);
        }
        if (this.name.Contains("zqsa"))
        {
            utotal[0] = Mathf.Clamp(utotal[0], -200f, 0f);
            utotal[6] = Mathf.Clamp(utotal[6], 0f, 200f);
        }
        for (int i = 0; i < ActionNum; i++) SetJointTargetDeg(acts[i], utotal[i]);
    }
    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 2000f;
        drive.damping = 100f;
        drive.forceLimit = 300f;
        drive.target = x;
        joint.xDrive = drive;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }

    void FixedUpdate()
    {

        if (this.name.Contains("Loong"))
        {
            SetJointTargetDeg(arts[16], 30*Mathf.Sin(3.14f * 1 * tp / T1));
            SetJointTargetDeg(arts[17], 80);
            SetJointTargetDeg(arts[18], 170);
            SetJointTargetDeg(arts[19], 0);
            
            SetJointTargetDeg(arts[23], 30*Mathf.Sin(3.14f * 1 * tp / T1));
            SetJointTargetDeg(arts[24], -80);
            SetJointTargetDeg(arts[25], -170);
            SetJointTargetDeg(arts[26], 0);
        }
        
        if (this.name.Contains("T1"))
        {
            SetJointTargetDeg(acts[12], 30*Mathf.Sin(3.14f * 1 * tp / T1));
            SetJointTargetDeg(acts[13], 80);
            SetJointTargetDeg(acts[14], 0);
            SetJointTargetDeg(acts[15], 4);

            SetJointTargetDeg(acts[16], -30*Mathf.Sin(3.14f * 1 * tp / T1));
            SetJointTargetDeg(acts[17], -80);
            SetJointTargetDeg(acts[18], 0);
            SetJointTargetDeg(acts[19], 0);
        }
        
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
        if (tt > 900 && ko < 0.5f)
        {
            //ko = 1f;
            //kw = 4f;
            //khaa = 1f;
            print(222222222222222);
        }
        var vel = body.InverseTransformDirection(arts[0].velocity);
        var wel = body.InverseTransformDirection(arts[0].angularVelocity);
        var live_reward = 1f;
        var ori_reward1 = -0.1f * Mathf.Abs(EulerTrans(body.eulerAngles[0]));
        var ori_reward2 = -0.1f * Mathf.Min(Mathf.Abs(body.eulerAngles[2]), Mathf.Abs(body.eulerAngles[2] - 360f));
        var wel_reward = - Mathf.Abs(wel[1] - wr);
        var vel_reward = Mathf.Clamp(vel[2],-5f,1.5f) - Mathf.Abs(vel[0]);
        var reward = live_reward + (ori_reward1 + ori_reward2) * ko +  wel_reward * kw + vel_reward;
        AddReward(reward);
        float fallang=30f;
        if(train)fallang=20f;
        if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > fallang || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > fallang)
        {
            //if(train)
            EndEpisode();
        }
    }

}
