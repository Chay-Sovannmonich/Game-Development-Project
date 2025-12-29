using UnityEngine;
using System.Collections;

public class ProximityFood : MonoBehaviour
{
    [Header("Proximity Settings")]
    public float appearDistance = 2.5f;
    public float disappearDistance = 4f;
    public float fadeSpeed = 5f;
    
    [Header("Timer Settings")]
    public bool useTimer = true;
    public float disappearAfterSeconds = 60f; // 1 minute
    public float warningTime = 10f; // Start warning 10 seconds before disappearing
    
    [Header("Visual Effects")]
    public bool useFadeEffect = true;
    public bool useScaleEffect = true;
    public float appearScale = 1f;
    public float hiddenScale = 0.1f;
    
    [Header("Warning Effect")]
    public bool useWarningEffect = true;
    public float warningFlashSpeed = 2f;
    public Color warningColor = Color.red;
    
    [Header("Worker Reference")]
    public WorkerAI targetWorker;
    public string workerTag = "Worker";
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public bool alwaysVisibleInEditor = false;
    
    // Private variables
    private Renderer[] renderers;
    private Collider foodCollider;
    private bool isVisible = false;
    private float currentAlpha = 0f;
    private Vector3 originalScale;
    private Material[] materials;
    private Color[] originalColors;
    
    // Timer variables
    private float timer = 0f;
    private bool timerActive = false;
    private bool warningActive = false;
    private float warningTimer = 0f;
    
    void Start()
    {
        InitializeComponents();
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: ProximityFood initialized. Appear distance: {appearDistance}, Disappear after: {disappearAfterSeconds}s");
    }
    
    void InitializeComponents()
    {
        // Get all renderers
        renderers = GetComponentsInChildren<Renderer>();
        
        // Get collider
        foodCollider = GetComponent<Collider>();
        if (foodCollider != null)
            foodCollider.enabled = false;
        
        // Store original scale
        originalScale = transform.localScale;
        
        // If no worker assigned, try to find one
        if (targetWorker == null)
        {
            GameObject workerObj = GameObject.FindGameObjectWithTag(workerTag);
            if (workerObj != null)
            {
                targetWorker = workerObj.GetComponent<WorkerAI>();
                if (showDebugInfo && targetWorker != null)
                    Debug.Log($"{gameObject.name}: Found worker: {targetWorker.name}");
            }
        }
        
        // Initialize materials for fade effect
        if (useFadeEffect && renderers.Length > 0)
        {
            materials = new Material[renderers.Length];
            originalColors = new Color[renderers.Length];
            
            for (int i = 0; i < renderers.Length; i++)
            {
                materials[i] = renderers[i].material;
                originalColors[i] = materials[i].color;
                
                // Start with transparent
                Color transparentColor = originalColors[i];
                transparentColor.a = 0f;
                materials[i].color = transparentColor;
            }
        }
        
        // Start hidden
        SetVisibility(false, true);
    }
    
    void Update()
    {
        if (targetWorker == null)
        {
            TryFindWorker();
            return;
        }
        
        CheckProximity();
        UpdateVisibility();
        UpdateTimer();
        UpdateWarningEffect();
    }
    
    void TryFindWorker()
    {
        GameObject workerObj = GameObject.FindGameObjectWithTag(workerTag);
        if (workerObj != null)
        {
            targetWorker = workerObj.GetComponent<WorkerAI>();
        }
    }
    
    void CheckProximity()
    {
        float distance = Vector3.Distance(transform.position, targetWorker.transform.position);
        
        if (!isVisible && distance <= appearDistance)
        {
            // Worker is close enough - appear
            SetVisibility(true);
            
            // Start timer if not already active
            if (useTimer && !timerActive)
            {
                StartTimer();
            }
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Appearing. Distance to worker: {distance}");
        }
        else if (isVisible && distance > disappearDistance)
        {
            // Worker is too far - disappear
            SetVisibility(false);
            
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Disappearing. Distance to worker: {distance}");
        }
    }
    
    void SetVisibility(bool visible, bool immediate = false)
    {
        if (isVisible == visible && !immediate)
            return;
            
        isVisible = visible;
        
        // Enable/disable collider
        if (foodCollider != null)
            foodCollider.enabled = visible;
        
        // Reset warning if disappearing
        if (!visible)
        {
            warningActive = false;
            warningTimer = 0f;
        }
        
        // Set target alpha
        float targetAlpha = visible ? 1f : 0f;
        
        if (immediate)
        {
            currentAlpha = targetAlpha;
            ApplyAlpha(currentAlpha);
            ApplyScale(visible ? appearScale : hiddenScale);
        }
    }
    
    void UpdateVisibility()
    {
        if (useFadeEffect)
        {
            float targetAlpha = isVisible ? 1f : 0f;
            currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            
            // Apply fade
            ApplyAlpha(currentAlpha);
            
            // If almost invisible, disable renderers
            if (currentAlpha < 0.01f && isVisible == false)
            {
                foreach (Renderer rend in renderers)
                {
                    rend.enabled = false;
                }
            }
            else if (currentAlpha > 0.01f)
            {
                foreach (Renderer rend in renderers)
                {
                    rend.enabled = true;
                }
            }
        }
        
        if (useScaleEffect)
        {
            float targetScale = isVisible ? appearScale : hiddenScale;
            float currentScale = Mathf.Lerp(transform.localScale.x / originalScale.x, targetScale, fadeSpeed * Time.deltaTime);
            ApplyScale(currentScale);
        }
        
        // If not using effects, just toggle renderers
        if (!useFadeEffect && !useScaleEffect)
        {
            foreach (Renderer rend in renderers)
            {
                rend.enabled = isVisible;
            }
        }
    }
    
