using UnityEngine;

public class LightInteractionEffect : MonoBehaviour
{
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private AnimationCurve sizeCurve;
    [SerializeField] private Color startColor = Color.white;
    [SerializeField] private Color endColor = new Color(1, 1, 0, 0);

    private float timer = 0;
    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void Start()
    {
        transform.forward = Camera.main.transform.forward; // Billboard effect
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / duration;

        // Adjust size
        transform.localScale = Vector3.one * sizeCurve.Evaluate(t);

        // Adjust color
        if (rend != null)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", Color.Lerp(startColor, endColor, t));
            rend.SetPropertyBlock(propertyBlock);
        }

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}