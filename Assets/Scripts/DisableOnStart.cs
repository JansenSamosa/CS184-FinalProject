using UnityEditor.ShaderGraph.Serialization;
using UnityEngine;

public class DisableOnStart : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(false);
    }
}
