using System.Collections;
using System.Collections.Generic;
using UnityEngine;
# if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Events;

public class NL_EventPortal : MonoBehaviour
{
    [Tooltip("Step of the script execution in seconds.")]
    [Range(0.008f, 1)]public float checkingUpdateStep = 0.06f;

    private WaitForSeconds waitForSec;
    private bool sideB = false;

    private Transform player;
    public float portalRadius = 1;
    public float portalLengthA = 1;
    public float portalLengthB = 1;
    public UnityEvent OnPlayerEnterSideA;
    public UnityEvent OnPlayerEnterSideB;

    void Start()
    {
        player = Camera.main.transform;

        waitForSec = new WaitForSeconds(checkingUpdateStep);

        if (player != null)
            StartCoroutine("PlayerDistanceChecking");
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator PlayerDistanceChecking()
    {
        while (true)
        {
            Vector3 thisPos = transform.position;

            //calculate distance by Z (forward) axis
            float dotZ = Vector3.Dot(player.position - thisPos, transform.forward);
            float sign = Mathf.Sign(dotZ);

            Vector3 z = thisPos + dotZ * transform.forward;
            float distanceZ = (thisPos - z).magnitude * sign;

            //calculate distane by X axis
            float dotX = Vector3.Dot(player.position - thisPos, transform.right);
            Vector3 x = thisPos + dotX * transform.right;
            float distanceX = (thisPos - x).magnitude;

            //calculate distane by Y axis
            float dotY = Vector3.Dot(player.position - thisPos, transform.up);
            Vector3 y = thisPos + dotY * transform.up;
            float distanceY = (thisPos - y).magnitude;

            if (!sideB)
            {
                if (distanceZ > 0 && distanceZ < portalLengthA && distanceX < portalRadius && distanceY < portalRadius)
                {
                    //Debug.Log(gameObject.name + " triggered an event from side A");
                    OnPlayerEnterSideA.Invoke();

                    sideB = true;
                    yield return null;
                }
                else
                {
                    yield return waitForSec;
                }
            }
            else //if(twoSided)
            {
                if (distanceZ < 0 && (distanceZ * -1) < portalLengthB && distanceX < portalRadius && distanceY < portalRadius)
                {
                    //Debug.Log(gameObject.name + " triggered an event from side B");
                    OnPlayerEnterSideB.Invoke();

                    sideB = false;
                    yield return null;
                }
                else
                {
                    yield return waitForSec;
                }
            }
        }
    }

    public void ResetEventPortal ()
    {
        this.enabled = true;
        sideB = false;

        if (player != null)
        {
            StartCoroutine("PlayerDistanceChecking");
        }
    }

    public void DisableEventPortal()
    {
        StopAllCoroutines();
        this.enabled = false;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Color color = new Color(0.8f, 0.8f, 0.8f, 1);
        Gizmos.color = color;
        Handles.color = color;

        if (Selection.Contains(gameObject))
        {
            color = new Color(0, 0.4f, 1, 0.1f);
            Handles.color = color;
            Gizmos.color = color;

            Handles.DrawSolidDisc(transform.position, transform.forward, portalRadius);

            Handles.DrawWireDisc(transform.position + transform.forward * portalLengthA, transform.forward, portalRadius);

            DrawWireCylinder(portalRadius, portalLengthA);

            Handles.DrawWireDisc(transform.position + transform.forward * -portalLengthB, transform.forward, portalRadius);
            DrawWireCylinder(portalRadius, -portalLengthB);
        }
        Handles.DrawWireDisc(transform.position, transform.forward, portalRadius);

        Quaternion rotation = transform.rotation;
        Matrix4x4 trs = Matrix4x4.TRS(transform.position, rotation, transform.lossyScale);
        Gizmos.matrix = trs;

        if (Selection.Contains(gameObject))
        {
            color = new Color(0, 0.4f, 1, 0.1f);
            Handles.color = color;

            color.a = 1;
            Gizmos.color = color;
        }    
        Gizmos.matrix = Matrix4x4.identity;

        DrawArrow(transform.position, transform.forward * portalLengthA);
        
        Handles.Label(transform.position - transform.forward * -portalLengthA, "A");

        DrawArrow(transform.position, transform.forward * -portalLengthB);
        Handles.Label(transform.position - transform.forward * portalLengthB, "B");

        Gizmos.DrawIcon(transform.position, "Event.png", false);
    }
    public void DrawArrow(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
    {
        Gizmos.DrawRay(pos, direction);

        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(-arrowHeadAngle, 0, 0) * Vector3.back;
        Vector3 up = Quaternion.LookRotation(direction) * Quaternion.Euler(0, arrowHeadAngle, 0) * Vector3.back;
        Vector3 down = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -arrowHeadAngle, 0) * Vector3.back;
        Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, up * arrowHeadLength);
        Gizmos.DrawRay(pos + direction, down * arrowHeadLength);
    }

    void DrawWireCylinder(float radius, float length)
    {
        Gizmos.DrawRay(transform.position + transform.up * radius, transform.forward * length);
        Gizmos.DrawRay(transform.position - transform.up * radius, transform.forward * length);
        Gizmos.DrawRay(transform.position + transform.right * radius, transform.forward * length);
        Gizmos.DrawRay(transform.position - transform.right * radius, transform.forward * length);

        Gizmos.DrawRay(transform.position + (transform.up + transform.right) * radius * 0.70711f, transform.forward * length);
        Gizmos.DrawRay(transform.position - (transform.up + transform.right) * radius * 0.70711f, transform.forward * length);
        Gizmos.DrawRay(transform.position + (transform.up - transform.right) * radius * 0.70711f, transform.forward * length);
        Gizmos.DrawRay(transform.position - (transform.up - transform.right) * radius * 0.70711f, transform.forward * length);

        Gizmos.DrawRay(transform.position + (transform.up + transform.right * 0.42f) * radius * 0.92f, transform.forward * length);
        Gizmos.DrawRay(transform.position + (transform.up + transform.right * -0.42f) * radius * 0.92f, transform.forward * length);
        Gizmos.DrawRay(transform.position + (transform.up + transform.right * -0.42f) * radius * -0.92f, transform.forward * length);
        Gizmos.DrawRay(transform.position + (transform.up + transform.right * 0.42f) * radius * -0.92f, transform.forward * length);

        Gizmos.DrawRay(transform.position + (transform.up + transform.right * 2.29f) * radius * 0.4f, transform.forward * length);
        Gizmos.DrawRay(transform.position + (transform.up + transform.right * -2.29f) * radius * 0.4f, transform.forward * length);
        Gizmos.DrawRay(transform.position + (transform.up + transform.right * -2.29f) * radius * -0.4f, transform.forward * length);
        Gizmos.DrawRay(transform.position + (transform.up + transform.right * 2.29f) * radius * -0.4f, transform.forward * length);
    }
#endif
}
