using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.CoreGame{
    public class ExitTrigger : MonoBehaviour
    {
        private GameManager gameManager;

        // Start is called before the first frame update
        void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        //Upon collision with another GameObject, this GameObject will reverse direction
        private void OnTriggerEnter(Collider other)
        {
            string tag = other.gameObject.tag;
            if (tag != "Player")
                return;

            gameManager.onPlayerAttemptExit(other.gameObject);
        }
    }
}