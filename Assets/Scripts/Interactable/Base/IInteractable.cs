using UnityEngine;

/// <summary>
/// Interface for objects that can be interacted with by the player.
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// Defines the interaction behavior for the implementing object.
    /// </summary>
    public void Interact();
}
