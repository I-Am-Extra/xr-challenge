using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace XR.Player
{
    public class PlayerScript : MonoBehaviour
    {
        [HideInInspector] public bool isDead = false;
        public Action<Pickup> onPickedUp;
        public Action onHit;
        public int score; //Score of player
        public int stars; //Amount of stars
        //--
        private GameObject mesh;
        private Animator animator;
        private NavigationTrail trail;

        // Start is called before the first frame update
        void Start()
        {
            mesh = transform.Find("PlayerMesh").gameObject;
            animator = mesh.GetComponent<Animator>();
            trail = FindObjectOfType<NavigationTrail>();
        }

        public int AddScore(int add)
        {
            score += add;
            stars += 1;

            return score;
        }

        public void onSwordHit()
        {
            onHit?.Invoke();
        }

        public void WinGame()
        {
            isDead = true;
            trail.gameObject.SetActive(false);
            animator.SetBool("moving",false);
        }

        public void KillPlayer()
        {
            isDead = true;
            trail.gameObject.SetActive(false);

            int rand = Random.Range(1, 3);
            animator.CrossFade("Death" +rand, 0.25f, 0);
        }

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