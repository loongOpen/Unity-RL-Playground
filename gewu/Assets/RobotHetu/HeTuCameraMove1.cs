using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class HeTuCameraMove1 : MonoBehaviour
{
    Vector3 pos;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        pos=this.transform.position;
        pos.x+=1f;
        this.transform.position=pos;
    }
}
