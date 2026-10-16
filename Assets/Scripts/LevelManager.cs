using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] private int pointsPerToken = 10;

    [Header("Moves")]
    [SerializeField, Min(1)] private int startingMoves = 20;

    public int CurrentScore { get; private set; }
    public int MovesRemaining { get; private set; }

    public bool HasMovesRemaining => MovesRemaining > 0;

    private void Awake()
    {
        CurrentScore = 0;
        MovesRemaining = startingMoves;
    }

    public void AddClearedTokens(int tokenCount)
    {
        if (tokenCount <= 0)
        {
            return;
        }

        int pointsEarned = tokenCount * pointsPerToken;

        CurrentScore += pointsEarned;

        Debug.Log("Earned: " + pointsEarned + " | Total Score: " + CurrentScore);
    }

    public void UseMove()
    {
        if (!HasMovesRemaining)
            return;

        MovesRemaining--;

        Debug.Log("Moves Remaining: " +  MovesRemaining);
    }
}
