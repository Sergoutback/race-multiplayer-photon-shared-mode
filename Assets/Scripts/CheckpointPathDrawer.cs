using UnityEngine;

[ExecuteAlways]
public class CheckpointPathDrawer : MonoBehaviour
{
    public Color LineColor = Color.yellow;
    public float SphereRadius = 0.5f;

    private void OnDrawGizmos()
    {
        Transform[] checkpoints = GetComponentsInChildren<Transform>();
        if (checkpoints.Length < 2) return;

        Gizmos.color = LineColor;

        for (int i = 1; i < checkpoints.Length; i++)
        {
            Transform prev = checkpoints[i - 1];
            Transform current = checkpoints[i];

            Gizmos.DrawLine(prev.position, current.position);
            Gizmos.DrawSphere(current.position, SphereRadius);
        }

        // Start sphere
        Gizmos.DrawSphere(checkpoints[1].position, SphereRadius);
    }
}