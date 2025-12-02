using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Player interacted with a door interactable -");
    }
}
