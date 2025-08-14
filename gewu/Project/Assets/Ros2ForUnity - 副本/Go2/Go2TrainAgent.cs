using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using Unity.Sentis;


public class Go2TrainAgent : Agent
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
    ArticulationBody[] acts = new ArticulationBody[12];
    GameObject robot;

    float[] kb = new float[12] { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };
    float[] qsit = new float[12]{0f,1.4f,-2.1f, 0f,1.4f,-2.1f, 0f,1.4f,-2.1f, 0f,1.4f,-2.1f};
    //float[] qsit = new float[12]{0f,1.4f,-2.1f, 0f,1.4f,-2.1f, 0f,1.4f,-2.1f, 0f,1.4f,-2.1f};
    
    float dh = 25;
    //float d0 = 15;
    float ko = 2;
    public float v1 = 0;
    public float v2 = 0;
    public float wr = 0;
    float currentv1 = 0;
    float currentv2 = 0;
    float currentwr = 0;

    public override void Initialize()
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
        SerializedProperty layer = layers.GetArrayElementAtIndex(15);
        int targetLayer = LayerMask.NameToLayer("robot");
        layer.stringValue = "robot";
        tagManager.ApplyModifiedProperties();
        Physics.IgnoreLayerCollision(15, 15, true);
        ChangeLayerRecursively(gameObject, 15);

        if (train && !_isClone) 
        {
            for (int i = 1; i < 24; i++)
            {
                GameObject clone = Instantiate(gameObject); 
                clone.name = $"{name}_Clone_{i}"; 
                clone.GetComponent<Go2TrainAgent>()._isClone = true; 
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
        if(train)
        {
            v1 = 0;
            v2 = 0;
            wr = 0;
            int a = Random.Range(0, 3);
            //if(a==0)v1 = Random.Range(-3f,3f);
            //if(a==1)v2 = Random.Range(-1f,1f);
            //if(a==2)wr = Random.Range(-1f,1f);

            if(a==0)v1 = Random.Range(-3,4);
            if(a==1)v2 = Random.Range(-1,2);
            if(a==2)wr = Random.Range(-1,2);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(body.InverseTransformDirection(Vector3.down));
        //sensor.AddObservation(EulerTrans(0*body.eulerAngles[0])*3.14f/180f);//pitch rad 
        //sensor.AddObservation(EulerTrans(0*body.eulerAngles[2])*3.14f/180f);//roll rad 
        
        sensor.AddObservation(body.InverseTransformDirection(arts[0].angularVelocity));
        //sensor.AddObservation(body.InverseTransformDirection(arts[0].velocity));
        for (int i = 0; i < ActionNum; i++)
        {
            sensor.AddObservation(acts[i].jointPosition[0]);
            sensor.AddObservation(acts[i].jointVelocity[0]);
        }
        //sensor.AddObservation(v1);
        //sensor.AddObservation(v2);
        //sensor.AddObservation(wr);
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
        
        kb = new float[12] { 30, 30, 50,   30, 30, 50,   30, 30, 50,   30, 30, 50 };
        for (int i = 0; i < ActionNum; i++)
        {
            u[i] = u[i] * kk + (1 - kk) * continuousActions[i];
            kb[i]=0.4f*180f/3.14f;
            utotal[i] = kb[i] * u[i];
            if (fixbody) utotal[i] = 0;
        }

        //d0 = 30;
        dh = 0.4f*180f/3.14f;
        T1 = 25;
        
        utotal[1] += dh * uf1  + qsit[1]*90f/3.14f;
        utotal[2] += (dh * uf1) * -2  + qsit[2]*90f/3.14f;
        utotal[4] += dh * uf2  + qsit[4]*90f/3.14f;
        utotal[5] += (dh * uf2) * -2  + qsit[5]*90f/3.14f;
        utotal[7] += dh * uf2  + qsit[7]*90f/3.14f;
        utotal[8] += (dh * uf2) * -2  + qsit[8]*90f/3.14f;
        utotal[10] += dh * uf1  + qsit[10]*90f/3.14f;
        utotal[11] += (dh * uf1) * -2  + qsit[11]*90f/3.14f;


        for (int i = 0; i < ActionNum; i++) SetJointTargetDeg(acts[i], utotal[i]);

        
    }
    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 50f;//180f;
        drive.damping = 2f;//8f;
        //drive.forceLimit = 200f;
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
        Vector3 randomForce=new Vector3(Random.Range(-1f, 1f),0,Random.Range(-1f, 1f));
        if(Random.Range(0, 100)==1)arts[0].AddForce(20*randomForce, ForceMode.Impulse);

        
        if (Input.GetKey(KeyCode.W))
        {
            currentv1 = Mathf.MoveTowards(currentv1, 3f, 3f * 0.01f);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            currentv1 = Mathf.MoveTowards(currentv1, -3f, 3f * 0.01f);
        }
        else
        {
            currentv1 = Mathf.MoveTowards(currentv1, 0f, 3f * 0.01f);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            currentv2 = Mathf.MoveTowards(currentv2, -1f, 1f * 0.01f);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            currentv2 = Mathf.MoveTowards(currentv2, 1f, 1f * 0.01f);
        }
        else
        {
            currentv2 = Mathf.MoveTowards(currentv2, 0f, 1f * 0.01f);
        }

        if (Input.GetKey(KeyCode.A))
        {
            currentwr = Mathf.MoveTowards(currentwr, -1f, 1f * 0.01f);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            currentwr = Mathf.MoveTowards(currentwr, 1f, 1f * 0.01f);
        }
        else
        {
            currentwr = Mathf.MoveTowards(currentwr, 0f, 1f * 0.01f);
        }

        if(!train)
        {
            v1 = currentv1;
            v2 = currentv2;
            wr = currentwr;
            if (Input.GetKey(KeyCode.Space))EndEpisode();
        }

        tp++;
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
        ko = 2f;
        float kv = 1f;
        wr=0;
        var vel = body.InverseTransformDirection(arts[0].velocity);
        var wel = body.InverseTransformDirection(arts[0].angularVelocity);
        var live_reward = 1f;
        var ori_reward1 = -0.1f * Mathf.Abs(EulerTrans(body.eulerAngles[0]));//-0.5f * Mathf.Min(Mathf.Abs(body.eulerAngles[0]), Mathf.Abs(body.eulerAngles[0] - 360f));
        var ori_reward2 = -0.1f * Mathf.Min(EulerTrans(body.eulerAngles[2]));
        var wel_reward = - Mathf.Abs(wel[1] - wr);
        //var vel_reward = - 0.4f * Mathf.Abs(vel[2] - v1) - Mathf.Abs(vel[0] - v2);
        var vel_reward = vel[2] - Mathf.Abs(vel[0]);
        var reward = live_reward + (ori_reward1 + ori_reward2) * ko + (wel_reward + vel_reward) * kv;
        AddReward(reward);
        if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > 20f || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > 20f || tt>=1000)
        {
            if(train)EndEpisode();
        }
        if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > 60f || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > 60f || tt>=1000)
        {
            if(!train)EndEpisode();
        }
        //print(111);
        //print(EulerTrans(body.eulerAngles[0]));
        //print(EulerTrans(body.eulerAngles[2]));
    }

}
