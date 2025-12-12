using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Monitors the player's vertical position and detects when the player has fallen
/// below the allowed Y-level (the "void"). When this happens, the current scene
/// is reloaded via the GameManager.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS
    // ====================================================================================

    /// <summary>
    /// Reference to the PlayerController to access Rigidbody position and kill height.
    /// Must be assigned in the Inspector.
    /// </summary>
    [SerializeField] private PlayerController _playerController;

    /// <summary>
    /// Reference to the GameManager used to reload the scene when the player dies.
    /// Must be assigned in the Inspector.
    /// </summary>
    [SerializeField] private GameManager _gameManager;

    // ====================================================================================
    // PROPERTIES
    // ====================================================================================

    /// <summary>
    /// True when the player's Y-position is below the kill threshold defined in PlayerController.
    /// </summary>
    private bool IsPlayerInsideVoid => _playerController.Rigidbody.transform.position.y <= _playerController.KillPlayerAfterHeight;

    // ====================================================================================
    // PRIVATE METHODS
    // ====================================================================================

    /// <summary>
    /// Handles the player's "death" when falling into the void.
    /// Currently reloads the active scene through the GameManager.
    /// </summary>
    private void PlayerEnteredVoidAction()
    {
        _gameManager.ReloadCurrentScene();
    }

    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================

    /// <summary>
    /// Checks each frame whether the player has fallen into the void,
    /// and executes the death action if so.
    /// </summary>
    private void Update()
    {
        if (IsPlayerInsideVoid)
        {
            PlayerEnteredVoidAction();
        }
    }
}
