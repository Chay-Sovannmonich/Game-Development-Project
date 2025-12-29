using UnityEngine;
using UnityEngine.UI;

public class CanvasCloser : MonoBehaviour
{
    [Header("=== CANVAS TO CLOSE ===")]
    [Tooltip("Drag the canvas you want to close here")]
    public Canvas canvasToClose;
    
    [Header("=== CANVAS TO DESTROY ===")]
    [Tooltip("Drag a SECOND canvas to DESTROY (optional)")]
    public Canvas canvasToDestroy;
    
    [Tooltip("Delay before destroying the second canvas")]
    public float destroyCanvasDelay = 0f;
    
    [Header("=== CLOSE BUTTON ===")]
    [Tooltip("Drag the button that triggers the close/destroy")]
    public Button closeButton;
    
    [Header("=== CLOSE SETTINGS ===")]
    [Tooltip("Delay before closing canvas (seconds)")]
    public float closeDelay = 0f;
    
    [Tooltip("Destroy the main canvas instead of just disabling it?")]
    public bool destroyMainInstead = false;
    
    [Header("=== VISUAL FEEDBACK ===")]
    [Tooltip("Play sound when closing")]
    public AudioClip closeSound;
    
    [Tooltip("Visual effect when closing")]
    public ParticleSystem closeEffect;
    
    private AudioSource audioSource;
    
    void Start()
    {
        Debug.Log("CanvasCloser initializing...");
        
        // Add audio source if needed
        if (closeSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        
        // Try to find canvas if not assigned
        if (canvasToClose == null)
        {
            canvasToClose = GetComponent<Canvas>();
            if (canvasToClose == null)
            {
                // Try to find by name
                GameObject canvasObj = GameObject.Find("Canvas");
                if (canvasObj != null) canvasToClose = canvasObj.GetComponent<Canvas>();
            }
        }
        
        // Setup button listener
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClick);
            Debug.Log($"Close button assigned: {closeButton.name}");
        }
        else
        {
            // Try to get button from this GameObject
            closeButton = GetComponent<Button>();
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(OnCloseButtonClick);
            }
        }
        
        // Log setup status
        Debug.Log($"Canvas to close: {canvasToClose?.name ?? "Not assigned"}");
        Debug.Log($"Canvas to destroy: {canvasToDestroy?.name ?? "Not assigned"}");
    }
    
    public void OnCloseButtonClick()
    {
        Debug.Log("Close button clicked!");
        
        // Play sound effect
        PlayCloseSound();
        
        // Play visual effect
        PlayCloseEffect();
        
        // Close the main canvas
        if (closeDelay > 0)
        {
            Invoke("CloseMainCanvas", closeDelay);
        }
        else
        {
            CloseMainCanvas();
        }
        
        // Destroy the second canvas (if assigned)
        if (canvasToDestroy != null)
        {
            if (destroyCanvasDelay > 0)
            {
                Invoke("DestroySecondCanvas", destroyCanvasDelay);
            }
            else
            {
                DestroySecondCanvas();
            }
        }
    }
    
    void CloseMainCanvas()
    {
        if (canvasToClose == null)
        {
            Debug.LogWarning("No canvas to close assigned!");
            return;
        }
        
        if (destroyMainInstead)
        {
            Destroy(canvasToClose.gameObject);
            Debug.Log($"Destroyed canvas: {canvasToClose.name}");
        }
        else
        {
            canvasToClose.gameObject.SetActive(false);
            Debug.Log($"Disabled canvas: {canvasToClose.name}");
        }
    }
    
    void DestroySecondCanvas()
    {
        if (canvasToDestroy == null)
        {
            Debug.LogWarning("No second canvas to destroy assigned!");
            return;
        }
        
        if (canvasToDestroy.gameObject != null)
        {
            Destroy(canvasToDestroy.gameObject);
            Debug.Log($"Destroyed second canvas: {canvasToDestroy.name}");
        }
    }
    
    void PlayCloseSound()
    {
        if (closeSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(closeSound);
        }
    }
    
    void PlayCloseEffect()
    {
        if (closeEffect != null)
        {
            Instantiate(closeEffect, transform.position, Quaternion.identity);
        }
    }
    
    // Public methods for other scripts to call
    public void CloseCanvasNow()
    {
        OnCloseButtonClick();
    }
    
    public void SetCanvasToClose(Canvas canvas)
    {
        canvasToClose = canvas;
    }
    
    public void SetCanvasToDestroy(Canvas canvas)
    {
        canvasToDestroy = canvas;
    }
    
    public void DestroyAllCanvases()
    {
        // Destroy ALL canvases in scene
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in allCanvases)
        {
            Destroy(canvas.gameObject);
        }
        Debug.Log($"Destroyed {allCanvases.Length} canvases");
    }
    
    void OnDestroy()
    {
        // Clean up button listener
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }
    }
}