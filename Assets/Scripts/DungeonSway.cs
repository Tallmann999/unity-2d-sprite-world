using UnityEngine;

/// <summary>Subtle pendulum motion around a top pivot, with optional lantern light flicker.</summary>
public sealed class DungeonSway : MonoBehaviour
{
    [Range(0f, 8f)] public float swingDegrees = 1.6f;
    [Min(0f)] public float swingSpeed = 0.75f;
    public float phase;
    public Light lanternLight;
    [Range(0f, 0.6f)] public float flickerAmount = 0.12f;
    [Min(0f)] public float flickerSpeed = 5f;

    private Quaternion restingRotation;
    private float restingIntensity;
    private float offset;

    private void OnEnable()
    {
        restingRotation = transform.localRotation;
        if (lanternLight != null) restingIntensity = lanternLight.intensity;
        offset = phase + Vector3.Dot(transform.position, new Vector3(0.37f, 0.61f, 1.13f));
    }

    private void Update()
    {
        float t = Time.time;
        float swing = Mathf.Sin(t * swingSpeed * Mathf.PI * 2f + offset) * swingDegrees;
        transform.localRotation = restingRotation * Quaternion.Euler(0f, 0f, swing);
        if (lanternLight != null)
        {
            float flame = (Mathf.PerlinNoise(offset + 10f, t * flickerSpeed) - 0.5f) * 2f;
            lanternLight.intensity = Mathf.Max(0f, restingIntensity * (1f + flame * flickerAmount));
        }
    }

    private void OnDisable()
    {
        transform.localRotation = restingRotation;
        if (lanternLight != null) lanternLight.intensity = restingIntensity;
    }
}
