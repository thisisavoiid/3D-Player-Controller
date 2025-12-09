using UnityEngine;

public class Box : MonoBehaviour, IInteractable
{
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }
    public void Interact()
    {
        _meshRenderer.material.color = Random.ColorHSV();
        Debug.Log("Player interacted with a box interactable -");
    }
}
