using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR.Player; //Using our player library
using XR.Hazards;

namespace XR.CoreGame{
    public class GameManager : MonoBehaviour
    {
        //General
        public int starsNeeded = 5; 
        public GameObject slasherPrefab;
        //--
        private UIManager uiManager;
        private MusicManager musicManager;
        private bool playerDetected = false;
        private int highestDetectionState = -1;

        //Player
        private GameObject player; //Player object
        private PlayerScript pScript; //(XR.Player.PlayerScript)

        //AI
        private EnemyMovement[] enemies;
        private GameObject[] stars;

        // Start is called before the first frame update
        void Start()
        {
            //Find scripts
            player = GameObject.FindWithTag("Player"); //Player object
            pScript = player.GetComponent<PlayerScript>(); //Player Script
            uiManager = FindObjectOfType<UIManager>(); //UI Manager
            musicManager = FindObjectOfType<MusicManager>(); //Music manager

            //Handle Lambda
            //(On Star Picked up)
            pScript.onPickedUp = (pickup) => {
                int score = pickup.ScoreValue;
                int total = pScript.AddScore( score );

                uiManager.SetScore(total);
            };

            //AI
            enemies = new EnemyMovement[starsNeeded];
            stars = GameObject.FindGameObjectsWithTag("Star");

            for(int i = 0; i < starsNeeded; i++)
            {
                GameObject star = stars[i];
                GameObject enemy = Instantiate(slasherPrefab);

                Vector3 starPos = star.transform.position;
                enemy.transform.position = new Vector3(starPos.x, 0, starPos.z);

                EnemyMovement enemyMovement = enemy.GetComponent<EnemyMovement>();
                enemyMovement.curStar = i;
                enemyMovement.stars = stars;

                enemies[i] = enemyMovement;
                print(star);
            }
        }

        public void onPlayerAttemptExit(GameObject ply)
        {
            int stars = pScript.stars; //we already have the player script component..
            if (stars < starsNeeded){
                return;
            }

            print("END GAME");
        }

        private void Update()
        {
            int highestState = -1;
            Vector3 ppos = player.transform.position;
            for (int i=0; i < enemies.Length; i++){
                bool canSee = enemies[i].seePlayer;
                if (canSee && playerDetected != canSee)
                    playerDetected = canSee;

                int enemyState = (int)enemies[i].state;
                if (enemyState > highestState)
                    highestState = enemyState;
            }
            highestDetectionState = highestState;
            //--
            musicManager.SetDetectionState(highestState);
        }

    }
}
