using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light flickerLight;

    public float minIntensity = 0.4f;
    public float maxIntensity = 1.2f;

    public float flickerSpeed = 0.05f;

    void Start()
    {
        if (flickerLight == null)
            flickerLight = GetComponent<Light>();
    }

    void Update()
    {
        flickerLight.intensity =
            Random.Range(minIntensity, maxIntensity);
    }
}