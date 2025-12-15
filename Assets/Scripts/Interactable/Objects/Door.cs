using UnityEngine;

/// <summary>
/// Handles player interaction with a door.
/// Toggles door state and animator when interacted with.
/// Implements the IInteractable interface.
/// </summary>
public class Door : MonoBehaviour, IInteractable
{
    // ====================================================================================
    // INSPECTOR FIELDS
    // ====================================================================================

    [SerializeField] private Animator _animator;

    // ====================================================================================
    // PRIVATE FIELDS
    // ====================================================================================

    private DoorState _currDoorState;

    /// <summary>
    /// Represents the current state of the door.
    /// </summary>
    private enum DoorState
    {
        Opened,
        Closed
    }

    // ====================================================================================
    // PUBLIC METHODS — INTERACTION
    // ====================================================================================

    /// <summary>
    /// Toggles the door between opened and closed states.
    /// Updates the animator accordingly.
    /// </summary>
    public void Interact()
    {
        Debug.Log("[DOOR] Player interacted with the door -");
        switch (_currDoorState)
        {
            case DoorState.Opened:
                _animator.SetBool("isDoorOpen", false);
                _currDoorState = DoorState.Closed;
                break;

            case DoorState.Closed:
                _animator.SetBool("isDoorOpen", true);
                _currDoorState = DoorState.Opened;
                break;
        }
        Debug.Log($"[DOOR] New door state: {_currDoorState} -");
    }
}
