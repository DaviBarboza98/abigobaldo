using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Header("Tecla para mostrar")]
    [SerializeField] private KeyCode teclaMostrar = KeyCode.E;

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        bool segurando = Input.GetKey(teclaMostrar);
        canvasGroup.alpha = segurando ? 1f : 0f;
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.forward);
    }
}