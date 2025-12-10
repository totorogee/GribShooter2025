using UnityEngine;
using System.Collections.Generic;
using GameTool;

/// <summary>
/// Manages multiple score types (gold, hp, energy, etc.) dynamically.
/// Listens to EventManager for score updates.
/// Score types are created automatically when first accessed.
/// Uses PrefabSingleton pattern - can be loaded from Resources/Singleton/ScoreManager prefab.
/// 
/// Usage:
/// - EventManager.TriggerEvent("ScoreChange", "gold", 100);  // Add 100 gold
/// - EventManager.TriggerEvent("ScoreSet", "hp", 50);        // Set hp to 50
/// - int gold = ScoreManager.Instance.GetScore("gold");
/// </summary>
public class ScoreManager : PrefabSingleton<ScoreManager>
{
    
    [Header("Score Storage")]
    [SerializeField] private Dictionary<string, int> scores = new Dictionary<string, int>();
    
    [Header("Event Names")]
    [SerializeField] private string scoreChangeEventName = "ScoreChange"; // (string scoreName, int amount)
    [SerializeField] private string scoreSetEventName = "ScoreSet";       // (string scoreName, int value)
    [SerializeField] private string scoreResetEventName = "ScoreReset";   // (string scoreName)
    
    [Header("Broadcast Events")]
    [SerializeField] private bool broadcastOnChange = true;
    
    protected override void Awake()
    {
        base.Awake(); // Call PrefabSingleton's Awake
        
        // Subscribe to events
        SubscribeToEvents();
    }
    
    public override void OnDestroy()
    {
        base.OnDestroy(); // Call PrefabSingleton's OnDestroy
        
        // Unsubscribe from events
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// Subscribe to EventManager events
    /// </summary>
    private void SubscribeToEvents()
    {
        EventManager.StartListening<string, int>(scoreChangeEventName, OnScoreChange);
        EventManager.StartListening<string, int>(scoreSetEventName, OnScoreSet);
        EventManager.StartListening<string>(scoreResetEventName, OnScoreReset);
    }
    
    /// <summary>
    /// Unsubscribe from EventManager events
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        EventManager.StopListening<string, int>(scoreChangeEventName, OnScoreChange);
        EventManager.StopListening<string, int>(scoreSetEventName, OnScoreSet);
        EventManager.StopListening<string>(scoreResetEventName, OnScoreReset);
    }
    
    /// <summary>
    /// Handle score change event (add/subtract)
    /// </summary>
    private void OnScoreChange(string scoreName, int amount)
    {
        AddScore(scoreName, amount);
    }
    
    /// <summary>
    /// Handle score set event (set to specific value)
    /// </summary>
    private void OnScoreSet(string scoreName, int value)
    {
        SetScore(scoreName, value);
    }
    
    /// <summary>
    /// Handle score reset event (reset to 0)
    /// </summary>
    private void OnScoreReset(string scoreName)
    {
        ResetScore(scoreName);
    }
    
    /// <summary>
    /// Get score value by name. Returns 0 if score doesn't exist.
    /// </summary>
    public int GetScore(string scoreName)
    {
        if (scores.TryGetValue(scoreName, out int value))
        {
            return value;
        }
        return 0;
    }
    
    /// <summary>
    /// Add amount to score (can be negative to subtract)
    /// </summary>
    public void AddScore(string scoreName, int amount)
    {
        int oldValue = GetScore(scoreName);
        int newValue = oldValue + amount;
        
        scores[scoreName] = newValue;
        
        Debug.Log($"Score '{scoreName}': {oldValue} + {amount} = {newValue}");
        
        // Broadcast change event
        if (broadcastOnChange)
        {
            EventManager.TriggerEvent($"Score_{scoreName}_Changed", oldValue, newValue);
            EventManager.TriggerEvent("ScoreUpdated", scoreName, newValue);
        }
    }
    
