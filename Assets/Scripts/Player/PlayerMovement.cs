using UnityEngine;

/// <summary>
/// Handles player movement, including grounded and in-air behavior,
/// sprinting, and jumping mechanics.
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
    /// Represents the current movement state of the player.
    /// </summary>
    public enum MovementState
    {
        Grounded,
        InAir
    }


    // ====================================================================================
    // PUBLIC METHODS
    // ====================================================================================

    /// <summary>
    /// Handles movement input and applies linearVelocity to the Rigidbody.
    /// Accounts for sprinting and in-air movement smoothing.
    /// </summary>
    public void Move()
    {
        // Get normalized input from PlayerController
        Vector3 moveValue = _playerController.Move.normalized;

        // Transform local input into world space
        Vector3 moveDirection = transform.TransformDirection(moveValue);
        Vector3 targetLinearVelocity = moveDirection * _playerController.Speed;

        // Apply sprint multiplier
        if (_playerController.IsSprinting)
        {
            targetLinearVelocity *= _playerController.SprintSpeedScale;
        }

        // Apply in-air smoothing and speed scaling
        if (_playerController.CurrentMovementState == MovementState.InAir)
        {
            Vector3 currentHorizontalVelocity = new Vector3(
                _playerController.Rigidbody.linearVelocity.x,
                0,
                _playerController.Rigidbody.linearVelocity.z
            );

            Vector3 targetHorizontalVelocity = targetLinearVelocity * _playerController.InAirSpeedScale;

            Vector3 lerpedHorizontalVelocity = Vector3.Lerp(
                currentHorizontalVelocity,
                targetHorizontalVelocity,
                Time.deltaTime * _playerController.InAirSlowdownSmoothing
            );

            targetLinearVelocity = new Vector3(
                lerpedHorizontalVelocity.x,
                targetLinearVelocity.y,
                lerpedHorizontalVelocity.z
            );
        }

        // Preserve vertical linearVelocity (gravity, jumping)
        targetLinearVelocity.y = _playerController.Rigidbody.linearVelocity.y;

        // Apply the calculated linearVelocity if there's input
        if (moveValue != Vector3.zero)
        {
            _playerController.Rigidbody.linearVelocity = targetLinearVelocity;
        }
    }

    /// <summary>
    /// Makes the player jump by applying an impulse force.
    /// Sets the movement state to InAir.
    /// </summary>
    public void Jump()
    {
        _playerController.CurrentMovementState = MovementState.InAir;
        Debug.Log($"[PLAYER] Updated movement state: {_playerController.CurrentMovementState}. -");

        _playerController.Rigidbody.AddForce(
            _playerController.Rigidbody.transform.up * _playerController.JumpPower,
            ForceMode.Impulse
        );
    }

    /// <summary>
    /// Sets the player's current movement state.
    /// </summary>
    /// <param name="state">The new movement state to apply.</param>
    public void SetMovementState(MovementState state)
    {
        _playerController.CurrentMovementState = state;
    }
}
