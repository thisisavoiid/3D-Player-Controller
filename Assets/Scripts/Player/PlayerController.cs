using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // ====================================================================================
    // ENUMS
    // ====================================================================================

    /// <summary>
    /// Defines the current movement state of the player.
    /// </summary>
    private enum MovementState
    {
        Grounded,
        InAir
    }

    /// <summary>
    /// Defines the current movement mode of the player.
    /// </summary>
    private enum MovementMode
    {
        Walk,
        Sprint
    }

    // ====================================================================================
    // INSPECTOR FIELDS (Configuration / Settings)
    // ====================================================================================

    [SerializeField] private float _speed = 7.0f;
    [SerializeField] private float _jumpPower = 7.5f;
    [SerializeField] private float _overlapBoxScale = 0.5f;
    [SerializeField] private float _inAirSpeedScale = 0.8f;
    [SerializeField] private float _sprintSpeedScale = 1.5f;
    [SerializeField] private float _inAirSlowdownSmoothing = 10f;
    [SerializeField] private float _lookSensitivity = 0.07f;
    [SerializeField] private CapsuleCollider _playerCollider;
    [SerializeField] private LayerMask _resetJumpLayers;

    // ====================================================================================
    // PRIVATE FIELDS (Components & Internal State)
    // ====================================================================================

    private Rigidbody _rb;
    private Camera _camera;
    private PlayerInteraction _interaction;
    private GameInput _gameInput;

    private InputAction _move;
    private InputAction _jump;
    private InputAction _look;
    private InputAction _interact;
    private InputAction _sprint;

    private float _cameraY;
    private float _cameraX;
    private MovementState _currentMovementState;
    private MovementMode _currentMovementMode;
    private Vector2 _playerColliderDimensions;

    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================

    private void Awake()
    {
        _gameInput = new GameInput();

        // Initialize Input Actions
        _jump = _gameInput.Player.Jump;
        _sprint = _gameInput.Player.Sprint;
        _move = _gameInput.Player.Move;
        _look = _gameInput.Player.Look;
        _interact = _gameInput.Player.Interact;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = true;

        _rb = GetComponent<Rigidbody>();
        _camera = GetComponentInChildren<Camera>();
        _interaction = GetComponent<PlayerInteraction>();

        _playerColliderDimensions = new Vector2(_playerCollider.radius * 2, _playerCollider.height);
    }

    private void OnEnable()
    {
        _gameInput.Enable();
    }

    private void OnDisable()
    {
        _gameInput.Disable();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider[] groundCollisionObjects = GetGroundCollisionObjects();

        if (groundCollisionObjects == null)
        {
            Debug.Log("[PLAYER] Invalid collision layer detected inside OverlapBox. -");
            return;
        }

        SetMovementState(MovementState.Grounded);
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Update()
    {
        Look();

        if (_interact.WasPressedThisFrame())
        {
            Debug.Log($"[PLAYER] Interaction event has been casted. -");
            Interact();
        }

        if (_jump.WasPressedThisFrame())
        {
            if (_currentMovementState != MovementState.InAir)
            {
                Jump();
            }
            else
            {
                Debug.Log($"[PLAYER] Jump failed: Player state is not Grounded. -");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            new Vector3(transform.position.x, transform.position.y - _playerColliderDimensions.y / 2, transform.position.z),
            Vector3.one * _overlapBoxScale
        );

        Gizmos.color = Color.cyan;
        if (_playerCollider != null && _rb != null)
        {
            Vector3 startPos = _rb.transform.position + (Vector3.down * (_playerColliderDimensions.y / 2));
            float distanceToGround = GetDistanceToGround();

            if (distanceToGround > 0)
            {
                Gizmos.DrawLine(
                    startPos,
                    startPos + Vector3.down * distanceToGround
                );
            }
        }

    }

    // ====================================================================================
    // INPUT & MOVEMENT LOGIC
    // ====================================================================================

    /// <summary>
    /// Moves the player based on input and applies forces relative to current <see cref="MovementState"/>.
    /// </summary>
    private void Move()
    {
        Vector3 moveValue = _move.ReadValue<Vector3>().normalized;
        Vector3 moveDirection = transform.TransformDirection(moveValue);
        Vector3 targetLinearVelocity = moveDirection * _speed;

        if (_sprint.IsPressed())
        {
            targetLinearVelocity *= _sprintSpeedScale;
        }

        if (_currentMovementState == MovementState.InAir)
        {
            Vector3 currentHorizontalVelocity = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
            Vector3 targetHorizontalVelocity = targetLinearVelocity * _inAirSpeedScale;
            Vector3 lerpedHorizontalVelocity = Vector3.Lerp(currentHorizontalVelocity, targetHorizontalVelocity, Time.deltaTime * _inAirSlowdownSmoothing);
            targetLinearVelocity = new Vector3(lerpedHorizontalVelocity.x, targetLinearVelocity.y, lerpedHorizontalVelocity.z);
        }

        targetLinearVelocity.y = _rb.linearVelocity.y;

        if (moveValue != Vector3.zero)
        {
            _rb.linearVelocity = targetLinearVelocity;
        }
    }

    /// <summary>
    /// Rotates the camera and player transform based on look input.
    /// </summary>
    private void Look()
    {
        Vector2 lookValue = _look.ReadValue<Vector2>();

        _cameraX += lookValue.x * _lookSensitivity;
        _cameraY += lookValue.y * _lookSensitivity;

        _cameraY = Mathf.Clamp(_cameraY, -90f, 90f);

        _camera.transform.rotation = Quaternion.Euler(
            _cameraY * -1,
            _cameraX,
            0
        );

        _rb.transform.rotation = Quaternion.Euler(
            _rb.transform.rotation.x,
            _cameraX,
            _rb.transform.rotation.z
        );
    }

    /// <summary>
    /// Makes the player jump and updates the <see cref="MovementState"/> to <see cref="MovementState.InAir"/>.
    /// </summary>
    private void Jump()
    {
        _currentMovementState = MovementState.InAir;
        Debug.Log($"[PLAYER] Updated movement state: {_currentMovementState}. -");
        _rb.AddForce(_rb.transform.up * _jumpPower, ForceMode.Impulse);
    }

    /// <summary>
    /// Attempts to interact with the current interactable object the player is looking at.
    /// </summary>
    private void Interact()
    {
        IInteractable interactable = _interaction.GetInteractable();

        if (interactable == null)
        {
            Debug.LogError("Player tried to interact but no valid interaction object could be found -");
            return;
        }

        interactable.Interact();
    }

    // ====================================================================================
    // HELPER METHODS (State & Collision)
    // ====================================================================================

    /// <summary>
    /// Updates the player's <see cref="MovementState"/>.
    /// </summary>
    /// <param name="state">New <see cref="MovementState"/> to apply.</param>
    private void SetMovementState(MovementState state)
    {
        _currentMovementState = state;
    }

    /// <summary>
    /// Calculates the distance between the player and the nearest 
    /// object from the <see cref="_resetJumpLayers"/> <see cref="LayerMask"/> on the y axis.
    /// </summary>
    /// <returns>Distance between the player and the nearest <see cref="_resetJumpLayers"/> object as float.</returns>
    private float GetDistanceToGround()
    {
        Physics.Raycast(
            _playerCollider.transform.position + (Vector3.down * (_playerColliderDimensions.y / 2)),
            _rb.transform.up * -1,
            out RaycastHit hitInfo,
            Mathf.Infinity,

            _resetJumpLayers);

        return Mathf.Max(0, Mathf.Abs(hitInfo.distance));
    }

    /// <summary>
    /// Returns all colliders under the player to detect ground contacts.
    /// </summary>
    /// <returns>Array of colliders detected under the player.</returns>
    private Collider[] GetGroundCollisionObjects()
    {
        Collider[] hitColliders = Physics.OverlapBox(
            new Vector3(
                transform.position.x,
                transform.position.y - _playerColliderDimensions.y / 2,
                transform.position.z
            ),
            Vector3.one * _overlapBoxScale,
            Quaternion.identity,
            _resetJumpLayers
        );
        return hitColliders;
    }
}
