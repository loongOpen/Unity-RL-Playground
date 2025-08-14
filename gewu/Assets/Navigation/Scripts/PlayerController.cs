using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
    public NavMeshAgent agent;
    //public Transform target;
    public LineRenderer lineRenderer;
	public Animator animator;
	//Ŀ���
	public Vector3 targetPos;
	public void Start()
	{
		targetPos = this.transform.position;
	}
	void Update()
    {
	
		// ����������Ƿ���
		if (Input.GetMouseButtonDown(0))
		{
			// �����������һ�����ߣ�����������λ��
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (EventSystem.current.IsPointerOverGameObject())
			{
				// Debug.Log("�����UI");
				return;
			}
			// ������߻�����ĳ������
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			{   //if (EventSystem.current.IsPointerOverGameObject()) return;
				
				// ��ӡ���е�λ��
				// print("TargetPos :" + hit.point);
				targetPos= hit.point;
			}
		}
		
		agent.SetDestination(targetPos);
		lineRenderer.positionCount = agent.path.corners.Length;
		var corners = agent.path.corners;
		lineRenderer.SetPositions(corners);
		if (lineRenderer.positionCount>0) // ���·�����ڼ�����
		{
			//animator.SetBool("Run", true);
			// Debug.Log("����ΪTrue��");
			
		}

		if (agent.remainingDistance <= agent.stoppingDistance)	
		{
			//animator.SetBool("Run", false);
			// Debug.Log("�ѵ���Ŀ��㣡");
			// ������ִ�е���Ŀ������߼�
		}

	}
}
