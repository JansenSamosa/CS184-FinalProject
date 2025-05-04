using UnityEngine;

public class LookAtPointer : MonoBehaviour
{
    private Camera cam;                  // assign in Inspector or will use Camera.main

    void Start() {
        cam = Camera.main;
    }

    void Update()
    {   
        if (Input.GetMouseButton(1)) {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) {
                Vector3 point = hit.point;
                transform.LookAt(point);
            }
        }
    }
}
