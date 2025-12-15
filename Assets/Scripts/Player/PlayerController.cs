using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;

/// <summary>
/// Central controller for all player-related systems.
/// Handles input, movement, camera control, collision checks, and interaction.
/// Exposes references to movement, look, collision, gizmos, and interaction components.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS — CONFIGURATION
    // ====================================================================================

    [Header("Movement Settings")]
    [SerializeField] private float _speed = 7f;
    public float Speed => _speed;

    [SerializeField] private float _jumpPower = 7.5f;
    public float JumpPower => _jumpPower;

    [SerializeField] private float _inAirSpeedScale = 0.8f;
    public float InAirSpeedScale => _inAirSpeedScale;

    [SerializeField] private float _sprintSpeedScale = 1.5f;
    public float SprintSpeedScale => _sprintSpeedScale;

    [SerializeField] private float _crouchSpeedScale = 0.6f;
    public float CrouchSpeedScale => _crouchSpeedScale;

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

    [SerializeField] private float _baseCameraFov = 70;
    public float BaseCameraFov => _baseCameraFov;

    [SerializeField] private float _walkCameraFov = 80;
    public float WalkCameraFov => _walkCameraFov;

    [SerializeField] private float _sprintCameraFov = 100;
    public float SprintCameraFov => _sprintCameraFov;

    [SerializeField] private float _cameraFovChangeSmoothingFactor = 5f;
    public float CameraFovChangeSmoothingFactor => _cameraFovChangeSmoothingFactor;

    [SerializeField] private bool _changeCameraFovWithMovement = true;
    public bool ChangeCameraFovWithMovement => _changeCameraFovWithMovement;

    [SerializeField] private float _verticalCrouchOffset = 0.3f;
    public float VerticalCrouchOffset => _verticalCrouchOffset;

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

    [Header("World Void Detection")]
    [SerializeField] private float _killPlayerAfterHeight = -8f;
    public float KillPlayerAfterHeight => _killPlayerAfterHeight;

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
    public bool WasJumpPressed => _jump.WasPressedThisFrame();

    private InputAction _crouch;
    public bool WasCrouchPressed => _crouch.WasPressedThisFrame();

    private InputAction _look;
    public Vector2 Look => _look.ReadValue<Vector2>();

    private InputAction _interact;
    public bool WasInteractPressed => _interact.WasPressedThisFrame();

    private InputAction _sprint;
    public bool IsSprintPressed => _sprint.IsPressed();

    // Camera rotation
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

    // Cached collider dimensions for ground checks
    private Vector2 _playerColliderDimensions;
    public Vector2 PlayerColliderDimensions => _playerColliderDimensions;

    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================

    private void Awake()
    {
        _gameInput = new GameInput();

        _jump = _gameInput.Player.Jump;
        _sprint = _gameInput.Player.Sprint;
        _move = _gameInput.Player.Move;
        _look = _gameInput.Player.Look;
        _interact = _gameInput.Player.Interact;
        _crouch = _gameInput.Player.Crouch;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _rb = GetComponent<Rigidbody>();
        _camera = GetComponentInChildren<Camera>();
        _playerInteraction = GetComponent<PlayerInteraction>();

        // Cache collider size for ground detection
        _playerColliderDimensions = new Vector2(
            _playerCollider.radius * 2f,
            _playerCollider.height
        );
    }

    private void OnEnable() => _gameInput.Enable();
    private void OnDisable() => _gameInput.Disable();

    private void OnCollisionEnter(Collision collision)
    {
        Collider[] groundCollisionObjects = _playerCollision.GetGroundCollisionObjects();

        if (groundCollisionObjects == null)
        {
            Debug.Log("[PLAYER] Invalid collision layer detected inside OverlapBox -");
            return;
        }

        _playerMovement.SetMovementState(PlayerMovement.MovementState.Grounded);
    }

    private void FixedUpdate()
    {
        _playerMovement.Move();

        if (_isJumpQueued)
        {
            _isJumpQueued = false;
            _playerMovement.Jump();
        }
    }

    private void Update()
    {
        _playerLook.Look();

        // Interaction handling
        if (_interact.WasPressedThisFrame())
        {
            Debug.Log("[PLAYER] Interaction event cast -");

            IInteractable interactable = _playerInteraction.GetInteractable();
            if (interactable != null)
            {
                interactable.Interact();
            }
            else
            {
                Debug.LogError("[PLAYER] Invalid interaction attempt -");
            }
        }

        // Jump queuing
        if (_jump.WasPressedThisFrame() && !_playerMovement.IsCrouching)
        {
            if (_currentMovementState != PlayerMovement.MovementState.InAir)
                _isJumpQueued = true;
            else
                Debug.Log("[PLAYER] Jump failed: Player is not grounded -");
        }

        // Crouch input
        if (_crouch.WasPressedThisFrame() &&
            _currentMovementState == PlayerMovement.MovementState.Grounded)
        {
            _playerMovement.ToggleCrouch();
        }
    }
}
