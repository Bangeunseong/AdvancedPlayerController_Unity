using System;
using System.Collections;
using Project.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace Project.Scripts.UI
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;

        private void Start()
        {
            UpdateScore();
        }

        public void UpdateScore()
        {
            // Make sure all logic has run before updating the score
            StartCoroutine(UpdateScoreNextFrame());
        }

        private IEnumerator UpdateScoreNextFrame()
        {
            yield return null;
            scoreText.text = GameManager.Instance.Score.ToString();
        }
    }
}