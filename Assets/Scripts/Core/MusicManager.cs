using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XR.CoreGame{
    public class MusicManager : MonoBehaviour
    {
        //General
        public AudioClip[] musicStates = new AudioClip[3]; //3 tracks (hidden, hunted, being chased)
        [Range(0,100)] public int userMaxVolume = 50; //Max volume music can play at
        //--
        private bool playingDeath = false; //Playing win/lose state sound? (stops music logic)
        private float maxVolume; //Max volume as float (0-1)
        [SerializeField] private int highestDetectionState = -1; //Current highest detection state (hidden, hunted, chased)
        private int prevDetectionState = -1; //Detection state before updating
        private float detectionStateTime = -1f; //Time of detection state change
        private bool handleStateChange = false; //Handle a new change?
        private GameObject cam; //Camera object
        private AudioSource musicPlayer; //Music Source
        [SerializeField] private int curClip; //Currently playing state (0-2)

        // Start is called before the first frame update
        void Start()
        {
            cam = Camera.main.gameObject;
            maxVolume = (float)userMaxVolume/100;

            //Add an audio source
            musicPlayer = cam.gameObject.AddComponent<AudioSource>() as AudioSource;
            musicPlayer.volume = maxVolume; //Update settings
            musicPlayer.loop = true;
        }

        // Update is called once per frame
        void Update()
        {
            //If handling win/lose state, do not run logic
            if (playingDeath)
                return;
            //-- --
            if (handleStateChange){
                AudioClip nextClip = musicStates[highestDetectionState];//Music track based on detection status

                //If resetting detection status OR hunting and not seen within 2 seconds
                //FADE the back to undetected track in
                if (highestDetectionState == 0 || highestDetectionState == 1 && Time.time >= (detectionStateTime+2) ){
                    //Update currently playing clip
                    curClip = highestDetectionState + 0;
                    handleStateChange = false;

                    //Fade this track in
                    StartCoroutine( FadeNextTrack(nextClip, 0.3f) );
                } else if (highestDetectionState == 2 && curClip != highestDetectionState) {
                    //If spotted and not playing current track
                    //Do not fade, play asap
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
            //Fade music out
            //While detection state and clip playing match
            while (musicPlayer.volume > 0 && highestDetectionState == curClip) {
                musicPlayer.volume -= volume * Time.deltaTime / fadeOutTime;
    
                yield return null;
            }
    
            //Restore original volume
            musicPlayer.Stop ();
            musicPlayer.volume = volume;

            //Play next track
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

        public void PlayDeathSound(AudioClip deathClip)
        {
            playingDeath = true;
            musicPlayer.Stop();

            musicPlayer.volume = maxVolume;
            musicPlayer.clip = deathClip;
            musicPlayer.Play();
        }
    }
}
