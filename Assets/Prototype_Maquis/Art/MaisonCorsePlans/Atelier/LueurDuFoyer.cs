using UnityEngine;

namespace CorseVR
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public sealed class LueurDuFoyer : MonoBehaviour
    {
        Light source;
        float intensite;
        void OnEnable()
        {
            source = GetComponent<Light>();
            intensite = source.intensity;
        }
        void Update()
        {
            float t = Time.time;
            float variation = 0.045f * Mathf.Sin(3.1f * t) + 0.026f * Mathf.Sin(8.2f * t + 0.7f);
            source.intensity = intensite * (1f + variation);
        }
        void OnDisable()
        {
            if (source) source.intensity = intensite;
        }
    }
}
