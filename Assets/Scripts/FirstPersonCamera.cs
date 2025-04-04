using Fusion;
using UnityEngine;

public class FirstPersonCamera : MonoBehaviour
{
    [Header("Target & Offset")]
    public Transform Target;
    public Vector3 Offset = new Vector3(-0.18f, 0.7f, 0.15f);

    [Header("Visual camera rotation when turning the car")]
    [Tooltip("Maximum angle of deviation of the gaze to the left/right")]
    [Range(0f, 60f)]
    public float VisualYawMax = 15f;

    [Tooltip("Speed ​​of camera rotation to the side (the more - the faster)")]
    [Range(0.1f, 20f)]
    public float VisualYawSmooth = 5f;

    [Tooltip("Initial speed of returning the gaze to the center")]
    [Range(0.1f, 20f)]
    public float ReturnSpeed = 5f;

    [Header("Debug (current state)")]
    [ReadOnly] public float VisualYawTarget = 0f;

    private float currentVisualYaw = 0f;

    void LateUpdate()
    {
        if (Target == null) return;

        transform.position = Target.position + Target.TransformVector(Offset);

        // Smooth transition to the desired rotation angle
        currentVisualYaw = Mathf.Lerp(currentVisualYaw, VisualYawTarget, Time.deltaTime * VisualYawSmooth);

        transform.rotation = Quaternion.Euler(0f, Target.eulerAngles.y + currentVisualYaw, 0f);
    }
}