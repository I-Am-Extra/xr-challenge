using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Player
{
    public class NavigationTrail : MonoBehaviour
    {
        //General
        public float rotateSpeed = 200; //Rotation speed of trail
        public int starsNeeded; //Set by external script
        //--
        private PlayerScript pScript;
        private Transform parent;
        private GameObject[] stars;
        private Transform exit;

        // Start is called before the first frame update
        void Start()
        {
            parent = transform.root;
            transform.SetParent(null); //We must unparent as player parent object will mess with our rotation
            pScript = parent.GetComponent<PlayerScript>();
            exit = GameObject.FindWithTag("Exit").transform;
            stars = GameObject.FindGameObjectsWithTag("Star");
        }

        // Update is called once per frame
        void Update()
        {
            Transform selectedStar = null;
            if (pScript.stars >= starsNeeded){
                selectedStar = exit; //If we have all stars, use exit as object
            } else {
                //Find closest star
                float minDist = 1000;
                for (int i=0; i < stars.Length; i++){
                    if (stars[i] == null)
                        continue;
                    //-- --
                    Transform star = stars[i].transform;
                    float dist = Vector3.Distance( star.position, parent.position );

                    //If distance is less than previous lowest distance
                    //Store this as the object
                    if (dist < minDist){
                        minDist = dist;
                        selectedStar = star;
                    }
                }                
            }

            //Fail safe
            if (selectedStar == null)
                return;

            //Handle trail pointing to star/exit
            //-- --
            Vector3 dirTo = (selectedStar.position - parent.position).normalized; //Get direction to object vector
            dirTo.y = 0; //To prevent pointing up/downwards

            //Make quaternion from forward and up values
            //Will make gameobject look in the direction of the forward we have provided
            Quaternion newRot = Quaternion.LookRotation(dirTo, Vector3.up);

            float step = rotateSpeed * Time.deltaTime; //Rotate speed
            transform.rotation = Quaternion.RotateTowards( transform.rotation, newRot, step ); //Slowly rotate towards goal
            transform.position = parent.position; //Constantly update position to our player
        }
    }
}
