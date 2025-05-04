using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct Particle
{
    public Transform transform;
    public Vector3 velocity;
    
    public bool settled;

    // constructor
    public Particle(Transform transform, Vector3 velocity)
    {
        this.transform = transform;
        this.velocity = velocity;
        this.settled = false;
    }
}

public class ParticleSystem : MonoBehaviour
{           
    [SerializeField] GameObject ParticlePrefab;
    [SerializeField] Transform boundingBox;
    [SerializeField] float scaleMin = 1;
    [SerializeField] float scaleMax = 1;

    [SerializeField] float maxVelocity = 1;

    [SerializeField] Vector3 addedForce = new Vector3(0, -0.01f, 0);
    [SerializeField] float drag = 0.01f;
    
    [SerializeField] int initialSpawnCount = 100;

    [HideInInspector] public List<Particle> activeParticles = new List<Particle>();   
    private Vector3 boundMin;
    private Vector3 boundMax;

    [SerializeField] WindField windField;

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
            if (p.settled) continue;
            
            // apply gravity
            p.velocity += addedForce * Time.deltaTime;
            
            // apply wind
            if (windField) {
                Vector3 curWind = windField.SampleWind(p.transform.position);
                p.velocity += curWind * Time.deltaTime;
            }

            p.velocity = Vector3.ClampMagnitude(p.velocity, maxVelocity);

            // apply drag
            p.velocity -= (p.velocity * drag * Time.deltaTime);
            
            Vector3 nextPos = p.transform.position + p.velocity * Time.deltaTime;
            if (Physics.Linecast(p.transform.position, nextPos, out RaycastHit hit)) {
                p.transform.position = hit.point;      // snap to contact
                p.velocity = Vector3.zero;   // zero out so they “settle”
                p.settled = true;
            }
            else {
                p.transform.position = nextPos;
            }


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
            UnityEngine.Random.Range(boundMin.x, boundMax.x),
            UnityEngine.Random.Range(boundMin.y, boundMax.y),
            UnityEngine.Random.Range(boundMin.z, boundMax.z)
        );
        float randomScale = UnityEngine.Random.Range(scaleMin, scaleMax);

        Vector3 randomVelocity = new Vector3(
            UnityEngine.Random.Range(-0.1f, 0.1f),
            UnityEngine.Random.Range(-0.1f, 0.1f),
            UnityEngine.Random.Range(-0.1f, 0.1f)
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