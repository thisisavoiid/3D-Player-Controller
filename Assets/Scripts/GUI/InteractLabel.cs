using UnityEngine;

public class InteractLabel : MonoBehaviour
{
    /// <summary>
    /// Shows the interaction label by activating its GameObject.
    /// </summary>
    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// Hides the interaction label by deactivating its GameObject.
    /// </summary>
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
