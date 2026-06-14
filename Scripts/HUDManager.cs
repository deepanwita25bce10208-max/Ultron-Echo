using UnityEngine;
using TMPro;

/// <summary>
/// HUD Manager — SHIELD terminal aesthetic.
/// Bars now correctly scale against 100-point max values.
/// Flash messages auto-expire. State text color-codes by danger.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("Stat Bars")]
    public TMP_Text ventText;
    public TMP_Text fearText;
    public TMP_Text lootText;
    public TMP_Text knowledgeText;
    public TMP_Text hideText;

    [Header("Status")]
    public TMP_Text dayText;
    public TMP_Text stateText;

    [Header("Flash Alert")]
    public TMP_Text flashText;
    private float   flashTimer = 0f;

    [Header("Knowledge Bar Color")]
    public Color safeColor      = Color.green;
    public Color warningColor   = Color.yellow;
    public Color dangerColor    = Color.red;

    void Awake() { Instance = this; }

    void Start()
    {
        if (flashText != null) flashText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (PlayerProfile.Instance == null || DayManager.Instance == null) return;

        var   d    = PlayerProfile.Instance.data;
        float know = PlayerProfile.Instance.knowledgeLevel;

        // ── Stat bars ────────────────────────────────────────────────────
        if (ventText      != null) ventText.text      = Bar("VENT AFFINITY",    d["ventCount"]);
        if (fearText      != null) fearText.text      = Bar("FEAR RESPONSE",    d["fearScore"]);
        if (lootText      != null) lootText.text      = Bar("LOOT SEEKING",     d["lootAffinity"]);
        if (hideText      != null) hideText.text      = Bar("HIDE TENDENCY",    d["hideCount"]);

        // Knowledge bar changes color as Ultron learns more
        if (knowledgeText != null)
        {
            knowledgeText.text  = Bar("ULTRON KNOWLEDGE", know);
            knowledgeText.color = know < 33f ? safeColor
                                : know < 66f ? warningColor
                                :              dangerColor;
        }

        // ── Status lines ──────────────────────────────────────────────────
        if (dayText   != null)
        {
            int inUniverseDay = DayManager.Instance.currentDay * 5;
            dayText.text = $"DAY {inUniverseDay:D2} / 50";
        }

        if (stateText != null)
        {
            AIState s = AIStateManager.Instance.currentState;
            stateText.text  = $"TACTIC: {s.ToString().ToUpper()}";
            stateText.color = s == AIState.Dormant      ? Color.grey
                            : s == AIState.Aggressive   ? dangerColor
                            : s == AIState.Predictive   ? Color.cyan
                            : s == AIState.Psychological? Color.magenta
                            :                             warningColor;
        }

        // ── Flash timer ───────────────────────────────────────────────────
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashText != null) flashText.gameObject.SetActive(true);
        }
        else
        {
            if (flashText != null) flashText.gameObject.SetActive(false);
        }
    }

    // ─── Bar renderer ──────────────────────────────────────────────────────
    // value is 0–100. Bar fills 0–10 blocks proportionally.
    string Bar(string label, float value)
    {
        int filled = Mathf.RoundToInt(Mathf.Clamp01(value / 100f) * 10f);
        string bar = "";
        for (int i = 0; i < 10; i++)
            bar += i < filled ? "█" : "░";
        return $"{label}\n[{bar}] {Mathf.RoundToInt(value)}%";
    }

    // ─── Flash alert ───────────────────────────────────────────────────────
    public void Flash(string message)
    {
        if (flashText == null) return;
        flashText.text = message;
        flashTimer = 3f;
        flashText.gameObject.SetActive(true);
    }
}