using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Hazards{
    public class EnemyAnimEvents : MonoBehaviour
    {
        private EnemyMovement movement;
        private EnemyKnight knight;

        // Start is called before the first frame update
        void Start()
        {
            Transform parent = transform.root;
            movement = parent.GetComponent<EnemyMovement>();
            knight = parent.GetComponent<EnemyKnight>();
        }

        public void onLeftFootStomp()
        {
            movement.onLeftFootStomp();
        }

        public void onRightFootStomp()
        {
            movement.onRightFootStomp();
        }

        public void swingFinish()
        {
            knight.onSwingFinish();
        }

        public void onSwordHitFloor()
        {
            knight.onSwordHitFloor();
        }

        public void onSwingSound()
        {
            knight.onSwingSound();
        }

    }
}
