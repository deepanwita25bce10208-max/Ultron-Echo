using UnityEngine;

/// <summary>
/// Tracks player movement intensity and stillness each frame.
/// Feeds Spike() calls into PlayerProfile for neuromorphic learning.
/// Also tracks direction bias (left/right) from movement relative to camera forward.
/// </summary>
public class PlayerTracker : MonoBehaviour
{
    public static PlayerTracker instance;

    [HideInInspector] public float movementIntensity;
    [HideInInspector] public float stillTime;

    private Vector3 lastPosition;
    private Camera  cam;

    void Awake()
    {
        instance = this;
        cam = Camera.main;
    }

    void Start() { lastPosition = transform.position; }

    void Update()
    {
        float dist = Vector3.Distance(transform.position, lastPosition);

        // Smooth movement intensity
        movementIntensity = Mathf.Lerp(movementIntensity, dist / Time.deltaTime, 0.1f);

        // Still time accumulator
        if (dist < 0.01f)
            stillTime += Time.deltaTime;
        else
            stillTime = 0f;

        // ── Neuromorphic spikes ───────────────────────────────────────────
        if (PlayerProfile.Instance == null) { lastPosition = transform.position; return; }

        // Running spike
        if (movementIntensity > 3f)
            PlayerProfile.Instance.Spike("runCount", 0.1f * Time.deltaTime);

        // Hiding spike (very still for 2+ seconds)
        if (stillTime > 2f)
        {
            PlayerProfile.Instance.Spike("hideCount", 0.1f * Time.deltaTime);

            // Log unique hide spot (throttled)
            if (Mathf.FloorToInt(stillTime) % 5 == 0 && Mathf.FloorToInt(stillTime) > 0)
                PlayerProfile.Instance.IncrementRaw("hideSpots");
        }

        // Direction bias: project movement onto local right axis
        if (dist > 0.05f && cam != null)
        {
            Vector3 moveDir = (transform.position - lastPosition).normalized;
            Vector3 camRight = cam.transform.right;
            float   dot      = Vector3.Dot(moveDir, camRight);

            if (dot > 0.4f)
                PlayerProfile.Instance.Spike("rightBias", 0.05f * Time.deltaTime);
            else if (dot < -0.4f)
                PlayerProfile.Instance.Spike("leftBias", 0.05f * Time.deltaTime);
        }

        lastPosition = transform.position;
    }
}