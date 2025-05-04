using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LightRayTracer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform lightSource;
    [SerializeField] private ParticleSystem[] particleSystems;  // Custom ParticleSystem with activeParticles

    [Header("Ray Settings")]
    [SerializeField] private int rayCount = 32;
    [SerializeField] private int segmentsPerRay = 10;
    [SerializeField] private float rayLength = 10f;
    [SerializeField] private float rayWidth = 0.05f;
    [SerializeField] private Color rayColor = new Color(1f, 1f, 0.8f, 0.5f);
    [SerializeField] private float rayIntensity = 1.5f;
    [SerializeField] private float attenuationFactor = 0.9f;
    [SerializeField] private float scatteringStrength = 0.3f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 0.5f;
    [SerializeField] private bool visualizeInteractions = true;
    [SerializeField] private GameObject interactionPrefab;
    [SerializeField] private float interactionScale = 0.2f;

    // Mesh and ray data
    private Mesh rayMesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;
    private List<RaySegment> rays = new List<RaySegment>();

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        rayMesh = new Mesh { name = "VolumetricRayMesh" };
        meshFilter.mesh = rayMesh;
        propertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        // Initialize ray material properties
        propertyBlock.SetColor("_Color", rayColor);
        propertyBlock.SetFloat("_Intensity", rayIntensity);
        propertyBlock.SetFloat("_Attenuation", attenuationFactor);
        propertyBlock.SetFloat("_Scattering", scatteringStrength);
        meshRenderer.SetPropertyBlock(propertyBlock);

        GenerateRays();
    }

    private void Update()
    {
        foreach (var ray in rays)
        {
            ray.Reset();
            ray.origin = lightSource.position;
            TraceRay(ray);
        }
        RebuildMesh();
    }

    private void GenerateRays()
    {
        rays.Clear();
        for (int i = 0; i < rayCount; i++)
        {
            Vector3 dir = Random.onUnitSphere;
            dir.y = -Mathf.Abs(dir.y);
            rays.Add(new RaySegment(lightSource.position, dir.normalized, rayIntensity));
        }
    }

    private void TraceRay(RaySegment ray)
    {
        Vector3 pos = ray.origin;
        Vector3 dir = ray.direction;
        float intensity = ray.intensity;
        float segmentLength = rayLength / segmentsPerRay;
        Vector3 camPos = Camera.main.transform.position;

        // Initial quad corners
        Vector3 prevOffset = Vector3.Cross(dir, camPos - pos).normalized * rayWidth;
        Vector3 prevLeft = pos + prevOffset;
        Vector3 prevRight = pos - prevOffset;

        for (int i = 0; i <= segmentsPerRay; i++)
        {
            Vector3 nextPos;
            RaycastHit wallHit;
            // Stop at walls
            if (Physics.Raycast(pos, dir, out wallHit, segmentLength))
            {
                nextPos = wallHit.point;
            }
            else
            {
                nextPos = pos + dir * segmentLength;
            }

            bool interactiveHit = false;
            // Check dust interactions
            foreach (var ps in particleSystems)
            {
                foreach (var part in ps.activeParticles)
                {
                    Vector3 worldPos = part.transform.position;
                    float size = part.transform.localScale.x;
                    float radius = interactionRadius + size * 0.5f;
                    Vector3 closest = ClosestPointOnSegment(pos, nextPos, worldPos);
                    if (Vector3.Distance(closest, worldPos) <= radius)
                    {
                        interactiveHit = true;
                        if (visualizeInteractions && interactionPrefab)
                        {
                            var go = Instantiate(interactionPrefab, closest, Quaternion.identity);
                            go.transform.localScale = Vector3.one * interactionScale;
                            Destroy(go, 1f);
                        }
                        Vector3 toPart = (worldPos - closest).normalized;
                        dir = Vector3.Lerp(dir, toPart, scatteringStrength).normalized;
                        intensity *= attenuationFactor;
                        break;
                    }
                }
                if (interactiveHit) break;
            }

            // Build segment quad
            Vector3 offset = Vector3.Cross(dir, camPos - pos).normalized * rayWidth;
            Vector3 left = nextPos + offset;
            Vector3 right = nextPos - offset;
            int baseIdx = ray.vertices.Count;
            ray.vertices.Add(prevLeft);
            ray.vertices.Add(prevRight);
            ray.vertices.Add(left);
            ray.vertices.Add(right);

            Vector3 normal = (camPos - pos).normalized;
            for (int n = 0; n < 4; n++) ray.normals.Add(normal);

            Color c = rayColor;
            c.a *= intensity;
            for (int n = 0; n < 4; n++) ray.colors.Add(c);

            float v0 = (float)i / segmentsPerRay;
            float v1 = v0 + 1f / segmentsPerRay;
            ray.uvs.Add(new Vector2(0, v0));
            ray.uvs.Add(new Vector2(1, v0));
            ray.uvs.Add(new Vector2(0, v1));
            ray.uvs.Add(new Vector2(1, v1));

            ray.indices.Add(baseIdx);
            ray.indices.Add(baseIdx + 1);
            ray.indices.Add(baseIdx + 2);
            ray.indices.Add(baseIdx + 1);
            ray.indices.Add(baseIdx + 3);
            ray.indices.Add(baseIdx + 2);

            prevLeft = left;
            prevRight = right;
            pos = nextPos;

            // Stop on wall hit or intensity too low or after interaction
            if (wallHit.collider != null || intensity < 0.01f || interactiveHit)
                break;
        }
    }

    private void RebuildMesh()
    {
        rayMesh.Clear();
        var verts = new List<Vector3>();
        var norms = new List<Vector3>();
        var cols = new List<Color>();
        var uvs = new List<Vector2>();
        var idxs = new List<int>();
        int offset = 0;
        foreach (var ray in rays)
        {
            verts.AddRange(ray.vertices);
            norms.AddRange(ray.normals);
            cols.AddRange(ray.colors);
            uvs.AddRange(ray.uvs);
            foreach (var i in ray.indices)
                idxs.Add(i + offset);
            offset += ray.vertices.Count;
        }
        rayMesh.SetVertices(verts);
        rayMesh.SetNormals(norms);
        rayMesh.SetColors(cols);
        rayMesh.SetUVs(0, uvs);
        rayMesh.SetTriangles(idxs, 0);
    }

    private Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        return a + ab * Mathf.Clamp01(t);
    }

    [System.Serializable]
    private class RaySegment
    {
        public Vector3 origin, direction;
        public float intensity;
        public List<Vector3> vertices = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();
        public List<Color> colors = new List<Color>();
        public List<Vector2> uvs = new List<Vector2>();
        public List<int> indices = new List<int>();

        public RaySegment(Vector3 o, Vector3 d, float i)
        {
            origin = o;
            direction = d;
            intensity = i;
        }
        public void Reset()
        {
            vertices.Clear();
            normals.Clear();
            colors.Clear();
            uvs.Clear();
            indices.Clear();
        }
    }
}
