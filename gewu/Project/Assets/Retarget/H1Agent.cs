using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgentsExamples;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

using System;
using System.Net;
using System.Text;
using System.Linq;

public class H1Agent : Agent
{
    float[] uff = new float[19] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0};
    float[] u = new float[19] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0};
    float[] u0 = new float[19] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0};
    float[] utotal = new float[19] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 ,0, 0, 0, 0, 0, 0, 0, 0, 0};
    
    private List<string> actionFolders = new List<string>();
    private int currentActionIndex = 0;

    private string dofFilePath;
    private string rotFilePath;
    private string posFilePath;

    private List<float[]> allDofData = new List<float[]>();
    private List<float[]> allPosData = new List<float[]>();
    private List<float[]> allRotData = new List<float[]>();
    
    private int currentFrame = 0;  
    private bool isEndEpisode = false;
    
    Transform body;
    List<float> P0 = new List<float>();
    List<float> W0 = new List<float>();
    ArticulationBody[] arts = new ArticulationBody[40];
    Vector3 pos0;
    Quaternion rot0;
    ArticulationBody[] jh = new ArticulationBody[19];
    ArticulationBody art0;

    public override void Initialize()
    {

        arts = this.GetComponentsInChildren<ArticulationBody>();
        art0 = arts[0];
        body = art0.transform;
        for (int k = 0; k < 19; k++)jh[k] = arts[k+1];

        pos0 = body.position;
        rot0 = body.rotation;
        art0.GetJointPositions(P0);
        art0.GetJointVelocities(W0);
        
        LoadActionFolders("./Assets/Retarget/feedforward");
        print(actionFolders[1]);
        LoadDataForAction(actionFolders[1]);

    }
    void LoadActionFolders(string basePath)
    {
        string[] folders = Directory.GetDirectories(basePath);
        actionFolders = folders.ToList();  
    }
    void LoadDataForAction(string actionFolder)
    {
        dofFilePath = Path.Combine(actionFolder, "dof.csv");
        rotFilePath = Path.Combine(actionFolder, "root_rot.csv");
        posFilePath = Path.Combine(actionFolder, "root_trans_offset.csv");

        allDofData = LoadDataFromFile(dofFilePath);
        allRotData = LoadDataFromFile(rotFilePath);
        allPosData = LoadDataFromFile(posFilePath);
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
                    if (float.TryParse(value.Trim(), out float parsedValue))
                    {
                        frameData.Add(parsedValue);
                    }
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
        art0.TeleportRoot(pos0, rot0);
        art0.velocity = Vector3.zero;
        art0.angularVelocity = Vector3.zero;
        art0.SetJointPositions(P0);
        art0.SetJointVelocities(W0);
        for (int i = 0; i < 19; i++) SetJointTargetDeg(jh[i], u0[i]);
        for (int i = 0; i < 19; i++) u[i] = 0;
        for (int i = 0; i < 19; i++) uff[i] = 0;
        
        LoadDataForAction(actionFolders[currentActionIndex]);
        currentFrame = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(EulerTrans(body.eulerAngles[0]) * 3.14f / 180f );//rad
        sensor.AddObservation(EulerTrans(body.eulerAngles[2]) * 3.14f / 180f );//rad
        sensor.AddObservation(body.InverseTransformDirection(art0.velocity));
        sensor.AddObservation(body.InverseTransformDirection(art0.angularVelocity));
        for (int i = 0; i < 19; i++)
        {
            sensor.AddObservation((jh[i].jointPosition[0]));
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
       
    }
    void FixedUpdate()
    {
        if (allDofData.Count > 0)
        {
            float[] currentDof = allDofData[currentFrame];
            float[] currentPos = allPosData[currentFrame];
            float[] currentRot = allRotData[currentFrame];
            for (int i = 0; i < 19; i++)
            {
                uff[i] = currentDof[i]* 180f / 3.14f;
                SetJointTargetDeg(jh[i], uff[i]);
            }
            
            Vector3 newPosition = new Vector3(-currentPos[1], currentPos[2], currentPos[0]);
            Quaternion newRotation = new Quaternion(
                currentRot[1], 
                -currentRot[2], 
                currentRot[0], 
                currentRot[3]
            );

            art0.TeleportRoot(newPosition, newRotation);
            art0.velocity = Vector3.zero;
            art0.angularVelocity = Vector3.zero;
                
            currentFrame = (currentFrame + 1) % allDofData.Count;
            
            if (currentFrame == 0) 
            {
                currentActionIndex = (currentActionIndex + 1) % actionFolders.Count;
                print("Switched to action: " + actionFolders[currentActionIndex]);
                EndEpisode();  
                currentFrame = 0;  
            }
                
            if (isEndEpisode)
            {
                    currentFrame = 0;
                    isEndEpisode = false; 
            }
        }
	
        
        var reward = 0;
        AddReward(reward);
        
    }


    void SetJointTargetDeg(ArticulationBody joint, float x)
    {
        var drive = joint.xDrive;
        drive.stiffness = 2000f;//180f;
        drive.damping = 200f;//8f;
        drive.forceLimit = 300f;//250f;// 33.5f;// 50f;
        drive.target = x;
        joint.xDrive = drive;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        
    }

    float ang_trans(ArticulationBody joint, float x)
    {
        x = (x + 1f) * 0.5f;
        var ang = Mathf.Lerp(joint.xDrive.lowerLimit, joint.xDrive.upperLimit, x);
        return ang;
    }
    float rad_normalize(ArticulationBody joint, float ang)
    {
        var ang1 = ang * 180f / 3.14f;
        var x = (ang1 - joint.xDrive.lowerLimit) / (joint.xDrive.upperLimit - joint.xDrive.lowerLimit);//0~1
        x = 2 * x - 1;//-1~1
        return x;
    }
    

}



