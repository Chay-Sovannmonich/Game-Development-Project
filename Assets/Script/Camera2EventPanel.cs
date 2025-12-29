using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Camera2EventPanel : MonoBehaviour
{
    [Header("=== PANEL SETTINGS ===")]
    public float displayTime = 1f;
    public bool fadeInOut = true;
    public float fadeSpeed = 2f;
    
    [Header("=== CAMERA REFERENCE ===")]
    public Camera targetCamera;
    
    [Header("=== UI ELEMENTS ===")]
    public GameObject panelObject;
    public Text messageText;
    public string displayMessage = "Camera 2 Active!";
    
    private CanvasGroup canvasGroup;
    private Coroutine currentCoroutine;
    private bool hasShownOnce = false; // NEW: Track if panel has shown before
    private bool isListening = false;
    
    void Start()
    {
        Debug.Log("Camera2EventPanel initializing...");
        
        // Get or add CanvasGroup for fade effects
        if (panelObject != null)
        {
            canvasGroup = panelObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null && fadeInOut)
            {
                canvasGroup = panelObject.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            panelObject = gameObject; // Use this GameObject if not assigned
        }
        
        // Set message text
        if (messageText != null)
        {
            messageText.text = displayMessage;
        }
        
        // Start listening for camera activation
        StartListening();
        
        // Hide panel initially
        HidePanelImmediate();
        
        Debug.Log($"Panel will show for {displayTime} seconds ONCE when Camera2 activates.");
    }
    
    void StartListening()
    {
        isListening = true;
        
        // If targetCamera not assigned, find it
        if (targetCamera == null)
        {
            targetCamera = FindCamera2();
        }
        
        if (targetCamera == null)
        {
            Debug.LogError("No Camera2 found! Panel will not work.");
            isListening = false;
            return;
        }
    }
    
    void Update()
    {
        if (!isListening || targetCamera == null) return;
        
        // Check if camera just became active AND panel hasn't shown before
        if (targetCamera.gameObject.activeSelf && !hasShownOnce && (currentCoroutine == null || !panelObject.activeSelf))
        {
            OnCameraActivated();
        }
        
        // Check if camera just became inactive
        if (!targetCamera.gameObject.activeSelf && panelObject.activeSelf)
        {
            OnCameraDeactivated();
        }
    }
    
    void OnCameraActivated()
    {
        // Only show if it hasn't shown before
        if (hasShownOnce)
        {
            Debug.Log("Panel already shown once. Skipping.");
            return;
        }
        
        Debug.Log("Camera2 activated! Showing panel for the FIRST time...");
        
        // Mark that panel will show
        hasShownOnce = true;
        
        // Stop any existing coroutine
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        
        // Show panel with timer
        currentCoroutine = StartCoroutine(ShowAndHidePanelOnce());
    }
    
    void OnCameraDeactivated()
    {
        Debug.Log("Camera2 deactivated!");
        
        // Only hide if panel is currently showing
        if (panelObject.activeSelf)
        {
            HidePanelImmediate();
        }
        
        // Stop coroutine if running
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }
    
    IEnumerator ShowAndHidePanelOnce()
    {
        // Show panel
        ShowPanel();
        
        // Wait for display time
        yield return new WaitForSeconds(displayTime);
        
        // Hide panel
        HidePanel();
        
        // IMPORTANT: Panel will NEVER show again automatically
        Debug.Log("Panel shown once. Will not appear again.");
        
        currentCoroutine = null;
    }
    
    void ShowPanel()
    {
        panelObject.SetActive(true);
        
        if (fadeInOut && canvasGroup != null)
        {
            StartCoroutine(FadePanel(0f, 1f, fadeSpeed));
        }
    }
    
    void HidePanel()
    {
        if (fadeInOut && canvasGroup != null)
        {
            StartCoroutine(FadePanel(1f, 0f, fadeSpeed, true));
        }
        else
        {
            HidePanelImmediate();
        }
    }
    
    void HidePanelImmediate()
    {
        panelObject.SetActive(false);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    IEnumerator FadePanel(float startAlpha, float endAlpha, float speed, bool disableAfter = false)
    {
        if (canvasGroup == null) yield break;
        
        float elapsed = 0f;
        canvasGroup.alpha = startAlpha;
        
        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * speed;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed);
            yield return null;
        }
        
        canvasGroup.alpha = endAlpha;
        
        if (disableAfter)
        {
            panelObject.SetActive(false);
        }
    }
    
    Camera FindCamera2()
    {
        // Try to find Camera2 by name
        GameObject camObj = GameObject.Find("Camera2");
        if (camObj != null) return camObj.GetComponent<Camera>();
        
        // Try to find any camera not tagged as MainCamera
        Camera[] allCams = FindObjectsOfType<Camera>(true);
        foreach (Camera cam in allCams)
        {
            if (cam.tag != "MainCamera")
            {
                return cam;
            }
        }
        
        return null;
    }
    
    // Public method to manually trigger panel (ONCE only)
    public void ShowForDuration(float customDuration = -1)
    {
        if (hasShownOnce)
        {
            Debug.Log("Panel already shown once. Use ResetPanel() to show again.");
            return;
        }
        
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        
        hasShownOnce = true;
        float duration = (customDuration > 0) ? customDuration : displayTime;
        currentCoroutine = StartCoroutine(ShowAndHideCustom(duration));
    }
    
    IEnumerator ShowAndHideCustom(float duration)
    {
        ShowPanel();
        yield return new WaitForSeconds(duration);
        HidePanel();
        currentCoroutine = null;
    }
    
    // NEW: Reset the panel so it can show again
    public void ResetPanel()
    {
        hasShownOnce = false;
        Debug.Log("Panel reset. It will show again when Camera2 activates.");
    }
    
    // NEW: Check if panel has shown before
    public bool HasPanelShown()
    {
        return hasShownOnce;
    }
    
    void OnDestroy()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }
        
        if (panelObject != null && panelObject.activeSelf)
        {
            panelObject.SetActive(false);
        }
    }
}