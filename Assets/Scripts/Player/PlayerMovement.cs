using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Handles player movement, speed scaling, sprint behavior,
/// grounded vs. in-air movement, and triggers movement-related events.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS
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
    // EVENTS
    // ====================================================================================

    /// <summary>
    /// Invoked when the player is sprinting. Passes the sprint FOV.
    /// </summary>
    public event Action<float> OnPlayerSprint;

    /// <summary>
    /// Invoked when the player is walking. Passes the walking FOV.
    /// </summary>
    public event Action<float> OnPlayerWalk;

    /// <summary>
    /// Invoked when the player is idle. Passes the base FOV.
    /// </summary>
    public event Action<float> OnPlayerIdle;


    // ====================================================================================
    // PUBLIC METHODS
    // ====================================================================================

    /// <summary>
    /// Handles horizontal movement, sprinting, in-air control smoothing,
    /// and invokes movement-state events to adjust camera FOV.
    /// </summary>
    public void Move()
    {
        Vector3 input = _playerController.Move.normalized;

        // Convert input to world space
        Vector3 moveDirection = transform.TransformDirection(input);
        Vector3 targetVelocity = moveDirection * _playerController.Speed;

        // Apply sprint multiplier
        if (_playerController.IsSprinting)
        {
            targetVelocity *= _playerController.SprintSpeedScale;
        }

        // Apply in-air smoothing for horizontal movement
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

        // Preserve vertical velocity (gravity and jumps)
        targetVelocity.y = _playerController.Rigidbody.linearVelocity.y;


        // ==========================================================
        // APPLY MOVEMENT + INVOKE EVENTS
        // ==========================================================

        if (input != Vector3.zero)
        {
            _playerController.Rigidbody.linearVelocity = targetVelocity;

            if (_playerController.IsSprinting)
            {
                OnPlayerSprint?.Invoke(_playerController.SprintCameraFov);
            }
            else
            {
                OnPlayerWalk?.Invoke(_playerController.WalkCameraFov);
            }
        }
        else
        {
            OnPlayerIdle?.Invoke(_playerController.BaseCameraFov);
        }
    }


    /// <summary>
    /// Makes the player jump by applying an upward force
    /// and switching to the in-air state.
    /// </summary>
    public void Jump()
    {
        _playerController.CurrentMovementState = MovementState.InAir;

        _playerController.Rigidbody.AddForce(
            _playerController.Rigidbody.transform.up * _playerController.JumpPower,
            ForceMode.Impulse
        );

        Debug.Log($"[PLAYER] Updated movement state: {_playerController.CurrentMovementState}");
    }


    /// <summary>
    /// Updates the player's current movement state.
    /// </summary>
    public void SetMovementState(MovementState state)
    {
        _playerController.CurrentMovementState = state;
    }
}
