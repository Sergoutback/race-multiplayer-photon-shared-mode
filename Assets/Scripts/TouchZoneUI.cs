using UnityEngine;
using UnityEngine.UI;

public class TouchZoneUI : MonoBehaviour
{
    public Image Left;
    public Image Center;
    public Image Right;

    private Color defaultColor = new Color(1f, 1f, 1f, 0.0f);
    private Color activeColor = new Color(1f, 1f, 1f, 0.05f);

    void Update()
    {
        // Reset color
        Left.color = defaultColor;
        Center.color = defaultColor;
        Right.color = defaultColor;

        if (Input.touchCount > 0)
        {
            Vector2 pos = Input.GetTouch(0).position;
            HighlightZone(pos);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 pos = Input.mousePosition;
            HighlightZone(pos);
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            // If the space bar is pressed, highlight the central area
            Center.color = activeColor;
        }
    }

    void HighlightZone(Vector2 pos)
    {
        float w = Screen.width;
        float h = Screen.height;

        float x = pos.x;
        float y = pos.y;

        if (y > h * 0.5f)
        {
            // Top of the screen - do nothing
            return;
        }

        // Bottom half: highlight the desired area
        if (x < w * 0.33f)
        {
            Left.color = activeColor;
        }
        else if (x > w * 0.66f)
        {
            Right.color = activeColor;
        }
        else
        {
            Center.color = activeColor;
        }
    }

}