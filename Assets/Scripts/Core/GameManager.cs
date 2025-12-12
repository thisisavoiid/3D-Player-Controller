using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles scene loading and navigation within the game, with optional transitions.
/// Provides methods to load specific scenes, reload the current scene,
/// and navigate to the next or previous scenes.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ====================================================================================
    // INSPECTOR FIELDS
    // ====================================================================================

    [SerializeField] private Animator _transitionAnimator;
    [SerializeField] private float _sceneLoadTransitionDelay = 2.0f;

    // Tracks whether a scene load is already queued to prevent double-loading
    private bool _isSceneLoadQueued = false;

    // ====================================================================================
    // PUBLIC METHODS — SCENE MANAGEMENT
    // ====================================================================================

    /// <summary>
    /// Loads a scene by its build index with a transition.
    /// </summary>
    /// <param name="sceneIndex">Index of the scene in Build Settings.</param>
    /// <param name="mode">Scene loading mode (Single or Additive).</param>
    public void LoadScene(int sceneIndex, LoadSceneMode mode = LoadSceneMode.Single)
    {
        if (_isSceneLoadQueued) return;

        _isSceneLoadQueued = true;

        int totalSceneCount = GetTotalScenesCount();

        if (sceneIndex < 0 || sceneIndex > totalSceneCount - 1)
        {
            Debug.LogError($"[{nameof(GameManager)}] Couldn't load target scene: index {sceneIndex} is out of bounds.");
            _isSceneLoadQueued = false;
            return;
        }

        // Trigger the scene transition animation
        _transitionAnimator?.SetTrigger("Trigger");

        // Start the delayed scene load
        StartCoroutine(LoadSceneWithTransition(sceneIndex, mode));

        Debug.Log($"[{nameof(GameManager)}] Queued scene load {sceneIndex} ({SceneManager.GetSceneByBuildIndex(sceneIndex).name}).");
    }

    /// <summary>
    /// Coroutine to handle delayed scene loading to allow transition animation to play.
    /// </summary>
    private IEnumerator LoadSceneWithTransition(int sceneIndex, LoadSceneMode mode)
    {
        yield return new WaitForSeconds(_sceneLoadTransitionDelay);

        SceneManager.LoadScene(sceneBuildIndex: sceneIndex, mode: mode);

        _isSceneLoadQueued = false;
    }

    /// <summary>
    /// Reloads the currently active scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        LoadScene(sceneIndex: currentSceneIndex, mode: LoadSceneMode.Single);
    }

    /// <summary>
    /// Loads the previous scene in build order, if it exists.
    /// </summary>
    public void LoadPreviousScene(LoadSceneMode mode = LoadSceneMode.Single)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        LoadScene(sceneIndex: currentSceneIndex - 1, mode: mode);
    }

    /// <summary>
    /// Loads the next scene in build order, if it exists.
    /// </summary>
    public void LoadNextScene(LoadSceneMode mode = LoadSceneMode.Single)
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        LoadScene(sceneIndex: currentSceneIndex + 1, mode: mode);
    }

    /// <summary>
    /// Returns the total number of scenes included in Build Settings.
    /// </summary>
    public int GetTotalScenesCount() => SceneManager.sceneCountInBuildSettings;
}
