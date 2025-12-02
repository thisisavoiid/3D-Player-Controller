using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS (Configuration / Settings)
    // ====================================================================================
    [SerializeField] private Color _interactColor;
    [SerializeField] private Color _baseColor;
    [SerializeField] private float _transitionDuration;

    // ====================================================================================
    // PRIVATE FIELDS (Components & State)
    // ====================================================================================
    private CrosshairState _state;
    private Image _crosshairImage;
    private IReadOnlyDictionary<CrosshairState, Color> _stateColorPairs;

    // ====================================================================================
    // ENUMS
    // ====================================================================================
    public enum CrosshairState
    {
        Base,
        CanInteract
    }

    // ====================================================================================
    // UNITY CALLBACKS
    // ====================================================================================
    private void Awake()
    {
        _stateColorPairs = new Dictionary<CrosshairState, Color>()
        {
            { CrosshairState.Base, _baseColor },
            { CrosshairState.CanInteract, _interactColor }
        };
    }

    private void Start()
    {
        _crosshairImage = GetComponent<Image>();
        _crosshairImage.color = _baseColor;
    }

    // ====================================================================================
    // PUBLIC METHODS
    // ====================================================================================

    /// <summary>
    /// Updates the crosshair to a new state and triggers a fade animation if the state changes.
    /// </summary>
    /// <param name="state">The target crosshair state.</param>
    public void SetCrosshairState(CrosshairState state)
    {
        if (state != _state)
        {
            StartCoroutine(FadeColorRoutine(state));
            _crosshairImage.color = _stateColorPairs[state];
        }
    }

    // ====================================================================================
    // PRIVATE METHODS
    // ====================================================================================

    /// <summary>
    /// Smoothly transitions the crosshair color from the current color to the target state's color over time.
    /// </summary>
    /// <param name="targetState">The target crosshair state to fade to.</param>
    private IEnumerator FadeColorRoutine(CrosshairState targetState)
    {
        float elapsedTime = 0f;
        Color currentColor = _crosshairImage.color;
        Color targetColor = _stateColorPairs[targetState];

        while (elapsedTime < _transitionDuration)
        {
            float normalizedTime = elapsedTime / _transitionDuration;
            _crosshairImage.color = Color.Lerp(currentColor, targetColor, normalizedTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        _crosshairImage.color = targetColor;
        _state = targetState;
    }
}
