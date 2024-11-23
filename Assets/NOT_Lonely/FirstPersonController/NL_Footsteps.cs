namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using NOT_Lonely.Weatherade;

    public class NL_Footsteps : MonoBehaviour
    {
        public AnimationCurve stepDurationModifierCurve;

        [System.Serializable]
        public struct Footstep
        {
            public PhysicMaterial surfacePhysMat;

            public ParticleSystem rightParticles;
            public ParticleSystem leftParticles;

            public AudioClip[] sfxClips;
        }

        public float minInterval = 0.2f;
        public Footstep[] footsteps;

        private Animator animator;
        private string lastAnim;
        private string curAnim;
        private AudioSource source;
        private CharacterController playerController;
        private SimpleFPController fpController;
        private Vector3 playerVelocity;
        private float normalizedSpeed;
        private float maxSpeed;
        private bool hasStepSurface;
        private int surfaceIndex;

        private float lastTime;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            playerController = NL_Utilities.FindObjectOfType<CharacterController>(true);
            fpController = NL_Utilities.FindObjectOfType<SimpleFPController>(true);
            animator = GetComponent<Animator>();

            maxSpeed = fpController.runSpeed;
        }

        private void OnEnable()
        {
            StartCoroutine(FootstepsLoop());
        }

        private void FixedUpdate()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, 0.1f, Vector3.down, out hitInfo, 0.7f, -1, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.sharedMaterial == null) return;

                for (int i = 0; i < footsteps.Length; i++)
                {
                    if (footsteps[i].surfacePhysMat.name == hitInfo.collider.sharedMaterial.name)
                    {
                        hasStepSurface = true;
                        surfaceIndex = i;
                        break;
                    }
                    else
                    {
                        hasStepSurface = false;
                    }
                }
            }
            else
            {
                hasStepSurface = false;
            }
        }

        Vector3 prevPos;
        float delta;
        IEnumerator FootstepsLoop()
        {
            while (true)
            {
                if (hasStepSurface)
                {
                    delta = (transform.position - prevPos).magnitude * 100;
                    prevPos = transform.position;

                    normalizedSpeed = NL_Utilities.Remap(delta, 0, maxSpeed, 0, 1);

                    if (normalizedSpeed == 0)
                    {
                        curAnim = "FootstepsIdle";
                    }
                    else
                    {
                        if (normalizedSpeed > 0.01f && normalizedSpeed < 0.2f)
                            curAnim = "FootstepsWalk";
                        else if (normalizedSpeed > 0.2f)
                            curAnim = "FootstepsRun";
                    }

                    if (curAnim != lastAnim)
                        animator.CrossFadeInFixedTime(curAnim, 0.3f);

                    animator.speed = Random.Range(0.8f, 1.1f);
                    lastAnim = curAnim;
                }

                yield return new WaitForFixedUpdate();
            }
        }

        public void PlayFootstepSFX()
        {
            if (source == null || !hasStepSurface || (Time.unscaledTime - lastTime) < minInterval) return;

            source.pitch = Random.Range(0.95f, 1.05f);
            normalizedSpeed *= 0.5f;
            normalizedSpeed += 0.5f;

            if (footsteps[surfaceIndex].sfxClips != null && footsteps[surfaceIndex].sfxClips.Length != 0)
                source.PlayOneShot(footsteps[surfaceIndex].sfxClips[Random.Range(0, footsteps[surfaceIndex].sfxClips.Length)], Mathf.Clamp01(Random.Range(normalizedSpeed - 0.2f, normalizedSpeed)));

            lastTime = Time.unscaledTime;
        }

        public void PlayLeftVFX()
        {
            if (!hasStepSurface) return;

            if (footsteps[surfaceIndex].leftParticles != null)
                footsteps[surfaceIndex].leftParticles.Play();
        }

        public void PlayRightVFX()
        {
            if (!hasStepSurface) return;

            if (footsteps[surfaceIndex].rightParticles != null)
                footsteps[surfaceIndex].rightParticles.Play();
        }
    }
}