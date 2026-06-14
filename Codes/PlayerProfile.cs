using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Neuromorphic player profile.
/// All values are 0–100 "weight" floats.
/// Spike() = synaptic strengthening. DecayAll() = forgetting on sleep.
/// </summary>
public class PlayerProfile : MonoBehaviour
{
    public static PlayerProfile Instance;

    public Dictionary<string, float> data = new Dictionary<string, float>()
    {
        { "hideCount",    0f },
        { "ventCount",    0f },
        { "runCount",     0f },
        { "leftBias",     0f },
        { "rightBias",    0f },
        { "fearScore",    0f },
        { "lootAffinity", 0f },
        { "reactionAvg",  0f },
    };

    // Raw integer counters for Day 50 dialogue — not decayed
    public Dictionary<string, int> rawCounts = new Dictionary<string, int>()
    {
        { "ventUses",    0 },
        { "hideSpots",   0 },
        { "leftTurns",   0 },
        { "rightTurns",  0 },
        { "lootPickups", 0 },
    };

    public float knowledgeLevel = 0f; // 0–100%
    public bool  lootTrapFired  = false;

    void Awake() { Instance = this; }

    /// <summary>Strengthen a synaptic weight (neuromorphic spike).</summary>
    public void Spike(string key, float delta)
    {
        if (!data.ContainsKey(key)) return;
        data[key] = Mathf.Clamp(data[key] + delta, 0f, 100f);
        RecalcKnowledge();
    }

    /// <summary>Increment a raw integer counter for end-game dialogue.</summary>
    public void IncrementRaw(string key)
    {
        if (rawCounts.ContainsKey(key))
            rawCounts[key]++;
    }

    void RecalcKnowledge()
    {
        float total = 0f;
        foreach (var v in data.Values) total += v;
        // Multiply so knowledge climbs fast and feels threatening
        knowledgeLevel = Mathf.Clamp(
            (total / (data.Count * 100f)) * 100f * 8f,
            0f, 100f);
    }

    /// <summary>Called each sleep cycle — biological forgetting.</summary>
    public void DecayAll()
    {
        var keys = new List<string>(data.Keys);
        foreach (var k in keys)
            data[k] *= 0.92f;
        // Raw counts intentionally NOT decayed — Ultron remembers totals
        RecalcKnowledge();
    }

    /// <summary>Dominant behavioral trait for Day 50 dialogue.</summary>
    public string GetDominantTrait()
    {
        string dominant = "hideCount";
        float max = 0f;
        foreach (var kv in data)
            if (kv.Value > max) { max = kv.Value; dominant = kv.Key; }
        return dominant;
    }
}