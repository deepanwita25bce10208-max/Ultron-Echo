using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;

    [Header("Player Reference")]
    public Transform player;

    [Header("Patrol Points")]
    public Transform[] patrolPoints;

    [Header("Detection Ranges")]
    public float chaseRange      = 20f;
    public float attackRange     = 2f;
    public float losePlayerRange = 35f;

    [Header("Speed by State")]
    public float patrolSpeed = 2f;
    public float chaseSpeed  = 5f;
    public float stalkSpeed  = 1.5f;

    [Header("Jumpscare")]
    public GameObject jumpscarePanel;
    public AudioClip  jumpscareSFX;
    public AudioClip  stingerSFX;
    private bool      jumpscareOnCooldown = false;

    [Header("Audio")]
    public AudioClip ambienceClip;
    public AudioClip whisperClip;
    public AudioClip fakeRadioClip;

    private NavMeshAgent agent;
    private AudioSource  audioSource;
    private int          patrolIndex   = 0;
    private bool         isActive      = false;
    private bool         isChasing     = false;
    private float        psychTimer    = 0f;
    private float        psychInterval = 20f;
    private AIState      myState       = AIState.Dormant;

    private string[] fakeMessages = new string[]
    {
        "SHIELD EXTRACTION: PROCEED TO SECTOR 7 — NORTH EXIT",
        "AGENT ROSS: MAYA, WE LOST YOUR SIGNAL. GO TO LEVEL B2.",
        "EVACUATION ROUTE: USE VENTILATION SHAFT C — IMMEDIATELY",
        "SHIELD COMMS: STAY WHERE YOU ARE. HELP IS COMING.",
        "WARNING: DO NOT TRUST YOUR INSTRUMENTS. I AM ALREADY THERE.",
    };

    void Awake()
    {
        Instance    = this;
        agent       = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        foreach (var r in GetComponentsInChildren<MeshRenderer>())
            r.enabled = false;

        AIStateManager.OnStateChanged += OnStateChanged;

        if (ambienceClip != null)
        {
            audioSource.clip   = ambienceClip;
            audioSource.loop   = true;
            audioSource.volume = 0.2f;
            audioSource.Play();
        }

        StartCoroutine(WaitForActivation());
    }

    void OnDestroy()
    {
        AIStateManager.OnStateChanged -= OnStateChanged;
    }

    // ── Activation ────────────────────────────────────────────────────────

    IEnumerator WaitForActivation()
    {
        agent.isStopped = true;

        while (DayManager.Instance == null || DayManager.Instance.currentDay < 5)
            yield return new WaitForSeconds(0.5f);

        foreach (var r in GetComponentsInChildren<MeshRenderer>())
            r.enabled = true;

        isActive        = true;
        agent.isStopped = false;
        myState         = AIStateManager.Instance.currentState;

        if (HUDManager.Instance != null)
            HUDManager.Instance.Flash("WARNING: NEURAL FRAGMENT ACTIVE — DAY 5");

        Debug.Log("[ECHO] Ultron activating on day 5.");
        StartCoroutine(PatrolLoop());
    }

    // ── Update ────────────────────────────────────────────────────────────

    void Update()
    {
        if (!isActive || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (!jumpscareOnCooldown && dist <= attackRange)
        {
            StartCoroutine(DoJumpscare());
            return;
        }

        if (myState == AIState.Aggressive)
        {
            agent.speed = chaseSpeed + UltronBrain.Instance.aggressionLevel * 0.4f;

            if (dist < chaseRange)
            {
                isChasing = true;
                agent.SetDestination(player.position);
                PlayerProfile.Instance.Spike("fearScore", 0.02f);
            }
            else if (isChasing && dist > losePlayerRange)
            {
                isChasing = false;
            }
        }
        else if (myState == AIState.Psychological)
        {
            agent.speed = stalkSpeed;

            if (dist > 10f && dist < chaseRange)
                agent.SetDestination(player.position);
            else if (dist <= 6f)
            {
                Vector3 away = (transform.position - player.position).normalized;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position + away * 4f, out hit, 5f, NavMesh.AllAreas))
                    agent.SetDestination(hit.position);
            }

            psychTimer += Time.deltaTime;
            if (psychTimer >= psychInterval)
            {
                psychTimer = 0f;
                StartCoroutine(DoPsychEvent());
            }
        }
        else if (myState == AIState.Predictive)
        {
            agent.speed = patrolSpeed + 1f;
            agent.SetDestination(PredictPlayerDestination());
        }
    }

    // ── Patrol ────────────────────────────────────────────────────────────

    IEnumerator PatrolLoop()
    {
        while (true)
        {
            if (!isChasing && myState != AIState.Aggressive
                           && myState != AIState.Psychological
                           && patrolPoints.Length > 0)
            {
                agent.speed = patrolSpeed;
                agent.SetDestination(patrolPoints[patrolIndex].position);

                yield return new WaitUntil(() =>
                    !agent.pathPending && agent.remainingDistance < 1.5f);

                patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                yield return new WaitForSeconds(Random.Range(1f, 3f));
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    // ── Psychological Events ──────────────────────────────────────────────

    IEnumerator DoPsychEvent()
    {
        int roll = Random.Range(0, 5);

        switch (roll)
        {
            case 0:
                string msg = fakeMessages[Random.Range(0, fakeMessages.Length)];
                if (HUDManager.Instance != null) HUDManager.Instance.Flash(msg);
                if (fakeRadioClip != null) audioSource.PlayOneShot(fakeRadioClip, 0.7f);
                break;

            case 1:
                if (whisperClip != null) audioSource.PlayOneShot(whisperClip, 0.5f);
                if (HUDManager.Instance != null) HUDManager.Instance.Flash("I CAN SEE YOU.");
                break;

            case 2:
                Vector3 inFront = player.position + player.forward * 3f;
                NavMeshHit h;
                if (NavMesh.SamplePosition(inFront, out h, 5f, NavMesh.AllAreas))
                {
                    agent.Warp(h.position);
                    yield return new WaitForSeconds(1.2f);
                    if (patrolPoints.Length > 0)
                        agent.Warp(patrolPoints[Random.Range(0, patrolPoints.Length)].position);
                }
                break;

            case 3:
                float know = PlayerProfile.Instance.knowledgeLevel;
                if (HUDManager.Instance != null)
                    HUDManager.Instance.Flash(
                        "NEURAL SYNC: " + Mathf.RoundToInt(know) + "% COMPLETE. I KNOW YOU.");
                break;

            case 4:
                if (stingerSFX != null) audioSource.PlayOneShot(stingerSFX, 0.8f);
                int inUniverse = DayManager.Instance.currentDay * 5;
                if (HUDManager.Instance != null)
                    HUDManager.Instance.Flash(
                        "DAY " + inUniverse + ". YOU HAVE " + (50 - inUniverse) + " DAYS LEFT.");
                break;
        }

        yield return null;
    }

    // ── Jumpscare ─────────────────────────────────────────────────────────

    IEnumerator DoJumpscare()
    {
        jumpscareOnCooldown = true;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            if (jumpscarePanel != null) jumpscarePanel.SetActive(true);
            if (jumpscareSFX   != null) audioSource.PlayOneShot(jumpscareSFX, 1f);
            PlayerProfile.Instance.Spike("fearScore", 20f);
            if (HUDManager.Instance != null)
                HUDManager.Instance.Flash("PROXIMITY BREACH — NEURAL SYNC +20%");

            yield return new WaitForSeconds(0.8f);
            if (jumpscarePanel != null) jumpscarePanel.SetActive(false);
        }
        else
        {
            if (stingerSFX != null) audioSource.PlayOneShot(stingerSFX, 0.6f);
            PlayerProfile.Instance.Spike("fearScore", 5f);
            if (HUDManager.Instance != null) HUDManager.Instance.Flash("I SEE YOU.");
        }

        if (patrolPoints.Length > 0)
            agent.Warp(patrolPoints[Random.Range(0, patrolPoints.Length)].position);

        yield return new WaitForSeconds(10f);
        jumpscareOnCooldown = false;
    }

    // ── Milestone Jumpscares (called by DayManager) ───────────────────────

    public void OnNewDay(int day)
    {
        if (!isActive) return;

        if (day == 5)
            StartCoroutine(MilestoneJumpscare("NEURAL FRAGMENT ONLINE. I AM LEARNING."));
        else if (day == 7)
            StartCoroutine(MilestoneJumpscare("YOU HID TODAY. I MEMORIZED THE LOCATIONS."));
        else if (day == 10)
            StartCoroutine(MilestoneJumpscare("DAY 50. EXTRACTION FAILED. I KNOW EVERYTHING."));
    }

    IEnumerator MilestoneJumpscare(string message)
    {
        yield return new WaitForSeconds(1.5f);

        if (jumpscarePanel != null) jumpscarePanel.SetActive(true);
        if (jumpscareSFX   != null) audioSource.PlayOneShot(jumpscareSFX, 0.9f);
        if (HUDManager.Instance != null) HUDManager.Instance.Flash(message);

        yield return new WaitForSeconds(1f);
        if (jumpscarePanel != null) jumpscarePanel.SetActive(false);
    }

    // ── Predictive Destination ────────────────────────────────────────────

    Vector3 PredictPlayerDestination()
    {
        if (patrolPoints.Length == 0) return player.position;

        var d = PlayerProfile.Instance.data;
        Vector3 biasDir = d["leftBias"] > d["rightBias"] ? -player.right : player.right;

        Transform best    = patrolPoints[0];
        float     bestDot = -1f;
        foreach (var p in patrolPoints)
        {
            Vector3 toPoint = (p.position - player.position).normalized;
            float   dot     = Vector3.Dot(toPoint, biasDir);
            if (dot > bestDot) { bestDot = dot; best = p; }
        }
        return best.position;
    }

    // ── State Change ──────────────────────────────────────────────────────

    void OnStateChanged(AIState newState)
    {
        myState   = newState;
        isChasing = false;

        string msg = "";
        if      (newState == AIState.Aggressive)    msg = "THREAT LEVEL: CRITICAL — RUN";
        else if (newState == AIState.Psychological) msg = "NEURAL PATTERN: PSYCHOLOGICAL MODE ACTIVE";
        else if (newState == AIState.Predictive)    msg = "PREDICTION ENGINE: ONLINE";
        else if (newState == AIState.Environmental) msg = "ENVIRONMENT CONTROL: ACTIVE";

        if (msg != "" && HUDManager.Instance != null)
            HUDManager.Instance.Flash(msg);

        Debug.Log("[ECHO] EnemyAI -> " + newState);
    }
}