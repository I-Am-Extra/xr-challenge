using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace XR.Hazards{
    public enum EnemyState
    {
        Guard = 0,
        Hunt = 1,
        Chase = 2
    }

    public class EnemyMovement : MonoBehaviour
    {
        //General
        public LayerMask ignoreMask;
        public float viewRadius;
	    [Range(0,360)] public float viewAngle;
        public Color[] lightColors = new Color[3];
        public EnemyState state;
        //--
        private NavMeshAgent agent;
        private GameObject mesh;
        private Animator animator;
        private Light viewLight = null;

        //Guard
        public int curStar = -1;
        public int patrolTimeSeconds = 5;
        public int patrolRange = 5;
        public GameObject[] stars;
        public bool seePlayer = false;
        //--
        private GameObject player;
        private CharacterController playerController;
        private GameObject exit;
        private Vector3 guardPoint = Vector3.zero;
        private float guardTime = -1f;

        //Chase
        [Range(4,15)] public int playerSearchTime = 5;
        //--
        private Vector3 lastKnownPos;
        private float startPlayerSearch = -1;

        //Footsteps
        public AudioClip[] footstepSounds;
        //--
        private AudioSource leftFoot;
        private AudioSource rightFoot;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            exit = GameObject.FindWithTag("Exit");
            player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<CharacterController>();
            guardTime = -1;

            //View Light (for vision cone visualisation)
            mesh = transform.GetChild(0).gameObject;
            Transform lightParent = mesh.transform.Find("VisionLight");
            if (lightParent != null){
                viewLight = lightParent.GetComponent<Light>();
                viewLight.range = viewRadius + 1;//Make light go slightly further
                viewLight.spotAngle = viewAngle;
            }
            
            //Animator
            animator = mesh.GetComponent<Animator>();

            //Footsteps
            Transform lfoot = transform.Find("LeftFootstep");
            Transform rfoot = transform.Find("RightFootstep");
            leftFoot = lfoot.GetComponent<AudioSource>();
            rightFoot = rfoot.GetComponent<AudioSource>();
            
            ResetState();
        }

        // Update is called once per frame
        void Update()
        {
            if (viewLight != null)
                HandleLightColor();

            //Anims
            HandleAnimations();

            switch (state)
            {
                case EnemyState.Guard:
                    HandleGuard();
                    break;
                case EnemyState.Hunt:
                    HandleHunt();
                    break;
                case EnemyState.Chase:
                    HandleChase();
                    break;
            }
        }

        private void HandleLightColor()
        {
            //Set vision cone color based on state
            Color newCol = lightColors[(int)state];
            if (viewLight.color != newCol)
                viewLight.color = newCol;
        }

        private void HandleAnimations()
        {
            float movementSpeed = agent.velocity.magnitude;
            float mag = (movementSpeed/agent.speed);
            animator.SetFloat("MovementMagnitude", mag);

            if (movementSpeed > 0.1f)
                animator.SetBool("moving", true);
            else
                animator.SetBool("moving", false);
        }

        //-----------
        //FOOTSTEPS
        public void onLeftFootStomp()
        {
            leftFoot.Stop();

            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            leftFoot.clip = clip;
        
            leftFoot.Play();
        }

        public void onRightFootStomp()
        {
            rightFoot.Stop();

            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            rightFoot.clip = clip;
        
            rightFoot.Play();
        }
        //-- -- -- --
        //STATES -- -
        private void ResetState()
        {
            state = EnemyState.Guard;
        }

        private void HandleGuard()
        {
            if (curStar == -1)
                return;
            
            seePlayer = FindPlayerInViewCone();

            //If we see the player engage 
            if (seePlayer){
                state = EnemyState.Chase;
                return;
            }
            else if (guardTime < Time.time){
                //Find a random position near current star to patrol around
                Vector3 starPos = stars[curStar].transform.position;
                int rand_x = Random.Range(-patrolRange, patrolRange);
                int rand_z = Random.Range(-patrolRange, patrolRange);
                
                guardPoint = starPos + new Vector3(rand_x, 0, rand_z);
                guardPoint.y = transform.position.y;
                guardTime = Time.time + patrolTimeSeconds;

                agent.ResetPath();
                agent.SetDestination(guardPoint);
            }
        }

        private void HandleHunt()
        {
            //If we see the player we chase them
            seePlayer = FindPlayerInViewCone();
            if (seePlayer){
                state = EnemyState.Chase;
                return;
            }

            if (startPlayerSearch + playerSearchTime > Time.time) {
                Vector3 playerPos = player.transform.position;

                //For the first second after starting search know exact position
                //To prevent just being able to exit their view cone to escape / Dumb AI
                if (startPlayerSearch + 1 > Time.time){
                    //We kind of cheat a little by making it so our AI knows when a player has just escaped Line of Sight
                    //For 1 second after escaping

                    //Go to last position
                    lastKnownPos = playerPos;
                    agent.SetDestination(lastKnownPos);
                }else if (guardTime < Time.time) { 
                    //If not found player, do a small patrol

                    //Patrol to a random point near last known position
                    //Change positions every half a second
                    //Do this for the remaining duration of Hunt
                    int rand_x = Random.Range(-patrolRange, patrolRange);
                    int rand_z = Random.Range(-patrolRange, patrolRange);
                    
                    guardPoint = lastKnownPos + new Vector3(rand_x, 0, rand_z);
                    guardPoint.y = transform.position.y;
                    guardTime = Time.time + 0.5f;

                    agent.ResetPath();
                    agent.SetDestination(guardPoint);
                }
            } else //If we haven't found player, return to guarding
                ResetState();
        }

        private void HandleChase()
        {
            seePlayer = FindPlayerInViewCone();
            Vector3 playerPos = player.transform.position;

            //If we see the player, go to where they are running to
            if (seePlayer){
                agent.SetDestination(playerPos + playerController.velocity);
                lastKnownPos = playerPos; //Last known position
                startPlayerSearch = Time.time; //Will always be exact time in which we last had eyes on player
            } else {
                agent.ResetPath();
                transform.LookAt(playerPos + playerController.velocity); //Look at where player is running to
                state = EnemyState.Hunt; //Hunt player
            }
        }
        //-- -- --
        //HELPER
        private bool FindPlayerInViewCone() 
        {
            bool found = false;
            float distToPlayer = Vector3.Distance (transform.position, player.transform.position);

            if (distToPlayer <= 1)
                return true;
            
            //If distance less than view radius, check for cone
            if (distToPlayer < viewRadius){
                //Direction towards player
                Vector3 playerDir = (player.transform.position - transform.position).normalized;

                //Angle between direction enemy is looking / direction to player < (less than) half of view Angle
                //The players direction is within the view cone
                if (Vector3.Angle(transform.forward, playerDir) < viewAngle / 2) {
                    //Check for obstacles via a raycast towards the player
                    //We ignore the players layer-mask but instead look for collisions on obstacles only
                    //If there is no collision (on obstacles) there must be a clear line of sight
                    if (!Physics.Raycast (transform.position, playerDir, distToPlayer, ignoreMask))
                        found = true;
                }
            }

            return found;
	    }
    }
}
