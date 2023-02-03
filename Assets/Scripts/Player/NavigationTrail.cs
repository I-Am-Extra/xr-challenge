using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Player
{
    public class NavigationTrail : MonoBehaviour
    {
        //General
        public float rotateSpeed = 200;
        public int starsNeeded;
        //--
        private PlayerScript pScript;
        private Transform parent;
        private GameObject[] stars;
        private float startingRotationX;
        private Transform exit;

        // Start is called before the first frame update
        void Start()
        {
            parent = transform.root;
            transform.SetParent(null);
            pScript = parent.GetComponent<PlayerScript>();
            exit = GameObject.FindWithTag("Exit").transform;

            stars = GameObject.FindGameObjectsWithTag("Star");
            startingRotationX = transform.eulerAngles.x;
        }

        // Update is called once per frame
        void Update()
        {
            Transform selectedStar = null;
            if (pScript.stars >= starsNeeded){
                selectedStar = exit;
            } else {
                float minDist = 1000;
                for (int i=0; i < stars.Length; i++){
                    if (stars[i] == null)
                        continue;
                    //-- --
                    Transform star = stars[i].transform;
                    float dist = Vector3.Distance( star.position, parent.position );

                    if (dist < minDist){
                        minDist = dist;
                        selectedStar = star;
                    }
                }                
            }

            if (selectedStar == null)
                return;

            Vector3 dirTo = (selectedStar.position - parent.position).normalized;
            dirTo.y = 0;
            Quaternion newRot = Quaternion.LookRotation(dirTo, Vector3.up);

            float step = rotateSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards( transform.rotation, newRot, step );;
            transform.position = parent.position;
        }
    }
}
