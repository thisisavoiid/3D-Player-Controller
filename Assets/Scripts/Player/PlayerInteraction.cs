using UnityEngine;

/// <summary>
/// Handles player interaction with objects in the world.
/// Detects interactable objects in front of the player, updates the crosshair,
/// and manages interaction UI elements like labels.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS (Configuration / Settings)
    // ====================================================================================

    [SerializeField] private PlayerController _playerController;

    [Header("Interaction Settings")]
    [SerializeField] private CrosshairManager _crosshair;
    [SerializeField] private float _interactionReachDistance;
    [SerializeField] private LayerMask _interactableLayers;
    [SerializeField] private InteractLabel _interactionLabel;


    // ====================================================================================
    // PRIVATE FIELDS (State)
    // ====================================================================================

    /// <summary>
    /// Color for visualizing the raycast in the Scene view.
    /// Green = interactable detected, Red = nothing detected.
    /// </summary>
    private Color _debugRayColor = Color.red;


    // ====================================================================================
    // PUBLIC METHODS
    // ====================================================================================

    /// <summary>
    /// Returns the interactable object the player is currently looking at within reach.
    /// Updates crosshair state and interaction label based on detection.
    /// </summary>
    /// <returns>The <see cref="IInteractable"/> object if found, otherwise null.</returns>
    public IInteractable GetInteractable()
    {
        IInteractable interactable = null;
        _debugRayColor = Color.red;

        // Check if any interactable object is in sight
        if (CheckForInteractable(out RaycastHit hit))
        {
            // Try to get the IInteractable component from the hit object
            interactable = hit.collider.gameObject.GetComponent<IInteractable>();

            if (interactable != null)
            {
                _debugRayColor = Color.green; // Highlight ray in green
                _crosshair.SetCrosshairState(CrosshairManager.CrosshairState.CanInteract);
                _interactionLabel.Show();
            }
            else
            {
                // Reset crosshair and label if object is not interactable
                _crosshair.SetCrosshairState(CrosshairManager.CrosshairState.Base);
                _interactionLabel.Hide();
            }
        }
        else
        {
            // Reset crosshair and label if nothing is detected
            _crosshair.SetCrosshairState(CrosshairManager.CrosshairState.Base);
            _interactionLabel.Hide();
        }

        return interactable;
    }


    // ====================================================================================
    // PRIVATE METHODS
    // ====================================================================================

    /// <summary>
    /// Performs a raycast from the player's camera to detect objects on the interactable layers.
    /// </summary>
    /// <param name="hit">Outputs the RaycastHit information if an object is detected.</param>
    /// <returns>True if an object is detected in sight, false otherwise.</returns>
    private bool CheckForInteractable(out RaycastHit hit)
    {
        bool isObjectInSight = Physics.Raycast(
            _playerController.Camera.transform.position,
            _playerController.Camera.transform.forward,
            out RaycastHit raycastHit,
            _interactionReachDistance,
            _interactableLayers
        );

        hit = raycastHit;
        return isObjectInSight;
    }


    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================

    /// <summary>
    /// Draws debug ray in the Scene view to visualize interaction raycast.
    /// Green if interactable detected, red otherwise.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (Application.isPlaying && _playerController != null)
        {
            bool hasHitSomething = CheckForInteractable(out RaycastHit hit);

            Gizmos.color = _debugRayColor;
            Gizmos.DrawRay(
                _playerController.Camera.transform.position,
                _playerController.Camera.transform.forward *
                (hasHitSomething ? hit.distance : _interactionReachDistance)
            );
        }
    }

    /// <summary>
    /// Updates interactable detection each physics frame.
    /// </summary>
    private void FixedUpdate()
    {
        GetInteractable();
    }
}
