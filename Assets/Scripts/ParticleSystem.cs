using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public struct Particle
{
    public Transform transform;
    public Vector3 velocity;

    // constructor
    public Particle(Transform transform, Vector3 velocity)
    {
        this.transform = transform;
        this.velocity = velocity;
    }
}

public class ParticleSystem : MonoBehaviour
{       
    [SerializeField]
    private GameObject ParticlePrefab;

    [SerializeField]
    private Transform boundingBox;
    private Vector3 boundMin;
    private Vector3 boundMax;

    [SerializeField] 
    private float scaleMin = 1;
    [SerializeField] 
    private float scaleMax = 1;
    
    [SerializeField]
    private Vector3 addedForce = new Vector3(0, -0.01f, 0);

    [SerializeField]
    private int initialSpawnCount = 100;

    private List<Particle> activeParticles = new List<Particle>();   

    void Start() {
        boundMin = boundingBox.position - boundingBox.localScale/2;
        boundMax = boundingBox.position + boundingBox.localScale/2;     
        
        for (int i = 0; i < initialSpawnCount; i++) {
            SpawnParticleRandomly();
        }   
    }

    void Update() {   
        // UPDATE PARTICLES
        for (int i = 0; i < activeParticles.Count; i++) {
            Particle p = activeParticles[i];

            // apply velocity to each particles position
            p.transform.position += p.velocity * Time.deltaTime;  
        
            // apply force
            p.velocity += addedForce * Time.deltaTime;

            activeParticles[i] = p;
        }

        // DEBUGGING CONTROLS
        if (Input.GetKeyDown(KeyCode.Space)) {
            for (int i = 0; i < initialSpawnCount; i++) {
                SpawnParticleRandomly();
            }
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            for (int i = 0; i < activeParticles.Count; i++) {
                Particle p = activeParticles[i];
                Destroy(p.transform.gameObject);
            } 
            activeParticles.Clear();
        }
    }
        
    private void SpawnParticleRandomly() {
        Vector3 randomPos = new Vector3(
            Random.Range(boundMin.x, boundMax.x),
            Random.Range(boundMin.y, boundMax.y),
            Random.Range(boundMin.z, boundMax.z)
        );
        float randomScale = Random.Range(scaleMin, scaleMax);

        Vector3 randomVelocity = new Vector3(
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f),
            Random.Range(-0.1f, 0.1f)
        );

        SpawnParticle(randomPos, randomScale, randomVelocity);
    }

    // spawns a particle in the world and adds a reference to the active particles list
    private void SpawnParticle(Vector3 pos, float scale = 1, Vector3 initialVelocity = default) {
        GameObject newParticleObject = GameObject.Instantiate(ParticlePrefab, pos, Quaternion.identity, transform);
        newParticleObject.transform.localScale *= scale;

        Particle newParticle = new Particle(newParticleObject.transform, initialVelocity);
        activeParticles.Add(newParticle);
    }
}
