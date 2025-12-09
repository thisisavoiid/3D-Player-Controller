using UnityEngine;

/// <summary>
/// Draws debug gizmos for the player in the Scene view.
/// Visualizes ground detection and OverlapBox area for collision checks.
/// </summary>
public class PlayerGizmos : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS
    // ====================================================================================

    [SerializeField] private PlayerController _playerController;


    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================

    /// <summary>
    /// Draws gizmos in the Scene view to help visualize player collision and ground checks.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (_playerController == null) return;

        // ----------------------------
        // Draw OverlapBox area (green)
        // ----------------------------
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
        Gizmos.color = Color.cyan;

        if (_playerController.PlayerCollider != null && _playerController.Rigidbody != null)
        {
            Vector3 startPos = _playerController.Rigidbody.transform.position +
                               Vector3.down * (_playerController.PlayerColliderDimensions.y / 2);

            float distanceToGround = _playerController.PlayerCollision.GetDistanceToGround();

            if (distanceToGround > 0f)
            {
                Gizmos.DrawLine(
                    startPos,
                    startPos + Vector3.down * distanceToGround
                );
            }
        }
    }
}