    void ApplyAlpha(float alpha)
    {
        if (!useFadeEffect || materials == null)
            return;
            
        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i] != null)
            {
                Color newColor = originalColors[i];
                newColor.a = alpha * originalColors[i].a; // Preserve original alpha
                materials[i].color = newColor;
            }
        }
    }
    
    void ApplyScale(float scaleMultiplier)
    {
        if (!useScaleEffect)
            return;
            
        transform.localScale = originalScale * scaleMultiplier;
    }
    
    // Timer functions
    void StartTimer()
    {
        timer = 0f;
        timerActive = true;
        warningActive = false;
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Timer started. Will disappear in {disappearAfterSeconds} seconds");
    }
    
    void UpdateTimer()
    {
        if (!useTimer || !timerActive || !isVisible)
            return;
            
        timer += Time.deltaTime;
        
        // Check for warning time
        if (!warningActive && timer >= (disappearAfterSeconds - warningTime))
        {
            warningActive = true;
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Warning! Will disappear in {warningTime} seconds");
        }
        
        // Check if time's up
        if (timer >= disappearAfterSeconds)
        {
            ForceDisappearWithTimer();
            timerActive = false;
        }
    }
    
    void UpdateWarningEffect()
    {
        if (!useWarningEffect || !warningActive || !isVisible)
            return;
            
        warningTimer += Time.deltaTime * warningFlashSpeed;
        float flashValue = (Mathf.Sin(warningTimer * Mathf.PI * 2) + 1f) * 0.5f; // 0 to 1 oscillation
        
        if (useFadeEffect && materials != null)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    // Flash between original color and warning color
                    Color flashColor = Color.Lerp(originalColors[i], warningColor, flashValue);
                    flashColor.a = materials[i].color.a; // Preserve alpha
                    materials[i].color = flashColor;
                }
            }
        }
    }
    
    void ForceDisappearWithTimer()
    {
        SetVisibility(false, true);
        timerActive = false;
        warningActive = false;
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Disappeared due to timer ({disappearAfterSeconds} seconds elapsed)");
    }
    
    // Public method to force appear/disappear
    public void ForceAppear()
    {
        SetVisibility(true, true);
        
        // Start timer
        if (useTimer)
        {
            StartTimer();
        }
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Forced to appear");
    }
    
    public void ForceDisappear()
    {
        SetVisibility(false, true);
        timerActive = false;
        warningActive = false;
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Forced to disappear");
    }
    
    // Reset timer without changing visibility
    public void ResetTimer()
    {
        if (useTimer)
        {
            StartTimer();
            if (showDebugInfo)
                Debug.Log($"{gameObject.name}: Timer reset");
        }
    }
    
    // Method called by WorkerAI when food is spawned
    public void InitializeForWorker(WorkerAI worker)
    {
        targetWorker = worker;
        SetVisibility(false, true);
        
        if (useTimer)
        {
            StartTimer();
        }
        
        if (showDebugInfo)
            Debug.Log($"{gameObject.name}: Initialized for worker: {worker.name}");
    }
    
    // Context menu for testing
    [ContextMenu("Test Appear")]
    public void TestAppear()
    {
        ForceAppear();
    }
    
    [ContextMenu("Test Disappear")]
    public void TestDisappear()
    {
        ForceDisappear();
    }
    
    [ContextMenu("Reset Timer")]
    public void ResetTimerMenu()
    {
        ResetTimer();
    }
    
    void OnDrawGizmosSelected()
    {
        if (alwaysVisibleInEditor && Application.isPlaying == false)
            return;
            
        // Draw proximity spheres
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // Orange
        Gizmos.DrawWireSphere(transform.position, appearDistance);
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f); // Red
        Gizmos.DrawWireSphere(transform.position, disappearDistance);
        
        // Draw line to worker if assigned
        if (targetWorker != null)
        {
            Gizmos.color = isVisible ? Color.green : Color.gray;
            Gizmos.DrawLine(transform.position, targetWorker.transform.position);
            
            #if UNITY_EDITOR
            // Timer info
            string timerInfo = useTimer ? 
                $"\nTimer: {timer:F1}/{disappearAfterSeconds:F0}s\nWarning: {warningActive}" : 
                "\nTimer: OFF";
                
            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, 
                $"Food: {gameObject.name}\n" +
                $"Visible: {isVisible}\n" +
                $"Alpha: {currentAlpha:F2}{timerInfo}");
            #endif
        }
    }
    
    // Properties for external access
    public bool IsVisible
    {
        get { return isVisible; }
    }
    
    public float TimeRemaining
    {
        get { return useTimer && timerActive ? Mathf.Max(0, disappearAfterSeconds - timer) : 0f; }
    }
    
    public bool IsWarningActive
    {
        get { return warningActive; }
    }
}