using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField] private int pointsPerToken = 10;

    public int CurrentScore { get; private set; }

    private void Awake()
    {
        CurrentScore = 0;
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
}
