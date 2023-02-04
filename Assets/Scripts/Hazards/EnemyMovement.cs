using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using XR.Player;

namespace XR.Hazards{
    public enum EnemyState
    {
        Guard = 0,
        Hunt = 1,
        Chase = 2
    }

    //This class is used to handle (solely) enemy movement based on view of the player and last known position
    //The end result is sumarised as a state enum for other scripts to inspect
    public class EnemyMovement : MonoBehaviour
    {
        //General
        public LayerMask collisionMask; //Collision mask for raycast
        public float viewRadius; //View cone radius
	    [Range(0,360)] public float viewAngle; //View cone angle
        public Color[] lightColors = new Color[3]; //Colors for 3 states (Guard, Hunt, Chase)
        public EnemyState state; //Current state
        public AudioClip[] spottedGrunts; //Array of grunts (when spotted)
        //--
        private NavMeshAgent agent;
        private GameObject mesh;
        private Animator animator;
        private Light viewLight = null;
        private AudioSource mouth;
        private float timeLastGrunted = -1f;
        private PlayerScript pScript;

        //Guard
        [HideInInspector] public GameObject curGuard; //Currently guarding object
        public int patrolTimeSeconds = 5; //Time between patrol points
        public int patrolRange = 5; //Range of patrol from target position
        [HideInInspector] public bool seePlayer = false; //Do we see player?
        //--
        private GameObject player;
        private CharacterController playerController;
        private GameObject exit;
        private Vector3 guardPoint = Vector3.zero;
        private float guardTime = -1f;

        //Chase
        [Range(4,15)] public int playerSearchTime = 5; //How long do we search for player?
        //--
        private Vector3 lastKnownPos;
        private float startPlayerSearch = -1;

        //Footsteps
        public AudioClip[] footstepSounds; //Footstep audio sounds
        //--
        private AudioSource leftFoot;
        private AudioSource rightFoot;

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            exit = GameObject.FindWithTag("Exit");
            player = GameObject.FindWithTag("Player");
            pScript = player.GetComponent<PlayerScript>();
            playerController = player.GetComponent<CharacterController>();
            mouth = transform.Find("Mouth").GetComponent<AudioSource>();
            guardTime = -1;

            //View Light (for vision cone visualisation)
            mesh = transform.GetChild(0).gameObject;
            Transform lightParent = mesh.transform.Find("VisionLight");
            if (lightParent != null){
                viewLight = lightParent.GetComponent<Light>();

                //Configure spotlight with view cone configurations
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

            //Handle State
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
            animator.SetFloat("MovementMagnitude", mag); //Set blend tree value based on cur speed

            if (movementSpeed > 0.1f)
                animator.SetBool("moving", true);
            else
                animator.SetBool("moving", false);
        }

        private void DoSpottedGrunt()
        {
            //To prevent spam (small 2 second cooldown)
            if (timeLastGrunted + 2 > Time.time)
                return;

            //Time
            timeLastGrunted = Time.time;

            //Play Grunt noise
            AudioClip grunt = spottedGrunts[Random.Range(0, spottedGrunts.Length)];
            mouth.clip = grunt;
            mouth.Play();
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

        private void CalcNewPatrolSpot(Vector3 startPos, float time, bool useRand=false) //Dont randomize by default
        {
            int rand_x = Random.Range(-patrolRange, patrolRange);
            int rand_z = Random.Range(-patrolRange, patrolRange);
            Vector3 randomDir = new Vector3(rand_x, 0, rand_z);
            guardPoint = startPos + randomDir;

            //Send a raycast to point we want to go to in case of walls
            RaycastHit hit;
            bool hitPoint = Physics.Raycast(startPos, randomDir.normalized, out hit);
            if (hitPoint)
                guardPoint = hit.point + (hit.normal * 1.5f); //If a wall exists, move just in front of it

            if (useRand)
                time = Random.Range( time*.25f, time+1 );

            guardPoint.y = transform.position.y;
            guardTime = Time.time + time;

            agent.ResetPath();
            agent.SetDestination(guardPoint);
        }

        public void SetCurGuarding(GameObject obj)
        {
            curGuard = obj;
            guardTime = -1f; //Reset patrol time
        }

        private void HandleGuard()
        {
            seePlayer = FindPlayerInViewCone();

            //If we see the player engage 
            if (seePlayer){
                DoSpottedGrunt();
                state = EnemyState.Chase;
                return;
            } else if (guardTime < Time.time){
                //Find a random position near current star to patrol around
                Vector3 starPos;
                //Check if star is valid (not picked up)
                if (curGuard != null)
                    starPos = curGuard.transform.position;
                else
                    starPos = transform.position;

                CalcNewPatrolSpot(starPos, patrolTimeSeconds, true);
            }
        }

        private void HandleHunt()
        {
            //If we see the player we chase them
            seePlayer = FindPlayerInViewCone();
            if (seePlayer){
                DoSpottedGrunt();
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
                    CalcNewPatrolSpot(lastKnownPos, 0.5f);
                }
            } else //If we haven't found player, return to guarding
                ResetState();
        }

        private void HandleChase()
        {
            seePlayer = FindPlayerInViewCone(); //Do we see the player?
            Vector3 playerPos = player.transform.position;

            //If we see the player, go to where they are running to
            if (seePlayer){
                Vector3 movementPos = playerPos + (playerController.velocity * Time.deltaTime); //Where player is moving to
                float distToMovePos = Vector3.Distance(transform.position, movementPos);

                //Don't go inside of the player position, but about 2 units away
                if (distToMovePos > 2){
                    agent.SetDestination(movementPos);
                    lastKnownPos = playerPos; //Last known position
                    startPlayerSearch = Time.time; //Will always be exact time in which we last had eyes on player
                } else { //Otherwise stand still, we are in attack range
                    agent.velocity = Vector3.zero;
                    //agent.ResetPath();
                }
            } else {
                //If we cannot see the player go into hunt mode
                //and look at where they were running to
                agent.ResetPath();
                transform.LookAt(playerPos + playerController.velocity); //Look at where player is running to
                state = EnemyState.Hunt; //Hunt player
            }
        }
        //-- -- --
        //HELPER
        private bool FindPlayerInViewCone() 
        {
            if (pScript.isDead)
                return false;
            //-- --
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
                    if (!Physics.Raycast (transform.position, playerDir, distToPlayer, collisionMask))
                        found = true;
                }
            }

            return found;
	    }
    }
}
