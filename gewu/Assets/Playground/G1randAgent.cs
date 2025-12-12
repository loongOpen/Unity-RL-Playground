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

public class G1randAgent : Agent
{
    int tp = 0;
    int tt = 0;

    public bool fixbody = false;
    public bool train;
    public bool keyboard = false;
    float uf1 = 0;
    float uf2 = 0;
    float[] u = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float[] utotal = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    int T1 = 50;
    int tp0 = 0;
    
    Transform body;
    public int ActionNum;

    List<float> P0 = new List<float>();
    List<float> W0 = new List<float>();
    List<Transform> bodypart = new List<Transform>();
    Vector3 pos0;
    Quaternion rot0;
    ArticulationBody[] arts = new ArticulationBody[40];
    ArticulationBody[] acts = new ArticulationBody[20];
    ArticulationBody[] arms = new ArticulationBody[10];
    GameObject robot;

    float[] kb = new float[12] { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };
    public float vr = 0;
    public float wr = 0;
    public float vd = 0;

    [Header("Domain Randomization")]
    public bool randomizeFriction = true;
    public Vector2 frictionRange = new Vector2(0.1f, 1.25f);
    public bool randomizeBaseMass = true;
    public Vector2 baseMassDeltaRange = new Vector2(-1f, 3f);
    public bool pushRobots = true;
    public float pushIntervalSeconds = 5f;
    public float maxPushVelocity = 1.5f;

    Collider[] _colliders;
    float _baseMass;
    float _pushTimer;
    [Header("Observation Noise")]
    public bool addNoise = true;
    public float gravityNoise = 0.05f;
    public float angularVelNoise = 0.05f;
    public float jointPosNoise = 0.01f;
    public float jointVelNoise = 0.075f;

    public override void Initialize()
    {
        arts = this.GetComponentsInChildren<ArticulationBody>();
        ActionNum = 0;
        int armnum = 0;
        for (int k = 0; k < arts.Length; k++)
        {
            if(arts[k].jointType.ToString() == "RevoluteJoint")
            {
                string jointName = arts[k].gameObject.name.ToLower();
                if (jointName.Contains("hip") || jointName.Contains("knee") || jointName.Contains("ankle"))
                {
                    acts[ActionNum] = arts[k];
                    ActionNum++;
                }
            }
            if(arts[k].jointType.ToString() == "RevoluteJoint")
            {
                string jointName = arts[k].gameObject.name.ToLower();
                if (jointName.Contains("shoulder") || jointName.Contains("elbow") || jointName.Contains("wrist"))
                {
                    arms[armnum] = arts[k];
                    armnum++;
                }
            }
        }
        ActionNum = 12;
        body = arts[0].GetComponent<Transform>();
        pos0 = body.position;
        rot0 = body.rotation;
        _baseMass = arts[0].mass;
        _colliders = GetComponentsInChildren<Collider>();
        arts[0].GetJointPositions(P0);
        arts[0].GetJointVelocities(W0);
       
    }

