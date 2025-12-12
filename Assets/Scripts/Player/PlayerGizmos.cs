using UnityEngine;

/// <summary>
/// Draws debug gizmos for the player in the Scene view.
/// Visualizes the player's collision box, ground detection ray, and a kill-height marker.
/// Helps developers debug physics, ground checks, and player boundaries.
/// </summary>
public class PlayerGizmos : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS
    // ====================================================================================

    /// <summary>
    /// Reference to the PlayerController script that holds player data and collision info.
    /// Required to access player collider dimensions, Rigidbody, and collision methods.
    /// </summary>
    [SerializeField] private PlayerController _playerController;


    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================

    /// <summary>
    /// Draws gizmos in the Scene view.
    /// Shows the OverlapBox area for collision detection, the ground ray, and the kill-zone marker.
    /// This method is called by Unity automatically in the editor when the object is selected.
    /// </summary>
    private void OnDrawGizmos()
    {
        // Ensure we have a PlayerController reference
        if (_playerController == null) return;

        // ----------------------------
        // Draw OverlapBox area (green)
        // ----------------------------
        // Visualizes the player's collision detection area for ground and object overlaps.
        Gizmos.color = Color.green;
        Vector3 boxCenter = new Vector3(
            transform.position.x,
            transform.position.y - _playerController.PlayerColliderDimensions.y / 2,
            transform.position.z
        );
        Vector3 boxSize = Vector3.one * _playerController.OverlapBoxScale;

        Gizmos.DrawWireCube(boxCenter, boxSize);

        // ---------------------------------------------
        // Draw ray to ground (cyan)
        // ---------------------------------------------
        // Visualizes the distance from the player's feet to the ground.
        if (_playerController.PlayerCollider != null && _playerController.Rigidbody != null)
        {
            Gizmos.color = Color.cyan;

            Vector3 startPos = _playerController.Rigidbody.transform.position +
                               Vector3.down * (_playerController.PlayerColliderDimensions.y / 2); // Start at player's feet

            float distanceToGround = _playerController.PlayerCollision.GetDistanceToGround();

            // Draw the line only if there is some distance to the ground
            if (distanceToGround > 0f)
            {
                Gizmos.DrawLine(
                    startPos,
                    startPos + Vector3.down * distanceToGround
                );
            }

            // ---------------------------------------------
            // Draw kill-zone marker (red)
            // ---------------------------------------------
            // Visualizes the Y-position where the player will be killed if fallen below it.
            Gizmos.color = Color.red;

            Gizmos.DrawWireCube(
                new Vector3(
                    _playerController.Rigidbody.transform.position.x,
                    _playerController.KillPlayerAfterHeight,
                    _playerController.Rigidbody.transform.position.z
                ),
                Vector3.one / 2
            );
        }
    }
}
