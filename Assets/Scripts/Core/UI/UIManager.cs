using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XR.CoreGame{
    public class UIManager : MonoBehaviour
    {
        public TMP_Text scoreText;
        public GameObject gameOverPanel;
        public AudioClip deathSound;
        public AudioClip winSound;
        public float textFadeTime = 1f;
        public Color failCol;
        public Color winCol;
        //--
        private MusicManager musicManager;
        private TMP_Text deathText;
        private Image panelImage;
        private Color lerpStartColor;
        private Color lerpEndColor;
        private float startFadeTime;
        private bool isLerping = false;

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
                float step = Mathf.Clamp((textFadeTime - timeLeft) / textFadeTime, 0, 1);

                deathText.color = Color.Lerp(lerpStartColor, lerpEndColor, step);
                panelImage.color = new Color(0,0,0,step);

                if (step >= 1)
                    isLerping = false;
            }
        }

        //--
        public void SetScore(int score)
        {
            scoreText.SetText(score.ToString());
        }

        public void MakeTextFadeIn(bool win=false)
        {
            gameOverPanel.active = true;
            //--
            Color originalCol = failCol;
            string text = "YOU DIED";
            if (win == true){
                originalCol = winCol;
                text = "YOU WIN";
            }

            Color newCol = new Color(originalCol.r, originalCol.g, originalCol.b, 0);
            deathText.color = newCol;
            deathText.SetText(text);

            lerpStartColor = newCol;
            lerpEndColor = originalCol;
            startFadeTime = Time.time;
            isLerping = true;
            //--
            if (win == false)
                musicManager.PlayDeathSound(deathSound);
            else
                musicManager.PlayDeathSound(winSound);
        }
    }
}
