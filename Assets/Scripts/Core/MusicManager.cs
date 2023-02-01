using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.CoreGame{
    public class MusicManager : MonoBehaviour
    {
        //General
        public AudioClip[] musicStates = new AudioClip[3];
        [Range(0,100)] public int userMaxVolume = 50;
        //--
        private float maxVolume;
        [SerializeField] private int highestDetectionState = -1;
        private int prevDetectionState = -1;
        private float detectionStateTime = -1f;
        private bool handleStateChange = false;
        private GameObject cam;
        private AudioSource musicPlayer;
        [SerializeField] private int curClip;

        // Start is called before the first frame update
        void Start()
        {
            cam = Camera.main.gameObject;
            maxVolume = (float)userMaxVolume/100;

            musicPlayer = cam.gameObject.AddComponent<AudioSource>() as AudioSource;
            musicPlayer.volume = maxVolume;
            musicPlayer.loop = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (handleStateChange){
                AudioClip nextClip = musicStates[highestDetectionState];//Music track based on detection status

                //If resetting detection status OR hunting and not seen within 2 seconds
                //FADE the next track in
                if (highestDetectionState == 0 || highestDetectionState == 1 && Time.time >= (detectionStateTime+2) ){
                    StartCoroutine( FadeNextTrack(nextClip, 0.3f) );
                    curClip = highestDetectionState + 0;
                    handleStateChange = false;
                } else if (highestDetectionState == 2 && curClip != highestDetectionState) {
                    //If spotted and not playing current track
                    musicPlayer.clip = nextClip;
                    musicPlayer.Play();
                    curClip = highestDetectionState + 0;
                    handleStateChange = false;
                }
            }
        }

        //Fade out current track and play next track
        private IEnumerator FadeNextTrack(AudioClip clip, float fadeOutTime) {
            float volume = musicPlayer.volume;
            while (musicPlayer.volume > 0) {
                musicPlayer.volume -= volume * Time.deltaTime / fadeOutTime;
    
                yield return null;
            }
    
            musicPlayer.Stop ();
            musicPlayer.volume = volume;
            musicPlayer.clip = clip;
            musicPlayer.Play();
        }

        //Set current detection status
        public void SetDetectionState(int state)
        {
            prevDetectionState = highestDetectionState + 0;
            highestDetectionState = state;
            //--
            if (highestDetectionState != prevDetectionState){
                handleStateChange = true;
                detectionStateTime = Time.time;
            }
        }
    }
}
