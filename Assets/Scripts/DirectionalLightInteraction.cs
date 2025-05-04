using UnityEngine;

public class DirectionalLightInteraction : MonoBehaviour
{
    public VLight vlight;
    public float intensityAdjustmentSensitivity = .2f;

    private Camera cam;                  // assign in Inspector or will use Camera.main
    private Light directionalLight;
    
    // retio of intensity of vlight and directional light.
    private float lightIntensityRatio;

    void Start() {
        cam = Camera.main;
        directionalLight = GetComponent<Light>();
        lightIntensityRatio = vlight.lightMultiplier / directionalLight.intensity;
    }

    void Update()
    {   
        if (Input.GetMouseButton(1)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {
                Vector3 point = hit.point;
                transform.LookAt(point);
                vlight.transform.position = transform.position;
                vlight.transform.LookAt(point);
            }
        }
        
        float intensityAdjustment = Input.GetAxis("Mouse ScrollWheel") * intensityAdjustmentSensitivity;
        directionalLight.intensity += intensityAdjustment;
        vlight.lightMultiplier = directionalLight.intensity * lightIntensityRatio;
    }
}
