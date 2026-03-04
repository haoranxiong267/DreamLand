using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ====== Movement Settings ======
    [Header("Movement Settings")]
    [Tooltip("Character movement speed")]
    public float moveSpeed = 5f;

    [Tooltip("Character rotation speed")]
    public float rotationSpeed = 10f;

    [Tooltip("Gravity acceleration")]
    public float gravity = -9.81f;

    // ====== Character Model Reference ======
    [Header("Character Model Reference")]
    [Tooltip("Drag the child object representing the character's visual model here")]
    public Transform characterModel;

    // ====== Component References ======
    [Header("Component References")]
    [Tooltip("Character Controller component (auto-fetched)")]
    private CharacterController characterController;

    [Tooltip("Animator component (auto-fetched)")]
    private Animator animator;

    // ====== Animation Parameters ======
    [Header("Animation Parameters")]
    [Tooltip("Speed parameter name (Float parameter in Animator)")]
    public string speedParam = "Speed";

    [Tooltip("Horizontal input parameter name")]
    public string horizontalParam = "Horizontal";

    [Tooltip("Vertical input parameter name")]
    public string verticalParam = "Vertical";

    // ====== Internal Variables ======
    private Vector3 moveDirection;    // Movement direction
    private Vector3 velocity;         // Velocity (includes gravity)
    private bool isGrounded;          // Is on ground
    private float currentSpeed;       // Remember current speed for animation

    void Start()
    {
        // 1. Get component references
        InitializeComponents();

        // 2. Initialize animation state
        InitializeAnimation();
    }

    void InitializeComponents()
    {
        // Get Animator component
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("PlayerController: Animator component not found!");
        }

        // Get or add CharacterController component
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            // Auto-add and configure CharacterController
            characterController = gameObject.AddComponent<CharacterController>();
            Debug.Log("PlayerController: CharacterController component auto-added.");

            // Set reasonable default values
            characterController.height = 2.0f;
            characterController.radius = 0.5f;
            characterController.center = new Vector3(0, 1.0f, 0);
            characterController.skinWidth = 0.08f;
            characterController.minMoveDistance = 0.001f;
        }
    }

    void InitializeAnimation()
    {
        // Ensure initial animation state is Idle
        if (animator != null)
        {
            // Reset all animation parameters
            animator.SetFloat(speedParam, 0f);
            animator.SetFloat(horizontalParam, 0f);
            animator.SetFloat(verticalParam, 0f);

            // Force transition to Idle state
            animator.Play("Idle", 0, 0f);
        }
    }

    void Update()
    {
        // Debug: print input values
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Debug.Log($"Input values - H: {h}, V: {v}");

        // Debug: print calculated movement direction and speed
        Debug.Log($"Movement direction: {moveDirection}, Speed: {currentSpeed}");

        // 1. Ground check
        CheckGround();

        // 2. Apply gravity
        ApplyGravity();

        // 3. Get player input
        HandleInput();

        // 4. Move the character
        MoveCharacter();

        // 5. Update animations
        UpdateAnimation();
    }

    void CheckGround()
    {
        // Use CharacterController's built-in ground detection
        isGrounded = characterController.isGrounded;

        // If grounded and vertical velocity is downward, reset vertical velocity
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep character grounded
        }
    }

    void ApplyGravity()
    {
        // Apply gravity acceleration
        velocity.y += gravity * Time.deltaTime;
    }

    void HandleInput()
    {
        // Get WASD input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate movement direction (based on world coordinates)
        moveDirection = new Vector3(horizontal, 0, vertical);
        currentSpeed = moveDirection.magnitude; // Calculate speed

        // *** KEY MODIFICATION: Rotate ONLY the character model, NOT the root object ***
        if (currentSpeed > 0.1f && characterModel != null)
        {
            // Calculate the target rotation to look in the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // Smoothly rotate the characterModel (visual part only)
            characterModel.rotation = Quaternion.Slerp(
                characterModel.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
            Debug.Log($"Rotating model to: {targetRotation.eulerAngles}");
        }
        // Warning if the visual model is not assigned
        else if (currentSpeed > 0.1f)
        {
            Debug.LogWarning("Movement input detected, but characterModel is not assigned. Visual model will not rotate.");
        }
        // *** Original code that rotates the entire object (including camera) is REMOVED ***
    }

    void MoveCharacter()
    {
        if (moveDirection.magnitude > 0.1f)
        {
            // 1. Apply movement speed
            Vector3 movement = moveDirection * moveSpeed;

            // 2. Add gravity effect
            movement.y = velocity.y;

            // 3. Execute movement
            characterController.Move(movement * Time.deltaTime);

            // 4. Smoothly rotate towards movement direction
            // RotateTowardsMovement(); // This call is COMMENTED OUT as requested
        }
        else
        {
            // When no input, only apply gravity
            characterController.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
        }
    }

    void RotateTowardsMovement()
    {
        // Create target rotation (look at movement direction)
        Vector3 lookDirection = new Vector3(moveDirection.x, 0, moveDirection.z);
        if (lookDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            Debug.Log($"RotateTowardsMovement called. Target angle: {targetRotation.eulerAngles}");

            // Smooth rotation of the character MODEL only
            if (characterModel != null)
            {
                characterModel.rotation = Quaternion.Slerp(
                    characterModel.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            // Note: The else branch that modifies transform.rotation is removed.
            // We only want rotation to occur when characterModel exists.
        }
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        // Get input values (for animation blending)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate total speed (for Speed parameter)
        float speed = Mathf.Clamp01(new Vector3(horizontal, 0, vertical).magnitude);

        // Set animation parameters
        animator.SetFloat(speedParam, speed);
        animator.SetFloat(horizontalParam, horizontal);
        animator.SetFloat(verticalParam, vertical);

        // Debug output (optional)
        // Debug.Log($"Animation Parameters - Speed: {speed}, H: {horizontal}, V: {vertical}");
    }

    // Methods callable by other scripts
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            // Stop animation when disabled
            if (animator != null)
            {
                animator.SetFloat(speedParam, 0f);
            }
        }
    }

    public bool IsMoving()
    {
        return moveDirection.magnitude > 0.1f;
    }
}