    private bool _isClone = false; 
    void Start()
    {
        Time.fixedDeltaTime = 0.01f;    
        
        int numrob=1;
        if(train)numrob=32;//2048;//512;
        if (!_isClone) 
        {
            for (int i = 1; i < numrob; i++)
            {
                GameObject clone = Instantiate(gameObject); 
                clone.name = $"{name}_Clone_{i}"; 
                clone.GetComponent<G1randAgent>()._isClone = true; 
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
        
        Quaternion randRot = rot0;
        if(train)randRot = rot0 * Quaternion.Euler(0, Random.Range(-180f,180f), 0);
        float px=0;
        float pz=0;
        if(train)
        {
            px = Random.Range(-2f,2f);
            pz = Random.Range(-2f,2f);
        }
        
        Vector3 randPos = new Vector3(pos0[0]+px, pos0[1], pos0[2]+pz);
        if (fixbody) arts[0].immovable = true;
        if (!fixbody)
        {
            arts[0].TeleportRoot(randPos, randRot);
            arts[0].velocity = Vector3.zero;
            arts[0].angularVelocity = Vector3.zero;
            arts[0].SetJointPositions(P0);
            arts[0].SetJointVelocities(W0);
        }

        vr=0;
        vd=0;
        wr=0;
        if(Random.Range(0,4)==0)
        {
            int a = Random.Range(0,2);
            vr = Random.Range(1f,2f)*a + Random.Range(-1.4f,-0.8f)*(1-a);
        }
        else if(Random.Range(0,3)==0) vd = Random.Range(0.4f,0.6f)*(Random.Range(0,2)*2-1);
        else if(Random.Range(0,2)==0) wr = Random.Range(1f,2f)*(Random.Range(0,2)*2-1);
           
        if(keyboard)
        {
            vr=0;
            vd=0;
            wr=0;
        }

        if(randomizeFriction) RandomizeFriction();
        if(randomizeBaseMass) RandomizeBaseMass();
        _pushTimer = 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(ApplyVectorNoise(body.InverseTransformDirection(Vector3.down), gravityNoise));
        sensor.AddObservation(ApplyVectorNoise(body.InverseTransformDirection(arts[0].angularVelocity), angularVelNoise));
        sensor.AddObservation(body.InverseTransformDirection(arts[0].velocity));
        for (int i = 0; i < ActionNum; i++)
        {
            sensor.AddObservation(ApplyNoise(acts[i].jointPosition[0], jointPosNoise));
            sensor.AddObservation(ApplyNoise(acts[i].jointVelocity[0], jointVelNoise));
        }
        sensor.AddObservation(vr);
        sensor.AddObservation(vd);
        sensor.AddObservation(wr);
        sensor.AddObservation(Mathf.Sin(3.14f * 1 * tp / T1));
        sensor.AddObservation(Mathf.Cos(3.14f * 1 * tp / T1));
    }
    float EulerTrans(float angle)
    {
        angle = angle % 360f;
        if (angle > 180f)angle -= 360f;
        else if (angle < -180f)angle += 360f;
        return angle;
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        for (int i = 0; i < 12; i++) utotal[i] = 0;
        var continuousActions = actionBuffers.ContinuousActions;
        var kk = 0.9f;
        kb = new float[12] { 40, 30, 20, 25, 40, 30,   40, 30, 20, 25, 40, 30};
        
        for (int i = 0; i < ActionNum; i++)
        {
            u[i] = u[i] * kk + (1 - kk) * continuousActions[i];
            utotal[i] = 2 * kb[i] * u[i];
            if (fixbody) utotal[i] = 0;
        }
        

        int[] idx = new int[6] { -1, -4, -5, -7, -10, -11 };
        T1 = 40;
        float d0 = 10;
        float dh = 30;
        if(vr==0 && wr==0 && vd==0)
        {
            if(Mathf.Abs(EulerTrans(body.eulerAngles[0])) < 2f && Mathf.Abs(EulerTrans(body.eulerAngles[2])) < 2f)
                dh=0;///////////////////////////////////////////////////
            else dh=30;
        }
        if(train && tt>800)
        {
            vr=0;
            vd=0;
            wr=0;
        }
        
        utotal[Mathf.Abs(idx[0]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[0]);
        utotal[Mathf.Abs(idx[1]) - 1] -= 2 * (dh * uf1 + d0) * Mathf.Sign(idx[1]);
        utotal[Mathf.Abs(idx[2]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[2]);
        utotal[Mathf.Abs(idx[3]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[3]);
        utotal[Mathf.Abs(idx[4]) - 1] -= 2 * (dh * uf2 + d0) * Mathf.Sign(idx[4]);
        utotal[Mathf.Abs(idx[5]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[5]);

        utotal[1] = Mathf.Clamp(utotal[1], -200f, 0f);
        utotal[7] = Mathf.Clamp(utotal[7], 0f, 200f);
        for (int i = 0; i < ActionNum; i++) SetJointTargetDeg(acts[i], utotal[i]);

        float[] uarm = new float[10] { Mathf.Clamp(vr,0,3)*20*Mathf.Sin(3.14f * 1 * tp / T1), -10, 0, 80, 0,      -Mathf.Clamp(vr,0,3)*20*Mathf.Sin(3.14f * 1 * tp / T1), 10, 0, 80, 0 };
        for (int i = 0; i < 10; i++) SetJointTargetDeg(arms[i], uarm[i]);
    }
    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 100f;
        drive.damping = 5f;
        //drive.forceLimit = 300f;
        drive.target = x;
        joint.xDrive = drive;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }

    void FixedUpdate()
    {
         
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
        if(keyboard)
        {
            float v=0.02f;
            if(vd==0 && wr==0)
            {
                if(Input.GetKey(KeyCode.W))vr=Mathf.MoveTowards(vr, 2f, v);
                else if(Input.GetKey(KeyCode.S))vr=Mathf.MoveTowards(vr, -1.4f, v);
                else vr=Mathf.MoveTowards(vr, 0f, v);
            }
            if(vr==0 && wr==0)
            {
                if(Input.GetKey(KeyCode.Q))vd=Mathf.MoveTowards(vd, -0.6f, v);
                else if(Input.GetKey(KeyCode.E))vd=Mathf.MoveTowards(vd, 0.6f, v/2f);
                else vd=Mathf.MoveTowards(vd, 0f, v);
            }
            if(vr==0 && vd==0)
            {
                if(Input.GetKey(KeyCode.A))wr=Mathf.MoveTowards(wr, -2f, v);
                else if(Input.GetKey(KeyCode.D))wr=Mathf.MoveTowards(wr, 2f, v);
                else wr=Mathf.MoveTowards(wr, 0f, v);
            }
        }

        float uSumAbs = Mathf.Abs(u[1]) + Mathf.Abs(u[2]) + Mathf.Abs(u[5]) + Mathf.Abs(u[7]) + Mathf.Abs(u[8]) + Mathf.Abs(u[11]);
        var u_penalty = -1f * uSumAbs;
        var vel = body.InverseTransformDirection(arts[0].velocity);
        var wel = body.InverseTransformDirection(arts[0].angularVelocity);
        var live_reward = 1f;
        var ori_reward1 = -0.1f * Mathf.Abs(EulerTrans(body.eulerAngles[0]));
        var ori_reward2 = -0.1f * Mathf.Abs(EulerTrans(body.eulerAngles[2]));
        var wel_reward = 1 - 2*Mathf.Abs(wel[1] - wr);
        var vel_reward = 1 - 2*Mathf.Abs(vel[2] - vr) - 2*Mathf.Abs(vel[0]-vd)- 1*Mathf.Abs(vel[1]);
        var reward = live_reward + (ori_reward1 + ori_reward2) * 1 +  wel_reward * 1 + vel_reward + u_penalty;

        AddReward(reward);
        float fallang=30f;
        if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > fallang || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > fallang)
        {
            EndEpisode();
        }
        if(train && tt>1000)EndEpisode();

        if(pushRobots) HandleRandomPush();
    }

    void RandomizeFriction()
    {
        if (_colliders == null || _colliders.Length == 0) return;
        var minFriction = Mathf.Min(frictionRange.x, frictionRange.y);
        var maxFriction = Mathf.Max(frictionRange.x, frictionRange.y);
        var frictionValue = Random.Range(minFriction, maxFriction);
        foreach (var col in _colliders)
        {
            if (col == null) continue;
            var mat = new PhysicMaterial
            {
                dynamicFriction = frictionValue,
                staticFriction = frictionValue,
                frictionCombine = PhysicMaterialCombine.Multiply
            };
            col.material = mat;
        }
    }

    void RandomizeBaseMass()
    {
        var minDelta = Mathf.Min(baseMassDeltaRange.x, baseMassDeltaRange.y);
        var maxDelta = Mathf.Max(baseMassDeltaRange.x, baseMassDeltaRange.y);
        var delta = Random.Range(minDelta, maxDelta);
        arts[0].mass = Mathf.Max(0.1f, _baseMass + delta);
    }

    void HandleRandomPush()
    {
        if (pushIntervalSeconds <= 0f || maxPushVelocity <= 0f) return;
        _pushTimer += Time.fixedDeltaTime;
        if (_pushTimer < pushIntervalSeconds) return;
        _pushTimer = 0f;
        var dir2D = Random.insideUnitCircle;
        if (dir2D == Vector2.zero) return;
        dir2D.Normalize();
        var magnitude = Random.Range(1f, maxPushVelocity);
        var pushVelocity = new Vector3(dir2D.x, 0f, dir2D.y) * magnitude;
        arts[0].AddForce(pushVelocity, ForceMode.VelocityChange);
    }

    Vector3 ApplyVectorNoise(Vector3 value, float magnitude)
    {
        if (!addNoise || magnitude <= 0f) return value;
        return new Vector3(
            ApplyNoise(value.x, magnitude),
            ApplyNoise(value.y, magnitude),
            ApplyNoise(value.z, magnitude));
    }

    float ApplyNoise(float value, float magnitude)
    {
        if (!addNoise || magnitude <= 0f) return value;
        return value + Random.Range(-magnitude, magnitude);
    }

}
