using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Hazards{
    
    //This class is used to handle knight attacks / particle effects
    public class EnemyKnight : MonoBehaviour
    {
        //General
        [Range(2,5)] public float attackRange = 2.5f; //Range in which knight can attack
        public AudioClip[] grunts; //All grunt sounds
        public AudioClip[] swingSounds; //All possible swing sounds
        public GameObject particlePrefab; //Particle on hit
        public ParticleSystem weaponTrail; //Weapon trail
        public ParticleSystem weaponGlow; //Sword glow up
        public SwordTrigger swordTrigger; //Used to detect player hit
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
            //If we see the player
            if (movement.seePlayer){
                //Check if we are in range
                Vector3 ppos = player.transform.position;
                float dist = Vector3.Distance(transform.position, ppos);

                //Attack in range
                if (dist < attackRange && isAttacking == false)
                    Attack();
            }
        }

        private void Attack()
        {
            isAttacking = true;
            //swordTrigger.isSwinging = true;

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

        //Called on animation event
        //On sword finishes swinging
        public void onSwingFinish()
        {
            isAttacking = false;
            swordTrigger.isSwinging = false;

            weaponTrail.Stop();
            weaponGlow.Stop();
        }

        //Called at top of swing
        public void onSwingSound()
        {
            swordTrigger.isSwinging = true;
            
            //Wep Swing
            AudioClip swingSound = swingSounds[Random.Range(0, swingSounds.Length)];
            swing.clip = swingSound;
            swing.Play();
        }

        //Called when sword hits floor
        public void onSwordHitFloor()
        {
            Vector3 particlePos = transform.position + transform.forward * attackRange;
            GameObject particleInstance = Instantiate(particlePrefab, particlePos, Quaternion.identity);
            particleInstance.transform.localRotation = Quaternion.identity;
        }
    }
}
