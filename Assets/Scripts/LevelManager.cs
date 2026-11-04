using UnityEngine;

public enum LevelState
{
    Playing,
    Won,
    Lost
}

public class LevelManager : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField, Min(1)] private int pointsPerToken = 10;

    [Header("Level Objective")]
    [SerializeField, Min(1)] private int targetScore = 500;

    [Header("Moves")]
    [SerializeField, Min(1)] private int startingMoves = 20;

    public int CurrentScore { get; private set; }
    public int MovesRemaining { get; private set; }
    public int TargetScore => targetScore;
    public LevelState CurrentState { get; private set; }
    public bool HasMovesRemaining => MovesRemaining > 0;
    public bool CanPlay => CurrentState == LevelState.Playing && HasMovesRemaining;

    private void Awake()
    {
        CurrentScore = 0;
        MovesRemaining = startingMoves;
        CurrentState = LevelState.Playing;
    }

    public void AddClearedTokens(int tokenCount)
    {
        if (tokenCount <= 0 || CurrentState != LevelState.Playing)
        {
            return;
        }

        int pointsEarned = tokenCount * pointsPerToken;

        CurrentScore += pointsEarned;

        //Debug.Log("Earned: " + pointsEarned + " | Total Score: " + CurrentScore);
    }

    public void UseMove()
    {
        if (!HasMovesRemaining)
            return;

        MovesRemaining--;

        //Debug.Log("Moves Remaining: " +  MovesRemaining);
    }

    //Check after all matches and cascades finish
    public void CheckLevelResult()
    {
        if (CurrentState != LevelState.Playing) { return; }

        //wining has priority
        if (CurrentScore >= targetScore)
        {
            CurrentState = LevelState.Won;

            Debug.Log("Level Won! Final Score: " + CurrentScore);
        }
        else if (!HasMovesRemaining)
        {
            CurrentState = LevelState.Lost;

            Debug.Log("Level Lost! Final Score: " + CurrentScore);
        }
    }
}
