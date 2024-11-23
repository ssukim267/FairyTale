using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public Transform target;
    public Vector3 posOffset;

    public bool followRotation = true;
    public Vector3 rotOffset;

    void Awake()
    {
        
    }

    void Update()
    {
        transform.position = target.TransformPoint(posOffset);
        if(followRotation) transform.rotation = target.rotation * Quaternion.Euler(rotOffset);
    }

    public void SetFollowTarget(Transform followTarget)
    {
        target = followTarget;
    }
}
