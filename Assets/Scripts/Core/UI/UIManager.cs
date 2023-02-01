using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace XR.CoreGame{
    public class UIManager : MonoBehaviour
    {
        public TMP_Text scoreText;

        public void SetScore(int score)
        {
            scoreText.SetText(score.ToString());
        }
    }
}
