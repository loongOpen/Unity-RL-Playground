using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    PlayerController playerController;
    void Start()
    {
		playerController=GameObject.FindObjectOfType<PlayerController>();

	}

    // Update is called once per frame
  
    public void FindTarget(GameObject obj) 
    {
        Debug.Log("Ñ°Â·Î»ÖÃ="+ obj.transform.position);
		playerController.targetPos=obj.transform.position;
        //

	}
}
