using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleCounter : MonoBehaviour
{
    [SerializeField] List<ParticleSystemOptimized> particleSystems;
    private Text countText;

    void Start()
    {
        countText = GetComponent<Text>();   
    }

    void Update()
    {
        int count = 0;
        for (int i = 0; i < particleSystems.Count; i++) {   
            count += particleSystems[i].activeParticles.Count + particleSystems[i].settledParticles.Count;
        }
        countText.text = "Particle Count: " + count;
    }
}
