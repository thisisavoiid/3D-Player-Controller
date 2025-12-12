using System;
using UnityEngine;

/// <summary>
/// Handles the player's camera rotation, player-body rotation,
/// and dynamic camera FOV changes based on movement events.
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

    private float _cameraY;        // Vertical rotation
    private float _cameraX;        // Horizontal rotation
    private float _cameraFOV;      // Target camera field of view


    // ====================================================================================
    // UNITY LIFECYCLE
    // ====================================================================================

    private void Start()
    {
        // Initialize FOV to the player's default setting
        _cameraFOV = _playerController.BaseCameraFov;

        // Subscribe to movement events (walk, sprint, idle)
        _playerController.PlayerMovement.OnPlayerWalk += ChangeCameraFov;
        _playerController.PlayerMovement.OnPlayerSprint += ChangeCameraFov;
        _playerController.PlayerMovement.OnPlayerIdle += ChangeCameraFov;
    }


    // ====================================================================================
    // PUBLIC METHODS
    // ====================================================================================

    /// <summary>
    /// Handles camera rotation, player-body rotation,
    /// and smoothly transitions FOV when enabled.
    /// </summary>
    public void Look()
    {
        Vector2 lookValue = _playerController.Look;

        // Apply sensitivity-scaled input
        _cameraX += lookValue.x * _playerController.LookSensitivity;
        _cameraY += lookValue.y * _playerController.LookSensitivity;

        // Clamp vertical rotation
        _cameraY = Mathf.Clamp(_cameraY, -90f, 90f);

        // Rotate camera (local rotation)
        _playerController.Camera.transform.rotation = Quaternion.Euler(
            -_cameraY,               // invert vertical input
            _cameraX,
            0f
        );

        // Rotate player body on Y-axis only
        _playerController.Rigidbody.transform.rotation = Quaternion.Euler(
            _playerController.Rigidbody.transform.rotation.eulerAngles.x,
            _cameraX,
            _playerController.Rigidbody.transform.rotation.eulerAngles.z
        );

        // Smoothly lerp camera FOV if enabled
        if (_playerController.ChangeCameraFovWithMovement)
        {
            float smoothedFov = Mathf.Lerp(
                _playerController.Camera.fieldOfView,
                _cameraFOV,
                Time.deltaTime * _playerController.CameraFovChangeSmoothingFactor
            );

            _playerController.Camera.fieldOfView = smoothedFov;
        }
        else if (_cameraFOV != _playerController.BaseCameraFov)
        {
            // Reset FOV when feature is disabled
            _cameraFOV = _playerController.BaseCameraFov;
        }
    }


    // ====================================================================================
    // PRIVATE METHODS
    // ====================================================================================

    /// <summary>
    /// Updates the target camera FOV when movement state changes.
    /// </summary>
    private void ChangeCameraFov(float fov)
    {
        _cameraFOV = fov;
    }
}
