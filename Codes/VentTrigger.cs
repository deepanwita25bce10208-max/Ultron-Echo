using UnityEngine;

/// <summary>
/// Place this on EVERY vent entrance/exit collider.
/// Tag the GameObject with "Vent" so EnemyAI can find them.
///
/// When player enters, spikes ventCount and increments raw counter
/// for Day 50 dialogue: "You used the ventilation system 7 times."
/// </summary>
public class VentTrigger : MonoBehaviour
{
    [Tooltip("Set a unique ID per vent so you can track distinct vents if needed.")]
    public string ventID = "vent_01";

    private float cooldown    = 0f;
    private float cooldownMax = 5f; // Prevent double-counting same vent

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (cooldown > 0f) return;

        cooldown = cooldownMax;

        PlayerProfile.Instance.Spike("ventCount", 8f);         // big spike — deliberate behavior
        PlayerProfile.Instance.IncrementRaw("ventUses");

        Debug.Log($"[ECHO] Vent used: {ventID}. Total: {PlayerProfile.Instance.rawCounts["ventUses"]}");

        // HUD flash
        if (HUDManager.Instance != null)
            HUDManager.Instance.Flash("NEURAL SYNC: VENT PATTERN LOGGED");
    }
}