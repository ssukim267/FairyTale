using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;


public class LookOrbit : MonoBehaviour {

	public bool mouseClickInteraction = false;
	public Transform target;
	private Transform followTarget;
    public float followSpeed = 5;
	public float HeightOffset = 0f;
    public Vector3 offset;
	public float distance= 1.5f;

	public float xSpeed= 175.0f;
	public float ySpeed= 75.0f;

	public int yMinLimit= 10;
	public int yMaxLimit= 80;
	
	public float minDistance= 0.5f;
	public float maxDistance= 1.5f;
	
    private float x= 0.0f;
	private float y= 0.0f;

	private bool CamFreeze = true;
	private bool ControlsFreeze = false;
	private bool RotationFreeze = false;
    private bool canRotate = false;
	
	void  Start ()
	{
		followTarget = new GameObject().GetComponent<Transform>();
		followTarget.position = new Vector3 (target.position.x, target.position.y + offset.x + offset.y, target.position.z + offset.z);

		transform.parent = followTarget;

		Vector3 angles= transform.eulerAngles;
		x = angles.y;
		y = angles.x;


		if (GetComponent<Rigidbody> ()) 
		{
				GetComponent<Rigidbody> ().freezeRotation = true;
		}
		StartCoroutine (UnfreezeCam());
	}
		

	IEnumerator UnfreezeCam () 
	{
				yield return null;
				yield return null;
				yield return null;
				yield return null;
				yield return null;
				yield return null;

				CamFreeze = false;
	}

	void  Update (){
		if (target && !CamFreeze && !RotationFreeze) 
		{			
			if(!ControlsFreeze)
			{
				distance += Input.GetAxis("Mouse ScrollWheel")*-distance;			
				distance = Mathf.Clamp(distance, minDistance, maxDistance);
			}

			if (mouseClickInteraction)
				canRotate = Input.GetMouseButton(0);
			else canRotate = true;

            if (!ControlsFreeze && canRotate) {

                if (EventSystem.current.IsPointerOverGameObject()) return;

                x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
				y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            }

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
			Vector3 vTemp = new Vector3(0.0f, 0.0f, -distance);
			Vector3 position= rotation * vTemp + followTarget.position;

			followTarget.position = Vector3.Slerp (followTarget.position, new Vector3 (target.position.x + offset.x, target.position.y + offset.y, target.position.z + offset.z), followSpeed * Time.deltaTime);

			transform.position = Vector3.Slerp (transform.position, position, 10 * Time.deltaTime);
			transform.rotation = rotation;	
		}

		transform.LookAt (followTarget);	
	}
	
	static float  ClampAngle (float angle, float min, float max){
		
		if (angle < -360) angle += 360;

        if (angle > 360) angle -= 360;

        angle = Mathf.Clamp(angle, min, max);

        return angle;		
	}
}