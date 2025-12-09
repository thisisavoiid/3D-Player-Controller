using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator _animator;

    private DoorState _currDoorState;

    private enum DoorState
    {
        Opened,
        Closed
    }

    public void Interact()
    {
        switch (_currDoorState)
        {
            case DoorState.Opened:
                {
                    _animator.SetBool("isDoorOpen", false);
                    _currDoorState = DoorState.Closed;
                    break;
                }
            case DoorState.Closed:
                {
                    _animator.SetBool("isDoorOpen", true);
                    _currDoorState = DoorState.Opened;
                    break;
                }
        }

    }
}
