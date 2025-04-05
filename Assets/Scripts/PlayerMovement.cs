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

        // Universal "mobile" control: works on Android and in the editor/PC
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
                    if (x < screenWidth * 0.33f)
                        leftTouched = true;
                    else if (x > screenWidth * 0.66f)
                        rightTouched = true;
                    else
                        centerTouched = true;
                }
            }

            // Turn processing
            if (leftTouched)
                turnAmount = Mathf.MoveTowards(turnAmount, -TurnMax, TurnAcceleration * Runner.DeltaTime);
            else if (rightTouched)
                turnAmount = Mathf.MoveTowards(turnAmount, TurnMax, TurnAcceleration * Runner.DeltaTime);
            else
                turnAmount = Mathf.MoveTowards(turnAmount, 0f, TurnAcceleration * Runner.DeltaTime);

            // Brake treatment
            if (centerTouched)
                brakeTimer += Runner.DeltaTime;
            else
                brakeTimer = 0f;
        }

        else if (Input.GetMouseButton(0)) // Mouse for PC
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
                    turnAmount = Mathf.MoveTowards(turnAmount, -TurnMax, TurnAcceleration * Runner.DeltaTime);
                }
                else if (x > screenWidth * 0.66f)
                {
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

            // Handling the brake taking into account the mouse and spacebar
            if (centerMouse || Input.GetKey(KeyCode.Space))
            {
                brakeTimer += Runner.DeltaTime;
            }
            else
            {
                brakeTimer = 0f;
            }
        }
        else
        {
            // Smoothly reset the turn
            turnAmount = Mathf.MoveTowards(turnAmount, 0f, TurnAcceleration * Runner.DeltaTime);

            // Separate handling of space - if the mouse is not pressed
            if (Input.GetKey(KeyCode.Space))
            {
                brakeTimer += Runner.DeltaTime;
            }
            else
            {
                brakeTimer = 0f;
            }
        }




        Vector3 inputDir = new Vector3(turnAmount, 0f, 1f).normalized;
        moveDir = cameraRotationY * inputDir;


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
        
        // Wheel rotation by speed
        float rotationAmount = (_currentSpeed * Runner.DeltaTime * 360f) / (2f * Mathf.PI * WheelRadius);
        if (WheelLeftFront != null)  WheelLeftFront.Rotate(Vector3.right, rotationAmount);
        if (WheelRightFront != null) WheelRightFront.Rotate(Vector3.right, rotationAmount);
        if (WheelLeftBack != null)   WheelLeftBack.Rotate(Vector3.right, rotationAmount);
        if (WheelRightBack != null)  WheelRightBack.Rotate(Vector3.right, rotationAmount);

        // Turning the front wheels left/right
        Quaternion steerRotation = Quaternion.Euler(0f, turnAmount * MaxSteerAngle, 0f);
        if (WheelLeftFront != null)  WheelLeftFront.localRotation = steerRotation * Quaternion.Euler(0f, 0f, WheelLeftFront.localRotation.z);
        if (WheelRightFront != null) WheelRightFront.localRotation = steerRotation * Quaternion.Euler(0f, 0f, WheelRightFront.localRotation.z);


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
