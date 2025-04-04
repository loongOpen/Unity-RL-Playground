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


public class LoongAgent : Agent
{
    int tp = 0;
    int tq = 0;
    int tr = 0;
    int tt = 0;
    int tk = 0;
    int tw = 0;
    //public Transform foot;
    public bool fixbody = false;
    public bool train;
    public bool accelerate;
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
    public enum StyleB
    {
        walk = 0,
        run = 1,
        jump = 2
    }
    public enum StyleQ
    {
        trot = 0,
        bound = 1,
        pronk = 2
    }
    public enum StyleL
    {
        drive = 0,
        walk = 1,
        jump = 2
    }
    public enum StyleR
    {
        biped = 0,
        quadruped = 1,
        legwheeled = 2
    }
    
    Transform body;
    public int ObservationNum;
    public int ActionNum;

    List<float> P0 = new List<float>();
    List<float> W0 = new List<float>();
    List<Transform> bodypart = new List<Transform>();
    Vector3 pos0;
    Vector3 posball0;
    Quaternion rot0;
    ArticulationBody[] arts = new ArticulationBody[40];
    ArticulationBody[] acts = new ArticulationBody[20];
    GameObject robot;

    float[] kb = new float[12] { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };
    float[] kb1 = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float[] kb2 = new float[12] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    float dh = 25;
    float dh0 = 25;
    float d0 = 15;
    float ko = 2;
    float kh = 0;
    public float vr = 0;
    public float wr = 0;
    public bool wasd = false;
    float currentWr = 0;
    float currentVr = 0;
    bool l_kick=false;
    bool r_kick=false;
    bool wait = false;
    float kbb=1f;
    public Transform ball;
    public Transform rival;

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
        posball0 = ball.position;
        /////////////////////////////////////////////////////
        robot = this.GetComponent<GameObject>();
        int LayerRobot = LayerMask.NameToLayer("robot");
        //robot.layer = LayerRobot;
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
        

