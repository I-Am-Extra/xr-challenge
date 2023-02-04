using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XR.Player;

namespace XR.Hazards{
    public class SwordTrigger : MonoBehaviour
    {
        public bool isSwinging = false;
        //--
        private GameObject player;
        private PlayerScript playerScript;

        // Start is called before the first frame update
        void Start()
        {
            player = GameObject.FindWithTag("Player");
            playerScript = player.GetComponent<PlayerScript>();
        }

        void OnTriggerEnter(Collider collision)
        {
            if (isSwinging == false)
                return;
            //--
            string tag = collision.gameObject.tag;
            if (tag != "Player")
                return;

            playerScript.onSwordHit();
        }
    }
}
