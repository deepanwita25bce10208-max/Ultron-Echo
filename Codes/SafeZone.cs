using UnityEngine;

// Attach this to the BED object.
// Player must LEAVE the bed trigger and return to increment day again.
public class SafeZone : MonoBehaviour
{
    private bool playerInside  = false;
    private bool dayTriggered  = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        if (!dayTriggered)
        {
            dayTriggered = true;
            Debug.Log("[BED] Player entered bed — ending day");
            DayManager.Instance.EndDay();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside  = false;
        dayTriggered  = false; // reset — player must come back to trigger again
        Debug.Log("[BED] Player left bed — ready for next day trigger");
    }
}