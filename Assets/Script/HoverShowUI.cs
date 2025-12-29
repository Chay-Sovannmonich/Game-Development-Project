using UnityEngine;

public class HoverShowUI : MonoBehaviour
{
    [Header("UI Panel to show on hover")]
    public GameObject panel;

    [Header("Hover Settings")]
    public float maxRayDistance = 100f;
    public LayerMask raycastLayerMask = ~0; // Default: everything
    
    private bool isHovering = false;
    private Camera mainCamera;
    private bool isInitialized = false;

    void Start()
    {
        // Initialize and check for errors
        Initialize();
    }

    void Initialize()
    {
        // Get main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError($"HoverShowUI on {gameObject.name}: No main camera found!");
            enabled = false;
            return;
        }

        // Check if panel is assigned
        if (panel == null)
        {
            Debug.LogWarning($"HoverShowUI on {gameObject.name}: Panel is not assigned in Inspector. Hover feature disabled.");
            enabled = false;
            return;
        }

        // Make sure panel is hidden at start
        panel.SetActive(false);
        
        // Check if this GameObject has a collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogWarning($"HoverShowUI on {gameObject.name}: No Collider component found! Add a Collider for hover detection.");
            enabled = false;
            return;
        }

        isInitialized = true;
        Debug.Log($"HoverShowUI initialized on {gameObject.name}");
    }

    void Update()
    {
        // Only run if properly initialized
        if (!isInitialized) return;
        
        DetectHover();
    }

    void DetectHover()
    {
        // Safety check for camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform raycast with layer mask and max distance
        if (Physics.Raycast(ray, out hit, maxRayDistance, raycastLayerMask))
        {
            // Check if this object is the one being hovered
            if (hit.collider.gameObject == this.gameObject)
            {
                if (!isHovering)
                {
                    ShowUI();
                    isHovering = true;
                }
            }
            else
            {
                if (isHovering)
                {
                    HideUI();
                    isHovering = false;
                }
            }
        }
        else
        {
            if (isHovering)
            {
                HideUI();
                isHovering = false;
            }
        }
    }

    void ShowUI()
    {
        // Double-check panel is not null
        if (panel != null && !panel.activeSelf)
        {
            panel.SetActive(true);
            Debug.Log($"Showing UI panel for {gameObject.name}");
        }
    }

    void HideUI()
    {
        // Double-check panel is not null
        if (panel != null && panel.activeSelf)
        {
            panel.SetActive(false);
            Debug.Log($"Hiding UI panel for {gameObject.name}");
        }
    }

    // Public methods for manual control
    public void ForceShowUI()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            isHovering = true;
        }
    }

    public void ForceHideUI()
    {
        if (panel != null)
        {
            panel.SetActive(false);
            isHovering = false;
        }
    }

    // Debug info in editor
    void OnDrawGizmosSelected()
    {
        if (enabled && isInitialized)
        {
            Gizmos.color = isHovering ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}