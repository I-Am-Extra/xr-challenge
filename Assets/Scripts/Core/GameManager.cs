using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR.Player; //Using our player library

namespace XR.CoreGame{
    public class GameManager : MonoBehaviour
    {
        //General
        public int starsNeeded = 5;
        private UIManager uiManager;

        //Player
        private GameObject player; //Player object
        private PlayerScript pScript; //(XR.Player.PlayerScript)

        // Start is called before the first frame update
        void Start()
        {
            //Find scripts
            player = GameObject.FindWithTag("Player"); //Player object
            pScript = player.GetComponent<PlayerScript>(); //Player Script
            uiManager = FindObjectOfType<UIManager>(); //UI Manager

            //Handle Lambda
            //(On Star Picked up)
            pScript.onPickedUp = (pickup) => {
                int score = pickup.ScoreValue;
                int total = pScript.AddScore( score );

                uiManager.SetScore(total);
            };
        }

        public void onPlayerAttemptExit(GameObject ply)
        {
            int stars = pScript.stars; //we already have the player script component..
            if (stars < starsNeeded){
                return;
            }

            print("END GAME");
        }

    }
}
