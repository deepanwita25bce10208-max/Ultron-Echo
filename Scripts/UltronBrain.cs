using UnityEngine;

/// <summary>
/// UltronBrain: live neuromorphic adaptation engine.
/// Drives aggression + prediction levels from PlayerTracker data.
/// Called each frame and each day-end by DayManager.
/// </summary>
public class UltronBrain : MonoBehaviour
{
    public static UltronBrain Instance;  // NOTE: was lowercase 'instance' — fixed to match EnemyAI

    [Header("Neuromorphic Weights")]
    [Range(1f, 5f)] public float aggressionLevel = 1f;
    [Range(0f, 1f)] public float predictionLevel = 0f;

    [Header("Evolution per Day")]
    public float aggressionGrowthPerDay = 0.2f; // Ultron gets more aggressive each day regardless

    private float dayMultiplier = 1f;

    void Awake() { Instance = this; }

    void Update()
    {
        if (PlayerTracker.instance == null) return;

        float move  = PlayerTracker.instance.movementIntensity;
        float still = PlayerTracker.instance.stillTime;

        // NEUROMORPHIC ADAPTATION — real-time spike learning
        if (move > 5f)
            aggressionLevel += 0.01f * dayMultiplier; // player panicking → Ultron escalates

        if (still > 2f)
            aggressionLevel += 0.02f * dayMultiplier; // camping detected → punish it

        aggressionLevel = Mathf.Clamp(aggressionLevel, 1f, 5f);

        predictionLevel = Mathf.Lerp(predictionLevel, move * 0.1f, 0.05f);
    }

    /// <summary>Called by DayManager at end of each sleep cycle.</summary>
    public void OnDayEnd(int completedDay)
    {
        // Each day, Ultron's base aggression permanently grows — neuromorphic reinforcement
        aggressionLevel = Mathf.Clamp(
            aggressionLevel + aggressionGrowthPerDay,
            1f, 5f);

        // Day multiplier scales how fast it adapts in real-time
        dayMultiplier = 1f + (completedDay / 10f);

        Debug.Log($"[ECHO] Day {completedDay} ended. Aggression: {aggressionLevel:F2} | Multiplier: {dayMultiplier:F2}");
    }
}