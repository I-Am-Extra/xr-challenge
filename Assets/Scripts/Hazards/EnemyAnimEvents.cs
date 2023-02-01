using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.Hazards{
    public class EnemyAnimEvents : MonoBehaviour
    {
        private EnemyMovement movement;

        // Start is called before the first frame update
        void Start()
        {
            movement = transform.root.GetComponent<EnemyMovement>();
        }

        public void onLeftFootStomp()
        {
            movement.onLeftFootStomp();
        }

        public void onRightFootStomp()
        {
            movement.onRightFootStomp();
        }

    }
}
