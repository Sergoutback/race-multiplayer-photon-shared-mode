using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Checkpoint : MonoBehaviour
{
    public int Index;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        Highlight(false); // at startup - gray
    }

    public void Highlight(bool active)
    {
        if (_renderer != null)
        {
            Color targetColor = active 
                ? new Color(0.2f, 1f, 0.2f, 0.3f) 
                : new Color(0.4f, 0.6f, 1f, 0.3f);

            _renderer.material.color = targetColor;

            Debug.Log($"Checkpoint {Index} highlight = {active}");
        }
    }


}