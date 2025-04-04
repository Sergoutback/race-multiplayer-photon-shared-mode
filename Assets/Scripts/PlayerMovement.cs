using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public Camera Camera;

    private CharacterController _controller;

    [Header("Movement and rotation")]
    public float TurnSpeed = 1f;
    public float Acceleration = 7f;
    public float MaxSpeed = 25f;
    public float Braking = 5f;

    [Header("Smooth turn")]
    public float TurnAcceleration = 1.5f;
    public float TurnMax = 1f;
    private float turnAmount = 0f;

    [Header("Smooth braking")]
    public float BrakeStrength = 1f;
    private float brakeTimer = 0f;

    [Header("Gravity")]
    public float Gravity = -9.81f;
    private float verticalVelocity = 0f;

    private float _currentSpeed = 0f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    public override void FixedUpdateNetwork()
    {
        Quaternion cameraRotationY = Quaternion.Euler(0, Camera.transform.rotation.eulerAngles.y, 0);
        Vector3 move = Vector3.zero;
        Vector3 moveDir = Vector3.zero;

#if UNITY_ANDROID && !UNITY_EDITOR
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float x = touch.position.x;
            float y = touch.position.y;

            if (y < screenHeight * 0.5f)
            {
                if (x < screenWidth * 0.33f)
                {
                    turnAmount = Mathf.MoveTowards(turnAmount, -TurnMax, TurnAcceleration * Runner.DeltaTime);
                    brakeTimer = 0f;
                }
                else if (x > screenWidth * 0.66f)
                {
                    turnAmount = Mathf.MoveTowards(turnAmount, TurnMax, TurnAcceleration * Runner.DeltaTime);
                    brakeTimer = 0f;
                }
                else
                {
                    brakeTimer += Runner.DeltaTime;
                    turnAmount = Mathf.MoveTowards(turnAmount, 0f, TurnAcceleration * Runner.DeltaTime);
                }
            }
            else
            {
                turnAmount = Mathf.MoveTowards(turnAmount, 0f, TurnAcceleration * Runner.DeltaTime);
                brakeTimer = 0f;
            }
        }
        else
        {
            turnAmount = Mathf.MoveTowards(turnAmount, 0f, TurnAcceleration * Runner.DeltaTime);
            brakeTimer = 0f;
        }

        Vector3 inputDir = new Vector3(turnAmount, 0f, 1f).normalized;
        moveDir = cameraRotationY * inputDir;
#else
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        bool isBraking = input == Vector3.zero;
        moveDir = cameraRotationY * input.normalized;
        if (isBraking)
        {
            brakeTimer += Runner.DeltaTime;
        }
        else
        {
            brakeTimer = 0f;
        }
#endif

        // Acceleration / braking
        if (brakeTimer > 0f)
        {
            _currentSpeed -= Braking * brakeTimer * Runner.DeltaTime;
        }
        else
        {
            _currentSpeed += Acceleration * Runner.DeltaTime;
        }

        _currentSpeed = Mathf.Clamp(_currentSpeed, 0f, MaxSpeed);
        float brakeFactor = Mathf.Clamp01(brakeTimer);
        float currentSpeed = _currentSpeed * (1f - brakeFactor * BrakeStrength * Runner.DeltaTime);

        move = moveDir * currentSpeed * Runner.DeltaTime;

        // Gravity
        if (_controller.isGrounded)
        {
            verticalVelocity = 0f;
        }
        else
        {
            verticalVelocity += Gravity * Runner.DeltaTime;
        }

        move.y = verticalVelocity;
        _controller.Move(move);

        // Rotation with surface alignment
        if (move != Vector3.zero)
        {
            Vector3 flatDirection = new Vector3(move.x, 0f, move.z);
            if (flatDirection.sqrMagnitude > 0.001f)
            {
                // Raycast down to get ground normal
                RaycastHit hit;
                Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
                Vector3 groundNormal = Vector3.up;

                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 2f))
                {
                    groundNormal = hit.normal;
                }

                // Rotation with terrain normal
                Quaternion toRotation = Quaternion.LookRotation(flatDirection, groundNormal);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, TurnSpeed * Runner.DeltaTime);
            }
        }

        // Visual camera yaw
        var cam = Camera.GetComponent<FirstPersonCamera>();
        if (cam != null)
        {
            cam.VisualYawTarget = turnAmount * cam.VisualYawMax;
        }
    }

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            Camera = Camera.main;
            Camera.GetComponent<FirstPersonCamera>().Target = transform;
        }
    }
}
