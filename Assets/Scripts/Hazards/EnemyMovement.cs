using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace XR.Hazards{

    public enum EnemyState
    {
        Guard,
        Hunt,
        Chase
    }

    public class EnemyMovement : MonoBehaviour
    {
        //General
        public LayerMask ignoreMask;
        public float viewRadius;
	    [Range(0,360)] public float viewAngle;
        //--
        private NavMeshAgent agent;
        [SerializeField] private EnemyState state;

        //Guard
        public int curStar = -1;
        public int patrolTimeSeconds = 5;
        public int patrolRange = 5;
        public GameObject[] stars;
        //--
        [SerializeField] private bool seePlayer = false;
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

        // Start is called before the first frame update
        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            exit = GameObject.FindWithTag("Exit");
            player = GameObject.FindWithTag("Player");
            playerController = player.GetComponent<CharacterController>();
            guardTime = -1;
            
            ResetState();
        }

        // Update is called once per frame
        void Update()
        {
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

            if (seePlayer){
                state = EnemyState.Chase;
                return;
            }
            else if (guardTime < Time.time){
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
                state = EnemyState.Guard;
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
