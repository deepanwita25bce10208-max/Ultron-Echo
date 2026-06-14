using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Attach this to a trigger collider placed at the safe room entrance.
/// Ultron will be blocked from entering and warped back to a patrol point.
/// </summary>
public class SafeRoomBarrier : MonoBehaviour
{
    [Tooltip("Where Ultron gets sent when he tries to enter. " +
             "Set this to a point just outside the safe room door.")]
    public Transform repelPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Only block ULTRON
        if (!other.CompareTag("Ultron")) return;

        Debug.Log("[BARRIER] Ultron blocked from safe room.");

        NavMeshAgent agent = other.GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = other.GetComponentInParent<NavMeshAgent>();

        if (agent != null && repelPoint != null)
        {
            // Warp Ultron back outside the safe room
            agent.Warp(repelPoint.position);

            // Send him to a random patrol point instead
            EnemyAI ai = other.GetComponent<EnemyAI>();
            if (ai == null) ai = other.GetComponentInParent<EnemyAI>();

            if (ai != null && ai.patrolPoints.Length > 0)
            {
                Transform dest = ai.patrolPoints[Random.Range(0, ai.patrolPoints.Length)];
                agent.SetDestination(dest.position);
            }
        }

        // Psychological taunt — Ultron knows you're hiding
        PlayerProfile.Instance.Spike("hideCount", 5f);
        if (HUDManager.Instance != null)
            HUDManager.Instance.Flash("SAFE ROOM DETECTED. PERIMETER LOGGED.");
    }
}