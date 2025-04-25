using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ParticleCounter : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> particleSystems;
    [SerializeField] Text countText;

    void Update()
    {
        int count = 0;
        for (int i = 0; i < particleSystems.Count; i++) {   
            count += particleSystems[i].activeParticles.Count;
        }
        countText.text = "Particle Count: " + count;
    }
}
