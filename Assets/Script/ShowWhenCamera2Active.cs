using UnityEngine;

public class ShowWhenCamera2Active : MonoBehaviour
{
    [Header("=== REFERENCES ===")]
    [Tooltip("Drag your Camera Switch Button script here")]
    public CameraSwitchButton cameraSwitchButton; // ⬅️ ASSIGN IN INSPECTOR
    
    [Header("=== OPTIONS ===")]
    [Tooltip("Should this object disappear when Camera2 is not active?")]
    public bool hideWhenNotCamera2 = true;
    
    [Tooltip("Check to show debug messages")]
    public bool showDebug = true;
    
    private void Start()
    {
        // Try to find CameraSwitchButton if not assigned
        if (cameraSwitchButton == null)
        {
            cameraSwitchButton = FindObjectOfType<CameraSwitchButton>();
            
            if (cameraSwitchButton == null)
            {
                Debug.LogError("ShowWhenCamera2Active: No CameraSwitchButton found! Please assign one.");
                return;
            }
        }
        
        // Subscribe to the camera switch event
        cameraSwitchButton.onSwitchedToCamera2 += OnSwitchedToCamera2;
        
        // Set initial visibility based on camera state
        UpdateVisibility();
        
        if (showDebug)
        {
            Debug.Log($"ShowWhenCamera2Active: Object '{gameObject.name}' initialized. Initially visible: {gameObject.activeSelf}");
        }
    }
    
    private void OnSwitchedToCamera2()
    {
        if (showDebug) Debug.Log($"ShowWhenCamera2Active: Camera2 switched event received!");
        
        // When Camera2 is activated, show this object
        if (hideWhenNotCamera2)
        {
            gameObject.SetActive(true);
            if (showDebug) Debug.Log($"ShowWhenCamera2Active: Made '{gameObject.name}' visible");
        }
    }
    
    private void Update()
    {
        // Optional: Continuously check camera state
        // This is more reliable than just responding to events
        UpdateVisibility();
    }
    
    private void UpdateVisibility()
    {
        if (cameraSwitchButton == null) return;
        
        bool shouldBeVisible = cameraSwitchButton.IsCamera2Active();
        
        // Only change if needed
        if (gameObject.activeSelf != shouldBeVisible)
        {
            gameObject.SetActive(shouldBeVisible);
            
            if (showDebug)
            {
                Debug.Log($"ShowWhenCamera2Active: Set '{gameObject.name}' visibility to {shouldBeVisible}");
            }
        }
    }
    
    private void OnDestroy()
    {
        // Clean up event subscription
        if (cameraSwitchButton != null)
        {
            cameraSwitchButton.onSwitchedToCamera2 -= OnSwitchedToCamera2;
        }
    }
}