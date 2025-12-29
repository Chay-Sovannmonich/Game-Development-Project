using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class SpawnCompletionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject completionPanel; // The panel to show when spawning is complete
    [SerializeField] private Text messageText; // Optional text component
    [SerializeField] private Button continueButton; // Optional button to close panel

    [Header("Settings")]
    [SerializeField] private float showDelay = 1f; // Delay before showing panel after spawn
    [SerializeField] private string completionMessage = "All items have been spawned!"; // Custom message
    
    [Header("Game Control")]
    [SerializeField] private bool pauseGameOnShow = true; // Whether to pause the game when panel shows
    [SerializeField] private bool autoHideAfterTime = false; // Auto-hide panel after time
    [SerializeField] private float autoHideTime = 3f; // Time before auto-hiding

    // Events
    public event Action OnPanelClosed; // Event triggered when panel is closed
    public event Action OnPanelShown; // Event triggered when panel is shown

    private void Start()
    {
        // Ensure panel is hidden at start
        if (completionPanel != null)
        {
            completionPanel.SetActive(false);
        }

        // Set up button listener if button exists
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(HidePanel);
        }

        // Set message text if available
        if (messageText != null && !string.IsNullOrEmpty(completionMessage))
        {
            messageText.text = completionMessage;
        }
    }

    // Call this method from your SpawnManager after spawning is complete
    public void OnSpawnComplete()
    {
        StartCoroutine(ShowPanelAfterDelay(showDelay));
    }

    // Alternative method with custom delay
    public void OnSpawnCompleteWithDelay(float customDelay)
    {
        StartCoroutine(ShowPanelAfterDelay(customDelay));
    }

    private IEnumerator ShowPanelAfterDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Show the panel
        ShowPanel();

        // Set up auto-hide if enabled
        if (autoHideAfterTime)
        {
            StartCoroutine(AutoHidePanel());
        }
    }

    private void ShowPanel()
    {
        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
            Debug.Log("SpawnCompletionPanel: Showing completion panel");

            // Trigger OnPanelShown event
            OnPanelShown?.Invoke();

            // Pause the game if configured
            if (pauseGameOnShow)
            {
                Time.timeScale = 0f;
            }

            // Optional: Play sound effect
            // AudioManager.Instance.PlaySFX("SpawnComplete");
        }
        else
        {
            Debug.LogWarning("SpawnCompletionPanel: No completion panel assigned!");
        }
    }

    public void HidePanel()
    {
        if (completionPanel != null && completionPanel.activeSelf)
        {
            completionPanel.SetActive(false);
            Debug.Log("SpawnCompletionPanel: Hiding completion panel");

            // Trigger OnPanelClosed event
            OnPanelClosed?.Invoke();

            // Resume the game
            Time.timeScale = 1f;

            // Optional: Play sound effect
            // AudioManager.Instance.PlaySFX("ButtonClick");
        }
    }

    private IEnumerator AutoHidePanel()
    {
        yield return new WaitForSecondsRealtime(autoHideTime); // Using realtime since game might be paused
        HidePanel();
    }

    // Optional: You can call this manually if needed
    public void TriggerPanel()
    {
        OnSpawnComplete();
    }

    // Update the message text dynamically
    public void UpdateMessage(string newMessage)
    {
        completionMessage = newMessage;
        if (messageText != null)
        {
            messageText.text = completionMessage;
        }
    }

    // Get the show delay (for SpawnManager to use)
    public float GetShowDelay()
    {
        return showDelay;
    }

    // Set a new show delay
    public void SetShowDelay(float newDelay)
    {
        showDelay = newDelay;
    }

    // Check if panel is currently visible
    public bool IsPanelVisible()
    {
        return completionPanel != null && completionPanel.activeSelf;
    }

    // Clean up
    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HidePanel);
        }
    }
}