using System;
using System.Collections.Generic;
using UnityEngine;

public enum LevelState
{
    Playing,
    Won,
    Lost
}

public enum LevelObjectiveType
{
    TargetScore,
    CollectToken
}

[Serializable]
public class LevelObjective
{
    public LevelObjectiveType type = LevelObjectiveType.CollectToken;

    public TokenType tokenType = TokenType.Red;

    [Min(1)]
    public int requiredAmount = 3;
}

public class LevelManager : MonoBehaviour
{
    [Header("Scoring")]
    [SerializeField, Min(1)]
    private int pointsPerToken = 10;

    [Header("Level Objectives")]
    [SerializeField]
    private List<LevelObjective> objectives =
        new List<LevelObjective>();

    [Header("Moves")]
    [SerializeField, Min(1)]
    private int startingMoves = 20;

    //progress for each objective.
    private int[] collectedCounts;

    public int CurrentScore { get; private set; }
    public int MovesRemaining { get; private set; }

    public LevelState CurrentState { get; private set; }

    //Used by GameUI to detect progress changes.
    public int ObjectiveProgressVersion { get; private set; }

    public bool HasMovesRemaining => MovesRemaining > 0;

    public bool CanPlay =>
        CurrentState == LevelState.Playing &&
        HasMovesRemaining;

    public int ObjectiveCount => objectives.Count;

    private void Awake()
    {
        CurrentScore = 0;
        MovesRemaining = startingMoves;
        CurrentState = LevelState.Playing;
        ObjectiveProgressVersion = 0;

        if (objectives == null)
            objectives = new List<LevelObjective>();

        collectedCounts = new int[objectives.Count];

        if (objectives.Count == 0)
        {
            Debug.LogWarning(
                "LevelManager has no objectives configured!"
            );
        }
    }


    //Score

    public void AddClearedTokens(int tokenCount)
    {
        if (tokenCount <= 0 ||
            CurrentState != LevelState.Playing)
        {
            return;
        }

        int pointsEarned = tokenCount * pointsPerToken;

        CurrentScore += pointsEarned;

        Debug.Log(
            "Earned: " + pointsEarned +
            " | Total Score: " + CurrentScore
        );
    }


    //Collection
    public void AddCollectedToken(TokenType tokenType)
    {
        if (CurrentState != LevelState.Playing)
            return;

        for (int i = 0; i < objectives.Count; i++)
        {
            LevelObjective objective = objectives[i];

            if (objective == null)
                continue;

            if (objective.type != LevelObjectiveType.CollectToken)
                continue;

            if (objective.tokenType != tokenType)
                continue;

            if (collectedCounts[i] >= objective.requiredAmount)
                continue;

            collectedCounts[i]++;

            ObjectiveProgressVersion++;

            Debug.Log(
                objective.tokenType + ": " +
                collectedCounts[i] + "/" +
                objective.requiredAmount
            );
        }
    }


    //Objective Access
    public LevelObjective GetObjective(int index)
    {
        return objectives[index];
    }

    public int GetObjectiveProgress(int index)
    {
        LevelObjective objective = objectives[index];

        if (objective.type == LevelObjectiveType.TargetScore)
        {
            return CurrentScore;
        }

        return collectedCounts[index];
    }

    public int GetRemainingAmount(int index)
    {
        LevelObjective objective = objectives[index];

        return Mathf.Max(
            0,
            objective.requiredAmount - GetObjectiveProgress(index)
        );
    }

    public bool IsObjectiveFinished(int index)
    {
        return GetRemainingAmount(index) == 0;
    }

    public string ObjectiveProgressText
    {
        get
        {
            if (objectives.Count == 0)
                return "No objectives";

            List<string> lines = new List<string>();

            for (int i = 0; i < objectives.Count; i++)
            {
                LevelObjective objective = objectives[i];

                if (objective == null)
                    continue;

                int progress = GetObjectiveProgress(i);

                if (objective.type == LevelObjectiveType.TargetScore)
                {
                    lines.Add(
                        "Score: " + progress + "/" +
                        objective.requiredAmount
                    );
                }
                else
                {
                    lines.Add(
                        objective.tokenType + ": " +
                        progress + "/" +
                        objective.requiredAmount
                    );
                }
            }

            return string.Join("\n", lines);
        }
    }

    // ---------------------------------------
    // MOVES
    // ---------------------------------------

    public void UseMove()
    {
        if (!CanPlay)
            return;

        MovesRemaining--;

        Debug.Log(
            "Moves Remaining: " + MovesRemaining
        );
    }

    // ---------------------------------------
    // OBJECTIVE COMPLETION
    // ---------------------------------------

    private bool AreAllObjectivesComplete()
    {
        if (objectives.Count == 0)
            return false;

        for (int i = 0; i < objectives.Count; i++)
        {
            LevelObjective objective = objectives[i];

            if (objective == null || objective.requiredAmount < 1)
                return false;

            if (!IsObjectiveFinished(i))
                return false;
        }

        return true;
    }

    //WIN / LOSE
    

    public void CheckLevelResult()
    {
        if (CurrentState != LevelState.Playing)
            return;

        if (AreAllObjectivesComplete())
        {
            CurrentState = LevelState.Won;

            Debug.Log("LEVEL WON! All objectives complete!");
        }
        else if (!HasMovesRemaining)
        {
            CurrentState = LevelState.Lost;

            Debug.Log("LEVEL LOST! Objectives incomplete.");
        }
    }
}
