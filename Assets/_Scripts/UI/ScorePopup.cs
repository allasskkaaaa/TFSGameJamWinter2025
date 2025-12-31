using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
[RequireComponent(typeof(MeshRenderer))]
public class ScorePopup : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float lifetime = 1f;

    [Header("Scale Animation")]
    [SerializeField] private float startScale = 0.6f;
    [SerializeField] private float peakScale = 1.4f;
    [SerializeField] private float endScale = 0.3f;
    [SerializeField] private float popTime = 0.2f;

    private TextMeshPro text;
    private MeshRenderer meshRenderer;
    private Color startColor;
    private float timer;

    private void Awake()
    {
        text = GetComponentInChildren<TextMeshPro>(); 
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (text == null)
        {
            Debug.LogError("ScorePopup: Missing TextMeshPro", this);
            Destroy(gameObject);
            return;
        }

        if (meshRenderer == null)
        {
            Debug.LogError("ScorePopup: Missing MeshRenderer", this);
            Destroy(gameObject);
            return;
        }

        if (text.font == null)
            text.font = TMP_Settings.defaultFontAsset;

        startColor = text.color;
        text.text = "";

        // FORCE render above sprites
        meshRenderer.sortingLayerName = "UI";
        meshRenderer.sortingOrder = 500;

        transform.localScale = Vector3.one * startScale;

        if (lifetime <= 0f)
            lifetime = 1f;

        Destroy(gameObject, lifetime + 0.1f);
    }

    public void Setup(int score)
    {
        text.text = $"+{score}";
        timer = 0f;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / lifetime);

        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        float scale;
        if (timer < popTime)
        {
            scale = Mathf.Lerp(startScale, peakScale, timer / popTime);
        }
        else
        {
            float shrinkT = (timer - popTime) / (lifetime - popTime);
            scale = Mathf.Lerp(peakScale, endScale, shrinkT);
        }

        transform.localScale = Vector3.one * scale;

        float alpha = Mathf.Lerp(1f, 0f, t);
        text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
    }
}
