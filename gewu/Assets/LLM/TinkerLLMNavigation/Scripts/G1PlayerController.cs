using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;
public class G1PlayerController : MonoBehaviour
{
    public NavMeshAgent agent;
    public LineRenderer lineRenderer;
    public Vector3 targetPos;
    float moveSpeed = 0.3f;

    public Transform LivingRoomPos;
    public Transform BedRoomPos;
    public Transform DiningRoomPos;
    public Transform KitchenPos;
    public Transform WashRoomPos;
    public Transform StorageRoomPos;
    public Transform BedWashRoomPos;

    public SpeechToTextAndLLM speechToTextAndLLM;
    void Start()
    {
        targetPos = this.transform.position;
        agent.speed = moveSpeed;
    }

    void Update()
    {
        if (speechToTextAndLLM.recognizedText.Contains("客厅"))
        {
            targetPos = LivingRoomPos.position;
        }
        else if (speechToTextAndLLM.recognizedText.Contains("卧室"))
        {
            targetPos = BedRoomPos.position;

        }
        else if (speechToTextAndLLM.recognizedText.Contains("餐厅"))
        {
            targetPos = DiningRoomPos.position;

        }
        else if (speechToTextAndLLM.recognizedText.Contains("厨房"))
        {
            targetPos = KitchenPos.position;

        }
        else if (speechToTextAndLLM.recognizedText.Contains("储物间"))
        {
            targetPos = StorageRoomPos.position;
        }
        else if (speechToTextAndLLM.recognizedText.Contains("卫生间"))
        {
            if (speechToTextAndLLM.recognizedText.Contains("卧室"))
            {
                targetPos = BedWashRoomPos.position;
            }
            else
            {
                targetPos = WashRoomPos.position;
            }
        }

        //语音识别后的目标点
        #region
        //if (ASR.instance.textResult.text.Contains("客厅"))
        //{
        //    targetPos = LivingRoomPos.position;
        //}
        //else if (ASR.instance.textResult.text.Contains("卧室"))
        //{
        //    targetPos = BedRoomPos.position;
        //}
        //else if (ASR.instance.textResult.text.Contains("餐厅"))
        //{
        //    targetPos = DiningRoomPos.position;
        //}
        //else if (ASR.instance.textResult.text.Contains("厨房"))
        //{
        //    targetPos = KitchenPos.position;
        //}
        //else if (ASR.instance.textResult.text.Contains("储物间"))
        //{
        //    targetPos = StorageRoomPos.position;
        //}
        //else if (ASR.instance.textResult.text.Contains("卫生间"))
        //{
        //    if (ASR.instance.textResult.text.Contains("卧室"))
        //    {
        //        targetPos = BedWashRoomPos.position;
        //    }
        //    else
        //    {
        //        targetPos = WashRoomPos.position;
        //    }
        //}
        #endregion

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;
        //    if (EventSystem.current.IsPointerOverGameObject())
        //    {
        //        return;
        //    }
        //    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        //    {
        //        targetPos = hit.point;
        //    }
        //}
        agent.SetDestination(targetPos);
        lineRenderer.positionCount = agent.path.corners.Length;
        var corners = agent.path.corners;
        lineRenderer.SetPositions(corners);
    }
}
