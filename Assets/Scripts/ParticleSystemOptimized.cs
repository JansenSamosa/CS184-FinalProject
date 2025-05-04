using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public struct ParticleData
{   
    public Vector3 position;
    public Vector3 scale;
    public Quaternion rotation;

    public Vector3 velocity;
    
    public bool settled;
    public float activeLifetime;
}

public class ParticleSystemOptimized : MonoBehaviour
{   
    [Header("Particle Rendering Parameters")]
    public Material particleMaterial;
    public Mesh particleMesh;
    public bool renderAsBillboardTexture = true;

    [Header("Particle Emission Parameters")]
    public Transform boundingBox;
    public int spawnCount = 100;

    private Vector3 boundMin;
    private Vector3 boundMax;
    
    [Header("Particle Attributes")]
    public float scaleMin = 1;
    public float scaleMax = 1;
    public float particleActiveLifetime = 5;
    public float maxVelocity = 1;

    [Header("External Forces")]
    public float drag = 0.01f;
    public Vector3 addedForce = new Vector3(0, -0.01f, 0);
    public WindField windField;

    [Header("Particles lists (Don't modify: for debugging purposes only)")]
    public List<ParticleData> activeParticles = new List<ParticleData>();   
    public List<ParticleData> settledParticles = new List<ParticleData>();
    
    private List<Matrix4x4> settledParticlesTransformations = new List<Matrix4x4>();

    void Start() {
        // set the particle emission bounds
        boundMin = boundingBox.position - boundingBox.localScale/2;
        boundMax = boundingBox.position + boundingBox.localScale/2;     

        // enable instancing for material
        particleMaterial.enableInstancing = true;

        // spawn particles
        for (int i = 0; i < spawnCount; i++) {
            SpawnParticleRandomly();
        }   
    }

    void Update() {   
        // transformation of each particle
        Matrix4x4[] activeParticleTransformations = new Matrix4x4[activeParticles.Count];
        
        Vector3 camPos = Camera.main.transform.position;
        // UPDATE PARTICLES
        for (int i = 0; i < activeParticles.Count; i++) {
            ParticleData p = activeParticles[i];
            
            // rotate billboard particles towards camera
            if (renderAsBillboardTexture) {
                p.rotation = Quaternion.LookRotation(p.position - camPos);;
            }
            
            // update particle life time
            p.activeLifetime -= Time.deltaTime;

            // apply gravity
            p.velocity += addedForce * Time.deltaTime;
            
            // apply wind
            if (windField) {
                Vector3 curWind = windField.SampleWind(p.position);
                p.velocity += curWind * Time.deltaTime;
            }

            // constrain velocity
            p.velocity = Vector3.ClampMagnitude(p.velocity, maxVelocity);

            // apply drag
            p.velocity -= (p.velocity * drag * Time.deltaTime);
            
            Vector3 nextPos = p.position + p.velocity * Time.deltaTime;
            if (Physics.Linecast(p.position, nextPos, out RaycastHit hit)) {
                p.position = hit.point;      // snap to contact
                p.velocity = Vector3.zero;   // zero out so they “settle”
                p.settled = true;
                settledParticles.Add(p);
                settledParticlesTransformations.Add(Matrix4x4.TRS(p.position, p.rotation, p.scale));
            }
            else {
                p.position = nextPos;
            }

            activeParticleTransformations[i] = Matrix4x4.TRS(p.position, p.rotation, p.scale);
            activeParticles[i] = p;
        }

        // render all particles
        if (activeParticles.Count > 0) {
            RenderParams rp = new RenderParams(particleMaterial);
            Graphics.RenderMeshInstanced(rp, particleMesh, 0, activeParticleTransformations);
        }

        if (settledParticles.Count > 0) {
            RenderParams rp = new RenderParams(particleMaterial);
            Graphics.RenderMeshInstanced(rp, particleMesh, 0, settledParticlesTransformations.ToArray());
        }


        activeParticles.RemoveAll(item => item.settled);
        activeParticles.RemoveAll(item => item.activeLifetime <= 0);

        // DEBUGGING CONTROLS
        if (Input.GetKeyDown(KeyCode.Space)) {
            for (int i = 0; i < spawnCount; i++) {
                SpawnParticleRandomly();
            }
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            activeParticles.Clear();
            settledParticles.Clear();
            settledParticlesTransformations.Clear();
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
        ParticleData newParticle = new ParticleData();
        newParticle.position = pos;
        newParticle.rotation = Quaternion.identity;
        newParticle.scale = new Vector3(scale, scale, scale);
        
        newParticle.velocity = initialVelocity;
        newParticle.settled = false;
        newParticle.activeLifetime = particleActiveLifetime;

        activeParticles.Add(newParticle);
    }    
}