namespace NOT_Lonely
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SimpleGun : MonoBehaviour
    {
        public Rigidbody projectileTemplate;
        public float impulsePower = 10;
        public float torquePower = 10;
        public float projectileSizeMul = 0.5f;

        [Range(0, 1)] public float sfxVolume = 0.5f;
        private AudioSource aSource;

        void Awake()
        {
            aSource = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Rigidbody projectile = Instantiate(projectileTemplate, transform.position, transform.rotation);
                projectile.transform.localScale = Vector3.one * projectileSizeMul;
                projectile.AddForce(transform.forward * impulsePower, ForceMode.Impulse);
                Vector3 torque = Random.insideUnitSphere * torquePower;
                projectile.AddTorque(torque);

                if (aSource != null) aSource.PlayOneShot(aSource.clip, sfxVolume);
            }
        }
    }
}
