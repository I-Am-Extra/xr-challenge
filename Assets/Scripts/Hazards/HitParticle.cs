using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Hazards{
    public class HitParticle : MonoBehaviour
    {
        //General
        public int lifetime = 2;
        //--
        private float deathTime = -1f;
        private ParticleSystem particle;

        // Start is called before the first frame update
        void Start()
        {
            particle = GetComponent<ParticleSystem>();
            particle.Play();

            deathTime = Time.time + lifetime;
        }

        // Update is called once per frame
        void Update()
        {
            if (Time.time >= deathTime)
                Destroy(this.gameObject);
        }
    }
}
