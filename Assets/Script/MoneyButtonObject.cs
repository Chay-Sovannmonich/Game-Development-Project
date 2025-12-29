using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class MoneyButtonObject : MonoBehaviour
{
    [Header("3D Object References")]
    public GameObject clickable3DObject; // The main 3D object to click
    public Renderer objectRenderer; // Renderer for the 3D object
    public TextMeshPro buttonText3D; // 3D text instructions
    
    [Header("Fixed Amount Text")]
    public TextMeshPro amountText3D; // Fixed amount text (set to +$50 initially)
    public string afterClickText = "550"; // Text to show after clicking
    
    [Header("Money System")]
    public MoneyManager moneyManager; // Reference to the MoneyManager
    public int moneyIncreaseAmount = 50;
    
    [Header("Spawn Position Settings")]
    public Vector3 spawnOffset = new Vector3(0, 2f, 0);
    public Transform customSpawnPosition;
    
    [Header("Visibility Settings")]
    public bool startHidden = true; // Start completely hidden
    public float showDelay = 0.5f; // Delay before showing
    public float fadeInDuration = 1f; // Fade in duration
    
    [Header("3D Object Animation Settings")]
    public float floatHeight = 0.5f; // Floating height (in world units)
    public float floatSpeed = 1f;
    public float rotationSpeed = 30f; // For 3D object rotation
    public float scalePulseSpeed = 2f;
    public float scalePulseAmount = 0.2f;
    
    [Header("Click Settings")]
    public Color clickColor = Color.green;
    public float clickColorDuration = 0.2f;
    public AudioClip clickSound;
    public ParticleSystem clickParticles;
    
    [Header("Auto Destroy")]
    public float destroyDelay = 5f; // How long before auto-destroy
    
    // Private variables
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Color originalObjectColor;
    private Color originalButtonTextColor;
    private Color originalAmountTextColor;
    private AudioSource audioSource;
    private bool isClicked = false;
    private bool isVisible = false;
    private float floatOffset;
    private Collider objectCollider; // For click detection
    private Material objectMaterial; // For color changes

    void Start()
    {
        // Try to find MoneyManager if not assigned
        if (moneyManager == null)
        {
            moneyManager = FindObjectOfType<MoneyManager>();
        }
        
        // Store original values
        originalPosition = transform.position;
        originalScale = transform.localScale;
        
        // Get collider for click detection
        objectCollider = clickable3DObject.GetComponent<Collider>();
        if (objectCollider == null)
        {
            objectCollider = clickable3DObject.AddComponent<BoxCollider>();
        }
        
        // Get renderer if not assigned
        if (objectRenderer == null)
        {
            objectRenderer = clickable3DObject.GetComponent<Renderer>();
        }
        
        // Get material for color changes
        if (objectRenderer != null)
        {
            objectMaterial = objectRenderer.material;
            originalObjectColor = objectMaterial.color;
        }
        
        // Store original text colors
        if (buttonText3D != null)
        {
            originalButtonTextColor = buttonText3D.color;
            buttonText3D.gameObject.SetActive(false); // Hide initially
        }
        
        if (amountText3D != null)
        {
            originalAmountTextColor = amountText3D.color;
            amountText3D.gameObject.SetActive(false); // Hide initially
            
            // Set initial amount text
            amountText3D.text = $"+${moneyIncreaseAmount}";
        }
        
        // Set up audio
        if (clickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = clickSound;
        }
        
        // Disable collider initially
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }
        
        // Initialize float offset for animation
        floatOffset = Random.Range(0f, 2f * Mathf.PI);
        
        // Hide everything initially
        HideAllElements();
        
        // Start showing process
        StartCoroutine(ShowAfterDelay());
        
        // Auto-destroy after delay
        if (destroyDelay > 0)
        {
            Destroy(gameObject, destroyDelay);
        }
        
        Debug.Log("MoneyButtonObject created. Waiting to appear...");
    }
    
    IEnumerator ShowAfterDelay()
    {
        // Wait for the delay
        yield return new WaitForSeconds(showDelay);
        
        // Show the object with fade in
        yield return StartCoroutine(FadeInObject());
        
        // Enable collider for clicking
        if (objectCollider != null)
        {
            objectCollider.enabled = true;
        }
        
        // Show text elements
        if (buttonText3D != null)
        {
            buttonText3D.gameObject.SetActive(true);
            buttonText3D.text = "Tap to Collect!";
        }
        
        if (amountText3D != null)
        {
            amountText3D.gameObject.SetActive(true);
        }
        
        isVisible = true;
        Debug.Log("MoneyButtonObject now visible and clickable!");
    }
    
    void Update()
    {
        // Only animate if visible
        if (!isVisible) return;
        
        // Check for mouse click
        if (Input.GetMouseButtonDown(0) && !isClicked && isVisible)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit) && hit.collider == objectCollider)
            {
                IncreaseMoney();
            }
        }
        
        // Floating animation
        if (floatSpeed > 0 && floatHeight > 0)
        {
            float floatY = Mathf.Sin((Time.time * floatSpeed) + floatOffset) * floatHeight;
            Vector3 newPos = originalPosition;
            newPos.y += floatY;
            transform.position = newPos;
        }
        
        // Scale pulsing
        if (scalePulseSpeed > 0 && scalePulseAmount > 0)
        {
            float pulse = Mathf.Sin(Time.time * scalePulseSpeed) * scalePulseAmount;
            transform.localScale = originalScale * (1 + pulse);
        }
        
        // Rotation
        if (rotationSpeed > 0)
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0, Space.World);
        }
    }
    
    public void IncreaseMoney()
    {
        if (isClicked || !isVisible) return;
        
        isClicked = true;
        
        // Add money to the MoneyManager
        if (moneyManager != null)
        {
            moneyManager.AddMoney(moneyIncreaseAmount);
            
            // Update afterClickText to show new total
            int newTotal = moneyManager.GetCurrentMoney();
            afterClickText = newTotal.ToString();
        }
        else
        {
            Debug.LogWarning("MoneyButtonObject: No MoneyManager found!");
        }
        
        // Visual and audio feedback
        StartCoroutine(ClickFeedback());
        
        // Destroy after feedback
        Destroy(gameObject, 0.5f);
    }
    
    IEnumerator ClickFeedback()
    {
        // Disable collider to prevent multiple clicks
        if (objectCollider != null)
        {
            objectCollider.enabled = false;
        }
        
        // Play sound
        if (audioSource != null && clickSound != null)
        {
            audioSource.Play();
        }
        
        // Show particles
        if (clickParticles != null)
        {
            clickParticles.Play();
        }
        
        // Change colors
        if (objectMaterial != null)
        {
            objectMaterial.color = clickColor;
        }
        
        if (buttonText3D != null)
        {
            buttonText3D.color = clickColor;
            buttonText3D.text = "Collected!";
        }
        
        if (amountText3D != null)
        {
            amountText3D.color = clickColor;
            // Change text to show new total
            amountText3D.text = afterClickText;
        }
        
        yield return new WaitForSeconds(clickColorDuration);
    }
    
    IEnumerator FadeInObject()
    {
        // Fade in the object and text
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeInDuration);
            
            // Fade object
            if (objectMaterial != null)
            {
                Color color = objectMaterial.color;
                color.a = alpha;
                objectMaterial.color = color;
            }
            
            // Fade text
            if (buttonText3D != null)
            {
                Color textColor = buttonText3D.color;
                textColor.a = alpha;
                buttonText3D.color = textColor;
            }
            
            if (amountText3D != null)
            {
                Color textColor = amountText3D.color;
                textColor.a = alpha;
                amountText3D.color = textColor;
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Set final alpha
        if (objectMaterial != null)
        {
            Color color = objectMaterial.color;
            color.a = 1;
            objectMaterial.color = color;
        }
        
        if (buttonText3D != null)
        {
            Color textColor = originalButtonTextColor;
            textColor.a = 1;
            buttonText3D.color = textColor;
        }
        
        if (amountText3D != null)
        {
            Color textColor = originalAmountTextColor;
            textColor.a = 1;
            amountText3D.color = textColor;
        }
    }
    
    void HideAllElements()
    {
        // Hide object
        if (objectMaterial != null)
        {
            Color color = objectMaterial.color;
            color.a = 0;
            objectMaterial.color = color;
        }
        
        // Hide text
        if (buttonText3D != null)
        {
            buttonText3D.gameObject.SetActive(false);
        }
        
        if (amountText3D != null)
        {
            amountText3D.gameObject.SetActive(false);
        }
        
        // Reset position and scale
        transform.position = originalPosition;
        transform.localScale = originalScale;
    }
    
    [ContextMenu("Test Increase Money")]
    public void TestIncreaseMoney()
    {
        if (!isVisible)
        {
            Debug.Log("Making object visible first...");
            StartCoroutine(ShowAfterDelay());
            StartCoroutine(DelayedTest());
        }
        else
        {
            IncreaseMoney();
        }
    }
    
    IEnumerator DelayedTest()
    {
        yield return new WaitForSeconds(showDelay + fadeInDuration + 0.5f);
        IncreaseMoney();
    }
    
    [ContextMenu("Test Show Object")]
    public void TestShowObject()
    {
        if (!isVisible)
        {
            StartCoroutine(ShowAfterDelay());
        }
    }
    
    [ContextMenu("Test Hide Object")]
    public void TestHideObject()
    {
        HideAllElements();
        isVisible = false;
    }
    
    public void SetSpawnPosition(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        originalPosition = position;
    }
    
    void OnDestroy()
    {
        Debug.Log("MoneyButtonObject destroyed.");
    }
}