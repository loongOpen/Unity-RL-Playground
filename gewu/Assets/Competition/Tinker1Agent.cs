using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using UnityEditor;
using Unity.Sentis;

public class Tinker1Agent : Agent
{
    int tp = 0;
    int tt = 0;
    int tp0 = 0;
    float kv = 0.4f;
    float uf1 = 0;
    float uf2 = 0;
    float[] u = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float[] utotal = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    Transform body;
    
    public Rigidbody ball;
    public bool fixbody = false;
    public bool train;
    public bool accelerate;
    public int ObservationNum;
    public int ActionNum;

    List<float> P0 = new List<float>();
    List<float> W0 = new List<float>();
    List<Transform> bodypart = new List<Transform>();
    Vector3 pos0;
    Vector3 posb0;
    Quaternion rot0;
    ArticulationBody[] arts = new ArticulationBody[40];
    ArticulationBody[] acts = new ArticulationBody[12];

    float ko = 1;
    public float vr = 0;
    public float wr = 0;
    public bool wasd = false;
    float currentWr = 0;
    float currentVr = 0;

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
        /*SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"));
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
        }*/

        if (train && !_isClone) 
        {
            for (int i = 1; i < 24; i++)
            {
                GameObject clone = Instantiate(gameObject); 
                //clone.transform.position = transform.position + new Vector3(i * 2f, 0, 0);
                clone.name = $"{name}_Clone_{i}"; 
                clone.GetComponent<Tinker1Agent>()._isClone = true; 
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
        vr = 0;
        wr = 0;
        if(Random.Range(0,2)==1)vr = Random.Range(0f,0.6f);
        else wr = Random.Range(-1f,1f);
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
        sensor.AddObservation(vr);
        sensor.AddObservation(wr);
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
        float[] kb = new float[12] { 10, 10, 30, 50, 30,     10, 10, 30, 50, 30, 0, 0 };
        for (int i = 0; i < ActionNum; i++)
        {
            u[i] = u[i] * kk + (1 - kk) * continuousActions[i];
            utotal[i] = kb[i] * u[i];
            if (fixbody) utotal[i] = 0;
        }

        int[] idx = new int[6] { -2, -3, 4, 7, 8, -9 };
        float d0 = 30;
        float dh = 20;
        utotal[Mathf.Abs(idx[0])] += (dh * uf1 + d0) * Mathf.Sign(idx[0]);
        utotal[Mathf.Abs(idx[1])] -= 2 * (dh * uf1 + d0) * Mathf.Sign(idx[1]);
        utotal[Mathf.Abs(idx[2])] += (dh * uf1 + d0) * Mathf.Sign(idx[2]);
        utotal[Mathf.Abs(idx[3])] += (dh * uf2 + d0) * Mathf.Sign(idx[3]);
        utotal[Mathf.Abs(idx[4])] -= 2 * (dh * uf2 + d0) * Mathf.Sign(idx[4]);
        utotal[Mathf.Abs(idx[5])] += (dh * uf2 + d0) * Mathf.Sign(idx[5]);

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
        if(!wasd)
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                currentWr = Mathf.MoveTowards(currentWr, -1f, 1f * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                currentWr = Mathf.MoveTowards(currentWr, 1f, 1f * Time.deltaTime);
            }
            else
            {
                currentWr = Mathf.MoveTowards(currentWr, 0f, 1f * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.UpArrow))
            {
                currentVr = Mathf.MoveTowards(currentVr, 0.6f, 0.6f * Time.deltaTime);
            }
            else
            {
                currentVr = Mathf.MoveTowards(currentVr, 0f, 0.6f * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.RightControl))EndEpisode();
        }
        if(wasd)
        {
            if (Input.GetKey(KeyCode.A)) 
            {
                currentWr = Mathf.MoveTowards(currentWr, -01f, 01f * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                currentWr = Mathf.MoveTowards(currentWr, 01f, 01f * Time.deltaTime);
            }
            else
            {
                currentWr = Mathf.MoveTowards(currentWr, 0f, 01f * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.W))
            {
                currentVr = Mathf.MoveTowards(currentVr, 0.6f, 0.6f * Time.deltaTime);
            }
            else
            {
                currentVr = Mathf.MoveTowards(currentVr, 0f, 0.6f * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.LeftControl))EndEpisode();
	    }
        if (Input.GetKey(KeyCode.Space) && ball != null)ball.position = posb0;

        if(!train)
        {
            vr = currentVr;
            wr = currentWr;
        }
        
        if (accelerate) Time.timeScale = 20;
        if (!accelerate) Time.timeScale = 1;
        tp++;
        int T1 = 25;
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
        ko = 0.1f;
        
        tt++;
        if (tt > 900 && kv < 0.5f)
        {
            kv = 0.7f;
            print(222222222222222);
        }
        var vel = body.InverseTransformDirection(arts[0].velocity);
        var wel = body.InverseTransformDirection(arts[0].angularVelocity);
        var live_reward = 1f;
        var ori_reward1 = -0.1f * Mathf.Abs(EulerTrans(body.eulerAngles[0]));
        var ori_reward2 = -0.1f * Mathf.Abs(EulerTrans(body.eulerAngles[2]));
        var wel_reward  = -5f * kv * Mathf.Abs(wel[1] - wr);
        var vel_reward =  1 - 10 * kv * Mathf.Abs(vel[2]-vr) - 01f*Mathf.Abs(vel[0]);
        var reward = live_reward + (ori_reward1 + ori_reward2 ) * 1* ko + wel_reward + vel_reward;
        AddReward(reward);
        if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > 20f || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > 20f)
        {
            if(train)EndEpisode();
        }
    }
}