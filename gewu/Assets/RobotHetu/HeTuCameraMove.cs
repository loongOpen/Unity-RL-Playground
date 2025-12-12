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

public class HeTuCameraMove : MonoBehaviour
{
    public float speed = 6/10f; // 行走速度
    public float jumpSpeed = 3f*3f; // 跳跃度
    public float gravity = 20f/1f; // 重力
    CharacterController con; // 角色控制器
    Vector3 myDirection; // 玩家移动的位置
    // Start is called before the first frame update
    void Start()
    {
        con = this.GetComponent<CharacterController>();
        myDirection = Vector3.zero; // x、y、z都为0

        /*SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty layers = tagManager.FindProperty("layers");

        SerializedProperty layer = layers.GetArrayElementAtIndex(16);
        int targetLayer = LayerMask.NameToLayer("robot2");
        layer.stringValue = "robot2";
        tagManager.ApplyModifiedProperties();
        Physics.IgnoreLayerCollision(16, 16, true);
        ChangeLayerRecursively(gameObject, 16);*/
    }

    void ChangeLayerRecursively(GameObject obj, int targetLayer)
    {
        obj.layer = targetLayer;
        foreach (Transform child in obj.transform)ChangeLayerRecursively(child.gameObject, targetLayer);
    }

    // Update is called once per frame
    void Update()
    {
        // 找到方向
        if (con.isGrounded) // 判断当前角色是否落地，是则为真
        {
            // 只响应方向键（上下左右），不响应 WASD
            float horizontal = 0f;
            float vertical = 0f;
            
            if (Input.GetKey(KeyCode.LeftArrow)) horizontal = -1f;
            else if (Input.GetKey(KeyCode.RightArrow)) horizontal = 1f;
            
            if (Input.GetKey(KeyCode.DownArrow)) vertical = -1f;
            else if (Input.GetKey(KeyCode.UpArrow)) vertical = 1f;
            
            myDirection = new Vector3(horizontal, 0, vertical);
            // transform.TransformDirection()从自身坐标转化为世界坐标
            myDirection = transform.TransformDirection(myDirection);
            if (Input.GetButton("Jump")) // 按下 space 键，向上跳跃
            {
                myDirection.y = jumpSpeed;
            }

        }
        myDirection.y -= gravity * Time.deltaTime; // 让角色落地
        con.Move(myDirection * speed * Time.deltaTime); // 角色移动
        if (Input.GetKeyDown(KeyCode.RightAlt))this.transform.Rotate(Vector3.up, -90); 
        if (Input.GetKeyDown(KeyCode.RightControl))this.transform.Rotate(Vector3.up, 90); 
    }
}
