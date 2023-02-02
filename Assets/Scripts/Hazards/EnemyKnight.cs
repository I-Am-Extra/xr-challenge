using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Hazards{
    public class EnemyKnight : MonoBehaviour
    {
        //General
        [Range(2,5)] public float attackRange = 2.5f;
        public AudioClip[] grunts;
        public AudioClip[] swingSounds;
        public GameObject particlePrefab;
        public ParticleSystem weaponTrail;
        public ParticleSystem weaponGlow;
        public SwordTrigger swordTrigger;
        //--
        private GameObject mesh;
        private GameObject player;
        private EnemyMovement movement;
        private Animator animator;
        private AudioSource mouth;
        private AudioSource swing;

        //Attack
        private bool isAttacking = false;

        // Start is called before the first frame update
        void Start()
        {
            mesh = transform.GetChild(0).gameObject;
            movement = GetComponent<EnemyMovement>();
            player = GameObject.FindWithTag("Player");
            mouth = transform.Find("Mouth").GetComponent<AudioSource>();
            swing = transform.Find("SwingSounds").GetComponent<AudioSource>();

           //Animator
            animator = mesh.GetComponent<Animator>();

            weaponTrail.Stop();
            weaponGlow.Stop();
        }

        // Update is called once per frame
        void Update()
        {
            if (movement.seePlayer){
                Vector3 ppos = player.transform.position;
                float dist = Vector3.Distance(transform.position, ppos);

                if (dist < attackRange && isAttacking == false)
                    Attack();
            }
        }

        private void Attack()
        {
            isAttacking = true;
            swordTrigger.isSwinging = true;

            //Animations
            int rand = Random.Range(1, 3);
            animator.CrossFade("Attack" +rand, 0.25f, 0);

            //Particles
            weaponTrail.Play();
            weaponGlow.Play();

            //Grunt
            AudioClip clip = grunts[Random.Range(0, grunts.Length)];
            mouth.clip = clip;
            mouth.Play();
        }

        public void onSwingFinish()
        {
            isAttacking = false;
            swordTrigger.isSwinging = false;

            weaponTrail.Stop();
            weaponGlow.Stop();
        }

        public void onSwingSound()
        {
            //Wep Swing
            AudioClip swingSound = swingSounds[Random.Range(0, swingSounds.Length)];
            swing.clip = swingSound;
            swing.Play();
        }

        public void onSwordHitFloor()
        {
            Vector3 particlePos = transform.position + transform.forward * attackRange;
            GameObject particleInstance = Instantiate(particlePrefab, particlePos, Quaternion.identity);
            particleInstance.transform.localRotation = Quaternion.identity;
        }
    }
}
