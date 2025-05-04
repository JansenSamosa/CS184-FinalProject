using UnityEngine;

public class WindField : MonoBehaviour
{
    public float noiseScale   = 0.1f;                 // spatial frequency
    public float noiseSpeed   = 0.2f;                 // temporal frequency
    public float strength     = 1.0f;                 // noise amplitude

    Vector3 mean;
    int numSampled = 0;
    void Start()
    {
        mean = Vector3.zero;
        Random.InitState(76543);
    }

    public Vector3 SampleWind(Vector3 worldPos)
{
    float t  = Time.time * noiseSpeed;
    float ns = noiseScale;

    // sample three orthogonal Perlin pairs and map [0..1]→[-1..1]
    float x = (Mathf.PerlinNoise(worldPos.y*ns + t, worldPos.z*ns + t) - 0.5f) * 2f;
    float y = (Mathf.PerlinNoise(worldPos.z*ns + t, worldPos.x*ns + t) - 0.5f) * 2f;
    float z = (Mathf.PerlinNoise(worldPos.x*ns + t, worldPos.y*ns + t) - 0.5f) * 2f;

    return new Vector3(x, y, z) * strength;
}
}
