using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using System;

public class G1mimicAgent : Agent
{
    public bool train = false;
    public bool replay = false;

    float[] uff = new float[29] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0};
    float[] u = new float[29] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0};
    float[] utotal = new float[29] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0};
    //int[] trainid=new int[13]{3,9, 5,13,6,14,15,   3,9, 5,13,16,17};
    int[] trainid=new int[13]{5,5,13,13,   6,14,15,   5,5,13,13,   16,17};
    
    private List<string> actionFolders = new List<string>();
    public int motion_id;
    public string motion_name;

    private string dofFilePath;
    private string rotFilePath;
    private string posFilePath;

    private List<float[]> refData = new List<float[]>();
    private List<float[]> itpData = new List<float[]>();
    
    private int currentFrame;  
    
    //private bool isEndEpisode = false;
    float[] currentData = new float[36];
    float[] currentPos = new float[3];
    float[] currentRot = new float[4];
    float[] currentDof = new float[29];
    
    Transform body;

    List<float> P0 = new List<float>();
    List<float> W0 = new List<float>();
    List<Transform> bodypart = new List<Transform>();
    Vector3 pos0;
    Quaternion rot0;
    Quaternion newRotation;
    Vector3 newPosition;
    ArticulationBody[] jh = new ArticulationBody[29];
    ArticulationBody[] arts = new ArticulationBody[40];
    ArticulationBody art0;
    int tt = 0;
    public int frame0 = 100;

    private bool _isClone = false; 
    void Start()
    {
        Time.fixedDeltaTime = 0.02f;
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
            for (int i = 1; i < 34; i++)
            {
                GameObject clone = Instantiate(gameObject); 
                clone.transform.position = transform.position + new Vector3(i * 2f, 0, 0);
                clone.name = $"{name}_Clone_{i}"; 
                clone.GetComponent<G1mimicAgent>()._isClone = true; 
            }
        }
    }

    void ChangeLayerRecursively(GameObject obj, int targetLayer)
    {
        obj.layer = targetLayer;
        foreach (Transform child in obj.transform)ChangeLayerRecursively(child.gameObject, targetLayer);
    }

    public override void Initialize()
    {

        arts = this.GetComponentsInChildren<ArticulationBody>();
        int ActionNum = 0;
        for (int k = 0; k < arts.Length; k++)
        {
            if(arts[k].jointType.ToString() == "RevoluteJoint")
            {
                jh[ActionNum] = arts[k];
                ActionNum++;
            }
        }

        body = jh[14].GetComponent<Transform>();
        art0 = body.GetComponent<ArticulationBody>();

        pos0 = body.position;
        rot0 = body.rotation;
        art0.GetJointPositions(P0);
        art0.GetJointVelocities(W0);
                
        List<string> csvFileNames = GetCsvFileNames("./Assets/Retarget/G1/dataset");
        refData = LoadDataFromFile(csvFileNames[motion_id]);
        float[] refT = new float[refData.Count];
        for(int i=0;i<refT.Length;i++)refT[i]=i/30f;
        float[] newT = new float[(int)(refData.Count*50f/30f)-1];
        for(int i=0;i<newT.Length;i++)newT[i]=i/50f;
        itpData = Interpolate(refT, refData, newT);
        motion_name=csvFileNames[motion_id].Replace("./Assets/Retarget/G1/dataset\\", "").Replace(".csv", "");
    }

    List<string> GetCsvFileNames(string directoryPath)
    {
        List<string> csvFiles = new List<string>();
        
        try
        {
            if (Directory.Exists(directoryPath))
            {
                string[] allFiles = Directory.GetFiles(directoryPath);
                foreach (string file in allFiles)
                {
                    if (Path.GetExtension(file).ToLower() == ".csv")
                    {
                        string fileName = Path.GetFileName(file);
                        csvFiles.Add(Path.Combine(directoryPath, fileName));
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Directory does not exist: " + directoryPath);
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("Error accessing directory: " + e.Message);
        }
        return csvFiles;
    }

    List<float[]> LoadDataFromFile(string filePath)
    {
        List<float[]> dataList = new List<float[]>();
        try
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                string[] values = line.Split(',');
                List<float> frameData = new List<float>();
                foreach (string value in values)
                {
                    if (float.TryParse(value.Trim(), out float parsedValue))frameData.Add(parsedValue);
                }
                dataList.Add(frameData.ToArray());
            }
        }
        catch (System.Exception e)
        {
            print("Error loading data from file " + filePath + ": " + e.Message);
        }
        return dataList;
    }
    public override void OnEpisodeBegin()
    {
        arts[0].TeleportRoot(pos0, rot0);
        arts[0].velocity = Vector3.zero;
        arts[0].angularVelocity = Vector3.zero;
        arts[0].SetJointPositions(P0);
        arts[0].SetJointVelocities(W0);
        for (int i = 0; i < 29; i++) u[i] = 0;
        for (int i = 0; i < 29; i++) uff[i] = 0;    
        currentFrame = frame0;
        if(replay)
        {
            //motion_id++;
            List<string> csvFileNames = GetCsvFileNames("./Assets/Retarget/G1/dataset");
            refData = LoadDataFromFile(csvFileNames[motion_id]);
            float[] refT = new float[refData.Count];
            for(int i=0;i<refT.Length;i++)refT[i]=i/30f;
            float[] newT = new float[(int)(refData.Count*50f/30f)-1];
            for(int i=0;i<newT.Length;i++)newT[i]=i/50f;
            itpData = Interpolate(refT, refData, newT);
            motion_name=csvFileNames[motion_id].Replace("./Assets/Retarget/G1/dataset\\", "").Replace(".csv", "");
            //if(motion_id==csvFileNames.Count)motion_id=0;
        }
        tt=0;
        currentData = refData[currentFrame];
        Array.Copy(currentData, 0, currentPos, 0, 3);
        Array.Copy(currentData, 3, currentRot, 0, 4);
        Array.Copy(currentData, 7, currentDof, 0, 29);
        

        Vector3 newPosition = new Vector3(-currentPos[1], currentPos[2], currentPos[0]);
        Quaternion newRotation = new Quaternion(
            currentRot[1], 
            -currentRot[2], 
            currentRot[0], 
            currentRot[3]
        );
        arts[0].TeleportRoot(newPosition, newRotation);
        arts[0].velocity = Vector3.zero;
        arts[0].angularVelocity = Vector3.zero;
        arts[0].immovable = true;
        for (int i = 0; i < 29; i++)
        {
            uff[i] = currentDof[i]* 180f / 3.14f;
            SetJointTargetDeg(jh[i], uff[i]);
        }
    }
 
    List<float[]> Interpolate(float[] t, List<float[]> posList, float[] targetT)
    {
        if (t.Length != posList.Count)
        {
            UnityEngine.Debug.LogError("t and posList must have the same length");
            return null;
        }
        int dimension = posList[0].Length;
        foreach (float[] arr in posList)
        {
            if (arr.Length != dimension)
            {
                UnityEngine.Debug.LogError("All arrays in posList must have the same length");
                return null;
            }
        }
        List<float[]> result = new List<float[]>();
        for (int i = 0; i < targetT.Length; i++)
        {
            float tValue = targetT[i];
            if (tValue < t[0] || tValue > t[t.Length - 1])
            {
                UnityEngine.Debug.LogError("tValue is out of range");
                return null;
            }
            int index = 0;
            while (index < t.Length - 1 && t[index + 1] < tValue)
            {
                index++;
            }
            float ratio = (tValue - t[index]) / (t[index + 1] - t[index]);
            float[] interpolatedPos = new float[dimension];
            for (int j = 0; j < dimension; j++)
            {
                interpolatedPos[j] = Mathf.Lerp(posList[index][j], posList[index + 1][j], ratio);
            }
            result.Add(interpolatedPos);
        }
         return result;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(EulerTrans(body.eulerAngles[0]) * 3.14f / 180f );//rad
        sensor.AddObservation(EulerTrans(body.eulerAngles[2]) * 3.14f / 180f );//rad
        //sensor.AddObservation(body.InverseTransformDirection(art0.velocity));
        sensor.AddObservation(body.InverseTransformDirection(art0.angularVelocity));
        for (int i = 0; i < 29; i++)
        {
            sensor.AddObservation(jh[i].jointPosition[0]);
            sensor.AddObservation(jh[i].jointVelocity[0]);
        }
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
        var continuousActions = actionBuffers.ContinuousActions;
        var kk = 0.9f;
        float kb = 20;
        if(replay)kb = 0;
        for (int i = 0; i < 29; i++)
        {
            u[i] = u[i] * kk + (1 - kk) * continuousActions[i];
            if(i>=15)kb=0;
            utotal[i] = kb * u[i] + uff[i];
            SetJointTargetDeg(jh[i], utotal[i]);
        }
    }

    void FixedUpdate()
    {
	if (itpData.Count > 0)
	{
        currentData = itpData[currentFrame];
        Array.Copy(currentData, 0, currentPos, 0, 3);
        Array.Copy(currentData, 3, currentRot, 0, 4);
        Array.Copy(currentData, 7, currentDof, 0, 29);
		for (int i = 0; i < 29; i++)uff[i] = currentDof[i]* 180f / 3.14f;

        Quaternion gymQuat = new Quaternion(
            currentRot[0], 
            currentRot[1], 
            currentRot[2], 
            currentRot[3]
        );
        Quaternion conversionQ = new Quaternion(0.5f, -0.5f, -0.5f, 0.5f);
        newRotation = conversionQ * gymQuat * Quaternion.Inverse(conversionQ);
		newPosition = new Vector3(-currentPos[1], currentPos[2]+0.04f, currentPos[0]);
        newRotation = new Quaternion(
            currentRot[1], 
            -currentRot[2], 
            currentRot[0], 
            currentRot[3]
        );
        if(replay)Physics.gravity = Vector3.zero;
    	if(replay)arts[0].TeleportRoot(newPosition, newRotation);//***************************************************************************************************************
	}
	
	/////////////////rewards/////////////////////////////////////////////////////////
        tt++;
        var vel = body.InverseTransformDirection(art0.velocity);
        var wel = body.InverseTransformDirection(art0.angularVelocity);
        
        var live_reward = 1f;
        float rot_reward = 0;
        float pos_reward = 0;
        if(tt>3)
        {
            arts[0].immovable = false;
            rot_reward = - 0.01f * Quaternion.Angle(body.rotation, newRotation);
            pos_reward = - 1f * (body.position - newPosition).magnitude;
            if (Quaternion.Angle(body.rotation, newRotation)>30f || (body.position - newPosition).magnitude>0.3f)if(!replay)EndEpisode();
        }
        
        var reward = live_reward + (rot_reward + pos_reward)*1f;
        AddReward(reward);
        currentFrame = (currentFrame + 1) ;
    }

    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 180f;//2000f;
        drive.damping = 8f;//200f;
        //drive.forceLimit = 300f;//250f;// 33.5f;// 50f;
        drive.target = x;
        joint.xDrive = drive;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }    
}