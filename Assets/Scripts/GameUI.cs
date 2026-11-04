using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text mobileScoreText;
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text mobileMovesText;

    private int lastScore = -1;
    private int lastMoves = -1;

    private void Start()
    {
        RefreshDisplay();
    }

    private void Update()
    {
        // Only change the text when a value changes.
        if (levelManager.CurrentScore != lastScore ||
            levelManager.MovesRemaining != lastMoves)
        {
            RefreshDisplay();
        }
    }

    private void RefreshDisplay()
    {
        lastScore = levelManager.CurrentScore;
        lastMoves = levelManager.MovesRemaining;

        scoreText.text = "Score: " + lastScore;
        mobileScoreText.text = "Score: " + lastScore;
        movesText.text = "" + lastMoves;
        mobileMovesText.text = "" + lastMoves;
    }
}