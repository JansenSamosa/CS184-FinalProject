using UnityEngine;
using UnityEngine.UI;

public class ToggleParticleSystem : MonoBehaviour
{
    private Toggle uiToggle;
    [SerializeField] GameObject particleSystemObject;

    void Start()
    {   
        uiToggle = GetComponent<Toggle>();
        uiToggle.onValueChanged.AddListener(isOn => 
            particleSystemObject.SetActive(isOn)
        );
        // initialize state
        particleSystemObject.SetActive(uiToggle.isOn);
    }
}
