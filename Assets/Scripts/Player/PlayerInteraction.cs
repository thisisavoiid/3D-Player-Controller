using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS (Configuration / Settings)
    // ====================================================================================
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private CrosshairManager _crosshair;
    [SerializeField] private float _interactionReachDistance;
    [SerializeField] private LayerMask _interactableLayers;
    [SerializeField] private InteractLabel _interactionLabel;

    // ====================================================================================
    // PRIVATE FIELDS (State)
    // ====================================================================================
    private Color _debugRayColor = Color.red;

    // ====================================================================================
    // PUBLIC METHODS
    // ====================================================================================

    /// <summary>
    /// Returns the interactable object the player is currently looking at within reach.
    /// Updates crosshair state based on whether an interactable is detected.
    /// </summary>
    /// <returns>The <see cref="IInteractable"/> object if found, otherwise null.</returns>
    public IInteractable GetInteractable()
    {
        IInteractable interactable = null;
        _debugRayColor = Color.red;

        if (CheckForInteractable(out RaycastHit hit))
        {
            // Try to get the IInteractable component from the object
            interactable = hit.collider.gameObject.GetComponent<IInteractable>();
            if (interactable != null)
            {
                _debugRayColor = Color.green;
                _crosshair.SetCrosshairState(CrosshairManager.CrosshairState.CanInteract);
                _interactionLabel.Show();
            }
            else
            {
                _crosshair.SetCrosshairState(CrosshairManager.CrosshairState.Base);
                _interactionLabel.Hide();
            }
        }
        else
        {
            _crosshair.SetCrosshairState(CrosshairManager.CrosshairState.Base);
            _interactionLabel.Hide();
        }

        return interactable;
    }

    // ====================================================================================
    // PRIVATE METHODS
    // ====================================================================================

    /// <summary>
    /// Performs a raycast to check if an object on the interactable layer is in the player's sight.
    /// </summary>
    /// <param name="hit">Outputs the RaycastHit information if an object is detected.</param>
    /// <returns>True if an object is in sight, false otherwise.</returns>
    private bool CheckForInteractable(out RaycastHit hit)
    {
        bool isObjectInSight = Physics.Raycast(
            _playerCamera.transform.position,
            _playerCamera.transform.forward,
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
    private void OnDrawGizmos()
    {
        bool hasHitSomething = CheckForInteractable(out RaycastHit hit);

        Gizmos.color = _debugRayColor;
        Gizmos.DrawRay(
            _playerCamera.transform.position,
            _playerCamera.transform.forward * (hasHitSomething ? hit.distance : _interactionReachDistance)
        );
    }

    private void FixedUpdate()
    {
        GetInteractable();
    }
}
