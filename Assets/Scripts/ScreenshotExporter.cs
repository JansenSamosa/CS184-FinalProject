using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenshotExporter : MonoBehaviour
{
    [Tooltip("Drag your Button here")]
    public Button exportButton;
    [Tooltip("Filename will be placed in Application.persistentDataPath")]
    public string fileName = "GameView.png";

    void Awake()
    {
        exportButton.onClick.AddListener(() => StartCoroutine(Capture()));
    }

    IEnumerator Capture()
    {
        yield return new WaitForEndOfFrame();
        var width  = Screen.width;
        var height = Screen.height;
        var tex    = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        var bytes = tex.EncodeToPNG();
        Destroy(tex);

        var path = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Saved screenshot to: {path}");
    }
}
