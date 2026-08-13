using UnityEngine;
using UnityEngine.UI;

public class BackgroundMovimento : MonoBehaviour
{
    public float velocidadeX = 0.05f;
    public float velocidadeY = 0.03f;

    private RawImage background;
    private Vector2 offset;

    void Start()
    {
        background = GetComponent<RawImage>();
        offset = background.uvRect.position;
    }

    void Update()
    {
        offset += new Vector2(velocidadeX, velocidadeY) * Time.deltaTime;

        background.uvRect = new Rect(
            offset.x,
            offset.y,
            background.uvRect.width,
            background.uvRect.height
        );
    }
}