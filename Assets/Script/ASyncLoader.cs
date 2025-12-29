using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ASyncLoader : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject mainMenu;

    [Header("Loading Screen Elements")]
    [SerializeField] private Image loadingImage; // Drag your image component here
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Text progressText;
    [SerializeField] private Text loadingTipText; // Optional: for showing loading tips

    [Header("Loading Settings")]
    [SerializeField] private float minimumLoadTime = 2f; // Minimum time to show loading screen
    [SerializeField] private string[] loadingTips; // Optional array of loading tips

    private void Start()
    {
        // Ensure loading screen is hidden initially
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
        
        // Ensure main menu is visible initially
        if (mainMenu != null)
            mainMenu.SetActive(true);
    }

    public void LoadLevel(string levelName)
    {
        StartCoroutine(LoadAsynchronously(levelName));
    }

    IEnumerator LoadAsynchronously(string levelName)
    {
        // Show loading screen and hide main menu
        if (mainMenu != null)
            mainMenu.SetActive(false);
        
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // Optional: Show a random loading tip
        if (loadingTipText != null && loadingTips != null && loadingTips.Length > 0)
        {
            int randomTip = Random.Range(0, loadingTips.Length);
            loadingTipText.text = loadingTips[randomTip];
        }

        // Record start time for minimum load time
        float startTime = Time.time;

        // Start loading the scene
        AsyncOperation operation = SceneManager.LoadSceneAsync(levelName);
        operation.allowSceneActivation = false;

        // Set initial values
        float progress = 0f;

        while (!operation.isDone)
        {
            // Calculate progress (0 to 0.9 for loading, then 0.9 to 1.0 for activation)
            progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Update UI elements
            if (loadingSlider != null)
                loadingSlider.value = progress;
            
            if (progressText != null)
                progressText.text = Mathf.Round(progress * 100f) + "%";

            // Check if loading is complete (progress will be 0.9 when ready)
            if (operation.progress >= 0.9f)
            {
                // Wait for minimum load time
                while (Time.time - startTime < minimumLoadTime)
                {
                    // Optional: Show "Press any key to continue" or similar
                    if (progressText != null)
                        progressText.text = "Press Space to Continue";
                    
                    // Allow player to trigger scene activation
                    if (Input.GetKeyDown(KeyCode.Space))
                        break;
                    
                    yield return null;
                }

                // Allow scene activation
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    // Optional: Add a method to preload scenes
    public void PreloadLevel(string levelName)
    {
        StartCoroutine(PreloadScene(levelName));
    }

    IEnumerator PreloadScene(string levelName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(levelName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log("Preloading progress: " + (progress * 100) + "%");
            
            if (operation.progress >= 0.9f)
            {
                Debug.Log("Scene preloaded and ready");
                break;
            }
            
            yield return null;
        }
    }
}