using UnityEngine;

public class ShowWhenCameraSwitched : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraSwitchButton cameraSwitchButton; // ⬅️ DRAG CAMERA SWITCH BUTTON SCRIPT HERE
    [SerializeField] private GameObject objectToShow; // ⬅️ DRAG OBJECT TO SHOW/HERE HERE (can be this object)
    
    [Header("Settings")]
    [SerializeField] private bool showOnCamera2 = true; // Show when Camera2 is active
    [SerializeField] private bool hideOnCamera2 = false; // Hide when Camera2 is active (alternative)
    [SerializeField] private bool useThisObject = true; // Use this GameObject if no object assigned
    
    void Start()
    {
        // If no object assigned and we want to use this object
        if (objectToShow == null && useThisObject)
        {
            objectToShow = this.gameObject;
        }
        
        // If still no object, log error
        if (objectToShow == null)
        {
            Debug.LogError("No object assigned to show/hide!");
            return;
        }
        
        // Subscribe to the camera switch event
        if (cameraSwitchButton != null)
        {
            cameraSwitchButton.onSwitchedToCamera2 += OnCameraSwitched;
            Debug.Log($"Subscribed to camera switch events. Object will {(showOnCamera2 ? "show" : "hide")} on Camera2");
        }
        else
        {
            Debug.LogError("CameraSwitchButton not assigned!");
        }
        
        // Set initial visibility based on current camera
        UpdateVisibility();
    }
    
    void OnCameraSwitched()
    {
        Debug.Log("Camera switch detected! Updating object visibility...");
        UpdateVisibility();
    }
    
    void UpdateVisibility()
    {
        if (cameraSwitchButton == null || objectToShow == null) return;
        
        bool isCamera2Active = cameraSwitchButton.IsCamera2Active();
        
        if (showOnCamera2)
        {
            // Show when Camera2 is active, hide when not
            objectToShow.SetActive(isCamera2Active);
            Debug.Log($"Object {objectToShow.name} set to: {(isCamera2Active ? "SHOWN" : "HIDDEN")} (showOnCamera2 mode)");
        }
        else if (hideOnCamera2)
        {
            // Hide when Camera2 is active, show when not
            objectToShow.SetActive(!isCamera2Active);
            Debug.Log($"Object {objectToShow.name} set to: {(!isCamera2Active ? "SHOWN" : "HIDDEN")} (hideOnCamera2 mode)");
        }
    }
    
    // Optional: Check visibility every frame (useful for debugging)
    void Update()
    {
        // Debug: Press F5 to check current state
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log($"=== VISIBILITY DEBUG ===");
            Debug.Log($"Camera2 Active: {cameraSwitchButton?.IsCamera2Active()}");
            Debug.Log($"Object Active: {objectToShow?.activeSelf}");
            Debug.Log($"Mode: {(showOnCamera2 ? "Show on Camera2" : hideOnCamera2 ? "Hide on Camera2" : "Manual")}");
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        if (cameraSwitchButton != null)
        {
            cameraSwitchButton.onSwitchedToCamera2 -= OnCameraSwitched;
        }
    }
}