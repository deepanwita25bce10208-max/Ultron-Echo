using UnityEngine;
using TMPro;
using System.Collections;

public class SleepTrigger : MonoBehaviour
{
    public GameObject sleepScreen;
    public TMP_Text sleepText;
    private bool sleeping = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !sleeping)
        {
            StartCoroutine(SleepSequence());
        }
    }

    IEnumerator SleepSequence()
    {
        sleeping = true;

        sleepScreen.SetActive(true);

        sleepText.text = "DAY COMPLETE\n\nAnalyzing Neural Activity...";
        yield return new WaitForSeconds(2f);

        sleepText.text =
            $"ULTRON PROFILE UPDATING...\n\n" +
            $"{PlayerProfile.Instance.knowledgeLevel:F1}% NEURAL MAPPING COMPLETE";

        yield return new WaitForSeconds(2f);

        // advance day (single source of truth)
        DayManager.Instance.EndDay();

        yield return new WaitForSeconds(1f);

        sleepScreen.SetActive(false);
        sleeping = false;
    }
}