        if (train && !_isClone) 
        {
            for (int i = 1; i < 14; i++)
            {
                GameObject clone = Instantiate(gameObject); 
                clone.transform.position = transform.position + new Vector3(i * 2f, 0, 0);
                clone.name = $"{name}_Clone_{i}"; 
                clone.GetComponent<LoongAgent>()._isClone = true; 
            }
        }
    }
    void ChangeLayerRecursively(GameObject obj, int targetLayer)
    {
        // 改变当前GameObject的Layer
        obj.layer = targetLayer;

        // 遍历所有子节点并改变它们的Layer
        foreach (Transform child in obj.transform)
        {
            ChangeLayerRecursively(child.gameObject, targetLayer);
        }
    }

    public override void OnEpisodeBegin()
    {
        tp = 0;
        tq = 0;
        tr = 0;
        tt = 0;
        tk = 0;
        tw = 0;
        for (int i = 0; i< 12; i++) u[i] = 0;
        for (int i = 0; i < 12; i++) ut[i] = 0;
        for (int i = 0; i < 12; i++) utt[i] = 0;
        //write_init();
        //art0.immovable = true;
        
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
            vr = 1.8f;//*Random.Range(0,2);//Random.Range(-0.5f,0.5f);
            wr = Random.Range(-1,2);
            //kbb = Random.Range(0.5f,1.5f);
        }
        T1 = 30;//Random.Range(30, 51);
        dh = 30;//Random.Range(20, 51);
        dh0 = dh;
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
        
        for (int i = 0; i < ActionNum; i++)
        {
            u[i] = u[i] * kk + (1 - kk) * continuousActions[i];
            ut[i] += u[i];
            utt[i] += ut[i];
            utotal[i] = kb[i] * u[i] + kb1[i] * ut[i] + kb2[i] * utt[i];
            if (fixbody) utotal[i] = 0;
        }

        string name = this.name;//this.name;
        int[] idx = new int[6] { 2, 3, 4, 7, 8, 9 };
        float[] ktemp1 = new float[12] { 5, 5, 30, 60, 30, 5, 5, 30, 60, 30, 0, 0 };
        d0 = 0;
        

        idx = new int[6] { 3, 4, 5, 9, 10, 11 };
        ktemp1 = new float[12]{ 20, 20, 30, 10, 30, 20,   20, 20, 30, 10, 30, 20 };
        //dh = 30;
        d0 = 5;
        if(Mathf.Abs(vr)<0.4f && Mathf.Abs(wr)<0.2f && Mathf.Abs(EulerTrans(body.eulerAngles[0]))<5f && Mathf.Abs(EulerTrans(body.eulerAngles[2]))<5f)dh=0;
        else dh = dh0;
        

                //float[] ktemp = new float[12] { 10, 10, 45, 20, 40, 10,   10, 10, 40, 20, 45, 10 };
                for (int i = 0; i < 12; i++) kb[i] = ktemp1[i]*01f*kbb;
                //T1 = 50;/////////////////////////////////////////////////////////////////
                //dh = 10;//10
                //d0 = 0;
                utotal[Mathf.Abs(idx[0]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[0]);
                utotal[Mathf.Abs(idx[1]) - 1] -= 2 * (dh * uf1 + d0) * Mathf.Sign(idx[1]);
                utotal[Mathf.Abs(idx[2]) - 1] += (dh * uf1 + d0) * Mathf.Sign(idx[2]);
                utotal[Mathf.Abs(idx[3]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[3]);
                utotal[Mathf.Abs(idx[4]) - 1] -= 2 * (dh * uf2 + d0) * Mathf.Sign(idx[4]);
                utotal[Mathf.Abs(idx[5]) - 1] += (dh * uf2 + d0) * Mathf.Sign(idx[5]);

        

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
        var continuousActionsOut = actionsOut.ContinuousActions;
        //continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[0] = 0;//all between-1 and 1
    }

    void FixedUpdate()
    {
        if(!wasd)
        {
            // 处理左右旋转控制
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
            // 处理前进控制
            if (Input.GetKey(KeyCode.UpArrow))
            {
                currentVr = Mathf.MoveTowards(currentVr, 1.9f, 1.9f * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                currentVr = Mathf.MoveTowards(currentVr, -1.9f, 1.9f * Time.deltaTime);
            }
            else
            {
                currentVr = Mathf.MoveTowards(currentVr, 0f, 1.9f * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.RightControl))EndEpisode();
            if (Input.GetKey(KeyCode.Slash))r_kick=true;
            if (Input.GetKey(KeyCode.RightShift))l_kick=true;
        }
        if(wasd)
        {
            // 处理左右旋转控制（A/D 键）
            if (Input.GetKey(KeyCode.A)) // A 键替代左箭头
            {
                currentWr = Mathf.MoveTowards(currentWr, -1f, 1f * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.D)) // D 键替代右箭头
            {
                currentWr = Mathf.MoveTowards(currentWr, 1f, 1f * Time.deltaTime);
            }
            else
            {
                currentWr = Mathf.MoveTowards(currentWr, 0f, 1f * Time.deltaTime);
            }

            // 处理前进控制（W 键）
            if (Input.GetKey(KeyCode.W)) // W 键替代上箭头
            {
                currentVr = Mathf.MoveTowards(currentVr, 1.9f, 1.9f * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                currentVr = Mathf.MoveTowards(currentVr, -1.9f, 1.9f * Time.deltaTime);
            }
            else
            {
                currentVr = Mathf.MoveTowards(currentVr, 0f, 1.9f * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.LeftControl))EndEpisode();
            if (Input.GetKey(KeyCode.Q))r_kick=true;
            if (Input.GetKey(KeyCode.E))l_kick=true;
	    }
        if (Input.GetKey(KeyCode.Space))ball.position=posball0;

        if(!train)
        {
            vr = 1.8f;//currentVr;
            wr = currentWr;
        }


        tk++;

        



        if(wasd)ball = rival;
        // 计算水平面方向向量
        Vector3 toBall = ball.position - body.position;
        toBall.y = 0; // 忽略垂直方向
        Vector3 toRival = rival.position - body.position;
        toRival.y = 0; // 忽略垂直方向
        
        

        // 计算当前前向方向（水平投影）
        Vector3 robotForward = body.forward;
        robotForward.y = 0;

        // 计算带符号角度差（-180°到180°）
        float angleDiff = Vector3.SignedAngle(robotForward.normalized, 
                                            toBall.normalized, 
                                            Vector3.up);

        // 计算角速度指令（比例控制）
        if(!train)wr = Mathf.Clamp(angleDiff * 0.3f, -1f, 1f);

        // 当距离过近时停止旋转
        if (toBall.magnitude < 0.2f) wr=0f;

        if(!train)
        {
            //vr = 0;
            if(Mathf.Abs(angleDiff)<2)vr=1.8f;
        }

        angleDiff = Vector3.SignedAngle(robotForward.normalized, 
                                            toRival.normalized, 
                                            Vector3.up);
        
        if(!train)
        {
            //if(toRival.magnitude < 0.9f)vr=0;
            if(Mathf.Abs(angleDiff)<20 && toRival.magnitude < 0.9f && r_kick==false && l_kick==false)
            {
                //vr = 0;
                wr = 0;
                if(Random.Range(0,3)==1)r_kick=true;
                else l_kick=true;
            }
            if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > 70f || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > 70f)EndEpisode();

        }
        
        
        




        if(l_kick && !wait)tq+=2;
        if(r_kick && !wait)tr++;
        T2=40;

        SetJointTargetDeg(arts[23], 50);
        SetJointTargetDeg(arts[24], -55);
        SetJointTargetDeg(arts[25], -170);
        SetJointTargetDeg(arts[26], 110);

        SetJointTargetDeg(arts[16], 60);
        SetJointTargetDeg(arts[17], 85);
        SetJointTargetDeg(arts[18], 80);
        SetJointTargetDeg(arts[19], 90);
        if (tq > 0 && tq <= T2)
        {
            SetJointTargetDeg(arts[23], 50 + 40 * (-Mathf.Cos(3.14f * 2* tq / T2)+1)/2f);
            SetJointTargetDeg(arts[24], -55 - 35 * (-Mathf.Cos(3.14f * 2* tq / T2)+1)/2f);
            SetJointTargetDeg(arts[25], -170 + 40 * (-Mathf.Cos(3.14f * 2* tq / T2)+1)/2f);
            SetJointTargetDeg(arts[26], 110- 110 * (-Mathf.Cos(3.14f * 2*tq / T2)+1)/2f);

            SetJointTargetDeg(arts[16], 60);
            SetJointTargetDeg(arts[17], 85);
            SetJointTargetDeg(arts[18], 80);
            SetJointTargetDeg(arts[19], 90);
        }
        if (tq >= T2) 
        {
            tq = 0;
            l_kick=false;
        }
        if (tr > 0 && tr <= T2)
        {
            SetJointTargetDeg(arts[23], -60 );
            SetJointTargetDeg(arts[24], -85);
            SetJointTargetDeg(arts[25], -80);
            SetJointTargetDeg(arts[26], 90);

            SetJointTargetDeg(arts[16], 60 - 120 * (-Mathf.Cos(3.14f * 2* tr / T2)+1)/2f);
            SetJointTargetDeg(arts[17], 85);
            SetJointTargetDeg(arts[18], 80);
            SetJointTargetDeg(arts[19], 90 - 40 * (-Mathf.Cos(3.14f * 2*tr / T2)+1)/2f);
        }
        if (tr >= T2) 
        {
            tr = 0;
            r_kick=false;
            wait = true;
        }
        if(wait)tw++;
        if(tw>50)
        {
            wait=false;
            tw=0;
        }

        
        if (accelerate) Time.timeScale = 20;
        if (!accelerate) Time.timeScale = 1;
        Vector3 Fd = Vector3.zero;
        Fd.x = Random.Range(-1f, 1f);
        Fd.z = Random.Range(-1f, 1f);
        //if (Random.Range(0, 100) == 1) art0.AddForce(5*Fd, ForceMode.Impulse);
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
        uff = (-Mathf.Cos(3.14f * 2 * tq / T2) + 1f) / 2f;

        float kk=1;
        if(Mathf.Abs(vr)<0.4f && Mathf.Abs(wr)<0.2f)kk=0;
        var vel = body.InverseTransformDirection(arts[0].velocity);
        var wel = body.InverseTransformDirection(arts[0].angularVelocity);
        var live_reward = 1f;
        var ori_reward1 = -0.1f * Mathf.Abs(EulerTrans(body.eulerAngles[0]));//-0.5f * Mathf.Min(Mathf.Abs(body.eulerAngles[0]), Mathf.Abs(body.eulerAngles[0] - 360f));
        var ori_reward2 = -01f * Mathf.Abs(wel[1]-01f*wr*kk);
        var ori_reward3 = -0.1f * Mathf.Min(Mathf.Abs(body.eulerAngles[2]), Mathf.Abs(body.eulerAngles[2] - 360f));
        var vel_reward1 = vel[2] - Mathf.Abs(vel[0]);
        var vel_reward2 = 0*vel[2]- 0.6f * Mathf.Abs(vel[2]-01f*vr*kk) - Mathf.Abs(vel[0]) + kh * Mathf.Abs(vel[1]);
        if (vr>0.4f)vel_reward2 = 1*vel[2] - Mathf.Abs(vel[0]) + kh * Mathf.Abs(vel[1]);
        if (vr<-0.4f)vel_reward2 = - vel[2] - Mathf.Abs(vel[0]) + kh * Mathf.Abs(vel[1]);
        var reward = live_reward + (ori_reward1 + ori_reward2 + ori_reward3) * ko*0.6f + vel_reward2;// + 5*foot.position.y;
        //if (BipedTargetMotion == StyleB.walk) reward += vel_reward1;
        //if (BipedTargetMotion == StyleB.run || BipedTargetMotion == StyleB.jump) reward += vel_reward2;
        AddReward(reward);
        if (Mathf.Abs(EulerTrans(body.eulerAngles[0])) > 20f || Mathf.Abs(EulerTrans(body.eulerAngles[2])) > 20f || tt>=1000)
        {
            if(train)EndEpisode();
            //print(stage);
        }
    }

}
