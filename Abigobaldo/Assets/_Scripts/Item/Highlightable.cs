using UnityEngine;

public class Highlightable : MonoBehaviour
{
    [SerializeField]
    private Behaviour outline;

    private void Awake()
    {
        outline.enabled = false;
    }

    public void Highlight()
    {
        outline.enabled = true;
    }

    public void RemoveHighlight()
    {
        outline.enabled = false;
    }
}