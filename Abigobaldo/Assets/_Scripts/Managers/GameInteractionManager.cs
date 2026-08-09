using UnityEngine;

public class GameInteractionManager : MonoBehaviour
{
    public static GameInteractionManager Instance { get; private set; }

    [Header("Highlight global")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color emissionColor = new Color(1f, 0.65f, 0.05f, 1f);
    [SerializeField] private float emissionIntensity = 1.1f;

    public Color HighlightColor => highlightColor;
    public Color EmissionColor => emissionColor;
    public float EmissionIntensity => emissionIntensity;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
}

