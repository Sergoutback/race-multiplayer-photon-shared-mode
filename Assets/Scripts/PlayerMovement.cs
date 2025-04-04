using Fusion;
using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
    public Camera Camera;
    
    private CharacterController _controller;

    public float PlayerSpeed = 2f;
    
    public float TurnSpeed = 100f; // rotation speed in degrees per second
    
    public float Acceleration = 5f;
    public float MaxSpeed = 10f;
    public float Braking = 10f;

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
        bool isBraking = false;

#if UNITY_ANDROID && !UNITY_EDITOR
    // Mobile control
    if (Input.touchCount > 0)
    {
        Touch touch = Input.GetTouch(0);
        float screenWidth = Screen.width;
        float x = touch.position.x;

        if (x < screenWidth * 0.33f)
        {
            // Turn left
            moveDir = new Vector3(-1, 0, 1);
        }
        else if (x > screenWidth * 0.66f)
        {
            // Turn right
            moveDir = new Vector3(1, 0, 1);
        }
        else
        {
            // The center is the brake
            isBraking = true;
        }
    }
    else
    {
        // Gas straight
        moveDir = Vector3.forward;
    }
#else
        // Control on PC
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        isBraking = input == Vector3.zero;
        moveDir = input;
#endif

        // Acceleration / braking
        if (isBraking)
        {
            _currentSpeed -= Braking * Runner.DeltaTime;
        }
        else
        {
            _currentSpeed += Acceleration * Runner.DeltaTime;
        }

        _currentSpeed = Mathf.Clamp(_currentSpeed, 0f, MaxSpeed);

        moveDir = cameraRotationY * moveDir.normalized;
        move = moveDir * _currentSpeed * Runner.DeltaTime;

        _controller.Move(move);

        if (move != Vector3.zero)
        {
            // Smooth turn of the car in the direction of movement
            Vector3 direction = move.normalized;
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, TurnSpeed * Runner.DeltaTime);
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