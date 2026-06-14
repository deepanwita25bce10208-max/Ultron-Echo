using UnityEngine;

public class DebugSkip : MonoBehaviour
{
    public void SkipToDay9()
    {
        DayManager.Instance.DebugSkipToDay(9);
        PlayerProfile.Instance.Spike("ventCount", 35f);
        PlayerProfile.Instance.Spike("hideCount", 60f);
        PlayerProfile.Instance.Spike("leftBias", 45f);
        PlayerProfile.Instance.Spike("fearScore", 25f);
        PlayerProfile.Instance.Spike("lootAffinity", 55f);
    }
}