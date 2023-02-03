using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XR.CoreGame{
    public class UIManager : MonoBehaviour
    {
        public TMP_Text scoreText; //Text used on Game Over Panel
        public GameObject gameOverPanel; //Game Over Panel
        public AudioClip deathSound; //Sound that plays on death
        public AudioClip winSound; //Sound that plays on win
        public float textFadeTime = 1f; //How long text takes to fade in (seconds)
        public Color failCol; //Color of failure text
        public Color winCol; //Color of winning screen text
        //--
        private MusicManager musicManager;
        private TMP_Text deathText;
        private Image panelImage;
        private Color lerpStartColor;
        private Color lerpEndColor;
        private float startFadeTime;
        private bool isLerping = false;

        //Consts
        const string winText = "YOU WIN";
        const string loseText = "YOU DIED";

        private void Start()
        {
            gameOverPanel.active = false;
            panelImage = gameOverPanel.GetComponent<Image>();
            deathText = gameOverPanel.transform.GetChild(0).GetComponent<TMP_Text>();
            musicManager = FindObjectOfType<MusicManager>();
        }

        private void Update()
        {
            //Handle Death Text Fading
            HandleDeathTextLerp();
        }

        private void HandleDeathTextLerp()
        {
            if (isLerping){
                float endFadeTime = startFadeTime + textFadeTime;
                float timeLeft = (endFadeTime - Time.time);
                float step = Mathf.Clamp((textFadeTime - timeLeft) / textFadeTime, 0, 1); //Use a timed lerp

                //Make text change from start - finish color
                //also make panel fade in at same rate
                deathText.color = Color.Lerp(lerpStartColor, lerpEndColor, step);
                panelImage.color = new Color(0,0,0,step);

                //If lerp is finished, end this callback
                if (step >= 1)
                    isLerping = false;
            }
        }

        //--
        public void SetScore(int score)
        {
            //Update score text
            scoreText.SetText("SCORE: " +score.ToString());
        }

        public void MakeTextFadeIn(bool win=false)
        {
            gameOverPanel.active = true;
            //--
            Color originalCol = failCol;
            string text = loseText;
            if (win == true){
                originalCol = winCol;
                text = winText;
            }

            //Lerp from start color
            Color newCol = new Color(originalCol.r, originalCol.g, originalCol.b, 0);
            deathText.color = newCol;

            //Set text
            deathText.SetText(text);

            //Configure lerp values (start/end col, start time)
            lerpStartColor = newCol;
            lerpEndColor = originalCol;
            startFadeTime = Time.time;
            isLerping = true;
            //--
            //Play win/lose track
            //(also stop currently playing in-game music in same call)
            if (win == false)
                musicManager.PlayDeathSound(deathSound);
            else
                musicManager.PlayDeathSound(winSound);
        }
    }
}
