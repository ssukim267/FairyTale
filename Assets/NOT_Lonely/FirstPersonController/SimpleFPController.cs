namespace NOT_Lonely
{
    using NOT_Lonely.Weatherade;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class SimpleFPController : MonoBehaviour
    {
        [Header("MOUSE LOOK")]
        public Vector2 mouseSensitivity = new Vector2(80, 80);
        public Vector2 verticalLookLimit = new Vector2(-85, 85);
        public float smooth = 0.5f;

        private float xRot;
        public Transform cam;

        [Header("MOVEMENT")]
        public bool physicsController = false;
        public float walkSpeed = 1;
        public float runSpeed = 3;
        //public float jumpForce = 2;
        private float speed = 1;

        [Header("CONTROLS")]
        public KeyCode forward = KeyCode.W;
        public KeyCode backward = KeyCode.S;
        public KeyCode strafeLeft = KeyCode.A;
        public KeyCode strafeRight = KeyCode.D;
        public KeyCode run = KeyCode.LeftShift;
        //public KeyCode jump = KeyCode.Space;

        [Header("SIGHT")]
        public bool sight = true;
        public GameObject sightPrefab;

        //private Rigidbody rb;

        public bool hideCursor = false;

        private bool forwardMove = false;
        private bool backwardMove = false;
        private bool leftMove = false;
        private bool rightMove = false;

        private CharacterController controller;
        private Animator camAnimator;
        private string lastAnim;
        private string curAnim;
        private float normalizedSpeed;

        private void OnDisable()
        {
            Cursor.visible = true;
        }

        void Start()
        {
            controller = GetComponent<CharacterController>();
            camAnimator = GetComponentInChildren<Camera>().GetComponent<Animator>();

            //cam = GetComponentInChildren<Camera>();
            //rb = GetComponent<Rigidbody>();

            if (hideCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }


            if (sight)
            {
                GameObject sightObj = Instantiate(sightPrefab);
                sightObj.transform.SetParent(transform.parent);
            }
        }

        void Update()
        {
            CameraLook();

            PlayerMove();
        }

        float refVelX;
        float refVelY;
        float xRotSmooth;
        float yRotSmooth;

        void CameraLook()
        {
            float mouseX = Input.GetAxis("Mouse X") * 0.01f * mouseSensitivity.x * 10;
            float mouseY = Input.GetAxis("Mouse Y") * 0.01f * mouseSensitivity.y * 10;

            xRot -= mouseY;
            xRot = Mathf.Clamp(xRot, verticalLookLimit.x, verticalLookLimit.y);

            xRotSmooth = Mathf.SmoothDamp(xRotSmooth, xRot, ref refVelX, smooth);
            yRotSmooth = Mathf.SmoothDamp(yRotSmooth, mouseX, ref refVelY, smooth);

            cam.transform.localEulerAngles = new Vector3(xRotSmooth, 0, 0);
            transform.Rotate(Vector3.up * yRotSmooth);
        }

        Vector3 prevPos;
        float delta;
        void PlayerMove()
        {
            if (Input.GetKey(run))
            {
                speed = runSpeed;
            }
            else
            {
                speed = walkSpeed;
            }

            if (Input.GetKey(forward))
            {
                forwardMove = true;

                if (speed == runSpeed) curAnim = "CamShakeRun";
                else curAnim = "CamShakeWalk";
            }
            else
            {
                forwardMove = false;
            }

            if (Input.GetKey(backward))
            {
                backwardMove = true;

                if (speed == runSpeed) curAnim = "CamShakeRun";
                else curAnim = "CamShakeWalk";
            }
            else
            {
                backwardMove = false;
            }

            if (Input.GetKey(strafeLeft))
            {
                leftMove = true;

                if (speed == runSpeed) curAnim = "CamShakeRun";
                else curAnim = "CamShakeWalk";
            }
            else
            {
                leftMove = false;
            }
            if (Input.GetKey(strafeRight))
            {
                rightMove = true;

                if (speed == runSpeed) curAnim = "CamShakeRun";
                else curAnim = "CamShakeWalk";
            }
            else
            {
                rightMove = false;
            }

            if (!Input.anyKey) curAnim = "CamShakeIdle";

            if (curAnim != lastAnim)
            {
                camAnimator.CrossFadeInFixedTime(curAnim, 0.3f);
            }

            lastAnim = curAnim;
        }

        private void FixedUpdate()
        {
            if (forwardMove)
            {
                controller.Move(controller.transform.forward * speed * 0.01f);
            }

            if (backwardMove)
            {
                controller.Move(controller.transform.forward * -speed * 0.01f);
            }

            if (leftMove)
            {
                controller.Move(controller.transform.right * -speed * 0.01f);
            }
            if (rightMove)
            {
                controller.Move(controller.transform.right * speed * 0.01f);
            }

            if (!forwardMove && !backwardMove && !leftMove && !rightMove)
                controller.velocity.Set(0, 0, 0);

            if (controller.isGrounded) return;

            if (Physics.SphereCast(transform.position, controller.radius, -transform.up, out RaycastHit hitInfo, 50, -1, QueryTriggerInteraction.Ignore))
            {
                transform.position = new Vector3(transform.position.x, hitInfo.point.y + controller.height / 2 + controller.skinWidth, transform.position.z);
            }
        }
    }
}
