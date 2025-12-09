using UnityEngine;

/// <summary>
/// Handles the player's camera and body rotation based on mouse or controller look input.
/// Separates camera vertical rotation from player body horizontal rotation.
/// </summary>
public class PlayerLook : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS
    // ====================================================================================

    [SerializeField] private PlayerController _playerController;


    // ====================================================================================
    // PRIVATE FIELDS
    // ====================================================================================

    private float _cameraY; // Vertical rotation (pitch)
    private float _cameraX; // Horizontal rotation (yaw)


    // ====================================================================================
    // PUBLIC METHODS
    // ====================================================================================

    /// <summary>
    /// Rotates the player and camera based on input from PlayerController.
    /// Clamps vertical rotation to prevent flipping the camera.
    /// </summary>
    public void Look()
    {
        // Get look input from PlayerController (mouse/controller)
        Vector2 lookValue = _playerController.Look;

        // Apply sensitivity and accumulate rotation
        _cameraX += lookValue.x * _playerController.LookSensitivity;
        _cameraY += lookValue.y * _playerController.LookSensitivity;

        // Clamp vertical rotation to avoid over-rotation
        _cameraY = Mathf.Clamp(_cameraY, -90f, 90f);

        // Rotate the camera (pitch and yaw)
        _playerController.Camera.transform.rotation = Quaternion.Euler(
            -_cameraY,   // invert vertical input
            _cameraX,
            0
        );

        // Rotate the player body only around the Y-axis (yaw)
        _playerController.Rigidbody.transform.rotation = Quaternion.Euler(
            _playerController.Rigidbody.transform.rotation.eulerAngles.x, // keep current X
            _cameraX,
            _playerController.Rigidbody.transform.rotation.eulerAngles.z  // keep current Z
        );
    }
}
