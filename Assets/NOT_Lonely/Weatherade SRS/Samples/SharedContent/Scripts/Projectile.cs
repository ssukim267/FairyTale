namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Projectile : MonoBehaviour
    {
        [Range(0, 1)] public float destructAngleThreshold = 0.4f;
        public float minVelocityForFX = 2;
        public ParticleSystem snowHitFX;
        public ParticleSystem destructFX;
        public LayerMask snowLayermask = ~0;
        private Collider coll;
        private MeshRenderer mr;
        private Rigidbody rb;

        private bool isHit;
        private bool isTouching;

        // Start is called before the first frame update
        void Awake()
        {
            coll = GetComponent<Collider>();
            mr = GetComponent<MeshRenderer>();
            rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isHit) return;

            ContactPoint contactPoint = collision.GetContact(0);
            Vector3 pos = contactPoint.point;
            Vector3 normal = contactPoint.normal;

            float dot = Vector3.Dot(normal, rb.velocity.normalized);
            if (destructAngleThreshold < dot)
            {
                ParticleSystem pSys = Instantiate(destructFX, pos, Quaternion.LookRotation(normal, Vector3.up));
                pSys.Play();
                isHit = true;

                Destroy(gameObject);
            }
            else
            {
                if ((snowLayermask.value & (1 << collision.gameObject.layer)) != 0)
                    PlayFX();

                StartCoroutine(DestroyDelayed());
            }
        }


        ParticleSystem.VelocityOverLifetimeModule velOLT;
        private void OnCollisionStay(Collision collision)
        {
            if ((snowLayermask.value & (1 << collision.gameObject.layer)) == 0) return;

            PlayFX();
        }

        private void OnCollisionExit(Collision collision)
        {
            if ((snowLayermask.value & (1 << collision.gameObject.layer)) == 0) return;

            snowHitFX.Stop();
        }

        private void PlayFX()
        {

            float speed = rb.velocity.magnitude;

            if (speed > minVelocityForFX)
            {
                velOLT = snowHitFX.velocityOverLifetime;
                velOLT.speedModifierMultiplier = speed * 0.5f;

                if (snowHitFX.isPlaying) return;

                snowHitFX.Play();
            }
            else
            {
                snowHitFX.Stop();
            }
        }

        IEnumerator DestroyDelayed()
        {
            yield return new WaitForSeconds(5);
            Destroy(gameObject);
        }
    }
}
