using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(GravityBody))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public PlanetGravity planet;

    [Header("Movement")]
    public float walkSpeed = 12f;
    public float sprintSpeed = 24f;
    public float acceleration = 20f;

    [Header("Jump")]
    public float jumpForce = 24f;

    [Header("Look")]
    public float mouseSensitivity = 2f;

    [Header("Grounding")]
    public LayerMask groundMask;
    public float groundCheckDistance = 1.5f;

    private Rigidbody rb;
    private InputSystem_Actions input;

    private Vector2 moveInput;
    private float currentYaw;
    private bool jumpRequested;
    private bool isSprinting;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.angularDamping = 10f;

        input = new InputSystem_Actions();
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        moveInput = input.Player.Move.ReadValue<Vector2>();

        // Yaw — rotate player body horizontally
        Vector2 lookInput = input.Player.Look.ReadValue<Vector2>();
        currentYaw += lookInput.x * mouseSensitivity;

        // Toggle sprint
        if (input.Player.Sprint.WasPressedThisFrame())
            isSprinting = !isSprinting;

        // Jump request
        if (input.Player.Jump.WasPressedThisFrame())
            jumpRequested = true;
    }

    void FixedUpdate()
    {
        if (planet == null) return;

        AlignToGravity();
        CheckGround();
        HandleMovement();
        HandleJump();
    }

    void AlignToGravity()
    {
        Vector3 up = (transform.position - planet.transform.position).normalized;
        Quaternion gravityAlign = Quaternion.FromToRotation(Vector3.up, up);
        Quaternion targetRot = gravityAlign * Quaternion.Euler(0f, currentYaw, 0f);
        rb.MoveRotation(targetRot);
    }

    void CheckGround()
    {
        Vector3 down = (planet.transform.position - transform.position).normalized;
        isGrounded = Physics.Raycast(transform.position, down, groundCheckDistance, groundMask);
    }

    void HandleMovement()
    {
        Vector3 up = (transform.position - planet.transform.position).normalized;
        Vector3 input3D = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        float targetSpeed = isSprinting ? sprintSpeed : walkSpeed;
        Vector3 desiredVel = transform.TransformDirection(input3D) * targetSpeed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 surfaceVelocity = Vector3.ProjectOnPlane(velocity, up);
        Vector3 verticalVelocity = Vector3.Project(velocity, up);
        Vector3 velocityChange = desiredVel - surfaceVelocity;

        rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);
        rb.linearVelocity = surfaceVelocity + verticalVelocity;
    }

    void HandleJump()
    {
        if (!jumpRequested) return;
        jumpRequested = false;
        if (!isGrounded) return;

        Vector3 up = (transform.position - planet.transform.position).normalized;
        rb.linearVelocity -= Vector3.Project(rb.linearVelocity, up);
        rb.AddForce(up * jumpForce, ForceMode.VelocityChange);
    }

    // Read by PlanetCameraController
    public Vector3 GravityUp =>
        planet != null ? (transform.position - planet.transform.position).normalized : Vector3.up;
}