    /// <summary>
    /// Set score to a specific value
    /// </summary>
    public void SetScore(string scoreName, int value)
    {
        int oldValue = GetScore(scoreName);
        scores[scoreName] = value;
        
        Debug.Log($"Score '{scoreName}': Set to {value} (was {oldValue})");
        
        // Broadcast change event
        if (broadcastOnChange)
        {
            EventManager.TriggerEvent($"Score_{scoreName}_Changed", oldValue, value);
            EventManager.TriggerEvent("ScoreUpdated", scoreName, value);
        }
    }
    
    /// <summary>
    /// Reset score to 0
    /// </summary>
    public void ResetScore(string scoreName)
    {
        SetScore(scoreName, 0);
    }
    
    /// <summary>
    /// Reset all scores to 0
    /// </summary>
    public void ResetAllScores()
    {
        List<string> scoreNames = new List<string>(scores.Keys);
        foreach (string scoreName in scoreNames)
        {
            ResetScore(scoreName);
        }
        Debug.Log("All scores reset to 0");
    }
    
    /// <summary>
    /// Clear all scores (remove from dictionary)
    /// </summary>
    public void ClearAllScores()
    {
        scores.Clear();
        Debug.Log("All scores cleared");
    }
    
    /// <summary>
    /// Check if a score exists
    /// </summary>
    public bool HasScore(string scoreName)
    {
        return scores.ContainsKey(scoreName);
    }
    
    /// <summary>
    /// Get all score names
    /// </summary>
    public List<string> GetAllScoreNames()
    {
        return new List<string>(scores.Keys);
    }
    
    /// <summary>
    /// Get all scores as a dictionary copy
    /// </summary>
    public Dictionary<string, int> GetAllScores()
    {
        return new Dictionary<string, int>(scores);
    }
    
    /// <summary>
    /// Get all scores as a list of score entries
    /// </summary>
    public List<ScoreEntry> GetScoreList()
    {
        List<ScoreEntry> scoreList = new List<ScoreEntry>();
        foreach (var kvp in scores)
        {
            scoreList.Add(new ScoreEntry(kvp.Key, kvp.Value));
        }
        return scoreList;
    }
    
    /// <summary>
    /// Score entry data structure for easy access to score name and value
    /// </summary>
    [System.Serializable]
    public class ScoreEntry
    {
        public string name;
        public int value;
        
        public ScoreEntry(string scoreName, int scoreValue)
        {
            name = scoreName;
            value = scoreValue;
        }
        
        public override string ToString()
        {
            return $"{name}: {value}";
        }
    }
    
    /// <summary>
    /// Remove a specific score
    /// </summary>
    public void RemoveScore(string scoreName)
    {
        if (scores.Remove(scoreName))
        {
            Debug.Log($"Score '{scoreName}' removed");
        }
        else
        {
            Debug.LogWarning($"Score '{scoreName}' not found");
        }
    }
    
    /// <summary>
    /// Clamp a score between min and max values
    /// </summary>
    public void ClampScore(string scoreName, int minValue, int maxValue)
    {
        int currentValue = GetScore(scoreName);
        int clampedValue = Mathf.Clamp(currentValue, minValue, maxValue);
        
        if (currentValue != clampedValue)
        {
            SetScore(scoreName, clampedValue);
        }
    }
    
    /// <summary>
    /// Get score count (number of different score types)
    /// </summary>
    public int GetScoreTypeCount()
    {
        return scores.Count;
    }
    
    // Debugging: Show all scores in inspector
    #if UNITY_EDITOR
    [Header("Debug - Current Scores")]
    [SerializeField] private List<ScoreDebugEntry> debugScores = new List<ScoreDebugEntry>();
    
    [System.Serializable]
    private class ScoreDebugEntry
    {
        public string name;
        public int value;
    }
    
    void Update()
    {
        // Update debug display
        debugScores.Clear();
        foreach (var kvp in scores)
        {
            debugScores.Add(new ScoreDebugEntry { name = kvp.Key, value = kvp.Value });
        }
    }
    #endif
}

