using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemRotate : MonoBehaviour
{
    public float rotateSpeed = 50;
    public GameObject openTarget;
    void Update()
    {
        this.transform.Rotate(Vector3.up*Time.deltaTime* rotateSpeed);
    }
	private void OnTriggerEnter(Collider other)
	{
        if (other.gameObject.name != "Player") return;
		openTarget.SetActive(true);
        this.gameObject.SetActive(false);
	}
}
