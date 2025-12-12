using UnityEngine;

/// <summary>
/// Handles player collision and ground-detection logic.
/// Provides helper methods to measure distance to the ground
/// and detect ground objects directly beneath the player.
/// </summary>
public class PlayerCollision : MonoBehaviour
{
    // ====================================================================================
    // REFERENCES
    // ====================================================================================

    [SerializeField] private PlayerController _playerController;


    // ====================================================================================
    // PUBLIC METHODS — GROUND CHECKING
    // ====================================================================================

    /// <summary>
    /// Returns the distance from the player to the ground using a downward raycast.
    /// </summary>
    /// <returns>
    /// Non-negative float representing the distance to the nearest ground surface.
    /// Returns 0 if the ray hits immediately below.
    /// </returns>
    public float GetDistanceToGround()
    {
        // Raycast downward from the bottom of the player's collider
        Physics.Raycast(
            _playerController.PlayerCollider.transform.position +
            (Vector3.down * (_playerController.PlayerColliderDimensions.y / 2)),
            _playerController.Rigidbody.transform.up * -1,   // downward direction
            out RaycastHit hitInfo,
            Mathf.Infinity,                                  // infinite distance
            _playerController.ResetJumpLayers                // ground layers
        );

        // Return non-negative distance
        return Mathf.Max(0f, Mathf.Abs(hitInfo.distance));
    }

    /// <summary>
    /// Returns all colliders overlapping the player’s ground area.
    /// Uses an OverlapBox positioned under the player.
    /// </summary>
    /// <returns>Array of colliders detected under the player.</returns>
    public Collider[] GetGroundCollisionObjects()
    {
        // Box center positioned at bottom of player collider
        Vector3 boxCenter = new Vector3(
            transform.position.x,
            transform.position.y - _playerController.PlayerColliderDimensions.y / 2,
            transform.position.z
        );

        // Perform OverlapBox to detect ground colliders
        Collider[] hitColliders = Physics.OverlapBox(
            boxCenter,
            Vector3.one * _playerController.OverlapBoxScale,  // box half-size
            Quaternion.identity,
            _playerController.ResetJumpLayers
        );

        return hitColliders;
    }
}
