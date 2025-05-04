using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetScene : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) {
            var active = SceneManager.GetActiveScene();
            SceneManager.LoadScene(active.name);
        }
    }
}
