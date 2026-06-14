using UnityEngine;
using System;

public enum AIState { Dormant, Aggressive, Predictive, Psychological, Environmental }

/// <summary>
/// Neuromorphic state machine — recalculates every 30 seconds from live PlayerProfile weights.
/// State changes broadcast to EnemyAI so it can switch behavior immediately.
/// </summary>
public class AIStateManager : MonoBehaviour
{
    public static AIStateManager Instance;

    public AIState currentState = AIState.Dormant;

    // Other systems subscribe to this to react to state changes
    public static event Action<AIState> OnStateChanged;

    [Header("Recalculation Interval (seconds)")]
    public float recalcInterval = 30f;
    private float recalcTimer   = 0f;

    void Awake() { Instance = this; }

    void Update()
    {
        recalcTimer += Time.deltaTime;
        if (recalcTimer >= recalcInterval)
        {
            recalcTimer = 0f;
            RecalcState();
        }
    }

    public void RecalcState()
    {
        var d   = PlayerProfile.Instance.data;
        int day = DayManager.Instance.currentDay;

        AIState newState;

        if (day <= 3)
        {
            newState = AIState.Dormant;
        }
        else
        {
            // Neuromorphic weighting: each axis is a sum of related behavioral spikes
            float aggressive    = d["runCount"]   + d["fearScore"];
            float predictive    = (d["ventCount"] + d["leftBias"] + d["rightBias"]) / 3f;
            float psychological = d["hideCount"]  + d["lootAffinity"];
            float environmental = d["reactionAvg"]; // slow reactor = manipulate environment

            float max = Mathf.Max(aggressive, predictive, psychological, environmental);

            if      (max == aggressive)    newState = AIState.Aggressive;
            else if (max == predictive)    newState = AIState.Predictive;
            else if (max == environmental) newState = AIState.Environmental;
            else                           newState = AIState.Psychological;
        }

        if (newState != currentState)
        {
            currentState = newState;
            BroadcastStateChange();
        }

        Debug.Log($"[ECHO] Day {day} — State: {currentState} | Know: {PlayerProfile.Instance.knowledgeLevel:F1}%");
    }

    public void BroadcastStateChange()
    {
        OnStateChanged?.Invoke(currentState);

        // Flash HUD
        if (HUDManager.Instance != null)
            HUDManager.Instance.Flash($"TACTIC SHIFT → {currentState.ToString().ToUpper()}");
    }
}