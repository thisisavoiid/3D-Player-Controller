using System;
using UnityEngine;

/// <summary>
/// Handles player movement, speed scaling, sprint behavior,
/// grounded vs. in-air movement, crouching, and triggers movement-related events.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR / DEPENDENCIES
    // ====================================================================================

    [SerializeField] private PlayerController _playerController;

    // ====================================================================================
    // PUBLIC TYPES
    // ====================================================================================

    /// <summary>
    /// Represents the player's current grounded or in-air state.
    /// </summary>
    public enum MovementState
    {
        Grounded,
        InAir
    }

    // ====================================================================================
    // PROPERTIES & PRIVATE FIELDS
    // ====================================================================================

    private bool _isCrouching = false;

    /// <summary>
    /// Returns true if the player is currently crouching.
    /// </summary>
    public bool IsCrouching => _isCrouching;

    /// <summary>
    /// Returns true if the player is moving forward relative to their facing direction.
    /// Used for sprint restrictions.
    /// </summary>
    public bool IsMovingForward => IsForwardMovement(
        transform.TransformDirection(_playerController.Move.normalized)
    );

    // ====================================================================================
    // EVENTS
    // ====================================================================================

    public event Action<float> OnPlayerSprint;
    public event Action<float> OnPlayerWalk;
    public event Action<float> OnPlayerIdle;

    // ====================================================================================
    // PUBLIC METHODS — MOVEMENT
    // ====================================================================================

    /// <summary>
    /// Handles horizontal movement, sprinting, crouch speed scaling,
    /// in-air smoothing, and invokes movement-state events to adjust camera FOV.
    /// </summary>
    public void Move()
    {
        Vector3 input = _playerController.Move.normalized;

        // Convert input to world space
        Vector3 moveDirection = transform.TransformDirection(input);
        Vector3 targetVelocity = moveDirection * _playerController.Speed;

        // Apply sprint multiplier
        if (_playerController.IsSprintPressed && !_isCrouching && IsMovingForward)
        {
            targetVelocity *= _playerController.SprintSpeedScale;
        }

        // Apply crouch multiplier
        if (_isCrouching)
        {
            targetVelocity *= _playerController.CrouchSpeedScale;
        }

        // In-air smoothing
        if (_playerController.CurrentMovementState == MovementState.InAir)
        {
            Vector3 currentVel = new Vector3(
                _playerController.Rigidbody.linearVelocity.x,
                0f,
                _playerController.Rigidbody.linearVelocity.z
            );

            Vector3 targetHorizontal = targetVelocity * _playerController.InAirSpeedScale;

            Vector3 smoothedHorizontal = Vector3.Lerp(
                currentVel,
                targetHorizontal,
                Time.deltaTime * _playerController.InAirSlowdownSmoothing
            );

            targetVelocity = new Vector3(
                smoothedHorizontal.x,
                targetVelocity.y,
                smoothedHorizontal.z
            );
        }

        // Preserve vertical velocity
        targetVelocity.y = _playerController.Rigidbody.linearVelocity.y;

        // ==========================================================
        // APPLY MOVEMENT + INVOKE EVENTS
        // ==========================================================

        if (input != Vector3.zero)
        {
            _playerController.Rigidbody.linearVelocity = targetVelocity;

            if (_playerController.IsSprintPressed && !_isCrouching && IsMovingForward)
                OnPlayerSprint?.Invoke(_playerController.SprintCameraFov);
            else
                OnPlayerWalk?.Invoke(_playerController.WalkCameraFov);
        }
        else
        {
            OnPlayerIdle?.Invoke(_playerController.BaseCameraFov);
        }
    }

    /// <summary>
    /// Toggles crouch state and adjusts camera position.
    /// </summary>
    public void ToggleCrouch()
    {
        _isCrouching = !_isCrouching;

        Vector3 currCameraPos = _playerController.Camera.transform.position;

        _playerController.Camera.transform.position = _isCrouching
            ? currCameraPos + Vector3.down * -_playerController.VerticalCrouchOffset
            : currCameraPos + Vector3.down * _playerController.VerticalCrouchOffset;

        Debug.Log($"[PLAYER] Crouch toggled. IsCrouching: {_isCrouching}");
    }

    /// <summary>
    /// Makes the player jump by applying an upward impulse
    /// and switching to the in-air state.
    /// </summary>
    public void Jump()
    {
        SetMovementState(MovementState.InAir);

        _playerController.Rigidbody.AddForce(
            _playerController.Rigidbody.transform.up * _playerController.JumpPower,
            ForceMode.Impulse
        );
    }

    /// <summary>
    /// Updates the player's current movement state.
    /// </summary>
    public void SetMovementState(MovementState state)
    {
        _playerController.CurrentMovementState = state;
        Debug.Log($"[PLAYER] Updated movement state: {_playerController.CurrentMovementState}");
    }

    // ====================================================================================
    // PRIVATE METHODS — HELPERS
    // ====================================================================================

    /// <summary>
    /// Checks if the input movement direction is roughly forward relative to the player.
    /// </summary>
    private bool IsForwardMovement(Vector3 direction)
    {
        return Vector3.Dot(_playerController.Rigidbody.transform.forward, direction) >= 0f;
    }
}
