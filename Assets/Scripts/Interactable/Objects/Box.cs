using UnityEngine;

/// <summary>
/// Handles player interaction with a box.
/// Changes the box color randomly upon interaction.
/// Implements the IInteractable interface.
/// </summary>
public class Box : MonoBehaviour, IInteractable
{
    // ====================================================================================
    // PRIVATE FIELDS
    // ====================================================================================

    private MeshRenderer _meshRenderer;

    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    // ====================================================================================
    // PUBLIC METHODS — INTERACTION
    // ====================================================================================

    /// <summary>
    /// Called when the player interacts with this box.
    /// Randomizes the box's color and logs the interaction.
    /// </summary>
    public void Interact()
    {
        _meshRenderer.material.color = Random.ColorHSV();
        Debug.Log("[Box] Player interacted with the box.");
    }
}
