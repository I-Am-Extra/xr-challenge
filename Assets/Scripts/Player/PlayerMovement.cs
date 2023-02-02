using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; //For easily finding controllers

//Associate this script with the XR.Player library
//On compile Unity will make a .dll for each assembly definition, allowing us to organize our code into libraries to promote modularity and reusability
//By default every script without a .asmdef will be put into Assembly-CSharp.dll
namespace XR.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        //General
        private Transform cam;
        private GameObject mesh;
        private Animator animator;
        private PlayerScript pScript;

        //Movement
        public float speed = 10;
        public float analogRotateSpeed = 200;
        //--
        private CharacterController movementController;

        //Input
        [SerializeField] private Vector2 input; //stick / keyboard value (0-1)
        private Gamepad controller; //only x-input supported
        [SerializeField] private bool useController = false;

        // Start is called before the first frame update
        void Start()
        {
            //Check for controller + set if available
            CheckForController();

            pScript = GetComponent<PlayerScript>();
            movementController = GetComponent<CharacterController>();
            cam = Camera.main.transform;
            //camScript = cam.GetComponent<GameCam>();

            mesh = transform.Find("PlayerMesh").gameObject;
            animator = mesh.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            if (pScript.isDead)
                return;

            //Read input every frame
            HandleInput();

            //Handle player movement every frame
            HandleMovement();

            //Handle Rotation
            HandleRotation();

            //Handle animations
            HandleAnimations();
        }

        //--------------
        //INPUT---------
        private void CheckForController()
        {
            controller = Gamepad.current; //Just set last connected controller to main
            useController = (controller != null);
        }

        //Handle controller input
        private void HandleXinput()
        {
            Vector2 leftStick = controller.leftStick.ReadValue();
            input = leftStick;
        }

        //Handle keyboard input
        private void HandleKeyboardInput()
        {
            float forward = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");

            input = new Vector2(horizontal, forward);
        }

        //Main handle input method
        private void HandleInput()
        {
            //Check if controller is plugged in every frame
            //CheckForController();

            //Read input (value 0-1 x/y)
            if (useController)
                HandleXinput();
            else
                HandleKeyboardInput();
        }

        //--------------
        //MOVEMENT------
        //Handle main movement
        private void HandleMovement()
        {
            Vector3 cam_right = cam.right;

            //Calculate forward vector as camera forward is messed up in top down
            //Usually it would be Cross (up x right) but Unity uses a left-handed, Y-Up coordinate system
            //So the formula becomes cross(right, up)
            Vector3 cam_forward = Vector3.Cross(cam_right, Vector3.up);
            cam_forward.y = 0;
            cam_right.y = 0;

            Vector3 normal_move = (cam_forward * input.y) + (cam_right * input.x);
            Vector3 full_move = normal_move * speed;
            Vector3 move = full_move * Time.deltaTime;

            movementController.Move(move);
        }

        //--------------
        //ROTATION
        private void HandleRotation()
        {
            float step = analogRotateSpeed * Time.deltaTime;

            Quaternion camera_face = Quaternion.Euler(0,cam.transform.eulerAngles.y,0);

            Vector3 inputVec = new Vector3(input.x, 0, input.y);
            if (inputVec != Vector3.zero){
                Quaternion finalRotation = camera_face * Quaternion.LookRotation(inputVec);
                Quaternion finalQuat = Quaternion.RotateTowards( transform.rotation, finalRotation, step );
                transform.rotation = finalQuat;
            }
        }

        //--------------
        //ANIMATIONS
        private void HandleAnimations()
        {
            float movementSpeed = movementController.velocity.magnitude;
            float mag = movementSpeed / speed;

            animator.SetFloat("MovementMagnitude", mag);
            animator.SetBool("moving", movementSpeed > 0.1f);
        }
    }   

}
