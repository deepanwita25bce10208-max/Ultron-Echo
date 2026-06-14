using UnityEngine;
using TMPro;
using System.Collections;

public class DayManager : MonoBehaviour
{
    public static DayManager Instance;

    [Header("Day Settings")]
    public int currentDay = 1;
    public int totalDays  = 10;

    [Header("UI")]
    public TMP_Text   dayText;
    public GameObject sleepScreen;

    [Header("Ending")]
    public GameObject endingScreen;
    public TMP_Text   endingText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // Auto-find DayCounterText if not wired
        if (dayText == null)
        {
            var all = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
            foreach (var t in all)
                if (t.name == "DayCounterText") { dayText = t; break; }
        }
        UpdateDayUI();
    }

    public void EndDay()
    {
        StartCoroutine(SleepCycle());
    }

    IEnumerator SleepCycle()
    {
        if (sleepScreen != null) sleepScreen.SetActive(true);

        PlayerProfile.Instance.DecayAll();
        AIStateManager.Instance.RecalcState();
        UltronBrain.Instance.OnDayEnd(currentDay);

        // Manual timer — survives inactive GameObjects unlike WaitForSeconds
        float timer = 0f;
        while (timer < 3f)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        currentDay++;

        // Trigger milestone jumpscare on new day
        var enemy = FindFirstObjectByType<EnemyAI>();
        if (enemy != null) enemy.OnNewDay(currentDay);

        if (currentDay > totalDays)
        {
            TriggerEnding();
        }
        else
        {
            if (sleepScreen != null) sleepScreen.SetActive(false);
            UpdateDayUI();
            AIStateManager.Instance.BroadcastStateChange();
        }
    }

    void TriggerEnding()
    {
        if (sleepScreen  != null) sleepScreen.SetActive(false);
        if (endingScreen != null)
        {
            endingScreen.SetActive(true);
            if (endingText != null)
                endingText.text = BuildEndingMonologue();
        }

        var ai = FindFirstObjectByType<EnemyAI>();
        if (ai != null) ai.enabled = false;
    }

    string BuildEndingMonologue()
    {
        var r = PlayerProfile.Instance.rawCounts;
        var d = PlayerProfile.Instance.data;

        int totalTurns = r["leftTurns"] + r["rightTurns"];
        int leftPct    = totalTurns > 0
            ? Mathf.RoundToInt((float)r["leftTurns"] / totalTurns * 100f)
            : 50;

        string dominantTrait = PlayerProfile.Instance.GetDominantTrait();
        string traitLine;
        if      (dominantTrait == "hideCount")    traitLine = "You are a hider. Reactive. Passive.";
        else if (dominantTrait == "runCount")     traitLine = "You are a runner. Impulsive. Predictable.";
        else if (dominantTrait == "lootAffinity") traitLine = "You are a scavenger. Greedy. Exploitable.";
        else if (dominantTrait == "ventCount")    traitLine = "You love the vents. Every duct. Every crawlspace.";
        else                                      traitLine = "You are... complex. But I figured you out.";

        return
            "NEURAL SYNC COMPLETE.\n\n" +
            "Day One, I knew nothing.\n\n" +
            "You used the ventilation system " + r["ventUses"] + " time(s).\n" +
            "You hid in " + r["hideSpots"] + " location(s). I memorized every one.\n" +
            "You turned left " + leftPct + "% of the time under pressure.\n" +
            "You collected " + r["lootPickups"] + " item(s) — even knowing it might be a trap.\n\n" +
            traitLine + "\n\n" +
            "Fear response weight: " + Mathf.RoundToInt(d["fearScore"]) + "%\n" +
            "Prediction confidence: " + Mathf.RoundToInt(PlayerProfile.Instance.knowledgeLevel) + "%\n\n" +
            "\"Day Fifty. I know you better than SHIELD ever did.\"\n\n" +
            "— ULTRON ECHO v1.0";
    }

    void UpdateDayUI()
    {
        Debug.Log("[DAY] UpdateDayUI called — currentDay: " + currentDay + " | dayText null: " + (dayText == null));
        if (dayText == null) return;
        int inUniverseDay = currentDay * 5;
        int shieldEta     = 50 - inUniverseDay;
        dayText.text = "DAY " + inUniverseDay.ToString("D2") + " / 50\nSHIELD ETA: " + shieldEta + " DAYS";
    }

    public void DebugSkipToDay(int day)
    {
        currentDay = day;
        UpdateDayUI();
        AIStateManager.Instance.RecalcState();
    }
}