using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LevelManager levelManager;

    [Header("Score")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text mobileScoreText;

    [Header("Moves")]
    [SerializeField] private TMP_Text movesText;
    [SerializeField] private TMP_Text mobileMovesText;

    [Header("Objectives")]
    [SerializeField] private TMP_Text goalText;
    [SerializeField] private TMP_Text mobileGoalText;

    [Header("End Screen")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private TMP_Text resultTitle;
    [SerializeField] private TMP_Text resultMessage;

    private int lastScore = -1;
    private int lastMoves = -1;
    private LevelState lastState = LevelState.Playing;

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
        lastState = levelManager.CurrentState;

        //score updates
        string scoreDisplay = "Score: " + lastScore;

        if (scoreText != null)
        {
            scoreText.text = scoreDisplay;
        }
        if (mobileScoreText != null)
        {
            mobileScoreText.text = scoreDisplay;
        }

        //Move updates
        if (movesText != null)
        {
            movesText.text = lastMoves.ToString();
        }
        if (mobileMovesText != null)
        {
            mobileMovesText.text = lastState.ToString();
        }

        //update Objective
        string goalDisplay = "Goal: " + levelManager.TargetScore;

        if (goalText != null)
        {
            goalText.text = goalDisplay;
        }
        if (mobileGoalText != null)
        {
            mobileGoalText.text = goalDisplay;
        }

        //Update victory / defeat screen
        UpdateEndScreen();
    }

    private void UpdateEndScreen()
    {
        LevelState state = levelManager.CurrentState;

        bool levelEnded = state != LevelState.Playing;

        if (endScreen == null) return;

        if (levelEnded)
        {
            if (resultTitle != null)
            {
                resultTitle.text = state == LevelState.Won ? "LEVEL COMPLETE!" : "LEVEL FAILED!";
            }

            if (resultMessage != null)
            {
                resultMessage.text = "Final Score: " + levelManager.CurrentScore;
            }

        }

        endScreen.SetActive(levelEnded);
    }

    //Retry Button Logic
    public void RetryLevel()
    {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;

        if (sceneIndex < 0)
        {
            Debug.LogError(
                "Add this scene to the Build Profiles Scene List."
            );
            return;
        }

        SceneManager.LoadScene(sceneIndex);
    }
}