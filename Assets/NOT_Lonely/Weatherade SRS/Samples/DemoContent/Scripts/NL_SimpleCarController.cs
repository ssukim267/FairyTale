using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NL_SimpleCarController : MonoBehaviour
{
    private float horizontalInput, verticalInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking;
    private bool isEngineWorking = true;

    [SerializeField] private float motorForce = 1000, breakForce = 3000, maxSteerAngle = 30;

    [Serializable]
    public class WheelColliders
    {
        public WheelCollider frontLeft;
        public WheelCollider frontRight;
        public WheelCollider backLeft;
        public WheelCollider backRight;
    }

    [Serializable]
    public class WheelTransforms
    {
        public Transform frontLeft;
        public Transform frontRight;
        public Transform backLeft;
        public Transform backRight;
    }

    public WheelTransforms wheelTransforms;
    public WheelColliders wheelColliders;

    [Serializable]
    public class WheelModelOffsets {
        [HideInInspector] public float frontLeft;
        [HideInInspector] public float frontRight;
        [HideInInspector] public float backLeft;
        [HideInInspector] public float backRight;
    }

    public WheelModelOffsets wheelModelOffsets;

    [Serializable]
    public class AudioSettings
    {
        [HideInInspector] public AudioSource oneShotSource;
        [HideInInspector] public AudioSource engineLoopSource;
        public AudioClip engineStartClip;
        public float engineStartVolume = 1;
    }

    public AudioSettings audioSettings;

    private Coroutine startEngineRoutine;

    private void Awake()
    {
        InitAudio();
        InitWheelOffsets();
    }

    private void InitWheelOffsets()
    {
        wheelModelOffsets.frontLeft = wheelTransforms.frontLeft.localPosition.x - wheelColliders.frontLeft.transform.localPosition.x;
        wheelModelOffsets.frontRight = wheelTransforms.frontRight.localPosition.x - wheelColliders.frontRight.transform.localPosition.x;

        wheelModelOffsets.backLeft = wheelTransforms.backLeft.localPosition.x - wheelColliders.backLeft.transform.localPosition.x;
        wheelModelOffsets.backRight = wheelTransforms.backRight.localPosition.x - wheelColliders.backRight.transform.localPosition.x;

        UpdateWheels();
    }

    private void InitAudio()
    {
        audioSettings.oneShotSource = gameObject.AddComponent<AudioSource>();
        audioSettings.engineLoopSource = gameObject.AddComponent<AudioSource>();

        audioSettings.oneShotSource.playOnAwake = false;
        audioSettings.engineLoopSource.playOnAwake = false;

        audioSettings.oneShotSource.spatialBlend = 1;
        audioSettings.engineLoopSource.spatialBlend = 1;

        audioSettings.oneShotSource.minDistance = 3;
        audioSettings.oneShotSource.maxDistance = 1000;
    }

    public void SwitchEngine(bool state)
    {
        if (state) {
            if (!isEngineWorking)
            {
                if (startEngineRoutine != null) StopCoroutine(startEngineRoutine);
                startEngineRoutine = StartCoroutine(StartEngine());
            }
        }
        else
        {
            isEngineWorking = false;
        }
    }

    private IEnumerator StartEngine()
    {
        if(audioSettings.engineStartClip != null) 
            audioSettings.oneShotSource.PlayOneShot(audioSettings.engineStartClip, audioSettings.engineStartVolume);
        
        yield return new WaitForSeconds(0.5f);

        isEngineWorking = true;
        startEngineRoutine = null;
    }

    private void FixedUpdate()
    {
        HandleMotor();

        if (!isEngineWorking) return;

        GetInput();
        Steering();
        UpdateWheels();
    }

    private void GetInput()
    {
        if (!isEngineWorking) return;

        horizontalInput = Input.GetAxis("Horizontal");

        verticalInput = Input.GetAxis("Vertical");

        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void HandleMotor()
    {
        if (isEngineWorking)
        {
            wheelColliders.frontLeft.motorTorque = verticalInput * motorForce * 0.3f;
            wheelColliders.frontRight.motorTorque = verticalInput * motorForce * 0.3f;
            wheelColliders.backLeft.motorTorque = verticalInput * motorForce * 0.6f;
            wheelColliders.backRight.motorTorque = verticalInput * motorForce * 0.6f;

            currentbreakForce = isBreaking ? breakForce : 0f;
        }
        else
        {
            currentbreakForce = breakForce;
        }

        Breaking();
    }

    private void Breaking()
    {
        wheelColliders.frontLeft.brakeTorque = currentbreakForce;
        wheelColliders.frontRight.brakeTorque = currentbreakForce;
        wheelColliders.backLeft.brakeTorque = currentbreakForce;
        wheelColliders.backRight.brakeTorque = currentbreakForce;
    }

    private void Steering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        wheelColliders.frontLeft.steerAngle = currentSteerAngle;
        wheelColliders.frontRight.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateWheel(wheelColliders.frontLeft, wheelTransforms.frontLeft, wheelModelOffsets.frontLeft);
        UpdateWheel(wheelColliders.frontRight, wheelTransforms.frontRight, wheelModelOffsets.frontRight);
        UpdateWheel(wheelColliders.backLeft, wheelTransforms.backLeft, wheelModelOffsets.backLeft);
        UpdateWheel(wheelColliders.backRight, wheelTransforms.backRight, wheelModelOffsets.backRight);
    }

    private void UpdateWheel(WheelCollider wheelCollider, Transform wheelTransform, float posOffset)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos + wheelTransform.right * posOffset;
    }
}