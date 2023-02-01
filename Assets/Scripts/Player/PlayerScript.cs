using System;
using UnityEngine;

namespace XR.Player
{
    public class PlayerScript : MonoBehaviour
    {
        public Action<Pickup> onPickedUp;
        public int score; //Score of player
        public int stars; //Amount of stars

        // Start is called before the first frame update
        void Start()
        {
            
        }

        public int AddScore(int add)
        {
            score += add;
            stars++;

            return score;
        }

        //Upon collision with another GameObject, this GameObject will reverse direction
        private void OnTriggerEnter(Collider other)
        {
            string tag = other.gameObject.tag;
            if (tag != "Star")
                return;

            Pickup pickup = other.gameObject.GetComponent<Pickup>();
            pickup.OnPickUp += onPickedUp;
            pickup.GetPickedUp();
        }
    }
}