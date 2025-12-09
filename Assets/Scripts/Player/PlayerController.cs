using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;

/// <summary>
/// Handles player input, movement, camera control, and interactions.
/// Encapsulates references to all player-related systems (movement, look, collision, interaction, etc.).
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS (Configuration / Settings)
    // ====================================================================================

    [Header("Movement Settings")]
    [SerializeField] private float _speed = 7.0f;
    public float Speed => _speed;

    [SerializeField] private float _jumpPower = 7.5f;
    public float JumpPower => _jumpPower;

    [SerializeField] private float _inAirSpeedScale = 0.8f;
    public float InAirSpeedScale => _inAirSpeedScale;

    [SerializeField] private float _sprintSpeedScale = 1.5f;
    public float SprintSpeedScale => _sprintSpeedScale;

    [SerializeField] private float _inAirSlowdownSmoothing = 10f;
    public float InAirSlowdownSmoothing => _inAirSlowdownSmoothing;

    [Header("Collision Settings")]
    [SerializeField] private float _overlapBoxScale = 0.5f;
    public float OverlapBoxScale => _overlapBoxScale;

    [SerializeField] private CapsuleCollider _playerCollider;
    public CapsuleCollider PlayerCollider => _playerCollider;

    [SerializeField] private LayerMask _resetJumpLayers;
    public LayerMask ResetJumpLayers => _resetJumpLayers;

    [Header("Camera Settings")]
    [SerializeField] private float _lookSensitivity = 0.07f;
    public float LookSensitivity => _lookSensitivity;

    [Header("Dependencies")]
    [SerializeField] private PlayerInteraction _playerInteraction;
    public PlayerInteraction PlayerInteraction => _playerInteraction;

    [SerializeField] private PlayerCollision _playerCollision;
    public PlayerCollision PlayerCollision => _playerCollision;

    [SerializeField] private PlayerGizmos _playerGizmos;
    public PlayerGizmos PlayerGizmos => _playerGizmos;

    [SerializeField] private PlayerLook _playerLook;
    public PlayerLook PlayerLook => _playerLook;

    [SerializeField] private PlayerMovement _playerMovement;
    public PlayerMovement PlayerMovement => _playerMovement;


    // ====================================================================================
    // PRIVATE FIELDS
    // ====================================================================================

    private Rigidbody _rb;
    public Rigidbody Rigidbody => _rb;

    private Camera _camera;
    public Camera Camera => _camera;

    private GameInput _gameInput;
    public GameInput GameInput => _gameInput;

    // Input actions
    private InputAction _move;
    public Vector3 Move => _move.ReadValue<Vector3>();

    private InputAction _jump;
    public bool IsJumping => _jump.WasPressedThisFrame();

    private InputAction _look;
    public Vector2 Look => _look.ReadValue<Vector2>();

    private InputAction _interact;
    public Vector2 Interact => _look.ReadValue<Vector2>(); // same as original logic

    private InputAction _sprint;
    public bool IsSprinting => _sprint.IsPressed();

    // Camera rotation values
    private float _cameraY;
    public float CameraY => _cameraY;

    private float _cameraX;
    public float CameraX => _cameraX;

    // Jump queuing
    private bool _isJumpQueued;
    public bool IsJumpQueued => _isJumpQueued;

    // Player movement state
    private PlayerMovement.MovementState _currentMovementState;
    public PlayerMovement.MovementState CurrentMovementState
    {
        get => _currentMovementState;
        set => _currentMovementState = value;
    }

    // Cached player collider dimensions for ground checks
    private Vector2 _playerColliderDimensions;
    public Vector2 PlayerColliderDimensions => _playerColliderDimensions;


    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================

    /// <summary>
    /// Initialize input actions and references before Start.
    /// </summary>
    private void Awake()
    {
        _gameInput = new GameInput();

        _jump = _gameInput.Player.Jump;
        _sprint = _gameInput.Player.Sprint;
        _move = _gameInput.Player.Move;
        _look = _gameInput.Player.Look;
        _interact = _gameInput.Player.Interact;
    }

    /// <summary>
    /// Cache components and set initial values.
    /// Lock cursor for first-person control.
    /// </summary>
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        _rb = GetComponent<Rigidbody>();
        _camera = GetComponentInChildren<Camera>();
        _playerInteraction = GetComponent<PlayerInteraction>();

        // Store player collider size for ground detection
        _playerColliderDimensions = new Vector2(
            _playerCollider.radius * 2,
            _playerCollider.height
        );
    }

    private void OnEnable() => _gameInput.Enable();
    private void OnDisable() => _gameInput.Disable();

    /// <summary>
    /// Handle collision events and set grounded state when player touches valid surfaces.
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        Collider[] groundCollisionObjects = _playerCollision.GetGroundCollisionObjects();

        if (groundCollisionObjects == null)
        {
            Debug.Log("[PLAYER] Invalid collision layer detected inside OverlapBox.");
            return;
        }

        _playerMovement.SetMovementState(PlayerMovement.MovementState.Grounded);
    }

    /// <summary>
    /// Physics-based movement and jump execution.
    /// </summary>
    private void FixedUpdate()
    {
        _playerMovement.Move();

        if (_isJumpQueued)
        {
            _isJumpQueued = false;
            _playerMovement.Jump();
        }
    }

    /// <summary>
    /// Handles input for looking, interaction, and queuing jumps each frame.
    /// </summary>
    private void Update()
    {
        // Update camera rotation
        _playerLook.Look();

        // Handle player interaction input
        if (_interact.WasPressedThisFrame())
        {
            Debug.Log("[PLAYER] Interaction event cast.");
            IInteractable interactable = _playerInteraction.GetInteractable();
            if (interactable != null)
            {
                interactable.Interact();
            }
            else
            {
                Debug.LogError("[PLAYER] Invalid interaction attempt.");
            }
        }

        // Handle jump input
        if (_jump.WasPressedThisFrame())
        {
            if (_currentMovementState != PlayerMovement.MovementState.InAir)
            {
                _isJumpQueued = true;
            }
            else
            {
                Debug.Log("[PLAYER] Jump failed: Player is not grounded.");
            }
        }
    }
}
