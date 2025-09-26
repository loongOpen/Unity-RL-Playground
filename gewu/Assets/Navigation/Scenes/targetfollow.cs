using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class targetfollow : MonoBehaviour
{
    public G1opAgent g1op;
    public Transform body;
    public Transform target;
    float vr;
    float wr;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        

        Vector3 toGoal = target.position - body.position;
        toGoal.y = 0;

        Vector3 robotForward = body.forward;
        robotForward.y = 0;

        vr = 0;
        wr = 0;

        float angleDiff = Vector3.SignedAngle(robotForward.normalized, toGoal.normalized, Vector3.up);
        if (toGoal.magnitude > 0.6f && Mathf.Abs(angleDiff) > 5) wr = Mathf.Clamp(angleDiff * 0.3f, -1f, 1f);
        else if(toGoal.magnitude > 0.6f)vr = Mathf.Clamp(toGoal.magnitude * 2f, 0f, 1.2f);

        g1op.setcmd(vr, wr);
    }
}
