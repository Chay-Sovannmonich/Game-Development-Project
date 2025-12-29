using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleCustomerTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public float maxWaitTime = 30f;  // Time waiting for food
    public float eatingTime = 60f;   // Time eating food
    
    [Header("UI Elements")]
    public TextMeshPro timerText;
    public Image fillImage;
    
    [Header("Position Offset")]
    public float height = 2f;        // Height above customer
    public float forwardOffset = 0.5f; // How far in front
    public float leftOffset = -1f;   // Negative = left, Positive = right
    
    private CharacterAI customer;
    private float currentTime;
    private bool isActive = false;
    private bool isWaitingForFood = true;
    
    void Update()
    {
        if (!isActive || customer == null) return;
        
        // Update position to follow customer
        FollowCustomer();
        
        // Update timer
        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            
            // Update text
            if (timerText != null)
            {
                if (isWaitingForFood)
                    timerText.text = $"Wait: {Mathf.CeilToInt(currentTime)}s";
                else
                    timerText.text = $"Eat: {Mathf.CeilToInt(currentTime)}s";
            }
            
            // Update fill
            if (fillImage != null)
            {
                float fillAmount = currentTime / (isWaitingForFood ? maxWaitTime : eatingTime);
                fillImage.fillAmount = fillAmount;
                
                // Change color
                if (isWaitingForFood)
                {
                    if (fillAmount < 0.3f)
                        fillImage.color = Color.red;
                    else if (fillAmount < 0.6f)
                        fillImage.color = Color.yellow;
                    else
                        fillImage.color = Color.green;
                }
                else
                {
                    fillImage.color = Color.blue;
                }
            }
        }
        else
        {
            // Timer finished
            if (isWaitingForFood)
            {
                Debug.Log($"{customer.gameObject.name}: Waiting time expired!");
                if (timerText != null)
                    timerText.text = "TIME UP!";
            }
            else
            {
                Debug.Log($"{customer.gameObject.name}: Finished eating!");
                if (timerText != null)
                    timerText.text = "DONE!";
            }
            isActive = false;
            
            // Hide timer after 1 second
            Invoke("HideTimer", 1f);
        }
    }
    
    // Initialize the timer - THIS STARTS THE TIMER
    public void Initialize(CharacterAI customerRef, float waitTime, float eatTime)
    {
        customer = customerRef;
        maxWaitTime = waitTime;
        eatingTime = eatTime;
        
        isActive = true;
        isWaitingForFood = true;
        currentTime = maxWaitTime;
        
        // Set initial position
        UpdatePosition();
        
        gameObject.SetActive(true);
        
        // Set initial UI values
        if (timerText != null)
            timerText.text = $"Wait: {maxWaitTime}s";
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.color = Color.green;
        }
        
        Debug.Log($"{customer.gameObject.name}: Timer initialized at position: " + transform.position);
    }
    
    // Called when food arrives
    public void StartEating()
    {
        if (!isActive) return;
        
        isWaitingForFood = false;
        currentTime = eatingTime;
        
        if (timerText != null)
            timerText.text = $"Eat: {eatingTime}s";
        if (fillImage != null)
        {
            fillImage.fillAmount = 1f;
            fillImage.color = Color.blue;
        }
    }
    
    void FollowCustomer()
    {
        if (customer != null)
        {
            // Update position to follow customer
            UpdatePosition();
            
            // Make it face the camera
            if (Camera.main != null)
            {
                transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                               Camera.main.transform.rotation * Vector3.up);
            }
        }
    }
    
    void UpdatePosition()
    {
        if (customer == null) return;
        
        // Calculate position with left offset
        Vector3 position = customer.transform.position + 
                          Vector3.up * height + 
                          customer.transform.forward * forwardOffset +
                          Vector3.left * Mathf.Abs(leftOffset); // Always move left
        
        transform.position = position;
    }
    
    void HideTimer()
    {
        gameObject.SetActive(false);
    }
    
    void OnDestroy()
    {
        CancelInvoke();
    }
    
    // DEBUG: Draw gizmo to see where timer is positioned
    void OnDrawGizmos()
    {
        if (customer != null && isActive)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.2f);
            Gizmos.DrawLine(customer.transform.position, transform.position);
        }
    }
}