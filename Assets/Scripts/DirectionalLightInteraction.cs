using UnityEngine;

public class DirectionalLightInteraction : MonoBehaviour
{
    public VLight vlight;
    public float intensityAdjustmentSensitivity = .2f;
    public float startAnimationLength = 1f;

    private Camera cam;                  // assign in Inspector or will use Camera.main
    private Light directionalLight;
    
    // retio of intensity of vlight and directional light.
    private float lightIntensityRatio;

    private float lightIntensityInitial = 1;
    private float timer = 0;

    void Start() {
        cam = Camera.main;
        directionalLight = GetComponent<Light>();
        lightIntensityRatio = vlight.lightMultiplier / directionalLight.intensity;

        lightIntensityInitial = directionalLight.intensity;
        directionalLight.intensity = 0;
        vlight.lightMultiplier = 0;

        timer = 0;
    }

    void Update()
    {   
        if (timer < startAnimationLength) {
            timer += Time.deltaTime;
            float t = 1 - (startAnimationLength - timer) / startAnimationLength;
            directionalLight.intensity = Mathf.Lerp(0, lightIntensityInitial, t * t);
            vlight.lightMultiplier = directionalLight.intensity * lightIntensityRatio;
            return;
        }
        if (Input.GetMouseButton(1)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {
                Vector3 point = hit.point;
                transform.LookAt(point);
                vlight.transform.LookAt(point);
            }
        }
        
        float intensityAdjustment = Input.GetAxis("Mouse ScrollWheel") * intensityAdjustmentSensitivity;
        directionalLight.intensity += intensityAdjustment;
        vlight.lightMultiplier = directionalLight.intensity * lightIntensityRatio;
    }

    
}
