using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(ParticleSystemRenderer))]
public class GlowFlicker : MonoBehaviour
{
    public float baseIntensity = 1.0f;   // overall brightness
    public float flickerAmount = 0.5f;   // 0.5 = ±50% brightness
    public float flickerSpeed = 8.0f;    // how fast it jitters

    private ParticleSystemRenderer psr;
    private Color baseColor;
    private float noiseOffset;

    void Awake()
    {
        psr = GetComponent<ParticleSystemRenderer>();
        noiseOffset = Random.value * 1000f;

        // Try to grab the main color from common properties
        if (psr.material.HasProperty("_TintColor"))
            baseColor = psr.material.GetColor("_TintColor");
        else if (psr.material.HasProperty("_Color"))
            baseColor = psr.material.GetColor("_Color");
        else
            baseColor = Color.white;
    }

    void Update()
    {
        float t = Time.time * flickerSpeed + noiseOffset;

        // Perlin 0..1 → -1..1
        float noise = Mathf.PerlinNoise(t, 0f) * 2f - 1f;

        // 1 ± flickerAmount
        float intensity = baseIntensity * (1f + noise * flickerAmount);

        Color c = baseColor * intensity;
        c.a = baseColor.a;

        if (psr.material.HasProperty("_TintColor"))
            psr.material.SetColor("_TintColor", c);
        if (psr.material.HasProperty("_Color"))
            psr.material.SetColor("_Color", c);
    }
}
