using UnityEngine;

/// <summary>
/// Attach to loot objects.
/// Picking up loot spikes lootAffinity.
/// When lootTrapFired = true, Ultron uses this to ambush via Psychology state.
///
/// Optional: set isTrap = true on ONE loot object — when player picks it up,
/// Ultron teleports nearby and a jumpscare triggers.
/// </summary>
public class LootPickup : MonoBehaviour
{
    [Header("Trap Settings")]
    public bool isTrap = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerProfile.Instance.Spike("lootAffinity", 10f);
        PlayerProfile.Instance.IncrementRaw("lootPickups");

        if (HUDManager.Instance != null)
            HUDManager.Instance.Flash("ITEM ACQUIRED");

        if (isTrap && !PlayerProfile.Instance.lootTrapFired)
        {
            PlayerProfile.Instance.lootTrapFired = true;
            TriggerLootTrap(other.transform);
        }

        Destroy(gameObject);
    }

    void TriggerLootTrap(Transform playerTransform)
    {
        // Spawn Ultron very close to player
        var ai = FindObjectOfType<EnemyAI>();
        if (ai == null) return;

        Vector3 spawnPos = playerTransform.position + playerTransform.forward * 2f;
        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(spawnPos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            ai.GetComponent<UnityEngine.AI.NavMeshAgent>().Warp(hit.position);

        if (HUDManager.Instance != null)
            HUDManager.Instance.Flash("LOOT BIAS CONFIRMED — AMBUSH PROTOCOL ACTIVE");

        Debug.Log("[ECHO] Loot trap triggered.");
    }
}