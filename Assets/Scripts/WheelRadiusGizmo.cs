using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class WheelRadiusGizmo : MonoBehaviour
{
    public float Radius = 0.3f;
    public Color GizmoColor = Color.yellow;

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmoColor;
        Gizmos.DrawWireSphere(transform.position, Radius);
    }

    // Method for calculating radius
    public void CalculateRadiusFromMesh()
    {
        var renderer = GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            Radius = renderer.bounds.size.y / 2f;
            Debug.Log($"[WheelRadiusGizmo] Calculated Radius: {Radius}");
        }
        else
        {
            Debug.LogWarning("[WheelRadiusGizmo] MeshRenderer not found.");
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(WheelRadiusGizmo))]
public class WheelRadiusGizmoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WheelRadiusGizmo gizmo = (WheelRadiusGizmo)target;
        if (GUILayout.Button("📏 Calculate radius from MeshRenderer"))
        {
            gizmo.CalculateRadiusFromMesh();
        }
    }
}
#endif