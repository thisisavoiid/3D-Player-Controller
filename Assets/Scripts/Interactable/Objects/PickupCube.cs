using System;
using UnityEngine;

/// <summary>
/// Handles player interaction with a pickup cube.
/// Supports picking up, carrying, following the player, and throwing.
/// Implements the IInteractable interface.
/// </summary>
public class PickupCube : MonoBehaviour, IInteractable
{
    // ====================================================================================
    // PRIVATE FIELDS
    // ====================================================================================

    private bool _isPickedUp = false;
    private bool _shouldThrow = false;
    private Rigidbody _rigidbody;
    private Collider _collider;

    // ====================================================================================
    // INSPECTOR FIELDS
    // ====================================================================================

    [Header("Object References")]
    [SerializeField] private Transform _pickupObjectPosPlaceholder;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private PlayerController _playerController;

    [Header("Pickup Settings")]
    [SerializeField] private float _throwForce = 5.0f;
    [SerializeField] private float _posFollowSmoothing = 10.0f;

    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (_isPickedUp)
        {
            SetPosition(_pickupObjectPosPlaceholder.position);
        }
    }

    private void FixedUpdate()
    {
        if (_shouldThrow)
        {
            _shouldThrow = false;
            Throw(_throwForce + _playerController.Rigidbody.linearVelocity.magnitude);
        }
    }

    private void OnDrawGizmos()
    {
        if (_rigidbody != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(_rigidbody.transform.position, transform.localScale);
        }
    }

    // ====================================================================================
    // PUBLIC METHODS — INTERACTION
    // ====================================================================================

    /// <summary>
    /// Toggles pickup state when player interacts with the cube.
    /// </summary>
    public void Interact()
    {
        TogglePickupState();
    }

    // ====================================================================================
    // PRIVATE METHODS — PICKUP LOGIC
    // ====================================================================================

    /// <summary>
    /// Toggles whether the cube is picked up or dropped.
    /// Prevents dropping if the cube would overlap other colliders.
    /// </summary>
    private void TogglePickupState()
    {
        if (_isPickedUp)
        {
            // Check for collisions before dropping
            Collider[] overlappingColliders = Physics.OverlapBox(_rigidbody.transform.position, transform.localScale / 2, Quaternion.identity);
            if (overlappingColliders.Length != 0)
            {
                if (!(overlappingColliders.Length == 1 && overlappingColliders[0] == _collider))
                {
                    Debug.Log("[PickupCube] Cannot drop object here, overlapping with other colliders.");
                    return;
                }
            }
        }

        _isPickedUp = !_isPickedUp;
        _rigidbody.isKinematic = _isPickedUp;
        _shouldThrow = !_isPickedUp;
    }

    /// <summary>
    /// Applies an impulse force to throw the cube forward.
    /// </summary>
    private void Throw(float force)
    {
        Debug.Log($"[PickupCube] Throwing cube with force: {force}");
        _rigidbody.AddForce(_playerTransform.forward * _throwForce, ForceMode.Impulse);
    }

    /// <summary>
    /// Smoothly moves the cube to the target position.
    /// </summary>
    private void SetPosition(Vector3 targetPos)
    {
        Vector3 currPos = _rigidbody.transform.position;
        _rigidbody.transform.position = Vector3.Lerp(currPos, targetPos, Time.fixedDeltaTime * _posFollowSmoothing);
    }
}
