using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.CoreGame{
    public class ExitTrigger : MonoBehaviour
    {
        private GameManager gameManager;

        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        //On Attempting to exit, relay to GameManager..
        private void OnTriggerEnter(Collider other)
        {
            string tag = other.gameObject.tag;
            if (tag != "Player")
                return;

            gameManager.onPlayerAttemptExit(other.gameObject);
        }
    }
}