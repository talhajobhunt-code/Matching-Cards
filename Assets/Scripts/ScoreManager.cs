using UnityEngine;
using System;

public class ScoreManager : MonoBehaviour
{
    [Header("Score Settings")]
    public int baseMatchScore = 100;
    public int comboMultiplier = 50;
    public int maxCombo = 10;
    public float timeBonus = 10f;
    public float timeBonusDecay = 1f;

    // Dependencies
    private GameManager gameManager;

    // Score State
    private int currentScore = 0;
    private int currentCombo = 0;
    private int totalMatches = 0;
    private int totalMismatches = 0;
    private float gameStartTime;
    private float lastMatchTime;

    // Events
    public event Action<int> OnScoreChanged;
    public event Action<int> OnComboChanged;
    public event Action<ScoreData> OnScoreUpdated;

    // Properties
    public int CurrentScore => currentScore;
    public int CurrentCombo => currentCombo;
    public int TotalMatches => totalMatches;
    public int TotalMismatches => totalMismatches;
    public float GameTime => Time.time - gameStartTime;

    public void Initialize(GameManager gm)
    {
        gameManager = gm;
        gameStartTime = Time.time;
        lastMatchTime = gameStartTime;

        // Subscribe to game events
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged += OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(GameState newState)
    {
        if (newState == GameState.Playing)
        {
            if (currentScore == 0) // Only reset start time for new games
            {
                gameStartTime = Time.time;
                lastMatchTime = gameStartTime;
            }
        }
    }

    public void AddMatchScore()
    {
        totalMatches++;
        currentCombo++;
        currentCombo = Mathf.Min(currentCombo, maxCombo);

        int matchScore = CalculateMatchScore();
        currentScore += matchScore;

        lastMatchTime = Time.time;

        // Fire events
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
        OnScoreUpdated?.Invoke(GetScoreData());

        Debug.Log($"Match! Score: +{matchScore} (Combo: {currentCombo}x) Total: {currentScore}");
    }

    public void ResetCombo()
    {
        totalMismatches++;
        currentCombo = 0;

        OnComboChanged?.Invoke(currentCombo);
        OnScoreUpdated?.Invoke(GetScoreData());

        Debug.Log($"Mismatch! Combo reset. Total mismatches: {totalMismatches}");
    }

    private int CalculateMatchScore()
    {
        int score = baseMatchScore;

        // Add combo bonus
        if (currentCombo > 1)
        {
            score += (currentCombo - 1) * comboMultiplier;
        }

        // Add time bonus (faster matches = more points)
        float timeSinceLastMatch = Time.time - lastMatchTime;
        if (timeSinceLastMatch < timeBonusDecay)
        {
            float timeBonusMultiplier = 1f - (timeSinceLastMatch / timeBonusDecay);
            score += Mathf.RoundToInt(timeBonus * timeBonusMultiplier);
        }

        return score;
    }

    public void ResetScore()
    {
        currentScore = 0;
        currentCombo = 0;
        totalMatches = 0;
        totalMismatches = 0;
        gameStartTime = Time.time;
        lastMatchTime = gameStartTime;

        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
        OnScoreUpdated?.Invoke(GetScoreData());
    }

    public ScoreData GetScoreData()
    {
        return new ScoreData
        {
            score = currentScore,
            combo = currentCombo,
            matches = totalMatches,
            mismatches = totalMismatches,
            gameTime = GameTime,
            accuracy = CalculateAccuracy()
        };
    }

    public void LoadScoreData(ScoreData data)
    {
        currentScore = data.score;
        currentCombo = data.combo;
        totalMatches = data.matches;
        totalMismatches = data.mismatches;

        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
        OnScoreUpdated?.Invoke(GetScoreData());
    }

    private float CalculateAccuracy()
    {
        int totalAttempts = totalMatches + totalMismatches;
        if (totalAttempts == 0) return 1f;

        return (float)totalMatches / totalAttempts;
    }

    public int CalculateFinalScore()
    {
        int finalScore = currentScore;

        // Add completion time bonus
        float gameTimeMinutes = GameTime / 60f;
        if (gameTimeMinutes < 2f) // Bonus for completing under 2 minutes
        {
            finalScore += Mathf.RoundToInt((2f - gameTimeMinutes) * 500f);
        }

        // Add accuracy bonus
        float accuracy = CalculateAccuracy();
        if (accuracy > 0.8f) // 80% accuracy bonus
        {
            finalScore += Mathf.RoundToInt((accuracy - 0.8f) * 1000f);
        }

        return finalScore;
    }

    public string GetScoreText()
    {
        return $"Score: {currentScore:N0}";
    }

    public string GetComboText()
    {
        if (currentCombo <= 1) return "";
        return $"Combo x{currentCombo}!";
    }

    public string GetStatsText()
    {
        float accuracy = CalculateAccuracy() * 100f;
        int minutes = Mathf.FloorToInt(GameTime / 60f);
        int seconds = Mathf.FloorToInt(GameTime % 60f);

        return $"Time: {minutes:00}:{seconds:00} | Accuracy: {accuracy:F1}% | Matches: {totalMatches}";
    }

    private void OnDestroy()
    {
        if (gameManager != null)
        {
            gameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }
}

[System.Serializable]
public class ScoreData
{
    public int score;
    public int combo;
    public int matches;
    public int mismatches;
    public float gameTime;
    public float accuracy;
}