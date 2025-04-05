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
    public float Braking = 15f;

    [Header("Wheels")]
    public Transform WheelLeftFront;
    public Transform WheelRightFront;
    public Transform WheelLeftBack;
    public Transform WheelRightBack;

    public float WheelRadius = 0.06f;
    public float MaxSteerAngle = 30f;

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

        // Flag for active turning
        bool isTurning = false;

        // Touch input (mobile)
        if (Input.touchCount > 0)
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            bool leftTouched = false;
            bool rightTouched = false;
            bool centerTouched = false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                float x = touch.position.x;
                float y = touch.position.y;

                if (y < screenHeight * 0.5f)
                {
                    if (x < screenWidth * 0.33f) leftTouched = true;
                    else if (x > screenWidth * 0.66f) rightTouched = true;
                    else centerTouched = true;
                }
            }

            if (leftTouched)
            {
                isTurning = true;
                turnAmount = Mathf.MoveTowards(turnAmount, -TurnMax, TurnAcceleration * Runner.DeltaTime);
            }
            else if (rightTouched)
            {
                isTurning = true;
                turnAmount = Mathf.MoveTowards(turnAmount, TurnMax, TurnAcceleration * Runner.DeltaTime);
            }
            else
            {
                turnAmount = Mathf.MoveTowards(turnAmount, 0f, TurnAcceleration * Runner.DeltaTime);
            }

            brakeTimer = centerTouched ? brakeTimer + Runner.DeltaTime : 0f;
        }
        // Mouse / Keyboard input (PC/editor)
        else if (Input.GetMouseButton(0))
        {
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            float x = Input.mousePosition.x;
            float y = Input.mousePosition.y;

            bool centerMouse = false;

            if (y < screenHeight * 0.5f)
            {
                if (x < screenWidth * 0.33f)
                {
                    isTurning = true;
                    turnAmount = Mathf.MoveTowards(turnAmount, -TurnMax, TurnAcceleration * Runner.DeltaTime);
                }
                else if (x > screenWidth * 0.66f)
                {
                    isTurning = true;
                    turnAmount = Mathf.MoveTowards(turnAmount, TurnMax, TurnAcceleration * Runner.DeltaTime);
                }
                else
                {
                    centerMouse = true;
                    turnAmount = Mathf.MoveTowards(turnAmount, 0f, TurnAcceleration * Runner.DeltaTime);
                }
            }
            else
            {
                turnAmount = Mathf.MoveTowards(turnAmount, 0f, TurnAcceleration * Runner.DeltaTime);
            }

            brakeTimer = (centerMouse || Input.GetKey(KeyCode.Space)) ? brakeTimer + Runner.DeltaTime : 0f;
        }
        else
        {
            turnAmount = Mathf.MoveTowards(turnAmount, 0f, TurnAcceleration * Runner.DeltaTime);
            brakeTimer = Input.GetKey(KeyCode.Space) ? brakeTimer + Runner.DeltaTime : 0f;
        }

        // Movement direction and speed
        Vector3 inputDir = new Vector3(turnAmount, 0f, 1f).normalized;
        moveDir = cameraRotationY * inputDir;

        _currentSpeed += (brakeTimer > 0f ? -Braking * brakeTimer : Acceleration) * Runner.DeltaTime;
        _currentSpeed = Mathf.Clamp(_currentSpeed, 0f, MaxSpeed);

        float brakeFactor = Mathf.Clamp01(brakeTimer);
        float currentSpeed = _currentSpeed * (1f - brakeFactor * BrakeStrength * Runner.DeltaTime);

        move = moveDir * currentSpeed * Runner.DeltaTime;

        verticalVelocity = _controller.isGrounded ? 0f : verticalVelocity + Gravity * Runner.DeltaTime;
        move.y = verticalVelocity;

        _controller.Move(move);

        // Calculate rotation amount (based on distance covered)
        float rotationAmount = (_currentSpeed * Runner.DeltaTime * 360f) / (2f * Mathf.PI * WheelRadius);

        // Rear wheels always rotate
        if (WheelLeftBack != null)  WheelLeftBack.Rotate(Vector3.right, rotationAmount);
        if (WheelRightBack != null) WheelRightBack.Rotate(Vector3.right, rotationAmount);

        // Front wheels — either rotate or steer
        if (isTurning)
        {
            // Apply steering (overwrite rotation visually)
            Quaternion steerRotation = Quaternion.Euler(0f, turnAmount * MaxSteerAngle, 0f);
            if (WheelLeftFront != null)  WheelLeftFront.localRotation = steerRotation;
            if (WheelRightFront != null) WheelRightFront.localRotation = steerRotation;
        }
        else
        {
            // Apply normal wheel spinning
            if (WheelLeftFront != null)  WheelLeftFront.Rotate(Vector3.right, rotationAmount);
            if (WheelRightFront != null) WheelRightFront.Rotate(Vector3.right, rotationAmount);
        }

        // Rotate the whole car to align with surface normal
        if (move != Vector3.zero)
        {
            Vector3 flatDirection = new Vector3(move.x, 0f, move.z);
            if (flatDirection.sqrMagnitude > 0.001f)
            {
                Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
                Vector3 groundNormal = Vector3.up;

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 2f))
                    groundNormal = hit.normal;

                Quaternion toRotation = Quaternion.LookRotation(flatDirection, groundNormal);
                transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, TurnSpeed * Runner.DeltaTime);
            }
        }

        // Apply camera visual yaw (if camera script is attached)
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
    public float CurrentSpeed => _currentSpeed;
}