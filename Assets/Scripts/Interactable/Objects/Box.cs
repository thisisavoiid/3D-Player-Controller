using UnityEngine;

public class Box : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Player interacted with a box interactable -");
    }
}
