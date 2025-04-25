using UnityEngine;

public class Billboard : MonoBehaviour
{
    // This script faces the object towards the camera
    // Placed primarly on particles so that the particle texture is always visible

    void Update()
    {
        transform.forward = Camera.main.transform.forward;
    }
}
